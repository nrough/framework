using System;
using System.Collections.Generic;

namespace Infovision.Utils
{
    [Serializable]
    public class Histogram<T>
        where T : struct, IComparable, IFormattable, IComparable<T>, IEquatable<T>  
    {
        #region Globals

        protected object syncRoot = new object();
        private SortedDictionary<T, Int32> histogramData;
        private T minValue;
        private T maxValue;

        #endregion

        #region Constructors

        public Histogram(int capacity = 0)
        {
            histogramData = new SortedDictionary<T, int>();

            minValue = default(T);
            maxValue = default(T);
        }

        public Histogram(IComparer<T> comparer)
        {
            histogramData = new SortedDictionary<T, int>(comparer);
            minValue = default(T);
            maxValue = default(T);
        }

        #endregion

        #region Methods

        public void Increase(T value)
        {
            int count;
            lock (syncRoot)
            {
                histogramData[value] = histogramData.TryGetValue(value, out count) ? ++count : 1;
                this.SetMinMaxElement(value);
            }
        }

        public int GetBinValue(T value)
        {
            int count;
            if (histogramData.TryGetValue(value, out count))
                return count;
            return 0;
        }

        private void SetMinMaxElement(T value)
        {
            lock (syncRoot)
            {
                if (value.CompareTo(minValue) < 0)
                    minValue = value;
                if (value.CompareTo(maxValue) > 0)
                    maxValue = value;
            }
        }

        #endregion

        #region Properties

        public T Min { get { return minValue; } }
        public T Max { get { return maxValue; } }
        public int Count { get { return histogramData.Count; } }
        public int this[T value] { get { return this.histogramData.ContainsKey(value) ? this.histogramData[value] : 0; } }

        #endregion
    }
}
