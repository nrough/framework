using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infovision.MachineLearning.Discretization
{
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
            int cpindex = 0, lastIndex = -1;

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
                        if (((freq - last) < (counter - freq)) && (lastIndex != -1))
                        {                           
                            cutPoints[cpindex] = (data[SortedIndices[lastIndex]] + data[SortedIndices[lastIndex + 1]]) / 2;
                            counter -= last;
                            last = counter;
                            lastIndex = i;
                        }
                        else
                        {                            
                            cutPoints[cpindex] = (data[SortedIndices[i]] + data[SortedIndices[i + 1]]) / 2;
                            counter = 0;
                            last = 0;
                            lastIndex = -1;
                        }

                        cpindex++;
                        freq = (weightsSum + counter) / ((cutPoints.Length + 1) - cpindex);
                    }
                    else
                    {
                        lastIndex = i;
                        last = counter;
                    }
                }
            }

            // Check whether there was another possibility for a cut point
            if ((cpindex < cutPoints.Length) && (lastIndex != -1))
            {                
                cutPoints[cpindex] = (data[SortedIndices[lastIndex]] + data[SortedIndices[lastIndex + 1]]) / 2;
                return cutPoints;
            }
            
            return null;           
        }

        #endregion
    }
}
