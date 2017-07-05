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

            this.node2indices = new Dictionary<IDecisionTreeNode, List<int>>(decisionTree.Count());
            foreach (IDecisionTreeNode node in this.DecisionTree)
                this.node2indices[node] = new List<int>();

            for (int i = 0; i < data.NumberOfRecords; i++)
                this.ComputePrediction(this.DecisionTree.Root, i);
        }

        private void ComputePrediction(IDecisionTreeNode node, int objectIdx)
        {            
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
                    .Where(x => x.Compute(PruningData.GetFieldValue(objectIdx, x.Attribute)))
                    .FirstOrDefault();
            }
        }

        private bool TryToPrune(IDecisionTreeNode node)
        {
            //keep copy of instances for later use
            var instances = node2indices[node].ToArray(); 
            if(instances.Length == 0)
            {
                node.Children = null;
                node.OutputDistribution = null;                
                return true;
            }

            double baselineError = ComputeError();
            baselineError = ErrorBasedPruning.UpperErrorBound(PruningData.NumberOfRecords, baselineError, Confidence);
            
            var majorDecision = node.OutputDistribution;
            double errorLeaf = ComputeErrorWithoutSubtree(node, majorDecision);
            errorLeaf = ErrorBasedPruning.UpperErrorBound(PruningData.NumberOfRecords, errorLeaf, Confidence);

            IDecisionTreeNode maxChild = GetMaxChild(node);
            double errorLargestBranch = ComputeErrorReplacingSubtrees(node, maxChild);
            errorLargestBranch = ErrorBasedPruning.UpperErrorBound(PruningData.NumberOfRecords, errorLargestBranch, Confidence);            

            bool changed = false;

            if ((errorLeaf <= (baselineError + GainThreshold + 0.00000001))
                || (errorLargestBranch <= (baselineError + GainThreshold + 0.00000001)))
            {
                TreeNodeTraversal.TraversePostOrder(node, n => { node2indices[n].Clear(); });

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
            //ClassificationResult result = Classifier.Default.Classify(this.DecisionTree, this.PruningData);
            //return result.Error;

            int[] correct = new int[PruningData.NumberOfRecords];
            ParallelOptions options = new ParallelOptions()
            {
                MaxDegreeOfParallelism = ConfigManager.MaxDegreeOfParallelism
            };

            Parallel.For(0, PruningData.NumberOfRecords, options, objectIndex =>
            {
                DataRecordInternal record = PruningData.GetRecordByIndex(objectIndex, false);
                var prediction = DecisionTree.Compute(record);

                if (prediction == Classifier.UnclassifiedOutput && DecisionTree.DefaultOutput.HasValue)
                    prediction = (long)DecisionTree.DefaultOutput;

                if (prediction == record[PruningData.DataStoreInfo.DecisionFieldId])
                    correct[objectIndex] = 1;
            });

            return 1.0 - ((double)correct.Sum() / (double)correct.Length);
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

        private double ComputeErrorReplacingSubtrees(IDecisionTreeNode node, IDecisionTreeNode child)
        {
            var branches = node.Children;
            var outputDistribution = node.OutputDistribution;

            node.Children = child.Children;
            node.OutputDistribution = child.OutputDistribution;

            double error = this.ComputeError();

            node.Children = branches;
            node.OutputDistribution = outputDistribution;

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
            return this.SingleRun();
        }

        private double SingleRun()
        {            
            List<IDecisionTreeNode> nodeList = new List<IDecisionTreeNode>(node2indices.Count);
            TreeNodeTraversal.TraverseLevelOrder(this.DecisionTree.Root, node => nodeList.Add(node));
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
