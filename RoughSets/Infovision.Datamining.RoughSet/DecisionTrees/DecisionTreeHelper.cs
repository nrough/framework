using System;
using System.Linq;
using Infovision.Data;
using Infovision.Utils;

namespace Infovision.Datamining.Roughset.DecisionTrees
{
    public static class DecisionTreeHelper
    {
        public static int CountLeaves(ITreeNode node)
        {
            int count = 0;
            TreeNodeTraversal.TraversePostOrder(node, n => count += (n.IsLeaf) ? 1 : 0);
            return count;
        }

        public static AttributeValueVector[] GetRulesFromTree(ITreeNode node, DataStore data)
        {
            int count = CountLeaves(node);

            AttributeValueVector[] conditions = count > 1 
                ? new AttributeValueVector[count] 
                : new AttributeValueVector[1];

            int i = 0;
            Action<ITreeNode> addConditions = n =>
            {
                if (n.IsRoot && n.Children == null)
                {
                    conditions[i++] = new AttributeValueVector(0);
                }
                else if (n.Children == null && n.Key != data.DataStoreInfo.DecisionFieldId)
                {
                    conditions[i++] = DecisionTreeHelper.CreateRuleConditionFromNode(n);
                }
                else if (n.Children != null && n.Children.First().Key == data.DataStoreInfo.DecisionFieldId)
                {
                    conditions[i++] = DecisionTreeHelper.CreateRuleConditionFromNode(n);
                }
                
            };
            TreeNodeTraversal.TraversePreOrder(node, addConditions);
            return conditions;
        }

        public static decimal CalcMajorityMeasureFromTree(ITreeNode node, DataStore data, decimal[] weights = null)
        {
            if (weights == null)
                weights = data.Weights;

            AttributeValueVector[] conditions = DecisionTreeHelper.GetRulesFromTree(node, data);
            EquivalenceClass[] map = new EquivalenceClass[conditions.Length];
            int[] fieldIndexLookup = data.DataStoreInfo.GetFieldIndexLookupTable();

            for (int i = 0; i < data.NumberOfRecords; i++)
            {
                for(int j = 0; j < conditions.Length; j++)
                {
                    bool flag = true;
                    for (int k = 0; k < conditions[j].Length; k++)
                    {
                        if(conditions[j].Values[k] != data.GetFieldIndexValue(i, fieldIndexLookup[conditions[j].Attributes[k]]))
                        {
                            flag = false;
                            break;
                        }
                    }

                    if (flag)
                    {
                        if (map[j] == null)
                            map[j] = new EquivalenceClass(conditions[j].Values, data);
                        map[j].AddObject(i, data.GetDecisionValue(i), weights[i]);
                        break;
                    }
                }
            }

            decimal sum = Decimal.Zero;
            foreach (var eq in map)
                sum += eq.DecisionWeights.FindMaxValuePair().Value;
            return sum;
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
