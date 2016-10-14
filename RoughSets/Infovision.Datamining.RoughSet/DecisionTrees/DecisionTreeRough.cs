using System;
using System.Linq;
using System.Collections.Generic;
using Infovision.Statistics;

namespace Infovision.Datamining.Roughset.DecisionTrees
{
    /// <summary>
    /// Decision tree learning where in each node a split is selected based on attribute generating highest measure M. 
    /// Measure M is calculated based on object weights.
    /// </summary>
    public class DecisionTreeRough : DecisionTreeBase
    {
        protected override SplitInfo GetSplitInfoSymbolic(int attributeId, EquivalenceClassCollection data, double parentMeasure)
        {
            var attributeEqClasses = EquivalenceClassCollection.Create(attributeId, data);
            return new SplitInfo(attributeId,
                InformationMeasureWeights.Instance.Calc(attributeEqClasses) + parentMeasure,
                //InformationMeasureWeights.Instance.Calc(attributeEqClasses),
                attributeEqClasses, SplitType.Discreet, ComparisonType.EqualTo, 0);
        }

        protected override SplitInfo GetSplitInfoNumeric(int attributeId, EquivalenceClassCollection data, double parentMeasure)
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
            double bestM = Double.NegativeInfinity;
            long bestThreshold;
            EquivalenceClassCollection bestEqCollection = null;

            if (threshold.Length > 0)
            {
                bestThreshold = threshold[0];
                for (int k = 0; k < threshold.Length; k++)
                {
                    int[] idx1 = indices.Where(idx => (this.TrainingData.GetFieldIndexValue(idx, attributeIdx) <= threshold[k])).ToArray();
                    int[] idx2 = indices.Where(idx => (this.TrainingData.GetFieldIndexValue(idx, attributeIdx) > threshold[k])).ToArray();

                    var tmpEqCollection = EquivalenceClassCollection.CreateFromBinaryPartition(
                        attributeId, idx1, idx2, data.Data);

                    double gain = InformationMeasureWeights.Instance.Calc(tmpEqCollection);

                    if (gain > bestM)
                    {
                        bestM = gain;
                        bestThreshold = threshold[k];
                        bestEqCollection = tmpEqCollection;
                    }
                }
            }
            else
            {
                bestM = Double.NegativeInfinity;
                bestThreshold = long.MaxValue;
                bestEqCollection = EquivalenceClassCollection
                    .CreateFromBinaryPartition(attributeId, indices, null, data.Data);
            }

            return new SplitInfo(
                attributeId,
                parentMeasure + bestM, 
                bestEqCollection, 
                SplitType.Binary, 
                ComparisonType.LessThanOrEqualTo, 
                bestThreshold);

        }


        protected override double GetCurrentScore(EquivalenceClassCollection eqClassCollection)
        {
            //return 0;
            return InformationMeasureWeights.Instance.Calc(eqClassCollection);
        }
    }
}
