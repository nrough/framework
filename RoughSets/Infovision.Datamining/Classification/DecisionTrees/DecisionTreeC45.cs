using Raccoon.Math;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Raccoon.MachineLearning.Classification.DecisionTrees
{            
    /// <summary>
    /// C4.5 Tree Implemetation
    /// </summary>
    public class DecisionTreeC45 : DecisionTreeBase
    {
        public DecisionTreeC45()
            : base()
        {
            this.ImpurityFunction = ImpurityMeasure.Entropy;
            this.ImpurityNormalize = ImpurityMeasure.SplitInformationNormalize;
        }

        public DecisionTreeC45(string modelName)
            : base(modelName)
        {
            this.ImpurityFunction = ImpurityMeasure.Entropy;
            this.ImpurityNormalize = ImpurityMeasure.SplitInformationNormalize;
        }                                
    }
}
