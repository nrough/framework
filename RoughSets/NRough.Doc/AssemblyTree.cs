using NRough.Core.DataStructures.Tree;
using NRough.Doc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NRough.Doc
{
    public class AssemblyTree : Tree
    {
        public AssemblyTree(string name)
            : base()
        {
            this.Root = new AssemblyFolderTreeNode(name);
        }

        public AssemblyTree()
            : this(String.Empty) { }
        
        public void Build(IAssemblyTreeBuilder builder)
        {
            Root = builder.Build().Root;
        }

        public override string ToString()
        {
            return this.ToString("G", new TreeStringFormatter());
        }

        public virtual string ToString(string format, IFormatProvider formatProvider)
        {
            if (formatProvider != null)
            {
                ICustomFormatter formatter = formatProvider.GetFormat(this.GetType()) as ICustomFormatter;
                if (formatter != null)
                    return formatter.Format(format, this, formatProvider);
            }

            if (String.IsNullOrEmpty(format))
                format = "G";

            switch (format)
            {
                case "G":
                default:
                    return this.ToString();
            }
        }
    }
}
