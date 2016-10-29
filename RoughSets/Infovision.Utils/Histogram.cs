using System;
using System.Collections.Generic;

namespace Infovision.Utils
{
    [Serializable]
    public class Histogram<T>
        where T : struct, IComparable, IFormattable, IComparable<T>, IEquatable<T>
    {
        #region Globals

        protected readonly object syncRoot = new object();
        private Dictionary<T, double> histogramData;
        private T minValue;
        private T maxValue;

        #endregion Globals

        #region Properties

        public double this[T value] { get { return this.histogramData.ContainsKey(value) ? this.histogramData[value] : 0; } }
        public T Min { get { return minValue; } }
        public T Max { get { return maxValue; } }
        public int Count { get { return histogramData.Count; } }

        #endregion Properties

        #region Constructors

        public Histogram(int capacity = 0)
        {
            if (capacity != 0)
                histogramData = new Dictionary<T, double>(capacity);
            else
                histogramData = new Dictionary<T, double>();

            minValue = default(T);
            maxValue = default(T);
        }

        #endregion Constructors

        #region Methods

        public void Increase(T key, double value = 1.0)
        {
            double count;
            lock (syncRoot)
            {
                histogramData[key] = histogramData.TryGetValue(key, out count) ? (count + value) : value;
                this.SetMinMaxElement(key);
            }
        }

        public double GetBinValue(T key)
        {
            double value;
            if (histogramData.TryGetValue(key, out value))
                return value;
            return 0;
        }

        private void SetMinMaxElement(T key)
        {
            if (key.CompareTo(minValue) < 0)
                minValue = key;
            if (key.CompareTo(maxValue) > 0)
                maxValue = key;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            Histogram<T> p = obj as Histogram<T>;
            if (p == null)
                return false;

            if (!p.minValue.Equals(this.minValue))
                return false;

            if (p.maxValue.Equals(this.maxValue))
                return false;

            return new DictionaryComparer<T, double>().Equals(p.histogramData, this.histogramData);
        }

        public override int GetHashCode()
        {
            throw new NotImplementedException();
        }

        #endregion Methods
    }
}