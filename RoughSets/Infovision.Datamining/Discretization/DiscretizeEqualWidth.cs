using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infovision.MachineLearning.Discretization
{
    public class DiscretizeEqualWidth : DiscretizeUnsupervisedBase
    {
        public DiscretizeEqualWidth()
            : base() { }

        public override long[] ComputeCuts(long[] data, double[] weights)
        {
            long max = data.Max();
            long min = data.Min();                        
            long binWidth = (max - min) / this.NumberOfBuckets;

            long[] cutPoints = null;
            if ((this.NumberOfBuckets > 1) && (binWidth > 0))
            {
                cutPoints = new long[this.NumberOfBuckets - 1];
                for (int i = 1; i < this.NumberOfBuckets; i++)
                    cutPoints[i - 1] = min + (binWidth * i);
            }

            return cutPoints;
        }
    }
}
