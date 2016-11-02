using System;
using System.Collections.Generic;
using System.Linq;

namespace Infovision.Utils
{
    public static class DictionaryExtensions
    {
        private static Dictionary<TKey, TValue>
            Merge<TKey, TValue>(this IEnumerable<Dictionary<TKey, TValue>> enumerable)
        {
            return enumerable.SelectMany(x => x).ToDictionary(x => x.Key, y => y.Value);
        }

        // Works in C#3/VS2008:
        // Returns a new dictionary of this ... others merged leftward.
        // Keeps the type of 'this', which must be default-instantiable.
        // Example:
        //   result = map.MergeLeft(other1, other2, ...)
        public static T MergeLeft<T, K, V>(this T me, params IDictionary<K, V>[] others)
            where T : IDictionary<K, V>, new()
        {
            T newMap = new T();
            foreach (IDictionary<K, V> src in
                (new List<IDictionary<K, V>> { me }).Concat(others))
            {
                // ^-- echk. Not quite there type-system.
                foreach (KeyValuePair<K, V> p in src)
                {
                    newMap[p.Key] = p.Value;
                }
            }
            return newMap;
        }

        public static Dictionary<TKey, TValue> CloneDictionaryCloningValues<TKey, TValue>
            (this Dictionary<TKey, TValue> original)
            where TKey : ICloneable
            where TValue : ICloneable
        {
            Dictionary<TKey, TValue> ret = new Dictionary<TKey, TValue>(original.Count, original.Comparer);
            foreach (KeyValuePair<TKey, TValue> entry in original)
                ret.Add((TKey)entry.Key.Clone(), (TValue)entry.Value.Clone());
            return ret;
        }

        public static Dictionary<TKey, TValue> Clone<TKey, TValue>
            (this Dictionary<TKey, TValue> original)
            where TKey : struct, IComparable
            where TValue : struct, IComparable
        {
            Dictionary<TKey, TValue> ret = new Dictionary<TKey, TValue>(original.Count, original.Comparer);
            foreach (KeyValuePair<TKey, TValue> entry in original)
                ret.Add(entry.Key, entry.Value);
            return ret;
        }

        public static TKey FindMaxValueKey<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, IComparer<TValue> comparer = null)
        {
            return dictionary.FindMaxValuePair(comparer).Key;
        }        

        public static KeyValuePair<long, double> FindMaxValuePair(this Dictionary<long, double> dictionary, IComparer<double> comparer = null)
        {
            if (comparer == null)
                comparer = Comparer<double>.Default;

            KeyValuePair<long, double> result = new KeyValuePair<long, double>(-1, 0.0);
            foreach (var kvp in dictionary)
                if (comparer.Compare(kvp.Value, result.Value) > 0)
                    result = kvp;
            return result;
        }

        public static KeyValuePair<long, int> FindMaxValuePair(this Dictionary<long, int> dictionary, IComparer<int> comparer = null)
        {
            if (comparer == null)
                comparer = Comparer<int>.Default;

            KeyValuePair<long, int> result = new KeyValuePair<long, int>(-1, 0);
            foreach (var kvp in dictionary)
                if (comparer.Compare(kvp.Value, result.Value) > 0)
                    result = kvp;
            return result;
        }

        public static KeyValuePair<TKey, TValue> FindMaxValuePair<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, IComparer<TValue> comparer = null)
        {
            if (comparer == null)
                comparer = Comparer<TValue>.Default;

            KeyValuePair<TKey, TValue> result = dictionary.FirstOrDefault();
            foreach (var kvp in dictionary)
                if (comparer.Compare(kvp.Value, result.Value) > 0)
                    result = kvp;
            return result;
        }
    }

    /// <summary>
    /// http://stackoverflow.com/questions/21758074/c-sharp-compare-two-dictionaries-for-equality
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    public class DictionaryComparer<TKey, TValue> :
            IEqualityComparer<Dictionary<TKey, TValue>>
    {
        private IEqualityComparer<TValue> valueComparer;

        public DictionaryComparer(IEqualityComparer<TValue> valueComparer = null)
        {
            this.valueComparer = valueComparer ?? EqualityComparer<TValue>.Default;
        }

        public bool Equals(Dictionary<TKey, TValue> x, Dictionary<TKey, TValue> y)
        {
            if (x.Count != y.Count)
                return false;
            if (x.Keys.Except(y.Keys).Any())
                return false;
            if (y.Keys.Except(x.Keys).Any())
                return false;
            foreach (var pair in x)
                if (!valueComparer.Equals(pair.Value, y[pair.Key]))
                    return false;
            return true;
        }

        public int GetHashCode(Dictionary<TKey, TValue> obj)
        {
            throw new NotImplementedException();
        }
    }
}