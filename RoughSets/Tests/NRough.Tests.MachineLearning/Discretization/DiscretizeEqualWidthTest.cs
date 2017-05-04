using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NRough.MachineLearning.Discretization;

namespace NRough.Tests.MachineLearning.Discretization
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
