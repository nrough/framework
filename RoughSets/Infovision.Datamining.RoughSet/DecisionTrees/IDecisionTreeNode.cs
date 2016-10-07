using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infovision.Datamining.Roughset.DecisionTrees
{
    public interface IDecisionTreeNode
    {
        int Id { get; }

        int Attribute { get; }
        IList<IDecisionTreeNode> Children { get; set; }
        
        long Value { get; }
        bool Compute(long value);        

        bool IsLeaf { get; }
        bool IsRoot { get; }
        int Level { get; }

        IDecisionTreeNode Parent { get; set; }
        
        double Measure { get; }
        
        long Output { get; set; }
    }
}
