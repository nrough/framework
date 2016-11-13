using Infovision.Data;
using Infovision.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infovision.Datamining.Roughset.DecisionTrees
{
    public class ObliviousDecisionTree : DecisionTreeBase
    {
        private Dictionary<int[], double> cache = new Dictionary<int[], double>(new ArrayComparer<int>());
        private object syncRoot = new object();

        public bool RankedAttributes { get; set; }
        public bool UseLocalOutput { get; set; }

        public ObliviousDecisionTree()
            : base()
        {         
            this.RankedAttributes = false;
            this.UseLocalOutput = false;
            this.ImpurityFunction = ImpurityFunctions.Majority;
            this.ImpurityNormalize = ImpurityFunctions.DummyNormalize;
        }

        public ObliviousDecisionTree(string modelName)
            : base(modelName)
        {            
            this.RankedAttributes = false;
            this.UseLocalOutput = false;
            this.ImpurityFunction = ImpurityFunctions.Majority;
            this.ImpurityNormalize = ImpurityFunctions.DummyNormalize;
        }

        protected override DecisionTreeBase CreateInstanceForClone()
        {
            return new ObliviousDecisionTree();
        }

        protected override void InitParametersFromOtherTree(DecisionTreeBase _decisionTree)
        {
            base.InitParametersFromOtherTree(_decisionTree);

            var tree = _decisionTree as ObliviousDecisionTree;
            if (tree != null)
            {
                this.RankedAttributes = tree.RankedAttributes;
                this.UseLocalOutput = tree.UseLocalOutput;
            }
        }

        public override long Compute(DataRecordInternal record)
        {
            if (this.Root == null)
                throw new InvalidOperationException("this.Root == null");

            IDecisionTreeNode current = this.Root;
            while (current != null)
            {
                if (current.IsLeaf)
                    return current.Output;

                IDecisionTreeNode next = current.Children.Where(x => x.Compute(record[x.Attribute])).FirstOrDefault();

                if (next == null && this.UseLocalOutput)
                    return current.Output;

                current = next;
            }

            return -1; //unclassified
        }

        protected override double CalculateImpurityBeforeSplit(EquivalenceClassCollection eqClassCollection)
        {
            if (eqClassCollection.Count != 1)
                throw new ArgumentException("eqClassCollection.Count != 1", "eqClassCollection");

            return 0;
        }

        protected override double CalculateImpurityAfterSplit(EquivalenceClassCollection eq)
        {
            throw new NotImplementedException();
        }

        protected override SplitInfo GetNextSplit(EquivalenceClassCollection eqClassCollection, int[] origAttributes, int[] attributesToTest, IDecisionTreeNode parentTreeNode)
        {
            if(this.RankedAttributes == false)
                return base.GetNextSplit(eqClassCollection, origAttributes, attributesToTest, parentTreeNode);

            int attributeId = origAttributes[parentTreeNode.Level];
            return this.GetSplitInfo(attributeId, eqClassCollection, this.CalculateImpurityBeforeSplit(eqClassCollection));
        }

        protected override SplitInfo GetSplitInfoSymbolic(int attributeId, EquivalenceClassCollection data, double parentMeasure)
        {
            if (parentMeasure < 0)
                throw new ArgumentException("currentScore < 0", "parentMeasure");

            if (data == null)
                throw new ArgumentNullException("data", "(data == null");

            int[] newAttributes = new int[data.Attributes.Length + 1];
            if(data.Attributes.Length != 0)
                Array.Copy(data.Attributes, newAttributes, data.Attributes.Length);
            newAttributes[data.Attributes.Length] = attributeId;
            
            double m;
            EquivalenceClassCollection attributeEqClasses = null;
            if (!cache.TryGetValue(newAttributes, out m))
            {
                lock (syncRoot)
                {
                    if (!cache.TryGetValue(newAttributes, out m))
                    {
                        attributeEqClasses = EquivalenceClassCollection.Create(newAttributes, data.Data, data.Data.Weights);
                        m = this.ImpurityFunction(attributeEqClasses);
                        cache.Add(newAttributes, m);
                    }
                }
            }

            attributeEqClasses = EquivalenceClassCollection.Create(newAttributes, data.Data, data.Data.Weights, data.Indices);
            return new SplitInfo(attributeId, m, attributeEqClasses, SplitType.Discreet, ComparisonType.EqualTo, 0);
        }
    }
}
