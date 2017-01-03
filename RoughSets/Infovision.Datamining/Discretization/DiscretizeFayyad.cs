using Infovision.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infovision.MachineLearning.Discretization
{
    public class DiscretizeFayyad : DiscretizeSupervisedBase
    {
        public DiscretizeFayyad()
            : base() {}

        public override long[] ComputeCuts(long[] data, long[] labels, double[] weights)
        {
            throw new NotImplementedException();
        }
    }
}
