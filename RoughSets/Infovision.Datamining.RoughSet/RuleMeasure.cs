using System;

namespace Infovision.Datamining.Roughset
{
    public interface IRuleMeasure
    {
        double Calc(long decisionValue, IReduct reduct, EquivalenceClass eqClass);        
        string Description();
    }

    [Serializable]
    public class RuleMeasureSupport : IRuleMeasure
    {
        //P(X,E)
        public virtual double Calc(long decisionValue, IReduct reduct, EquivalenceClass equivalenceClassInfo)
        {
            if (reduct.ObjectSetInfo.NumberOfRecords > 0)
            {
                return (double)equivalenceClassInfo.GetNumberOfObjectsWithDecision(decisionValue) / (double)reduct.ObjectSetInfo.NumberOfRecords;
            }
            return 0;
        }

        public string Description()
        {
            return "#SUPPORT";
        }
    }

    [Serializable]
    public class RuleMeasureWeightSupport : IRuleMeasure
    {
        //Pw(X,E)
        public virtual double Calc(long decisionValue, IReduct reduct, EquivalenceClass eqClass)
        {            
            return eqClass.GetWeight(decisionValue);

            /*
            double weightSum = 0;            
            foreach (int objectIndex in eqClass.GetObjectIndexes(decisionValue))
            {
                weightSum += reduct.Weights[objectIndex];
            }

            return weightSum;
            */
        }

        public string Description()
        {
            return "#SUPPORT_WEIGHT";
        }
    }

    [Serializable]
    public class RuleMeasureConfidence : IRuleMeasure
    {
        // P(X|E) = P(X,E)/P(E)
        public virtual double Calc(long decisionValue, IReduct reduct, EquivalenceClass eqClass)
        {
            if (eqClass.NumberOfObjects > 0)
            {
                return (double)eqClass.GetNumberOfObjectsWithDecision(decisionValue) / (double)eqClass.NumberOfObjects;
            }

            return 0;
        }

        public string Description()
        {
            return "#CONFIDENCE";
        }
    }

    [Serializable]
    public class RuleMeasureWeightConfidence : IRuleMeasure
    {
        // Pw(X|E) = Pw(X,E)/Pw(E)
        public virtual double Calc(long decisionValue, IReduct reduct, EquivalenceClass eqClass)
        {
            double weightSum_XE = eqClass.GetWeight(decisionValue);                       
            double weightSum_E = eqClass.GetWeight();            
            return weightSum_E != 0.0 ? weightSum_XE / weightSum_E : 0.0;
        }

        public string Description()
        {
            return "#WEIGHT_CONFIDENCE";
        }
    }

    [Serializable]
    public class RuleMeasureCoverage : IRuleMeasure
    {
        //P(E|X) = P(X,E)/P(X)
        public virtual double Calc(long decisionValue, IReduct reduct, EquivalenceClass eqClass)
        {
            if (reduct.ObjectSetInfo.NumberOfObjectsWithDecision(decisionValue) > 0)
            {
                return (double)eqClass.GetNumberOfObjectsWithDecision(decisionValue)
                     / (double)reduct.ObjectSetInfo.NumberOfObjectsWithDecision(decisionValue);
            }

            return 0;
        }

        public string Description()
        {
            return "#COVERAGE";
        }
    }

    [Serializable]
    public class RuleMeasureWeightCoverage : IRuleMeasure
    {
        //Pw(E|X) = Pw(X,E)/Pw(X)
        public virtual double Calc(long decisionValue, IReduct reduct, EquivalenceClass eqClass)
        {
            double weightSum_XE = eqClass.GetWeight(decisionValue);            
            double weightSum_X = 0.0;
            foreach (int objectIndex in reduct.DataStore.GetObjectIndexes(decisionValue))
                weightSum_X += reduct.Weights[objectIndex];

            return weightSum_X != 0 ? weightSum_XE / weightSum_X : 0;
        }

        public string Description()
        {
            return "#WEIGHT_COVERAGE";
        }
    }

