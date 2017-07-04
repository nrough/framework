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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NRough.MachineLearning.Classification.DecisionTrees
{
    public static class DecisionTreeMetric
    {
        public static int GetNumberOfRules(IDecisionTree tree)
        {
            int count = 0;
            TreeNodeTraversal.TraversePostOrder(tree.Root, n =>
            {
                if (n.IsLeaf)
                    count++;
            });
            return count;
        }

        public static int GetHeight(IDecisionTree tree)
        {
            int maxHeight = 0;
            TreeNodeTraversal.TraversePostOrder(tree.Root, n =>
            {
                if (n.IsLeaf && n.Level > maxHeight)
                    maxHeight = n.Level;
            });
            return maxHeight;
        }

        public static double GetAvgHeight(IDecisionTree tree)
        {
            double sumHeight = 0.0;
            double count = 0.0;
            TreeNodeTraversal.TraversePostOrder(tree.Root, n =>
            {
                if (n.IsLeaf)
                {
                    sumHeight += n.Level;
                    count++;
                }
            });

            return count > 0 ? sumHeight / count : 0.0;
        }

        public static int GetNumberOfAttributes(IDecisionTree tree)
        {
            return (tree.Root != null) && (tree.Root is DecisionTreeNode)
                ? ((DecisionTreeNode)tree.Root).GetChildUniqueKeys().Count
                : 0;
        }        
    }
}
