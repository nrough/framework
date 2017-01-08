using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infovision.MachineLearning.Discretization
{
    public interface IDiscretizationUnsupervised : IDiscretization
    {
        long[] ComputeCuts(long[] data, double[] weights);        
    }
}
