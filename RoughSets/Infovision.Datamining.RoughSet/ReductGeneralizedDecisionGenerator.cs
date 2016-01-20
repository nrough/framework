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

        protected bool IsQualityCalculated
        {
            get;
            set;
        }

        public IReductStoreCollection ReductStoreCollection
        {
            get;
            protected set;
        }        

        public override void InitFromArgs(Args args)
        {
            base.InitFromArgs(args);

            if (args.Exist(ReductGeneratorParamHelper.WeightGenerator))
                this.weightGenerator = (WeightGenerator)args.GetParameter(ReductGeneratorParamHelper.WeightGenerator);
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

        public override void Generate()
        {
            ParallelOptions options = new ParallelOptions()
            {
                MaxDegreeOfParallelism = System.Math.Max(1, Environment.ProcessorCount / 2)
            };
#if DEBUG
            options.MaxDegreeOfParallelism = 1;
#endif

            this.ReductStoreCollection = new ReductStoreCollection(1);
            ReductStore localReductPool = new ReductStore(this.Permutations.Count);
            //foreach (Permutation permutation in this.Permutations)
            Parallel.ForEach(this.Permutations, options, permutation =>
            {
                localReductPool.DoAddReduct(this.CalculateReduct(permutation.ToArray(), localReductPool));
            });
            this.ReductStoreCollection.AddStore(localReductPool);               
        }

        public override IReduct CreateReduct(int[] permutation, decimal epsilon, decimal[] weights)
        {
            EquivalenceClassCollection eqClasses = EquivalenceClassCollection.Create(permutation, this.DataStore, epsilon, weights, false);

            eqClasses.EqWeightSum = this.DataSetQuality;

            this.KeepMajorDecisions(eqClasses, epsilon);

            int len = permutation.Length;
            int step = this.ReductionStep > 0 ? this.ReductionStep : 1;
            while (step >= 1)
            {
                for (int i = 0; i < len && step <= len; i += step)
                {
                    EquivalenceClassCollection newEqClasses = this.Reduce(eqClasses, i, step);

                    //reduction was made
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

            return this.CreateReductObject(eqClasses.Attributes, epsilon, this.GetNextReductId().ToString());
        }

        protected virtual void KeepMajorDecisions(EquivalenceClassCollection eqClasses, decimal epsilon = Decimal.Zero)
        {
            foreach (EquivalenceClass eq in eqClasses)
                eq.KeepMajorDecisions(epsilon);
        }

        public virtual IReduct CalculateReduct(int[] attributes, IReductStore reductStore = null)
        {
            return this.CreateReduct(attributes, this.Epsilon, this.WeightGenerator.Weights);
        }

        protected virtual EquivalenceClassCollection Reduce(EquivalenceClassCollection eqClasses, int attributeIdx, int length, IReductStore reductStore = null)
        {
            EquivalenceClassCollection newEqClasses = new EquivalenceClassCollection(eqClasses.Attributes.RemoveAt(attributeIdx, length));

            foreach (EquivalenceClass eq in eqClasses)
            {
                long[] newInstance = eq.Instance.RemoveAt(attributeIdx, length);
                EquivalenceClass newEqClass = null;
                if (newEqClasses.Partitions.TryGetValue(newInstance, out newEqClass))
                {
                    newEqClass.DecisionSet = newEqClass.DecisionSet.Intersection(eq.DecisionSet);

                    if (newEqClass.DecisionSet.Count == 0)
                        return eqClasses;

                    newEqClass.WeightSum += eq.WeightSum;
                }
                else
                {
                    newEqClass = new EquivalenceClass(newInstance, this.DataStore, false);
                    newEqClass.DecisionSet = new PascalSet<long>(eq.DecisionSet);
                    newEqClass.WeightSum += eq.WeightSum;
                    newEqClasses.Partitions[newInstance] = newEqClass;
                }
            }

            return newEqClasses;
        }
        
        protected virtual EquivalenceClassCollection Reduce(EquivalenceClassCollection eqClasses, int attributeIdx, IReductStore reductStore = null)
        {
            return this.Reduce(eqClasses, attributeIdx, 1, reductStore);
        }

        protected override IReduct CreateReductObject(int[] fieldIds, decimal epsilon, string id)
        {
            ReductWeights r = new ReductWeights(this.DataStore, fieldIds, this.WeightGenerator.Weights, epsilon);
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
            DataStore dataStore = (DataStore)args.GetParameter(ReductGeneratorParamHelper.DataStore);
            return new PermutationGenerator(dataStore);
        }
    }

    public class ReductGeneralizedMajorityDecisionApproximateGenerator : ReductGeneralizedMajorityDecisionGenerator
    {
        public bool UseExceptionRules { get; set; }

        public override void Generate()
        {
            ParallelOptions options = new ParallelOptions();
            options.MaxDegreeOfParallelism = System.Math.Max(1, Environment.ProcessorCount / 2);
#if DEBUG
            options.MaxDegreeOfParallelism = 1;
#endif

            if (this.UseExceptionRules)
            {
                this.ReductStoreCollection = new ReductStoreCollection(this.Permutations.Count);
                //foreach (Permutation permutation in this.Permutations)
                Parallel.ForEach(this.Permutations, options, permutation =>
                {
                    ReductStore localReductPool = new ReductStore(1);
                    localReductPool.DoAddReduct(this.CalculateReduct(permutation.ToArray(), localReductPool));
                    this.ReductStoreCollection.AddStore(localReductPool);
                });
            }
            else
            {
                this.ReductStoreCollection = new ReductStoreCollection(1);
                ReductStore localReductPool = new ReductStore(this.Permutations.Count);
                //foreach (Permutation permutation in this.Permutations)
                Parallel.ForEach(this.Permutations, options, permutation =>
                {
                    //Console.WriteLine("P: {0}", permutation);

                    IReduct r = this.CalculateReduct(permutation.ToArray(), localReductPool);
                    Console.WriteLine(r.ToString());
                    localReductPool.DoAddReduct(r);
                });
                this.ReductStoreCollection.AddStore(localReductPool);
            }
        }
        
        protected override EquivalenceClassCollection Reduce(EquivalenceClassCollection eqClasses, int attributeIdx, int length, IReductStore reductStore = null)
        {
            var newAttributes = eqClasses.Attributes.RemoveAt(attributeIdx, length);

            //if (newAttributes.Length == 0)
            //{
            //    Console.WriteLine("Empty!");
            //}

            EquivalenceClassCollection newEqClasses = new EquivalenceClassCollection(newAttributes);
            newEqClasses.EqWeightSum = eqClasses.EqWeightSum;
   
            EquivalenceClass[] eqArray =  eqClasses.Partitions.Values.ToArray();
            eqArray.Shuffle();

            decimal weightDropLimit = Decimal.Round((Decimal.One - this.Epsilon) * this.DataSetQuality, 17);
            //decimal newEqWeightSum = Decimal.Zero;

            foreach(EquivalenceClass eq in eqArray)
            {
                var newInstance = eq.Instance.RemoveAt(attributeIdx, length);

                EquivalenceClass newEqClass = null;
                if (newEqClasses.Partitions.TryGetValue(newInstance, out newEqClass))
                {
                    PascalSet<long> newDecisionSet = newEqClass.DecisionSet.Intersection(eq.DecisionSet);

                    if (newDecisionSet.Count > 0)
                    {
                        newEqClass.DecisionSet = newDecisionSet;
                        newEqClass.WeightSum += eq.WeightSum;
                        //newEqWeightSum += eq.WeightSum;
                        
                        if(this.UseExceptionRules)
                            newEqClass.AddObjectInstances(eq.Instances);
                    }
                    else
                    {
                        newEqClasses.EqWeightSum -= eq.WeightSum;
                        if (Decimal.Round(newEqClasses.EqWeightSum, 17) < weightDropLimit)
                            return eqClasses;

                        if (this.UseExceptionRules && reductStore != null)
                        {
                            Bireduct exceptionReduct = new Bireduct(this.DataStore,
                                newAttributes, 
                                eq.ObjectIndexes.ToArray(), 
                                this.Epsilon,
                                this.WeightGenerator.Weights);

                            exceptionReduct.IsException = true;
                            reductStore.AddReduct(exceptionReduct);
                        }
                    }
                }
                else
                {
                    bool localUseStat = (this.UseExceptionRules && reductStore != null);
                    newEqClass = new EquivalenceClass(newInstance, this.DataStore, localUseStat);
                    newEqClass.DecisionSet = new PascalSet<long>(eq.DecisionSet);
                    newEqClass.WeightSum += eq.WeightSum;
                    //newEqWeightSum += eq.WeightSum;
                    
                     if (this.UseExceptionRules)
                        newEqClass.AddObjectInstances(eq.Instances);

                    newEqClasses.Partitions[newInstance] = newEqClass;
                }
            }

            //Check
            //if (Decimal.Round(newEqWeightSum, 17) != Decimal.Round(newEqClasses.EqWeightSum, 17))
            //{
            //    throw new InvalidOperationException("");
            //}

            return newEqClasses;
        }

        public override void InitFromArgs(Args args)
        {
            base.InitFromArgs(args);

            if (args.Exist(ReductGeneratorParamHelper.UseExceptionRules))
                this.UseExceptionRules = (bool)args.GetParameter(ReductGeneratorParamHelper.UseExceptionRules);
        }

        protected override void KeepMajorDecisions(EquivalenceClassCollection eqClasses, decimal epsilon = Decimal.Zero)
        {
            base.KeepMajorDecisions(eqClasses, Decimal.Zero);
        }        
    }

    public class ReductGeneralizedMajorityDecisionApproximateFactory : IReductFactory
    {
        public virtual string FactoryKey
        {
            get { return ReductFactoryKeyHelper.GeneralizedMajorityDecisionApproximate; }
        }

        public virtual IReductGenerator GetReductGenerator(Args args)
        {
            ReductGeneralizedMajorityDecisionApproximateGenerator rGen = new ReductGeneralizedMajorityDecisionApproximateGenerator();
            rGen.InitFromArgs(args);
            return rGen;
        }

        public virtual IPermutationGenerator GetPermutationGenerator(Args args)
        {
            DataStore dataStore = (DataStore)args.GetParameter(ReductGeneratorParamHelper.DataStore);
            return new PermutationGenerator(dataStore);
        }
    }
}