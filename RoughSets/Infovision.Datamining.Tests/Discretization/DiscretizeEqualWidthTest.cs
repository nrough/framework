using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Raccoon.MachineLearning.Discretization;

namespace Raccoon.MachineLearning.Tests.Discretization
{
    [TestFixture]
    public class DiscretizeEqualWidthTest : DiscretizeUnsupervisedTest
    {
        public override IDiscretizer GetDiscretizer()
        {
            return new DiscretizeEqualWidth();
        }
    }
}
