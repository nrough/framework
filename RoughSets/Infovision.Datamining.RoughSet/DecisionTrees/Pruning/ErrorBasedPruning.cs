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
    public class ErrorBasedPruning : DecisionTreePruningBase
    {       
        private object syncRoot = new object();
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
                this.TrackDecisions(this.DecisionTree.Root, i);            
        }

        private void TrackDecisions(IDecisionTreeNode node, int objectIdx)
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
                    .Where(x => x.Compute(record[x.Key]))
                    .FirstOrDefault();
            }
        }

        private bool TryToPrune(IDecisionTreeNode node)
        {
            int[] instances = node2indices[node].ToArray();            
            if (instances.Length == 0)
            {
                node.Children = null;
                node.Output = -1;
                return true; 
            }            

            int size = instances.Length;

            //double baselineError = this.ComputeError(;)
            double baselineError = this.ComputeSubtreeError(instances);
            baselineError = ErrorBasedPruning.UpperBound(baselineError, size, this.Confidence);

            long[] outputs = new long[instances.Length];
            for (int i = 0; i < outputs.Length; i++)
                outputs[i] = this.PruningData.GetDecisionValue(instances[i]);

            long majorDecision = outputs.Mode();
            double pruneError = ComputeErrorWithoutSubtree(node, majorDecision);
            pruneError = ErrorBasedPruning.UpperBound(pruneError, size, this.Confidence);

            IDecisionTreeNode maxChild = GetMaxChild(node);            

            double replaceError = ComputeErrorReplacingSubtrees(node, maxChild);
            replaceError = ErrorBasedPruning.UpperBound(replaceError, size, this.Confidence);

            bool changed = false;
            if (System.Math.Abs(pruneError - baselineError) < this.Threshold
                || System.Math.Abs(replaceError - baselineError) < this.Threshold)
            {
                if (replaceError < pruneError)
                {
                    //TODO is This correct replacement?

                    // Replace the subtree with its maximum child                                        
                    node.Children = maxChild.Children;
                    node.Output = maxChild.Output;                    

                    if(maxChild.IsLeaf == false)
                        foreach (var child in node.Children)
                            child.Parent = node;
                }
                else
                {
                    // Prune the subtree
                    node.Children = null;
                    node.Output = majorDecision;
                }

                changed = true;

                TreeNodeTraversal.TraversePostOrder(node, n => { node2indices[n].Clear(); });

                //We cleared all the object indices from the node 2 indices map
                //So now we should once again recalc indices on this tree
                                                
                for (int i = 0; i < instances.Length; i++)
                    this.TrackDecisions(node, instances[i]);
                
            }

            return changed;
        }

        //http://www.statsblogs.com/2015/10/13/understanding-margins-of-error/
        private static double UpperBound(double error, int n, double confidence)
        {
            //TODO Apply Confidence Factor 
            //http://www.statisticshowto.com/how-to-find-a-confidence-interval/

            //Move this method to Statistics lib
            //1.96 * System.Math.Sqrt((error * (1 - error)) / n) - MOE (Margin Of Error)
            //1.96 this is only for 0.025 Confidence and infinite degrees of freedom
            //degrees of fredom = n - 1 (it is only importand for small n)
            return error + 1.96 * System.Math.Sqrt((error * (1 - error)) / n);
        }

        private double ComputeErrorWithoutSubtree(IDecisionTreeNode tree, long majorDecision)
        {
            var children = tree.Children;
            var output = tree.Output;

            tree.Children = null;
            tree.Output = majorDecision;

            double error = this.ComputeError();

            tree.Children = children;
            tree.Output = output;

            return error;
        }

        private double ComputeErrorReplacingSubtrees(IDecisionTreeNode tree, IDecisionTreeNode child)
        {
            var branches = tree.Children;
            var output = tree.Output;

            tree.Children = child.Children;
            tree.Output = child.Output;

            double error = this.ComputeError();

            tree.Children = branches;
            tree.Output = output;

            return error;
        }

        private double ComputeError()
        {
            ClassificationResult result = Classifier.DefaultClassifer.Classify(this.DecisionTree, this.PruningData);
            return 1.0 - result.Accuracy;
        }

        private double ComputeSubtreeError(int[] indices)
        {
            if (indices == null)
                throw new ArgumentNullException("indices");

            if (indices.Length == 0)
                throw new ArgumentException("indices", "indices.Length == 0");

            int correct = 0;
            for (int i = 0; i < indices.Length; i++)
                if (this.predictionResult[indices[i]] == this.PruningData.GetDecisionValue(i))
                    correct++;

            return (double)correct / (double)indices.Length;
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

        public override double Run()
        {
            double lastError;
            double error = Double.MaxValue;

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
            List<IDecisionTreeNode> nodeList = new List<IDecisionTreeNode>();
            TreeNodeTraversal.TraversePostOrder(this.DecisionTree.Root, node => nodeList.Add(node));
            foreach (IDecisionTreeNode node in nodeList)
            {
                if (node.IsLeaf)
                    continue;

                if (this.TryToPrune(node))
                    break;
            }

            return this.ComputeError();
        }
    }
}
