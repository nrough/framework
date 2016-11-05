using Infovision.Data;
using Infovision.Datamining.Roughset.DecisionTrees;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infovision.Datamining.Roughset.DecisionTables
{
    public class DecisionTableLocal : ILearner, IPredictionModel
    {
        private ObliviousDecisionTree obliviousDecisionTree;

        public long? DefaultOutput { get; set; }
        public double Epsilon { get; set; }

        public virtual ClassificationResult Learn(DataStore data, int[] attributes)
        {


            return Classifier.DefaultClassifer.Classify(this, data);
        }

        public virtual void SetClassificationResultParameters(ClassificationResult result)
        {
            result.Epsilon = this.Epsilon;
            result.EnsembleSize = 1;
            result.ModelName = this.GetType().Name;
            //result.NumberOfRules = this.eqClassCollection.Count;
            //result.QualityRatio = this.eqClassCollection.Attributes.Length;
        }

        public virtual long Compute(DataRecordInternal record)
        {
            return this.obliviousDecisionTree.Compute(record);
        }
    }
}
