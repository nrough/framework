﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NRough.MachineLearning.Discretization
{
    public interface IDiscretizerUnsupervised : IDiscretizer
    {
        long[] ComputeCuts(long[] data, double[] weights);        
    }
}