    [Serializable]
    public class RuleMeasureRatio : IRuleMeasure
    {
        //P(X,E)/P(E) / P(X) = P(X|E)/P(X)
        public virtual double Calc(long decisionValue, IReduct reduct, EquivalenceClass eqClass)
        {
            double result = 0.0;
            if (reduct.ObjectSetInfo.NumberOfObjectsWithDecision(decisionValue) > 0
                && eqClass.NumberOfObjects > 0
                && reduct.ObjectSetInfo.NumberOfRecords > 0)
            {
                result = ((double)eqClass.GetNumberOfObjectsWithDecision(decisionValue) / (double)eqClass.NumberOfObjects)
                     / ((double)reduct.ObjectSetInfo.NumberOfObjectsWithDecision(decisionValue) / (double)reduct.ObjectSetInfo.NumberOfRecords);

                return result;
            }

            return 0;
        }

        public string Description()
        {
            return "#RATIO";
        }
    }

    [Serializable]
    public class RuleMeasureWeightRatio : IRuleMeasure
    {
        //Pw(X|E)/Pw(X) = Pw(X,E)/(Pw(E) * Pw(X))
        public virtual double Calc(long decisionValue, IReduct reduct, EquivalenceClass eqClass)
        {
            double weightSum_XE = eqClass.GetWeight(decisionValue);            
            double weightSum_E = eqClass.GetWeight();            
            double weightSum_X = 0;
            foreach (int objectIndex in reduct.DataStore.GetObjectIndexes(decisionValue))
                weightSum_X += reduct.Weights[objectIndex];

            return (weightSum_E * weightSum_X) != 0 ? weightSum_XE / (weightSum_E * weightSum_X) : 0;
        }

        public string Description()
        {
            return "#WEIGHT_RATIO";
        }
    }

    [Serializable]
    public class RuleMeasureStrenght : IRuleMeasure
    {
        //P(E)
        public virtual double Calc(long decisionValue, IReduct reduct, EquivalenceClass eqClass)
        {
            if (reduct.ObjectSetInfo.NumberOfRecords > 0)
                return (double)eqClass.NumberOfObjects / (double)reduct.ObjectSetInfo.NumberOfRecords;
            return 0;
        }

        public string Description()
        {
            return "#STRENGH";
        }
    }

    [Serializable]
    public class RuleMeasureWeightStrenght : IRuleMeasure
    {
        //Pw(E)
        public virtual double Calc(long decisionValue, IReduct reduct, EquivalenceClass eqClass)
        {
            double weightSum_E = eqClass.GetWeight();
            
            /*
            double weightSum_E = 0;
            foreach (int objectIndex in eqClass.ObjectIndexes)
            {
                weightSum_E += reduct.Weights[objectIndex];
            }
            */

            return weightSum_E;
        }

        public string Description()
        {
            return "#WEIGHT_STRENGH";
        }
    }

    [Serializable]
    public class RuleMeasureConfidenceRelative : IRuleMeasure
    {
        //P*(X|E) = (|X & E|/|X|) / (sum_{i} |X_{i} & E| / |X_{i}| )
        public virtual double Calc(long decisionValue, IReduct reduct, EquivalenceClass eqClass)
        {
            double sum = 0;
            foreach (long decision in reduct.ObjectSetInfo.GetDecisionValues())
            {
                if (reduct.ObjectSetInfo.NumberOfObjectsWithDecision(decision) > 0)
                {
                    sum += (double)eqClass.GetNumberOfObjectsWithDecision(decision)
                            / (double)reduct.ObjectSetInfo.NumberOfObjectsWithDecision(decision);
                }
            }

            if (reduct.ObjectSetInfo.NumberOfObjectsWithDecision(decisionValue) > 0 
                && sum > 0)
            {
                return ((double)eqClass.GetNumberOfObjectsWithDecision(decisionValue)
                        / (double)reduct.ObjectSetInfo.NumberOfObjectsWithDecision(decisionValue))
                        / (double)sum;
            }

            return 0;
        }

        public string Description()
        {
            return "#CONFIDENCE_RELATIVE";
        }
    }
}