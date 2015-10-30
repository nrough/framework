using System;

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
        decimal Calc(IReduct reduct);
        string Description();
    }

    [Serializable]
    public abstract class InformationMeasureBase : IInformationMeasure, IReductMeasure
    {
        public string FactoryKey { get { return this.Description(); } }
        
        #region Methods

        public abstract decimal Calc(IReduct reduct);
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

        public SortDirection SortDirection
        {
            get { return SortDirection.Descending; }
        }

        #endregion
    }

    [Serializable]
    public class InformationMeasurePositive : InformationMeasureBase
    {
        #region Methods

        public override decimal Calc(IReduct reduct)
        {
            decimal result = 0;
            foreach (EquivalenceClass e in reduct.EquivalenceClasses)
                if (e.NumberOfDecisions == 1)
                    result += e.NumberOfObjects;

            return (reduct.ObjectSetInfo.NumberOfRecords != 0) 
                ? result / (decimal)reduct.ObjectSetInfo.NumberOfRecords 
                : Decimal.Zero;
        }

        public override string Description()
        {
            return "Positive";
        }

        public override string ToString()
        {
            return "Positive";
        }

        #endregion
    }

    [Serializable]
    public class InformationMeasureRelative : InformationMeasureBase
    {
        #region Methods

        public override decimal Calc(IReduct reduct)
        {
            decimal result = Decimal.Zero;
            decimal maxValue, decProbability;            

            foreach (EquivalenceClass e in reduct.EquivalenceClasses)
            {
                maxValue = Decimal.MinValue;
                foreach (long decisionValue in e.DecisionValues)
                {
                    decProbability = Decimal.Divide(e.GetDecisionProbability(decisionValue), 
                                                    reduct.ObjectSetInfo.PriorDecisionProbability(decisionValue));
                    
                    if (Decimal.Round(decProbability, 17) > Decimal.Round(maxValue, 17))
                        maxValue = decProbability;
                }

                result += e.NumberOfObjects * maxValue;
            }

            return Decimal.Round(result / (decimal)reduct.ObjectSetInfo.NumberOfRecords, 17);
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

        public override decimal Calc(IReduct reduct)
        {
            decimal result = Decimal.Zero;
            decimal maxDecisionProbability = Decimal.MinValue;
            decimal decProbability;
            foreach (EquivalenceClass e in reduct.EquivalenceClasses)
            {
                maxDecisionProbability = Decimal.MinValue;
                foreach (long decisionValue in e.DecisionValues)
                {
                    decProbability = e.GetDecisionProbability(decisionValue);
                    if (Decimal.Round(decProbability, 17) > Decimal.Round(maxDecisionProbability, 17))
                        maxDecisionProbability = decProbability;
                }
                result += e.NumberOfObjects * maxDecisionProbability;
            }
            return Decimal.Round(result / (decimal)reduct.ObjectSetInfo.NumberOfRecords, 17);
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

        public override decimal Calc(IReduct reduct)
        {
            decimal result = Decimal.Zero;
            decimal maxValue, sum;
            
            foreach (EquivalenceClass e in reduct.EquivalenceClasses)
            {
                maxValue = Decimal.MinValue;
                foreach (long decisionValue in e.DecisionValues)
                {
                    sum = Decimal.Zero;
                    foreach (int objectIdx in e.GetObjectIndexes(decisionValue))
                        sum += reduct.Weights[objectIdx];
                    if (sum > maxValue)
                        maxValue = sum;
                }
                result += maxValue;
            }
            return Decimal.Round(result, 17);            
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
