// 
// NRough Framework
// http://www.nrough.net
// 
// Copyright (c) Sebastian Widz, 2017
// authors at nrough.net
// 
// This file is part of NRough Framework.
// 
// NRough Framework is free software; you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation; either version 3 of the License, or
// (at your option) any later version.
// 
// NRough Framework is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with NRough Framework; if not, write to the Free Software
// Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA  02110-1301  USA
//

using NRough.Data;
using NRough.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NRough.Core.CollectionExtensions;

namespace NRough.MachineLearning.Classification.DecisionRules
{
    /// <summary>
    /// For each attribute a, form a rule as follows: <br />
    ///     For each value v from the domain of a<br />
    ///         Select the set of instances where a has value v<br />
    ///         Let c be the most frequent class in that set<br />
    ///         Add the following clause to the rule for a<br />
    ///             if a has value v then the class is c<br />
    ///         Use the rule with the highest classification accuracy<br />
    /// </summary>
    [Serializable]
    public class Holte1R : ModelBase, ILearner, IClassificationModel
    {
        #region TODO

        //TODO Missing values (Training & Testing)
        //TODO Refactor Learn Method
        //TODO Optimize DiscretizationInfo (Output, OutputWeight, Check)

        #endregion

        private DecisionListCollection decisionLists;
        private DecisionDistribution aprioriDecisionDistribution;

        public long? DefaultOutput { get; set; }        
        public double? Small { get; set; }
        public int[] Attributes { get; set; }

        private class DiscretizeInfo
        {
            public long LowerBound { get; set; }
            public long UpperBound { get; set; }
            public Dictionary<long, double> OutputDistribution { get; set; }
            public long Output { get { return this.OutputDistribution.FindMaxValueKey(); } }            
            public double OutputWeight { get { return this.OutputDistribution.FindMaxValuePair().Value; } }
            public DiscretizeInfo(ICollection<long> outputs)
            {
                this.OutputDistribution = new Dictionary<long, double>(outputs.Count);
                foreach (long val in outputs)
                    this.OutputDistribution[val] = 0.0;
            }
            public bool Check(double min)
            {
                foreach (var element in this.OutputDistribution)
                    if (element.Value > min)
                        return true;
                return false;
            }
            public void Merge(DiscretizeInfo info)
            {
                this.LowerBound = this.LowerBound > info.LowerBound ? info.LowerBound : this.LowerBound;
                this.UpperBound = this.UpperBound < info.UpperBound ? info.UpperBound : this.UpperBound;
                foreach (var kvp in info.OutputDistribution)
                    this.OutputDistribution[kvp.Key] += kvp.Value;
            }
        }
        
