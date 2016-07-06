using System;
using Infovision.Data;
using MiscUtil;

namespace Infovision.Datamining.Filters.Unsupervised.Attribute
{
    public class BinaryDiscretization<T>
        where T : struct, IComparable, IFormattable, IComparable<T>, IEquatable<T>
    {
        private double[] cuts;

        public double[] Cuts
        {
            get { return cuts; }
            set { cuts = value; }
        }

        public BinaryDiscretization(double splitPoint)
        {
            this.cuts = new double[1];
            this.cuts[0] = splitPoint;
        }

        public int Search(T value)
        {
            if (this.cuts == null)
                return 0;

            for (int i = 0; i < cuts.Length; i++)
                if (Operator.Convert<T, double>(value).CompareTo(cuts[i]) <= 0)
                    return i;

            return cuts.Length;
        }

        public string[] Discretize(DataStore data, DataFieldInfo fieldInfo)
        {
            string[] result = new string[data.NumberOfRecords];
            int fieldIdx = data.DataStoreInfo.GetFieldIndex(fieldInfo.Id);

            if (fieldInfo.HasMissingValues)
            {
                for (int i = 0; i < data.NumberOfRecords; i++)
                {
                    long value = data.GetFieldIndexValue(i, fieldIdx);
                    if (value == fieldInfo.MissingValueInternal)
                        result[i] = "?";
                    else
                        result[i] = this.Search((T)fieldInfo.Internal2External(value)).ToString();
                }
            }
            else
            {
                for (int i = 0; i < data.NumberOfRecords; i++)
                {
                    long value = data.GetFieldIndexValue(i, fieldIdx);
                    result[i] = this.Search((T)fieldInfo.Internal2External(value)).ToString();
                }
            }

            return result;
        }
    }
}