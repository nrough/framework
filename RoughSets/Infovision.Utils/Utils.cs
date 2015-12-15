using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Management;
using System.Reflection;
using System.Text;

namespace Infovision.Utils
{
    public static class EnumHelper
    {
        public static IEnumerable<T> GetValues<T>()
        {
            return Enum.GetValues(typeof(T)).Cast<T>();
        }

        public static T SetAll<T>(this Enum value)
        {
            Type type = value.GetType();
            object result = value;
            string[] names = Enum.GetNames(type);
            foreach (var name in names)
            {
                ((Enum)result).SetFlag(Enum.Parse(type, name));
            }

            return (T)result;
            //Enum.Parse(type, result.ToString());
        }

        /// <summary>
        /// Includes an enumerated type and returns the new value
        /// </summary>
        public static T SetFlag<T>(this Enum value, T append)
        {
            Debug.Assert(false, " do not use the extension due to performance reason, use binary operation with the explanatory comment instead \n flags |= flag;// SetFlag");
            
            Type type = value.GetType();

            //determine the values
            object result = value;
            var parsed = new _Value(append, type);
            if (parsed.Signed is long)
            {
                result = Convert.ToInt64(value) | (long)parsed.Signed;
            }
            else if (parsed.Unsigned is ulong)
            {
                result = Convert.ToUInt64(value) | (ulong)parsed.Unsigned;
            }

            //return the final value
            return (T)Enum.Parse(type, result.ToString());
        }

        /*
        /// <summary>
        /// Check to see if a flags enumeration has a specific flag set.
        /// </summary>
        /// <param name="variable">Flags enumeration to check</param>
        /// <param name="value">Flag to check for</param>
        /// <returns></returns>
        public static bool HasFlag(this Enum variable, Enum value)
        {
            if (variable == null)
                return false;

            if (value == null)
                throw new ArgumentNullException("value");

            // Not as good as the .NET 4 version of this function, 
            // but should be good enough
            if (!Enum.IsDefined(variable.GetType(), value))
            {
                throw new ArgumentException(string.Format(
                    "Enumeration type mismatch.  The flag is of type '{0}', " +
                    "was expecting '{1}'.", value.GetType(),
                    variable.GetType()));
            }

            ulong num = Convert.ToUInt64(value);
            return ((Convert.ToUInt64(variable) & num) == num);
        }
        */ 


        /// <summary>
        /// Removes an enumerated type and returns the new value
        /// </summary>
        public static T ClearFlag<T>(this Enum value, T remove)
        {
            Debug.Assert(false, " do not use the extension due to performance reason, use binary operation with the explanatory comment instead \n flags &= ~flag; // ClearFlag  ");

            Type type = value.GetType();

            //determine the values
            object result = value;
            var parsed = new _Value(remove, type);
            if (parsed.Signed is long)
            {
                result = Convert.ToInt64(value) & ~(long)parsed.Signed;
            }
            else if (parsed.Unsigned is ulong)
            {
                result = Convert.ToUInt64(value) & ~(ulong)parsed.Unsigned;
            }

            //return the final value
            return (T)Enum.Parse(type, result.ToString());
        }

        //class to simplfy narrowing values between
        //a ulong and long since either value should
        //cover any lesser value
        private class _Value
        {
            //cached comparisons for tye to use            
            private static readonly Type _UInt32 = typeof(long);
            private static readonly Type _UInt64 = typeof(ulong);

            public readonly long? Signed;
            public readonly ulong? Unsigned;

            public _Value(object value, Type type)
            {
                //make sure it is even an enum to work with
                if (!type.IsEnum)
                {
                    throw new ArgumentException(
                        "Value provided is not an enumerated type!");
                }

                //then check for the enumerated value
                Type compare = Enum.GetUnderlyingType(type);

                //if this is an unsigned long then the only
                //value that can hold it would be a ulong
                if (compare.Equals(_UInt32) || compare.Equals(_UInt64))
                {
                    Unsigned = Convert.ToUInt64(value);
                }
                //otherwise, a long should cover anything else
                else
                {
                    Signed = Convert.ToInt64(value);
                }
            }
        }
    }
    
    public static class InfovisionHelper
    {
        public static int NumberOfCores()
        {
            int coreCount = 0;
            foreach (var item in new ManagementObjectSearcher("Select * from Win32_Processor").Get())
            {
                coreCount += int.Parse(item["NumberOfCores"].ToString(), CultureInfo.InvariantCulture);
            }
            return coreCount;
        }

        public static Type String2Type(string value)
        {
            int intResult;
            decimal decimalResult;
            double doubleResult;

            if (Int32.TryParse(value, out intResult))
            {
                return typeof(int);
            }
            else if (Double.TryParse(value, out doubleResult))
            {
                return typeof(double);
            }
            else if (Decimal.TryParse(value, out decimalResult))
            {
                return typeof(decimal);
            }

            return typeof(string);
        }

