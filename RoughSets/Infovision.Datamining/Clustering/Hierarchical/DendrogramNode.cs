using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infovision.Datamining.Clustering.Hierarchical
{
          
    [Serializable]    
    //TODO single points should also be DendrogramNodes! for simplicity!    

    public class DendrogramNode : IComparable, IComparable<DendrogramNode>
    {
        private DendrogramNode left;
        private DendrogramNode right;
        private DendrogramNode parent;
        private int nodeId;
        private int leftInstance;
        private int rightInstance;        
        private double leftLength;
        private double rightLength;
        private double height;

        public int LeftInstance
        {
            get { return this.leftInstance; }
            set { this.leftInstance = value; }
        }

        public int RightInstance
        {
            get { return this.rightInstance; }
            set { this.rightInstance = value; }
        }

        public double LeftLength
        {
            get { return this.leftLength; }
            set { this.leftLength = value; }
        }

        public double RightLength
        {
            get { return this.rightLength; }
            set { this.rightLength = value; }
        }

        public DendrogramNode LeftNode
        {
            get { return this.left; }
            set { this.left = value; }
        }

        public DendrogramNode RightNode
        {
            get { return this.right ; }
            set { this.right = value; }
        }

        public DendrogramNode Parent
        {
            get { return this.parent; }
            set { this.parent = value; }
        }

        public int NodeId
        {
            get { return this.nodeId; }
            set { this.nodeId = value; }
        }

        public virtual bool IsRoot
        {
            get { return parent == null && left != null && right != null; }
        }

        public virtual bool IsLeaf
        {
            get { return left == null && right == null && parent != null; }
        }

        public virtual double Height
        {
            get { return this.height; }
            set { this.height = value; }
        }

        public DendrogramNode()
        {
        }

        public DendrogramNode(int nodeId)
            : this()
        {            
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
