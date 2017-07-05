// 
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NRough.Core.CollectionExtensions
{
    public static class IDictionaryExtensions
    {
        public static TKey FindMaxValueKey<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, IComparer<TValue> comparer = null)
        {
            return dictionary.FindMaxValuePair(comparer).Key;
        }

        public static KeyValuePair<TKey, TValue> FindMaxValuePair<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, IComparer<TValue> comparer = null)
        {
            if (comparer == null)
                comparer = Comparer<TValue>.Default;

            KeyValuePair<TKey, TValue> result = dictionary.FirstOrDefault();
            foreach (var kvp in dictionary)
                if (comparer.Compare(kvp.Value, result.Value) > 0)
                    result = kvp;
            return result;
        }

        public static KeyValuePair<long, double> FindMaxValuePair(this IDictionary<long, double> dictionary, IComparer<double> comparer = null)
        {
            if (comparer == null)
                comparer = Comparer<double>.Default;

            KeyValuePair<long, double> result = new KeyValuePair<long, double>(-1, 0.0);
            foreach (var kvp in dictionary)
                if (comparer.Compare(kvp.Value, result.Value) > 0)
                    result = kvp;
            return result;
        }

        public static KeyValuePair<long, int> FindMaxValuePair(this IDictionary<long, int> dictionary, IComparer<int> comparer = null)
        {
            if (comparer == null)
                comparer = Comparer<int>.Default;

            KeyValuePair<long, int> result = new KeyValuePair<long, int>(-1, 0);
            foreach (var kvp in dictionary)
                if (comparer.Compare(kvp.Value, result.Value) > 0)
                    result = kvp;
            return result;
        }
    }
}
