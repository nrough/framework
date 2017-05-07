using NRough.Data;
using NRough.Math;
using NRough.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NRough.Core.CollectionExtensions;

namespace NRough.MachineLearning.Classification.DecisionTrees.Pruning
{
    public class ReducedErrorPruning : DecisionTreePruningBase
    {
        private readonly object syncRoot = new object();
        private long[] predictionResult;
        private Dictionary<IDecisionTreeNode, NodeInfo> info;

        private class NodeInfo
        {
            public List<int> subset;
            public double errorMajorityClass;    
            public double errorSubTree;
            public double gain;

            public NodeInfo() { subset = new List<int>(); }

            public override string ToString()
            {
                return String.Format("{0} {1} {2}", gain, errorMajorityClass, errorSubTree);
            }
        }

        public ReducedErrorPruning(IDecisionTree decisionTree, DataStore data)
            : base(decisionTree, data)
        {
            this.info = new Dictionary<IDecisionTreeNode, NodeInfo>();

            foreach (var node in decisionTree)
                this.info[node] = new NodeInfo();

            this.predictionResult = new long[data.NumberOfRecords];
            this.predictionResult.SetAll(Classifier.UnclassifiedOutput);

            for (int i = 0; i < data.NumberOfRecords; i++)
                this.ComputePrediction(decisionTree.Root, i);
        }

        private void ComputePrediction(IDecisionTreeNode node, int objectIdx)
        {
            //DataRecordInternal record = node.IsLeaf ? null : this.PruningData.GetRecordByIndex(objectIdx, false);
            IDecisionTreeNode current = node;
            while (current != null)
            {
                this.info[current].subset.Add(objectIdx);
                if (current.IsLeaf)
                {
                    predictionResult[objectIdx] = current.Output;
                    return;
                }
                current = current.Children
                    //.Where(x => x.Compute(record[x.Attribute]))
                    .Where(x => x.Compute(PruningData.GetFieldValue(objectIdx, x.Attribute)))
                    .FirstOrDefault();
            }
        }
        
        private double ComputeSubtreeError(IEnumerable<int> indices)
        {                        
            double error = 0;
            foreach(int idx in indices)
                if (this.PruningData.GetDecisionValue(idx) != predictionResult[idx])                
                    error += this.PruningData.GetWeight(idx);
            return error;
        }

        private double ComputeSubtreeBaselineError(IDecisionTreeNode node, IEnumerable<int> indices)
        {                        
            double error = 0;
            foreach(int idx in indices)            
                if (this.PruningData.GetDecisionValue(idx) != node.Output)
                    error += this.PruningData.GetWeight(idx);
            return error;
        }
        
        private double ComputeError()
        {
            ClassificationResult result = Classifier.Default.Classify(this.DecisionTree, this.PruningData);
            return result.Error;
        }

        public override double Prune()
        {
            double currentError = this.ComputeError();
            double lastError = currentError;

            while (true)
            {
                foreach (var node in this.DecisionTree)
                {
                    var nodeInfo = this.info[node];
                    if (node.IsLeaf)
                    {
                        nodeInfo.gain = Double.NegativeInfinity;
                        continue;
                    }

                    nodeInfo.errorMajorityClass = this.ComputeSubtreeBaselineError(node, nodeInfo.subset);
                    nodeInfo.errorSubTree = this.ComputeSubtreeError(nodeInfo.subset);
                    nodeInfo.gain = nodeInfo.errorSubTree - nodeInfo.errorMajorityClass;
                    
                }

                double maxGain = Double.NegativeInfinity;
                IDecisionTreeNode maxNode = null;
                foreach (var node in this.DecisionTree)
                {
                    var nodeInfo = this.info[node];
                    if (nodeInfo.gain >= 0 && nodeInfo.gain >= maxGain)
                    {
                        maxGain = nodeInfo.gain;
                        maxNode = node;
                    }
                }

                if (maxGain >= 0 && maxNode != null)
                {
                    maxNode.Children = null;
                    foreach (int idx in this.info[maxNode].subset)
                        predictionResult[idx] = maxNode.Output;
                }
                else
                {
                    break;
                }

                currentError = this.ComputeError();
                if (currentError > lastError)
                    break;

                lastError = currentError;
            }            

            return currentError;
        }
    }
}
