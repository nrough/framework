using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NRough.MachineLearning.Discretization
{
    public interface IDiscretizerSupervised : IDiscretizer
    {
        long[] ComputeCuts(long[] data, long[] labels, int start, int end, double[] weights);
    }
}
