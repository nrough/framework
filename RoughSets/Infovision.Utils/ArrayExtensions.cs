using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infovision.Utils
{
    public static class ArrayExtensions
    {
        public static T[] RemoveAt<T>(this T[] array, int idx)
        {            
            if (idx < 0)
                throw new ArgumentOutOfRangeException("idx");

            if (idx >= array.Length)
                throw new ArgumentOutOfRangeException("idx");

            T[] newArray = new T[array.Length];

            if (idx > 0)
                Array.Copy(array, 0, newArray, 0, idx);                            

            if (idx < array.Length - 1)
                Array.Copy(array, idx + 1, newArray, idx, array.Length - idx - 1);                

            return newArray;
        }

        /*
        public static void RemoveAt<T>(ref T[] array, int idx)
        {
            if (idx < 0)
                throw new ArgumentOutOfRangeException("idx");

            if (idx >= array.Length)
                throw new ArgumentOutOfRangeException("idx");

            for (int a = idx; a < array.Length - 1; a++)
                array[a] = array[a + 1];
            Array.Resize(ref array, array.Length - 1);
        }
        */

        public static T[] RemoveValue<T>(this T[] array, T value)
        {            
            int numIndex = Array.IndexOf(array, value);
            return array.Where((val, idx) => idx != numIndex).ToArray();
        }
    }
}
