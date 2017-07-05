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
