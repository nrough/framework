using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NRough.MachineLearning
{
    public interface IApproximationModel
    {
        double Epsilon { get; }
    }
}
