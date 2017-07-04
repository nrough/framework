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
using System.Linq;
using System.Text;

namespace NRough.Core.CollectionExtensions
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
                throw new ArgumentOutOfRangeException("startIndex", "startIndex < 0");
            if (startIndex + length > array.Length)
                throw new ArgumentOutOfRangeException("length", "startIndex + length > array.Length");

            T[] newArray = new T[array.Length - length];

            Array.Copy(array, 0, newArray, 0, startIndex);
            Array.Copy(array, startIndex + length, newArray, startIndex, array.Length - startIndex - length);

            return newArray;
        }

        public static T[] RemoveValue<T>(this T[] array, T value)
        {
            int numIndex = Array.IndexOf(array, value);
            return array.RemoveAt(numIndex);            
        }

        public static T[][] Transpose<T>(this T[][] source)
        {
            var numRows = source.Max(a => a.Length);

            //Will be adjusting multiple "rows" at the same time so need to use a more flexible collection
            var items = new List<List<T>>();
            for (int row = 0; row < source.Length; ++row)
            {
                for (int col = 0; col < source[row].Length; ++col)
                {
                    //Get the current "row" for this column, if any
                    if (items.Count <= col)
                        items.Add(new List<T>());

                    var current = items[col];

                    //Insert the value into the row
                    current.Add(source[row][col]);
                };
            };

            //Convert the nested lists back into a jagged array
            return (from i in items
                    select i.ToArray()
                    ).ToArray();
        }
        
        public static string ToStr<T>(this T[] array, string separator = "|", 
            string format = "", IFormatProvider formatProvider = null)
        {
            if (array == null)
                return "NULL";

            if (array.Length == 0)
                return "NOELEMENTS";

            StringBuilder sb = new StringBuilder();

            if (!String.IsNullOrEmpty(format) && typeof(T).GetInterface("IFormattable") != null)
            {
                Converter<T, IFormattable> c = new Converter<T, IFormattable>(input => (IFormattable)input);
                IFormattable[] formattableArray = Array.ConvertAll<T, IFormattable>(array, c);
                for (int i = 0; i < formattableArray.Length; i++)
                {                    
                    sb.Append(formattableArray[i].ToString(format, formatProvider));
                    if (i < formattableArray.Length - 1)
                        sb.Append(separator);
                }
            }
            else
            { 
                for (int i = 0; i < array.Length; i++)
                {
                    sb.Append(array[i].ToString());
                    if (i < array.Length - 1)
                        sb.Append(separator);
                }
            }
            return sb.ToString();
        }

        public static string ToStr2d<T>(
            this T[][] array,
            string colSeparator = " ",
            string recordSeparator = "\n",
            bool transpose = true,
            string format = "",
            IFormatProvider formatProvider = null,
            string[] colNames = null,
            string[] rowNames = null)
        {
            if (array == null)
                return String.Empty;            

            if (colNames != null)
            {
                if (rowNames != null)
                {
                    if (colNames.Length != array.Length + 1)
                        throw new ArgumentException("colNames.Length != array.Length + 1", "colNames");
                }
                else
                {
                    if (colNames.Length != array.Length)
                        throw new ArgumentException("colNames.Length != array.Length", "colNames");
                }
            }

            if (rowNames != null)
            {
                if (rowNames.Length != array[0].Length)
                    throw new ArgumentException("rowNames.Length != array[0].Length", "rowNames");
            }

            T[][] localArray = null;
            if (transpose)
                localArray = array.Transpose();
            else
                localArray = array.CopyArrayBuiltIn();

            StringBuilder sb = new StringBuilder();

            if (colNames != null)
            {
                for (int i = 0; i < colNames.Length; i++)
                {
                    sb.Append(colNames[i]);
                    if(i != colNames.Length - 1)
                        sb.Append(colSeparator);
                    else
                        sb.Append(recordSeparator);
                }
            }
            
            if (!String.IsNullOrEmpty(format) && typeof(T).GetInterface("IFormattable") != null)
            {
                for (int i = 0; i < localArray.Length; i++)
                {
                    if (rowNames != null)
                    {
                        sb.Append(rowNames[i]);
                        sb.Append(colSeparator);
                    }

                    Converter<T, IFormattable> c = new Converter<T, IFormattable>(input => (IFormattable)input);
                    IFormattable[] formattableArray = Array.ConvertAll<T, IFormattable>(localArray[i], c);

                    for (int j = 0; j < localArray[i].Length; j++)
                    {
                        sb.Append(formattableArray[j].ToString(format, formatProvider));
                        if (j < formattableArray.Length - 1)
                            sb.Append(colSeparator);
                        else
                            sb.Append(recordSeparator);
                    }                    
                }
            }
            else
            {
                for (int i = 0; i < localArray.Length; i++)
                {
                    if (rowNames != null)
                    {
                        sb.Append(rowNames[i]);
                        sb.Append(colSeparator);
                    }

                    for (int j = 0; j < localArray[i].Length; j++)
                    {
                        sb.Append(localArray[i][j].ToString());
                        if (j < localArray[i].Length - 1)
                            sb.Append(colSeparator);
                        else
                            sb.Append(recordSeparator);
                    }                    
                }
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
            if (array == null) throw new ArgumentNullException("array");
            for (int i = 0; i < array.Length; i++)
                array[i] = value;
        }

        public static void SetAll<T>(this T[][] array, T value)
        {
            if (array == null) throw new ArgumentNullException("array");
            for (int i = 0; i < array.Length; i++)
                for(int j=0; j<array[i].Length; j++)                
                    array[i][j] = value;
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

        public static T[][] CopyArrayLinq<T>(this T[][] source)
        {
            return source.Select(s => s.ToArray()).ToArray();
        }

        public static T[][] CopyArrayBuiltIn<T>(this T[][] source)
        {
            var len = source.Length;
            var dest = new T[len][];

            for (var x = 0; x < len; x++)
            {
                var inner = source[x];
                var ilen = inner.Length;
                var newer = new T[ilen];
                Array.Copy(inner, newer, ilen);
                dest[x] = newer;
            }

            return dest;
        }
    }
}