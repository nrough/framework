using System;

namespace NRough.MachineLearning.Roughsets
{
    [Serializable]
    public abstract class ReductMeasure : IReductMeasure
    {
        #region Properties

        public abstract string FactoryKey { get; }

        public virtual SortDirection SortDirection
        {
            get { return SortDirection.Ascending; }
        }

        #endregion Properties

        #region Methods

        public abstract double Calc(IReduct reduct);

        #endregion Methods
    }

    [Serializable]
    public class ReductMeasureLength : ReductMeasure
    {
        private static volatile ReductMeasureLength instance;
        private static object syncRoot = new object();

        public ReductMeasureLength()
            : base()
        {
        }

        public static ReductMeasureLength Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                            instance = new ReductMeasureLength();
                    }
                }

                return instance;
            }
        }

        #region Properties

        public override string FactoryKey
        {
            get { return "ReductMeasureLength"; }
        }

        public override SortDirection SortDirection
        {
            get { return SortDirection.Ascending; }
        }

        #endregion Properties

        #region Methods

        public override double Calc(IReduct reduct)
        {
            return (double)reduct.Attributes.Count;
        }

        public override string ToString()
        {
            return "Length";
        }

        #endregion Methods
    }

    [Serializable]
    public class ReductMeasureNumberOfPartitions : ReductMeasure
    {
        private static volatile ReductMeasureNumberOfPartitions instance;
        private static object syncRoot = new object();

        public ReductMeasureNumberOfPartitions()
            : base()
        {
        }

        public static ReductMeasureNumberOfPartitions Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                            instance = new ReductMeasureNumberOfPartitions();
                    }
                }

                return instance;
            }
        }

        #region Properties

        public override string FactoryKey
        {
            get { return "ReductMeasureNumberOfPartitions"; }
        }

        public override SortDirection SortDirection
        {
            get { return SortDirection.Ascending; }
        }

        #endregion Properties

        #region Methods

        public override double Calc(IReduct reduct)
        {
            return (double)reduct.EquivalenceClasses.Count;
        }

        public override string ToString()
        {
            return "Partitions";
        }

        #endregion Methods
    }

    [Serializable]
    public class BireductMeasureMajority : ReductMeasure
    {
        #region Properties

        public override string FactoryKey
        {
            get { return "BireductMeasureMajority"; }
        }

        public override SortDirection SortDirection
        {
            get { return SortDirection.Descending; }
        }

        #endregion Properties

        #region Methods

        public override double Calc(IReduct reduct)
        {
            return 
                (double) reduct.EquivalenceClasses.NumberOfObjects
                / (double) reduct.DataStore.NumberOfRecords;
        }

        public override string ToString()
        {
            return "SizeOfX";
        }

        #endregion Methods
    }

    [Serializable]
    public class BireductMeasureRelative : ReductMeasure
    {
        #region Properties

        public override string FactoryKey
        {
            get { return "BireductMeasureRelative"; }
        }

        public override SortDirection SortDirection
        {
            get { return SortDirection.Descending; }
        }

        #endregion Properties

        #region Methods

        public override double Calc(IReduct reduct)
        {
            double result = 0;
            foreach (int objectIdx in reduct.SupportedObjects)
            {
                long decisionValue = reduct.DataStore.GetDecisionValue(objectIdx);
                result += ((double)reduct.EquivalenceClasses.CountDecision(decisionValue)
                    / (double) reduct.EquivalenceClasses.NumberOfObjects);
            }

            return result;
        }

        public override string ToString()
        {
            return ReductTypes.BireductRelative;
        }

        #endregion Methods
    }
}