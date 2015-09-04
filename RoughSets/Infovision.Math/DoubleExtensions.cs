using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infovision.Math
{
    public static class DoubleExtensions
    {
        public static readonly long ExponentMask = 0x7FF0000000000000;
        public static bool IsSubnormal(this double v)
        {
            long bithack = BitConverter.DoubleToInt64Bits(v);
            if (bithack == 0) return false;
            return (bithack & ExponentMask) == 0;
        }

        public static bool IsNormal(this double v)
        {
            long bithack = BitConverter.DoubleToInt64Bits(v);
            if (bithack == 0) return true;
            bithack &= ExponentMask;
            return (bithack != 0) && (bithack != ExponentMask);
        }                

        public static bool SignBit(this double d)
        {
            // Translate the double into sign, exponent and mantissa.
            long bits = BitConverter.DoubleToInt64Bits(d);
            // Note that the shift is sign-extended, hence the test against -1 not 1            
            return (bits < 0);
        }
    }       
}
