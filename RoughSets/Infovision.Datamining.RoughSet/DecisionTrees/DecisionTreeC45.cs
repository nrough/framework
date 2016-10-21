using Infovision.Statistics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Infovision.Data;
using Infovision.Datamining.Roughset.DecisionTrees.Pruning;

namespace Infovision.Datamining.Roughset.DecisionTrees
{            
    /// <summary>
    /// C4.5 Tree Implemetation
    /// </summary>
    public class DecisionTreeC45 : DecisionTreeBase
    {
        
        public bool Pruning { get; set; }
        public int PruningCVFolds { get; set; }
        public PruningObjectiveType PruningObjective { get; set;}

        public DecisionTreeC45()
            : base()
        {
            this.Pruning = false;
            this.PruningObjective = PruningObjectiveType.MinimizeNumberOfLeafs;
            this.PruningCVFolds = 3;
        }

        private ClassificationResult LearnAndPrune(DataStore data, int[] attributes)
        {
            this.Init(data, attributes);

            this.CheckPruningConditions();

            DataStoreSplitter cvSplitter = new DataStoreSplitter(data, this.PruningCVFolds);
            DataStore trainSet = null, pruningSet = null;
            DecisionTreeC45 bestModel = null;

            int bestNumOfRules = Int32.MaxValue;
            double bestError = Double.PositiveInfinity;
            int bestMaxHeight = Int32.MaxValue;
            double bestAvgHeight = Double.PositiveInfinity;

            for (int f = 0; f < this.PruningCVFolds; f++)
            {
                cvSplitter.Split(ref trainSet, ref pruningSet, f);

                DecisionTreeC45 tmpTree = new DecisionTreeC45();
                tmpTree.MinimumNumOfInstancesPerLeaf = this.MinimumNumOfInstancesPerLeaf;
                tmpTree.Epsilon = this.Epsilon;
                tmpTree.MaxHeight = this.MaxHeight;
                tmpTree.NumberOfAttributesToCheckForSplit = this.NumberOfAttributesToCheckForSplit;
                tmpTree.Pruning = false;
                tmpTree.MinimumNumOfInstancesPerLeaf = this.MinimumNumOfInstancesPerLeaf;
                tmpTree.Learn(trainSet, attributes);

                ErrorBasedPruning pruningMethod = new ErrorBasedPruning(tmpTree, pruningSet);
                pruningMethod.Prune();

                ClassificationResult tmpResult = Classifier.DefaultClassifer.Classify(tmpTree, pruningSet);
                int numOfRules = DecisionTreeBase.GetNumberOfRules(tmpTree);
                int maxHeight = DecisionTreeBase.GetHeight(tmpTree);
                double avgHeight = DecisionTreeBase.GetAvgHeight(tmpTree);

                switch (this.PruningObjective)
                {
                    case PruningObjectiveType.MinimizeNumberOfLeafs:
                        if (numOfRules < bestNumOfRules)
                        {
                            bestNumOfRules = numOfRules;
                            bestModel = tmpTree;
                        }
                        break;

                    case PruningObjectiveType.MinimizeTreeMaxHeight:
                        if (maxHeight < bestMaxHeight)
                        {
                            bestMaxHeight = maxHeight;
                            bestModel = tmpTree;
                        }
                        break;

                    case PruningObjectiveType.MinimizeError:
                        if (tmpResult.Error < bestError)
                        {
                            bestError = tmpResult.Error;
                            bestModel = tmpTree;
                        }
                        break;

                    case PruningObjectiveType.MinimizeTreeAvgHeight:
                        if (avgHeight < bestAvgHeight)
                        {
                            bestAvgHeight = avgHeight;
                            bestModel = tmpTree;
                        }
                        break;

                    default:
                        throw new NotImplementedException(String.Format("Pruning objective type {0} is not implemented", this.PruningObjective));
                }
            }

            this.Root = bestModel.Root;

            return Classifier.DefaultClassifer.Classify(this, data);
        }

        private void CheckPruningConditions()
        {
            if (this.PruningCVFolds <= 1)
                throw new InvalidOperationException("this.PruningCVFolds <= 1");
            if (this.PruningObjective == PruningObjectiveType.None)
                throw new InvalidOperationException("this.PruningObjective == PruningObjectiveType.None");
            if (this.PruningCVFolds > this.TrainingData.NumberOfRecords)
                throw new InvalidOperationException("this.PruningCVFolds > data.NumberOfRecords");
        }


        public override ClassificationResult Learn(DataStore data, int[] attributes)
        {            
            if (this.Pruning)       
                return this.LearnAndPrune(data, attributes);
            
            return base.Learn(data, attributes);            
        }
                

