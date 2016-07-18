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
                case InformationMeasureType.Positive:
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

        #endregion Methods
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

        #endregion Methods
    }

    [Serializable]
    public class InformationMeasureRelative : InformationMeasureBase
    {
        #region Methods

        public override decimal Calc(IReduct reduct)
        {
            decimal result = Decimal.Zero;
            decimal maxValue, relativeCount;

            foreach (EquivalenceClass e in reduct.EquivalenceClasses)
            {
                maxValue = Decimal.MinValue;
                foreach (long decisionValue in e.DecisionValues)
                {
                    //relativeCount = (decimal) e.GetNumberOfObjectsWithDecision(decisionValue) / (decimal)reduct.EquivalenceClasses.CountDecision(decisionValue);
                    relativeCount = (decimal)e.GetNumberOfObjectsWithDecision(decisionValue) / reduct.DataStore.DataStoreInfo.DecisionInfo.Histogram[decisionValue];

                    if (relativeCount > maxValue)
                        maxValue = relativeCount;
                }

                result += maxValue;
            }

            return result / (decimal)reduct.DataStore.DataStoreInfo.NumberOfDecisionValues;
        }

        public override string Description()
        {
            return "Relative";
        }

        public override string ToString()
        {
            return "Relative";
        }

        #endregion Methods
    }

    [Serializable]
    public class InformationMeasureMajority : InformationMeasureBase
    {
        #region Methods

        public override decimal Calc(IReduct reduct)
        {
            decimal result = Decimal.Zero;
            int maxDecisionProbability = Int32.MinValue;
            int majorCount;
            foreach (EquivalenceClass e in reduct.EquivalenceClasses)
            {
                maxDecisionProbability = Int32.MinValue;
                foreach (long decisionValue in e.DecisionValues)
                {
                    majorCount = e.GetNumberOfObjectsWithDecision(decisionValue);
                    if (majorCount > maxDecisionProbability)
                        maxDecisionProbability = majorCount;
                }
                result += (decimal)maxDecisionProbability;
            }
            return result / (decimal)reduct.DataStore.NumberOfRecords;
        }

        public override string Description()
        {
            return "Majority";
        }

        public override string ToString()
        {
            return "Majority";
        }

        #endregion Methods
    }

    [Serializable]
    public class InformationMeasureWeights : InformationMeasureBase
    {
        #region Methods

        public override decimal Calc(IReduct reduct)
        {
            decimal result = Decimal.Zero;
            result = Decimal.Zero;
            decimal maxValue, sum;
            foreach (EquivalenceClass e in reduct.EquivalenceClasses)
            {
                maxValue = Decimal.MinValue;
                foreach (long decisionValue in e.DecisionValues)
                {
                    sum = e.GetDecisionWeight(decisionValue);
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

        #endregion Methods
    }
}