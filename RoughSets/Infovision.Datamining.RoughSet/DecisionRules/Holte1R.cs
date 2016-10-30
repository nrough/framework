using Infovision.Data;
using Infovision.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infovision.Datamining.Roughset.DecisionRules
{
    [Serializable]
    public class Holte1R : ILearner, IPredictionModel
    {
        private DecisionListCollection decisionLists;

        public double Epsilon { get; set; }
        
        public ClassificationResult Learn(DataStore data, int[] attributes)
        {
            /* For each attribute a, form a rule as follows:
             *  For each value v from the domain of a
             *      Select the set of instances where a has value v
             *      Let c be the most frequent class in that set
             *      Add the following clause to the rule for a
             *          if a has value v then the class is c
             * Use the rule with the highest classification accuracy
             */

            decisionLists = new DecisionListCollection(attributes.Length);

            for (int i = 0; i < attributes.Length; i++)
            {
                DataFieldInfo attributeInfo = data.DataStoreInfo.GetFieldInfo(attributes[i]);
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
                    //TODO
                }
            }
            decisionLists.Sort();

            return Classifier.DefaultClassifer.Classify(this, data);              
        }

        public void SetClassificationResultParameters(ClassificationResult result)
        {
            result.Epsilon = this.Epsilon;
            result.EnsembleSize = 1;
            result.ModelName = this.GetType().Name;
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
    }
}
