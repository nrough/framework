using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infovision.Datamining.Roughset.DecisionTrees
{
    /// <summary>
    /// ID3 Tree Implementation
    /// </summary>
    public class DecisionTreeID3 : DecisionTreeBase
    {
        protected override DecisionTreeBase CreateInstanceForClone()
        {
            return new DecisionTreeID3();
        }

        protected override double CalculateImpurityBeforeSplit(EquivalenceClassCollection eqClassCollection)
        {
            double entropy = 0;
            foreach (long dec in this.Decisions)
            {
                double decWeightedProbability = eqClassCollection.CountWeightDecision(dec);
                double p = (double)(decWeightedProbability / eqClassCollection.WeightSum);
                if (p != 0)
                    entropy -= p * System.Math.Log(p, 2);
            }
            return entropy;
        }

        protected override double CalculateImpurityAfterSplit(EquivalenceClassCollection equivalenceClasses)
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

        
    }
}
