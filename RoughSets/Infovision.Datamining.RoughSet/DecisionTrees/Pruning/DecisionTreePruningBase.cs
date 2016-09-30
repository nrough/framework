using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infovision.Datamining.Roughset.DecisionTrees.Pruning
{
    public class DecisionTreePruningBase : IDecisionTreePruning
    {
        public double Treshold { get; set; }
    }
}
