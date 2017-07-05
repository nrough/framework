// 
// NRough Framework
// http://www.nrough.net
// 
// Copyright (c) Sebastian Widz, 2017
// authors at nrough.net
// 
// This file is part of NRough Framework.
// 
// NRough Framework is free software; you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation; either version 3 of the License, or
// (at your option) any later version.
// 
// NRough Framework is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with NRough Framework; if not, write to the Free Software
// Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA  02110-1301  USA
//

using NRough.Core.Comparers;
using System;

namespace NRough.Core.BaseTypeExtensions
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

        public static bool IsNegative(this double d)
        {
            return d < 0;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <param name="representationTolerance"></param>
        /// <returns></returns>
        /// <remarks>Not that it does not work if two numbers are almost equal but have different signs</remarks>
        public static bool AlmostEquals(this double left, double right, double maxDiff, long representationTolerance)
        {
            double diff = System.Math.Abs(left - right);
            if (diff <= maxDiff)
                return true;

            if (left.IsNegative() != right.IsNegative())
                return false;

            long leftAsBits = left.ToBits2Complement();
            long rightAsBits = right.ToBits2Complement();
            long floatingPointRepresentationsDiff = System.Math.Abs(leftAsBits - rightAsBits);
            return (floatingPointRepresentationsDiff <= representationTolerance);
        }

        public static int CompareToEpsilon(this double left, double right)
        {
            return DoubleEpsilonComparer.Instance.Compare(left, right);
        }

        private static unsafe long ToBits2Complement(this double value)
        {
            double* valueAsDoublePtr = &value;
            long* valueAsLongPtr = (long*)valueAsDoublePtr;
            long valueAsLong = *valueAsLongPtr;
            return valueAsLong < 0
                ? (long)(0x8000000000000000 - (ulong)valueAsLong)
                : valueAsLong;
        }
    }
}