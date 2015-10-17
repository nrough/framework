using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infovision.Utils
{
    public static class DateTimeUtils
    {
        public static string Ticks2Time(long ticks)
        {
            var hh = Math.Floor(ticks / 3600.0);
            var mm = Math.Floor((ticks % 3600.0) / 60.0);
            var ss = (ticks % 3600) % 60;

            return hh.ToString() + ":" + mm.ToString() + ":" + ss.ToString();
        }

    }
}
