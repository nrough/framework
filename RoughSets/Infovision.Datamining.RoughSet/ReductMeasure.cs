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
            get { return SortDirection.Ascending;  }
        }

        #endregion

        #region Methods

        public abstract double Calc(IReduct reduct);

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

        public override double Calc(IReduct reduct)
        {
            return (double)reduct.Attributes.Count;
        }

        public override string ToString()
        {
            return "Length";
        }

        #endregion
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

        #endregion

        #region Methods

        public override double Calc(IReduct reduct)
        {
            return (double)reduct.EquivalenceClasses.NumberOfPartitions;
        }

        public override string ToString()
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

        public override double Calc(IReduct reduct)
        {
            return (double)reduct.ObjectSetInfo.NumberOfRecords / (double)reduct.DataStore.NumberOfRecords;
        }

        public override string ToString()
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

        public override double Calc(IReduct reduct)
        {
            double result = 0;
            foreach (int objectIdx in reduct.ObjectSet)
            {
                long decisionValue = reduct.DataStore.GetDecisionValue(objectIdx);
                result += (double)reduct.ObjectSet.NumberOfObjectsWithDecision(decisionValue) / (double)reduct.ObjectSetInfo.NumberOfRecords;
            }

            return result;
        }

        public override string ToString()
        {
            return ReductFactoryKeyHelper.BireductRelative;
        }

        #endregion
    }      
}
