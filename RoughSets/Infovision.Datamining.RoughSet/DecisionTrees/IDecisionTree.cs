namespace Infovision.Datamining.Roughset.DecisionTrees
{
    /// <summary>
    /// Interface of a decision tree
    /// </summary>
    public interface IDecisionTree : ILearner, IPredictionModel
    {
        ITreeNode Root { get; }
        decimal Epsilon { get; set; }
        int NumberOfAttributesToCheckForSplit { get; set; }
    }
}
