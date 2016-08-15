using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Infovision.Data;
using Infovision.Utils;

namespace Infovision.Datamining.Roughset
{
    public static class DecisionTreeHelper
    {
        public static int CountLeaves(ITreeNode node)
        {
            int count = 0;
            TreeNodeTraversal.TraverseInOrder(node, n => count += (n.IsLeaf) ? 1 : 0);
            return count;
        }

        public static List<AttributeValueVector> GetRulesFromTree(ITreeNode node, DataStore data)
        {
            List<AttributeValueVector> conditions = new List<AttributeValueVector>(CountLeaves(node));
            Action<ITreeNode> addConditions = n =>
            {
                if (n.IsRoot && n.Children == null)
                {
                    conditions.Add(new AttributeValueVector(0));
                }
                else if (n.Children != null && n.Children.First().Key == data.DataStoreInfo.DecisionFieldId)
                {
                    conditions.Add(DecisionTreeHelper.CreateRuleConditionFromNode(n));
                }

            };
            TreeNodeTraversal.TraversePreOrder(node, addConditions);
            return conditions;
        }

        public static decimal CalcMajorityMeasureFromTree(ITreeNode node, DataStore data, decimal[] weights = null)
        {
            List<AttributeValueVector> conditions = DecisionTreeHelper.GetRulesFromTree(node, data);

            if (weights == null)
                weights = data.Weights;

            Dictionary<AttributeValueVector, EquivalenceClass> map 
                = new Dictionary<AttributeValueVector, EquivalenceClass>(conditions.Count);
            for (int i = 0; i < data.NumberOfRecords; i++)
            {
                DataRecordInternal record = data.GetRecordByIndex(i);
                foreach (AttributeValueVector condition in conditions)
                {
                    bool flag = true;

                    for (int j = 0; j < condition.Length; j++)
                    {
                        if (record[condition.Attributes[j]] != condition.Values[j])
                        {
                            flag = false;
                            break;
                        }
                    }

                    if (flag)
                    {
                        EquivalenceClass eq = null;
                        if (!map.TryGetValue(condition, out eq))
                        {
                            eq = new EquivalenceClass(condition.Values, data);
                            map.Add(condition, eq);
                        }
                        eq.AddObject(i, data.GetDecisionValue(i), weights[i]);
                        break;
                    }
                }
            }

            decimal sum = Decimal.Zero;
            foreach (var kvp in map)
                sum += kvp.Value.DecisionWeights.FindMaxValuePair().Value;

            return sum;
        }

        public static bool CheckTreeConverged(ITreeNode node, DataStore data, decimal epsilon, decimal[] weights = null)
        {            
            int[] attributes = data.DataStoreInfo.GetFieldIds(FieldTypes.Standard).ToArray();
            EquivalenceClassCollection eqClassCollection = EquivalenceClassCollection.Create(attributes, data, weights);
            IReduct reduct = new ReductWeights(data, attributes, Decimal.Zero, weights, eqClassCollection);
            decimal MA = new InformationMeasureWeights().Calc(reduct);

            decimal m = DecisionTreeHelper.CalcMajorityMeasureFromTree(node, data, weights);

            if ((Decimal.One - epsilon) * MA <= m)
                return true;
                
            return false;
        }

        public static AttributeValueVector CreateRuleConditionFromNode(ITreeNode node)
        {            
            AttributeValueVector result = new AttributeValueVector(node.Level);
            ITreeNode n = node;
            int size = result.Length - 1;
            while (n.Parent != null)
            {
                result.Set(size--, n.Key, n.Value);
                n = n.Parent;
            }
            return result;
        }
    }
}
