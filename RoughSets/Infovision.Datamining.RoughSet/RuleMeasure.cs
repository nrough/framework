using System;

namespace Infovision.Datamining.Roughset
{
    public delegate decimal RuleQualityFunction(long decisionValue, IReduct reduct, EquivalenceClass eqClass);

    public static class RuleQuality
    {
        //P(X,E)
        public static decimal Support(long decisionValue, IReduct reduct, EquivalenceClass eqClass)
        {
            int count_U = reduct.DataStore.NumberOfRecords;
            if (eqClass != null && count_U > 0)
                return (decimal)eqClass.GetNumberOfObjectsWithDecision(decisionValue) / (decimal)count_U;
            return 0;
        }

        //Pw(X,E)
        public static decimal SupportW(long decisionValue, IReduct reduct, EquivalenceClass eqClass)
        {
            return eqClass != null ? eqClass.GetDecisionWeight(decisionValue) : 0;
        }

        // P(X|E) = P(X,E)/P(E)
        public static decimal Confidence(long decisionValue, IReduct reduct, EquivalenceClass eqClass)
        {
            if (eqClass != null && eqClass.NumberOfObjects > 0)
                return (decimal)eqClass.GetNumberOfObjectsWithDecision(decisionValue) / (decimal)eqClass.NumberOfObjects;
            return 0;
        }

        // Pw(X|E) = Pw(X,E)/Pw(E)
        public static decimal ConfidenceW(long decisionValue, IReduct reduct, EquivalenceClass eqClass)
        {
            if (eqClass == null) return 0;
            decimal weightSum_XE = eqClass.GetDecisionWeight(decisionValue);
            decimal weightSum_E = eqClass.WeightSum;
            return weightSum_E != 0 ? weightSum_XE / weightSum_E : 0;
        }

        //P(E|X) = P(X,E)/P(X)
        public static decimal Coverage(long decisionValue, IReduct reduct, EquivalenceClass eqClass)
        {
            if (eqClass == null) return 0;
            int count_XE = eqClass.GetNumberOfObjectsWithDecision(decisionValue);
            if (count_XE == 0) return 0;
            int count_X = (int)reduct.DataStore.DataStoreInfo.DecisionInfo.Histogram[decisionValue];
            return count_X != 0 ? (decimal)count_XE / (decimal)count_X : 0;
        }

        //Pw(E|X) = Pw(X,E)/Pw(X)
        public static decimal CoverageW(long decisionValue, IReduct reduct, EquivalenceClass eqClass)
        {
            if (eqClass == null) return 0;
            decimal weight_XE = eqClass.GetDecisionWeight(decisionValue);
            if (weight_XE == 0) return 0;
            decimal weight_X = reduct.DataStore.DataStoreInfo.DecisionInfo.HistogramWeights[decisionValue];
            return (weight_X != 0) ? weight_XE / weight_X : 0;
        }

        //P(X,E)/P(E) / P(X) = P(X|E)/P(X)
        public static decimal Ratio(long decisionValue, IReduct reduct, EquivalenceClass eqClass)
        {
            if (eqClass == null) return 0;
            int count_XE = eqClass.GetNumberOfObjectsWithDecision(decisionValue);
            int count_E = eqClass.NumberOfObjects;
            if (count_E == 0) return 0;
            int count_X = (int)reduct.DataStore.DataStoreInfo.DecisionInfo.Histogram[decisionValue];
            return (count_E * count_X) != 0 ? ((decimal)count_XE / (decimal)(count_E * count_X)) : 0;
        }

        //Pw(X|E)/Pw(X) = Pw(X,E)/(Pw(E) * Pw(X))
        public static decimal RatioW(long decisionValue, IReduct reduct, EquivalenceClass eqClass)
        {
            if (eqClass == null) return 0;
            decimal weight_XE = eqClass.GetDecisionWeight(decisionValue);
            decimal weight_E = eqClass.WeightSum;
            if (weight_E == 0) return 0;
            decimal weight_X = reduct.DataStore.DataStoreInfo.DecisionInfo.HistogramWeights[decisionValue];
            return (weight_E * weight_X) != 0 ? weight_XE / (weight_E * weight_X) : 0;
        }

