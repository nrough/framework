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
using NUnit.Framework;
using NRough.Core;
using NRough.Core.CollectionExtensions;

namespace NRough.Tests.Core
{
    [TestFixture]
    internal class ArrayExtensionsTest
    {
        private int[] array = new int[] { -1, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13 };

        [Test]
        public void RemoveAtTest()
        {
            Console.WriteLine(array.ToStr());
            Console.WriteLine();

            Console.WriteLine(array.RemoveAt(1, 0).ToStr());
            Console.WriteLine(array.RemoveAt(1, 1).ToStr());
            Console.WriteLine(array.RemoveAt(1, -1).ToStr());
            Console.WriteLine(array.RemoveAt(5, 2).ToStr());
            Console.WriteLine(array.RemoveAt(5, -2).ToStr());
            Console.WriteLine(array.RemoveAt(5, 3).ToStr());
            Console.WriteLine(array.RemoveAt(5, -3).ToStr());
            Console.WriteLine(array.RemoveAt(12, 1).ToStr());
        }
    }
}