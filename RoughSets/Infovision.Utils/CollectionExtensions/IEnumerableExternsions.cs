using System.Collections.Generic;

namespace Infovision.Utils
{
    public static class IEnumerableExternsions
    {
        public static bool SetEquals<T>(this IEnumerable<T> first, IEnumerable<T> second, IEqualityComparer<T> comparer)
        {
            return new HashSet<T>(second, comparer ?? EqualityComparer<T>.Default).SetEquals(first);
        }
    }
}