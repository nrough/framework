//
// NRough Framework
// http://www.nrough.net
//
// Copyright © Sebastian Widz, 2017
// authors at nrough.net

//This file is part of NRough.

//Foobar is free software; you can redistribute it and/or modify
//it under the terms of the GNU General Public License as published by
//the Free Software Foundation; either version 3 of the License, or
//(at your option) any later version.
//
//Foobar is distributed in the hope that it will be useful,
//but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
//GNU General Public License for more details.
//
//You should have received a copy of the GNU General Public License
//along with Foobar; if not, write to the Free Software
//Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA  02110-1301  USA
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NRough.MachineLearning
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
