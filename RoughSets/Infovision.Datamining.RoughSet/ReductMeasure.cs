using System;

namespace Infovision.Datamining.Roughset
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

        public abstract decimal Calc(IReduct reduct);

        #endregion Methods
    }

    [Serializable]
    public class ReductMeasureLength : ReductMeasure
    {
        #region Properties

        public override string FactoryKey
        {
            get { return "ReductMeasureLength"; }
        }

        public override SortDirection SortDirection
        {
            get { return Roughset.SortDirection.Ascending; }
        }

        #endregion Properties

        #region Methods

        public override decimal Calc(IReduct reduct)
        {
            return (decimal)reduct.Attributes.Count;
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

        public override decimal Calc(IReduct reduct)
        {
            return (decimal)reduct.EquivalenceClasses.NumberOfPartitions;
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

        public override decimal Calc(IReduct reduct)
        {
            return Decimal.Divide(
                reduct.ObjectSetInfo.NumberOfRecords,
                reduct.DataStore.NumberOfRecords);
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

        public override decimal Calc(IReduct reduct)
        {
            decimal result = 0;
            foreach (int objectIdx in reduct.ObjectSet)
            {
                long decisionValue = reduct.DataStore.GetDecisionValue(objectIdx);
                result += Decimal.Divide(
                    reduct.ObjectSet.NumberOfObjectsWithDecision(decisionValue),
                    reduct.ObjectSetInfo.NumberOfRecords);
            }

            return result;
        }

        public override string ToString()
        {
            return ReductFactoryKeyHelper.BireductRelative;
        }

        #endregion Methods
    }
}