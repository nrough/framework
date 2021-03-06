﻿// 
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

using NRough.Core.CollectionExtensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace NRough.Core.DataStructures
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

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            foreach (var kvp in histogramData)
                sb.AppendFormat("({0} {1}) ", kvp.Key, kvp.Value);
            return sb.ToString();
        }

        #endregion Methods
    }
}