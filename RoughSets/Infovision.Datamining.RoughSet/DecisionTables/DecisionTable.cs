using Infovision.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infovision.Datamining.Roughset.DecisionTables
{
    public class DecisionTable : ILearner, IPredictionModel
    {
        private EquivalenceClassCollection eqClassCollection;

        public long? DefaultOutput { get; set; }        

        public virtual ClassificationResult Learn(DataStore data, int[] attributes)
        {
            this.eqClassCollection = EquivalenceClassCollection.Create(attributes, data);
            return Classifier.DefaultClassifer.Classify(this, data);
        }

        public virtual void SetClassificationResultParameters(ClassificationResult result)
        {            
            result.EnsembleSize = 1;
            result.ModelName = this.GetType().Name;
            result.NumberOfRules = this.eqClassCollection.Count;
            result.AvgNumberOfAttributes = this.eqClassCollection.Attributes.Length;
        }

        public virtual long Compute(DataRecordInternal record)
        {
            EquivalenceClass eq = this.eqClassCollection.Find(record);
            if (eq != null)
                return eq.DecisionDistribution.Output;
            return -1;
        }
    }
}