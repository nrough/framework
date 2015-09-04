using System;
using System.Collections;
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


        public static double[][] GetBinaryVectors()
        {
            double[][] result = new double[32][];
            int k;

            for (int i = 0; i < 32; i++)
            {
                result[i] = new double[5];
                BitArray bits = new BitArray(new int[] { i });
                for (int j = 0; j < bits.Count && j < 5; j++)
                {
                    if (bits[j])
                    {
                        result[i][j] = 1.0;
                    }
                }
            }

            return result;
        }

        public static double[] GetWeights()
        {
            double[] w = new double[32];
            for (int i = 0; i < 32; i++)
                w[i] = 1.0;
            return w;
        }
        
        public static IEnumerable<Func<double[], double[], double[], double>> GetSimilarityWeightedFunctions()
        {
                        
            List<Func<double[], double[], double[], double>> list = new List<Func<double[], double[], double[], double>>();
            
            list.Add(Similarity.CosineB);
            list.Add(Similarity.DiceB);
            list.Add(Similarity.EuclideanB);
            list.Add(Similarity.ForbesB);
            list.Add(Similarity.HammanB);
            list.Add(Similarity.JaccardB);
            list.Add(Similarity.KulczynskiB);
            list.Add(Similarity.ManhattanB);
            list.Add(Similarity.MatchingB);
            list.Add(Similarity.PearsonB);
            list.Add(Similarity.RogersTanimotoB);
            list.Add(Similarity.RussellRaoB);
            list.Add(Similarity.SimpsonB);
            list.Add(Similarity.TanimotoB);
            list.Add(Similarity.YuleB);
            list.Add(Similarity.SokalSneathB);
            list.Add(Similarity.KulsinskiB);

            //TODO Tversky
            //TODO Minkowski

            return list;
        }

        public static IEnumerable<Func<double[], double[], double>> GetSimilarityNotWeightedFunctions()
        {
            List<Func<double[], double[], double>> list = new List<Func<double[], double[], double>>();

            list.Add(Similarity.Euclidean);
            list.Add(Similarity.Manhattan);
            list.Add(Similarity.Hamming);
            list.Add(Similarity.JaccardFuzzy);
            list.Add(Similarity.SquaredEuclidean);
            list.Add(Similarity.Cosine);
            list.Add(Similarity.BrayCurtis);
            list.Add(Similarity.Canberra);

            return list;
        }

        [Test, TestCaseSource("GetSimilarityWeightedFunctions")]
        public void WeightedFunctionsTest(Func<double[], double[], double[], double> distance)
        {
                        
            double[][] v = SimilarityTest.GetBinaryVectors();
            double[] w = SimilarityTest.GetWeights();

            //TODO select vectors
            
            CalcDistanceWeighted(distance, v[0], v[1], w);
            CalcDistanceWeighted(distance, v[0], v[1], w);
            CalcDistanceWeighted(distance, v[0], v[1], w);
            CalcDistanceWeighted(distance, v[0], v[1], w);
            CalcDistanceWeighted(distance, v[0], v[1], w);
        }

        public void CalcDistanceWeighted(Func<double[], double[], double[], double> distance, double[] v1, double[] v2, double[] w)
        {
            Console.WriteLine("{0}.{1}", distance.Method.DeclaringType.Name, distance.Method.Name);

            //TODO print v1
            //TODO print v2
            //TODO print distance
        }

        [Test, TestCaseSource("GetSimilarityNotWeightedFunctions")]
        public void WeightedFunctionsTest(Func<double[], double[], double> distance)
        {

            double[][] v = SimilarityTest.GetBinaryVectors();

            //TODO select vectors

            CalcDistanceNotWeighted(distance, v[0], v[1]);
            CalcDistanceNotWeighted(distance, v[0], v[1]);
            CalcDistanceNotWeighted(distance, v[0], v[1]);
            CalcDistanceNotWeighted(distance, v[0], v[1]);
            CalcDistanceNotWeighted(distance, v[0], v[1]);
        }

        public void CalcDistanceNotWeighted(Func<double[], double[], double> distance, double[] v1, double[] v2)
        {
            Console.WriteLine("{0}.{1}", distance.Method.DeclaringType.Name, distance.Method.Name);

            //TODO print v1
            //TODO print v2
            //TODO print distance
        }
    }
}
