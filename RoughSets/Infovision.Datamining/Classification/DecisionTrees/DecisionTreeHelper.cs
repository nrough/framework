using System;
using System.Linq;
using Infovision.Data;
using Infovision.Core;

namespace Infovision.MachineLearning.Classification.DecisionTrees
{
    public static class DecisionTreeHelper
    {
        public static int CountLeaves(IDecisionTreeNode node)
        {
            int count = 0;
            TreeNodeTraversal.TraversePostOrder(node, n => count += (n.IsLeaf) ? 1 : 0);
            return count;
        }

        public static AttributeValueVector[] GetRulesFromTree(IDecisionTreeNode node, DataStore data)
        {
            int count = CountLeaves(node);

            AttributeValueVector[] conditions = count > 1 
                ? new AttributeValueVector[count] 
                : new AttributeValueVector[1];

            int i = 0;
            Action<IDecisionTreeNode> addConditions = n =>
            {
                if (n.IsRoot && n.Children == null)
                {
                    conditions[i++] = new AttributeValueVector(0);
                }
                else if (n.Children == null && n.Attribute != data.DataStoreInfo.DecisionFieldId)
                {
                    conditions[i++] = DecisionTreeHelper.CreateRuleConditionFromNode(n);
                }
                else if (n.Children != null && n.Children.First().Attribute == data.DataStoreInfo.DecisionFieldId)
                {
                    conditions[i++] = DecisionTreeHelper.CreateRuleConditionFromNode(n);
                }
                
            };
            TreeNodeTraversal.TraversePreOrder(node, addConditions);
            return conditions;
        }

        public static AttributeValueVector CreateRuleConditionFromNode(IDecisionTreeNode node)
        {            
            AttributeValueVector result = new AttributeValueVector(node.Level);
            IDecisionTreeNode n = node;
            int size = result.Length - 1;
            while (n.Parent != null)
            {
                result.Set(size--, n.Attribute, n.Value);
                n = n.Parent;
            }
            return result;
        }
    }
}
