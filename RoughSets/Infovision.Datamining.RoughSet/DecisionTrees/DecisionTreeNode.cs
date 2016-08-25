using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Infovision.Data;

namespace Infovision.Datamining.Roughset.DecisionTrees
{
    public class DecisionTreeNode : ITreeNode, IEnumerable<DecisionTreeNode>
    {
        private List<DecisionTreeNode> children;
        public static readonly string ROOT = "ROOT";
        int level;
        decimal m;

        public DecisionTreeNode(int key, long value, ITreeNode parent)
        {
            this.Key = key;
            this.Value = value;
            this.Parent = parent;
            this.level = parent == null ? 0 : parent.Level + 1;
            this.m = Decimal.Zero;
        }

        public DecisionTreeNode(int key, long value, decimal measure, ITreeNode parent)
            : this(key, value, parent)
        {
            this.m = measure;
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
            get { return this.level; }
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

        public decimal Measure
        {
            get { return this.m; }
            set { this.m = value; }
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
}