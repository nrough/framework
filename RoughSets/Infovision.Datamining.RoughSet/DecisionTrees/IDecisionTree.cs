﻿using System.Collections.Generic;

namespace Infovision.Datamining.Roughset.DecisionTrees
{
    /// <summary>
    /// Interface of a decision tree
    /// </summary>
    public interface IDecisionTree : ILearner, IPredictionModel, IEnumerable<IDecisionTreeNode>
    {
        IDecisionTreeNode Root { get; }        
        int NumberOfAttributesToCheckForSplit { get; set; }
    }
}
