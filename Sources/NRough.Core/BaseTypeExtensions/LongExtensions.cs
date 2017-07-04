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

namespace NRough.Core.BaseTypeExtensions
{
    public static class LongExtensions
    {
        public static double ConvertToDouble(this long value, int precision)
        {
            if (precision > 0)
                return value / System.Math.Pow(10, precision);
            return (double)value;
        }

        public static float ConvertToFloat(this long value, int precision)
        {
            if (precision > 0)
                return value / (float)System.Math.Pow(10, precision);
            return (float)value;
        }

        public static int ConvertToInt(this long value, int precision)
        {
            if(precision > 0)
                return (int)(value / System.Math.Pow(10, precision));
            return (int)value;
        }

        public static long ConvertToInt64(this long value, int precision)
        {
            return value;
        }
    }
}
