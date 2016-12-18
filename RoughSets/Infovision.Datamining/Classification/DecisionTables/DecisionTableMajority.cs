﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Infovision.Data;

namespace Infovision.MachineLearning.Classification.DecisionTables
{
    public class DecisionTableMajority : DecisionTable
    {
        private DecisionDistribution aprioriDistribution;

        public DecisionTableMajority()
            : base()
        {
        }

        public DecisionTableMajority(string modelName)
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