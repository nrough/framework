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
            throw new NotImplementedException();
        }
    }
}
