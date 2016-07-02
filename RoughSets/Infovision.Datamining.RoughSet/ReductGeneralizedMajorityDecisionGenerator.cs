using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Infovision.Data;
using Infovision.Utils;

namespace Infovision.Datamining.Roughset
{   
    public class ReductGeneralizedMajorityDecisionGenerator : ReductGenerator
    {
        private decimal dataSetQuality = Decimal.One;
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
                            this.weightGenerator = new WeightGeneratorConstant(this.DataStore);
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

        public decimal DataSetQuality
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

            if (args.Exist(ReductGeneratorParamHelper.MinReductLength))
                this.MinReductLength = (int)args.GetParameter(ReductGeneratorParamHelper.MinReductLength);

            if (args.Exist(ReductGeneratorParamHelper.MaxReductLength))
                this.MaxReductLength = (int)args.GetParameter(ReductGeneratorParamHelper.MaxReductLength);

            if (args.Exist(ReductGeneratorParamHelper.WeightGenerator))
                this.weightGenerator = (WeightGenerator)args.GetParameter(ReductGeneratorParamHelper.WeightGenerator);

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
            IReduct reduct = this.CreateReductObject(this.DataStore.DataStoreInfo.GetFieldIds(FieldTypes.Standard).ToArray(), 0, "");
            this.DataSetQuality = this.GetPartitionQuality(reduct);
        }

        protected virtual decimal GetPartitionQuality(IReduct reduct)
        {
            return new InformationMeasureWeights().Calc(reduct);
        }

        protected override void Generate()
        {
            
            ParallelOptions options = new ParallelOptions()
            {
                MaxDegreeOfParallelism = System.Math.Max(1, Environment.ProcessorCount / 2)
            };
#if DEBUG
            options.MaxDegreeOfParallelism = 1;
#endif
                        
            ReductStore localReductPool = new ReductStore(this.attributePermutations.Count);
            //foreach(var permutation in this.attributePermutations)
            Parallel.ForEach(this.attributePermutations, options, permutation =>
            {                                
                localReductPool.AddReduct(this.CalculateReduct(permutation, localReductPool));
            }
            );

            this.ReductStoreCollection.AddStore(localReductPool);
        }

        public override IReduct CreateReduct(int[] permutation, decimal epsilon, decimal[] weights, IReductStore reductStore = null, IReductStoreCollection reductStoreCollection = null)
        {
            EquivalenceClassCollection eqClasses = EquivalenceClassCollection.Create(permutation, this.DataStore, epsilon, weights);
            int len = permutation.Length;            

            eqClasses.WeightSum = this.DataSetQuality;
            eqClasses.ObjectsCount = this.DataStore.NumberOfRecords;
            
            this.KeepMajorDecisions(eqClasses, epsilon);

            int step = this.ReductionStep > 0 ? this.ReductionStep : 1;

            EquivalenceClassCollection newEqClasses = null;
            while (step >= 1)
            {
                for (int i = 0; i < len && step <= len; i += step)
                {
                    newEqClasses = this.Reduce(eqClasses, i, step, reductStore);
                    //reduction was successful
                    if (!Object.ReferenceEquals(newEqClasses, eqClasses)) 
                    {
                        eqClasses = newEqClasses;
                        len -= step;
                        i -= step;
                    }
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

        protected virtual bool CheckIsReduct(IReduct reduct, decimal epsilon)
        {
            if (this.GetPartitionQuality(reduct) >= ((Decimal.One - epsilon) * this.DataSetQuality))
                return true;
            return false;
        }

        public virtual IReduct CalculateReduct(int[] attributes, IReductStore reductStore = null, IReductStoreCollection reductStoreCollection = null)
        {
            return this.CreateReduct(attributes, this.Epsilon, this.WeightGenerator.Weights, reductStore, reductStoreCollection);
        }

        protected virtual void KeepMajorDecisions(EquivalenceClassCollection eqClasses, decimal epsilon = Decimal.Zero)
        {
            if (epsilon >= Decimal.One) return;
            foreach (EquivalenceClass eq in eqClasses)
                eq.KeepMajorDecisions(epsilon);
        }
        
        protected virtual EquivalenceClassCollection Reduce(EquivalenceClassCollection eqClasses, int attributeIdx, int length, IReductStore reductStore = null, IReductStoreCollection reductStoreCollection = null)
        {
            EquivalenceClassCollection newEqClasses = new EquivalenceClassCollection(
                this.DataStore,
                eqClasses.Attributes.RemoveAt(attributeIdx, length),
                eqClasses.Partitions.Count);
                
            newEqClasses.WeightSum = eqClasses.WeightSum;
            newEqClasses.ObjectsCount = eqClasses.ObjectsCount;

            foreach (EquivalenceClass eq in eqClasses)
            {
                long[] newInstance = eq.Instance.RemoveAt(attributeIdx, length);
                EquivalenceClass newEqClass = null;
                if (newEqClasses.Partitions.TryGetValue(newInstance, out newEqClass))
                {
                    //Update m_d
                    newEqClass.DecisionSet = newEqClass.DecisionSet.IntersectionFast(eq.DecisionSet);

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

                    newEqClasses.Partitions[newInstance] = newEqClass;
                }
            }

            return newEqClasses;
        }

        protected override IReduct CreateReductObject(int[] fieldIds, decimal epsilon, string id)
        {
            ReductWeights r = new ReductWeights(this.DataStore, fieldIds, epsilon, this.WeightGenerator.Weights);
            r.Id = id;
            return r;
        }
        
        protected override IReduct CreateReductObject(int[] fieldIds, decimal epsilon, string id, EquivalenceClassCollection eqClasses)
        {
            ReductWeights r = new ReductWeights(this.DataStore, fieldIds, epsilon, this.WeightGenerator.Weights, eqClasses);
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
            get { return ReductFactoryKeyHelper.GeneralizedMajorityDecision; }
        }

        public virtual IReductGenerator GetReductGenerator(Args args)
        {
            ReductGeneralizedMajorityDecisionGenerator rGen = new ReductGeneralizedMajorityDecisionGenerator();
            rGen.InitFromArgs(args);
            return rGen;
        }

        public virtual IPermutationGenerator GetPermutationGenerator(Args args)
        {
            DataStore dataStore = (DataStore)args.GetParameter(ReductGeneratorParamHelper.TrainData);
            return new PermutationGenerator(dataStore);
        }
    }
    
    
}