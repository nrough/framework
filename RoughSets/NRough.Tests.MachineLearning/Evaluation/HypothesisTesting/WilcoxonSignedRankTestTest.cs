using NRough.MachineLearning.Evaluation.HypothesisTesting;
using NRough.Tests.MachineLearning.Classification;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NRough.Tests.MachineLearning.Evaluation.HypothesisTesting
{
    [TextFixture]
    public class WilcoxonSignedRankTestTest
    {
        [Test]
        public void ComputeTest()
        {
            var wilcoxon = new WilcoxonSignedRankTest();
            double z = wilcoxon.Compute(
                new double[] { 0.763, 0.599, 0.954, 0.628, 0.882, 0.936, 0.661, 0.583, 0.775, 1.000, 0.940, 0.619, 0.972, 0.957 },
                new double[] { 0.768, 0.591, 0.971, 0.661, 0.888, 0.931, 0.668, 0.583, 0.838, 1.000, 0.962, 0.666, 0.981, 0.978 });

        }
    }
}
