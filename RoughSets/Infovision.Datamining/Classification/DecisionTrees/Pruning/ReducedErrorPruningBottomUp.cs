using Raccoon.Data;
using Raccoon.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Raccoon.MachineLearning.Classification.DecisionTrees.Pruning
{
    class ReducedErrorPruningBottomUp : DecisionTreePruningBase
    {
        private readonly object syncRoot = new object();
        private long[] predictionResult;
        private Dictionary<IDecisionTreeNode, NodeInfo> info;

        private class NodeInfo
        {
            public double errorSubTree;
            public double errorMajorityClass;
            public List<int> subset = new List<int>();
            public double gain { get { return this.errorSubTree - this.errorMajorityClass; } }
        }

        public ReducedErrorPruningBottomUp(IDecisionTree decisionTree, DataStore data)
            : base(decisionTree, data)
        {
            this.info = new Dictionary<IDecisionTreeNode, NodeInfo>();

            foreach (var node in decisionTree)
                this.info[node] = new NodeInfo();

            this.predictionResult = new long[data.NumberOfRecords];
            this.predictionResult.SetAll(-1);

            for (int i = 0; i < data.NumberOfRecords; i++)
                this.ComputePrediction(decisionTree.Root, i);
        }

        private void ComputePrediction(IDecisionTreeNode node, int objectIdx)
        {
            DataRecordInternal record = node.IsLeaf ? null : this.PruningData.GetRecordByIndex(objectIdx, false);
            IDecisionTreeNode current = node;
            while (current != null)
            {
                this.info[current].subset.Add(objectIdx);
                if (current.IsLeaf)
                {
                    predictionResult[objectIdx] = current.Output;
                    return;
                }
                current = current.Children.Where(x => x.Compute(record[x.Attribute])).FirstOrDefault();
            }
        }

        private double ComputeSubtreeError(IEnumerable<int> indices)
        {
            double error = 0;
            foreach (int idx in indices)
                if (this.PruningData.GetDecisionValue(idx) != predictionResult[idx])
                    error += this.PruningData.GetWeight(idx);
            return error;
        }

        private double ComputeSubtreeBaselineError(IDecisionTreeNode node, IEnumerable<int> indices)
        {
            double error = 0;
            foreach (int idx in indices)
                if (this.PruningData.GetDecisionValue(idx) != node.Output)
                    error += this.PruningData.GetWeight(idx);
            return error;
        }

        private double ComputeError()
        {
            ClassificationResult result = Classifier.DefaultClassifer.Classify(this.DecisionTree, this.PruningData);
            return result.Error;
        }

        public override double Prune()
        {
            bool reduction = false;
            double lastError = this.ComputeError();
            while (true)
            {
                reduction = false;
                var i_nodes = TreeNodeTraversal.GetEnumeratorBottomUp(this.DecisionTree.Root);
                while(i_nodes.MoveNext())
                {
                    IDecisionTreeNode node = i_nodes.Current;
                    if (node.IsLeaf)
                        continue;

                    var nodeInfo = this.info[node];
                    nodeInfo.errorMajorityClass = this.ComputeSubtreeBaselineError(node, nodeInfo.subset);
                    nodeInfo.errorSubTree = this.ComputeSubtreeError(nodeInfo.subset);
                    if (nodeInfo.gain >= 0)
                    {
                        var tmpChildren = node.Children;
                        node.Children = null;
                        double currentError = this.ComputeError();
                        if (currentError > lastError)
                        {
                            node.Children = tmpChildren;
                        }
                        else
                        {
                            reduction = true;
                            foreach (int idx in nodeInfo.subset)
                                predictionResult[idx] = node.Output;

                            lastError = currentError;
                            break;
                        }
                    }
                }

                if(!reduction)
                    break;
            }

            return lastError;
        }
    }
}
