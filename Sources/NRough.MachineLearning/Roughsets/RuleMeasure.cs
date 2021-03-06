﻿// 
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

using System;

namespace NRough.MachineLearning.Roughsets
{
    public delegate double RuleQualityMethod(long decisionValue, IReduct reduct, EquivalenceClass eqClass);

    public static class RuleQualityMethods
    {
        //P(X,E)
        public static double Support(long decisionValue, IReduct reduct, EquivalenceClass eqClass)
        {
            int count_U = reduct.DataStore.NumberOfRecords;
            if (eqClass != null && count_U > 0)
                return (double)eqClass.GetNumberOfObjectsWithDecision(decisionValue) / (double)count_U;
            return 0;
        }

        //Pw(X,E)
        public static double SupportW(long decisionValue, IReduct reduct, EquivalenceClass eqClass)
        {
            return eqClass != null ? eqClass.GetDecisionWeight(decisionValue) : 0;
        }

        // P(X|E) = P(X,E)/P(E)
        public static double Confidence(long decisionValue, IReduct reduct, EquivalenceClass eqClass)
        {
            if (eqClass != null && eqClass.NumberOfObjects > 0)
                return (double)eqClass.GetNumberOfObjectsWithDecision(decisionValue) / (double)eqClass.NumberOfObjects;
            return 0;
        }

        // Pw(X|E) = Pw(X,E)/Pw(E)
        public static double ConfidenceW(long decisionValue, IReduct reduct, EquivalenceClass eqClass)
        {
            if (eqClass == null) return 0;
            double weightSum_XE = eqClass.GetDecisionWeight(decisionValue);
            double weightSum_E = eqClass.WeightSum;
            return weightSum_E != 0 ? weightSum_XE / weightSum_E : 0;
        }

        //P(E|X) = P(X,E)/P(X)
        public static double Coverage(long decisionValue, IReduct reduct, EquivalenceClass eqClass)
        {
            if (eqClass == null) return 0;
            int count_XE = eqClass.GetNumberOfObjectsWithDecision(decisionValue);
            if (count_XE == 0) return 0;
            int count_X = (int)reduct.DataStore.DataStoreInfo.DecisionInfo.Histogram[decisionValue];
            return count_X != 0 ? (double)count_XE / (double)count_X : 0;
        }

        //Pw(E|X) = Pw(X,E)/Pw(X)
        public static double CoverageW(long decisionValue, IReduct reduct, EquivalenceClass eqClass)
        {
            if (eqClass == null) return 0;
            double weight_XE = eqClass.GetDecisionWeight(decisionValue);
            if (weight_XE == 0) return 0;
            double weight_X = reduct.DataStore.DataStoreInfo.DecisionInfo.HistogramWeights[decisionValue];
            return (weight_X != 0) ? weight_XE / weight_X : 0;
        }

        //P(X,E)/P(E) / P(X) = P(X|E)/P(X)
        public static double Ratio(long decisionValue, IReduct reduct, EquivalenceClass eqClass)
        {
            if (eqClass == null) return 0;
            int count_XE = eqClass.GetNumberOfObjectsWithDecision(decisionValue);
            int count_E = eqClass.NumberOfObjects;
            if (count_E == 0) return 0;
            int count_X = (int)reduct.DataStore.DataStoreInfo.DecisionInfo.Histogram[decisionValue];
            return (count_E * count_X) != 0 ? ((double)count_XE / (double)(count_E * count_X)) : 0;
        }

        //Pw(X|E)/Pw(X) = Pw(X,E)/(Pw(E) * Pw(X))
        public static double RatioW(long decisionValue, IReduct reduct, EquivalenceClass eqClass)
        {
            if (eqClass == null) return 0;
            double weight_XE = eqClass.GetDecisionWeight(decisionValue);
            double weight_E = eqClass.WeightSum;
            if (weight_E == 0) return 0;
            double weight_X = reduct.DataStore.DataStoreInfo.DecisionInfo.HistogramWeights[decisionValue];
            return (weight_E * weight_X) != 0 ? weight_XE / (weight_E * weight_X) : 0;
        }

        //P(E)
        public static double Strength(long decisionValue, IReduct reduct, EquivalenceClass eqClass)
        {
            if (eqClass == null) return 0;
            int count_U = reduct.DataStore.NumberOfRecords;
            if (count_U > 0)
                return (double)eqClass.NumberOfObjects / (double)count_U;
            return 0;
        }

        //Pw(E)
        public static double StrengthW(long decisionValue, IReduct reduct, EquivalenceClass eqClass)
        {
            if (eqClass == null) return 0;
            return eqClass.WeightSum;
        }

        //P*(X|E) = (|X * E|/|X|) / (sum_{i} |X_{i} * E| / |X_{i}| )
        public static double ConfidenceRelative(long decisionValue, IReduct reduct, EquivalenceClass eqClass)
        {
            if (eqClass == null) return 0;
            double sum = 0;
            
            foreach (long dec in reduct.EquivalenceClasses.DecisionWeight.Keys)
            {
                int localCount_X = (int)reduct.DataStore.DataStoreInfo.DecisionInfo.Histogram[dec];
                if (localCount_X > 0)
                    sum += ((double)eqClass.GetNumberOfObjectsWithDecision(dec) / (double)localCount_X);
            }

            if (sum > 0)
            {
                int count_X = (int)reduct.DataStore.DataStoreInfo.DecisionInfo.Histogram[decisionValue];
                if (count_X > 0)
                    return ((double)eqClass.GetNumberOfObjectsWithDecision(decisionValue) / (double)count_X) / sum;
            }

            return 0;
        }

