using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Infovision.MachineLearning.Discretization;

namespace Infovision.MachineLearning.Tests.Discretization
{
    [TestFixture]
    public class DiscretizeEqualWidthTest : DiscretizeUnsupervisedTest
    {
        public override IDiscretization GetDiscretizer()
        {
            return new DiscretizeEqualWidth();
        }
    }
}
