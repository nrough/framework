using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NRough.MachineLearning.Discretization
{
    public interface IDiscretizer : ICloneable
    {
        long[] Cuts { get; set; }
        long[] Apply(long[] data);
        long Apply(long value);
        void Compute(long[] data, long[] labels, double[] weights);
        bool UseWeights { get; set; }
    }      
}
