﻿using NRough.Data;
using NRough.Core;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NRough.MachineLearning.Classification.DecisionTrees
{
    public class ObliviousDecisionTree : DecisionTreeBase
    {
        private Dictionary<int[], SplitInfo> cache = null;
        private object syncRoot = new object();

        public bool RankedAttributes { get; set; } = false;

        public ObliviousDecisionTree()
            : base()
        {                     
            this.ImpurityFunction = ImpurityMeasure.Majority;
            this.ImpurityNormalize = ImpurityMeasure.DummyNormalize;
        }

        public ObliviousDecisionTree(string modelName)
            : base(modelName)
        {            
            this.ImpurityFunction = ImpurityMeasure.Majority;
            this.ImpurityNormalize = ImpurityMeasure.DummyNormalize;
        }

        protected override void CleanUp()
        {
            base.CleanUp();
            this.cache = null;
        }

        public override ClassificationResult Learn(DataStore data, int[] attributes)
        {
            this.cache = new Dictionary<int[], SplitInfo>(attributes.Length + 1, ArrayComparer<int>.Instance);
            return base.Learn(data, attributes);
        }

        protected override SplitInfo GetNextSplit(
            EquivalenceClassCollection eqClassCollection, 
            int[] origAttributes, 
            int[] attributesToTest, 
            IDecisionTreeNode parentTreeNode)
        {
            if (this.RankedAttributes == false)
            {
                SplitInfo baseResult = null;
                if (!cache.TryGetValue(attributesToTest, out baseResult))
                {
                    lock (syncRoot)
                    {
                        if (!cache.TryGetValue(attributesToTest, out baseResult))
                        {
                            baseResult = base.GetNextSplit(eqClassCollection, origAttributes, attributesToTest, parentTreeNode);
                            
                            //cache only splits that are valid
                            if (baseResult.SplitType != SplitType.None)
                                cache.Add(attributesToTest, baseResult);

                            return baseResult;
                        }
                    }
                }

                return this.GetSplitInfo(baseResult.AttributeId, eqClassCollection, 
                    this.CalculateImpurityBeforeSplit(eqClassCollection), parentTreeNode);
            }

            int attributeId = origAttributes[parentTreeNode.Level];
            return this.GetSplitInfo(attributeId, eqClassCollection, 
                this.CalculateImpurityBeforeSplit(eqClassCollection), parentTreeNode);
        }

        protected override SplitInfo GetSplitInfoSymbolic(int attributeId, EquivalenceClassCollection data, double parentMeasure, IDecisionTreeNode parentNode)
        {
            if (parentMeasure < 0) throw new ArgumentException("currentScore < 0", "parentMeasure");
            if (data == null) throw new ArgumentNullException("data", "(data == null");

            EquivalenceClassCollection eqClasses = EquivalenceClassCollection.Create(attributeId, data);
            if (eqClasses.Count <= 1)
                return SplitInfo.NoSplit;

            int[] newAttributes = new int[parentNode.Level + 1];
            IDecisionTreeNode current = parentNode;
            int j = 0;
            while (current != null && current.Attribute != -1)
            {
                newAttributes[j++] = current.Attribute;
                current = current.Parent;         
            }
            newAttributes[j] = attributeId;

            return new SplitInfo(
                attributeId,
                this.ImpurityFunction(EquivalenceClassCollection.Create(newAttributes, data.Data, data.Data.Weights)),
                eqClasses, SplitType.Discreet, ComparisonType.EqualTo, 0);                        
        }

        protected override double CalculateImpurityBeforeSplit(EquivalenceClassCollection eqClassCollection)
        {
            if (eqClassCollection.Count != 1)
                throw new ArgumentException("eqClassCollection.Count != 1", "eqClassCollection");

            return 1.0;
        }

        protected override double CalculateImpurityAfterSplit(EquivalenceClassCollection eq)
        {
            throw new NotImplementedException();
        }
    }
}