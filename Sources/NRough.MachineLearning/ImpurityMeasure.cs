// 
// NRough Framework
// http://www.nrough.net
// 
// Copyright (c) Sebastian Widz, 2017
// authors at nrough.net
// 
// This file is part of NRough Framework.
// 
// NRough Framework is free software; you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation; either version 3 of the License, or
// (at your option) any later version.
// 
// NRough Framework is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with NRough Framework; if not, write to the Free Software
// Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA  02110-1301  USA
//

using NRough.MachineLearning.Roughsets;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace NRough.MachineLearning
{
    public delegate double ImpurityFunc(EquivalenceClassCollection equivalenceClasses);
    public delegate double ImpurityNormalizeFunc(double value, EquivalenceClassCollection equivalenceClasses);

    public static class ImpurityMeasure
    {
        public static double One(EquivalenceClassCollection equivalenceClasses)
        {
            return 1.0;
        }

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
                        double pd = eq.GetDecisionWeight(dec);
                        if (pd > maxPd)
                            maxPd = pd;
                    }
                    error += eq.WeightSum  * (1.0 - (maxPd / eq.WeightSum));
                }

                return error / equivalenceClasses.WeightSum;
            }

            return 0;
        }

        public static double Majority(EquivalenceClassCollection equivalenceClasses)
        {                        
            double result = 0.0;
            double maxValue, sum;
            if (equivalenceClasses.WeightSum > 0)
            {
                foreach (EquivalenceClass eq in equivalenceClasses)
                {
                    maxValue = Double.NegativeInfinity;
                    foreach (long decisionValue in eq.DecisionValues)
                    {
                        sum = eq.GetDecisionWeight(decisionValue);
                        if (sum > maxValue)
                            maxValue = sum;
                    }

                    result += (eq.WeightSum / equivalenceClasses.WeightSum) * (maxValue / eq.WeightSum);
                }                
            }
                       
            return result;
        }

        public static double Majority2(EquivalenceClassCollection equivalenceClasses)
        {
            var result = InformationMeasureMajority.Instance.Calc(equivalenceClasses);
            //var result2 = Majority(equivalenceClasses);
            //var result3 = InformationMeasureWeights.Instance.Calc(equivalenceClasses);
            return 1.0 - result;
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
