using System;

namespace Infovision.Datamining.Roughset
{
    public class ReductCacheInfo
    {
        private double reductApproximationThreshold = -1;
        private double notReductApproximationThreshold = -1;
        private bool reductThresholdSet = false;
        private bool notReductThresholdSet = false;

        public ReductCacheInfo(bool isReduct, double approximationLevel)
        {
            this.SetApproximationRanges(isReduct, approximationLevel);
        }

        public void SetApproximationRanges(bool isReduct, double approximationLevel)
        {
            if (isReduct)
            {
                if (approximationLevel <= this.reductApproximationThreshold
                        || this.reductThresholdSet == false)
                {
                    this.reductApproximationThreshold = approximationLevel;
                    this.reductThresholdSet = true;
                }
            }
            else
            {
                if (approximationLevel >= this.notReductApproximationThreshold)
                {
                    this.notReductApproximationThreshold = approximationLevel;
                    this.notReductThresholdSet = true;
                }
            }

            if (this.reductThresholdSet == true
                && this.notReductThresholdSet == true
                && this.reductApproximationThreshold <= this.notReductApproximationThreshold)
            {
                throw new InvalidOperationException("Reduct approximation ranges are overlapping");
            }
        }

        public NoYesUnknown CheckIsReduct(double approximationLevel)
        {
            if (this.reductThresholdSet == true
                && approximationLevel >= (this.reductApproximationThreshold - 0.000000001))
            {
                return NoYesUnknown.Yes;
            }

            if (this.notReductThresholdSet == true
                && approximationLevel <= (this.notReductApproximationThreshold + 0.000000001))
            {
                return NoYesUnknown.No;
            }

            return NoYesUnknown.Unknown;
        }
    }
}
