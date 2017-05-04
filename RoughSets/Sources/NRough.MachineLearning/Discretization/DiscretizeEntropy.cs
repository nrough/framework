using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NRough.MachineLearning.Discretization
{
    [Serializable]
    public class DiscretizeEntropy : DiscretizeUnsupervisedBase
    {
        public DiscretizeEntropy()
            : base()
        {
            this.IsDataSorted = true;
            this.UseWeights = true;
            //max number of buckets
            this.NumberOfBuckets = 10; 
        }

        public DiscretizeEntropy(int maxNumberOfBuckets)
            : base()
        {
            this.NumberOfBuckets = maxNumberOfBuckets;
        }

        public override long[] ComputeCuts(long[] data, double[] weights)
        {
            long min = data.Min();
            long max = data.Max();                        
            double binWidth = 0.0;            
            int bestNumOfBins = 1;
            double[] distribution;
            double bestEntropy = Double.PositiveInfinity;
            for (int i = 0; i < this.NumberOfBuckets; i++)
            {
                distribution = new double[i + 1];
                binWidth = (max - min) / (double)(i + 1);                
                for (int j = 0; j < data.Length; j++)
                    for (int k = 0; k < i + 1; k++)
                        if (data[j] <= (min + ((k + 1) * binWidth)))
                        {
                            distribution[k] += (weights == null) ? 1.0 : weights[j];
                            break;
                        }
            
                double entropy = 0;
                for (int k = 0; k < i + 1; k++)
                {
                    if (distribution[k] < 2)
                    {
                        entropy = Double.PositiveInfinity;
                        break;
                    }
                    entropy -= distribution[k] * System.Math.Log((distribution[k] - 1) / binWidth, 2);
                }

                if (entropy < bestEntropy)
                {
                    bestEntropy = entropy;
                    bestNumOfBins = i + 1;
                }
            }
                                  
            binWidth = bestNumOfBins > 0 ? (max - min) / bestNumOfBins : 0.0;
            if ((bestNumOfBins > 1) && (binWidth > 0))
            {
                long[] cutPoints = new long[bestNumOfBins - 1];
                for (int i = 1; i < bestNumOfBins; i++)
                    cutPoints[i - 1] = (long) (min + (binWidth * i));
                return cutPoints;
            }

            return null;            
        }
    }
}
