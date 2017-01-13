using System;
using Infovision.Data;

namespace Infovision.MachineLearning.Discretization
{
    public class BinaryDiscretization : DiscretizeUnsupervisedBase
    {
        public BinaryDiscretization()
            : base()
        {
            this.IsDataSorted = true;
        }

        public BinaryDiscretization(long splitPoint)
            : this()
        {
            this.Cuts = new long[1];
            this.Cuts[0] = splitPoint;
        }

        public override long[] ComputeCuts(long[] data, double[] weights)
        {
            return this.Cuts;
        }

        public long[] Discretize(DataStore data, DataFieldInfo fieldInfo)
        {
            long[] result = new long[data.NumberOfRecords];
            int fieldIdx = data.DataStoreInfo.GetFieldIndex(fieldInfo.Id);

            if (fieldInfo.HasMissingValues)
            {
                for (int i = 0; i < data.NumberOfRecords; i++)
                {
                    long value = data.GetFieldIndexValue(i, fieldIdx);
                    if (value == fieldInfo.MissingValueInternal)
                        result[i] = fieldInfo.MissingValueInternal;
                    else
                        result[i] = this.Apply(value);
                }
            }
            else
            {
                for (int i = 0; i < data.NumberOfRecords; i++)
                {
                    long value = data.GetFieldIndexValue(i, fieldIdx);
                    result[i] = this.Apply(value);
                }
            }

            return result;
        }
    }
}