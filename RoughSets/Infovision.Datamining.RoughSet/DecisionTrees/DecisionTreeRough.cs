using System;
using System.Linq;
using System.Collections.Generic;
using Infovision.Statistics;

namespace Infovision.Datamining.Roughset.DecisionTrees
{
    /// <summary>
    /// Decision tree learning where in each node a split is selected based on attribute generating highest measure M. 
    /// Measure M is calculated based on object weights.
    /// </summary>
    public class DecisionTreeRough : DecisionTreeBase
    {
        protected override SplitInfo GetSplitInfoSymbolic(int attributeId, EquivalenceClassCollection data, double parentMeasure)
        {
            var attributeEqClasses = EquivalenceClassCollection.Create(attributeId, data);
            double m = InformationMeasureWeights.Instance.Calc(attributeEqClasses);// + parentMeasure;
            //double splitInfo = this.SplitInformation(attributeEqClasses);
            //double normalizedM = splitInfo == 0 ? 0 : m / splitInfo;            

            return new SplitInfo(attributeId, m, attributeEqClasses, SplitType.Discreet, ComparisonType.EqualTo, 0);
        }

        private double SplitInformation(EquivalenceClassCollection eqClassCollection)
        {
            double result = 0;
            foreach (var eq in eqClassCollection)
            {
                double p = eq.WeightSum / eqClassCollection.WeightSum;
                if (p != 0)
                    result -= p * System.Math.Log(p, 2);
            }
            return result;
        }


        protected override double GetCurrentScore(EquivalenceClassCollection eqClassCollection)
        {
            //return 0;
            return InformationMeasureWeights.Instance.Calc(eqClassCollection);
        }
    }
}
