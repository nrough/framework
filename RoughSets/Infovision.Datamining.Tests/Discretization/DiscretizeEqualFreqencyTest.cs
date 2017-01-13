﻿using Infovision.MachineLearning.Discretization;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infovision.MachineLearning.Tests.Discretization
{
    [TestFixture]
    public class DiscretizeEqualFreqencyTest : DiscretizeUnsupervisedTest
    {
        public override IDiscretizer GetDiscretizer()
        {
            DiscretizeEqualFreqency disc = new DiscretizeEqualFreqency();
            return disc;
        }
    }
}
