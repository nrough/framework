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
    public class DiscretizeFayyadTest : DiscretizeSupervisedBaseTest
    {
        [Test]
        public void ComputeTest()
        {
            IDiscretizationSupervised fayyad = new DiscretizeFayyad();
            fayyad.Compute(data, labels, null);

            for (int i = 0; i < data.Length; i++)
                Console.WriteLine("{0} {1}", data[i], fayyad.Apply(data[i]));

            for (int i = 0; i < dataNotExisting.Length; i++)
                Console.WriteLine("{0} {1}", dataNotExisting[i], fayyad.Apply(dataNotExisting[i]));

        }
    }
}
