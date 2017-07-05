// 
// NRough Framework
// http://www.nrough.net
// 
// Copyright (c) Sebastian Widz, 2017
// authors at nrough.net
// 
// This file is part of NRough Framework.
// 
// NRough Framework is free software; you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation; either version 3 of the License, or
// (at your option) any later version.
// 
// NRough Framework is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with NRough Framework; if not, write to the Free Software
// Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA  02110-1301  USA
//

using System;
using NRough.Data;

namespace NRough.MachineLearning.Discretization
{
    /// <summary>
    /// Manual discretization. Cuts must be given by the user explicitly. 
    /// </summary>
    public class DiscretizeManual : DiscretizeUnsupervisedBase
    {
        public DiscretizeManual()
            : base()
        {
            this.IsDataSorted = true;
        }

        public DiscretizeManual(long splitPoint)
            : this()
        {
            this.Cuts = new long[1];
            this.Cuts[0] = splitPoint;
        }

        public DiscretizeManual(long[] cutPoints)
            : this()
        {
            this.Cuts = new long[cutPoints.Length];
            Array.Copy(cutPoints, this.Cuts, cutPoints.Length);
        }

        public override long[] ComputeCuts(long[] data, double[] weights)
        {
            return this.Cuts;
        }

        public long[] Discretize(DataStore data, AttributeInfo fieldInfo)
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