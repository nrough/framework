using System;

namespace Infovision.Datamining.Roughset.DecisionTrees
{
    /// <summary>
    /// Decision tree learning where in each node a split is selected based on attribute generating highest measure M. 
    /// Measure M is calculated based on object weights.
    /// </summary>
    public class DecisionTreeRough : DecisionTreeBase
    {
        protected override SplitInfo GetSplitInfoSymbolic(int attributeId, EquivalenceClassCollection data, double dummy)
        {
            var attributeEqClasses = EquivalenceClassCollection.Create(attributeId, data);
            return new SplitInfo(attributeId, 
                InformationMeasureWeights.Instance.Calc(attributeEqClasses), 
                attributeEqClasses, SplitType.Discreet, ComparisonType.EqualTo, 0);
        }

        protected override double GetCurrentScore(EquivalenceClassCollection eqClassCollection)
        {
            return 0;
        }
    }
}
