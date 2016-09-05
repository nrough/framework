using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Infovision.Data;
using Infovision.Utils;

namespace Infovision.Datamining.Roughset
{
    public class ReductGeneralizedMajorityDecisionApproximateGenerator : ReductGeneralizedMajorityDecisionGenerator
    {
        #region Members

        private IComparer<EquivalenceClass> lengthComparer;
        private IComparer<EquivalenceClass> lengthComparerReverse;

        #endregion Members

        #region Properties

        public bool UseExceptionRules { get; set; }
        public SortDirection EquivalenceClassSortDirection { get; set; }
        public decimal Gamma { get; set; }
        protected decimal WeightDropLimit { get; set; }
        protected decimal ObjectWeightSum { get; set; }

        #endregion Properties

        #region Constructors

        public ReductGeneralizedMajorityDecisionApproximateGenerator()
            : base()
        {
            lengthComparer = Comparer<EquivalenceClass>.Create((a, b) => a.NumberOfObjects.CompareTo(b.NumberOfObjects));
            lengthComparerReverse = Comparer<EquivalenceClass>.Create((a, b) => b.NumberOfObjects.CompareTo(a.NumberOfObjects));
        }

        #endregion Constructors

        #region Methods

        protected override void Generate()
        {
            if (this.UseExceptionRules)
            {
                ParallelOptions options = new ParallelOptions()
                {
                    MaxDegreeOfParallelism = InfovisionConfiguration.MaxDegreeOfParallelism
                };

                Parallel.ForEach(this.attributePermutations, options, permutation =>
                //foreach (var permutation in this.attributePermutations)
                {
                    ReductStore localReductPool = new ReductStore();
                    localReductPool.AllowDuplicates = true;
                    localReductPool.DoAddReduct(this.CalculateReduct(permutation, localReductPool, this.ReductStoreCollection));

                    this.ReductStoreCollection.AddStore(localReductPool);
                }
                );

                this.ReductStoreCollection.ReductPerStore = true;
            }
            else
            {
                base.Generate();
            }
        }

        protected override EquivalenceClassCollection Reduce(
            EquivalenceClassCollection eqClasses,
            int attributeIdx,
            int length,
            IReductStore reductStore = null,
            IReductStoreCollection reductStoreCollection = null)
        {
            if (this.UseExceptionRules)
                return this.ReduceWithExceptions(eqClasses, attributeIdx, length, reductStore, reductStoreCollection);
            return this.ReduceWithoutExceptions(eqClasses, attributeIdx, length, reductStore, reductStoreCollection);
        }

        private EquivalenceClassCollection ReduceWithExceptions(
            EquivalenceClassCollection eqClasses,
            int attributeIdx,
            int length,
            IReductStore reductStore,
            IReductStoreCollection reductStoreCollection = null)
        {
            var newAttributes = eqClasses.Attributes.RemoveAt(attributeIdx, length);
            EquivalenceClassCollection newEqClasses = new EquivalenceClassCollection(this.DataStore, newAttributes, eqClasses.Partitions.Count);
            newEqClasses.WeightSum = eqClasses.WeightSum;
            newEqClasses.NumberOfObjects = eqClasses.NumberOfObjects;

            EquivalenceClass[] eqArray = eqClasses.Partitions.Values.ToArray();
            this.SortEquivalenceClassArray(eqArray);

            EquivalenceClassCollection exceptionEqClasses = null;
            EquivalenceClass exeptionEq = null;

            foreach (EquivalenceClass eq in eqArray)
            {
                var newInstance = eq.Instance.RemoveAt(attributeIdx, length);

                EquivalenceClass newEqClass = null;
                if (newEqClasses.Partitions.TryGetValue(newInstance, out newEqClass))
                {
                    //PascalSet<long> newDecisionSet = newEqClass.DecisionSet.IntersectionFast(eq.DecisionSet);

                    HashSet<long> newDecisionSet = new HashSet<long>(newEqClass.DecisionSet);
                    newDecisionSet.IntersectWith(eq.DecisionSet);

                    if (newDecisionSet.Count > 0)
                    {
                        //Update |X * E|w as new weighted average
                        newEqClass.AvgConfidenceWeight += eq.AvgConfidenceWeight;

                        //Update |X * E| count
                        newEqClass.AvgConfidenceSum += eq.AvgConfidenceSum;

                        //Update |E|
                        newEqClass.AddObjectInstances(eq.Instances);

                        //Update |E|w
                        newEqClass.WeightSum += eq.WeightSum;

                        //Update m_d
                        newEqClass.DecisionSet = newDecisionSet;
                    }
                    else
                    {
                        newEqClasses.WeightSum -= eq.WeightSum;
                        newEqClasses.NumberOfObjects -= eq.NumberOfObjects;

                        if (newEqClasses.WeightSum < this.WeightDropLimit)
                            return eqClasses;

                        if (exceptionEqClasses == null)
                        {
                            exceptionEqClasses = new EquivalenceClassCollection(this.DataStore, eqClasses.Attributes);
                        }

                        exeptionEq = new EquivalenceClass(eq.Instance, eq.Instances, eq.DecisionSet);
                        exeptionEq.AvgConfidenceWeight = eq.AvgConfidenceWeight;
                        exeptionEq.AvgConfidenceSum = eq.AvgConfidenceSum;
                        exeptionEq.WeightSum = eq.WeightSum;

                        exceptionEqClasses.Partitions.Add(eq.Instance, exeptionEq);
                    }
                }
                else
                {
                    newEqClass = new EquivalenceClass(newInstance, eq.Instances, eq.DecisionSet);
                    newEqClass.AvgConfidenceWeight = eq.AvgConfidenceWeight;
                    newEqClass.AvgConfidenceSum = eq.AvgConfidenceSum;
                    newEqClass.WeightSum = eq.WeightSum;

                    newEqClasses.Partitions.Add(newInstance, newEqClass);
                }
            }

            if (exceptionEqClasses != null)
            {
                //exceptionEqClasses.RecalcEquivalenceClassStatistic(this.DataStore);
                ReductWeights exception = new ReductWeights(
                    this.DataStore,
                    eqClasses.Attributes,
                    this.Epsilon,
                    this.WeightGenerator.Weights,
                    exceptionEqClasses);
                exception.IsException = true;

                reductStore.AddReduct(exception);
            }

            return newEqClasses;
        }

