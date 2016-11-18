using Infovision.Data;
using Infovision.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infovision.Datamining.Roughset.DecisionTrees
{
    public class ObliviousDecisionTree : DecisionTreeBase
    {        
        private Dictionary<int[], SplitInfo> cache = new Dictionary<int[], SplitInfo>(ArrayComparer<int>.Instance);
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

            return 1.0;
        }

        protected override double CalculateImpurityAfterSplit(EquivalenceClassCollection eq)
        {
            throw new NotImplementedException();
        }

        protected override SplitInfo GetNextSplit(
            EquivalenceClassCollection eqClassCollection, 
            int[] origAttributes, 
            int[] attributesToTest, 
            IDecisionTreeNode parentTreeNode)
        {
            if (this.RankedAttributes == false)
            {
                Console.WriteLine("Checking {0}", attributesToTest.ToStr(' '));

                SplitInfo baseResult = null;
                if (!cache.TryGetValue(attributesToTest, out baseResult))
                {
                    lock (syncRoot)
                    {
                        if (!cache.TryGetValue(attributesToTest, out baseResult))
                        {
                            Console.WriteLine("Not found in cache");

                            baseResult = base.GetNextSplit(eqClassCollection, origAttributes, attributesToTest, parentTreeNode);
                            //cache only splits that are valid (invalid split can be valid in other branch)
                            if (baseResult.AttributeId != -1 && baseResult.SplitType != SplitType.None)
                                cache.Add(attributesToTest, baseResult);

                            return baseResult;
                        }
                    }
                }

                Console.WriteLine("Found in cache");

                /*
                return this.GetSplitInfo(baseResult.AttributeId, 
                    eqClassCollection, 
                    this.CalculateImpurityBeforeSplit(eqClassCollection), 
                    parentTreeNode);
                */

                return baseResult;
            }

            int attributeId = origAttributes[parentTreeNode.Level];
            return this.GetSplitInfo(attributeId, eqClassCollection, 
                this.CalculateImpurityBeforeSplit(eqClassCollection), parentTreeNode);
        }

        protected override SplitInfo GetSplitInfoSymbolic(int attributeId, EquivalenceClassCollection data, double parentMeasure, IDecisionTreeNode parentNode)
        {
            if (parentMeasure < 0) throw new ArgumentException("currentScore < 0", "parentMeasure");
            if (data == null) throw new ArgumentNullException("data", "(data == null");            

            int[] newAttributes = new int[parentNode.Level + 1];
            IDecisionTreeNode current = parentNode;
            int j = 0;
            while (current != null && current.Attribute != -1) //not root
            {
                newAttributes[j++] = current.Attribute;
                current = current.Parent;         
            }
            newAttributes[j] = attributeId;

            Console.WriteLine("Calculating split based on {0}", newAttributes.ToStr());
                                    
            return new SplitInfo(
                attributeId,
                this.ImpurityFunction(EquivalenceClassCollection.Create(newAttributes, data.Data, data.Data.Weights)),
                EquivalenceClassCollection.Create(attributeId, data), SplitType.Discreet, ComparisonType.EqualTo, 0);                        
        }
    }
}
