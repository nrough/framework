﻿using Raccoon.Data;
using Raccoon.MachineLearning.Classification.DecisionTrees;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Raccoon.MachineLearning.Classification.DecisionTables
{
    public class DecisionTableLocal : ClassificationModelBase, ILearner, IPredictionModel
    {
        private ObliviousDecisionTree obliviousDecisionTree = null;
        private DecisionDistribution aprioriDistribution;
     
        public bool RankedAttributes { get; set; }
        public IDecisionTree ObiliviousTree { get { return this.obliviousDecisionTree; } }        

        public virtual ClassificationResult Learn(DataStore data, int[] attributes)
        {
            this.aprioriDistribution = EquivalenceClassCollection.Create(new int[] { }, data, data.Weights).DecisionDistribution;
            this.DefaultOutput = this.aprioriDistribution.Output;

            this.obliviousDecisionTree = new ObliviousDecisionTree();
            this.obliviousDecisionTree.ImpurityFunction = ImpurityMeasure.Entropy;
            this.obliviousDecisionTree.ImpurityNormalize = ImpurityMeasure.SplitInformationNormalize;
            this.obliviousDecisionTree.UseLocalOutput = true;
            this.obliviousDecisionTree.RankedAttributes = this.RankedAttributes;
            this.obliviousDecisionTree.Learn(data, attributes);

            return Classifier.DefaultClassifer.Classify(this, data);
        }

        public virtual void SetClassificationResultParameters(ClassificationResult result)
        {            
            result.EnsembleSize = 1;
            result.ModelName = this.GetType().Name;
            result.NumberOfRules = DecisionTreeMetric.GetNumberOfRules(this.obliviousDecisionTree);
            result.AvgNumberOfAttributes = DecisionTreeMetric.GetNumberOfAttributes(this.obliviousDecisionTree);
            result.AvgTreeHeight = DecisionTreeMetric.GetAvgHeight(this.obliviousDecisionTree);
            result.MaxTreeHeight = DecisionTreeMetric.GetHeight(this.obliviousDecisionTree);
        }

        public virtual long Compute(DataRecordInternal record)
        {
            return this.obliviousDecisionTree.Compute(record);
        }
    }
}
