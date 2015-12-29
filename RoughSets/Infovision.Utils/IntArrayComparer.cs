using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infovision.Utils
{
    [Serializable]
    public class Int64ArrayEqualityComparer : IEqualityComparer<long[]>
    {
        public bool Equals(long[] x, long[] y)
        {
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
                int hash = 0;
                int step = array.Length <= 30
                         ? 1
                         : array.Length <= 100 ? 2
                         : array.Length <= 200 ? 4
                         : array.Length <= 500 ? 8 : 16;

                for (int i = 0; i < array.Length; i += step)
                    hash = 31 * hash + array[i].GetHashCode();
                return hash;
            }
            
        }
    }
}
