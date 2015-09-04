using System;
using System.Collections.Generic;

namespace Infovision.Utils
{
    [Serializable]
    public class Histogram
    {
        #region Globals

        private Dictionary<Int64, Int32> histogramData;
        private Int64 minElement, maxElement;

        #endregion

        #region Constructors

        public Histogram()
        {
            histogramData = new Dictionary<Int64, Int32>();
            minElement = Int64.MaxValue;
            maxElement = Int64.MinValue;
        }

        #endregion

        #region Methods

        public void IncreaseCount(Int64 item)
        {
            Int32 value;
            histogramData[item] = histogramData.TryGetValue(item, out value) ? ++value : 1;
            this.SetMinMaxElement(item);
        }

        public Int32 GetBinValue(Int64 item)
        {
            Int32 value;
            if (histogramData.TryGetValue(item, out value))
            {
                return value;
            }
            return 0;
        }

        private void SetMinMaxElement(Int64 item)
        {
            if (item < minElement)
                minElement = item;
            if (item > maxElement)
                maxElement = item;
        }
        #endregion

        #region Properties

        public Int64 MinElement
        {
            get { return minElement; }
        }
        public Int64 MaxElement
        {
            get { return maxElement; }
        }
        public Int64 Elements
        {
            get { return histogramData.Count; }
        }
        #endregion

    }
}
