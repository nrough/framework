using NRough.Core.DataStructures.Tree;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NRough.Doc
{
    public interface IAssemblyTreeFormatter
    {
    }

    public class AssemblyTreeStringFormatter : TreeStringFormatter
    {

    }

    public class LatexForestAssemblyTreeFormatter
        : AssemblyTreeStringFormatter
    {
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

            //if (String.IsNullOrEmpty(format))
            //    return result.ToString();

            StringBuilder sb = new StringBuilder();
            Build(result.Root, 0, sb);
            return sb.ToString();
        }

        private void Build(ITreeNode node, int currentLevel, StringBuilder sb)
        {
            if (node == null) throw new ArgumentNullException("node");
            if (sb == null) throw new ArgumentNullException("sb");

            var anode = node as IAssemblyTreeNode;
            if (anode != null
                && anode.IsLeaf
                && anode.NodeType == AssemblyTreeNodeType.Folder)
            {
                return;
            }

            sb.Append(new string(' ', this.Indent * currentLevel));
            sb.Append("[ ");
            sb.Append(node.ToString());

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
    }
}
