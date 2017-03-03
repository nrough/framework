using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NRough.Doc
{
    public interface IAssemblyTreeNode
    {
        string Name { get; set; }
        bool IsLeaf { get; }
        bool IsRoot { get; }
        IList<IAssemblyTreeNode> Children { get; }
        IAssemblyTreeNode Parent { get; set; }
        void AddChild(IAssemblyTreeNode node);

        AssemblyTreeNodeType NodeType { get; }
    }

    public abstract class AssemblyTreeNode : IAssemblyTreeNode
    {
        public string Name { get; set; }
        public IList<IAssemblyTreeNode> Children { get; set; }
        public IAssemblyTreeNode Parent { get; set; }
        public bool IsLeaf { get { return Children == null || Children.Count == 0; } }
        public bool IsRoot { get { return Parent == null; } }

        public abstract AssemblyTreeNodeType NodeType { get; }

        public AssemblyTreeNode(string name)        
        {
            Name = name;
        }

        public void AddChild(IAssemblyTreeNode node)
        {
            if (Children == null)
                Children = new List<IAssemblyTreeNode>();
            node.Parent = this;
            Children.Add(node);
        }

        public override string ToString()
        {
            return Name;
        }
    }

    public class AssemblyAssemblyTreeNode : AssemblyTreeNode
    {
        public override AssemblyTreeNodeType NodeType
        {
            get { return AssemblyTreeNodeType.Assembly; }
        }

        public Assembly Assembly { get; set; }

        public AssemblyAssemblyTreeNode(string name, Assembly assembly)
            : base(name)
        {
            Assembly = assembly;
        }
    }

    public abstract class AssemblyTypeTreeNode : AssemblyTreeNode
    {
        public Type Type { get; set; }

        public AssemblyTypeTreeNode(string name, Type type)
            : base(name)
        {
            if (name == "OnTrainingDataSubmission")
                Debugger.Break();

            Type = type;
        }
    }

    public class AssemblyClassTreeNode : AssemblyTypeTreeNode
    {
        public override AssemblyTreeNodeType NodeType
        {
            get { return AssemblyTreeNodeType.Class; }
        }

        public AssemblyClassTreeNode(string name, Type type)
            : base(name, type)
        {
        }

        public override string ToString()
        {
            if(Type.IsGenericType)
                return "{$" + base.ToString() + "$}";
            return base.ToString();
        }
    }

    public class AssemblyInterfaceTreeNode : AssemblyTypeTreeNode
    {
        public override AssemblyTreeNodeType NodeType
        {
            get { return AssemblyTreeNodeType.Interface; }
        }

        public AssemblyInterfaceTreeNode(string name, Type type)
            : base(name, type)
        {
        }
    }

    public class AssemblyDelegateTreeNode : AssemblyTypeTreeNode
    {
        public override AssemblyTreeNodeType NodeType
        {
            get { return AssemblyTreeNodeType.Delegate; }
        }

        public AssemblyDelegateTreeNode(string name, Type type)
            : base(name, type)
        {
        }
    }

    public class AssemblyEnumTreeNode : AssemblyTypeTreeNode
    {
        public override AssemblyTreeNodeType NodeType
        {
            get { return AssemblyTreeNodeType.Enum; }
        }

        public AssemblyEnumTreeNode(string name, Type type)
            : base(name, type)
        {
        }
    }

    public class AssemblyFolderTreeNode : AssemblyTreeNode
    {
        public override AssemblyTreeNodeType NodeType
        {
            get { return AssemblyTreeNodeType.Folder; }
        }

        public AssemblyFolderTreeNode(string name)
            : base(name)
        {
        }
    }
}
