using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infovision.Datamining.Roughset.DecisionTrees
{
    /// <summary>
    /// CART Tree implementation
    /// </summary>
    /// <remarks>
    /// Implementation is based on the following example http://csucidatamining.weebly.com/assign-4.html
    /// </remarks>
    public class DecisionTreeCART : DecisionTree
    {
        protected override double GetSplitScore(EquivalenceClassCollection attributeEqClasses, double gini)
        {
            double attributeGini = 0;
            foreach (var eq in attributeEqClasses)
            {
                double pA = (double)(eq.WeightSum / attributeEqClasses.WeightSum);

                double s2 = 0;
                foreach (long dec in eq.DecisionSet)
                {
                    double pD = (double)(eq.GetDecisionWeight(dec) / eq.WeightSum);
                    s2 += (pD * pD);
                }
                attributeGini += (pA * (1.0 - s2));
            }
            return gini - attributeGini;
        }

        protected override double GetCurrentScore(EquivalenceClassCollection eqClassCollection)
        {
            double s2 = 0;
            foreach (long dec in this.Decisions)
            {
                decimal decWeightedProbability = eqClassCollection.CountWeightDecision(dec);
                double p = (double)(decWeightedProbability / eqClassCollection.WeightSum);
                s2 += (p * p);
            }
            return 1.0 - s2;
        }
    }
}
