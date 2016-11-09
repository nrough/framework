using Infovision.Statistics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Infovision.Datamining.Roughset.DecisionTrees
{            
    /// <summary>
    /// C4.5 Tree Implemetation
    /// </summary>
    public class DecisionTreeC45 : DecisionTreeBase
    {
        public DecisionTreeC45()
            : base()
        {
            this.ImpurityFunction = ImpurityFunctions.Entropy;
            this.ImpurityNormalize = ImpurityFunctions.SplitInformationNormalize;
        }

        protected override DecisionTreeBase CreateInstanceForClone()
        {
            return new DecisionTreeC45();
        }                         
    }
}
