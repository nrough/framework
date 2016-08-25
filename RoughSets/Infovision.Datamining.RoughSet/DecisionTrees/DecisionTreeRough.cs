using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infovision.Datamining.Roughset.DecisionTrees
{
    public class DecisionTreeRough : DecisionTree
    {
        protected override double GetSplitScore(EquivalenceClassCollection attributeEqClasses, double dummy)
        {
            decimal result = Decimal.Zero;
            decimal maxValue, sum;
            foreach (var eq in attributeEqClasses)
            {
                maxValue = Decimal.MinValue;
                foreach (long decisionValue in eq.DecisionValues)
                {
                    sum = eq.GetDecisionWeight(decisionValue);
                    if (sum > maxValue)
                        maxValue = sum;
                }
                result += maxValue;
            }
            return (double)result;
        }

        protected override double GetCurrentScore(EquivalenceClassCollection eqClassCollection)
        {
            return 0;
        }
    }
}
