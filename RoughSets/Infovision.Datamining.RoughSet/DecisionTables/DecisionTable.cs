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
        public string ModelName { get; set; }

        public DecisionTable()
        {
            this.ModelName = this.GetType().Name;
        }

        public DecisionTable(string modelName)
            : this()
        {
            this.ModelName = modelName;
        }

        public virtual ClassificationResult Learn(DataStore data, int[] attributes)
        {
            this.eqClassCollection = EquivalenceClassCollection.Create(attributes, data);
            return Classifier.DefaultClassifer.Classify(this, data);
        }

        public virtual void SetClassificationResultParameters(ClassificationResult result)
        {            
            result.EnsembleSize = 1;
            result.ModelName = this.ModelName;
            result.NumberOfRules = this.eqClassCollection.Count;
            result.AvgNumberOfAttributes = this.eqClassCollection.Attributes.Length;
            result.MaxTreeHeight = this.eqClassCollection.Attributes.Length;
            result.AvgTreeHeight = this.eqClassCollection.Attributes.Length;
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