using System;

namespace Infovision.Datamining.Roughset
{
    public class ReductCacheInfo
    {
        private decimal epsilonThreshold = -1;
        private decimal notReductEpsilonThreshold = -1;
        private bool reductThresholdSet = false;
        private bool notReductThresholdSet = false;

        public ReductCacheInfo(bool isReduct, decimal epsilon)
        {
            this.SetApproximationRanges(isReduct, epsilon);
        }

        public void SetApproximationRanges(bool isReduct, decimal epsilon)
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

        public NoYesUnknown CheckIsReduct(decimal epsilon)
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
