using System;
using NUnit.Framework;

namespace NRough.Core.Tests
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