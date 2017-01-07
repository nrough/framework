using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infovision.MachineLearning.Discretization
{
    [Serializable]
    public class DiscretizeEqualFreqency : DiscretizeUnsupervisedBase
    {
        #region Properties

        double FixedWeightPerInterval { get; set; } = -1.0;

        #endregion

        #region Constructors

        public DiscretizeEqualFreqency()
            : base() { }

        #endregion

        #region Methods

        public override long[] ComputeCuts(long[] data, double[] weights)
        {                        
            double weightsSum = (weights == null) ? data.Length : weights.Sum();

            double freq = this.FixedWeightPerInterval > 0 
                        ? this.FixedWeightPerInterval 
                        : (weightsSum / this.NumberOfBuckets);

            long[] cutPoints = this.FixedWeightPerInterval > 0
                             ? new long[(int)(weightsSum / this.FixedWeightPerInterval)]
                             : new long[this.NumberOfBuckets - 1];
            
            double counter = 0, last = 0;
            int cutPointsIdx = 0, lastIdx = -1;

            for (int i = 0; i < data.Length - 1; i++)
            {
                counter += (weights == null) ? 1.0 : weights[i];
                weightsSum -= (weights == null) ? 1.0 : weights[i];
                
                if (data[SortedIndices[i]] < data[SortedIndices[i + 1]])
                {
                    // Have we passed the ideal size?
                    if (counter >= freq)
                    {
                        // Is this break point worse than the last one?
                        if (((freq - last) < (counter - freq)) && (lastIdx != -1))
                        {                           
                            cutPoints[cutPointsIdx] = (data[SortedIndices[lastIdx]] + data[SortedIndices[lastIdx + 1]]) / 2;
                            counter -= last;
                            last = counter;
                            lastIdx = i;
                        }
                        else
                        {                            
                            cutPoints[cutPointsIdx] = (data[SortedIndices[i]] + data[SortedIndices[i + 1]]) / 2;
                            counter = 0;
                            last = 0;
                            lastIdx = -1;
                        }

                        cutPointsIdx++;
                        freq = (weightsSum + counter) / ((cutPoints.Length + 1) - cutPointsIdx);
                    }
                    else
                    {
                        lastIdx = i;
                        last = counter;
                    }
                }
            }

            // Check whether there was another possibility for a cut point
            if ((cutPointsIdx < cutPoints.Length) && (lastIdx != -1))
            {                
                cutPoints[cutPointsIdx] = (data[SortedIndices[lastIdx]] + data[SortedIndices[lastIdx + 1]]) / 2;
                return cutPoints;
            }
            
            return null;           
        }

        #endregion
    }
}
