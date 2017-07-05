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
    public class DiscretizeEqualFreqency : DiscretizeUnsupervisedBase
    {
        #region Properties

        double FixedWeightPerInterval { get; set; } = -1.0;

        #endregion

        #region Constructors

        public DiscretizeEqualFreqency()
            : base()
        {
            this.UseWeights = true;
        }

        #endregion

        #region Methods

        public override long[] ComputeCuts(long[] data, double[] weights)
        {                        
            double weightsSum = (weights == null) ? data.Length : weights.Sum();

            double freq = this.FixedWeightPerInterval > 0 
                        ? this.FixedWeightPerInterval 
                        : (weightsSum / this.NumberOfBuckets);

            long[] cutPoints = this.FixedWeightPerInterval > 0
                             ? new long[(int)(weightsSum / this.FixedWeightPerInterval)]
                             : new long[this.NumberOfBuckets - 1];
            
            double counter = 0, last = 0;
            int cutPointsIdx = 0, lastIdx = -1;

            for (int i = 0; i < data.Length - 1; i++)
            {
                counter += (weights == null) ? 1.0 : weights[i];
                weightsSum -= (weights == null) ? 1.0 : weights[i];
                
                if (data[SortedIndices[i]] < data[SortedIndices[i + 1]])
                {
                    // Have we passed the ideal size?
                    if (counter >= freq)
                    {
                        // Is this break point worse than the last one?
                        if (((freq - last) < (counter - freq)) && (lastIdx != -1))
                        {                           
                            cutPoints[cutPointsIdx] = (data[SortedIndices[lastIdx]] + data[SortedIndices[lastIdx + 1]]) / 2;
                            counter -= last;
                            last = counter;
                            lastIdx = i;
                        }
                        else
                        {                            
                            cutPoints[cutPointsIdx] = (data[SortedIndices[i]] + data[SortedIndices[i + 1]]) / 2;
                            counter = 0;
                            last = 0;
                            lastIdx = -1;
                        }

                        cutPointsIdx++;
                        freq = (weightsSum + counter) / ((cutPoints.Length + 1) - cutPointsIdx);
                    }
                    else
                    {
                        lastIdx = i;
                        last = counter;
                    }
                }
            }

            // Check whether there was another possibility for a cut point
            if ((cutPointsIdx < cutPoints.Length) && (lastIdx != -1))
            {                
                cutPoints[cutPointsIdx] = (data[SortedIndices[lastIdx]] + data[SortedIndices[lastIdx + 1]]) / 2;
                return cutPoints;
            }
            
            return null;           
        }

        #endregion
    }
}
