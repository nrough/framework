using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NRough.Doc
{
    public interface IAssemblyTree
    {
        IAssemblyTreeNode Root { get; }
    }

    public class AssemblyTree : IAssemblyTree
    {
        public string Name { get; set; }
        public IAssemblyTreeNode Root { get; set; }

        public AssemblyTree(string name)
            : base()
        {
            Name = name;
            Root = new AssemblyFolderTreeNode(name);
        }

        public AssemblyTree()
            : this(String.Empty) { }
        
        public void Build(IAssemblyTreeBuilder builder)
        {
            Root = builder.Build().Root;
            if (!String.IsNullOrEmpty(Name))
                Root.Name = Name;
        }

        public override string ToString()
        {
            return this.ToString("G", new AssemblyTreeStringFormatter());
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
