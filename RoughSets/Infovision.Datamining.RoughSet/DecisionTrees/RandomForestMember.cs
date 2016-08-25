using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infovision.Datamining.Roughset.DecisionTrees
{
    public class RandomForestMember : IRandomForestMember
    {
        public IDecisionTree Tree { get; set; }
        public double Error { get; set; }
    }
}
