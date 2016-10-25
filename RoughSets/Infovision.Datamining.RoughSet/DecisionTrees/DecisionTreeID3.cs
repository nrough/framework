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

        protected override SplitInfo GetSplitInfoSymbolic(int attributeId, EquivalenceClassCollection data, double entropy)
        {
            var attributeEqClasses = EquivalenceClassCollection.Create(attributeId, data);

            double result = 0;
            foreach (var eq in attributeEqClasses)
            {
                double localEntropy = 0;
                foreach (var dec in eq.DecisionSet)
                {
                    double decWeight = eq.GetDecisionWeight(dec);
                    double p = decWeight / eq.WeightSum;
                    if (p != 0)
                        localEntropy -= p * System.Math.Log(p, 2);
                }

                result += (eq.WeightSum / data.WeightSum) * localEntropy;
            }

            double gain = entropy - result;

            return new SplitInfo(attributeId, gain, attributeEqClasses, SplitType.Discreet, ComparisonType.EqualTo, 0);
        }

        protected override double GetCurrentScore(EquivalenceClassCollection eqClassCollection)
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
    }
}
