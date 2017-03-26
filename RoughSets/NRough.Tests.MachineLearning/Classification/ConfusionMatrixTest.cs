using NRough.MachineLearning.Classification;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NRough.Tests.MachineLearning.Classification
{
    [TextFixture]
    public class ConfusionMatrixTest
    {
        [Test]
        public void ExampleTest()
        {
            long[] labels = new long[] { 1, 2, 3 };
            var confusionMatrix = new ConfusionMatrix(labels);

            confusionMatrix.AddResult(1, 1, 5);
            confusionMatrix.AddResult(1, 2, 3);
            confusionMatrix.AddResult(1, 3, 0);

            confusionMatrix.AddResult(2, 1, 2);
            confusionMatrix.AddResult(2, 2, 3);
            confusionMatrix.AddResult(2, 3, 1);

            confusionMatrix.AddResult(3, 1, 0);
            confusionMatrix.AddResult(3, 2, 2);
            confusionMatrix.AddResult(3, 3, 11);

            Console.WriteLine(confusionMatrix);

            Assert.AreEqual(5, confusionMatrix.TruePositive(1));
            Assert.AreEqual(2, confusionMatrix.FalsePositive(1));
            Assert.AreEqual(3, confusionMatrix.FalseNegative(1));
            Assert.AreEqual(17, confusionMatrix.TrueNegative(1));

            Assert.AreEqual(3, confusionMatrix.TruePositive(2));
            Assert.AreEqual(5, confusionMatrix.FalsePositive(2));
            Assert.AreEqual(3, confusionMatrix.FalseNegative(2));
            Assert.AreEqual(16, confusionMatrix.TrueNegative(2));

            Assert.AreEqual(11, confusionMatrix.TruePositive(3));
            Assert.AreEqual(1, confusionMatrix.FalsePositive(3));
            Assert.AreEqual(2, confusionMatrix.FalseNegative(3));
            Assert.AreEqual(13, confusionMatrix.TrueNegative(3));

            Console.WriteLine(confusionMatrix.TruePositiveAvg);
            Console.WriteLine(confusionMatrix.FalsePositiveAvg);
            Console.WriteLine(confusionMatrix.FalseNegativeAvg);
            Console.WriteLine(confusionMatrix.TrueNegativeAvg);


        }
    }
}
