using Infovision.Statistics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Infovision.Datamining.Roughset.DecisionTrees
{            
    /// <summary>
    /// C4.5 Tree Implemetation
    /// </summary>
    public class DecisionTreeC45 : DecisionTreeBase
    {
        public DecisionTreeC45()
            : base()
        {
        }

        protected override DecisionTreeBase CreateInstanceForClone()
        {
            return new DecisionTreeC45();
        }        

        protected override double GetCurrentScore(EquivalenceClassCollection eqClassCollection)
        {
            double entropy = 0;
            foreach (long dec in this.Decisions)
            {
                double decWeightedProbability = eqClassCollection.CountWeightDecision(dec);
                double p = decWeightedProbability / eqClassCollection.WeightSum;
                if (p != 0)
                    entropy -= p * System.Math.Log(p, 2);
            }
            return entropy;
        }

        protected override double CalculateImpurity(EquivalenceClassCollection equivalenceClasses)
        {
            double result = 0;
            foreach (var eq in equivalenceClasses)
            {
                double localEntropy = 0;
                foreach (var dec in eq.DecisionSet)
                {
                    double decWeight = eq.GetDecisionWeight(dec);
                    double p = decWeight / eq.WeightSum;
                    if (p != 0)
                        localEntropy -= p * System.Math.Log(p, 2);
                }
                
                result += (eq.WeightSum / equivalenceClasses.WeightSum) * localEntropy;
            }

            return result;            
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
            if (eqClassCollection == null)
                return 0;

            double result = 0;
            foreach (var eq in eqClassCollection)
            {
                double p = eq.WeightSum / eqClassCollection.WeightSum;
                if (p != 0)
                    result -= p * System.Math.Log(p, 2);
            }            

            return result;
        }   
    }
}
