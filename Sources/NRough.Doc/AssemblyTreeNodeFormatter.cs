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
