using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace Infovision.CudaLib
{
    class Program
    {
        static void Main(string[] args)
        {
            var rand = new Random();
            int[] array = new int[2000000];
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = rand.Next(1000);
            }

            //Console.WriteLine("Before: ");
            //Console.WriteLine(string.Join(" ", array));


            Stopwatch sw = new Stopwatch();

            sw.Start();
            CudaLib.QuickSort qSort = new CudaLib.QuickSort();
            qSort.GPUParalell(array);
            sw.Stop();

            int[] index = qSort.GetIndexTable();

            Console.WriteLine("After GPU Paralell: {0}", sw.Elapsed);

            sw.Reset();

            sw.Start();
            qSort = new CudaLib.QuickSort();
            //qSort.CPURecursive(array);
            sw.Stop();

            index = qSort.GetIndexTable();
            
            Console.WriteLine("After CPU Recursive: {0}", sw.Elapsed);
            
            /*
            for (int i = 0; i < array.Length; i++)
            {
                Console.Write("{0} ", array[index[i]]);
            }
            Console.WriteLine();
            */

            sw.Reset();

            sw.Start();
            qSort = new CudaLib.QuickSort();
            //qSort.CPUIterative(array);
            sw.Stop();

            index = qSort.GetIndexTable();
            
            
            Console.WriteLine("After CPU Iterative: {0}", sw.Elapsed);
            /*
            for (int i = 0; i < array.Length; i++)
            {
                Console.Write("{0} ", array[index[i]]);
            }
            Console.WriteLine();
            */
            

            sw.Reset();

            sw.Start();
            qSort = new CudaLib.QuickSort();
            //qSort.CPUParalell(array);
            sw.Stop();

            index = qSort.GetIndexTable();
            
            Console.WriteLine("After CPU Parallel: {0}", sw.Elapsed);
            
            /*
            for (int i = 0; i < array.Length; i++)
            {
                Console.Write("{0} ", array[index[i]]);
            }
            Console.WriteLine();
            */

            Console.ReadLine();
        }
    }
}
