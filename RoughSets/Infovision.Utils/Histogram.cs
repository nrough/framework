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
        private Dictionary<T, decimal> histogramData;
        private T minValue;
        private T maxValue;

        #endregion

        #region Properties

        public decimal this[T value] { get { return this.histogramData.ContainsKey(value) ? this.histogramData[value] : 0; } }
        public T Min { get { return minValue; } }
        public T Max { get { return maxValue; } }
        public int Count { get { return histogramData.Count; } }
        
        #endregion

        #region Constructors

        public Histogram(int capacity = 0)
        {
            if (capacity != 0)
                histogramData = new Dictionary<T, decimal>(capacity);
            else
                histogramData = new Dictionary<T, decimal>();

            minValue = default(T);
            maxValue = default(T);
        }        

        #endregion

        #region Methods

        public void Increase(T key, decimal value = Decimal.One)
        {
            decimal count;
            lock (syncRoot)
            {
                histogramData[key] = histogramData.TryGetValue(key, out count) ? (count + value) : value;
                this.SetMinMaxElement(key);
            }
        }

        public decimal GetBinValue(T key)
        {
            decimal value;
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

            //TODO compare two dictionaries
            if (!p.minValue.Equals(this.minValue))
                return false;

            if (p.maxValue.Equals(this.maxValue))
                return false;

            return new DictionaryComparer<T, decimal>().Equals(p.histogramData, this.histogramData);
        }

        public override int GetHashCode()
        {
            throw new NotImplementedException();    
        }

        #endregion       
    }
}
