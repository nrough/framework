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
    public class DecisionTreeCART : DecisionTreeBase
    {
        public DecisionTreeCART()
            : base()
        {
            this.ImpurityFunction = ImpurityFunctions.Gini;
        }

        public DecisionTreeCART(string modelName)
            : base(modelName)
        {
            this.ImpurityFunction = ImpurityFunctions.Gini;
        }

        //protected override DecisionTreeBase CreateInstanceForClone()
        //{
        //    return new DecisionTreeCART();
        //}

        /*
        protected override double CalculateImpurityBeforeSplit(EquivalenceClassCollection eqClassCollection)
        {
            if (eqClassCollection.Count != 1)
                throw new ArgumentException("eqClassCollection.Count != 1", "eqClassCollection");

            double s2 = 0;
            foreach (long dec in this.Decisions)
            {
                double decWeightedProbability = eqClassCollection.CountWeightDecision(dec);
                double p = (double)(decWeightedProbability / eqClassCollection.WeightSum);
                s2 += (p * p);
            }
            return 1.0 - s2;
        }
        */

        /*
        protected override double CalculateImpurityAfterSplit(EquivalenceClassCollection eq)
        {
            double attributeGini = 0;
            foreach (var e in eq)
            {
                double pA = (double)(e.WeightSum / eq.WeightSum);

                double s2 = 0;
                foreach (long dec in e.DecisionSet)
                {
                    double pD = (double)(e.GetDecisionWeight(dec) / e.WeightSum);
                    s2 += (pD * pD);
                }
                attributeGini += (pA * (1.0 - s2));
            }

            return attributeGini;
        }
        */
    }
}
