using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NRough.Data;
using NRough.Core;
using NRough.MachineLearning.Weighting;
using NRough.MachineLearning.Permutations;

namespace NRough.MachineLearning.Roughsets
{
    public class ReductGeneralizedMajorityDecisionGenerator : ReductGenerator
    {
        private double dataSetQuality = 1.0;
        private WeightGenerator weightGenerator = null;
        protected List<int[]> attributePermutations;

        public WeightGenerator WeightGenerator
        {
            get
            {
                if (this.weightGenerator == null)
                {
                    lock (mutex)
                    {
                        if (this.weightGenerator == null)
                        {
                            this.weightGenerator = new WeightGeneratorConstant(this.DecisionTable);
                        }
                    }
                }
                return this.weightGenerator;
            }

            set
            {
                lock (mutex)
                {
                    this.weightGenerator = value;
                }
            }
        }

        public double DataSetQuality
        {
            get
            {
                if (!this.IsQualityCalculated)
                {
                    lock (mutex)
                    {
                        if (!this.IsQualityCalculated)
                        {
                            this.CalcDataSetQuality();
                            this.IsQualityCalculated = true;
                        }
                    }
                }

                return this.dataSetQuality;
            }

            set
            {
                lock (mutex)
                {
                    this.dataSetQuality = value;
                    this.IsQualityCalculated = true;
                }
            }
        }

        protected bool IsQualityCalculated { get; set; }

        public IReductStoreCollection ReductStoreCollection
        {
            get;
            protected set;
        }

        public int MinReductLength { get; set; }
        public int MaxReductLength { get; set; }

        public override void InitFromArgs(Args args)
        {
            base.InitFromArgs(args);

            if (args.Exist(ReductFactoryOptions.MinReductLength))
                this.MinReductLength = (int)args.GetParameter(ReductFactoryOptions.MinReductLength);

            if (args.Exist(ReductFactoryOptions.MaxReductLength))
                this.MaxReductLength = (int)args.GetParameter(ReductFactoryOptions.MaxReductLength);

            if (args.Exist(ReductFactoryOptions.WeightGenerator))
                this.weightGenerator = (WeightGenerator)args.GetParameter(ReductFactoryOptions.WeightGenerator);

            this.InitAttributePermutations();

            this.InitReductStoreCollection();

            this.CalcDataSetQuality();
        }

        protected virtual void InitAttributePermutations()
        {
            attributePermutations = new List<int[]>(this.Permutations.Count);
            foreach (var permutation in this.Permutations)
            {
                int cut = this.MaxReductLength > 0
                        ? this.MaxReductLength
                        : permutation.Length;

                int[] attributes = new int[cut];
                for (int i = 0; i < cut; i++)
                    attributes[i] = permutation[i];
                attributePermutations.Add(attributes);
            }
        }

        protected virtual void InitReductStoreCollection(int capacity = 1)
        {
            this.ReductStoreCollection = capacity > 0 ? new ReductStoreCollection(capacity) : new ReductStoreCollection();
        }

        protected virtual void CalcDataSetQuality()
        {
            IReduct reduct = this.CreateReductObject(this.DecisionTable.GetStandardFields(), 0, "");
            this.DataSetQuality = this.GetPartitionQuality(reduct);
        }

        protected virtual double GetPartitionQuality(IReduct reduct)
        {
            return InformationMeasureWeights.Instance.Calc(reduct);
        }

        protected override void Generate()
        {
            ParallelOptions options = new ParallelOptions()
            {
                MaxDegreeOfParallelism = ConfigManager.MaxDegreeOfParallelism
            };

            ReductStore localReductPool = new ReductStore(this.attributePermutations.Count);
            //foreach(var permutation in this.attributePermutations)
            Parallel.ForEach(this.attributePermutations, options, permutation =>
            {
                localReductPool.AddReduct(this.CalculateReduct(permutation, localReductPool));
            }
            );

            this.ReductStoreCollection.AddStore(localReductPool);
        }

        public override IReduct CreateReduct(
            int[] permutation, 
            double epsilon, 
            double[] weights, 
            IReductStore reductStore = null, 
            IReductStoreCollection reductStoreCollection = null)
        {
            EquivalenceClassCollection eqClasses = EquivalenceClassCollection.Create(permutation, this.DecisionTable, weights);
            int len = permutation.Length;

            eqClasses.WeightSum = this.DataSetQuality;
            eqClasses.NumberOfObjects = this.DecisionTable.NumberOfRecords;

            this.KeepMajorDecisions(eqClasses, epsilon);

            int step = this.ReductionStep > 0 ? this.ReductionStep : 1;

            //Console.WriteLine(permutation.ToStr(' '));

            EquivalenceClassCollection newEqClasses = null;
            while (step >= 1)
            {
                for (int i = 0; i < len && step <= len; i += step)
                {
                    //Console.WriteLine("+Trying to remove {0}", eqClasses.Attributes[i]);

                    newEqClasses = this.Reduce(eqClasses, i, step, reductStore);

                    //reduction was successful
                    if (!Object.ReferenceEquals(newEqClasses, eqClasses))
                    {
                        //Console.WriteLine("+Success to remove {0}", eqClasses.Attributes[i]);

                        eqClasses = newEqClasses;
                        len -= step;
                        i -= step;
                    }
                    //else
                    //{
                    //    Console.WriteLine("+Failed to remove {0}", eqClasses.Attributes[i]);
                    //}

                }

                if (step == 1)
                    break;

                step /= 2;
            }

            //eqClasses.RecalcEquivalenceClassStatistic(this.DataStore);

            return this.CreateReductObject(
                eqClasses.Attributes,
                epsilon,
                this.GetNextReductId().ToString(),
                eqClasses);
        }

