using System;

namespace NRough.MachineLearning.Roughsets
{
    public delegate double FMeasure(EquivalenceClassCollection eqClasses);

    public static class FMeasures
    {
        public static double Majority(EquivalenceClassCollection eqClasses)
        {
            return InformationMeasureMajority.Instance.Calc(eqClasses);
        }

        public static double Relative(EquivalenceClassCollection eqClasses)
        {
            return InformationMeasureRelative.Instance.Calc(eqClasses);
        }

        public static double Gamma(EquivalenceClassCollection eqClasses)
        {
            return InformationMeasurePositive.Instance.Calc(eqClasses);
        }

        public static double MajorityWeighted(EquivalenceClassCollection eqClasses)
        {
            return InformationMeasureWeights.Instance.Calc(eqClasses);
        }
    }

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
        double Calc(EquivalenceClassCollection eqClasses);

        string Description();
    }

    [Serializable]
    public abstract class InformationMeasureBase : IInformationMeasure, IReductMeasure
    {
        public string FactoryKey { get { return this.Description(); } }

        #region Methods

        public abstract double Calc(IReduct reduct);
        public abstract double Calc(EquivalenceClassCollection equivalenceClassCollection);

        public abstract string Description();

        public static IInformationMeasure Construct(InformationMeasureType measureType)
        {
            IInformationMeasure roughMeasure = null;

            switch (measureType)
            {
                case InformationMeasureType.Positive:
                    roughMeasure = InformationMeasurePositive.Instance;
                    break;

                case InformationMeasureType.Relative:
                    roughMeasure = InformationMeasureRelative.Instance;
                    break;

                case InformationMeasureType.Majority:
                    roughMeasure = InformationMeasureMajority.Instance;
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
        private static readonly object syncRoot = new object();

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

        public override double Calc(IReduct reduct)
        {
            double result = 0;
            foreach (EquivalenceClass e in reduct.EquivalenceClasses)
                if (e.NumberOfDecisions == 1)
                    result += e.NumberOfObjects;

            return (reduct.EquivalenceClasses.NumberOfObjects != 0)
                ? result / reduct.EquivalenceClasses.NumberOfObjects
                : 0.0;
        }

        public override double Calc(EquivalenceClassCollection equivalenceClassCollection)
        {
            double result = 0;
            foreach (EquivalenceClass e in equivalenceClassCollection)
                if (e.NumberOfDecisions == 1)
                    result += e.NumberOfObjects;

            return (equivalenceClassCollection.NumberOfObjects != 0)
                ? result / equivalenceClassCollection.NumberOfObjects
                : 0.0;
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
        private static readonly object syncRoot = new object();

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

        public override double Calc(IReduct reduct)
        {
            double result = 0.0;
            double maxValue, relativeCount;

            foreach (EquivalenceClass e in reduct.EquivalenceClasses)
            {
                maxValue = Double.MinValue;
                foreach (long decisionValue in e.DecisionValues)
                {                    
                    relativeCount =
                        (double)e.GetNumberOfObjectsWithDecision(decisionValue) /
                            reduct.DataStore.DataStoreInfo.DecisionInfo.Histogram[decisionValue];

                    if (relativeCount > maxValue)
                        maxValue = relativeCount;
                }

                result += maxValue;
            }

            return result / (double)reduct.DataStore.DataStoreInfo.NumberOfDecisionValues;
        }

        public override double Calc(EquivalenceClassCollection equivalenceClassCollection)
        {
            double result = 0.0;
            double maxValue, relativeCount;

            foreach (EquivalenceClass e in equivalenceClassCollection)
            {
                maxValue = Double.MinValue;
                foreach (long decisionValue in e.DecisionValues)
                {
                    relativeCount = (double)e.GetNumberOfObjectsWithDecision(decisionValue) 
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
        private static readonly object syncRoot = new object();

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

        public override double Calc(IReduct reduct)
        {
            double result = 0.0;
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
                result += maxDecisionProbability;
            }
            return result / (double)reduct.DataStore.NumberOfRecords;
        }

        public override double Calc(EquivalenceClassCollection equivalenceClassCollection)
        {
            double result = 0.0;
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
                result += (double)maxDecisionProbability;
            }
            return result / (double)equivalenceClassCollection.NumberOfObjects;
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
        private static readonly object syncRoot = new object();

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

        public override double Calc(IReduct reduct)
        {
            double result = 0.0;
            double maxValue, sum;
            foreach (EquivalenceClass e in reduct.EquivalenceClasses)
            {
                maxValue = Double.MinValue;
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

        public override double Calc(EquivalenceClassCollection equivalenceClassCollection)
        {
            double result = 0.0;
            double maxValue, sum;
            foreach (EquivalenceClass e in equivalenceClassCollection)
            {
                maxValue = Double.MinValue;
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