using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infovision.Datamining.Filters
{
    public enum DiscretizationType : int
    {
        None = 0,
        Unsupervised_Entropy = 1,
        Unsupervised_EqualFrequency = 2,
        Unsupervised_EqualWidth = 3,
        Supervised_KononenkoMDL = 4,
        Supervised_KononenkoMDL_BetterEncoding = 5,
        Supervised_FayyadAndIranisMDL = 6,
        Supervised_FayyadAndIranisMDL_BetterEncoding = 7,
        Unsupervised_Binary = 8
    }
}
