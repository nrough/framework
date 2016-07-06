using System;
using System.Collections.Generic;

namespace Infovision.Utils
{
    [Serializable]
    public class Int64ArrayEqualityComparer : IEqualityComparer<long[]>
    {
        private static volatile Int64ArrayEqualityComparer instance;
        private static object syncRoot = new object();

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

        private Int64ArrayEqualityComparer()
        {
        }

        public bool Equals(long[] x, long[] y)
        {
            if (x == y)
                return true;

            if (x == null || y == null)
                return false;

            if (x.Length != y.Length)
                return false;

            for (int i = 0; i < x.Length; i++)
                if (x[i] != y[i])
                    return false;

            return true;
        }

        public int GetHashCode(long[] array)
        {
            unchecked
            {
                /*
                int hash = 0;
                int step = array.Length <= 30
                         ? 1
                         : array.Length <= 100 ? 2
                         : array.Length <= 200 ? 4
                         : array.Length <= 500 ? 8 : 16;

                for (int i = 0; i < array.Length; i += step)
                    hash = 31 * hash + array[i].GetHashCode();
                return hash;
                */

                if (array == null)
                    return 0;

                int hash = 17;
                for (int i = 0; i < array.Length; i += 1)
                    hash = 31 * hash + array[i].GetHashCode();
                return hash;
            }
        }
    }
}