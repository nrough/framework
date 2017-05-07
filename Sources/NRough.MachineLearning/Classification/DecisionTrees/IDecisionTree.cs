using System.Collections.Generic;

namespace NRough.MachineLearning.Classification.DecisionTrees
{
    /// <summary>
    /// Interface of a decision tree
    /// </summary>
    public interface IDecisionTree : ILearner, IClassificationModel, IEnumerable<IDecisionTreeNode>
    {
        IDecisionTreeNode Root { get; }
        int NumberOfAttributesToCheckForSplit { get; set; }
        double Gamma { get; set; }
    }
}
