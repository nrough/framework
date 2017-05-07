using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NRough.Data;

namespace NRough.MachineLearning.Classification.DecisionLookup
{
    public class DecisionLookupMajority : DecisionLookup
    {
        private DecisionDistribution aprioriDistribution;

        public DecisionLookupMajority()
            : base()
        {
        }

        public DecisionLookupMajority(string modelName)
            : base(modelName)
        {
        }

        public override ClassificationResult Learn(DataStore data, int[] attributes)
        {
            this.aprioriDistribution = EquivalenceClassCollection.Create(new int[] { }, data).DecisionDistribution;
            return base.Learn(data, attributes);
        }

        public override long Compute(DataRecordInternal record)
        {
            long result = base.Compute(record);
            if (result != -1)
                return result;
            return (this.DefaultOutput == null) ? this.aprioriDistribution.Output : (long)this.DefaultOutput;
        }
    }
}
