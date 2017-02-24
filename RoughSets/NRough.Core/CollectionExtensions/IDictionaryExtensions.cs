using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NRough.Core
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
