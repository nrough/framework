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

using NRough.Core.Helpers;
using System;
using System.Collections.Generic;

namespace NRough.Core.Comparers
{
    [Serializable]
    public class Int64ArrayEqualityComparer : IEqualityComparer<long[]>
    {
        private static volatile Int64ArrayEqualityComparer instance;
        private static readonly object syncRoot = new object();

        public static Int64ArrayEqualityComparer Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                            instance = new Int64ArrayEqualityComparer();
                    }
                }

                return instance;
            }
        }

        private Int64ArrayEqualityComparer() { }

        public bool Equals(long[] x, long[] y)
        {
            if (x == y) return true;
            if (x == null || y == null) return false;
            if (x.Length != y.Length) return false;

            for (int i = x.Length - 1; i >= 0 ; i--)
                if (x[i] != y[i])
                    return false;

            return true;
        }

        public int GetHashCode(long[] array)
        {
            return HashHelper.GetHashCode<long>(array);
            //return HashHelper.ArrayHashMedium<long>(array);
            //return HashHelper.LongArrayHash(array);
        }
    }
}