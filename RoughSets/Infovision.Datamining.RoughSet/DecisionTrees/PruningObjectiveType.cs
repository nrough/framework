using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infovision.Datamining.Roughset.DecisionTrees
{
    public enum PruningObjectiveType
    {
        None = 0,
        MinimizeNumberOfLeafs = 1,
        MinimizeError = 2,
        MinimizeTreeMaxHeight = 3,
        MinimizeTreeAvgHeight = 4
    }
}
