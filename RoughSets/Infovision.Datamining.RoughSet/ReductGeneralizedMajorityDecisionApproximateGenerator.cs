using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Infovision.Data;
using Infovision.Utils;

namespace Infovision.Datamining.Roughset
{
    public class ReductGeneralizedMajorityDecisionApproximateGenerator : ReductGeneralizedMajorityDecisionGenerator
    {
        #region Properties

        public bool UseExceptionRules { get; set; }
        public SortDirection EquivalenceClassSortDirection { get; set; }
        public decimal Gamma { get; set; }        
        protected decimal WeightDropLimit { get; set; }
        protected decimal ObjectWeightSum { get; set; }

        #endregion

        #region Methods

        protected override void Generate()
        {            
            if (this.UseExceptionRules)
            {
                ParallelOptions options = new ParallelOptions()
                {
                    MaxDegreeOfParallelism = System.Math.Max(1, Environment.ProcessorCount / 2)
                };

#if DEBUG
                options.MaxDegreeOfParallelism = 1;
#endif
                
                Parallel.ForEach(this.attributePermutations, options, permutation =>
                //foreach (var permutation in this.attributePermutations)
                {                    
                    ReductStore localReductPool = new ReductStore();
                    localReductPool.AddReduct(this.CalculateReduct(permutation, localReductPool));
                    this.ReductStoreCollection.AddStore(localReductPool);
                }
                );
            }
            else
            {
                base.Generate();
            }
        }

        protected override EquivalenceClassCollection Reduce(EquivalenceClassCollection eqClasses, int attributeIdx, int length, IReductStore reductStore = null)
        {
            if (this.UseExceptionRules)
                return this.ReduceWithExceptions(eqClasses, attributeIdx, length, reductStore);            
            return this.ReduceWithoutExceptions(eqClasses, attributeIdx, length, reductStore);            
        }

        private EquivalenceClassCollection ReduceWithExceptions(EquivalenceClassCollection eqClasses, int attributeIdx, int length, IReductStore reductStore)
        {
            if (reductStore == null)
                throw new ArgumentNullException("Reduct generating with exceptions requires initialized reductStore", "reductStore");

            var newAttributes = eqClasses.Attributes.RemoveAt(attributeIdx, length);            
            EquivalenceClassCollection newEqClasses = new EquivalenceClassCollection(this.DataStore, newAttributes);
            newEqClasses.CountWeightObjects = eqClasses.CountWeightObjects;
            newEqClasses.CountObjects = eqClasses.CountObjects;

            EquivalenceClass[] eqArray = eqClasses.Partitions.Values.ToArray();
            ReductStore localReductStore = new ReductStore();
         
            switch(this.EquivalenceClassSortDirection)
            {
                case SortDirection.Ascending:
                    Array.Sort<EquivalenceClass>(eqArray, Comparer<EquivalenceClass>.Create((a, b) => a.NumberOfObjects.CompareTo(b.NumberOfObjects)));
                    break;

                case SortDirection.Descending:
                    Array.Sort<EquivalenceClass>(eqArray, Comparer<EquivalenceClass>.Create((a, b) => b.NumberOfObjects.CompareTo(a.NumberOfObjects)));
                    break;

                default :
                    eqArray.Shuffle();
                    break;
            }

            EquivalenceClassCollection exceptionEqClasses = null;
            ReductWeights exception = null;
            EquivalenceClass exeptionEq = null;

            foreach (EquivalenceClass eq in eqArray)
            {
                var newInstance = eq.Instance.RemoveAt(attributeIdx, length);

                EquivalenceClass newEqClass = null;
                if (newEqClasses.Partitions.TryGetValue(newInstance, out newEqClass))
                {
                    PascalSet<long> newDecisionSet = newEqClass.DecisionSet.Intersection(eq.DecisionSet);

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
                        newEqClasses.CountWeightObjects -= eq.WeightSum;
                        newEqClasses.CountObjects -= eq.NumberOfObjects;

                        if (Decimal.Round(newEqClasses.CountWeightObjects, 17) < this.WeightDropLimit)
                            return eqClasses;

                        if (exception == null)
                        {
                            exceptionEqClasses = new EquivalenceClassCollection(this.DataStore, eqClasses.Attributes);

                            exeptionEq = new EquivalenceClass(eq.Instance, this.DataStore);
                            exeptionEq.AvgConfidenceWeight = eq.AvgConfidenceWeight;
                            exeptionEq.AvgConfidenceSum = eq.AvgConfidenceSum;
                            exeptionEq.Instances = new Dictionary<int, decimal>(eq.Instances);
                            exeptionEq.WeightSum = eq.WeightSum;
                            exeptionEq.DecisionSet = new PascalSet<long>(eq.DecisionSet);

                            exceptionEqClasses.Partitions.Add(newInstance, exeptionEq);
                            
                            exception = new ReductWeights(this.DataStore, newAttributes, this.Epsilon, this.WeightGenerator.Weights);
                            exception.SetEquivalenceClassCollection(exceptionEqClasses);
                            exception.IsException = true;
                        }
                        else
                        {
                            exeptionEq = new EquivalenceClass(eq.Instance, this.DataStore);
                            exeptionEq.AvgConfidenceWeight = eq.AvgConfidenceWeight;
                            exeptionEq.AvgConfidenceSum = eq.AvgConfidenceSum;
                            exeptionEq.Instances = new Dictionary<int, decimal>(eq.Instances);
                            exeptionEq.WeightSum = eq.WeightSum;
                            exeptionEq.DecisionSet = new PascalSet<long>(eq.DecisionSet);

                            exceptionEqClasses.Partitions.Add(eq.Instance, exeptionEq);
                        }
                    }
                }
                else
                {
                    newEqClass = new EquivalenceClass(newInstance, this.DataStore);
                    newEqClass.AvgConfidenceWeight = eq.AvgConfidenceWeight;
                    newEqClass.AvgConfidenceSum = eq.AvgConfidenceSum;
                    newEqClass.Instances = new Dictionary<int, decimal>(eq.Instances);
                    newEqClass.WeightSum = eq.WeightSum;
                    newEqClass.DecisionSet = new PascalSet<long>(eq.DecisionSet);

                    newEqClasses.Partitions.Add(newInstance, newEqClass);
                }
            }

