using Infovision.Statistics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infovision.Datamining.Roughset.DecisionTrees
{
    /// <summary>
    /// C4.5 Tree Implemetation
    /// </summary>
    public class DecisionTreeC45 : DecisionTreeBase
    {
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
                int decIdx = this.TrainingData.DataStoreInfo.DecisionFieldIndex;
                for (int k = 0; k < threshold.Length; k++)
                {
                    int[] idx1 = indices.Where(idx => (this.TrainingData.GetFieldIndexValue(idx, attributeIdx) <= threshold[k])).ToArray();
                    int[] idx2 = indices.Where(idx => (this.TrainingData.GetFieldIndexValue(idx, attributeIdx) > threshold[k])).ToArray();

                    long[] output1 = new long[idx1.Length];
                    long[] output2 = new long[idx2.Length];

                    for (int j = 0; j < idx1.Length; j++)
                        output1[j] = this.TrainingData.GetFieldIndexValue(idx1[j], decIdx);

                    for (int j = 0; j < idx2.Length; j++)
                        output2[j] = this.TrainingData.GetFieldIndexValue(idx2[j], decIdx);

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

            return new SplitInfo(attributeId, entropy + bestGain, attributeEqClasses, SplitType.Binary, ComparisonType.LessThanOrEqualTo, bestThreshold);

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
