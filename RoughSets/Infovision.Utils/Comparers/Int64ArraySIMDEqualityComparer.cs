using System;
using System.Collections.Generic;
using System.Numerics;

namespace Infovision.Core
{
    [Serializable]
    public class Int64ArraySIMDEqualityComparer : IEqualityComparer<long[]>
    {
        private static volatile Int64ArraySIMDEqualityComparer instance;
        private static readonly object syncRoot = new object();

        public static Int64ArraySIMDEqualityComparer Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                            instance = new Int64ArraySIMDEqualityComparer();
                    }
                }

                return instance;
            }
        }

        private Int64ArraySIMDEqualityComparer() { }

        public bool Equals(long[] x, long[] y)
        {
            if (x == y) return true;
            if (x == null || y == null) return false;
            if (x.Length != y.Length) return false;

            //TODO Implement Vector Hash calculation
            /*
            for (int i = 0; i < x.Length; i += Vector<long>.Count)
            {
                Vector<int> v = new Vector<int>(A, i) + new Vector<int>(B, i);
                v.CopyTo(C, i);
            }
            */

            for (int i = x.Length - 1; i >= 0; i--)
                if (x[i] != y[i])
                    return false;

            return true;
        }

        public int GetHashCode(long[] array)
        {
            return HashHelper.GetHashCode<long>(array);
        }
    }
}
