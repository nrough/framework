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
