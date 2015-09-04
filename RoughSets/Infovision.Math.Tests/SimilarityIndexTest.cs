using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit;
using NUnit.Framework;

namespace Infovision.Math.Tests
{
    [TestFixture]
    class SimilarityTest
    {
        private double[][] vectors = new double[][] { 
            new double[] { 0.5, 0.5, 0.5, 0.0, 0.0, 0.0, 0.33, 0.33, 0.33, 0.33 },
            new double[] { 0.5, 0.5, 0.0, 0.0, 0.0, 0.0, 0.0, 0.33, 0.33, 0.33 },
            new double[] { 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0 },
            new double[] { 0.5, 0.5, 0.5, 0.33, 0.33, 0.33, 0.33, 0.33, 0.33, 0.33 }
        };

        [TestCase(0, 0, 1.0, 1.0)]
        [TestCase(0, 0, 1.0, 0.0)]
        [TestCase(0, 0, 0.0, 1.0)]
        [TestCase(0, 0, 0.5, 0.5)]
        [TestCase(0, 0, 0.3, 0.7)]
        [TestCase(0, 0, 0.7, 0.3)]
        [TestCase(0, 0, 0.0, 0.0)]

        [TestCase(1, 0, 1.0, 1.0)]
        [TestCase(1, 0, 1.0, 0.0)]
        [TestCase(1, 0, 0.0, 1.0)]
        [TestCase(1, 0, 0.5, 0.5)]
        [TestCase(1, 0, 0.3, 0.7)]
        [TestCase(1, 0, 0.7, 0.3)]
        [TestCase(1, 0, 0.0, 0.0)]

        [TestCase(0, 1, 1.0, 1.0)]
        [TestCase(0, 1, 1.0, 0.0)]
        [TestCase(0, 1, 0.0, 1.0)]
        [TestCase(0, 1, 0.5, 0.5)]
        [TestCase(0, 1, 0.3, 0.7)]
        [TestCase(0, 1, 0.7, 0.3)]
        [TestCase(0, 1, 0.0, 0.0)]

        [TestCase(2, 3, 1.0, 1.0)]
        [TestCase(2, 3, 1.0, 0.0)]
        [TestCase(2, 3, 0.0, 1.0)]
        [TestCase(2, 3, 0.5, 0.5)]
        [TestCase(2, 3, 0.3, 0.7)]
        [TestCase(2, 3, 0.7, 0.3)]
        [TestCase(2, 3, 0.0, 0.0)]

        [TestCase(3, 2, 1.0, 1.0)]
        [TestCase(3, 2, 1.0, 0.0)]
        [TestCase(3, 2, 0.0, 1.0)]
        [TestCase(3, 2, 0.5, 0.5)]
        [TestCase(3, 2, 0.3, 0.7)]
        [TestCase(3, 2, 0.7, 0.3)]
        [TestCase(3, 2, 0.0, 0.0)]

        [TestCase(2, 2, 1.0, 1.0)]
        [TestCase(2, 2, 1.0, 0.0)]
        [TestCase(2, 2, 0.0, 1.0)]
        [TestCase(2, 2, 0.5, 0.5)]
        [TestCase(2, 2, 0.3, 0.7)]
        [TestCase(2, 2, 0.7, 0.3)]
        [TestCase(2, 2, 0.0, 0.0)]

        [TestCase(3, 3, 1.0, 1.0)]
        [TestCase(3, 3, 1.0, 0.0)]
        [TestCase(3, 3, 0.0, 1.0)]
        [TestCase(3, 3, 0.5, 0.5)]
        [TestCase(3, 3, 0.3, 0.7)]
        [TestCase(3, 3, 0.7, 0.3)]
        [TestCase(3, 3, 0.0, 0.0)]
        public void TverskyTest(int prototypeIdx, int variantIdx, double alpha, double beta)
        {
            RunTversky(vectors[prototypeIdx],
                       vectors[variantIdx],
                       alpha,
                       beta);
        }

        private void RunTversky(double[] vec1, double[] vec2, double alpha, double beta)
        {
            ShowVector(vec1);
            ShowVector(vec2);
            double result = Similarity.Tversky(vec1, vec2, alpha, beta);
            Assert.That(result, Is.Not.NaN);
            Console.WriteLine("Tversky ({0}; {1}): {2}", alpha, beta, result);
            Assert.IsTrue(true);
        }

        private void ShowVector(double[] vector)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < vector.Length; i++)
                sb.Append(String.Format("{0:f2}", vector[i])).Append(" ");
            Console.WriteLine(sb.ToString());
        }

        [TestCase(1.0, 1.0)]
        [TestCase(1.0, 0.0)]
        [TestCase(0.0, 1.0)]
        [TestCase(0.5, 0.5)]
        public void TverskySymetricTest(double alpha, double beta)
        {
            RunTverskySymetric(vectors[1], vectors[1], alpha, beta);
            RunTverskySymetric(vectors[1], vectors[1], alpha, beta);
            RunTverskySymetric(vectors[1], vectors[1], alpha, beta);
            RunTverskySymetric(vectors[1], vectors[1], alpha, beta);
        }

        private void RunTverskySymetric(double[] vec1, double[] vec2, double alpha, double beta)
        {
            double result = Similarity.TverskySymetric(vec1, vec2, alpha, beta);
            Console.WriteLine("Tversky ({0}; {1}): {2}", alpha, beta, result);
            Assert.IsTrue(true);
        }             
    }
}
