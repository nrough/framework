using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Reflection;

namespace NRough.Core.CollectionExtensions
{
    public static class IEnumerableExternsions
    {
        public static bool SetEquals<T>(this IEnumerable<T> first, IEnumerable<T> second, IEqualityComparer<T> comparer)
        {
            return new HashSet<T>(second, comparer ?? EqualityComparer<T>.Default).SetEquals(first);
        }
    }
}