        public ClassificationResult Learn(DataStore data, int[] attributes)
        {            
            this.aprioriDecisionDistribution = EquivalenceClassCollection.Create(new int[] { }, data).DecisionDistribution;

            this.decisionLists = new DecisionListCollection(attributes.Length);
            this.decisionLists.DefaultDecision = this.aprioriDecisionDistribution.Output;

            for (int i = 0; i < attributes.Length; i++)
            {
                AttributeInfo attributeInfo = data.DataStoreInfo.GetFieldInfo(attributes[i]);

                if (attributeInfo.IsSymbolic)
                {
                    EquivalenceClassCollection eqClasses = EquivalenceClassCollection.Create(new int[] { attributes[i] }, data);
                    DecisionList holteRule = new DecisionList(eqClasses.Count);
                    double accuracy = 0;
                    foreach (var eqClass in eqClasses)
                    {
                        DecisionRule rule = new DecisionRule(
                            new DecisionRuleCondition(attributes[i], ComparisonType.EqualTo, eqClass.Instance[0]),
                            eqClass.DecisionDistribution);
                        holteRule.Add(rule);
                        accuracy += eqClass.DecisionDistribution[rule.Output];
                    }
                    holteRule.Accuracy = accuracy;

                    decisionLists.Add(holteRule);
                }
                else if (attributeInfo.IsNumeric)
                {

                    double small = (this.Small != null)
                                 ? (double) this.Small 
                                 : (data.NumberOfRecords < 50) 
                                 ? (3.0 / (double)data.NumberOfRecords) 
                                 : (6.0 / (double)data.NumberOfRecords);

                    int[] indices = Enumerable.Range(0, data.NumberOfRecords).ToArray();
                    long[] values = data.GetColumnInternal(attributes[i]);
                    Array.Sort(values, indices);
                    long[] outputs = data.GetFieldValue(indices, data.DataStoreInfo.DecisionFieldId);

                    List<DiscretizeInfo> cuts = new List<DiscretizeInfo>(attributeInfo.NumberOfValues);
                    DiscretizeInfo cut = new DiscretizeInfo(data.DataStoreInfo.DecisionInfo.InternalValues());
                    cuts.Add(cut);
                    cut.LowerBound = attributeInfo.MinValue;
                    for (int j = 0; j < indices.Length - 1; j++)
                    {
                        cut.OutputDistribution[outputs[j]] += data.GetWeight(indices[j]);
                        cut.UpperBound = (values[j] + values[j + 1]) / 2;
                        if (cut.Check(small))
                        {
                            long majorDecision = cut.Output;
                            int k = j + 1;
                            while (k < indices.Length - 1 && majorDecision == outputs[k])
                            {                                
                                cut.OutputDistribution[outputs[k]] += data.GetWeight(indices[k]);
                                cut.UpperBound = (values[k] + values[k + 1]) / 2;
                                k++;
                            }

                            int m = k;
                            while (m < indices.Length - 1 && values[k] == values[m])
                            {                                
                                cut.OutputDistribution[outputs[m]] += data.GetWeight(indices[m]);
                                cut.UpperBound = (values[m] + values[m + 1]) / 2;
                                m++;
                            }

                            j = m - 1;

                            long prevUpperBound = cut.UpperBound;
                            cut = new DiscretizeInfo(data.DataStoreInfo.DecisionInfo.InternalValues());
                            cut.LowerBound = prevUpperBound;
                            cuts.Add(cut);
                        }
                    }

                    cut.UpperBound = values[values.Length - 1];
                    cut.OutputDistribution[outputs[values.Length - 1]] += data.GetWeight(indices[values.Length - 1]);

                    //Merge
                    for (int j = 0; j < cuts.Count - 1;)
                    {
                        if (cuts.ElementAt(j).Output == cuts.ElementAt(j + 1).Output)
                        {                            
                            cuts.ElementAt(j).Merge(cuts.ElementAt(j + 1));
                            cuts.RemoveAt(j + 1);
                        }
                        else
                            j++;
                    }

                    double accuracy = 0;
                    DecisionList holteRule = new DecisionList(cuts.Count);
                    foreach (var range in cuts)
                    {
                        DecisionRule rule = new DecisionRule(
                            new DecisionRuleCondition(attributes[i], ComparisonType.LessThanOrEqualTo, range.UpperBound),
                            new DecisionDistribution(range.OutputDistribution));
                        holteRule.Add(rule);
                        accuracy += range.OutputWeight;
                    }
                    holteRule.Accuracy = accuracy;
                    decisionLists.Add(holteRule);
                }
            }

            decisionLists.Shuffle();
            decisionLists.Sort();

            return Classifier.Default.Classify(this, data);              
        }

        public void SetClassificationResultParameters(ClassificationResult result)
        {            
            result.EnsembleSize = 1;
            result.ModelName = this.GetType().Name;

            result.NumberOfRules = 1.0;
            result.AvgNumberOfAttributes = 1.0;
        }

        public long Compute(DataRecordInternal record)
        {
            foreach (var decisionList in this.decisionLists)
            {
                long decision = decisionList.Compute(record);
                if (decision != -1)
                    return decision;
            }

            return -1;
        }

        public double ComputeUpperBound(DataStore testData)
        {
            return 0.0;
        }

        public DecisionList GetRule()
        {
            return this.decisionLists.FirstOrDefault();
        }

        public IEnumerable<DecisionList> GetRules()
        {
            return this.decisionLists;
        }
    }
}
