using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NRough.Core.DataStructures.Tree
{
    public class TreeStringFormatter : IFormatProvider, ICustomFormatter
    {
        public int Indent { get; set; }

        public TreeStringFormatter()
        {
            Indent = 4;
        }

        public virtual object GetFormat(Type formatType)
        {
            return formatType.IsAssignableFrom(typeof(Tree)) ? this : null;
        }

        public virtual string Format(string format, object args, IFormatProvider formatProvider)
        {
            Tree result = args as Tree;
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

        private string NodeToString(ITreeNode node, int currentLevel)
        {
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
