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
        private int leftInstance;
        private int rightInstance;        
        private double leftDistance;
        private double rightDistance;

        public virtual int LeftInstance
        {
            get { return this.leftInstance; }
            private set { this.leftInstance = value; }
        }

        public virtual int RightInstance
        {
            get { return this.rightInstance; }
            private set { this.rightInstance = value; }
        }

        public virtual double LeftDistance
        {
            get { return this.leftDistance; }
            private set { this.leftDistance = value; }
        }

        public virtual double RightDistance
        {
            get { return this.rightDistance; }
            private set { this.rightDistance = value; }
        }

        public virtual bool IsRoot
        {
            get { return parent == null && left != null && right != null; }
        }

        public virtual bool IsLeaf
        {
            get { return left == null && right == null && parent != null; }
        }

        public DendrogramNode()
        {            
        }        
    }
}
