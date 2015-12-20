using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infovision.Datamining.Roughset
{
    public enum WeightGeneratorType : int
    {
        None = 0,
        Majority = 1,
        Relative = 2,
        Constant = 3,
        Random = 4,
        Boosting = 5
    }
}
