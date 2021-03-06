﻿// 
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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NRough.Data;
using NRough.Core;
using NRough.MachineLearning.Permutations;
using NRough.Core.CollectionExtensions;
using NRough.Core.Comparers;
using System.Collections.Generic;
using System.Diagnostics;

namespace NRough.MachineLearning.Roughsets
{
    [Serializable]
    public abstract class ReductGeneratorMeasure : ReductGenerator
    {
        #region Members

        private IInformationMeasure informationMeasure;
        private double dataSetQuality;

        protected EquivalenceClassCollection initialEqClasses;

        #endregion Members

        #region Properties

        protected double DataSetQuality
        {
            get
            {
                if (!this.IsDataSetQualityCalculated())
                    this.CalcDataSetQuality();

                return this.dataSetQuality;
            }
        }

        protected IInformationMeasure InformationMeasure
        {
            get { return this.informationMeasure; }
            set { this.informationMeasure = value; }
        }

        public bool UsePerformanceImprovements { get; set; }
        public bool Greedy { get; set; }

        #endregion Properties

        #region Constructors

        protected ReductGeneratorMeasure()
            : base()
        {
            this.dataSetQuality = Double.MinValue;
            this.UsePerformanceImprovements = true;
        }

        #endregion Constructors

        

        #region Methods

        public override void InitFromArgs(Args args)
        {
            base.InitFromArgs(args);

            if (args.Exist(ReductFactoryOptions.DataSetQuality))
                this.dataSetQuality = args.GetParameter<double>(ReductFactoryOptions.DataSetQuality);

            if (args.Exist(ReductFactoryOptions.InitialEquivalenceClassCollection))
            {
                this.initialEqClasses = args.GetParameter<EquivalenceClassCollection>(
                    ReductFactoryOptions.InitialEquivalenceClassCollection);
                this.dataSetQuality = this.InformationMeasure.Calc(this.initialEqClasses);
            }

            this.CalcDataSetQuality();
        }

        protected bool IsDataSetQualityCalculated()
        {
            return this.dataSetQuality > 0;
        }

        protected virtual void CalcDataSetQuality()
        {
            if (!this.IsDataSetQualityCalculated())
            {
                IReduct tmpReduct = this.CreateReductObject(this.DecisionTable.GetStandardFields(), 0, "tmpReduct");
                this.dataSetQuality = this.informationMeasure.Calc(tmpReduct);
            }
        }

        protected virtual void CreateReductStoreFromPermutationCollection(PermutationCollection permutationList)
        {
            if (permutationList == null) throw new ArgumentNullException("permutationList");

            ParallelOptions options = new ParallelOptions()
            {
                MaxDegreeOfParallelism = ConfigManager.MaxDegreeOfParallelism
            };

            IReductStore reductStore = this.CreateReductStore(permutationList.Count);
            reductStore.AllowDuplicates = true;

            Parallel.ForEach(permutationList, options, permutation =>
            {
                IReduct reduct = this.CalculateReduct(permutation.ToArray(), reductStore, this.UseCache, this.Epsilon);
                reductStore.AddReduct(reduct);
            }
            );

            this.ReductPool = reductStore;
        }

        protected override void Generate()
        {
            this.CreateReductStoreFromPermutationCollection(this.Permutations);
        }

        protected override IReduct CreateReductObject(int[] fieldIds, double epsilon, string id)
        {
            Reduct r = new Reduct(this.DecisionTable, fieldIds, epsilon);
            r.Id = id;
            return r;
        }

        protected override IReduct CreateReductObject(int[] fieldIds, double epsilon, string id, EquivalenceClassCollection equivalenceClasses)
        {
            Reduct r = new Reduct(this.DecisionTable, fieldIds, epsilon, this.DecisionTable.Weights, equivalenceClasses);
            r.Id = id;
            return r;
        }

        public override IReduct CreateReduct(int[] permutation, double epsilon, double[] weights, IReductStore reductStore = null, IReductStoreCollection reductStoreCollection = null)
        {
            IReductStore localReductStore = this.CreateReductStore();
            return this.CalculateReduct(permutation, localReductStore, false, epsilon);
        }

