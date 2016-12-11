using Infovision.Data;
using Infovision.Math;
using Infovision.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infovision.MachineLearning.Classification.DecisionTrees.Pruning
{
    public class ErrorBasedPruning : DecisionTreePruningBase
    {       
        //private object syncRoot = new object();
        private Dictionary<IDecisionTreeNode, List<int>> node2indices;
        private long[] predictionResult;

        public double Confidence { get; set; }        

        public ErrorBasedPruning(IDecisionTree decisionTree, DataStore data)
            : base(decisionTree, data)
        {            
            this.Confidence = 0.025;

            this.predictionResult = new long[data.NumberOfRecords];
            this.predictionResult.SetAll(-1);

            this.node2indices = new Dictionary<IDecisionTreeNode, List<int>>();
            foreach (IDecisionTreeNode node in this.DecisionTree)
                this.node2indices[node] = new List<int>();

            for (int i = 0; i < data.NumberOfRecords; i++)
                this.ComputePrediction(this.DecisionTree.Root, i);            
        }

        private void ComputePrediction(IDecisionTreeNode node, int objectIdx)
        {
            DataRecordInternal record = this.PruningData.GetRecordByIndex(objectIdx, false);

            IDecisionTreeNode current = node;
            while (current != null)
            {
                node2indices[current].Add(objectIdx);
                if (current.IsLeaf)
                {
                    predictionResult[objectIdx] = current.Output;
                    break;
                }

                current = current.Children
                    .Where(x => x.Compute(record[x.Attribute]))
                    .FirstOrDefault();
            }
        }

        private bool  TryToPrune(IDecisionTreeNode node)
        {
            int[] instances = node2indices[node].ToArray();

            if (instances.Length == 0)
            {
                node.Children = null;
                node.OutputDistribution = null;

                return true;
            }

            double baselineError = this.ComputeError();
            baselineError = ErrorBasedPruning.UpperErrorBound(this.PruningData.NumberOfRecords, baselineError, this.Confidence);
            
            var majorDecision = node.OutputDistribution;
            double errorLeaf = ComputeErrorWithoutSubtree(node, majorDecision);
            errorLeaf = ErrorBasedPruning.UpperErrorBound(this.PruningData.NumberOfRecords, errorLeaf, this.Confidence);

            IDecisionTreeNode maxChild = GetMaxChild(node);
            double errorLargestBranch = ComputeErrorReplacingSubtrees(node, maxChild);
            errorLargestBranch = ErrorBasedPruning.UpperErrorBound(this.PruningData.NumberOfRecords, errorLargestBranch, this.Confidence);            

            bool changed = false;

            if ((errorLeaf <= (baselineError + this.GainThreshold + 0.00000001))
                || (errorLargestBranch <= (baselineError + this.GainThreshold + 0.00000001)))
            {
                if (errorLargestBranch < errorLeaf)
                {
                    // Replace the subtree with its maximum child
                    node.Children = maxChild.Children;
                    node.OutputDistribution = maxChild.OutputDistribution;

                    if (maxChild.IsLeaf == false)
                        foreach (var child in node.Children)
                            child.Parent = node;
                }
                else
                {
                    // Prune the subtree
                    node.Children = null;
                    node.OutputDistribution = majorDecision;
                }

                changed = true;

                TreeNodeTraversal.TraversePostOrder(node, n => { node2indices[n].Clear(); });

                //We cleared all the object indices from the node 2 indices map
                //So now we should once again recalc indices on this tree

                for (int i = 0; i < instances.Length; i++)
                    this.ComputePrediction(node, instances[i]);
            }

            return changed;
        }               
        
        public static double UpperErrorBound(int n, double errorRate, double confidence)
        {
            return errorRate + Tools.MarginOfErrorUpper(n, errorRate, confidence);
        }

        private double ComputeError()
        {
            ClassificationResult result = Classifier.DefaultClassifer.Classify(this.DecisionTree, this.PruningData);
            return result.Error;
        }

        //private double ComputeErrorWithoutSubtree(IDecisionTreeNode tree, long majorDecision)
        private double ComputeErrorWithoutSubtree(IDecisionTreeNode tree, DecisionDistribution majorDecision)
        {
            var children = tree.Children;
            var outputDistribution = tree.OutputDistribution;

            tree.Children = null;
            tree.OutputDistribution = majorDecision;

            double error = this.ComputeError();

            tree.Children = children;
            tree.OutputDistribution = outputDistribution;

            return error;
        }

        private double ComputeErrorReplacingSubtrees(IDecisionTreeNode tree, IDecisionTreeNode child)
        {
            var branches = tree.Children;
            var outputDistribution = tree.OutputDistribution;

            tree.Children = child.Children;
            tree.OutputDistribution = child.OutputDistribution;

            double error = this.ComputeError();

            tree.Children = branches;
            tree.OutputDistribution = outputDistribution;

            return error;
        }
               
        private IDecisionTreeNode GetMaxChild(IDecisionTreeNode tree)
        {
            IDecisionTreeNode max = null;
            int maxCount = Int32.MinValue;

            foreach (var child in tree.Children)
            {
                var list = node2indices[child];
                if (list.Count > maxCount)
                {
                    max = child;
                    maxCount = list.Count;
                }
            }

            return max;
        }

        public override double Prune()
        {
            this.SingleRun();
            return this.ComputeError();            
        }

        private double SingleRun()
        {            
            List<IDecisionTreeNode> nodeList = new List<IDecisionTreeNode>();
            TreeNodeTraversal.TraversePostOrder(this.DecisionTree.Root, node => nodeList.Add(node));            
            foreach (IDecisionTreeNode node in nodeList)
            {
                if (node.IsLeaf)
                    continue;
                this.TryToPrune(node);                
            }

            return this.ComputeError();
        }
    }
}
