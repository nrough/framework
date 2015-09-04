using System;


namespace Infovision.Datamining.Roughset
{
    [Serializable]
    public abstract class ReductMeasure : IReductMeasure
    {
        #region Properties 

        public abstract String FactoryKey { get; }
        
        public virtual SortDirection SortDirection
        {
            get { return SortDirection.Ascending;  }
        }

        #endregion

        #region Methods

        public abstract Double Calc(IReduct reduct);

        #endregion
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

        #endregion
        
        #region Methods

        public override Double Calc(IReduct reduct)
        {
            return (Double)reduct.AttributeSet.Count;
        }

        public override String ToString()
        {
            return "Length";
        }

        #endregion
    }

    [Serializable]
    public class ReductMeasureNumberOfPartitions : ReductMeasure
    {
        #region Properties

        public override String FactoryKey
        {
            get { return "ReductMeasureNumberOfPartitions"; }
        }
        
        public override SortDirection SortDirection
        {
            get { return SortDirection.Ascending; }
        }

        #endregion

        #region Methods

        public override Double Calc(IReduct reduct)
        {
            return (Double)reduct.EquivalenceClassMap.Count;
        }

        public override String ToString()
        {
            return "Partitions";
        }

        #endregion
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

        #endregion

        #region Methods

        public override Double Calc(IReduct reduct)
        {
            return (Double)reduct.ObjectSetInfo.NumberOfRecords / (Double)reduct.DataStore.NumberOfRecords;
        }

        public override String ToString()
        {
            return "SizeOfX";
        }

        #endregion
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

        #endregion

        #region Methods

        public override Double Calc(IReduct reduct)
        {
            Double result = 0;
            foreach (int objectIdx in reduct.ObjectSet)
            {
                Int64 decisionValue = reduct.DataStore.GetDecisionValue(objectIdx);
                result += (Double)reduct.ObjectSet.NumberOfObjectsWithDecision(decisionValue) / (Double)reduct.ObjectSetInfo.NumberOfRecords;
            }

            return result;
        }

        public override String ToString()
        {
            return "BireductRelative";
        }

        #endregion
    }
}
