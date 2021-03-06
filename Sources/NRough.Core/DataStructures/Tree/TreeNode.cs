﻿// 
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

namespace NRough.Core.DataStructures.Tree
{
    public interface ITreeNode
    {
        string Name { get; set; }
        bool IsLeaf { get; }
        bool IsRoot { get; }
        IList<ITreeNode> Children { get; }
        ITreeNode Parent { get; set; }
        void AddChild(ITreeNode node);
    }

    public class TreeNode : ITreeNode
    {
        public string Name { get; set; }
        public IList<ITreeNode> Children { get; set; }
        public ITreeNode Parent { get; set; }
        public bool IsLeaf { get { return Children == null || Children.Count == 0; } }
        public bool IsRoot { get { return Parent == null; } }

        public TreeNode(string name)
        {
            Name = name;
        }

        public void AddChild(ITreeNode node)
        {
            if (Children == null)
                Children = new List<ITreeNode>();
            node.Parent = this;
            Children.Add(node);
        }

        public override string ToString()
        {
            return Name;
        }

    }
}