        //P(E)
        public static decimal Strength(long decisionValue, IReduct reduct, EquivalenceClass eqClass)
        {
            if (eqClass == null) return 0;
            int count_U = reduct.DataStore.NumberOfRecords;
            if (count_U > 0)
                return (decimal)eqClass.NumberOfObjects / (decimal)count_U;
            return 0;
        }

        //Pw(E)
        public static decimal StrengthW(long decisionValue, IReduct reduct, EquivalenceClass eqClass)
        {
            if (eqClass == null) return 0;
            return eqClass.WeightSum;
        }

        //P*(X|E) = (|X * E|/|X|) / (sum_{i} |X_{i} * E| / |X_{i}| )
        public static decimal ConfidenceRelative(long decisionValue, IReduct reduct, EquivalenceClass eqClass)
        {
            if (eqClass == null) return 0;
            decimal sum = 0;

            //TODO should we consider all decision in case of intersections?
            foreach (long dec in reduct.ObjectSetInfo.GetDecisionValues())
            {
                int localCount_X = (int)reduct.DataStore.DataStoreInfo.DecisionInfo.Histogram[dec];
                if (localCount_X > 0)
                    sum += ((decimal)eqClass.GetNumberOfObjectsWithDecision(dec) / (decimal)localCount_X);
            }

            if (sum > 0)
            {
                int count_X = (int)reduct.DataStore.DataStoreInfo.DecisionInfo.Histogram[decisionValue];
                if (count_X > 0)
                    return ((decimal)eqClass.GetNumberOfObjectsWithDecision(decisionValue) / (decimal)count_X) / sum;
            }

            return 0;
        }

        //Pw*(X|E) = (|X * E|w/|X|w) / (sum_{i} |X_{i} * E|w / |X_{i}|w )
        public static decimal ConfidenceRelativeW(long decisionValue, IReduct reduct, EquivalenceClass eqClass)
        {
            if (eqClass == null) return 0;
            decimal sum = 0;

            //TODO should we consider all decision in case of intersections?
            foreach (long dec in reduct.ObjectSetInfo.GetDecisionValues())
            {
                decimal localWeight_X = reduct.DataStore.DataStoreInfo.DecisionInfo.HistogramWeights[dec];
                if (localWeight_X > 0)
                    sum += (eqClass.GetDecisionWeight(dec) / localWeight_X);
            }

            if (sum > 0)
            {
                //decimal weight_X = reduct.EquivalenceClasses.CountWeightDecision(decisionValue);
                decimal weight_X = reduct.DataStore.DataStoreInfo.DecisionInfo.HistogramWeights[decisionValue];
                if (weight_X > 0)
                    return (eqClass.GetDecisionWeight(decisionValue) / weight_X) / sum;
            }

            return 0;
        }

        //Used for rule voting, returns one
        public static decimal SingleVote(long decisionValue, IReduct reduct, EquivalenceClass eqClass)
        {
            return Decimal.One;
        }
    }

    public static class RuleQualityAvg
    {
        //P(X,E)
        public static decimal Support(long decisionValue, IReduct reduct, EquivalenceClass eqClass)
        {
            int count_U = reduct.DataStore.NumberOfRecords;
            if (eqClass != null && count_U > 0)
                return (decimal)eqClass.AvgConfidenceSum / (decimal)count_U;
            return 0;
        }

        //Pw(X,E)
        public static decimal SupportW(long decisionValue, IReduct reduct, EquivalenceClass eqClass)
        {
            return eqClass != null ? eqClass.AvgConfidenceWeight : 0;
        }

        // P(X|E) = P(X,E)/P(E)
        public static decimal Confidence(long decisionValue, IReduct reduct, EquivalenceClass eqClass)
        {
            if (eqClass != null && eqClass.NumberOfObjects > 0)
                return (decimal)eqClass.AvgConfidenceSum / (decimal)eqClass.NumberOfObjects;
            return 0;
        }

