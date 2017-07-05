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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NRough.MachineLearning.Discretization
{
    [Serializable]
    public class DiscretizeEqualWidth : DiscretizeUnsupervisedBase
    {
        public DiscretizeEqualWidth()
            : base()
        {
            //sorting is not necessary
            this.IsDataSorted = true;
            this.UseWeights = true;
        }

        public override long[] ComputeCuts(long[] data, double[] weights)
        {
            long max = data.Max();
            long min = data.Min();                        
            long binWidth = (max - min) / this.NumberOfBuckets;

            long[] cutPoints = null;
            if ((this.NumberOfBuckets > 1) && (binWidth > 0))
            {
                cutPoints = new long[this.NumberOfBuckets - 1];
                for (int i = 1; i < this.NumberOfBuckets; i++)
                    cutPoints[i - 1] = min + (binWidth * i);
            }

            return cutPoints;
        }
    }
}
