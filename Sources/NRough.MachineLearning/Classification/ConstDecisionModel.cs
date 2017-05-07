using NRough.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NRough.MachineLearning.Classification
{
    [Serializable]
    public class ConstDecisionModel : ClassificationModelBase, IClassificationModel, ILearner, ICloneable
    {
        private long output = -1;
        public long Output
        {
            get { return output; }
            set { this.output = value; }
        }
        public ClassificationResult Learn(DataStore data, int[] attributes)
        {
            if (Output == -1)
            {
                DecisionDistribution dist =
                    EquivalenceClassCollection.Create(
                        new int[] { }, data).DecisionDistribution;
                this.Output = dist.Output;
            }

            return Classifier.Default.Classify(this, data);
        }

        public long Compute(DataRecordInternal record)
        {
            return Output;
        }        
    }
}
