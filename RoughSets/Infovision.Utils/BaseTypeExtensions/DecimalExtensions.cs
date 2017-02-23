using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NRough.Core
{
    public static class DecimalExtensions
    {
        public static int GetNumberOfDecimals(this decimal d)
        {
            return BitConverter.GetBytes(decimal.GetBits(d)[3])[2];
        }
    }
}
