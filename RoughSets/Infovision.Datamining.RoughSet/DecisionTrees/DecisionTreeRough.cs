using System;

namespace Infovision.Datamining.Roughset.DecisionTrees
{
    /// <summary>
    /// Decision tree learning where in each node a split is selected based on attribute generating highest measure M. 
    /// Measure M is calculated based on object weights.
    /// </summary>
    public class DecisionTreeRough : DecisionTreeBase
    {
        protected override double GetSplitScore(EquivalenceClassCollection attributeEqClasses, double dummy)
        {
            return (double) InformationMeasureWeights.Instance.Calc(attributeEqClasses);
        }

        protected override double GetCurrentScore(EquivalenceClassCollection eqClassCollection)
        {
            return 0;
        }
    }
}
