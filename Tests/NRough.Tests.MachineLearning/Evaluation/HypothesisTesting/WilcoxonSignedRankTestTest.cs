using NRough.MachineLearning.Evaluation.HypothesisTesting;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NRough.Tests.MachineLearning.Evaluation.HypothesisTesting
{
    [TestFixture]
    public class WilcoxonSignedRankTestTest
    {
        [Test]
        public void Compute3Test()
        {
            double[] serie1 = new double[] { 0.63, 0.17, 0.35, 0.49, 0.18, 0.43, 0.12, 0.20, 0.47, 1.36, 0.51, 0.45, 0.84, 0.32, 0.40 };
            double[] serie2 = new double[] { 1.13, 0.54, 0.96, 0.26, 0.39, 0.88, 0.92, 0.53, 1.01, 0.48, 0.89, 1.07, 1.11, 0.58 };
            var wilcoxon = new WilcoxonSignedRankTest();
            wilcoxon.Compute(serie1, serie2);            
        }
    }
}
