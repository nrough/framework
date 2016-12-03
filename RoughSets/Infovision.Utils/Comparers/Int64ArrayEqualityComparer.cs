using System;
using System.Collections.Generic;

namespace Infovision.Core
{
    [Serializable]
    public class Int64ArrayEqualityComparer : IEqualityComparer<long[]>
    {
        private static volatile Int64ArrayEqualityComparer instance;
        private static readonly object syncRoot = new object();

        public static Int64ArrayEqualityComparer Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                            instance = new Int64ArrayEqualityComparer();
                    }
                }

                return instance;
            }
        }

        private Int64ArrayEqualityComparer() { }

        public bool Equals(long[] x, long[] y)
        {
            if (x == y) return true;
            if (x == null || y == null) return false;
            if (x.Length != y.Length) return false;

            for (int i = x.Length - 1; i >= 0 ; i--)
                if (x[i] != y[i])
                    return false;

            return true;
        }

        public int GetHashCode(long[] array)
        {
            return HashHelper.GetHashCode<long>(array);
            //return HashHelper.ArrayHashMedium<long>(array);
            //return HashHelper.LongArrayHash(array);
        }
    }
}