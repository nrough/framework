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

namespace NRough.MachineLearning.Roughsets
{
    public class ReductCacheInfo
    {
        private double epsilonThreshold = -1;
        private double notReductEpsilonThreshold = -1;
        private bool reductThresholdSet = false;
        private bool notReductThresholdSet = false;

        public ReductCacheInfo(bool isReduct, double epsilon)
        {
            this.SetApproximationRanges(isReduct, epsilon);
        }

        public void SetApproximationRanges(bool isReduct, double epsilon)
        {
            if (isReduct)
            {
                if (epsilon <= this.epsilonThreshold
                        || this.reductThresholdSet == false)
                {
                    this.epsilonThreshold = epsilon;
                    this.reductThresholdSet = true;
                }
            }
            else
            {
                if (epsilon >= this.notReductEpsilonThreshold)
                {
                    this.notReductEpsilonThreshold = epsilon;
                    this.notReductThresholdSet = true;
                }
            }

            if (this.reductThresholdSet == true
                && this.notReductThresholdSet == true
                && this.epsilonThreshold <= this.notReductEpsilonThreshold)
            {
                throw new InvalidOperationException("Reduct approximation ranges are overlapping");
            }
        }

        public NoYesUnknown CheckIsReduct(double epsilon)
        {
            /*
            if (this.reductThresholdSet == true
                && epsilon >= (this.epsilonThreshold - 0.000000001))
                return NoYesUnknown.Yes;

            if (this.notReductThresholdSet == true
                && epsilon <= (this.notReductEpsilonThreshold + 0.000000001))
                return NoYesUnknown.No;
            */

            if (this.reductThresholdSet == true
                && epsilon >= this.epsilonThreshold)
                return NoYesUnknown.Yes;

            if (this.notReductThresholdSet == true
                && epsilon <= this.notReductEpsilonThreshold)
                return NoYesUnknown.No;

            return NoYesUnknown.Unknown;
        }
    }
}