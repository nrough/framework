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
        [Ignore("For now do not test Tversky")]
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
        [Ignore("For now do not test Tversky")]
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


        /// <summary>
        /// <para>Generates array of 32 double vectors created as binary representation of integers from range &lt;0, 32)</para>
        /// <para>Index 0 is 0.0 0.0 0.0 0.0 0.0 </para>
        /// <para>Index 1 is 1.0 0.0 0.0 0.0 0.0 </para>
        /// <para>Index 2 is 0.0 1.0 0.0 0.0 0.0 </para>
        /// <para>Index 3 is 1.0 1.0 0.0 0.0 0.0 </para>
        /// <para>...</para>
        /// <para>Index 31 is 1.0 1.0 1.0 1.0 1.0 </para>
        /// </summary>
        /// <returns></returns>
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

        [Test, TestCaseSource("GetSimilarityWeightedFunctions")]
        public void WeightedFunctionsTest(Func<double[], double[], double[], double> distance)
        {
            Console.WriteLine("{0}.{1}", distance.Method.DeclaringType.Name, distance.Method.Name);
            double[][] v = SimilarityTest.GetBinaryVectors();
            double[] w = SimilarityTest.GetWeights();
                        
            CalcDistanceWeighted("(0, 0)", distance, v[0], v[0], w);//1 
            CalcDistanceWeighted("(0, 31)", distance, v[0], v[31], w);//2
            CalcDistanceWeighted("(7, 0)", distance, v[7], v[0], w);//3
            //CalcDistanceWeighted("(7, 1)", distance, v[7], v[1], w);//4
            CalcDistanceWeighted("(7, 2)", distance, v[7], v[2], w);//5
            CalcDistanceWeighted("(7, 3)", distance, v[7], v[3], w);//6
            //CalcDistanceWeighted("(7, 4)", distance, v[7], v[4], w);//7
            //CalcDistanceWeighted("(7, 5)", distance, v[7], v[5], w);//8
            //CalcDistanceWeighted("(7, 6)", distance, v[7], v[6], w);//9
            CalcDistanceWeighted("(7, 7)", distance, v[7], v[7], w);//10

            CalcDistanceWeighted("(7, 8)", distance, v[7], v[8], w);//11
            //CalcDistanceWeighted("(7, 9)", distance, v[7], v[9], w);//12
            CalcDistanceWeighted("(7, 10)", distance, v[7], v[10], w);//13
            CalcDistanceWeighted("(7, 11)", distance, v[7], v[11], w);//14
            //CalcDistanceWeighted("(7, 12)", distance, v[7], v[12], w);//15
            //CalcDistanceWeighted("(7, 13)", distance, v[7], v[13], w);//16
            //CalcDistanceWeighted("(7, 14)", distance, v[7], v[14], w);//17
            CalcDistanceWeighted("(7, 15)", distance, v[7], v[15], w);//18
            //CalcDistanceWeighted("(7, 16)", distance, v[7], v[16], w);//19
            //CalcDistanceWeighted("(7, 17)", distance, v[7], v[17], w);//20

            //CalcDistanceWeighted("(7, 18)", distance, v[7], v[18], w);//21
            //CalcDistanceWeighted("(7, 19)", distance, v[7], v[19], w);//22
            //CalcDistanceWeighted("(7, 20)", distance, v[7], v[20], w);//23
            //CalcDistanceWeighted("(7, 21)", distance, v[7], v[21], w);//24
            //CalcDistanceWeighted("(7, 22)", distance, v[7], v[22], w);//25
            //CalcDistanceWeighted("(7, 23)", distance, v[7], v[23], w);//26
            CalcDistanceWeighted("(7, 24)", distance, v[7], v[24], w);//27
            CalcDistanceWeighted("(7, 25)", distance, v[7], v[25], w);//28
            //CalcDistanceWeighted("(7, 26)", distance, v[7], v[26], w);//29
            CalcDistanceWeighted("(7, 27)", distance, v[7], v[27], w);//30

            //CalcDistanceWeighted("(7, 28)", distance, v[7], v[28], w);//31
            //CalcDistanceWeighted("(7, 29)", distance, v[7], v[29], w);//32
            //CalcDistanceWeighted("(7, 30)", distance, v[7], v[30], w);//33
            CalcDistanceWeighted("(7, 31)", distance, v[7], v[31], w);//34
            CalcDistanceWeighted("(31, 31)", distance, v[31], v[31], w);//35
                     
        }

        public double CalcDistanceWeighted(string id, Func<double[], double[], double[], double> distance, double[] v1, double[] v2, double[] w)
        {            
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("{0} ", id);
            sb.Append("d([");
            for (int i = 0; i < v1.Length; i++)
            {
                sb.Append(v1[i]);
                if (i != v1.Length - 1) sb.Append(' ');
            }
            sb.Append("], [");
            for (int i = 0; i < v2.Length; i++)
            {
                sb.Append(v2[i]);
                if (i != v2.Length - 1) sb.Append(' ');
            }
            double d = distance.Invoke(v1, v2, w);
            sb.Append("]) = ");
            sb.Append(d);
            Console.WriteLine(sb.ToString());

            return d;
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

        [Test, TestCaseSource("GetSimilarityNotWeightedFunctions")]
        public void NotWeightedFunctionsTest(Func<double[], double[], double> distance)
        {
            Console.WriteLine("{0}.{1}", distance.Method.DeclaringType.Name, distance.Method.Name);
            double[][] v = SimilarityTest.GetBinaryVectors();

            CalcDistanceNotWeighted("(0, 0)", distance, v[0], v[0]);//1 
            CalcDistanceNotWeighted("(0, 31)", distance, v[0], v[31]);//2
            CalcDistanceNotWeighted("(7, 0)", distance, v[7], v[0]);//3
            //CalcDistanceNotWeighted("(7, 1)", distance, v[7], v[1]);//4
            CalcDistanceNotWeighted("(7, 2)", distance, v[7], v[2]);//5
            CalcDistanceNotWeighted("(7, 3)", distance, v[7], v[3]);//6
            //CalcDistanceNotWeighted("(7, 4)", distance, v[7], v[4]);//7
            //CalcDistanceNotWeighted("(7, 5)", distance, v[7], v[5]);//8
            //CalcDistanceNotWeighted("(7, 6)", distance, v[7], v[6]);//9
            CalcDistanceNotWeighted("(7, 7)", distance, v[7], v[7]);//10

            CalcDistanceNotWeighted("(7, 8)", distance, v[7], v[8]);//11
            //CalcDistanceNotWeighted("(7, 9)", distance, v[7], v[9]);//12
            CalcDistanceNotWeighted("(7, 10)", distance, v[7], v[10]);//13
            CalcDistanceNotWeighted("(7, 11)", distance, v[7], v[11]);//14
            //CalcDistanceNotWeighted("(7, 12)", distance, v[7], v[12]);//15
            //CalcDistanceNotWeighted("(7, 13)", distance, v[7], v[13]);//16
            //CalcDistanceNotWeighted("(7, 14)", distance, v[7], v[14]);//17
            CalcDistanceNotWeighted("(7, 15)", distance, v[7], v[15]);//18
            //CalcDistanceNotWeighted("(7, 16)", distance, v[7], v[16]);//19
            //CalcDistanceNotWeighted("(7, 17)", distance, v[7], v[17]);//20

            //CalcDistanceNotWeighted("(7, 18)", distance, v[7], v[18]);//21
            //CalcDistanceNotWeighted("(7, 19)", distance, v[7], v[19]);//22
            //CalcDistanceNotWeighted("(7, 20)", distance, v[7], v[20]);//23
            //CalcDistanceNotWeighted("(7, 21)", distance, v[7], v[21]);//24
            //CalcDistanceNotWeighted("(7, 22)", distance, v[7], v[22]);//25
            //CalcDistanceNotWeighted("(7, 23)", distance, v[7], v[23]);//26
            CalcDistanceNotWeighted("(7, 24)", distance, v[7], v[24]);//27
            CalcDistanceNotWeighted("(7, 25)", distance, v[7], v[25]);//28
            //CalcDistanceNotWeighted("(7, 26)", distance, v[7], v[26]);//29
            CalcDistanceNotWeighted("(7, 27)", distance, v[7], v[27]);//30

            //CalcDistanceNotWeighted("(7, 28)", distance, v[7], v[28]);//31
            //CalcDistanceNotWeighted("(7, 29)", distance, v[7], v[29]);//32
            //CalcDistanceNotWeighted("(7, 30)", distance, v[7], v[30]);//33
            CalcDistanceNotWeighted("(7, 31)", distance, v[7], v[31]);//34
            CalcDistanceNotWeighted("(31, 31)", distance, v[31], v[31]);//35 
        }

        public double CalcDistanceNotWeighted(string id, Func<double[], double[], double> distance, double[] v1, double[] v2)
        {                        
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("{0} ", id);
            sb.Append("d([");
            for (int i = 0; i < v1.Length; i++)
            {
                sb.Append(v1[i]);
                if(i != v1.Length - 1) sb.Append(' ');
            }
            sb.Append("], [");
            for (int i = 0; i < v2.Length; i++)
            { 
                sb.Append(v2[i]);
                if(i != v2.Length - 1) sb.Append(' ');
            }
            double d = distance.Invoke(v1, v2);
            sb.Append("]) = ");
            sb.Append(d);
            Console.WriteLine(sb.ToString());

            return d;
        }        
    }
}
