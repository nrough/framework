using Infovision.MachineLearning.Discretization;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infovision.MachineLearning.Tests.Discretization
{
    [TestFixture]
    public class DiscretizeEntropyTest : DiscretizeUnsupervisedTest
    {
        public override IDiscretization GetDiscretizer()
        {
            DiscretizeEntropy disc = new DiscretizeEntropy();
            return disc;
        }
    }
}