        protected override double GetCurrentScore(EquivalenceClassCollection eqClassCollection)
        {
            double entropy = 0;
            foreach (long dec in this.Decisions)
            {
                double decWeightedProbability = eqClassCollection.CountWeightDecision(dec);
                double p = decWeightedProbability / eqClassCollection.WeightSum;
                if (p != 0)
                    entropy -= p * System.Math.Log(p, 2);
            }
            return entropy;
        }
       
        protected override SplitInfo GetSplitInfoSymbolic(int attributeId, EquivalenceClassCollection data, double entropy)
        {
            //double gain = base.GetSplitScore(attributeEqClasses, entropy);

            var attributeEqClasses = EquivalenceClassCollection.Create(attributeId, data);

            double gain;
            double result = 0;
            foreach (var eq in attributeEqClasses)
            {
                double localEntropy = 0;
                foreach (var dec in eq.DecisionSet)
                {
                    double decWeight = eq.GetDecisionWeight(dec);
                    double p = decWeight / eq.WeightSum;
                    if (p != 0)
                        localEntropy -= p * System.Math.Log(p, 2);
                }

                //in other place where entropy for continuous attr is calculated we have result -= ...
                result += (eq.WeightSum / data.WeightSum) * localEntropy; 
            }

            gain = entropy - result;

            double splitInfo = this.SplitInformation(attributeEqClasses);
            double normalizedGain = (splitInfo == 0) ? 0 : gain / splitInfo;

            return new SplitInfo(attributeId, normalizedGain, attributeEqClasses, SplitType.Discreet, ComparisonType.EqualTo, 0);            
        }

        protected override SplitInfo GetSplitInfoNumeric(int attributeId, EquivalenceClassCollection data, double entropy)
        {
            int attributeIdx = this.TrainingData.DataStoreInfo.GetFieldIndex(attributeId);
            int[] indices = data.Indices;
            long[] values = this.TrainingData.GetFieldIndexValue(indices, attributeIdx);

            //TODO can improve?
            Array.Sort(values, indices);

            List<long> thresholds = new List<long>(values.Length);

            if(values.Length > 0)
                thresholds.Add(values[0]);

            for (int k = 0; k < values.Length - 1; k++)
                if (values[k] != values[k + 1])
                    thresholds.Add((values[k] + values[k + 1]) / 2);

            long[] threshold = thresholds.ToArray();
            thresholds.Clear();
            double bestGain = Double.NegativeInfinity;
            int[] bestIdx1 = null, bestIdx2 = null;
            long bestThreshold;

            if (threshold.Length > 0)
            {
                bestThreshold = threshold[0];
                for (int k = 0; k < threshold.Length; k++)
                {
                    int[] idx1 = indices.Where(idx => (this.TrainingData.GetFieldIndexValue(idx, attributeIdx) <= threshold[k])).ToArray();
                    int[] idx2 = indices.Where(idx => (this.TrainingData.GetFieldIndexValue(idx, attributeIdx) > threshold[k])).ToArray();

                    long[] output1 = new long[idx1.Length];
                    long[] output2 = new long[idx2.Length];

                    for (int j = 0; j < idx1.Length; j++)
                        output1[j] = this.TrainingData.GetDecisionValue(idx1[j]);

                    for (int j = 0; j < idx2.Length; j++)
                        output2[j] = this.TrainingData.GetDecisionValue(idx2[j]);

                    double p1 = output1.Length / (double)indices.Length;
                    double p2 = output2.Length / (double)indices.Length;

                    double gain = -p1 * Tools.Entropy(output1, this.Decisions) +
                                  -p2 * Tools.Entropy(output2, this.Decisions);

                    if (gain > bestGain)
                    {
                        bestGain = gain;
                        bestThreshold = threshold[k];
                        bestIdx1 = idx1;
                        bestIdx2 = idx2;
                    }
                }
            }
            else
            {
                bestIdx1 = indices;
                bestGain = Double.NegativeInfinity;
                bestThreshold = long.MaxValue;
            }

            var attributeEqClasses = EquivalenceClassCollection
                .CreateFromBinaryPartition(attributeId, bestIdx1, bestIdx2, data.Data);

            double gain2 = entropy + bestGain;
            double splitInfo = this.SplitInformation(attributeEqClasses);
            double normalizedGain = (splitInfo == 0) ? 0 : gain2 / splitInfo;

            return new SplitInfo(attributeId, normalizedGain, attributeEqClasses, SplitType.Binary, ComparisonType.LessThanOrEqualTo, bestThreshold);

        }        

        private double SplitInformation(EquivalenceClassCollection eqClassCollection)
        {
            double result = 0;
            foreach (var eq in eqClassCollection)
            {
                double p = eq.WeightSum / eqClassCollection.WeightSum;
                if (p != 0)
                    result -= p * System.Math.Log(p, 2);
            }            

            return result;
        }
    }
}
