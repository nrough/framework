using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Raccoon.MachineLearning.Classification.DecisionTrees
{
    public interface IDecisionTreeNode
    {
        int Id { get; }
        int Attribute { get; }

        IDecisionTreeNode Parent { get; set; }
        IList<IDecisionTreeNode> Children { get; set; }
        
        long Value { get; }
        bool Compute(long value);        

        bool IsLeaf { get; }
        bool IsRoot { get; }
        int Level { get; }
                               
        long Output { get; }
        DecisionDistribution OutputDistribution { get; set; }

        double Measure { get; }
    }
}
