using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infovision.MachineLearning.Discretization
{
    public interface IDiscretization
    {
        long[] Cuts { get; set; }
        long[] Apply(long[] data);
        long Apply(long value);
    }      
}
