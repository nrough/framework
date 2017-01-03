using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infovision.MachineLearning.Discretization
{
    public class DiscretizeEqualFreqency : DiscretizeUnsupervisedBase
    {
        public DiscretizeEqualFreqency()
            : base() { }

        public override long[] ComputeCuts(long[] data, double[] weights)
        {
            throw new NotImplementedException();
        }
    }
}
