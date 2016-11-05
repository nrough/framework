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
        private ObliviousDecisionTree obliviousDecisionTree = null;
        private DecisionDistribution aprioriDistribution;

        public long? DefaultOutput { get; set; }
        public double Epsilon { get; set; }
        public bool RankedAttributes { get; set; }

        public virtual ClassificationResult Learn(DataStore data, int[] attributes)
        {
            this.aprioriDistribution = EquivalenceClassCollection.Create(new int[] { }, data, data.Weights).DecisionDistribution;
            this.DefaultOutput = this.aprioriDistribution.Output;

            this.obliviousDecisionTree = new ObliviousDecisionTree();
            this.obliviousDecisionTree.UseLocalOutput = true;
            this.obliviousDecisionTree.RankedAttributes = this.RankedAttributes;
            this.obliviousDecisionTree.Learn(data, attributes);

            return Classifier.DefaultClassifer.Classify(this, data);
        }

        public virtual void SetClassificationResultParameters(ClassificationResult result)
        {
            result.Epsilon = this.Epsilon;
            result.EnsembleSize = 1;
            result.ModelName = this.GetType().Name;
            result.NumberOfRules = DecisionTreeBase.GetNumberOfRules(this.obliviousDecisionTree);
            result.QualityRatio = DecisionTreeBase.GetNumberOfAttributes(this.obliviousDecisionTree);
            result.AvgTreeHeight = DecisionTreeBase.GetAvgHeight(this.obliviousDecisionTree);
            result.MaxTreeHeight = DecisionTreeBase.GetHeight(this.obliviousDecisionTree);
        }

        public virtual long Compute(DataRecordInternal record)
        {
            return this.obliviousDecisionTree.Compute(record);
        }
    }
}
