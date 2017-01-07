﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infovision.MachineLearning.Tests.Discretization
{
    public class DiscretizeBaseTest
    {
        protected long[] data = {
                        10, 12, 13, 14, 16, 40, 41, 42,
                        5, 5, 5, 5, 5, 7, 8, 9,
                        43, 44, 45, 45, 46, 47, 48, 49 };

        protected long[] dataNotExisting = { 0, 1, 2, 3, 4, 5, 6,
                                    11, 15, 16, 17, 20, 21, 22, 23, 24, 30, 31, 32, 33, 34,
                                    50, 60, 70, 80 };
    }
}