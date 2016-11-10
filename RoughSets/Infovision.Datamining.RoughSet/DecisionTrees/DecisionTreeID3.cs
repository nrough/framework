using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infovision.Datamining.Roughset.DecisionTrees
{
    /// <summary>
    /// ID3 Tree Implementation
    /// </summary>
    public class DecisionTreeID3 : DecisionTreeBase
    {
        public DecisionTreeID3()
            : base()
        {
            this.ImpurityFunction = ImpurityFunctions.Entropy;
        }

        public DecisionTreeID3(string modelName)
            : base(modelName)
        {
            this.ImpurityFunction = ImpurityFunctions.Entropy;
        }

        protected override DecisionTreeBase CreateInstanceForClone()
        {
            return new DecisionTreeID3();
        }                             
    }
}
