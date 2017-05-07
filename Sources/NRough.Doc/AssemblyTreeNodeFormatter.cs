using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NRough.Doc
{
    public class AssemblyTreeNodeFormatter : IFormatProvider, ICustomFormatter
    {
        internal Dictionary<string, string> EscapeCharTable { get; set; }

        public AssemblyTreeNodeFormatter()
        {
            EscapeCharTable = new Dictionary<string, string>();
        }

        public virtual object GetFormat(Type formatType)
        {
            //return formatType.IsAssignableFrom(typeof(IAssemblyTreeNode)) ? this : null;
            return formatType.GetInterfaces().Contains(typeof(IAssemblyTreeNode)) ? this : null;
        }

        public virtual string Format(string format, object args, IFormatProvider formatProvider)
        {
            IAssemblyTreeNode result = args as IAssemblyTreeNode;
            if (result == null)
                return args.ToString();

            if (String.IsNullOrEmpty(format))
                return result.ToString();

            switch (format)
            {
                case "forest":
                    return "{" + EscapeSpecialCharacters(result.ToString()) + "}";                    

                default:
                    return result.ToString();
            }
        }

        private string EscapeSpecialCharacters(string s)
        {
            var result = new StringBuilder(s);
            foreach (var kvp in EscapeCharTable)
                result.Replace(kvp.Key, kvp.Value);
            return result.ToString();
        }
    }
}
