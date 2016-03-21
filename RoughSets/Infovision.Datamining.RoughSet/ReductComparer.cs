using System.Collections.Generic;

namespace Infovision.Datamining.Roughset
{
    public class ReductLenghtComparer : Comparer<IReduct>
    {
        public override int Compare(IReduct left, IReduct right)
        {
            if (left.Attributes.Count > right.Attributes.Count)
            {
                return 1;
            }
            else if (left.Attributes.Count < right.Attributes.Count)
            {
                return -1;
            }

            return 0;
        }
    }

    public class ReductRuleNumberComparer : Comparer<IReduct>
    {
        public override int Compare(IReduct left, IReduct right)
        {
            if (left.EquivalenceClasses.NumberOfPartitions > right.EquivalenceClasses.NumberOfPartitions)
            {
                return 1;
            }
            else if (left.EquivalenceClasses.NumberOfPartitions < right.EquivalenceClasses.NumberOfPartitions)
            {
                return -1;
            }

            return 0;
        }
    }

    public class BireductSizeComparer : Comparer<IReduct>
    {
        public override int Compare(IReduct left, IReduct right)
        {
            if (left.ObjectSetInfo.NumberOfRecords < right.ObjectSetInfo.NumberOfRecords)
            {
                return 1;
            }
            else if (left.ObjectSetInfo.NumberOfRecords > right.ObjectSetInfo.NumberOfRecords)
            {
                return -1;
            }

            return 0;
        }
    }

    public class BireductRelativeComparer : Comparer<IReduct>
    {
        BireductMeasureRelative bireductMeasureRelative = new BireductMeasureRelative();

        public override int Compare(IReduct left, IReduct right)
        {            
            decimal leftResult = bireductMeasureRelative.Calc(left);
            decimal rightResult = bireductMeasureRelative.Calc(right);

            if (leftResult < rightResult)
            {
                return 1;
            }
            else if (leftResult > rightResult)
            {
                return -1;
            }

            return 0;
        }
    }

    public class DiversityComparer : Comparer<IReduct>
    {                
        public override int Compare(IReduct left, IReduct right)
        {
            //TODO DiversityComparer.Compare(...)
            return 0;
        }
    }

    public class ReductStoreLenghtComparer : Comparer<IReductStore>
    {
        public bool IncludeExceptions { get; set; }

        public ReductStoreLenghtComparer(bool includeExceptions)
            : base()
        {
            this.IncludeExceptions = IncludeExceptions;
        }
        
        public override int Compare(IReductStore left, IReductStore right)
        {
            double avgLengthLeft = left.GetWeightedAvgMeasure(new ReductMeasureLength(), this.IncludeExceptions);
            double avgLengthRight = right.GetWeightedAvgMeasure(new ReductMeasureLength(), this.IncludeExceptions);

            if (avgLengthLeft > avgLengthRight)
            {
                return 1;
            }
            else if (avgLengthLeft < avgLengthRight)
            {
                return -1;
            }

            return 0;
        }
    }
}
