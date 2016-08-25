using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infovision.Datamining.Roughset.DecisionTrees
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
        decimal Measure { get; }
    }
}
