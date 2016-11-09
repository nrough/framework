using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Infovision.Datamining.Roughset.DecisionTrees
{
    public delegate double ImpurityFunc(EquivalenceClassCollection equivalenceClasses);
    public delegate double ImpurityNormalizeFunc(double value, EquivalenceClassCollection equivalenceClasses);

    public static class ImpurityFunctions
    {
        public static double Entropy(EquivalenceClassCollection equivalenceClasses)
        {
            double result = 0;
            if (equivalenceClasses.WeightSum > 0)
            {
                foreach (var eq in equivalenceClasses)
                {
                    double localEntropy = 0;
                    foreach (var dec in eq.DecisionSet)
                    {
                        if (eq.WeightSum > 0)
                        {
                            double p = eq.GetDecisionWeight(dec) / eq.WeightSum;
                            if (p != 0)
                                localEntropy -= p * System.Math.Log(p, 2);
                        }
                    }

                    result += (eq.WeightSum / equivalenceClasses.WeightSum) * localEntropy;
                }
            }

            return result;
        }

        public static double Error(EquivalenceClassCollection equivalenceClasses)
        {
            double error = 0;
            if (equivalenceClasses.WeightSum > 0)
            {
                foreach (var eq in equivalenceClasses)
                {
                    double maxPd = Double.NegativeInfinity;
                    foreach (long dec in eq.DecisionSet)
                    {
                        double pd = eq.GetDecisionWeight(dec) / eq.WeightSum;
                        if (pd > maxPd)
                            maxPd = pd;
                    }
                    error += (eq.WeightSum / equivalenceClasses.WeightSum) * (1.0 - maxPd);
                }
            }

            return error;
        }

        public static double Gini(EquivalenceClassCollection equivalenceClasses)
        {
            double attributeGini = 0;
            if (equivalenceClasses.WeightSum > 0)
            {
                foreach (var eq in equivalenceClasses)
                {
                    double s2 = 0;
                    foreach (long dec in eq.DecisionSet)
                    {
                        double pD = eq.GetDecisionWeight(dec) / eq.WeightSum;
                        s2 += (pD * pD);
                    }
                    attributeGini += (eq.WeightSum / equivalenceClasses.WeightSum) * (1.0 - s2);
                }
            }

            return attributeGini;
        }

        public static double Majority(EquivalenceClassCollection equivalenceClasses)
        {
            return InformationMeasureWeights.Instance.Calc(equivalenceClasses);
        }

        public static double SplitInformationNormalize(double value, EquivalenceClassCollection equivalenceClasses)
        {
            double result = 0;
            foreach (var eq in equivalenceClasses)
            {
                double p = eq.WeightSum / equivalenceClasses.WeightSum;
                if (p != 0)
                    result -= p * System.Math.Log(p, 2);
            }
            return (result == 0) ? 0 : value / result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double DummyNormalize(double value, EquivalenceClassCollection equivalenceClasses)
        {
            return value;
        }
    }
}