        public static string IntArray2Ranges(long[] array, string rangeSeparator = "..", string elementSeparator = " ")
        {
            List<KeyValuePair<long, long>> ranges = new List<KeyValuePair<long, long>>();
            StringBuilder stringBuilder = new StringBuilder();
            ;

            int i = 0;
            while (i < array.Length)
            {
                int j = i;
                while (j + 1 < array.Length && array[j + 1] == array[j] + 1)
                {
                    j++;
                }

                ranges.Add(new KeyValuePair<long, long>(array[i], array[j]));
                i = j + 1;
            }

            bool first = true;
            foreach (KeyValuePair<long, long> kvp in ranges)
            {
                if (!first)
                    stringBuilder.Append(elementSeparator);

                if (kvp.Key == (kvp.Value - 1))
                    stringBuilder.Append(kvp.Key).Append(elementSeparator).Append(kvp.Value);
                else if (kvp.Key != kvp.Value)
                    stringBuilder.Append(kvp.Key).Append(rangeSeparator).Append(kvp.Value);
                else
                    stringBuilder.Append(kvp.Key);

                first = false;
            }

            return stringBuilder.ToString();
        }

        public static string IntArray2Ranges(int[] array, string rangeSeparator = "..", string elementSeparator = " ")
        {
            long[] longArray = Array.ConvertAll<int, long>(array, delegate(int i) { return (long)i; });
            return IntArray2Ranges(longArray, rangeSeparator, elementSeparator);
        }

        /// <summary>
        /// Function to get byte array from a object
        /// </summary>
        /// <param name="_Object">object to get byte array</param>
        /// <returns>Byte Array</returns>
        public static byte[] ObjectToByteArray(object _Object)
        {
            try
            {
                // create new memory stream
                System.IO.MemoryStream _MemoryStream = new System.IO.MemoryStream();

                // create new BinaryFormatter
                System.Runtime.Serialization.Formatters.Binary.BinaryFormatter _BinaryFormatter
                            = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();

                // Serializes an object, or graph of connected objects, to the given stream.
                _BinaryFormatter.Serialize(_MemoryStream, _Object);

                // convert stream to byte array and return
                return _MemoryStream.ToArray();
            }
            catch (Exception _Exception)
            {
                // Error
                Console.WriteLine("Exception caught in process: {0}", _Exception.ToString());
            }

            // Error occured, return null
            return null;
        }

        /// <summary>
        /// Returns an Object with the specified Type and whose value is equivalent to the specified object.
        /// </summary>
        /// <param name="value">An Object that implements the IConvertible interface.</param>
        /// <param name="conversionType">The Type to which value is to be converted.</param>
        /// <returns>An object whose Type is conversionType (or conversionType's underlying type if conversionType
        /// is Nullable&lt;&gt;) and whose value is equivalent to value. -or- a null reference, if value is a null
        /// reference and conversionType is not a value type.</returns>
        /// <remarks>
        /// This method exists as a workaround to System.Convert.ChangeType(Object, Type) which does not handle
        /// nullables as of version 2.0 (2.0.50727.42) of the .NET Framework. The idea is that this method will
        /// be deleted once Convert.ChangeType is updated in a future version of the .NET Framework to handle
        /// nullable types, so we want this to behave as closely to Convert.ChangeType as possible.
        /// This method was written by Peter Johnson at:
        /// http://aspalliance.com/author.aspx?uId=1026.
        /// </remarks>
        public static object ChangeType(object value, Type conversionType)
        {
            // Note: This if block was taken from Convert.ChangeType as is, and is needed here since we're
            // checking properties on conversionType below.
            if (conversionType == null)
            {
                throw new ArgumentNullException("conversionType");
            }

            // If it's not a nullable type, just pass through the parameters to Convert.ChangeType

            if (conversionType.IsGenericType &&
              conversionType.GetGenericTypeDefinition().Equals(typeof(Nullable<>)))
            {
                // It's a nullable type, so instead of calling Convert.ChangeType directly which would throw a
                // InvalidCastException (per http://weblogs.asp.net/pjohnson/archive/2006/02/07/437631.aspx),
                // determine what the underlying type is
                // If it's null, it won't convert to the underlying type, but that's fine since nulls don't really
                // have a type--so just return null
                // Note: We only do this check if we're converting to a nullable type, since doing it outside
                // would diverge from Convert.ChangeType's behavior, which throws an InvalidCastException if
                // value is null and conversionType is a value type.
                if (value == null)
                {
                    return null;
                } // end if

                // It's a nullable type, and not null, so that means it can be converted to its underlying type,
                // so overwrite the passed-in conversion type with this underlying type
                NullableConverter nullableConverter = new NullableConverter(conversionType);
                conversionType = nullableConverter.UnderlyingType;
            } // end if

            // Now that we've guaranteed conversionType is something Convert.ChangeType can handle (i.e. not a
            // nullable type), pass the call on to Convert.ChangeType
            return Convert.ChangeType(value, conversionType, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Checks if a number is a power of two.
        /// </summary>
        /// <param name="x"></param>
        /// Number to be checked
        /// <returns>true if the number is a power of two, false otherwise</returns>
        public static bool IsPowerOfTwo(long x)
        {
            return (x != 0) && ((x & (x - 1)) == 0);
        }
    }
    
