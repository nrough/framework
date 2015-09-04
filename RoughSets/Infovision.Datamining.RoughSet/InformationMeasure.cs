﻿using System;

namespace Infovision.Datamining.Roughset
{
    public enum InformationMeasureType
    {
        Positive = 0,
        Relative = 1,
        Majority = 2,
        ObjectWeights = 3
    }

    public interface IInformationMeasure
    {
        double Calc(IReduct reduct);
        string Description();
    }

    [Serializable]
    public abstract class InformationMeasureBase : IInformationMeasure
    {
        #region Methods

        public abstract double Calc(IReduct reduct);
        public abstract string Description();

        public static IInformationMeasure Construct(InformationMeasureType measureType)
        {
            IInformationMeasure roughMeasure = null;

            switch (measureType)
            {
                case InformationMeasureType.Positive :
                    roughMeasure = new InformationMeasurePositive();
                    break;

                case InformationMeasureType.Relative:
                    roughMeasure = new InformationMeasureRelative();
                    break;

                case InformationMeasureType.Majority:
                    roughMeasure = new InformationMeasureMajority();
                    break;

                case InformationMeasureType.ObjectWeights:
                    roughMeasure = new InformationMeasureWeights();
                    break;
            }

            if (roughMeasure == null)
                throw new System.InvalidOperationException();

            return roughMeasure;
        }

        #endregion
    }

    [Serializable]
    public class InformationMeasurePositive : InformationMeasureBase
    {
        #region Methods

        public override double Calc(IReduct reduct)
        {
            int result = 0;
            foreach (EquivalenceClass e in reduct.EquivalenceClassMap)
            {
                if (e.NumberOfDecisions == 1)
                {
                    result += e.NumberOfObjects;
                }
            }
            return reduct.ObjectSetInfo.NumberOfRecords != 0 ? (double)result / (double)reduct.ObjectSetInfo.NumberOfRecords : 0;
        }

        public override String Description()
        {
            return "Positive";
        }

        public override String ToString()
        {
            return "Positive";
        }

        #endregion
    }

    [Serializable]
    public class InformationMeasureRelative : InformationMeasureBase
    {
        #region Methods

        public override double Calc(IReduct reduct)
        {
            double result = 0;
            double maxDecisionProbability = -1;
            double decProbability;

            foreach (EquivalenceClass e in reduct.EquivalenceClassMap)
            {
                maxDecisionProbability = -1;
                foreach (Int64 decisionValue in e.DecisionValues)
                {
                    decProbability = e.DecisionProbability(decisionValue) / reduct.ObjectSetInfo.PriorDecisionProbability(decisionValue);

                    if ( decProbability > (maxDecisionProbability + (0.0001 / (double)reduct.ObjectSetInfo.NumberOfRecords)) )
                    {
                        maxDecisionProbability = decProbability;
                    }
                }

                result += (double)e.NumberOfObjects * maxDecisionProbability;
            }

            return result / (double)reduct.ObjectSetInfo.NumberOfRecords;
        }

        public override string Description()
        {
            return "Relative";
        }

        public override string ToString()
        {
            return "Relative";
        }

        #endregion
    }

    [Serializable]
    public class InformationMeasureMajority : InformationMeasureBase
    {
        #region Methods

        public override double Calc(IReduct reduct)
        {
            double result = 0;
            double maxDecisionProbability = -1;
            double decProbability;

            foreach (EquivalenceClass e in reduct.EquivalenceClassMap)
            {
                maxDecisionProbability = -1;
                foreach (Int64 decisionValue in e.DecisionValues)
                {
                    decProbability = e.DecisionProbability(decisionValue);

                    if (decProbability > maxDecisionProbability + (0.0001 / (double)reduct.ObjectSetInfo.NumberOfRecords))
                    {
                        maxDecisionProbability = decProbability;
                    }
                }

                result += (double)e.NumberOfObjects * maxDecisionProbability;
            }

            return result / (double)reduct.ObjectSetInfo.NumberOfRecords;
        }

        public override string Description()
        {
            return "Majority";
        }

        public override string ToString()
        {
            return "Majority";
        }

        #endregion
    }

    [Serializable]
    public class InformationMeasureWeights : InformationMeasureBase
    {
        #region Methods

        public override double Calc(IReduct reduct)
        {
            double result = 0;
            foreach (EquivalenceClass e in reduct.EquivalenceClassMap)
            {
                double maxValue = Double.MinValue;
                long maxDecision = -1;
                foreach (long decisionValue in e.DecisionValues)
                {
                    double sum = 0;
                    foreach (int objectIdx in e.GetObjectIndexes(decisionValue))
                    {
                        sum += reduct.Weights[objectIdx];
                    }
                    
                    if (sum > (maxValue + (0.0001 / (double)reduct.ObjectSetInfo.NumberOfRecords)) )
                    {
                        maxValue = sum;
                        maxDecision = decisionValue;
                    }
                } 
                                                
                result += maxValue;
            }

            return result;
        }

        public override string Description()
        {
            return "ObjectWeights";
        }

        public override string ToString()
        {
            return "ObjectWeights";
        }

        #endregion
    }
}
