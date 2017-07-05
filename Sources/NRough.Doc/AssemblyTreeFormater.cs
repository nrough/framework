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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NRough.Doc
{
    public interface IAssemblyTreeFormatter : IFormatProvider, ICustomFormatter
    {
    }

    public class AssemblyTreeStringFormatter : IAssemblyTreeFormatter
    {
        public int Indent { get; set; }        

        public AssemblyTreeStringFormatter()
        {
            Indent = 4;
        }

        public virtual object GetFormat(Type formatType)
        {
            return formatType.IsAssignableFrom(typeof(IAssemblyTree)) ? this : null;
        }

        public virtual string Format(string format, object args, IFormatProvider formatProvider)
        {
            IAssemblyTree result = args as IAssemblyTree;
            if (result == null)
                return args.ToString();

            if (result.Root == null)
                return result.ToString();

            if (String.IsNullOrEmpty(format))
                return result.ToString();

            StringBuilder sb = new StringBuilder();
            Build(result.Root, 0, sb);
            return sb.ToString();
        }

        private string NodeToString(IAssemblyTreeNode node, int currentLevel)
        {
            return string.Format("{0}{1}", new string(' ', this.Indent * currentLevel), node.ToString());
        }

        private void Build(IAssemblyTreeNode node, int currentLevel, StringBuilder sb)
        {
            sb.AppendLine(NodeToString(node, currentLevel));
            if (node.Children != null)
                foreach (var child in node.Children)
                    Build(child, currentLevel + 1, sb);
        }
    }

    public class LatexForestAssemblyTreeFormatter
        : AssemblyTreeStringFormatter
    {
        public bool IncludeStyle { get; set; }
        public string Header { get; set; }
        public string Footer { get; set; }
        private AssemblyTreeNodeFormatter NodeFormatter { get; set; }
        
        public LatexForestAssemblyTreeFormatter()
            : base()
        {
            IncludeStyle = false;
            NodeFormatter = new AssemblyTreeNodeFormatter();
            NodeFormatter.EscapeCharTable = EscapeCharTable();
            Header = DefaultHeader();
            Footer = DefaultFooter();
        }

        public LatexForestAssemblyTreeFormatter(bool includeStyle)
            : this()
        {
            IncludeStyle = includeStyle;            
        }

        public override object GetFormat(Type formatType)
        {
            return formatType.IsAssignableFrom(typeof(AssemblyTree)) ? this : null;
        }

        public override string Format(string format, object args, IFormatProvider formatProvider)
        {
            AssemblyTree result = args as AssemblyTree;
            if (result == null)
                return args.ToString();

            if (result.Root == null)
                return String.Empty;            

            StringBuilder sb = new StringBuilder();
            sb.AppendLine(Header);
            Build(result.Root, 0, sb);
            sb.AppendLine(Footer);
            return sb.ToString();
        }        

        private void Build(IAssemblyTreeNode node, int currentLevel, StringBuilder sb)
        {
            if (node == null) throw new ArgumentNullException("node");
            if (sb == null) throw new ArgumentNullException("sb");            

            if (node != null 
                && (node.NodeType == AssemblyTreeNodeType.Folder 
                    || node.NodeType == AssemblyTreeNodeType.Namespace))
            {
                if(node.IsLeaf)
                    return;

                var children = AssemblyTreeNode.GetEnumerator(node).ToIEnumerable<IAssemblyTreeNode>();
                if (!children
                    .Any(n => n.NodeType != AssemblyTreeNodeType.Folder 
                        && n.NodeType != AssemblyTreeNodeType.Namespace))
                    return;
            }
            
            sb.Append(new string(' ', this.Indent * currentLevel));
            sb.Append("[ ");

            var anode = node as AssemblyTreeNode;
            if (anode != null)
                sb.Append(anode.ToString("forest", NodeFormatter));
            else
                NodeFormatter.Format("forest", node, null);

            if (IncludeStyle)
            {
                if(node.IsRoot)
                    sb.Append(", root");
                else
                    switch (node.NodeType)
                    {
                        case AssemblyTreeNodeType.Assembly: sb.Append(", assembly"); break;
                        case AssemblyTreeNodeType.Class: sb.Append(", class"); break;
                        case AssemblyTreeNodeType.Enum: sb.Append(", enum"); break;
                        case AssemblyTreeNodeType.Interface: sb.Append(", interface"); break;
                        case AssemblyTreeNodeType.Delegate: sb.Append(", delegate"); break;
                        case AssemblyTreeNodeType.Namespace: sb.Append(", namespace"); break;
                        case AssemblyTreeNodeType.Folder: sb.Append(", fldr"); break;

                        default:
                            //do nothing
                            break;
                    }
            }

            if (node.Children != null)
            {
                sb.AppendLine();
                for (int i = 0; i < node.Children.Count; i++)
                    Build(node.Children.ElementAt(i), currentLevel + 1, sb);
                sb.Append(new string(' ', this.Indent * currentLevel));
            }
            else
            {
                sb.Append(" ");
            }

            sb.Append("]");
            sb.AppendLine();
        }

        private Dictionary<string, string> EscapeCharTable()
        {
            var result  = new Dictionary<string, string>();

            result.Add("_", "\\_");
            result.Add("^", "\\^");
            result.Add("<", "$\\langle$");
            result.Add(">", "$\\rangle$");

            return result;
        }

        private string DefaultHeader()
        {
            return @"\begin{forest}
for tree = {folder,grow'=0,fit=band,font=\scriptsize,inner ysep=0.5pt}";
        }

        private string DefaultFooter()
        {
            return @"\end{forest}";
        }
    }
}
