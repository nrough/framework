// 
// NRough Framework
// http://www.nrough.net
// 
// Copyright (c) Sebastian Widz, 2017
// authors at nrough.net
// 
// This file is part of NRough Framework.
// 
// NRough Framework is free software; you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation; either version 3 of the License, or
// (at your option) any later version.
// 
// NRough Framework is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with NRough Framework; if not, write to the Free Software
// Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA  02110-1301  USA
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NRough.Data;
using NRough.Core;
using NRough.MachineLearning.Permutations;
using NRough.Core.CollectionExtensions;
using System.Diagnostics;

namespace NRough.MachineLearning.Roughsets
{
    public class ReductGeneralizedMajorityDecisionApproximateGenerator : ReductGeneralizedMajorityDecisionGenerator
    {
        #region Members

        private IComparer<EquivalenceClass> lengthComparer;
        private IComparer<EquivalenceClass> lengthComparerReverse;

        #endregion Members

        #region Properties

        //public bool UseExceptionRules { get; set; }
        public SortDirection EquivalenceClassSortDirection { get; set; }
        public double Gamma { get; set; }
        protected double WeightDropLimit { get; set; }
        protected double ObjectWeightSum { get; set; }

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
                    MaxDegreeOfParallelism = ConfigManager.MaxDegreeOfParallelism
                };

                Parallel.ForEach(this.attributePermutations, options, permutation =>                
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
            return this.ReduceWithExceptions(eqClasses, attributeIdx, length, reductStore, reductStoreCollection);

            //if (this.UseExceptionRules)            
            //    return this.ReduceWithExceptions(eqClasses, attributeIdx, length, reductStore, reductStoreCollection);
            //return this.ReduceWithoutExceptions(eqClasses, attributeIdx, length, reductStore, reductStoreCollection);
        }

        private EquivalenceClassCollection ReduceWithExceptions(
            EquivalenceClassCollection eqClasses,
            int attributeIdx,
            int length,
            IReductStore reductStore,
            IReductStoreCollection reductStoreCollection = null)
        {
            if (reductStore == null)
                throw new ArgumentNullException("reductStore", "reductStore cannot be null when calculating exceptions");

            var newAttributes = eqClasses.Attributes.RemoveAt(attributeIdx, length);
            EquivalenceClassCollection newEqClasses = new EquivalenceClassCollection(
                this.DecisionTable, newAttributes, eqClasses.Count);
            newEqClasses.WeightSum = eqClasses.WeightSum;
            newEqClasses.NumberOfObjects = eqClasses.NumberOfObjects;

            EquivalenceClass[] eqArray = eqClasses.ToArray();
            this.SortEquivalenceClassArray(eqArray);

            EquivalenceClassCollection exceptionEqClasses = null;
            EquivalenceClass exeptionEq = null;

            foreach (EquivalenceClass eq in eqArray)
            {
                var newInstance = eq.Instance.RemoveAt(attributeIdx, length);

                EquivalenceClass newEqClass = newEqClasses.Find(newInstance);
                if (newEqClass != null)
                {
                    /*
                    if (newEqClass.DecisionSet.Count == 0)
                        throw new InvalidOperationException("");
                    if(newEqClass.WeightSum <= 0)
                        throw new InvalidOperationException("");
                    */


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

                        if (this.UseExceptionRules)
                        {
                            if (exceptionEqClasses == null)
                            {
                                exceptionEqClasses = new EquivalenceClassCollection(this.DecisionTable, eqClasses.Attributes);
                            }

                            exeptionEq = new EquivalenceClass(eq.Instance, eq.Instances, eq.DecisionSet);
                            exeptionEq.AvgConfidenceWeight = eq.AvgConfidenceWeight;
                            exeptionEq.AvgConfidenceSum = eq.AvgConfidenceSum;
                            exeptionEq.WeightSum = eq.WeightSum;

                            exceptionEqClasses.Add(exeptionEq);
                            exceptionEqClasses.NumberOfObjects += eq.NumberOfObjects;
                            exceptionEqClasses.WeightSum += eq.WeightSum;
                        }
                        else
                        {
                            //Update |E|
                            //newEqClass.AddObjectInstances(eq.Instances);
                            
                            //Update |E|w
                            //newEqClass.WeightSum += eq.WeightSum;
                        }
                    }
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

            if (this.UseExceptionRules && exceptionEqClasses != null)
            {
                //exceptionEqClasses.RecalcEquivalenceClassStatistic(this.DataStore);
                ReductWeights exception = new ReductWeights(
                    this.DecisionTable,
                    eqClasses.Attributes,
                    this.Epsilon,
                    this.WeightGenerator.Weights,
                    exceptionEqClasses);
                exception.IsException = true;

                reductStore.AddReduct(exception);

                //Console.WriteLine("Exceptions:");
                //TraceEquivalenceClasses(exceptionEqClasses);
                //Console.WriteLine();
            }

            //TraceEquivalenceClasses(newEqClasses);

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

        /*
        private EquivalenceClassCollection ReduceWithoutExceptions(
            EquivalenceClassCollection eqClasses,
            int attributeIdx,
            int length,
            IReductStore reductStore,
            IReductStoreCollection reductStoreCollection = null)
        {
            var newAttributes = eqClasses.Attributes.RemoveAt(attributeIdx, length);
            EquivalenceClassCollection newEqClasses = new EquivalenceClassCollection(this.DecisionTable, newAttributes, eqClasses.Count);
            newEqClasses.WeightSum = eqClasses.WeightSum;
            newEqClasses.NumberOfObjects = eqClasses.NumberOfObjects;

            EquivalenceClass[] eqArray = eqClasses.ToArray();
            this.SortEquivalenceClassArray(eqArray);

            foreach (EquivalenceClass eq in eqArray)
            {
                var newInstance = eq.Instance.RemoveAt(attributeIdx, length);

                EquivalenceClass newEqClass = newEqClasses.Find(newInstance);
                if (newEqClass != null)
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

                    newEqClasses.Add(newEqClass);
                }
            }

            return newEqClasses;
        }
        */

        public override void InitDefaultParameters()
        {
            base.InitDefaultParameters();
            this.UseExceptionRules = true;
            this.EquivalenceClassSortDirection = SortDirection.Random;
        }

        public override void InitFromArgs(Args args)
        {
            base.InitFromArgs(args);

            if (args.Exist(ReductFactoryOptions.UseExceptionRules))
                this.UseExceptionRules = (bool)args.GetParameter(ReductFactoryOptions.UseExceptionRules);

            if (args.Exist(ReductFactoryOptions.EquivalenceClassSortDirection))
                this.EquivalenceClassSortDirection = (SortDirection)args.GetParameter(ReductFactoryOptions.EquivalenceClassSortDirection);

            if (this.UseExceptionRules)
            {
                this.InitReductStoreCollection(this.Permutations.Count);
            }

            this.Gamma = this.Epsilon;
            this.Epsilon = 0;

            this.ObjectWeightSum = this.WeightGenerator.Weights.Sum();
            this.WeightDropLimit = (1.0 - this.Gamma) * this.ObjectWeightSum;
        }

        #endregion Methods
    }

    public class ReductGeneralizedMajorityDecisionApproximateFactory : IReductFactory
    {
        public virtual string FactoryKey
        {
            get { return ReductTypes.GeneralizedMajorityDecisionApproximate; }
        }

        public virtual IReductGenerator GetReductGenerator(Args args)
        {
            ReductGeneralizedMajorityDecisionApproximateGenerator rGen = new ReductGeneralizedMajorityDecisionApproximateGenerator();
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