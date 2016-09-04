using System;
using System.Linq;
using System.Text;

namespace Infovision.Utils
{
    public static class ArrayExtensions
    {
        public static void Shuffle<T>(this T[] array)
        {
            int n = array.Length;
            while (n > 1)
            {
                n--;
                int k = RandomSingleton.Random.Next(n + 1);
                T element = array[k];
                array[k] = array[n];
                array[n] = element;
            }
        }

        public static void ShuffleFwd<T>(this T[] array, int number = 0)
        {
            if (number > array.Length)
                number = array.Length;

            for (int i = 0; i < number; i++)
            {
                int k = RandomSingleton.Random.Next(i, array.Length);
                T element = array[k];
                array[k] = array[i];
                array[i] = element;
            }
        }

        public static T[] ShuffleDuplicate<T>(this T[] array)
        {
            T[] tmp = new T[array.Length];
            Array.Copy(array, tmp, array.Length);

            for (int i = 0; i < array.Length; i++)
            {
                int k = RandomSingleton.Random.Next(0, array.Length);
                T element = tmp[i];
                tmp[i] = tmp[k];
                tmp[k] = element;
            }
            return tmp;
        }

        public static T[] RemoveAt<T>(this T[] array, int idx)
        {
            if (array == null)
                throw new ArgumentNullException("array");

            if (idx < 0)
                throw new ArgumentOutOfRangeException("idx", "idx < 0");

            if (idx >= array.Length)
                throw new ArgumentOutOfRangeException("idx", "idx >= array.Length");

            T[] newArray = new T[array.Length - 1];

            if (idx > 0)
                Array.Copy(array, 0, newArray, 0, idx);

            if (idx < array.Length - 1)
                Array.Copy(array, idx + 1, newArray, idx, array.Length - idx - 1);

            return newArray;
        }        

        //Code reviwed
        //http://codereview.stackexchange.com/questions/132630/removing-n-elements-from-array-starting-from-index/132635#132635
        public static T[] RemoveAt<T>(this T[] array, int startIndex, int length)
        {
            if (array == null)
                throw new ArgumentNullException("array");

            if (length < 0)
            {
                startIndex += 1 + length;
                length = -length;
            }

            if (startIndex < 0)
                throw new ArgumentOutOfRangeException("startIndex");
            if (startIndex + length > array.Length)
                throw new ArgumentOutOfRangeException("length");

            T[] newArray = new T[array.Length - length];

            Array.Copy(array, 0, newArray, 0, startIndex);
            Array.Copy(array, startIndex + length, newArray, startIndex, array.Length - startIndex - length);

            return newArray;
        }

        public static T[] RemoveValue<T>(this T[] array, T value)
        {
            int numIndex = Array.IndexOf(array, value);
            return array.RemoveAt(numIndex);
            //return array.Where((val, idx) => idx != numIndex).ToArray();
        }

        public static string ToStr<T>(this T[] array, char separator = '|')
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < array.Length; i++)
            {
                sb.Append(array[i].ToString());
                if (i < array.Length - 1)
                    sb.Append(separator);
            }
            return sb.ToString();
        }

        public static T[] SubArray<T>(this T[] array, int index, int length)
        {
            if (array == null)
                throw new ArgumentNullException("array");

            if (index + length > array.Length)
                throw new IndexOutOfRangeException("index + length > array.Length");

            T[] result = new T[length];
            Array.Copy(array, index, result, 0, length);
            return result;
        }

        public static int IndexOf<T>(this T[] array, T value)
        {
            int result = -1;
            for (int i = 0; i < array.Length; i++)
                if (array[i].Equals(value))
                {
                    result = i;
                    break;
                }
            return result;
        }

        public static int[] IndicesOf<T>(this T[] array, T[] values)
        {
            int[] result = new int[values.Length];
            for (int i = 0; i < values.Length; i++)
                result[i] = array.IndexOf(values[i]);
            return result;
        }

        public static int[] IndicesOfOrderedByValue<T>(this T[] array, T[] values)
        {
            int[] result = new int[values.Length];            
            for (int i = 0, j = 0; i < array.Length; i++)
                if(values.IndexOf(array[i]) != -1)
                    result[j++] = i;
            return result;
        }

        public static T[] KeepIndices<T>(this T[] array, int[] indicesToKeep)
        {
            T[] result = new T[indicesToKeep.Length];
            for (int i = 0; i < indicesToKeep.Length; i++)
                result[i] = array[indicesToKeep[i]];
            return result;
        }

        public static void SetAll<T>(this T[] array, T value)
        {
            if (array == null)
                throw new ArgumentNullException("array");

            for (int i = 0; i < array.Length; i++)
                array[i] = value;
        }

        public static T[] RandomSubArray<T>(this T[] array, int size)
        {
            if (array == null)
                throw new ArgumentNullException("array");
            if (size < 0)
                throw new ArgumentException("size < 0", "size");
            if (size > array.Length)
                throw new ArgumentException("size > array.Length", "size");

            T[] result = new T[size];

            if (size == array.Length)
            {
                Array.Copy(array, result, array.Length);
                return result;
            }
            
            int[] tmpArray = Enumerable.Range(0, array.Length).ToArray();
            tmpArray.Shuffle();
            for (int i = 0; i < size; i++)
                result[i] = array[tmpArray[i]];
            return result;
        }
    }
}