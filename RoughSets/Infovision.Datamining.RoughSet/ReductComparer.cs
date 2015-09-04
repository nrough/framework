using System.Collections.Generic;

namespace Infovision.Datamining.Roughset
{
    public class ReductLenghtComparer : Comparer<IReduct>
    {
        public override int Compare(IReduct left, IReduct right)
        {
            if (left.AttributeSet.Count > right.AttributeSet.Count)
            {
                return 1;
            }
            else if (left.AttributeSet.Count < right.AttributeSet.Count)
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
            if (left.EquivalenceClassMap.Count > right.EquivalenceClassMap.Count)
            {
                return 1;
            }
            else if (left.EquivalenceClassMap.Count < right.EquivalenceClassMap.Count)
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
        public override int Compare(IReduct left, IReduct right)
        {
            BireductMeasureRelative bireductMeasureRelative = new BireductMeasureRelative();
            double leftResult = bireductMeasureRelative.Calc(left);
            double rightResult = bireductMeasureRelative.Calc(right);

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
}
