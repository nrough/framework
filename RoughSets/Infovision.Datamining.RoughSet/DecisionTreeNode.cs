using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infovision.Datamining.Roughset
{
    public interface ITreeNode
    {
        IReadOnlyList<ITreeNode> Children { get; }
        int Key { get; }
        long Value { get; }
        bool IsLeaf { get; }
    }

    public class DecisionTreeNode : ITreeNode
    {
        private List<DecisionTreeNode> children;


        public DecisionTreeNode(int key, long value)
            : this(key)
        {            
            this.Value = value;
        }

        public DecisionTreeNode(int key)
            : this()
        {
            this.Key = key;
        }

        private DecisionTreeNode()
        {
            this.Value = Int64.MinValue;
        }

        public IReadOnlyList<ITreeNode> Children
        {
            get
            {
                return (IReadOnlyList<ITreeNode>)this.children;
            }
        }
        
        public bool IsLeaf 
        { 
            get 
            {
                return this.children == null || this.children.Count == 0;
            }
        }

        public long Value
        {
            get;
            private set;
        }

        public int Key
        {
            get;
            private set;
        }

        public EquivalenceClassCollection EquivalenceClasses
        {
            get;
            set;
        }

        public void AddChild(DecisionTreeNode child)
        {
            if(children == null)
                children = new List<DecisionTreeNode>();
            children.Add(child);
        }

        public override string ToString()
        {
            if (this.Key == -1 && this.Value == -1)
                return "ROOT";

            return String.Format("{0} == {1}", this.Key, this.Value); 
        }
    }

    /// <summary>
    /// https://en.wikipedia.org/wiki/Tree_traversal
    /// </summary>
    public static class TreeNodeTraversal
    {
        /// <summary>
        /// Traverse tree in level order and perform Action for every tree node (aka Breadth-first search (BFS))
        /// </summary>
        /// <param name="node"></param>
        /// <param name="action"></param>
        public static void TraverseLevelOrder(ITreeNode node, Action<ITreeNode> action) 
        {
            Queue<ITreeNode> queue = new Queue<ITreeNode>();
            queue.Enqueue(node);
            while (queue.Count != 0)
            {
                ITreeNode currentNode = queue.Dequeue();
                action.Invoke(currentNode);

                if (currentNode.Children != null)
                    foreach (ITreeNode child in currentNode.Children)
                        if (child != null)
                            queue.Enqueue(child);
            }
        }

        public static void TraversePreOrder(ITreeNode node, Action<ITreeNode> action)
        {
            if (node == null)
                return;
            action.Invoke(node);
            if (node.Children != null)
                foreach (ITreeNode child in node.Children)
                    TreeNodeTraversal.TraversePreOrder(child, action);
        }
        
        public static void TraverseInOrder(ITreeNode node, Action<ITreeNode> action)
        {
            if (node == null)
                return;

            bool nodeActionFinished = false;
            
            if (node.Children != null)
                foreach (ITreeNode child in node.Children)
                {
                    TreeNodeTraversal.TraversePreOrder(child, action);
                    if (nodeActionFinished == false)
                    {
                        action.Invoke(node);
                        nodeActionFinished = true;
                    }
                }
        }
        
        public static void TraversePostOrder(ITreeNode node, Action<ITreeNode> action)
        {
            if (node == null)
                return;
            if (node.Children != null)
                foreach (ITreeNode child in node.Children)
                    TreeNodeTraversal.TraversePostOrder(child, action);
            action.Invoke(node);
        }

        public static void TraverseEulerPath(ITreeNode node, Action<ITreeNode> action)
        {
            if (node == null)
                return;
            action.Invoke(node);
            foreach (ITreeNode child in node.Children)
            {
                TreeNodeTraversal.TraverseEulerPath(child, action);
                action.Invoke(node);
            }
        }
    }
}
