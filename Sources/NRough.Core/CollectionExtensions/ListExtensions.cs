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
using NRough.Core.Random;
using System;
using System.Collections.Generic;

namespace NRough.Core.CollectionExtensions
{
    public static class ListExtensions
    {
        public static void Shuffle<T>(this List<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = RandomSingleton.Random.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }
    }

    public class ListComparer<T> : IEqualityComparer<List<T>>
    {
        private IEqualityComparer<T> valueComparer;

        public ListComparer(IEqualityComparer<T> valueComparer = null)
        {
            this.valueComparer = valueComparer ?? EqualityComparer<T>.Default;
        }

        public bool Equals(List<T> x, List<T> y)
        {
            return x.SetEquals(y, valueComparer);
        }

        public int GetHashCode(List<T> obj)
        {
            throw new NotImplementedException();
        }
    }
}