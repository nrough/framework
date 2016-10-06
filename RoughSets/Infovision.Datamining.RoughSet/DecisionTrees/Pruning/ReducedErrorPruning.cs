using Infovision.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infovision.Datamining.Roughset.DecisionTrees.Pruning
{
    public class ReducedErrorPruning : DecisionTreePruningBase
    {
        public ReducedErrorPruning(IDecisionTree decisionTree, DataStore data)
            : base(decisionTree, data)
        {

        }

        public override double Run()
        {
            throw new NotImplementedException();
        }
    }
}
