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
    public class DiscretizeKononenkoTest : DiscretizeSupervisedBaseTest
    {        
        [Test]
        public void ComputeTest()
        {
            IDiscretizationSupervised kononenko = new DiscretizeKononenko();
            kononenko.Compute(data, labels, null);

            for (int i = 0; i < data.Length; i++)
                Console.WriteLine("{0} {1}", data[i], kononenko.Apply(data[i]));

            for (int i = 0; i < dataNotExisting.Length; i++)
                Console.WriteLine("{0} {1}", dataNotExisting[i], kononenko.Apply(dataNotExisting[i]));

        }
    }
}
