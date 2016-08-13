using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Infovision.Data;

namespace Infovision.Datamining.Roughset
{
    public interface ITreeNode
    {
        IReadOnlyList<ITreeNode> Children { get; }
        int Key { get; }
        long Value { get; }
        bool IsLeaf { get; }
        bool IsRoot { get; }
        ITreeNode Parent { get; }
        int Level { get; }
    }

    public class DecisionTreeNode : ITreeNode, IEnumerable<DecisionTreeNode>
    {
        private List<DecisionTreeNode> children;
        public static readonly string ROOT = "ROOT";

        public DecisionTreeNode(int key, long value, ITreeNode parent)
        {
            this.Key = key;
            this.Value = value;
            this.Parent = parent;
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

        public bool IsRoot
        {
            get
            {
                return this.Parent == null;
            }
        }

        public ITreeNode Parent
        {
            get;
            set;
        }

        public int Level
        {
            get
            {
                int level = 0;
                ITreeNode n = this;
                while(n.Parent != null)
                {
                    n = n.Parent;
                    level++;
                }
                return level;
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

        public void AddChild(DecisionTreeNode child)
        {
            if (children == null)
                children = new List<DecisionTreeNode>();
            children.Add(child);
        } 

        public override string ToString()
        {
            return this.ToString(null);
        }

        public string ToString(DataStoreInfo info)
        {
            if (this.IsRoot)
                return DecisionTreeNode.ROOT;

            return string.Format("({0} == {1})",
                (info != null) ? info.GetFieldInfo(this.Key).Name : this.Key.ToString(),
                (info != null) ? info.GetFieldInfo(this.Key).Internal2External(this.Value).ToString() : this.Value.ToString());
        }

        /// <summary>
        ///   Returns an enumerator that iterates through the node's subtree.
        /// </summary>
        ///
        /// <returns>
        ///   A <see cref="T:System.Collections.Generic.IEnumerator`1" /> that can be used to iterate through the collection.
        /// </returns>
        ///
        public IEnumerator<DecisionTreeNode> GetEnumerator()
        {
            var stack = new Stack<DecisionTreeNode>(new[] { this });

            while (stack.Count != 0)
            {
                DecisionTreeNode current = stack.Pop();

                yield return current;

                if (current.Children != null)
                    for (int i = current.Children.Count - 1; i >= 0; i--)
                        stack.Push((DecisionTreeNode)current.Children[i]);
            }
        }

        /// <summary>
        ///   Returns an enumerator that iterates through the node's subtree.
        /// </summary>
        ///
        /// <returns>
        ///   An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.
        /// </returns>
        ///
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public ICollection<int> GetChildUniqueKeys()
        {
            return this.Where(x => x.IsLeaf == false && x.Key != -1)
                    .GroupBy(x => x.Key)
                    .Select(g => g.First().Key)
                    .OrderBy(x => x).ToArray().ToArray();
        }
    }

    /// <summary>
    /// https://en.wikipedia.org/wiki/Tree_traversal
    /// </summary>
    public static class TreeNodeTraversal
    {
        /// <summary>
        /// Traverse tree in level order and perform Action for every tree node (aka Breadth-startFromIdx search (BFS))
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

    public class DecisionTreeFormatter
    {
        public static DecisionTreeFormatter Construct(ITreeNode node, DataStore data, int indent)
        {
            DecisionTreeFormatter treeFormatter = new DecisionTreeFormatter();
            treeFormatter.Root = node;
            treeFormatter.Data = data;
            treeFormatter.Indent = indent;
            return treeFormatter;
        }

        private DecisionTreeFormatter()
        {
            this.Indent = 2;
        }

        public ITreeNode Root { get; set; }
        public DataStore Data { get; set; }
        public int Indent { get; set; }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            Build(this.Root, 0, sb);
            return sb.ToString();
        }

        private string NodeToString(ITreeNode node, int currentLevel)
        {
            if (node is DecisionTreeNode)
                return string.Format("{0}{1}", new string(' ', this.Indent * currentLevel), ((DecisionTreeNode)node).ToString(this.Data.DataStoreInfo));
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