using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Raccoon.MachineLearning.Classification.DecisionTrees
{
    /// <summary>
    /// CART Tree implementation
    /// </summary>
    /// <remarks>
    /// Implementation is based on the following example http://csucidatamining.weebly.com/assign-4.html
    /// </remarks>
    public class DecisionTreeCART : DecisionTreeBase
    {
        public DecisionTreeCART()
            : base()
        {
            this.ImpurityFunction = ImpurityMeasure.Gini;
        }

        public DecisionTreeCART(string modelName)
            : base(modelName)
        {
            this.ImpurityFunction = ImpurityMeasure.Gini;
        }        
    }
}
