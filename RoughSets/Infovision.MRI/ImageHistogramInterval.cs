using System;

namespace Raccoon.MRI
{
    [Serializable]
    public class ImageHistogramInterval
    {
        public double LowerBound { get; set; }
        public double UpperBound { get; set; }
        public int Label { get; set; }

        public ImageHistogramInterval()
        {
        }

        public ImageHistogramInterval(double lowerBound, double upperBound, int label)
            : this()
        {
            this.LowerBound = lowerBound;
            this.UpperBound = upperBound;
            this.Label = label;
        }
    }
}