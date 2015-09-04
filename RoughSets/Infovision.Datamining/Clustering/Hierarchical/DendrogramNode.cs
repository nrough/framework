using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infovision.Datamining.Clustering.Hierarchical
{
          
    [Serializable]        

    public class DendrogramNode : IComparable, IComparable<DendrogramNode>
    {
        public int Id { get; set; }
        public DendrogramNode LeftNode { get; set; }
        public DendrogramNode RightNode { get; set; }
        public DendrogramNode Parent { get; set; }        
        public double Height { get; set; }
        public double LeftLength { get; set; }
        public double RightLength { get; set; }

        public virtual bool IsRoot
        {
            get { return this.Parent == null && (this.LeftNode != null || this.RightNode != null); }
        }

        public virtual bool IsLeaf
        {
            get { return this.LeftNode == null && this.RightNode == null && this.Parent != null; }
        }        

        public DendrogramNode()
        {
        }

        public DendrogramNode(int nodeId)
            : this()
        {
            this.Id = nodeId;
        }

        public int CompareTo(object obj)
        {
            if (obj == null)
                return 1;

            DendrogramNode node = obj as DendrogramNode;
            if (node == null)
                throw new ArgumentException("Object is not a DendrogramNode");

            return this.Height.CompareTo(node.Height);
        }

        public int CompareTo(DendrogramNode other)
        {            
            if (other == null)
                return 1;
            return this.Height.CompareTo(other.Height);            
        }        
    }

    public class DendrogramNodeAscendingComparer : Comparer<DendrogramNode>
    {
        public override int Compare(DendrogramNode x, DendrogramNode y)
        {
            return x.CompareTo(y);
        }
    }

    public class DendrogramNodeDescendingComparer : Comparer<DendrogramNode>
    {
        public override int Compare(DendrogramNode x, DendrogramNode y)
        {
            return -1 * x.CompareTo(y);
        }
    }
}
