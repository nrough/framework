namespace Infovision.Datamining.Roughset.DecisionTrees
{
    /// <summary>
    /// Interface of a decision tree
    /// </summary>
    public interface IDecisionTree : ILearner, IPredictionModel
    {
        ITreeNode Root { get; }        
        int NumberOfAttributesToCheckForSplit { get; set; }
    }
}
