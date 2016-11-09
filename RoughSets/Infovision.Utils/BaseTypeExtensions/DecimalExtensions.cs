﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infovision.Utils
{
    public static class DecimalExtensions
    {
        public static int GetNumberOfDecimals(this decimal d)
        {
            return BitConverter.GetBytes(decimal.GetBits(d)[3])[2];
        }
    }
}