using Infovision.Data;
using Infovision.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infovision.Datamining.Roughset.DecisionTrees
{
    public class ObliviousDecisionTree : DecisionTreeBase
    {
        protected override void BuildTree(EquivalenceClassCollection eqClassCollection, DecisionTreeNode parent, int[] attributes)
        {
            var splitInfo = Tuple.Create<EquivalenceClassCollection, DecisionTreeNode, int[]>(eqClassCollection, parent, attributes);
            var queue = new Queue<Tuple<EquivalenceClassCollection, DecisionTreeNode, int[]>>();
            queue.Enqueue(splitInfo);
            bool isConverged = false;

            while (queue.Count != 0)
            {
                var currentInfo = queue.Dequeue();
                EquivalenceClassCollection currentEqClassCollection = currentInfo.Item1;
                DecisionTreeNode currentParent = currentInfo.Item2;
                int[] currentAttributes = currentInfo.Item3;

                var decision = currentEqClassCollection.DecisionWeights.FindMaxValuePair();
                this.SetNodeOutput(currentParent, decision.Key, decision.Value);

                if (this.CheckStopCondition(currentEqClassCollection, currentAttributes, ref isConverged, currentParent))
                    continue;

                var nextSplit = this.GetNextSplit(currentEqClassCollection, currentAttributes);

                if (nextSplit.SplitType == SplitType.None)
                    continue;

                var subEqClasses = EquivalenceClassCollection.Split(nextSplit.AttributeId, nextSplit.EquivalenceClassCollection);

                if (nextSplit.SplitType == SplitType.Binary && subEqClasses.Count != 2)
                    throw new InvalidOperationException("Binary split must have two branches.");

                //RemoveValue makes a copy of the array
                currentAttributes = currentAttributes.RemoveValue(nextSplit.AttributeId);
                bool binarySplitFlag = true;
                foreach (var kvp in subEqClasses)
                {
                    DecisionTreeNode newNode = null;
                    switch (nextSplit.SplitType)
                    {
                        case SplitType.Discreet:
                            newNode = new DecisionTreeNode(this.GetId(), nextSplit.AttributeId, nextSplit.ComparisonType, kvp.Key, currentParent);
                            break;

                        case SplitType.Binary:
                            newNode = new DecisionTreeNode(this.GetId(), nextSplit.AttributeId,
                                binarySplitFlag ? nextSplit.ComparisonType : nextSplit.ComparisonType.Complement(),
                                nextSplit.Cut, currentParent);
                            binarySplitFlag = false;
                            break;

                        default:
                            throw new NotImplementedException(String.Format("Split type {0} is not implemented", nextSplit.SplitType));
                    }

                    currentParent.AddChild(newNode);

                    if (this.Epsilon >= 0.0)
                        newNode.Measure = InformationMeasureWeights.Instance.Calc(kvp.Value);

                    var newSplitInfo = Tuple.Create<EquivalenceClassCollection, DecisionTreeNode, int[]>(
                            kvp.Value,
                            newNode,
                            currentAttributes);

                    queue.Enqueue(newSplitInfo);
                }
            }
        }
    }
}
