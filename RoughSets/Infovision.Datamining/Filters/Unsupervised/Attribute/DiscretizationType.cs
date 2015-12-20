using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infovision.Datamining.Filters.Unsupervised.Attribute
{
    public enum DiscretizationType : int
    {
        None = 0,
        Entropy = 1,
        EqualFrequency = 2,
        EqualWidth = 3
    }
}