    public static class Extension
    {
        #region Methods
        
        public static IList<T> Clone<T>
            (this IList<T> listToClone) where T : ICloneable
        {
            return listToClone.Select(item => (T)item.Clone()).ToList();
        }        

        public static StringBuilder Digits(this StringBuilder builder, int number)
        {
            if (number >= 100000000)
            {
                // Use system ToString.
                builder.Append(number.ToString(CultureInfo.InvariantCulture));
                return builder;
            }
            if (number < 0)
            {
                // Negative.
                builder.Append(number.ToString(CultureInfo.InvariantCulture));
                return builder;
            }
            int copy;
            int digit;
            if (number >= 10000000)
            {
                // 8.
                copy = number % 100000000;
                digit = copy / 10000000;
                builder.Append((char)(digit + 48));
            }
            if (number >= 1000000)
            {
                // 7.
                copy = number % 10000000;
                digit = copy / 1000000;
                builder.Append((char)(digit + 48));
            }
            if (number >= 100000)
            {
                // 6.
                copy = number % 1000000;
                digit = copy / 100000;
                builder.Append((char)(digit + 48));
            }
            if (number >= 10000)
            {
                // 5.
                copy = number % 100000;
                digit = copy / 10000;
                builder.Append((char)(digit + 48));
            }
            if (number >= 1000)
            {
                // 4.
                copy = number % 10000;
                digit = copy / 1000;
                builder.Append((char)(digit + 48));
            }
            if (number >= 100)
            {
                // 3.
                copy = number % 1000;
                digit = copy / 100;
                builder.Append((char)(digit + 48));
            }
            if (number >= 10)
            {
                // 2.
                copy = number % 100;
                digit = copy / 10;
                builder.Append((char)(digit + 48));
            }
            if (number >= 0)
            {
                // 1.
                copy = number % 10;
                digit = copy / 1;
                builder.Append((char)(digit + 48));
            }

            return builder;
        }

        #endregion
    }

    public static class Qickselect
    {
        public static int RandomizedSelect(int[] a, int p, int r, int k)
        {
            if (p == r)
            {
                return a[p];
            }
            int q = RandomizedPartition(a, p, r);
            int length = q - p + 1;
            if (k == length)
            {
                return a[q];
            }
            else if (k < length)
            {
                return RandomizedSelect(a, p, q - 1, k);
            }
            else
            {
                return RandomizedSelect(a, q + 1, r, k - length);
            }
        }

        static int RandomizedPartition(int[] a, int p, int r)
        {
            int i = new Random(Environment.TickCount).Next(p, r + 1);
            Swap(ref a[r], ref a[i]);
            return Partition(a, p, r);
        }

        static int Partition(int[] a, int p, int r)
        {
            int x = a[r];
            int i = p - 1;
            for (int j = p; j < r; j++)
            {
                if (a[j] <= x)
                {
                    i++;
                    Swap(ref a[i], ref a[j]);
                }
            }
            Swap(ref a[i + 1], ref a[r]);
            return i + 1;
        }

        static void Swap(ref int a, ref int b)
        {
            int temp = a;
            a = b;
            b = temp;
        }
    }

    public static class BitArrayExtensions
    {
        static FieldInfo _internalArrayGetter = GetInternalArrayGetter();

        static FieldInfo GetInternalArrayGetter()
        {
            return typeof(BitArray).GetField("m_array", BindingFlags.NonPublic | BindingFlags.Instance);
        }

        static int[] GetInternalArray(BitArray array)
        {
            return (int[])_internalArrayGetter.GetValue(array);
        }

        public static IEnumerable<int> GetInternalValues(this BitArray array)
        {
            return GetInternalArray(array);
        }
    }

    public static class DataTableExtensions
    {
        public static void WriteToCSVFile(this DataTable dataTable, string filePath, string separator)
        {
            StringBuilder sb = new StringBuilder(); 
            IEnumerable<string> columnNames = dataTable.Columns.Cast<DataColumn>().
                                              Select(column => column.ColumnName);
            sb.AppendLine(string.Join(separator, columnNames));

            foreach (DataRow row in dataTable.Rows)
            {
                IEnumerable<string> fields = row.ItemArray.Select(field => field.ToString());
                sb.AppendLine(string.Join(separator, fields));
            }

            System.IO.File.WriteAllText(filePath, sb.ToString());
        }

        /// <summary>
        ///   Returns a subtable extracted from the current table.
        /// </summary>
        /// 
        /// <param name="source">The table to return the subtable from.</param>
        /// <param name="indexes">Array of indices.</param>
        /// 
        public static DataTable Subtable(this DataTable source, int[] indexes)
        {
            if (source == null)
                throw new ArgumentNullException("source");
            if (indexes == null)
                throw new ArgumentNullException("indexes");

            DataTable destination = source.Clone();
            foreach (int i in indexes)
            {
                DataRow row = source.Rows[i];
                destination.ImportRow(row);
            }
            return destination;
        }            
    }
}
