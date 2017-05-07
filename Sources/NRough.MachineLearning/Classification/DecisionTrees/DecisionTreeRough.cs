using System;
using System.Linq;
using System.Collections.Generic;
using NRough.Math;

namespace NRough.MachineLearning.Classification.DecisionTrees
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
            this.ImpurityFunction = ImpurityMeasure.Majority2;
            //this.ImpurityNormalize = ImpurityMeasure.SplitInformationNormalize;
        }

        public DecisionTreeRough(string modelName)
            : base(modelName)
        {
            this.ImpurityFunction = ImpurityMeasure.Majority2;
            //this.ImpurityNormalize = ImpurityMeasure.SplitInformationNormalize;
        }               
    }
}
