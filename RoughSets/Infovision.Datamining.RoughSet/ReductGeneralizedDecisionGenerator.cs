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

        public int MinReductLength { get; set; }
        public int MaxReductLength { get; set; }

        public override void InitFromArgs(Args args)
        {
            base.InitFromArgs(args);

            if (args.Exist(ReductGeneratorParamHelper.WeightGenerator))
                this.weightGenerator = (WeightGenerator)args.GetParameter(ReductGeneratorParamHelper.WeightGenerator);

            this.MinReductLength = 0;
            this.MaxReductLength = this.DataStore.DataStoreInfo.GetNumberOfFields(FieldTypes.Standard);
            
            if (args.Exist(ReductGeneratorParamHelper.MinReductLength))
                this.MinReductLength = (int)args.GetParameter(ReductGeneratorParamHelper.MinReductLength);

            if (args.Exist(ReductGeneratorParamHelper.MaxReductLength))
                this.MaxReductLength = (int)args.GetParameter(ReductGeneratorParamHelper.MaxReductLength);
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
                MaxDegreeOfParallelism = System.Math.Max(1, Environment.ProcessorCount - 1)
            };
#if DEBUG
            options.MaxDegreeOfParallelism = 1;
#endif

            this.ReductStoreCollection = new ReductStoreCollection(1);
            ReductStore localReductPool = new ReductStore(this.Permutations.Count);
            Parallel.ForEach(this.Permutations, options, permutation =>
            {
                int cut = this.MaxReductLength > 0
                        ? this.MaxReductLength
                        : permutation.Length;

                int[] attributes = new int[cut];
                for (int i = 0; i < cut; i++)
                    attributes[i] = permutation[i];

                IReduct reduct = this.CalculateReduct(attributes, localReductPool);
                
                localReductPool.DoAddReduct(reduct);
            });
            this.ReductStoreCollection.AddStore(localReductPool);
        }

        public override IReduct CreateReduct(int[] permutation, decimal epsilon, decimal[] weights, IReductStore reductStore = null)
        {            
            EquivalenceClassCollection eqClasses = EquivalenceClassCollection.Create(permutation, this.DataStore, epsilon, weights, false);

            eqClasses.CountWeightObjects = this.DataSetQuality;
            eqClasses.CountObjects = this.DataStore.NumberOfRecords;
            
            this.KeepMajorDecisions(eqClasses, epsilon);

            int len = permutation.Length;
            int step = this.ReductionStep > 0 ? this.ReductionStep : 1;
            while (step >= 1)
            {
                for (int i = 0; i < len && step <= len; i += step)
                {
                    EquivalenceClassCollection newEqClasses = this.Reduce(eqClasses, i, step, reductStore);

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

            eqClasses.RecalcEquivalenceClassStatistic(this.DataStore);
            return this.CreateReductObject(eqClasses.Attributes, epsilon, this.GetNextReductId().ToString(), eqClasses);
        }

        public virtual IReduct CalculateReduct(int[] attributes, IReductStore reductStore = null)
        {
            return this.CreateReduct(attributes, this.Epsilon, this.WeightGenerator.Weights, reductStore);
        }

        protected virtual void KeepMajorDecisions(EquivalenceClassCollection eqClasses, decimal epsilon = Decimal.Zero)
        {
            foreach (EquivalenceClass eq in eqClasses)
                eq.KeepMajorDecisions(epsilon);
        }
        
        protected virtual EquivalenceClassCollection Reduce(EquivalenceClassCollection eqClasses, int attributeIdx, int length, IReductStore reductStore = null)
        {
            EquivalenceClassCollection newEqClasses = new EquivalenceClassCollection(eqClasses.Attributes.RemoveAt(attributeIdx, length), this.DataStore.DataStoreInfo.NumberOfDecisionValues);
            newEqClasses.CountWeightObjects = eqClasses.CountWeightObjects;
            newEqClasses.CountObjects = eqClasses.CountObjects;

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
                    newEqClass.AddObjectInstances(eq.Instances);
                }
                else
                {
                    newEqClass = new EquivalenceClass(newInstance, this.DataStore, false);
                    newEqClass.DecisionSet = new PascalSet<long>(eq.DecisionSet);
                    newEqClass.WeightSum += eq.WeightSum;
                    newEqClass.Instances = new Dictionary<int, decimal>(eq.Instances);

                    newEqClasses.Partitions[newInstance] = newEqClass;
                }
            }

            return newEqClasses;
        }               

        protected override IReduct CreateReductObject(int[] fieldIds, decimal epsilon, string id)
        {
            ReductWeights r = new ReductWeights(this.DataStore, fieldIds, this.WeightGenerator.Weights, epsilon);
            r.Id = id;
            return r;
        }
        
        protected override IReduct CreateReductObject(int[] fieldIds, decimal epsilon, string id, EquivalenceClassCollection eqClasses)
        {
            ReductWeights r = new ReductWeights(this.DataStore, fieldIds, this.WeightGenerator.Weights, epsilon, eqClasses);
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

    public class ReductGeneralizedMajorityDecisionApproximateGenerator : ReductGeneralizedMajorityDecisionGenerator
    {
        public bool UseExceptionRules { get; set; }
        public bool ExceptionRulesAsGaps { get; set; }
        public decimal Gamma { get; set; }
        protected decimal WeightDropLimit { get; set; }
        protected decimal ObjectWeightSum { get; set; }


        protected override void Generate()
        {
            ParallelOptions options = new ParallelOptions();
            options.MaxDegreeOfParallelism = System.Math.Max(1, Environment.ProcessorCount / 2);
#if DEBUG
            options.MaxDegreeOfParallelism = 1;
#endif
            
            this.ObjectWeightSum = this.WeightGenerator.Weights.Sum();
            this.WeightDropLimit = Decimal.Round((Decimal.One - this.Epsilon) * this.ObjectWeightSum, 17);
            
            if (this.UseExceptionRules)
            {
                this.ReductStoreCollection = new ReductStoreCollection(this.Permutations.Count);
                Parallel.ForEach(this.Permutations, options, permutation =>
                {
                    int cut = this.MaxReductLength > 0
                        ? this.MaxReductLength
                        : permutation.Length;

                    int[] attributes = new int[cut];
                    for (int i = 0; i < cut; i++)
                        attributes[i] = permutation[i];
                    
                    ReductStore localReductPool = new ReductStore(1);
                    localReductPool.DoAddReduct(this.CalculateReduct(attributes, localReductPool));
                    this.ReductStoreCollection.AddStore(localReductPool);
                });
            }
            else
            {
                this.ReductStoreCollection = new ReductStoreCollection(1);
                ReductStore localReductPool = new ReductStore(this.Permutations.Count);
                Parallel.ForEach(this.Permutations, options, permutation =>
                {
                    int cut = this.MinReductLength > 0
                        ? this.MaxReductLength
                        : permutation.Length;

                    int[] attributes = new int[cut];
                    for (int i = 0; i < cut; i++)
                        attributes[i] = permutation[i];

                    IReduct r = this.CalculateReduct(attributes, localReductPool);
                    localReductPool.DoAddReduct(r);
                });

                this.ReductStoreCollection.AddStore(localReductPool);
            }
        }

        public override IReduct CreateReduct(int[] permutation, decimal epsilon, decimal[] weights, IReductStore reductStore = null)
        {
            EquivalenceClassCollection eqClasses =
                (this.UseExceptionRules && reductStore != null)
                    ? EquivalenceClassCollection.Create(permutation, this.DataStore, epsilon, weights, true)
                    : EquivalenceClassCollection.Create(permutation, this.DataStore, epsilon, weights, false);

            eqClasses.CountWeightObjects = this.ObjectWeightSum;
            eqClasses.CountObjects = this.DataStore.NumberOfRecords;

            this.KeepMajorDecisions(eqClasses, Decimal.Zero);

            int len = permutation.Length;
            int step = this.ReductionStep > 0 ? this.ReductionStep : 1;
            while (step >= 1)
            {
                for (int i = 0; i < len && step <= len; i += step)
                {
                    EquivalenceClassCollection newEqClasses = this.Reduce(eqClasses, i, step, reductStore);

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

            eqClasses.RecalcEquivalenceClassStatistic(this.DataStore);
            return this.CreateReductObject(eqClasses.Attributes, epsilon, this.GetNextReductId().ToString(), eqClasses);
        }
        
        protected override EquivalenceClassCollection Reduce(EquivalenceClassCollection eqClasses, int attributeIdx, int length, IReductStore reductStore = null)
        {
            var newAttributes = eqClasses.Attributes.RemoveAt(attributeIdx, length);           
            EquivalenceClassCollection newEqClasses = new EquivalenceClassCollection(newAttributes, this.DataStore.DataStoreInfo.NumberOfDecisionValues);
            newEqClasses.CountWeightObjects = eqClasses.CountWeightObjects;
            newEqClasses.CountObjects = eqClasses.CountObjects;

            EquivalenceClass[] eqArray =  eqClasses.Partitions.Values.ToArray();
            ReductStore localReductStore = new ReductStore();
            
#if !DEBUG            
            eqArray.Shuffle();
#endif
                        
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
                        newEqClass.AddObjectInstances(eq.Instances);
                    }
                    else
                    {
                        newEqClasses.CountWeightObjects -= eq.WeightSum;
                        newEqClasses.CountObjects -= eq.NumberOfObjects;

                        if (Decimal.Round(newEqClasses.CountWeightObjects, 17) < this.WeightDropLimit)
                            return eqClasses;

                        if (this.UseExceptionRules && reductStore != null)
                        {
                            Bireduct exceptionReduct = new Bireduct(this.DataStore,
                                newAttributes, 
                                eq.ObjectIndexes.ToArray(), 
                                this.Epsilon,
                                this.WeightGenerator.Weights);

                            exceptionReduct.IsException = this.UseExceptionRules;
                            exceptionReduct.IsGap = this.ExceptionRulesAsGaps;

                            localReductStore.AddReduct(exceptionReduct);
                        }
                    }
                }
                else
                {
                    bool localUseStat = (this.UseExceptionRules && reductStore != null);
                    
                    newEqClass = new EquivalenceClass(newInstance, this.DataStore, localUseStat);
                    newEqClass.Instances = new Dictionary<int, decimal>(eq.Instances);
                    newEqClass.DecisionSet = new PascalSet<long>(eq.DecisionSet);
                    newEqClass.WeightSum += eq.WeightSum;

                    newEqClasses.Partitions[newInstance] = newEqClass;
                }
            }

            for (int i = 0; i < localReductStore.Count; i++)
                reductStore.AddReduct(localReductStore.GetReduct(i));

            return newEqClasses;
        }

        public override void InitFromArgs(Args args)
        {
            base.InitFromArgs(args);

            if (args.Exist(ReductGeneratorParamHelper.UseExceptionRules))
                this.UseExceptionRules = (bool)args.GetParameter(ReductGeneratorParamHelper.UseExceptionRules);

            if (args.Exist(ReductGeneratorParamHelper.ExceptionRulesAsGaps))
                this.ExceptionRulesAsGaps = (bool)args.GetParameter(ReductGeneratorParamHelper.ExceptionRulesAsGaps);
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
            DataStore dataStore = (DataStore)args.GetParameter(ReductGeneratorParamHelper.TrainData);
            return new PermutationGenerator(dataStore);
        }
    }
}