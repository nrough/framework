// 
// NRough Framework
// http://www.nrough.net
// 
// Copyright (c) Sebastian Widz, 2017
// authors at nrough.net
// 
// This file is part of NRough Framework.
// 
// NRough Framework is free software; you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation; either version 3 of the License, or
// (at your option) any later version.
// 
// NRough Framework is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with NRough Framework; if not, write to the Free Software
// Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA  02110-1301  USA
//

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using NRough.Math;

namespace NRough.Tests.Math
{
    [TestFixture]
    internal class SimilarityTest
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
            double result = Distance.Tversky(vec1, vec2, alpha, beta);
            Assert.That(result, Is.Not.NaN);
            //Console.WriteLine("Tversky ({0}; {1}): {2}", alpha, beta, result);
            Assert.IsTrue(true);
        }

        private void ShowVector(double[] vector)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < vector.Length; i++)
                sb.Append(String.Format("{0:f2}", vector[i])).Append(" ");
            //Console.WriteLine(sb.ToString());
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
            double result = Distance.TverskySymetric(vec1, vec2, alpha, beta);
            //Console.WriteLine("Tversky ({0}; {1}): {2}", alpha, beta, result);
            Assert.IsTrue(true);
        }

        private class DistanceFunctionResult
        {
            public double Distance { get; set; }
            public string Description { get; set; }

            public override string ToString()
            {
                return this.Description;
            }
        }

        private class DistanceFunctionResultComparer : Comparer<DistanceFunctionResult>
        {
            public override int Compare(DistanceFunctionResult left, DistanceFunctionResult right)
            {
                return left.Distance.CompareTo(right.Distance);
            }
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
        private static double[][] GetBinaryVectors()
        {
            double[][] result = new double[32][];
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

        private static double[] GetWeights()
        {
            double[] w = new double[32];
            for (int i = 0; i < 32; i++)
                w[i] = 1.0;
            return w;
        }

        public static IEnumerable<Func<double[], double[], double[], double>> GetSimilarityWeightedFunctions()
        {
            List<Func<double[], double[], double[], double>> list = new List<Func<double[], double[], double[], double>>();

            list.Add(Distance.CosineB);
            list.Add(Distance.DiceB);
            list.Add(Distance.EuclideanB);
            list.Add(Distance.ForbesB);
            list.Add(Distance.HammanB);
            list.Add(Distance.JaccardB);
            list.Add(Distance.KulczynskiB);
            list.Add(Distance.ManhattanB);
            list.Add(Distance.MatchingB);
            list.Add(Distance.PearsonB);
            list.Add(Distance.RogersTanimotoB);
            list.Add(Distance.RussellRaoB);
            list.Add(Distance.SimpsonB);
            list.Add(Distance.TanimotoB);
            list.Add(Distance.YuleB);
            list.Add(Distance.SokalSneathB);
            list.Add(Distance.KulsinskiB);

            //TODO Tversky
            //TODO Minkowski

            return list;
        }

        [Test, TestCaseSource("GetSimilarityWeightedFunctions")]
        public void WeightedFunctionsTest(Func<double[], double[], double[], double> distance)
        {
            //Console.WriteLine("{0}.{1}", distance.Method.DeclaringType.Name, distance.Method.Name);
            double[][] v = SimilarityTest.GetBinaryVectors();
            double[] w = SimilarityTest.GetWeights();

            List<DistanceFunctionResult> testResults = new List<DistanceFunctionResult>();

            CalcDistanceWeighted("(0, 0)", distance, v[0], v[0], w, testResults);//1
            CalcDistanceWeighted("(0, 31)", distance, v[0], v[31], w, testResults);//2
            CalcDistanceWeighted("(7, 0)", distance, v[7], v[0], w, testResults);//3
            //CalcDistanceWeighted("(7, 1)", distance, v[7], v[1], w, testResults);//4
            CalcDistanceWeighted("(7, 2)", distance, v[7], v[2], w, testResults);//5
            CalcDistanceWeighted("(7, 3)", distance, v[7], v[3], w, testResults);//6
            //CalcDistanceWeighted("(7, 4)", distance, v[7], v[4], w, testResults);//7
            //CalcDistanceWeighted("(7, 5)", distance, v[7], v[5], w, testResults);//8
            //CalcDistanceWeighted("(7, 6)", distance, v[7], v[6], w, testResults);//9
            CalcDistanceWeighted("(7, 7)", distance, v[7], v[7], w, testResults);//10

            CalcDistanceWeighted("(7, 8)", distance, v[7], v[8], w, testResults);//11
            //CalcDistanceWeighted("(7, 9)", distance, v[7], v[9], w, testResults);//12
            CalcDistanceWeighted("(7, 10)", distance, v[7], v[10], w, testResults);//13
            CalcDistanceWeighted("(7, 11)", distance, v[7], v[11], w, testResults);//14
            //CalcDistanceWeighted("(7, 12)", distance, v[7], v[12], w, testResults);//15
            //CalcDistanceWeighted("(7, 13)", distance, v[7], v[13], w, testResults);//16
            //CalcDistanceWeighted("(7, 14)", distance, v[7], v[14], w, testResults);//17
            CalcDistanceWeighted("(7, 15)", distance, v[7], v[15], w, testResults);//18
            //CalcDistanceWeighted("(7, 16)", distance, v[7], v[16], w, testResults);//19
            //CalcDistanceWeighted("(7, 17)", distance, v[7], v[17], w, testResults);//20

            //CalcDistanceWeighted("(7, 18)", distance, v[7], v[18], w, testResults);//21
            //CalcDistanceWeighted("(7, 19)", distance, v[7], v[19], w, testResults);//22
            //CalcDistanceWeighted("(7, 20)", distance, v[7], v[20], w, testResults);//23
            //CalcDistanceWeighted("(7, 21)", distance, v[7], v[21], w, testResults);//24
            //CalcDistanceWeighted("(7, 22)", distance, v[7], v[22], w, testResults);//25
            //CalcDistanceWeighted("(7, 23)", distance, v[7], v[23], w, testResults);//26
            CalcDistanceWeighted("(7, 24)", distance, v[7], v[24], w, testResults);//27
            CalcDistanceWeighted("(7, 25)", distance, v[7], v[25], w, testResults);//28
            //CalcDistanceWeighted("(7, 26)", distance, v[7], v[26], w, testResults);//29
            CalcDistanceWeighted("(7, 27)", distance, v[7], v[27], w, testResults);//30

            //CalcDistanceWeighted("(7, 28)", distance, v[7], v[28], w, testResults);//31
            //CalcDistanceWeighted("(7, 29)", distance, v[7], v[29], w, testResults);//32
            //CalcDistanceWeighted("(7, 30)", distance, v[7], v[30], w, testResults);//33
            CalcDistanceWeighted("(7, 31)", distance, v[7], v[31], w, testResults);//34
            CalcDistanceWeighted("(31, 31)", distance, v[31], v[31], w, testResults);//35

            testResults.Sort(new DistanceFunctionResultComparer());
            //foreach (DistanceFunctionResult result in testResults)
            //    Console.WriteLine(result);
        }

        private double CalcDistanceWeighted(string id, Func<double[], double[], double[], double> distance, double[] v1, double[] v2, double[] w, List<DistanceFunctionResult> resultList)
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
            //Console.WriteLine(sb.ToString());

            resultList.Add(new DistanceFunctionResult
                {
                    Distance = d,
                    Description = sb.ToString()
                });

            return d;
        }

        public static IEnumerable<Func<double[], double[], double>> GetSimilarityNotWeightedFunctions()
        {
            List<Func<double[], double[], double>> list = new List<Func<double[], double[], double>>();

            list.Add(Distance.Euclidean);
            list.Add(Distance.Manhattan);
            list.Add(Distance.Hamming);
            list.Add(Distance.JaccardFuzzy);
            list.Add(Distance.SquaredEuclidean);
            list.Add(Distance.Cosine);
            list.Add(Distance.BrayCurtis);
            list.Add(Distance.Canberra);

            return list;
        }

        [Test, TestCaseSource("GetSimilarityNotWeightedFunctions")]
        public void NotWeightedFunctionsTest(Func<double[], double[], double> distance)
        {
            //Console.WriteLine("{0}.{1}", distance.Method.DeclaringType.Name, distance.Method.Name);
            double[][] v = SimilarityTest.GetBinaryVectors();

            List<DistanceFunctionResult> testResults = new List<DistanceFunctionResult>(41);

            CalcDistanceNotWeighted("(0, 0)", distance, v[0], v[0], testResults);//1
            CalcDistanceNotWeighted("(0, 31)", distance, v[0], v[31], testResults);//2
            CalcDistanceNotWeighted("(7, 0)", distance, v[7], v[0], testResults);//3
            //CalcDistanceNotWeighted("(7, 1)", distance, v[7], v[1], testResults);//4
            CalcDistanceNotWeighted("(7, 2)", distance, v[7], v[2], testResults);//5
            CalcDistanceNotWeighted("(7, 3)", distance, v[7], v[3], testResults);//6
            //CalcDistanceNotWeighted("(7, 4)", distance, v[7], v[4], testResults);//7
            //CalcDistanceNotWeighted("(7, 5)", distance, v[7], v[5], testResults);//8
            //CalcDistanceNotWeighted("(7, 6)", distance, v[7], v[6], testResults);//9
            CalcDistanceNotWeighted("(7, 7)", distance, v[7], v[7], testResults);//10

            CalcDistanceNotWeighted("(7, 8)", distance, v[7], v[8], testResults);//11
            //CalcDistanceNotWeighted("(7, 9)", distance, v[7], v[9], testResults);//12
            CalcDistanceNotWeighted("(7, 10)", distance, v[7], v[10], testResults);//13
            CalcDistanceNotWeighted("(7, 11)", distance, v[7], v[11], testResults);//14
            //CalcDistanceNotWeighted("(7, 12)", distance, v[7], v[12], testResults);//15
            //CalcDistanceNotWeighted("(7, 13)", distance, v[7], v[13], testResults);//16
            //CalcDistanceNotWeighted("(7, 14)", distance, v[7], v[14], testResults);//17
            CalcDistanceNotWeighted("(7, 15)", distance, v[7], v[15], testResults);//18
            //CalcDistanceNotWeighted("(7, 16)", distance, v[7], v[16], testResults);//19
            //CalcDistanceNotWeighted("(7, 17)", distance, v[7], v[17], testResults);//20

            //CalcDistanceNotWeighted("(7, 18)", distance, v[7], v[18], testResults);//21
            //CalcDistanceNotWeighted("(7, 19)", distance, v[7], v[19], testResults);//22
            //CalcDistanceNotWeighted("(7, 20)", distance, v[7], v[20], testResults);//23
            //CalcDistanceNotWeighted("(7, 21)", distance, v[7], v[21], testResults);//24
            //CalcDistanceNotWeighted("(7, 22)", distance, v[7], v[22], testResults);//25
            //CalcDistanceNotWeighted("(7, 23)", distance, v[7], v[23], testResults);//26
            CalcDistanceNotWeighted("(7, 24)", distance, v[7], v[24], testResults);//27
            CalcDistanceNotWeighted("(7, 25)", distance, v[7], v[25], testResults);//28
            //CalcDistanceNotWeighted("(7, 26)", distance, v[7], v[26], testResults);//29
            CalcDistanceNotWeighted("(7, 27)", distance, v[7], v[27], testResults);//30

            //CalcDistanceNotWeighted("(7, 28)", distance, v[7], v[28], testResults);//31
            //CalcDistanceNotWeighted("(7, 29)", distance, v[7], v[29], testResults);//32
            //CalcDistanceNotWeighted("(7, 30)", distance, v[7], v[30], testResults);//33
            CalcDistanceNotWeighted("(7, 31)", distance, v[7], v[31], testResults);//34
            CalcDistanceNotWeighted("(31, 31)", distance, v[31], v[31], testResults);//35

            testResults.Sort(new DistanceFunctionResultComparer());
            //foreach (DistanceFunctionResult result in testResults)
            //    Console.WriteLine(result);
        }

        private double CalcDistanceNotWeighted(string id, Func<double[], double[], double> distance, double[] v1, double[] v2, List<DistanceFunctionResult> resultList)
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
            double d = distance.Invoke(v1, v2);
            sb.Append("]) = ");
            sb.Append(d);
            //Console.WriteLine(sb.ToString());

            resultList.Add(new DistanceFunctionResult
            {
                Distance = d,
                Description = sb.ToString()
            });

            return d;
        }
    }
}