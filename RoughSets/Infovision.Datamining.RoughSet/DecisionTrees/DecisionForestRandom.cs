using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Infovision.Datamining;
using Infovision.Data;
using Infovision.Utils;

namespace Infovision.Datamining.Roughset.DecisionTrees
{
    public class DecisionForestRandom<T> : DecisionForestBase<T>
        where T : IDecisionTree, new()
    {
        public int NumberOfAttributesToCheckForSplit { get; set; }

        public DecisionForestRandom()
            : base()
        {
            this.NumberOfAttributesToCheckForSplit = -1;
        }

        protected override T InitDecisionTree()
        {
            T tree = base.InitDecisionTree();

            if (this.NumberOfAttributesToCheckForSplit > 0)
                tree.NumberOfAttributesToCheckForSplit = this.NumberOfAttributesToCheckForSplit;

            return tree;
        }        
    }
}
