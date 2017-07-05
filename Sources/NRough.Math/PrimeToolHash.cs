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

using NRough.Doc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NRough.Math
{
    /// <summary>
    /// 
    /// </summary>
    /// <remarks>
    /// http://www.dotnetperls.com/prime
    /// </remarks>
    [AssemblyTreeVisible(false)]
    public static class PrimeToolHash
    {
        public static int[] primes;

        static PrimeToolHash()
        {
            //
            // Initialize array of first primes before methods are called. 
            // 72 values
            primes = new int[]
            {
                3, 7, 11, 17, 23, 29, 37,
                47, 59, 71, 89, 107, 131,
                163, 197, 239, 293, 353,
                431, 521, 631, 761, 919,
                1103, 1327, 1597, 1931,
                2333, 2801, 3371, 4049,
                4861, 5839, 7013, 8419,
                10103, 12143, 14591, 17519,
                21023, 25229, 30293, 36353,
                43627, 52361, 62851, 75431,
                90523, 108631, 130363,
                156437, 187751, 225307,
                270371, 324449, 389357,
                467237, 560689, 672827,
                807403, 968897, 1162687,
                1395263, 1674319, 2009191,
                2411033, 2893249, 3471899,
                4166287, 4999559, 5999471,
                7199369
            };
        }

        public static int GetPrime(int min)
        {
            //
            // Get the first hashtable prime number
            // ... that is equal to or greater than the parameter.
            //
            for (int i = 0; i < primes.Length; i++)
            {
                int num2 = primes[i];
                if (num2 >= min)
                {
                    return num2;
                }
            }
            for (int j = min | 1; j < 2147483647; j += 2)
            {
                if (PrimeTool.IsPrime(j))
                {
                    return j;
                }
            }

            return min;
        }
    }
}
