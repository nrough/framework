using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Infovision.Utils.UnitTests
{
    [TestFixture]
    class ArrayExtensionsTest
    {
        int[] array = new int[] { -1, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13 };
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

            Console.WriteLine(array.RemoveAt(12, 7).ToStr());
        }
    }
}
