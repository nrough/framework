using NRough.Core.Random;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NRough.Core.CollectionExtensions
{
    public static class IListExtensions
    {
        public static IList<T> Clone<T>
            (this IList<T> listToClone) where T : ICloneable
        {
            return listToClone.Select(item => (T)item.Clone()).ToList();
        }

        public static void Shuffle<T>(this IList<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = RandomSingleton.Random.Next(n + 1);
                T element = list.ElementAt(k);
                list.Insert(k, list.ElementAt(n));
                list.Insert(n, element);
            }
        }
    }
}
