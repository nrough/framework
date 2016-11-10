using System;
using System.Linq;
using System.Collections.Generic;
using Infovision.Statistics;

namespace Infovision.Datamining.Roughset.DecisionTrees
{
    /// <summary>
    /// Decision tree learning where in each node a split is selected based on attribute generating highest measure M. 
    /// Measure M is calculated based on object weights.
    /// </summary>
    public class DecisionTreeRough : DecisionTreeBase 
    {
        public DecisionTreeRough()
            : base()
        {
            this.ImpurityFunction = ImpurityFunctions.Majority;
            this.ImpurityNormalize = ImpurityFunctions.DummyNormalize;
        }

        public DecisionTreeRough(string modelName)
            : base(modelName)
        {
            this.ModelName = modelName;

            this.ImpurityFunction = ImpurityFunctions.Majority;
            this.ImpurityNormalize = ImpurityFunctions.DummyNormalize;
        }

        protected override DecisionTreeBase CreateInstanceForClone()
        {
            return new DecisionTreeRough();
        }        
    }
}
