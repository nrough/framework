using Infovision.Data;
using Infovision.Statistics;
using Infovision.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infovision.Datamining.Roughset.DecisionTrees.Pruning
{
    public class ReducedErrorPruning : DecisionTreePruningBase
    {
        //private object syncRoot = new object();
        private long[] predictionResult;
        private Dictionary<IDecisionTreeNode, NodeInfo> info;

        private class NodeInfo
        {
            public List<int> subset;
            public double error;
            public double gain;

            public NodeInfo()
            {
                subset = new List<int>();
            }
        }

        public ReducedErrorPruning(IDecisionTree decisionTree, DataStore data)
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
            DataRecordInternal record = this.PruningData.GetRecordByIndex(objectIdx, false);

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
                    .Where(x => x.Compute(record[x.Attribute]))
                    .FirstOrDefault();
            }
        }

        private double ComputeSubtreeError(int[] indices)
        {
            if (indices.Length == 0)
                return 0;

            int error = 0;
            int decIdx = this.PruningData.DataStoreInfo.DecisionFieldIndex;
            for (int i = 0; i < indices.Length; i++)
                if (this.PruningData.GetFieldIndexValue(i, decIdx) != predictionResult[i])
                    error++;

            return error / (double)indices.Length;
        }

        private double ComputeGain(IDecisionTreeNode node)
        {
            if (node.IsLeaf)
                return Double.NegativeInfinity;

            // Compute the sum of misclassifications at the children
            double sum = 0;
            foreach (var child in node.Children)
                sum += info[child].error;

            // Get the misclassifications at the current node
            double current = info[node].error;

            // Compute the expected gain at the current node:
            return sum - current;
        }
        
        private double ComputeError()
        {
            ClassificationResult result = Classifier.DefaultClassifer.Classify(this.DecisionTree, this.PruningData);
            return result.Error;
        }

        public override double Prune()
        {
            double lastError;
            double error = Classifier.DefaultClassifer.Classify(this.DecisionTree, this.PruningData).Error;

            do
            {
                lastError = error;
                error = this.SingleRun();
            }
            while (error < lastError);

            return error;
        }

        private double SingleRun()
        {
            foreach (var node in this.DecisionTree)
            {
                var nodeInfo = this.info[node];
                nodeInfo.error = this.ComputeSubtreeError(nodeInfo.subset.ToArray());
            }

            foreach (var node in this.DecisionTree)
                this.info[node].gain = this.ComputeGain(node);

            double maxGain = Double.NegativeInfinity;
            IDecisionTreeNode maxNode = null;
            foreach (var node in this.DecisionTree)
            {
                var nodeInfo = this.info[node];
                if (nodeInfo.gain > maxGain)
                {
                    maxGain = nodeInfo.gain;
                    maxNode = node;
                }
            }

            if (maxGain >= 0 && maxNode != null)
            {
                int[] indices = this.info[maxNode].subset.ToArray();
                long[] outputs = this.PruningData.GetDecisionValue(indices);
                
                long majorDecision = outputs.Mode();                

                maxNode.Children = null;
                maxNode.Output = majorDecision;

                for (int i = 0; i < indices.Length; i++)
                    this.ComputePrediction(maxNode, indices[i]);
                
            }

            return this.ComputeError();
        }
    }
}
