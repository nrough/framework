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

namespace NRough.MachineLearning.Classification.DecisionTrees
{
    public class DecisionTreeFormatter
    {
        public static DecisionTreeFormatter Construct(IDecisionTree decisionTree)
        {
            DecisionTreeBase treeBase = decisionTree as DecisionTreeBase;
            if(treeBase != null)
                return DecisionTreeFormatter.Construct(treeBase.Root, treeBase.TrainingData.DataStoreInfo, 4);

            return DecisionTreeFormatter.Construct(decisionTree.Root, null, 4);
        }

        public static DecisionTreeFormatter Construct(IDecisionTreeNode node, DataStoreInfo data)
        {
            return DecisionTreeFormatter.Construct(node, data, 4);
        }

        public static DecisionTreeFormatter Construct(IDecisionTreeNode node, DataStoreInfo data, int indent)
        {
            DecisionTreeFormatter treeFormatter = new DecisionTreeFormatter();
            treeFormatter.Root = node;
            treeFormatter.DataStoreInfo = data;
            treeFormatter.Indent = indent;
            return treeFormatter;
        }

        private DecisionTreeFormatter()
        {
            this.Indent = 4;
        }

        public IDecisionTreeNode Root { get; set; }
        public DataStoreInfo DataStoreInfo { get; set; }
        public int Indent { get; set; }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            Build(this.Root, 0, sb);
            return sb.ToString();
        }

        private string NodeToString(IDecisionTreeNode node, int currentLevel)
        {            
            if (node is DecisionTreeNode)                
                return string.Format("{0}{1}", new string(' ', this.Indent * currentLevel), ((DecisionTreeNode)node).ToString(this.DataStoreInfo));
            return string.Format("{0}{1}", new string(' ', this.Indent * currentLevel), node.ToString());
        }

        private void Build(IDecisionTreeNode node, int currentLevel, StringBuilder sb)
        {
            sb.AppendLine(NodeToString(node, currentLevel));
            if (node.Children != null)
                foreach (var child in node.Children)
                    Build(child, currentLevel + 1, sb);
        }
    }
}
