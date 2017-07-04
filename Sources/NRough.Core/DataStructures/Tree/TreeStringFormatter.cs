//
// NRough Framework
// http://www.nrough.net
//
// Copyright © Sebastian Widz, 2017
// authors at nrough.net

//This file is part of NRough.

//Foobar is free software; you can redistribute it and/or modify
//it under the terms of the GNU General Public License as published by
//the Free Software Foundation; either version 3 of the License, or
//(at your option) any later version.
//
//Foobar is distributed in the hope that it will be useful,
//but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
//GNU General Public License for more details.
//
//You should have received a copy of the GNU General Public License
//along with Foobar; if not, write to the Free Software
//Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA  02110-1301  USA
//
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
