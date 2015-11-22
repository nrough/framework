using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infovision.Utils
{
    public static class DictionaryExtensions
    {
        static Dictionary<TKey, TValue>
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

    }
}
