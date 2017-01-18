using System;

namespace Raccoon.MachineLearning.Roughset
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