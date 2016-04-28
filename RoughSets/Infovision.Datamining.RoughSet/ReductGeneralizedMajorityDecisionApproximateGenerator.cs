﻿using System;
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
        public decimal Gamma { get; set; }        
        protected decimal WeightDropLimit { get; set; }
        protected decimal ObjectWeightSum { get; set; }

        #endregion

        #region Methods

        protected override void Generate()
        {            
            if (this.UseExceptionRules)
            {
                /*
                ParallelOptions options = new ParallelOptions();
                options.MaxDegreeOfParallelism = System.Math.Max(1, Environment.ProcessorCount / 2);
#if DEBUG
                options.MaxDegreeOfParallelism = 1;
#endif
                */

                //Parallel.ForEach(this.attributePermutations, options, permutation =>
                foreach (var permutation in this.attributePermutations)                
                {                    
                    ReductStore localReductPool = new ReductStore();
                    localReductPool.DoAddReduct(this.CalculateReduct(permutation, localReductPool));
                    this.ReductStoreCollection.AddStore(localReductPool);
                }
                //);
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
            var newAttributes = eqClasses.Attributes.RemoveAt(attributeIdx, length);
            EquivalenceClassCollection newEqClasses = new EquivalenceClassCollection(this.DataStore, newAttributes, this.DataStore.DataStoreInfo.NumberOfDecisionValues);
            newEqClasses.CountWeightObjects = eqClasses.CountWeightObjects;
            newEqClasses.CountObjects = eqClasses.CountObjects;

            EquivalenceClass[] eqArray = eqClasses.Partitions.Values.ToArray();
            ReductStore localReductStore = new ReductStore();

#if !DEBUG            
            eqArray.Shuffle();
#endif

            foreach (EquivalenceClass eq in eqArray)
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
                        
                        Bireduct exception = new Bireduct(this.DataStore,
                            newAttributes,
                            eq.ObjectIndexes,
                            this.Epsilon,
                            this.WeightGenerator.Weights);

                        EquivalenceClassCollection exceptionEqClasses = new EquivalenceClassCollection(
                            this.DataStore, newAttributes, eqClasses.DecisionValues().Count);

                        EquivalenceClass exeptionEq = new EquivalenceClass(newInstance, this.DataStore);
                        exeptionEq.Instances = new Dictionary<int, decimal>(eq.Instances);
                        exeptionEq.DecisionSet = new PascalSet<long>(eq.DecisionSet);
                        exeptionEq.WeightSum += eq.WeightSum;
                        exceptionEqClasses.Partitions[newInstance] = exeptionEq;
                        exception.SetEquivalenceClassCollection(exceptionEqClasses);
                        exception.IsException = true;
                        localReductStore.AddReduct(exception);
                    }
                }
                else
                {
                    newEqClass = new EquivalenceClass(newInstance, this.DataStore);
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

        private EquivalenceClassCollection ReduceWithoutExceptions(EquivalenceClassCollection eqClasses, int attributeIdx, int length, IReductStore reductStore)
        {
            if (reductStore == null)
                throw new ArgumentNullException("Reduct generating with exceptions requires initialized reductStore", "reductStore");

            var newAttributes = eqClasses.Attributes.RemoveAt(attributeIdx, length);
            EquivalenceClassCollection newEqClasses = new EquivalenceClassCollection(this.DataStore, newAttributes, this.DataStore.DataStoreInfo.NumberOfDecisionValues);
            newEqClasses.CountWeightObjects = eqClasses.CountWeightObjects;
            newEqClasses.CountObjects = eqClasses.CountObjects;

            EquivalenceClass[] eqArray = eqClasses.Partitions.Values.ToArray();
            ReductStore localReductStore = new ReductStore();

#if !DEBUG            
            eqArray.Shuffle();
#endif

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
                        newEqClass.AvgConfidenceWeigth
                            = ((newEqClass.AvgConfidenceWeigth * newEqClass.WeightSum)
                            + (eq.AvgConfidenceWeigth * eq.WeightSum))
                            / (newEqClass.WeightSum + eq.WeightSum);

                        //Update |X * E| count
                        newEqClass.ConfidenceCount += eq.ConfidenceCount;

                        //Update |E|
                        newEqClass.AddObjectInstances(eq.Instances);

                        //Update X
                        newEqClass.DecisionSet = newDecisionSet;

                        //Update |E|w
                        newEqClass.WeightSum += eq.WeightSum;                                                
                    }
                    else
                    {
                        //Increase |E| 
                        //Do not change |X * E|
                        newEqClass.AddObjectInstances(eq.Instances);
                        
                        newEqClasses.CountWeightObjects -= eq.WeightSum;
                        newEqClasses.CountObjects -= eq.NumberOfObjects;                        

                        if (Decimal.Round(newEqClasses.CountWeightObjects, 17) < this.WeightDropLimit)
                            return eqClasses;
                    }
                }
                else
                {
                    newEqClass = new EquivalenceClass(newInstance, this.DataStore);
                    newEqClass.AvgConfidenceWeigth = eq.AvgConfidenceWeigth;
                    newEqClass.ConfidenceCount = eq.ConfidenceCount;
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
