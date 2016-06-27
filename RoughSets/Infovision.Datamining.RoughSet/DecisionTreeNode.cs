using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infovision.Datamining.Roughset
{
    public class DecisionTreeNode
    {
        private List<DecisionTreeNode> children;


        public DecisionTreeNode(int attribute, long value)
            : this(attribute)
        {            
            this.Value = value;
        }

        public DecisionTreeNode(int attribute)
            : this()
        {
            this.Attribute = attribute;
        }

        private DecisionTreeNode()
        {
            this.Value = Int64.MinValue;
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

        public int Attribute
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
    }
}
