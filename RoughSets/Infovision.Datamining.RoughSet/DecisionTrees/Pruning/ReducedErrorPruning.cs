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
        private double baselineError;

        private class NodeInfo
        {
            public List<int> subset;
            public double baselineError;    //error of using majority class (current node's output)
            public double error;            //error of using subtree
            public double gain;             //gain calculated as subtraction of baseLineError and error

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

        private double ComputeGain(IDecisionTreeNode node)
        {
            if (node.IsLeaf)
                return Double.NegativeInfinity;

            // Compute the sum of misclassifications at the children
            double sum = 0;
            foreach (var child in node.Children)
                sum += info[child].error;

            // Get the misclassifications at the current node
            //double current = info[node].error;
            double current = info[node].baselineError;

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
            this.baselineError = Classifier.DefaultClassifer.Classify(this.DecisionTree, this.PruningData).Error;

            double error = this.baselineError;
            double lastError;

            //TODO Change so we prune only if error rate is improved. Currently at least single prune is done.
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
                nodeInfo.error = this.ComputeSubtreeError(nodeInfo.subset);
                nodeInfo.baselineError = this.ComputeSubtreeBaselineError(node, nodeInfo.subset);
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
                maxNode.Children = null;

                //check
                if (maxNode.Output == -1)
                    throw new InvalidOperationException("maxNode.Output == -1");

                foreach(int idx in this.info[maxNode].subset.ToArray())
                    this.ComputePrediction(maxNode, idx);                                
            }

            return this.ComputeError();
        }
    }
}
