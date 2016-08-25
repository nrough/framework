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
        public abstract decimal Calc(EquivalenceClassCollection equivalenceClassCollection);

        public abstract string Description();

        public static IInformationMeasure Construct(InformationMeasureType measureType)
        {
            IInformationMeasure roughMeasure = null;

            switch (measureType)
            {
                case InformationMeasureType.Positive:
                    roughMeasure = InformationMeasurePositive.Instance;;
                    break;

                case InformationMeasureType.Relative:
                    roughMeasure = InformationMeasureRelative.Instance;;
                    break;

                case InformationMeasureType.Majority:
                    roughMeasure = InformationMeasureMajority.Instance;;
                    break;

                case InformationMeasureType.ObjectWeights:
                    roughMeasure = InformationMeasureWeights.Instance;
                    break;
            }

            if (roughMeasure == null)
                throw new InvalidOperationException();

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
        private static volatile InformationMeasurePositive instance = null;
        private static object syncRoot = new object();

        public static InformationMeasurePositive Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                        {
                            instance = new InformationMeasurePositive();
                        }
                    }
                }

                return instance;
            }
        }

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

        public override decimal Calc(EquivalenceClassCollection equivalenceClassCollection)
        {
            decimal result = 0;
            foreach (EquivalenceClass e in equivalenceClassCollection)
                if (e.NumberOfDecisions == 1)
                    result += e.NumberOfObjects;

            return (equivalenceClassCollection.NumberOfObjects != 0)
                ? result / (decimal)equivalenceClassCollection.NumberOfObjects
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
        private static volatile InformationMeasureRelative instance = null;
        private static object syncRoot = new object();

        public static InformationMeasureRelative Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                        {
                            instance = new InformationMeasureRelative();
                        }
                    }
                }

                return instance;
            }
        }

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
                    relativeCount = (decimal)e.GetNumberOfObjectsWithDecision(decisionValue) 
                        / reduct.DataStore.DataStoreInfo.DecisionInfo.Histogram[decisionValue];

                    if (relativeCount > maxValue)
                        maxValue = relativeCount;
                }

                result += maxValue;
            }

            return result / (decimal)reduct.DataStore.DataStoreInfo.NumberOfDecisionValues;
        }

        public override decimal Calc(EquivalenceClassCollection equivalenceClassCollection)
        {
            decimal result = Decimal.Zero;
            decimal maxValue, relativeCount;

            foreach (EquivalenceClass e in equivalenceClassCollection)
            {
                maxValue = Decimal.MinValue;
                foreach (long decisionValue in e.DecisionValues)
                {
                    relativeCount = (decimal)e.GetNumberOfObjectsWithDecision(decisionValue) 
                        / equivalenceClassCollection.Data.DataStoreInfo.DecisionInfo.Histogram[decisionValue];

                    if (relativeCount > maxValue)
                        maxValue = relativeCount;
                }

                result += maxValue;
            }

            return result / equivalenceClassCollection.Data.DataStoreInfo.NumberOfDecisionValues;
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
        private static volatile InformationMeasureMajority instance = null;
        private static object syncRoot = new object();

        public static InformationMeasureMajority Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                        {
                            instance = new InformationMeasureMajority();
                        }
                    }
                }

                return instance;
            }
        }

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

        public override decimal Calc(EquivalenceClassCollection equivalenceClassCollection)
        {
            decimal result = Decimal.Zero;
            int maxDecisionProbability = Int32.MinValue;
            int majorCount;
            foreach (EquivalenceClass e in equivalenceClassCollection)
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
            return result / (decimal)equivalenceClassCollection.NumberOfObjects;
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
        private static volatile InformationMeasureWeights instance = null;
        private static object syncRoot = new object();

        public static InformationMeasureWeights Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                        {
                            instance = new InformationMeasureWeights();
                        }
                    }
                }

                return instance;
            }
        }

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
                    sum = e.GetDecisionWeight(decisionValue);
                    if (sum > maxValue)
                        maxValue = sum;
                }
                result += maxValue;
            }

            return Decimal.Round(result, 17);
        }

        public override decimal Calc(EquivalenceClassCollection equivalenceClassCollection)
        {
            decimal result = Decimal.Zero;
            decimal maxValue, sum;
            foreach (EquivalenceClass e in equivalenceClassCollection)
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

        #endregion Methods
    }
}