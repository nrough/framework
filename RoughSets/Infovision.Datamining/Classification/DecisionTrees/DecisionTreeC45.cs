using Infovision.Statistics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Infovision.MachineLearning.Classification.DecisionTrees
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

        public DecisionTreeC45(string modelName)
            : base(modelName)
        {
            this.ImpurityFunction = ImpurityFunctions.Entropy;
            this.ImpurityNormalize = ImpurityFunctions.SplitInformationNormalize;
        }                                
    }
}
