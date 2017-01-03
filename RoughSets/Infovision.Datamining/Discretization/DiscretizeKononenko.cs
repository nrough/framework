using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infovision.MachineLearning.Discretization
{
    public class DiscretizeKononenko : DiscretizeSupervisedBase
    {
        public DiscretizeKononenko()
            : base() {}

        public override long[] ComputeCuts(long[] data, long[] labels, double[] weights)
        {
            throw new NotImplementedException();
        }
    }
}