        protected virtual IReduct CalculateReduct(int[] permutation, IReductStore reductStore, bool useCache, double epsilon)
        {
            IReduct reduct = null;

            if (Greedy)
            {
                reduct = this.CreateReductObject(new int[] { }, epsilon, this.GetNextReductId().ToString());
                GreedyReach(reduct, permutation, reductStore, useCache);
                return reduct;
            }

            if (this.UsePerformanceImprovements)
            {                
                reduct = this.CreateReductObject(new int[] { }, epsilon, this.GetNextReductId().ToString());
                this.Reach(reduct, permutation, reductStore, useCache);
                this.Reduce(reduct, permutation, reductStore, useCache);
            }
            else
            {
                if (this.initialEqClasses != null)
                {
                    reduct = this.CreateReductObject(this.initialEqClasses.Attributes, epsilon, this.GetNextReductId().ToString(), this.initialEqClasses);
                    this.Reduce(reduct, permutation, reductStore, useCache);
                }
                else
                {
                    reduct = this.CreateReductObject(permutation, epsilon, this.GetNextReductId().ToString());
                    //this.Reduce(reduct, permutation, reductStore, useCache);
                    this.ReduceForward(reduct, permutation, reductStore, useCache);
                }
            }

            return reduct;
        }

        public virtual IReduct CalculateReduct(int[] attributes)
        {
            //IReduct reduct = this.CreateReductObject(new int[] { }, this.Epsilon, this.GetNextReductId().ToString());
            //this.Reach(reduct, attributes, null, false);

            IReduct reduct = this.CreateReductObject(attributes, this.Epsilon, this.GetNextReductId().ToString());
            this.ReduceForward(reduct, attributes, null, false);

            return reduct;
        }

        protected virtual void GreedyReach(IReduct reduct, int[] attributes, IReductStore reductStore, bool useCache)
        {
            if (reduct == null) throw new ArgumentNullException("reduct");
            if (attributes == null) throw new ArgumentNullException("reduct");

            if (attributes.Length == 0)
                return;

            var attrTmp = new HashSet<int>(attributes);
            do
            {
                int bestAttribute = -1;
                double bestValue = -1;
                foreach (int attr in attrTmp)
                {
                    var value = InformationMeasureMajority.Instance.Calc(
                        EquivalenceClassCollection.Create(
                            reduct.Attributes.Union(new int[] { attr }).ToArray(),
                            reduct.DataStore));

                    if (value > bestValue)
                    {
                        bestValue = value;
                        bestAttribute = attr;
                    }
                }                

                reduct.AddAttribute(bestAttribute);
                attrTmp.Remove(bestAttribute);
            }
            while (attrTmp.Count != 0 
                && !IsReduct(reduct, reductStore, useCache));
        }

        protected virtual void Reach(IReduct reduct, int[] permutation, IReductStore reductStore, bool useCache)
        {
            for (int i = 0; i < permutation.Length; i++)
            {
                reduct.AddAttribute(permutation[i]);
                if (this.IsReduct(reduct, reductStore, useCache))
                    return;
            }
        }

        protected virtual void Reduce(IReduct reduct, int[] permutation, IReductStore reductStore, bool useCache)
        {
            int len = permutation.Length - 1;
            for (int i = len; i >= 0; i--)
            {
                int attributeId = permutation[i];
                if (reduct.TryRemoveAttribute(attributeId))
                {
                    if (!this.IsReduct(reduct, reductStore, useCache))
                        reduct.AddAttribute(attributeId);
                }
            }
        }

        protected virtual void ReduceForward(IReduct reduct, int[] permutation, IReductStore reductStore, bool useCache)
        {            
            for (int i = 0; i < permutation.Length; i++)
            {                
                if (reduct.TryRemoveAttribute(permutation[i]))
                {
                    if (!this.IsReduct(reduct, reductStore, useCache))
                        reduct.AddAttribute(permutation[i]);
                }
            }
        }

        public virtual bool CheckIsReduct(IReduct reduct)
        {
            return ToleranceDoubleComparer.Instance.Compare(
                this.GetPartitionQuality(reduct),
                (1.0 - this.Epsilon) * this.DataSetQuality) != -1;
        }

