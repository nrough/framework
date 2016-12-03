using Infovision.Statistics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infovision.MachineLearning.Filters.Supervised.Attribute
{
    public class DiscretizationMDL
    {
        //TODO
        /*        
        public void Compute(long[] values, long[] outputs, long[] labels)
        {
            long[] localValues = new long[values.Length];
            Array.Copy(values, localValues, values.Length);

            //Array.Sort(values, indices);
            Array.Sort(localValues);

            List<long> thresholds = new List<long>(values.Length);

            for (int k = 0; k < values.Length - 1; k++)
                if (values[k] != values[k + 1])
                    thresholds.Add((values[k] + values[k + 1]) / 2);

            long[] threshold = thresholds.ToArray();
            thresholds = null;

            double bestM = Double.NegativeInfinity;
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

                    double gain = -p1 * Tools.Entropy(output1, labels) +
                                  -p2 * Tools.Entropy(output2, labels);

                    if (gain > bestM)
                    {
                        bestM = gain;
                        bestThreshold = threshold[k];
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
        }
        */
    }
}
