//
// NRough Framework
// http://www.nrough.net
//
// Copyright © Sebastian Widz, 2017
// authors at nrough.net

//This file is part of NRough.

//Foobar is free software; you can redistribute it and/or modify
//it under the terms of the GNU General Public License as published by
//the Free Software Foundation; either version 3 of the License, or
//(at your option) any later version.
//
//Foobar is distributed in the hope that it will be useful,
//but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
//GNU General Public License for more details.
//
//You should have received a copy of the GNU General Public License
//along with Foobar; if not, write to the Free Software
//Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA  02110-1301  USA
//
using NRough.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NRough.MachineLearning.Classification.DecisionLookup
{
    public class DecisionLookup : ClassificationModelBase, ILearner, IClassificationModel
    {
        private EquivalenceClassCollection eqClassCollection;        

        public DecisionLookup()
            : base()
        {            
        }

        public DecisionLookup(string modelName)
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

            return Classifier.Default.Classify(this, selectedData);
        }

        public override void SetClassificationResultParameters(ClassificationResult result)
        {
            base.SetClassificationResultParameters(result);

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