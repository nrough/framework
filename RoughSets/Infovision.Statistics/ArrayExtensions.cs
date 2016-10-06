using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infovision.Statistics
{
    public static class ArrayExtensions
    {
        public static T Mode<T>(this T[] array)
        {
            var freq = new Dictionary<T, int>();
            for (int i = 0; i < array.Length; i++)
            {
                int count = 0;
                freq[array[i]] = freq.TryGetValue(array[i], out count) ? ++count : 1;
            }

            T result = default(T);
            int max = Int32.MinValue;
            foreach (var kvp in freq)
                if (max < kvp.Value)
                {
                    max = kvp.Value;
                    result = kvp.Key;
                }

            return result;
        }
    }
}
