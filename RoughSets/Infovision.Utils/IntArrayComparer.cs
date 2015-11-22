using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infovision.Utils
{
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
            return HashHelper.GetHashCode<long>(array);    
        }
    }
}
