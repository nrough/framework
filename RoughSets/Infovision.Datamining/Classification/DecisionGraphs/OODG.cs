using Raccoon.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Raccoon.MachineLearning.Classification.DecisionGraphs
{
    public class OODG : ModelBase, ILearner, IPredictionModel
    {
        public long? DefaultOutput { get; set; }

        public virtual ClassificationResult Learn(DataStore data, int[] attributes)
        {
            return null;
        }

        public virtual long Compute(DataRecordInternal record)
        {
            return -1;
        }

        public virtual void SetClassificationResultParameters(ClassificationResult result)
        {
            result.EnsembleSize = 1;
            result.ModelName = this.ModelName;            
        }
    }
}