        protected virtual bool IsReduct(IReduct reduct, IReductStore reductStore, bool useCache)
        {
            string key = String.Empty;
            ReductCacheInfo reductInfo = null;

            if (useCache)
            {
                key = this.GetReductCacheKey(reduct);
                reductInfo = ReductCache.Instance.Get(key) as ReductCacheInfo;
                if (reductInfo != null)
                {
                    if (reductInfo.CheckIsReduct(this.Epsilon) == NoYesUnknown.Yes)
                        return true;
                    if (reductInfo.CheckIsReduct(this.Epsilon) == NoYesUnknown.No)
                        return false;
                }
            }

            bool isReduct = false;
            if (this.UsePerformanceImprovements && reductStore != null && reductStore.IsSuperSet(reduct))
                isReduct = true;
            else
                isReduct = this.CheckIsReduct(reduct);

            if (useCache)
                this.UpdateReductCacheInfo(reductInfo, key, isReduct);

            return isReduct;
        }

        protected virtual double GetPartitionQuality(IReduct reduct)
        {
            return this.InformationMeasure.Calc(reduct);
        }

        private void UpdateReductCacheInfo(ReductCacheInfo reductInfo, string key, bool isReduct)
        {
            if (reductInfo != null)
            {
                reductInfo.SetApproximationRanges(isReduct, this.Epsilon);
            }
            else
            {
                ReductCache.Instance.Set(key, new ReductCacheInfo(isReduct, this.Epsilon));
            }
        }

        protected virtual string GetReductCacheKey(IReduct reduct)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append("m=").Append(this.GetType().Name);
            stringBuilder.Append("|d=").Append(this.DecisionTable.Name);
            stringBuilder.Append("|a=").Append(reduct.Attributes.ToArray().ToStr(" "));
            return stringBuilder.ToString();
        }

        #endregion Methods
    }

    public abstract class ApproximateReductFactory : IReductFactory
    {
        public abstract string FactoryKey { get; }

        public virtual IPermutationGenerator GetPermutationGenerator(Args args)
        {
            DataStore dataStore = (DataStore)args.GetParameter(ReductFactoryOptions.DecisionTable);
            return new PermutationGeneratorReverse(dataStore);
        }

        public abstract IReductGenerator GetReductGenerator(Args args);
    }

    [Serializable]
    public class ReductGeneratorRelative : ReductGeneratorMeasure
    {
        #region Constructors

        public ReductGeneratorRelative()
            : base()
        {            
            this.InformationMeasure = InformationMeasureRelative.Instance;
        }

        #endregion Constructors
    }

    public class ApproximateReductRelativeFactory : ApproximateReductFactory
    {
        public override string FactoryKey
        {
            get { return ReductTypes.ApproximateReductRelative; }
        }

        public override IReductGenerator GetReductGenerator(Args args)
        {
            ReductGeneratorRelative rGen = new ReductGeneratorRelative();
            rGen.InitFromArgs(args);
            return rGen;
        }
    }

    [Serializable]
    public class ReductGeneratorMajority : ReductGeneratorMeasure
    {
        #region Constructors

        public ReductGeneratorMajority()
            : base()
        {            
            this.InformationMeasure = InformationMeasureMajority.Instance;
        }

        #endregion Constructors
    }

    public class ApproximateReductMajorityFactory : ApproximateReductFactory
    {
        public override string FactoryKey
        {
            get { return ReductTypes.ApproximateReductMajority; }
        }

        public override IReductGenerator GetReductGenerator(Args args)
        {
            ReductGeneratorMajority rGen = new ReductGeneratorMajority();
            rGen.InitFromArgs(args);
            return rGen;
        }
    }

    [Serializable]
    public class ReductGeneratorPositive : ReductGeneratorMeasure
    {
        #region Constructors

        public ReductGeneratorPositive()
            : base()
        {
            this.InformationMeasure = InformationMeasurePositive.Instance;
        }

        #endregion Constructors
    }

    public class ApproximateReductPositiveFactory : ApproximateReductFactory
    {
        public override string FactoryKey
        {
            get { return ReductTypes.ApproximateReductPositive; }
        }

        public override IReductGenerator GetReductGenerator(Args args)
        {
            ReductGeneratorPositive rGen = new ReductGeneratorPositive();
            rGen.InitFromArgs(args);
            return rGen;
        }
    }
}