        private void SortEquivalenceClassArray(EquivalenceClass[] eqArray)
        {
            switch (this.EquivalenceClassSortDirection)
            {
                case SortDirection.Ascending:
                    Array.Sort<EquivalenceClass>(eqArray, lengthComparer);
                    break;

                case SortDirection.Descending:
                    Array.Sort<EquivalenceClass>(eqArray, lengthComparerReverse);
                    break;

                case SortDirection.Random:
                    eqArray.Shuffle();
                    break;

                case SortDirection.None:
                    //do nothing
                    break;

                default:
                    eqArray.Shuffle();
                    break;
            }
        }

        private EquivalenceClassCollection ReduceWithoutExceptions(
            EquivalenceClassCollection eqClasses,
            int attributeIdx,
            int length,
            IReductStore reductStore,
            IReductStoreCollection reductStoreCollection = null)
        {
            var newAttributes = eqClasses.Attributes.RemoveAt(attributeIdx, length);
            EquivalenceClassCollection newEqClasses = new EquivalenceClassCollection(this.DataStore, newAttributes, eqClasses.Partitions.Count);
            newEqClasses.WeightSum = eqClasses.WeightSum;
            newEqClasses.NumberOfObjects = eqClasses.NumberOfObjects;

            EquivalenceClass[] eqArray = eqClasses.Partitions.Values.ToArray();
            this.SortEquivalenceClassArray(eqArray);

            foreach (EquivalenceClass eq in eqArray)
            {
                var newInstance = eq.Instance.RemoveAt(attributeIdx, length);

                EquivalenceClass newEqClass = null;
                if (newEqClasses.Partitions.TryGetValue(newInstance, out newEqClass))
                {
                    //PascalSet<long> newDecisionSet = newEqClass.DecisionSet.IntersectionFast(eq.DecisionSet);
                    HashSet<long> newDecisionSet = new HashSet<long>(newEqClass.DecisionSet);
                    newDecisionSet.IntersectWith(eq.DecisionSet);

                    if (newDecisionSet.Count > 0)
                    {
                        //Update |X * E|w as new weighted average
                        newEqClass.AvgConfidenceWeight += eq.AvgConfidenceWeight;

                        //Update |X * E| count
                        newEqClass.AvgConfidenceSum += eq.AvgConfidenceSum;

                        //Update |E|
                        newEqClass.AddObjectInstances(eq.Instances);

                        //Update |E|w
                        newEqClass.WeightSum += eq.WeightSum;

                        //Update m_d
                        newEqClass.DecisionSet = newDecisionSet;
                    }
                    else
                    {
                        newEqClasses.WeightSum -= eq.WeightSum;
                        newEqClasses.NumberOfObjects -= eq.NumberOfObjects;

                        if (newEqClasses.WeightSum < this.WeightDropLimit)
                            return eqClasses;

                        //Update |E|
                        newEqClass.AddObjectInstances(eq.Instances);
                        //Update |E|w
                        newEqClass.WeightSum += eq.WeightSum;
                    }
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

        public override void InitDefaultParameters()
        {
            base.InitDefaultParameters();
            this.UseExceptionRules = true;
            this.EquivalenceClassSortDirection = SortDirection.Random;
        }

        public override void InitFromArgs(Args args)
        {
            base.InitFromArgs(args);

            if (args.Exist(ReductGeneratorParamHelper.UseExceptionRules))
                this.UseExceptionRules = (bool)args.GetParameter(ReductGeneratorParamHelper.UseExceptionRules);

            if (args.Exist(ReductGeneratorParamHelper.EquivalenceClassSortDirection))
                this.EquivalenceClassSortDirection = (SortDirection)args.GetParameter(ReductGeneratorParamHelper.EquivalenceClassSortDirection);

            if (this.UseExceptionRules)
            {
                this.InitReductStoreCollection(this.Permutations.Count);
            }

            this.Gamma = this.Epsilon;
            this.Epsilon = 0;

            this.ObjectWeightSum = this.WeightGenerator.Weights.Sum();
            this.WeightDropLimit = (Decimal.One - this.Gamma) * this.ObjectWeightSum;
        }

        #endregion Methods
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