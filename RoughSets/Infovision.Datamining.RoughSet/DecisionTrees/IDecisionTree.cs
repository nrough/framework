using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infovision.Datamining.Roughset.DecisionTrees
{
    /// <summary>
    /// Interface of a decision tree
    /// </summary>
    public interface IDecisionTree : ILearner, IPredictionModel
    {
        ITreeNode Root { get; }
        decimal Epsilon { get; }
        int NumberOfAttributesToCheckForSplit { get; set; }
    }
}
