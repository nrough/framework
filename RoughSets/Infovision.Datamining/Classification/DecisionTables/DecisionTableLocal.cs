﻿using Infovision.Data;
using Infovision.MachineLearning.Classification.DecisionTrees;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infovision.MachineLearning.Classification.DecisionTables
{
    public class DecisionTableLocal : ModelBase, ILearner, IPredictionModel
    {
        private ObliviousDecisionTree obliviousDecisionTree = null;
        private DecisionDistribution aprioriDistribution;

        public long? DefaultOutput { get; set; }        
        public bool RankedAttributes { get; set; }
        public IDecisionTree ObiliviousTree { get { return this.obliviousDecisionTree; } }        

        public virtual ClassificationResult Learn(DataStore data, int[] attributes)
        {
            this.aprioriDistribution = EquivalenceClassCollection.Create(new int[] { }, data, data.Weights).DecisionDistribution;
            this.DefaultOutput = this.aprioriDistribution.Output;

            this.obliviousDecisionTree = new ObliviousDecisionTree();
            this.obliviousDecisionTree.ImpurityFunction = ImpurityFunctions.Entropy;
            this.obliviousDecisionTree.ImpurityNormalize = ImpurityFunctions.SplitInformationNormalize;
            this.obliviousDecisionTree.UseLocalOutput = true;
            this.obliviousDecisionTree.RankedAttributes = this.RankedAttributes;
            this.obliviousDecisionTree.Learn(data, attributes);

            return Classifier.DefaultClassifer.Classify(this, data);
        }

        public virtual void SetClassificationResultParameters(ClassificationResult result)
        {            
            result.EnsembleSize = 1;
            result.ModelName = this.GetType().Name;
            result.NumberOfRules = DecisionTreeBase.GetNumberOfRules(this.obliviousDecisionTree);
            result.AvgNumberOfAttributes = DecisionTreeBase.GetNumberOfAttributes(this.obliviousDecisionTree);
            result.AvgTreeHeight = DecisionTreeBase.GetAvgHeight(this.obliviousDecisionTree);
            result.MaxTreeHeight = DecisionTreeBase.GetHeight(this.obliviousDecisionTree);
        }

        public virtual long Compute(DataRecordInternal record)
        {
            return this.obliviousDecisionTree.Compute(record);
        }
    }
}