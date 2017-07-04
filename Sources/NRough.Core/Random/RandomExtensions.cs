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

namespace NRough.Core.Random
{
    public static class RandomExtensions
    {
        public static int[] RandomVectorNoRepetition(int n, int min, int max)
        {
            int size = max - min + 1;
            if (n > size)
                throw new ArgumentException("Argument n is too big for given range.", "n");
            int[] tmp = new int[size];
            for (int i = 0; i < size; i++)
                tmp[i] = i + min;
            for (int i = 0; i < size; i++)
            {
                int k = RandomSingleton.Random.Next(0, size);
                int t = tmp[i];
                tmp[i] = tmp[k];
                tmp[k] = t;
            }

            int[] result = new int[n];
            Array.Copy(tmp, result, n);
            return result;
        }
    }
}