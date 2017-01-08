using System;
using Infovision.Data;
using MiscUtil;

namespace Infovision.MachineLearning.Discretization
{
    public class BinaryDiscretization
    {
        private long[] cuts;
        public long[] Cuts
        {
            get { return cuts; }
            set { cuts = value; }
        }

        public BinaryDiscretization(long splitPoint)
        {
            this.cuts = new long[1];
            this.cuts[0] = splitPoint;
        }

        public int Apply(long value)
        {
            if (this.cuts == null)
                return 0;

            for (int i = 0; i < cuts.Length; i++)
                if (value <= cuts[i])
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
                        result[i] = this.Apply(value).ToString();
                }
            }
            else
            {
                for (int i = 0; i < data.NumberOfRecords; i++)
                {
                    long value = data.GetFieldIndexValue(i, fieldIdx);
                    result[i] = this.Apply(value).ToString();
                }
            }

            return result;
        }
    }
}