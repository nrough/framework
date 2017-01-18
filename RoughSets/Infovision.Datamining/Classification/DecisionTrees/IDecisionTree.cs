using System.Collections.Generic;

namespace Raccoon.MachineLearning.Classification.DecisionTrees
{
    /// <summary>
    /// Interface of a decision tree
    /// </summary>
    public interface IDecisionTree : ILearner, IPredictionModel, IEnumerable<IDecisionTreeNode>
    {
        IDecisionTreeNode Root { get; }
        int NumberOfAttributesToCheckForSplit { get; set; }
        double Gamma { get; set; }
    }
}
