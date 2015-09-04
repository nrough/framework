using System;
using System.Collections.Generic;

namespace Infovision.Utils
{
    [Serializable]
    public class Histogram
    {
        #region Globals

        private Dictionary<Int64, Int32> histogramData;
        private long minElement, maxElement;

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

        public void IncreaseCount(long item)
        {
            int value;
            histogramData[item] = histogramData.TryGetValue(item, out value) ? ++value : 1;
            this.SetMinMaxElement(item);
        }

        public int GetBinValue(long item)
        {
            int value;
            if (histogramData.TryGetValue(item, out value))
            {
                return value;
            }
            return 0;
        }

        private void SetMinMaxElement(long item)
        {
            if (item < minElement)
                minElement = item;
            if (item > maxElement)
                maxElement = item;
        }
        #endregion

        #region Properties

        public long MinElement
        {
            get { return minElement; }
        }
        public long MaxElement
        {
            get { return maxElement; }
        }
        public long Elements
        {
            get { return histogramData.Count; }
        }
        #endregion

    }
}
