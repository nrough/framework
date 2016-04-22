using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Infovision.Datamining.Roughset
{
    public delegate decimal RuleQualityFunction(long decisionValue, IReduct reduct, EquivalenceClass eqClass);
    
    public static class RuleQuality_DEL
    {
        //P(X,E)
        public static decimal Support(long decisionValue, IReduct reduct, EquivalenceClass eqClass)
        {
            if (eqClass != null && reduct.ObjectSetInfo.NumberOfRecords > 0)
                return (decimal)eqClass.GetNumberOfObjectsWithDecision(decisionValue) / (decimal)reduct.ObjectSetInfo.NumberOfRecords;
            return 0;
        }

        //Pw(X,E)
        public static decimal SupportW(long decisionValue, IReduct reduct, EquivalenceClass eqClass)
        {
            return eqClass != null ? eqClass.GetDecisionWeigth(decisionValue) : 0;            
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
            if (eqClass != null)
            {
                decimal weightSum_XE = eqClass.GetDecisionWeigth(decisionValue);
                decimal weightSum_E = eqClass.WeightSum;
                return weightSum_E != 0 ? weightSum_XE / weightSum_E : 0;
            }
            return 0;
        }

        //P(E|X) = P(X,E)/P(X)
        public static decimal Coverage(long decisionValue, IReduct reduct, EquivalenceClass eqClass)
        {            
            if (eqClass != null && reduct.ObjectSetInfo.NumberOfObjectsWithDecision(decisionValue) > 0)
            {
                return (decimal)eqClass.GetNumberOfObjectsWithDecision(decisionValue)
                     / (decimal)reduct.ObjectSetInfo.NumberOfObjectsWithDecision(decisionValue);
            }
            return 0;
        }

        //Pw(E|X) = Pw(X,E)/Pw(X)
        public static decimal CoverageW(long decisionValue, IReduct reduct, EquivalenceClass eqClass)
        {
            if (eqClass == null)
                return 0;

            decimal weightSum_XE = eqClass.GetDecisionWeigth(decisionValue);
            if (weightSum_XE == 0)
                return 0;

            decimal weightSum_X = 0;            
            foreach (int objectIndex in reduct.DataStore.GetObjectIndexes(decisionValue))
                weightSum_X += reduct.Weights[objectIndex];                

            return (weightSum_X != 0) ? weightSum_XE / weightSum_X : 0;
        }

        //P(X,E)/P(E) / P(X) = P(X|E)/P(X)
        public static decimal Ratio(long decisionValue, IReduct reduct, EquivalenceClass eqClass)
        {
            if (eqClass == null)
                return 0;

            decimal result = 0;
            if (reduct.ObjectSetInfo.NumberOfObjectsWithDecision(decisionValue) > 0
                && eqClass.NumberOfObjects > 0
                && reduct.ObjectSetInfo.NumberOfRecords > 0)
            {
                result = ((decimal)eqClass.GetNumberOfObjectsWithDecision(decisionValue) / (decimal)eqClass.NumberOfObjects)
                     / ((decimal)reduct.ObjectSetInfo.NumberOfObjectsWithDecision(decisionValue) / (decimal)reduct.ObjectSetInfo.NumberOfRecords);

                return result;
            }

            return 0;
        }

        //Pw(X|E)/Pw(X) = Pw(X,E)/(Pw(E) * Pw(X))
        public static decimal RatioW(long decisionValue, IReduct reduct, EquivalenceClass eqClass)
        {
            if (eqClass == null)
                return 0;

            decimal weightSum_XE = eqClass.GetDecisionWeigth(decisionValue);
            decimal weightSum_E = eqClass.WeightSum;
            decimal weightSum_X = 0;

            if (weightSum_E == Decimal.Zero)
                return 0;

            //TODO DataStore -> ObjectSet 
            // Not correct way. For bireducts and intersecting reducts we need to base all calculation on Eq Class Collection
            foreach (int objectIndex in reduct.DataStore.GetObjectIndexes(decisionValue))
                weightSum_X += reduct.Weights[objectIndex];

            return (weightSum_E * weightSum_X) != 0 ? weightSum_XE / (weightSum_E * weightSum_X) : 0;            
        }

        //P(E)
        public static decimal Strength(long decisionValue, IReduct reduct, EquivalenceClass eqClass)
        {
            if (eqClass == null)
                return 0;

            if (reduct.ObjectSetInfo.NumberOfRecords > 0)
                return (decimal)eqClass.NumberOfObjects / (decimal)reduct.ObjectSetInfo.NumberOfRecords;
            return 0;
        }

        //Pw(E)
        public static decimal StrengthW(long decisionValue, IReduct reduct, EquivalenceClass eqClass)
        {
            if (eqClass == null)
                return 0;
            decimal weightSum_E = eqClass.WeightSum;
            return weightSum_E;
        }

        //P*(X|E) = (|X & E|/|X|) / (sum_{i} |X_{i} & E| / |X_{i}| )
        public static decimal ConfidenceRelative(long decisionValue, IReduct reduct, EquivalenceClass eqClass)
        {
            if (eqClass == null)
                return 0;

            decimal sum = 0;
            foreach (long decision in reduct.ObjectSetInfo.GetDecisionValues())
            {
                if (reduct.ObjectSetInfo.NumberOfObjectsWithDecision(decision) > 0)
                {
                    sum += (decimal)eqClass.GetNumberOfObjectsWithDecision(decision)
                            / (decimal)reduct.ObjectSetInfo.NumberOfObjectsWithDecision(decision);
                }
            }

            if (reduct.ObjectSetInfo.NumberOfObjectsWithDecision(decisionValue) > 0
                && sum > 0)
            {
                return ((decimal)eqClass.GetNumberOfObjectsWithDecision(decisionValue)
                        / (decimal)reduct.ObjectSetInfo.NumberOfObjectsWithDecision(decisionValue))
                        / sum;
            }

            return 0;
        }

        //Used for rule voting, returns one
        public static decimal SingleVote(long decisionValue, IReduct reduct, EquivalenceClass eqClass)
        {
            return Decimal.One;
        }
    }

    public static class RuleQuality
    {
        //P(X,E)
        public static decimal Support2(long decisionValue, IReduct reduct, EquivalenceClass eqClass)
        {
            int count_U = reduct.EquivalenceClasses.CountUniverse();
            if (eqClass != null && count_U > 0)
                return (decimal)eqClass.GetNumberOfObjectsWithDecision(decisionValue) / (decimal)count_U;
            return 0;
        }

        //Pw(X,E)
        public static decimal SupportW2(long decisionValue, IReduct reduct, EquivalenceClass eqClass)
        {
            return eqClass != null ? eqClass.GetDecisionWeigth(decisionValue) : 0;
        }

        // P(X|E) = P(X,E)/P(E)
        public static decimal Confidence2(long decisionValue, IReduct reduct, EquivalenceClass eqClass)
        {
            if (eqClass != null && eqClass.NumberOfObjects > 0)
                return (decimal)eqClass.GetNumberOfObjectsWithDecision(decisionValue) / (decimal)eqClass.NumberOfObjects;
            return 0;
        }

        // Pw(X|E) = Pw(X,E)/Pw(E)
        public static decimal ConfidenceW2(long decisionValue, IReduct reduct, EquivalenceClass eqClass)
        {
            if (eqClass != null)
            {
                decimal weightSum_XE = eqClass.GetDecisionWeigth(decisionValue);
                decimal weightSum_E = eqClass.WeightSum;
                return weightSum_E != 0 ? weightSum_XE / weightSum_E : 0;
            }
            return 0;
        }

        //P(E|X) = P(X,E)/P(X)
        public static decimal Coverage2(long decisionValue, IReduct reduct, EquivalenceClass eqClass)
        {
            int count_X = reduct.ObjectSetInfo.NumberOfObjectsWithDecision(decisionValue);
            if (eqClass != null && count_X > 0)
            {
                return (decimal)eqClass.GetNumberOfObjectsWithDecision(decisionValue) / (decimal)count_X;
            }
            return 0;
        }

        //Pw(E|X) = Pw(X,E)/Pw(X)
        public static decimal CoverageW2(long decisionValue, IReduct reduct, EquivalenceClass eqClass)
        {
            if (eqClass == null) return 0;
            decimal weight_XE = eqClass.GetDecisionWeigth(decisionValue);
            if (weight_XE == 0) return 0;
            decimal weight_X = reduct.EquivalenceClasses.CountWeightDecision(decisionValue);
            return (weight_X != 0) ? weight_XE / weight_X : 0;
        }

        //P(X,E)/P(E) / P(X) = P(X|E)/P(X)
        public static decimal Ratio2(long decisionValue, IReduct reduct, EquivalenceClass eqClass)
        {
            if (eqClass == null) return 0;
            decimal count_XE = eqClass.GetNumberOfObjectsWithDecision(decisionValue);
            decimal count_E = eqClass.NumberOfObjects;
            if (count_E == 0) return 0;
            decimal count_X = reduct.EquivalenceClasses.CountWeightDecision(decisionValue);
            return (count_E * count_X) != 0 ? ((decimal)count_XE / (decimal)(count_E * count_X)) : 0;
        }

        //Pw(X|E)/Pw(X) = Pw(X,E)/(Pw(E) * Pw(X))
        public static decimal RatioW2(long decisionValue, IReduct reduct, EquivalenceClass eqClass)
        {
            if (eqClass == null) return 0;
            decimal weight_XE = eqClass.GetDecisionWeigth(decisionValue);
            decimal weight_E = eqClass.WeightSum;
            if (weight_E == 0) return 0;
            decimal weight_X = reduct.EquivalenceClasses.CountWeightDecision(decisionValue);
            return (weight_E * weight_X) != 0 ? weight_XE / (weight_E * weight_X) : 0;
        }

        //P(E)
        public static decimal Strength2(long decisionValue, IReduct reduct, EquivalenceClass eqClass)
        {
            if (eqClass == null) return 0;
            int count_U = reduct.EquivalenceClasses.CountUniverse();
            if (count_U > 0)
                return (decimal)eqClass.NumberOfObjects / (decimal)count_U;
            return 0;
        }

        //Pw(E)
        public static decimal StrengthW2(long decisionValue, IReduct reduct, EquivalenceClass eqClass)
        {
            if (eqClass == null) return 0;
            return eqClass.WeightSum;            
        }

        //P*(X|E) = (|X & E|/|X|) / (sum_{i} |X_{i} & E| / |X_{i}| )
        public static decimal ConfidenceRelative2(long decisionValue, IReduct reduct, EquivalenceClass eqClass)
        {
            if (eqClass == null) return 0;
            decimal sum = 0;
            foreach (long dec in reduct.ObjectSetInfo.GetDecisionValues())
            {
                int localCount_X = reduct.EquivalenceClasses.CountDecision(dec);
                if (localCount_X > 0)
                    sum += ((decimal)eqClass.GetNumberOfObjectsWithDecision(dec) / (decimal)localCount_X);
            }

            if (sum > 0)
            {
                int count_X = reduct.EquivalenceClasses.CountDecision(decisionValue);
                if (count_X > 0)
                    return ((decimal)eqClass.GetNumberOfObjectsWithDecision(decisionValue) / (decimal)count_X) / sum;
            }

            return 0;
        }

        public static decimal ConfidenceRelativeW2(long decisionValue, IReduct reduct, EquivalenceClass eqClass)
        {
            if (eqClass == null) return 0;
            decimal sum = 0;
            foreach (long dec in reduct.ObjectSetInfo.GetDecisionValues()) //TODO should we consider all decision in case of intersections
            {
                decimal localWeight_X = reduct.EquivalenceClasses.CountWeightDecision(dec);
                if (localWeight_X > 0)
                    sum += (eqClass.GetDecisionWeigth(dec) / localWeight_X);
            }

            if (sum > 0)
            {
                decimal weight_X = reduct.EquivalenceClasses.CountWeightDecision(decisionValue);
                if (weight_X > 0)
                    return (eqClass.GetDecisionWeigth(decisionValue) / weight_X) / sum;
            }

            return 0;
        }

        //Used for rule voting, returns one
        public static decimal SingleVote2(long decisionValue, IReduct reduct, EquivalenceClass eqClass)
        {
            return Decimal.One;
        }
    }
}