        protected virtual bool CheckIsReduct(IReduct reduct, double epsilon)
        {
            return ToleranceDoubleComparer.Instance.Compare(
                this.GetPartitionQuality(reduct), 
                (1.0 - this.Epsilon) * this.DataSetQuality) != -1;
                            
            /*
            if (this.GetPartitionQuality(reduct) >= ((1.0 - epsilon) * this.DataSetQuality))
                return true;
            return false;
            */
        }

        public virtual IReduct CalculateReduct(int[] attributes, IReductStore reductStore = null, IReductStoreCollection reductStoreCollection = null)
        {
            return this.CreateReduct(attributes, this.Epsilon, this.WeightGenerator.Weights, reductStore, reductStoreCollection);
        }

        protected virtual void KeepMajorDecisions(EquivalenceClassCollection eqClasses, double epsilon = 0.0)
        {
            if (epsilon >= 1.0) return;
            foreach (EquivalenceClass eq in eqClasses)
                eq.KeepMajorDecisions(epsilon);
        }

        protected virtual EquivalenceClassCollection Reduce(EquivalenceClassCollection eqClasses, int attributeIdx, int length, IReductStore reductStore = null, IReductStoreCollection reductStoreCollection = null)
        {
            EquivalenceClassCollection newEqClasses = new EquivalenceClassCollection(
                this.DecisionTable,
                eqClasses.Attributes.RemoveAt(attributeIdx, length),
                eqClasses.Count);

            newEqClasses.WeightSum = eqClasses.WeightSum;
            newEqClasses.NumberOfObjects = eqClasses.NumberOfObjects;

            foreach (EquivalenceClass eq in eqClasses)
            {
                long[] newInstance = eq.Instance.RemoveAt(attributeIdx, length);
                EquivalenceClass newEqClass = newEqClasses.Find(newInstance);
                if (newEqClass != null)
                {
                    //Update m_d
                    newEqClass.DecisionSet.IntersectWith(eq.DecisionSet);

                    if (newEqClass.DecisionSet.Count == 0)
                        return eqClasses;

                    //Update |X * E|w as new weighted average
                    newEqClass.AvgConfidenceWeight += eq.AvgConfidenceWeight;

                    //Update |X * E| count
                    newEqClass.AvgConfidenceSum += eq.AvgConfidenceSum;

                    //Update |E|
                    newEqClass.AddObjectInstances(eq.Instances);

                    //Update |E|w
                    newEqClass.WeightSum += eq.WeightSum;
                }
                else
                {
                    newEqClass = new EquivalenceClass(newInstance, eq.Instances, eq.DecisionSet);
                    newEqClass.AvgConfidenceWeight = eq.AvgConfidenceWeight;
                    newEqClass.AvgConfidenceSum = eq.AvgConfidenceSum;
                    newEqClass.WeightSum = eq.WeightSum;

                    newEqClasses.Add(newEqClass);
                }
            }

            return newEqClasses;
        }

        protected override IReduct CreateReductObject(int[] fieldIds, double epsilon, string id)
        {            
            ReductWeights r = new ReductWeights(this.DecisionTable, fieldIds, epsilon, this.WeightGenerator.Weights);
            r.Id = id;
            return r;
        }

        protected override IReduct CreateReductObject(int[] fieldIds, double epsilon, string id, EquivalenceClassCollection eqClasses)
        {
            ReductWeights r = new ReductWeights(this.DecisionTable, fieldIds, epsilon, this.WeightGenerator.Weights, eqClasses);
            r.Id = id;
            return r;
        }

        public override IReductStoreCollection GetReductStoreCollection(int numberOfEnsembles = Int32.MaxValue)
        {
            if (this.ReductStoreCollection == null)
            {
                this.ReductStoreCollection = new ReductStoreCollection(1);
                this.ReductStoreCollection.AddStore(this.ReductPool);
            }

            return this.ReductStoreCollection;
        }
    }

    public class ReductGeneralizedMajorityDecisionFactory : IReductFactory
    {
        public virtual string FactoryKey
        {
            get { return ReductTypes.GeneralizedMajorityDecision; }
        }

        public virtual IReductGenerator GetReductGenerator(Args args)
        {
            ReductGeneralizedMajorityDecisionGenerator rGen = new ReductGeneralizedMajorityDecisionGenerator();
            rGen.InitFromArgs(args);
            return rGen;
        }

        public virtual IPermutationGenerator GetPermutationGenerator(Args args)
        {
            DataStore dataStore = (DataStore)args.GetParameter(ReductFactoryOptions.DecisionTable);
            return new PermutationGenerator(dataStore);
        }
    }
}