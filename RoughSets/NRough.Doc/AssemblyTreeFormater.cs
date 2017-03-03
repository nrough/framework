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

        private void Build(IAssemblyTreeNode node, int currentLevel, StringBuilder sb)
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
