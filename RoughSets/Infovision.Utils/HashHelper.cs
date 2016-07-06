using System;
using System.Collections;
using System.Collections.Generic;

namespace Infovision.Utils
{
    public static class HashHelper
    {
        public static int Fnv(Int64[] array)
        {
            unchecked
            {
                const int p = 16777619;
                int hash = (int)2166136261;
                for (int i = 0; i < array.Length; i++)
                {
                    byte[] byteArray = BitConverter.GetBytes(array[i]);
                    for (int j = 0; j < byteArray.Length; j++)
                    {
                        hash = (hash ^ byteArray[j]) * p;
                    }
                }

                hash += hash << 13;
                hash ^= hash >> 7;
                hash += hash << 3;
                hash ^= hash >> 17;
                hash += hash << 5;

                return hash;
            }
        }

        public static int GetHashCode(BitArray array)
        {
            uint hash = 17;
            int bitsRemaining = array.Length;
            foreach (int value in array.GetInternalValues())
            {
                uint cleanValue = (uint)value;
                if (bitsRemaining < 32)
                {
                    //clear any bits that are beyond the end of the array
                    int bitsToWipe = 32 - bitsRemaining;
                    cleanValue <<= bitsToWipe;
                    cleanValue >>= bitsToWipe;
                }

                hash = hash * 23 + cleanValue;
                bitsRemaining -= 32;
            }
            return (int)hash;
        }

        public static int GetHashCode<T1, T2>(T1 arg1, T2 arg2)
        {
            unchecked
            {
                return 31 * arg1.GetHashCode() + arg2.GetHashCode();
            }
        }

        public static int GetHashCode<T1, T2, T3>(T1 arg1, T2 arg2, T3 arg3)
        {
            unchecked
            {
                int hash = arg1.GetHashCode();
                hash = 31 * hash + arg2.GetHashCode();
                return 31 * hash + arg3.GetHashCode();
            }
        }

        public static int GetHashCode<T1, T2, T3, T4>(T1 arg1, T2 arg2, T3 arg3, T4 arg4)
        {
            unchecked
            {
                int hash = arg1.GetHashCode();
                hash = 31 * hash + arg2.GetHashCode();
                hash = 31 * hash + arg3.GetHashCode();
                return 31 * hash + arg4.GetHashCode();
            }
        }

        public static int GetHashCode<T>(T[] list)
        {
            unchecked
            {
                int hash = 0;
                foreach (var item in list)
                    hash = 31 * hash + item.GetHashCode();
                return hash;
            }
        }

        public static int GetHashCode<T>(IEnumerable<T> list)
        {
            unchecked
            {
                int hash = 0;
                foreach (var item in list)
                    hash = 31 * hash + item.GetHashCode();
                return hash;
            }
        }

        /// <summary>
        /// Gets a hashcode for a collection for that the order of items
        /// does not matter.
        /// So {1, 2, 3} and {3, 2, 1} will get same hash code.
        /// </summary>
        public static int GetHashCodeForOrderNoMatterCollection<T>(IEnumerable<T> list)
        {
            unchecked
            {
                int hash = 0;
                int count = 0;
                foreach (var item in list)
                {
                    hash += item.GetHashCode();
                    count++;
                }
                return 31 * hash + count.GetHashCode();
            }
        }

        /// <summary>
        /// Alternative way to get a hashcode is to use a fluent
        /// interface like this:<br />
        /// return 0.CombineHashCode(field1).CombineHashCode(field2).
        ///     CombineHashCode(field3);
        /// </summary>
        public static int CombineHashCode<T>(this int hashCode, T arg)
        {
            unchecked
            {
                return 31 * hashCode + arg.GetHashCode();
            }
        }
    }
}