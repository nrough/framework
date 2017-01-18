using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Raccoon.Core.CollectionExtensions
{
    public static class BitArrayExtensions
    {
        private static FieldInfo _internalArrayGetter = GetInternalArrayGetter();

        private static FieldInfo GetInternalArrayGetter()
        {
            return typeof(BitArray).GetField("m_array", BindingFlags.NonPublic | BindingFlags.Instance);
        }

        private static int[] GetInternalArray(BitArray array)
        {
            return (int[])_internalArrayGetter.GetValue(array);
        }

        public static IEnumerable<int> GetInternalValues(this BitArray array)
        {
            return GetInternalArray(array);
        }
    }
}
