﻿using Infovision.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infovision.Datamining.Roughset.DecisionTrees
{
    public class DecisionTreeFormatter
    {
        public static DecisionTreeFormatter Construct(ITreeNode node, DataStore data, int indent)
        {
            DecisionTreeFormatter treeFormatter = new DecisionTreeFormatter();
            treeFormatter.Root = node;
            treeFormatter.Data = data;
            treeFormatter.Indent = indent;
            return treeFormatter;
        }

        private DecisionTreeFormatter()
        {
            this.Indent = 2;
        }

        public ITreeNode Root { get; set; }
        public DataStore Data { get; set; }
        public int Indent { get; set; }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            Build(this.Root, 0, sb);
            return sb.ToString();
        }

        private string NodeToString(ITreeNode node, int currentLevel)
        {
            if (node is DecisionTreeNode)
                return string.Format("{0}{1}", new string(' ', this.Indent * currentLevel), ((DecisionTreeNode)node).ToString(this.Data.DataStoreInfo));
            return string.Format("{0}{1}", new string(' ', this.Indent * currentLevel), node.ToString());
        }

        private void Build(ITreeNode node, int currentLevel, StringBuilder sb)
        {
            sb.AppendLine(NodeToString(node, currentLevel));
            if (node.Children != null)
                foreach (var child in node.Children)
                    Build(child, currentLevel + 1, sb);
        }
    }
}