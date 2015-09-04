using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infovision.Datamining.Clustering.Hierarchical
{
          
    [Serializable]
    public class DendrogramNode
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

        public static DendrogramNode Swap(DendrogramNode node)
        {
            DendrogramNode newNode = new DendrogramNode(node.NodeId);
            newNode.LeftInstance = node.RightInstance;
            newNode.LeftLength = node.RightLength;
            newNode.LeftNode = node.RightNode;
            newNode.RightInstance = node.LeftInstance;
            newNode.RightLength = node.LeftLength;
            newNode.RightNode = node.LeftNode;
            newNode.Parent = node.Parent;

            return newNode;
        }
    }
}
