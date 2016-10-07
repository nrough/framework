using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Infovision.Data;
using Infovision.Utils;

namespace Infovision.Datamining.Roughset.DecisionTrees
{
    public class DecisionTreeNode : IDecisionTreeNode, IEnumerable<DecisionTreeNode>
    {
        #region Members

        private IList<IDecisionTreeNode> children;
        private static readonly string ROOT = "ROOT";

        #endregion

        #region Properties

        public int Id { get; private set; }

        public IList<IDecisionTreeNode> Children
        {
            get
            {
                return (IList<IDecisionTreeNode>)this.children;
            }

            set
            {
                this.children = value;
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

        public IDecisionTreeNode Parent { get; set; }

        public int Level
        {
            get;
            private set;
        }

        public long Value { get; private set; }

        public int Attribute { get; private set; }

        public double Measure
        {
            get;
            set;
        }

        public ComparisonType Comparison { get; private set; }

        public long Output { get; set; }

        #endregion

        #region Constructors

        public DecisionTreeNode(int id, int attribute, ComparisonType comparisonType, long value, IDecisionTreeNode parent)
        {
            this.Id = id;
            this.Attribute = attribute;
            this.Value = value;
            this.Parent = parent;
            this.Level = (parent == null) ? 0 : parent.Level + 1;
            this.Measure = 0.0;
            this.Comparison = comparisonType;
            this.Output = -1;
        }

        public DecisionTreeNode(int id, int attribute, ComparisonType comparisonType, long value, IDecisionTreeNode parent, double measure)
            : this(id, attribute, comparisonType, value, parent)
        {
            this.Measure = measure;
        }

        #endregion

        #region Methods

        public void AddChild(IDecisionTreeNode child)
        {
            if (children == null)
                children = new List<IDecisionTreeNode>();

            children.Add(child);
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
            return this.Where(x => x.Attribute != -1)
                    .GroupBy(x => x.Attribute)
                    .Select(g => g.First().Attribute)
                    .OrderBy(x => x).ToArray().ToArray();

            //return this.Where(x => x.IsLeaf == false && x.Key != -1)
            //        .GroupBy(x => x.Key)
            //        .Select(g => g.First().Key)
            //        .OrderBy(x => x).ToArray().ToArray();
        }

        public bool Compute(long value)
        {
            switch (this.Comparison)
            {                
                case ComparisonType.EqualTo:
                    return (this.Value == value);

                case ComparisonType.NotEqualTo:
                    return (this.Value != value);

                case ComparisonType.LessThan:
                    return (value < this.Value);

                case ComparisonType.LessThanOrEqualTo:
                    return (value <= this.Value);

                case ComparisonType.GreaterThan:
                    return (value > this.Value);

                case ComparisonType.GreaterThanOrEqualTo:
                    return (value >= this.Value);

                default:
                    throw new NotImplementedException("Comparison type not implemented");
            }
        }

        public override string ToString()
        {
            return this.ToString(null);
        }

        public string ToString(DataStoreInfo info)
        {
            if (this.IsRoot)
                return DecisionTreeNode.ROOT;

            if(!this.IsLeaf)
                return string.Format("[{0}] ({1} {2} {3})",
                    this.Id,
                    (info != null) ? info.GetFieldInfo(this.Attribute).Name : this.Attribute.ToString(),
                    this.Comparison.ToSymbol(),
                    (info != null) ? info.GetFieldInfo(this.Attribute).Internal2External(this.Value).ToString() : this.Value.ToString());

            return string.Format("[{0}] ({1} {2} {3}) ==> {4}",
                    this.Id,
                    (info != null) ? info.GetFieldInfo(this.Attribute).Name : this.Attribute.ToString(),
                    this.Comparison.ToSymbol(),
                    (info != null) ? info.GetFieldInfo(this.Attribute).Internal2External(this.Value).ToString() : this.Value.ToString(),
                    this.Output);
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            DecisionTreeNode node = obj as DecisionTreeNode;
            if (node == null)
                return false;

            /*
            return node.Attribute == this.Attribute 
                && node.Value == this.Value                 
                && node.Comparison == this.Comparison;            
            */

            return node.Id == this.Id;
        }

        public override int GetHashCode()
        {
            //return HashHelper.GetHashCode<int, long>(this.Attribute, this.Value);
            return this.Id;
        }

        #endregion
    }
}