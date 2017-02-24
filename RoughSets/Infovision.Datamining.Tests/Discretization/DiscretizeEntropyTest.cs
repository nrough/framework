using NRough.MachineLearning.Discretization;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NRough.Tests.MachineLearning.Discretization
{
    [TestFixture]
    public class DiscretizeEntropyTest : DiscretizeUnsupervisedTest
    {
        public override IDiscretizer GetDiscretizer()
        {
            DiscretizeEntropy disc = new DiscretizeEntropy();
            return disc;
        }
    }
}
