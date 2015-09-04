using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NUnit;
using NUnit.Framework;

using Infovision.CudaLib;


namespace Infovision.CudaLib.UnitTests
{
    [TestFixture]
    public class QuickSort
    {
        [Test]
        public void SortCPURecursive()
        {
            var rand = new Random();
            var array = new int[100];
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = rand.Next(1000);
            }

            Console.WriteLine("Before: ");
            Console.WriteLine(string.Join(" ", array));

            CudaLib.QuickSort<int> qSort = new CudaLib.QuickSort<int>();
            qSort.SortCPURecursive(array);

            int [] index = qSort.GetIndexTable();
            
            for (int i = 0; i < array.Length; i++)
            {
                Console.Write("{0} ", array[index[i]]);
            }                
        }
    }
}
