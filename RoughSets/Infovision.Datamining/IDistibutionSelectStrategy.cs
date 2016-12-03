using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infovision.MachineLearning
{
    public interface IDistibutionSelectStrategy
    {
        long Select(IDictionary<long, double> distrubution);
    }
}
