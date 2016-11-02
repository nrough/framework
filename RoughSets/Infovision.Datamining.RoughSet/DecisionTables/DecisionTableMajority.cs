using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Infovision.Data;

namespace Infovision.Datamining.Roughset.DecisionTables
{
    public class DecisionTableMajority : DecisionTable
    {
        private DecisionDistribution aprioriDistribution;

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
            return this.aprioriDistribution.Output;
        }
    }
}
