﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MiscUtil;

namespace Infovision.Utils
{
    public static class ArrayExtensions
    {
        public static void Shuffle<T>(this T[] array)
        {
            for (int i = array.Length - 1; i > 0; i--)
            {
                int k = RandomSingleton.Random.Next() % (i + 1);
                T element = array[k];
                array[k] = array[i];
                array[i] = element;
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
            //if (idx < 0)
            //    throw new ArgumentOutOfRangeException("idx");

            //if (idx >= array.Length)
            //    throw new ArgumentOutOfRangeException("idx");

            T[] newArray = new T[array.Length - 1];

            if (idx > 0)
                Array.Copy(array, 0, newArray, 0, idx);                            

            if (idx < array.Length - 1)
                Array.Copy(array, idx + 1, newArray, idx, array.Length - idx - 1);                

            return newArray;
        }

        public static T[] RemoveAt<T>(this T[] array, int idx, int len)
        {
            T[] newArray;

            if (len >= 0)
            {
                if (idx + len > array.Length)
                    len = array.Length - idx;
                
                newArray = new T[array.Length - len];
                if (idx > 0)
                    Array.Copy(array, 0, newArray, 0, idx);

                if (idx < array.Length - 1)
                    Array.Copy(array, idx + len, newArray, idx, array.Length - idx - len);
            }
            else
            {
                newArray = new T[array.Length + len];                
                if (idx > 0)
                    Array.Copy(array, 0, newArray, 0, idx + len);
                
                if (idx < array.Length - 1)
                    Array.Copy(array, idx, newArray, idx + len, array.Length - idx);
            }

            //if (newArray.Contains(Operator<T>.Zero))
            //    throw new InvalidOperationException();


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

        public static string ToStr<T>(this T[] array)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < array.Length; i++)
            {
                sb.Append(array[i].ToString());
                if (i < array.Length - 1)
                    sb.Append('|');
            }
            return sb.ToString();
        }

        public static T[] SubArray<T>(this T[] array, int index, int length)
        {
            T[] result = new T[length];
            Array.Copy(array, index, result, 0, length);
            return result;
        }
    }
}
