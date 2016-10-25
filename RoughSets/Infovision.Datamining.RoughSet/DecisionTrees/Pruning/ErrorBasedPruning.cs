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
        //private object syncRoot = new object();
        private Dictionary<IDecisionTreeNode, List<int>> node2indices;
        private long[] predictionResult;

        public double Confidence { get; set; }        

        public ErrorBasedPruning(IDecisionTree decisionTree, DataStore data)
            : base(decisionTree, data)
        {            
            this.Confidence = 0.25;

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
                    predictionResult[objectIdx] = current.Output.FindMaxValueKey();
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
                node.Output = null;

                return true;
            }

            double baselineError = this.ComputeError();
            baselineError = ErrorBasedPruning.UpperBound(this.PruningData.NumberOfRecords, baselineError, this.Confidence);
            
            var majorDecision = node.Output;
            double errorLeaf = ComputeErrorWithoutSubtree(node, majorDecision);
            errorLeaf = ErrorBasedPruning.UpperBound(this.PruningData.NumberOfRecords, errorLeaf, this.Confidence);

            IDecisionTreeNode maxChild = GetMaxChild(node);
            double errorLargestBranch = ComputeErrorReplacingSubtrees(node, maxChild);
            errorLargestBranch = ErrorBasedPruning.UpperBound(this.PruningData.NumberOfRecords, errorLargestBranch, this.Confidence);            

            bool changed = false;

            if ((errorLeaf <= (baselineError + this.GainThreshold + 0.00000001))
                || (errorLargestBranch <= (baselineError + this.GainThreshold + 0.00000001)))
            {
                if (errorLargestBranch < errorLeaf)
                {
                    // Replace the subtree with its maximum child
                    node.Children = maxChild.Children;
                    node.Output = maxChild.Output;

                    if (maxChild.IsLeaf == false)
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
                    this.ComputePrediction(node, instances[i]);

            }

            return changed;
        }

        /*
        private bool TryToPruneLocal(IDecisionTreeNode node)
        {
            int[] instances = node2indices[node].ToArray();

            if (instances.Length == 0)
            {
                node.Children = null;
                node.Output = -1;

                return true; 
            }                        
            
            double baselineError = this.ComputeErrorLocal(instances);
            baselineError = ErrorBasedPruning.UpperBound(instances.Length, baselineError, this.Confidence);

            //long majorDecision = this.PruningData.GetDecisionValue(instances).Mode();
            //double errorLeaf = ComputeErrorWithoutSubtree(node, majorDecision);
            long majorDecision = node.Output;
            double errorLeaf = ComputeErrorWithoutSubtree(node, node.Output);l
            errorLeaf = ErrorBasedPruning.UpperBound(instances.Length, errorLeaf, this.Confidence);

            IDecisionTreeNode maxChild = GetMaxChild(node);
            double errorLargestBranch = ComputeErrorReplacingSubtreesLocal(node, maxChild);
            errorLargestBranch = ErrorBasedPruning.UpperBound(instances.Length, errorLargestBranch, this.Confidence);            

            bool changed = false;

            //if (System.Math.Abs(pruneError - baselineError) < this.GainThreshold
            //    || System.Math.Abs(replaceError - baselineError) < this.GainThreshold)
            if ((errorLeaf <= (baselineError + this.GainThreshold + 0.00000001))
                || (errorLargestBranch <= (baselineError + this.GainThreshold + 0.00000001)))
            {
                if (errorLargestBranch < errorLeaf)
                {                    
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
        */

        //http://www.statsblogs.com/2015/10/13/understanding-margins-of-error/
        //http://www.statisticshowto.com/how-to-find-a-confidence-interval/
        private static double UpperBound(int n, double errorRate, double confidence)
        {
            return errorRate + MarginOfError_WEKA(n, errorRate * n, confidence);
            
            //return errorRate + 1.96 * System.Math.Sqrt((errorRate * (1 - errorRate)) / n);
        }

        /**
         * Computes estimated extra error for given total number of instances
         * and error using normal approximation to binomial distribution
         * (and continuity correction).
         *
         * @param N number of instances
         * @param e observed error
         * @param CF confidence value
         */
        private static double MarginOfError_WEKA(double N, double e, double CF)
        {
            // Ignore stupid values for CF
            if (CF > 0.5)
                throw new ArgumentException("CF", "CF > 0.5");

            // Check for extreme cases at the low end because the
            // normal approximation won't work
            if (e < 1)
            {
                // Base case (i.e. e == 0) from documenta Geigy Scientific
                // Tables, 6th edition, page 185

                double baseError = N * (1 - System.Math.Pow(CF, 1 / N));
                if (e == 0)
                {
                    return baseError;
                }

                // Use linear interpolation between 0 and 1 like C4.5 does
                return baseError + e * (MarginOfError_WEKA(N, 1, CF) - baseError);
            }

            // Use linear interpolation at the high end (i.e. between N - 0.5
            // and N) because of the continuity correction
            if (e + 0.5 >= N)
            {

                // Make sure that we never return anything smaller than zero
                return System.Math.Max(N - e, 0);
            }

            // Get z-score corresponding to CF
            double z = Tools.NormalInverse(1 - CF);

            // Compute upper limit of confidence interval
            double f = (e + 0.5) / N;
            double r = (f + (z * z) / (2 * N) +
                z * System.Math.Sqrt((f / N) -
                          (f * f / N) +
                          (z * z / (4 * N * N)))) /
              (1 + (z * z) / N);

            return (r * N) - e;
        }

        /** Function to compute estimated extra error for given total number of itemsets and errors.
         *
         * @param N		The weight of all the itemsets.
         * @param e		The weight of the itemsets incorrectly classified.
         * @param CF	Minimum confidence.
         *
         * @return		The errors.
         */
        private static double MarginOfError_KEEL(double N, double e, double CF)
        {
            // Some constants for the interpolation.
            double[] Val = {0, 0.000000001, 0.00000001, 0.0000001, 0.000001,
                0.00001, 0.00005, 0.0001,
                0.0005, 0.001, 0.005, 0.01, 0.05, 0.10, 0.20, 0.40, 1.00};

            double[] Dev = {100, 6.0, 5.61, 5.2, 4.75, 4.26, 3.89, 3.72, 3.29, 3.09,
                2.58,
                2.33, 1.65, 1.28, 0.84, 0.25, 0.00};

            double Val0, Pr, Coeff = 0;
            int i = 0;

            while (CF > Val[i])
            {
                i++;
            }

            Coeff = Dev[i - 1] +
                    (Dev[i] - Dev[i - 1]) * (CF - Val[i - 1]) / (Val[i] - Val[i - 1]);
            Coeff = Coeff * Coeff;

            if (e == 0)
            {
                return N * (1 - System.Math.Exp(System.Math.Log(CF) / N));
            }
            else
            {
                if (e < 0.9999)
                {
                    Val0 = N * (1 - System.Math.Exp(System.Math.Log(CF) / N));

                    return Val0 + e * (MarginOfError_KEEL(N, 1.0, CF) - Val0);
                }
                else
                {
                    if (e + 0.5 >= N)
                    {
                        return 0.67 * (N - e);
                    }
                    else
                    {
                        Pr = (e + 0.5 + Coeff / 2 + System.Math.Sqrt(Coeff * ((e + 0.5)
                                * (1 - (e + 0.5) / N) + Coeff / 4))) / (N + Coeff);

                        return (N * Pr - e);
                    }
                }
            }
        }        

        /*
        private double ComputeErrorLocal(int[] indices)
        {
            if (indices == null)
                throw new ArgumentNullException("indices");

            if (indices.Length == 0)
                throw new ArgumentException("indices", "indices.Length == 0");

            int error = 0;
            for (int i = 0; i < indices.Length; i++)
                if (this.predictionResult[indices[i]] != this.PruningData.GetDecisionValue(i))
                    error++;

            return error / (double)indices.Length;
        }

        private double ComputeErrorWithoutSubtreeLocal(IDecisionTreeNode tree, long majorDecision)
        {
            int[] instances = this.node2indices[tree].ToArray();
            long[] outputs = this.PruningData.GetDecisionValue(instances);
            int error = 0;
            for (int i = 0; i < outputs.Length; i++)
                if (outputs[i] != majorDecision)
                    error++;
            return error / (double)instances.Length;
        }

        private double ComputeErrorReplacingSubtreesLocal(IDecisionTreeNode tree, IDecisionTreeNode child)
        {
            var branches = tree.Children;
            var output = tree.Output;

            tree.Children = child.Children;
            tree.Output = child.Output;

            int[] instances = this.node2indices[tree].ToArray();
            int error = 0;

            for (int i = 0; i < instances.Length; i++)
            {
                DataRecordInternal record = this.PruningData.GetRecordByIndex(i, false);
                IDecisionTreeNode current = tree;
                while (current != null)
                {                    
                    if (current.IsLeaf)
                    {
                        if (this.PruningData.GetDecisionValue(i) != current.Output)
                            error++;
                        break;                        
                    }

                    current = current.Children
                        .Where(x => x.Compute(record[x.Attribute]))
                        .FirstOrDefault();
                }
            }

            tree.Children = branches;
            tree.Output = output;

            return error / (double)instances.Length;
        }
        */

        private double ComputeError()
        {
            ClassificationResult result = Classifier.DefaultClassifer.Classify(this.DecisionTree, this.PruningData);
            return result.Error;
        }

        //private double ComputeErrorWithoutSubtree(IDecisionTreeNode tree, long majorDecision)
        private double ComputeErrorWithoutSubtree(IDecisionTreeNode tree, IDictionary<long, double> majorDecision)
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
            /*
            double lastError;
            double error = Double.PositiveInfinity;

            do
            {
                lastError = error;
                error = this.SingleRun();
            }
            while (error < lastError);

            return error;                        
            */
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

                /*
                if (this.CalculateErrorOnSubtrees)
                {
                    this.TryToPruneLocal(node);

                    //if (this.TryToPruneLocal(node))
                    //    break;
                }
                else
                {
                    this.TryToPrune(node);
                    //if (this.TryToPrune(node))
                    //    break;
                }
                */
            }

            return this.ComputeError();
        }
    }
}
