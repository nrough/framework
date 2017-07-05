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
using System.Collections;
using System.Collections.Generic;

namespace NRough.Core.Comparers
{
    public class ArrayComparer<T> : EqualityComparer<T[]>, IComparer<T[]>, IComparer
        where T : IComparable, IEquatable<T>
    {
        private static volatile ArrayComparer<T> arrayComparer = null;
        private static readonly object syncRoot = new object();

        public static ArrayComparer<T> Instance
        {
            get
            {
                if (arrayComparer == null)
                {
                    lock (syncRoot)
                    {
                        if (arrayComparer == null)
                        {                            
                            arrayComparer = new ArrayComparer<T>();
                        }
                    }
                }

                return arrayComparer;
            }            
        }



        public int Compare(object x, object y)
        {
            T[] a1 = x as T[];            
            T[] a2 = y as T[];

            return Compare(a1, a2);
        }

        public int Compare(T[] x, T[] y)
        {
            if (x == null && y == null)
                return 0;

            if (x == null)
                return -1;

            if (y == null)
                return 1;

            int checkLength = x.Length.CompareTo(y.Length);
            if (checkLength != 0)
                return checkLength;

            for (int i = 0; i < x.Length; i++)
            {
                int checkElement = x[i].CompareTo(y[i]);
                if (checkElement != 0)
                    return checkElement;
            }

            return 0;
        }

        public override bool Equals(T[] x, T[] y)
        {
            if (x == null && y == null)
                return true;

            if (x == null)
                return false;

            if (y == null)
                return false;

            if (x.Length != y.Length)
                return false;

            for (int i = 0; i < x.Length; i++)
                if (!x[i].Equals(y[i]))
                    return false;
          
            return true;
        }

        public override int GetHashCode(T[] obj)
        {
            return HashHelper.ArrayHashMedium(obj);
        }        
    }


}