            if (exception != null)
            {
                exceptionEqClasses.RecalcEquivalenceClassStatistic(this.DataStore);
                reductStore.AddReduct(exception);
            }            

            return newEqClasses;
        }

        private EquivalenceClassCollection ReduceWithoutExceptions(EquivalenceClassCollection eqClasses, int attributeIdx, int length, IReductStore reductStore)
        {
            var newAttributes = eqClasses.Attributes.RemoveAt(attributeIdx, length);            
            EquivalenceClassCollection newEqClasses = new EquivalenceClassCollection(this.DataStore, newAttributes);
            newEqClasses.CountWeightObjects = eqClasses.CountWeightObjects;
            newEqClasses.CountObjects = eqClasses.CountObjects;

            EquivalenceClass[] eqArray = eqClasses.Partitions.Values.ToArray();

            switch (this.EquivalenceClassSortDirection)
            {
                case SortDirection.Ascending:
                    Array.Sort<EquivalenceClass>(eqArray, Comparer<EquivalenceClass>.Create((a, b) => a.NumberOfObjects.CompareTo(b.NumberOfObjects)));
                    break;

                case SortDirection.Descending:
                    Array.Sort<EquivalenceClass>(eqArray, Comparer<EquivalenceClass>.Create((a, b) => b.NumberOfObjects.CompareTo(a.NumberOfObjects)));
                    break;

                default:
                    eqArray.Shuffle();
                    break;
            }

            foreach (EquivalenceClass eq in eqArray)
            {
                var newInstance = eq.Instance.RemoveAt(attributeIdx, length);

                EquivalenceClass newEqClass = null;
                if (newEqClasses.Partitions.TryGetValue(newInstance, out newEqClass))
                {
                    PascalSet<long> newDecisionSet = newEqClass.DecisionSet.Intersection(eq.DecisionSet);

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
                        newEqClasses.CountWeightObjects -= eq.WeightSum;
                        newEqClasses.CountObjects -= eq.NumberOfObjects;

                        if (Decimal.Round(newEqClasses.CountWeightObjects, 17) < this.WeightDropLimit)
                            return eqClasses;

                        //Update |E|
                        newEqClass.AddObjectInstances(eq.Instances);
                        //Update |E|w
                        newEqClass.WeightSum += eq.WeightSum;
                    }
                }
                else
                {
                    newEqClass = new EquivalenceClass(newInstance, this.DataStore);
                    newEqClass.AvgConfidenceWeight = eq.AvgConfidenceWeight;
                    newEqClass.AvgConfidenceSum = eq.AvgConfidenceSum;
                    newEqClass.Instances = new Dictionary<int, decimal>(eq.Instances);
                    newEqClass.WeightSum = eq.WeightSum;
                    newEqClass.DecisionSet = new PascalSet<long>(eq.DecisionSet);
                                        
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
                this.UseExceptionRules = (bool) args.GetParameter(ReductGeneratorParamHelper.UseExceptionRules);

            if (args.Exist(ReductGeneratorParamHelper.EquivalenceClassSortDirection))
                this.EquivalenceClassSortDirection = (SortDirection) args.GetParameter(ReductGeneratorParamHelper.EquivalenceClassSortDirection);

            if (this.UseExceptionRules)
            {
                this.InitReductStoreCollection(this.Permutations.Count);
            }

            this.Gamma = this.Epsilon;
            this.Epsilon = 0;

            this.ObjectWeightSum = this.WeightGenerator.Weights.Sum();
            this.WeightDropLimit = Decimal.Round((Decimal.One - this.Gamma) * this.ObjectWeightSum, 17);
        }

        #endregion
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
