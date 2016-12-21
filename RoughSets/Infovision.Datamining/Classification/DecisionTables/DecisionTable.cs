﻿using Infovision.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infovision.MachineLearning.Classification.DecisionTables
{
    public class DecisionTable : ClassificationModelBase, ILearner, IPredictionModel
    {
        private EquivalenceClassCollection eqClassCollection;        

        public DecisionTable()
            : base()
        {            
        }

        public DecisionTable(string modelName)
            : base(modelName)
        {            
        }

        public virtual ClassificationResult Learn(DataStore data, int[] attributes)
        {
            DataStore selectedData = this.OnTrainingDataSubmission != null 
                                   ? this.OnTrainingDataSubmission(this, attributes, data) 
                                   : data;

            int[] selectedAttributes = this.OnInputAttributeSubmission != null 
                                     ? this.OnInputAttributeSubmission(this, attributes, selectedData) 
                                     : attributes;

            this.eqClassCollection = EquivalenceClassCollection.Create(selectedAttributes, selectedData);

            return Classifier.DefaultClassifer.Classify(this, selectedData);
        }

        public virtual void SetClassificationResultParameters(ClassificationResult result)
        {            
            result.EnsembleSize = 1;
            result.ModelName = this.ModelName;

            if (this.eqClassCollection != null)
            {
                result.NumberOfRules = this.eqClassCollection.Count;
                result.AvgNumberOfAttributes = this.eqClassCollection.Attributes.Length;
                result.MaxTreeHeight = this.eqClassCollection.Attributes.Length;
                result.AvgTreeHeight = this.eqClassCollection.Attributes.Length;
            }
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