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
using NRough.MachineLearning.Classification.DecisionTrees;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NRough.MachineLearning.Classification.DecisionLookup
{
    public class DecisionLookupLocal : ClassificationModelBase, ILearner, IClassificationModel
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

            return Classifier.Default.Classify(this, data);
        }

        public override void SetClassificationResultParameters(ClassificationResult result)
        {
            base.SetClassificationResultParameters(result);

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
