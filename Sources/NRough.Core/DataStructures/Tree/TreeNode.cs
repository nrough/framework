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
