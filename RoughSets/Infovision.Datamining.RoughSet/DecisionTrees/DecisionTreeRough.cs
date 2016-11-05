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
        protected override DecisionTreeBase CreateInstanceForClone()
        {
            return new DecisionTreeRough();
        }

        protected override double CalculateImpurityAfterSplit(EquivalenceClassCollection eq)
        {
            return InformationMeasureWeights.Instance.Calc(eq);
        }

        
        protected override SplitInfo GetSplitInfoSymbolic(int attributeId, EquivalenceClassCollection data, double entropy)
        {
            SplitInfo result = base.GetSplitInfoSymbolic(attributeId, data, entropy);
            double splitInfo = this.SplitInformation(result.EquivalenceClassCollection);
            result.Gain = (splitInfo == 0) ? 0 : result.Gain / splitInfo;
            return result;
        }

        protected override SplitInfo GetSplitInfoNumeric(int attributeId, EquivalenceClassCollection data, double entropy)
        {
            SplitInfo result = base.GetSplitInfoNumeric(attributeId, data, entropy);
            double splitInfo = this.SplitInformation(result.EquivalenceClassCollection);
            result.Gain = (splitInfo == 0) ? 0 : result.Gain / splitInfo;
            return result;
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


        protected override double CalculateImpurityBeforeSplit(EquivalenceClassCollection eqClassCollection)
        {            
            return InformationMeasureWeights.Instance.Calc(eqClassCollection);
        }
    }
}
