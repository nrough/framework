using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;

namespace Infovision.Core
{
    public static class StringExtensions
    {
        public static int GetNumberOfDecimals(this string s)
        {
            try
            {
                decimal d = Decimal.Parse(s, NumberStyles.Any, CultureInfo.InvariantCulture);
                return d.GetNumberOfDecimals();
            }
            catch
            {
                return 0;
            }
        } 
    }
}