        //Pw*(X|E) = (|X * E|w/|X|w) / (sum_{i} |X_{i} * E|w / |X_{i}|w )
        public static double ConfidenceRelativeW(long decisionValue, IReduct reduct, EquivalenceClass eqClass)
        {
            if (eqClass == null) return 0;
            double sum = 0;
            
            foreach (long dec in reduct.EquivalenceClasses.DecisionWeight.Keys)
            {
                double localWeight_X = reduct.DataStore.DataStoreInfo.DecisionInfo.HistogramWeights[dec];
                if (localWeight_X > 0)
                    sum += (eqClass.GetDecisionWeight(dec) / localWeight_X);
            }

            if (sum > 0)
            {
                //double weight_X = reduct.EquivalenceClasses.CountWeightDecision(decisionValue);
                double weight_X = reduct.DataStore.DataStoreInfo.DecisionInfo.HistogramWeights[decisionValue];
                if (weight_X > 0)
                    return (eqClass.GetDecisionWeight(decisionValue) / weight_X) / sum;
            }

            return 0;
        }

        //Used for rule voting, returns one
        public static double SingleVote(long decisionValue, IReduct reduct, EquivalenceClass eqClass)
        {
            return 1.0;
        }
    }

    public static class RuleQualityAvgMethods
    {
        //P(X,E)
        public static double Support(long decisionValue, IReduct reduct, EquivalenceClass eqClass)
        {
            int count_U = reduct.DataStore.NumberOfRecords;
            if (eqClass != null && count_U > 0)
                return (double)eqClass.AvgConfidenceSum / (double)count_U;
            return 0;
        }

        //Pw(X,E)
        public static double SupportW(long decisionValue, IReduct reduct, EquivalenceClass eqClass)
        {
            return eqClass != null ? eqClass.AvgConfidenceWeight : 0;
        }

        // P(X|E) = P(X,E)/P(E)
        public static double Confidence(long decisionValue, IReduct reduct, EquivalenceClass eqClass)
        {
            if (eqClass != null && eqClass.NumberOfObjects > 0)
                return (double)eqClass.AvgConfidenceSum / (double)eqClass.NumberOfObjects;
            return 0;
        }

        // Pw(X|E) = Pw(X,E)/Pw(E)
        public static double ConfidenceW(long decisionValue, IReduct reduct, EquivalenceClass eqClass)
        {
            if (eqClass == null) return 0;
            double weightSum_XE = eqClass.AvgConfidenceWeight;
            double weightSum_E = eqClass.WeightSum;
            return weightSum_E != 0 ? weightSum_XE / weightSum_E : 0;
        }

        //P(E|X) = P(X,E)/P(X)
        public static double Coverage(long decisionValue, IReduct reduct, EquivalenceClass eqClass)
        {
            if (eqClass == null) return 0;
            int count_XE = eqClass.AvgConfidenceSum;
            if (count_XE == 0) return 0;
            int count_X = (int)reduct.DataStore.DataStoreInfo.DecisionInfo.Histogram[decisionValue];
            return count_X != 0 ? (double)count_XE / (double)count_X : 0;
        }

        //Pw(E|X) = Pw(X,E)/Pw(X)
        public static double CoverageW(long decisionValue, IReduct reduct, EquivalenceClass eqClass)
        {
            if (eqClass == null) return 0;
            double weight_XE = eqClass.AvgConfidenceWeight;
            if (weight_XE == 0) return 0;
            double weight_X = reduct.DataStore.DataStoreInfo.DecisionInfo.HistogramWeights[decisionValue];
            return (weight_X != 0) ? weight_XE / weight_X : 0;
        }

        //P(X,E)/P(E) / P(X) = P(X|E)/P(X)
        public static double Ratio(long decisionValue, IReduct reduct, EquivalenceClass eqClass)
        {
            if (eqClass == null) return 0;
            int count_XE = eqClass.AvgConfidenceSum;
            int count_E = eqClass.NumberOfObjects;
            if (count_E == 0) return 0;
            int count_X = (int)reduct.DataStore.DataStoreInfo.DecisionInfo.Histogram[decisionValue];
            return (count_E * count_X) != 0 ? ((double)count_XE / (double)(count_E * count_X)) : 0;
        }

        //Pw(X|E)/Pw(X) = Pw(X,E)/(Pw(E) * Pw(X))
        public static double RatioW(long decisionValue, IReduct reduct, EquivalenceClass eqClass)
        {
            if (eqClass == null) return 0;
            double weight_XE = eqClass.AvgConfidenceWeight;
            double weight_E = eqClass.WeightSum;
            if (weight_E == 0) return 0;
            double weight_X = reduct.DataStore.DataStoreInfo.DecisionInfo.HistogramWeights[decisionValue];
            return (weight_E * weight_X) != 0 ? weight_XE / (weight_E * weight_X) : 0;
        }

        //Pw*(X|E) = (|X * E|w/|X|w) / (sum_{i} |X_{i} * E|w / |X_{i}|w )
        public static double ConfidenceRelativeW(long decisionValue, IReduct reduct, EquivalenceClass eqClass)
        {
            if (eqClass == null) return 0;
            double sum = 0;
            
            foreach (long dec in reduct.EquivalenceClasses.DecisionWeight.Keys)
            {
                double localWeight_X = reduct.DataStore.DataStoreInfo.DecisionInfo.HistogramWeights[dec];
                if (localWeight_X > 0)
                    sum += (eqClass.AvgConfidenceWeight / localWeight_X);
            }

            if (sum > 0)
            {
                double weight_X = reduct.DataStore.DataStoreInfo.DecisionInfo.HistogramWeights[decisionValue];
                if (weight_X > 0)
                    return (eqClass.AvgConfidenceWeight / weight_X) / sum;
            }

            return 0;
        }
    }
}