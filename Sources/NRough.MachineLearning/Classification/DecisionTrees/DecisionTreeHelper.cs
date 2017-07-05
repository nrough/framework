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

using System;
using System.Linq;
using NRough.Data;
using NRough.Core;

namespace NRough.MachineLearning.Classification.DecisionTrees
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