        // Pw(X|E) = Pw(X,E)/Pw(E)
        public static decimal ConfidenceW(long decisionValue, IReduct reduct, EquivalenceClass eqClass)
        {
            if (eqClass == null) return 0;
            decimal weightSum_XE = eqClass.AvgConfidenceWeight;
            decimal weightSum_E = eqClass.WeightSum;
            return weightSum_E != 0 ? weightSum_XE / weightSum_E : 0;
        }

        //P(E|X) = P(X,E)/P(X)
        public static decimal Coverage(long decisionValue, IReduct reduct, EquivalenceClass eqClass)
        {
            if (eqClass == null) return 0;
            int count_XE = eqClass.AvgConfidenceSum;
            if (count_XE == 0) return 0;
            int count_X = (int)reduct.DataStore.DataStoreInfo.DecisionInfo.Histogram[decisionValue];
            return count_X != 0 ? (decimal)count_XE / (decimal)count_X : 0;
        }

        //Pw(E|X) = Pw(X,E)/Pw(X)
        public static decimal CoverageW(long decisionValue, IReduct reduct, EquivalenceClass eqClass)
        {
            if (eqClass == null) return 0;
            decimal weight_XE = eqClass.AvgConfidenceWeight;
            if (weight_XE == 0) return 0;
            decimal weight_X = reduct.DataStore.DataStoreInfo.DecisionInfo.HistogramWeights[decisionValue];
            return (weight_X != 0) ? weight_XE / weight_X : 0;
        }

        //P(X,E)/P(E) / P(X) = P(X|E)/P(X)
        public static decimal Ratio(long decisionValue, IReduct reduct, EquivalenceClass eqClass)
        {
            if (eqClass == null) return 0;
            int count_XE = eqClass.AvgConfidenceSum;
            int count_E = eqClass.NumberOfObjects;
            if (count_E == 0) return 0;
            int count_X = (int)reduct.DataStore.DataStoreInfo.DecisionInfo.Histogram[decisionValue];
            return (count_E * count_X) != 0 ? ((decimal)count_XE / (decimal)(count_E * count_X)) : 0;
        }

        //Pw(X|E)/Pw(X) = Pw(X,E)/(Pw(E) * Pw(X))
        public static decimal RatioW(long decisionValue, IReduct reduct, EquivalenceClass eqClass)
        {
            if (eqClass == null) return 0;
            decimal weight_XE = eqClass.AvgConfidenceWeight;
            decimal weight_E = eqClass.WeightSum;
            if (weight_E == 0) return 0;
            decimal weight_X = reduct.DataStore.DataStoreInfo.DecisionInfo.HistogramWeights[decisionValue];
            return (weight_E * weight_X) != 0 ? weight_XE / (weight_E * weight_X) : 0;
        }

        //Pw*(X|E) = (|X * E|w/|X|w) / (sum_{i} |X_{i} * E|w / |X_{i}|w )
        public static decimal ConfidenceRelativeW(long decisionValue, IReduct reduct, EquivalenceClass eqClass)
        {
            if (eqClass == null) return 0;
            decimal sum = 0;

            //TODO should we consider all decision in case of intersections?
            foreach (long dec in reduct.ObjectSetInfo.GetDecisionValues())
            {
                decimal localWeight_X = reduct.DataStore.DataStoreInfo.DecisionInfo.HistogramWeights[dec];
                if (localWeight_X > 0)
                    sum += (eqClass.AvgConfidenceWeight / localWeight_X);
            }

            if (sum > 0)
            {
                decimal weight_X = reduct.DataStore.DataStoreInfo.DecisionInfo.HistogramWeights[decisionValue];
                if (weight_X > 0)
                    return (eqClass.AvgConfidenceWeight / weight_X) / sum;
            }

            return 0;
        }
    }
}