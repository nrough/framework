using Raccoon.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Raccoon.MachineLearning.Classification.DecisionTrees.Pruning
{
    public interface IDecisionTreePruning
    {
        double GainThreshold { get; set; }
        double Prune();
    }
}
