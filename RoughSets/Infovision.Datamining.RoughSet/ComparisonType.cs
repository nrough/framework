using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infovision.Datamining.Roughset
{
    public enum ComparisonType
    {
        None = 0,
        EqualTo = 1,
        NotEqualTo = 2,
        LessThan = 3,
        LessThanOrEqualTo = 4,
        GreaterThan = 5,
        GreaterThanOrEqualTo = 6
    }

    public static class ComparisonTypeExternsions
    {        
        public static string ToSymbol(this ComparisonType comparison)
        {
            switch (comparison)
            {
                case ComparisonType.EqualTo:
                    return "==";

                case ComparisonType.NotEqualTo:
                    return "!=";

                case ComparisonType.LessThan:
                    return "<";

                case ComparisonType.LessThanOrEqualTo:
                    return "<=";

                case ComparisonType.GreaterThan:
                    return ">";

                case ComparisonType.GreaterThanOrEqualTo:
                    return ">=";
                               
                default:
                    throw new NotImplementedException("Comparison type not implemented");
            }
        }

        public static ComparisonType Complement(this ComparisonType comparison)
        {
            switch (comparison)
            {
                case ComparisonType.EqualTo:
                    return ComparisonType.NotEqualTo;

                case ComparisonType.NotEqualTo:
                    return ComparisonType.EqualTo;

                case ComparisonType.LessThan:
                    return ComparisonType.GreaterThanOrEqualTo;

                case ComparisonType.LessThanOrEqualTo:
                    return ComparisonType.GreaterThan;

                case ComparisonType.GreaterThan:
                    return ComparisonType.LessThanOrEqualTo;

                case ComparisonType.GreaterThanOrEqualTo:
                    return ComparisonType.LessThan;

                default:
                    throw new NotImplementedException("Comparison type not implemented");
            }
        }
    }
}
