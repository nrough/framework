using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infovision.Math
{
    public static class Similarity
    {
        private static readonly double tinyDouble = 0.0000000001;

        public static double Euclidean(double[] a, double[] b)
        {
            return System.Math.Sqrt(Similarity.SquaredEuclidean(a, b));
        }

        public static double Manhattan(double[] a, double[] b)
        {
            double sum = 0;
            for (int i = 0; i < a.Length; i++)
                sum += System.Math.Abs(a[i] - b[i]);
            return sum;
        }

        public static double Hamming(int[] a, int[] b)
        {
            double sum = 0;
            for (int i = 0; i < a.Length; i++)
                if (a[i] != b[i])
                    sum++;
            return sum;
        }

        public static double Hamming(string a, string b)
        {
            char[] ac = a.ToCharArray();
            char[] bc = b.ToCharArray();

            return Hamming(ac, bc);
        }        

        public static double Hamming(char[] a, char[] b)
        {
            double sum = 0;
            for (int i = 0; i < a.Length; i++)
                if (a[i] != b[i])
                    sum++;
            return sum;
        }        

        public static double Jacard(double[] a, double[] b)
        {
            double minSum = 0;
            double maxSum = 0;

            for (int i = 0; i < a.Length; i++)
            {
                minSum += System.Math.Min(a[i], b[i]);
                maxSum += System.Math.Max(a[i], b[i]);
            }

            if(maxSum == 0)
                return 1.0;

            return 1.0 - (minSum / maxSum);    
        }

        public static double Levenshtein(string s, string t)
        {
            // degenerate cases
            if (s == t) return 0;
            if (s.Length == 0) return t.Length;
            if (t.Length == 0) return s.Length;

            // create two work vectors of integer distances
            int[] v0 = new int[t.Length + 1];
            int[] v1 = new int[t.Length + 1];

            // initialize v0 (the previous row of distances)
            // this row is A[0][i]: edit distance for an empty s
            // the distance is just the number of characters to delete from t
            for (int i = 0; i < v0.Length; i++)
                v0[i] = i;

            for (int i = 0; i < s.Length; i++)
            {
                // calculate v1 (current row distances) from the previous row v0

                // first element of v1 is A[i+1][0]
                //   edit distance is delete (i+1) chars from s to match empty t
                v1[0] = i + 1;

                // use formula to fill in the rest of the row
                for (int j = 0; j < t.Length; j++)
                {
                    var cost = (s[i] == t[j]) ? 0 : 1;
                    v1[j + 1] = System.Math.Min(System.Math.Min(v1[j] + 1, v0[j + 1] + 1), v0[j] + cost);
                }

                // copy v1 (current row) to v0 (previous row) for next iteration
                for (int j = 0; j < v0.Length; j++)
                    v0[j] = v1[j];
            }

            return v1[t.Length];
        }

        public static double SquaredEuclidean(double[] a, double[] b)
        {
            double sum = 0;
            for (int i = 0; i < a.Length; i++)
                sum += System.Math.Pow((a[i] - b[i]), 2.0);
            return sum;
        }        

        public static double Tversky_DEL(double[] prototype, double[] variant, double alpha, double beta)
        {
            int[] assoc = Similarity.BinaryAssociation(prototype, variant);
            int a = assoc[0];
            int b = assoc[1];
            int c = assoc[2];

            double denominator = (alpha * a + beta * b + c);

            if (DoubleEpsilonComparer.NearlyEqual(denominator, 0.0, tinyDouble))
                return 0.0;

            return c / denominator;
        }

        public static double Tversky(double[] prototype, double[] variant, double alpha, double beta)
        {
            return Similarity.TverskyDelegate(alpha, beta).Invoke(prototype, variant);
        }

        public static Func<double[], double[], double> TverskyDelegate(double alpha, double beta)
        {
            Func<double[], double[], double> tverskyDistance = (p, v) =>
            {
                int[] assoc = Similarity.BinaryAssociation(p, v);
                int a = assoc[0];
                int b = assoc[1];
                int c = assoc[2];

                double denominator = (alpha * a + beta * b + c);

                if (DoubleEpsilonComparer.NearlyEqual(denominator, 0.0, tinyDouble))
                    return 0.0;

                return c / denominator;
            };

            return tverskyDistance;
        }

        public static double TverskySymetric(double[] prototype, double[] variant, double alpha, double beta)
        {
            int[] assoc = Similarity.BinaryAssociation(prototype, variant);
            int a = assoc[0];
            int b = assoc[1];
            int c = assoc[2];

            int x = System.Math.Min(a, b);
            int y = System.Math.Max(a, b);

            double denominator = beta * (alpha * x + (1.0 - alpha) * y) + c;

            if (DoubleEpsilonComparer.NearlyEqual(denominator, 0.0, tinyDouble))
                return 0.0;

            return c / denominator;
        }

        public static Func<double[], double[], double> TverskySymetricDelegate(double alpha, double beta)
        {
            Func<double[], double[], double> tverskyDistance = (p, v) =>
            {
                int[] assoc = Similarity.BinaryAssociation(p, v);
                int a = assoc[0];
                int b = assoc[1];
                int c = assoc[2];

                int x = System.Math.Min(a, b);
                int y = System.Math.Max(a, b);

                double denominator = beta * (alpha * x + (1.0 - alpha) * y) + c;

                if (DoubleEpsilonComparer.NearlyEqual(denominator, 0.0, tinyDouble))
                    return 0.0;

                return c / denominator;
            };

            return tverskyDistance;
        }

        /// <summary>
        /// |X| + |Y| - alpha * |X && Y|
        /// </summary>
        /// <param name="vec1"></param>
        /// <param name="vec2"></param>
        /// <param name="alpha"></param>
        /// <returns></returns>
        public static double ReductSim(double[] vec1, double[] vec2, double alpha)
        {
            return Similarity.ReductSimDelegate(alpha).Invoke(vec1, vec2);
        }

        public static Func<double[], double[], double> ReductSimDelegate(double alpha)
        {
            Func<double[], double[], double> tverskyDistance = (p, v) =>
            {
                int[] assoc = Similarity.BinaryAssociation(p, v);
                int a = assoc[0];
                int b = assoc[1];
                int c = assoc[2];

                return (a + c) + (b + c) - (alpha * c);
            };

            return tverskyDistance;
        }

        //TODO Implement Other Similarity and disimilarity measures
        //http://www.daylight.com/dayhtml/doc/theory/theory.finger.html
        //Cosine
        //Dice
        //Euclid
        //Forbes
        //Hamman
        //Jaccard
        //Kulczynski
        //Manthattan
        //Matching
        //Pearson
        //Rogers-Tanimoto
        //Rusell-Rao
        //Simpson
        //Tanimoto
        //Yule

        private static int[] BinaryAssociation(int[] vec1, int[] vec2)
        {
            int a = 0; //0
            int b = 0; //1
            int c = 0; //2
            int d = 0; //3

            for (int i = 0; i < vec1.Length; i++)
            {
                if (vec1[i] == 0 && vec2[i] == 0)
                {
                    d++;
                }
                else if (vec1[i] == 0)
                {
                    b++;
                }
                else if (vec2[i] == 0)
                {
                    a++;
                }
                else
                {
                    c++;
                }
            }

            return new int[] { a, b, c, d };
        }

        /// <summary>
        /// Calculated four binary association factors and returns them in the form of an array int[] {a, b, c, d} where:<br />
        /// a is the count of bits on in object A but not in object B.<br />
        /// b is the count of bits on in object B but not in object A. <br />   
        /// c is the count of the bits on in both object A and object B.<br />
        /// d is the count of the bits off in both object A and object B.<br />
        /// </summary>
        /// <param name="vec1"></param>
        /// <param name="vec2"></param>
        /// <returns></returns>
        private static int[] BinaryAssociation(double[] vec1, double[] vec2)
        {
            int a = 0; //0
            int b = 0; //1
            int c = 0; //2
            int d = 0; //3            

            for (int i = 0; i < vec1.Length; i++)
            {
                if (DoubleEpsilonComparer.NearlyEqual(vec1[i], 0.0, tinyDouble)
                    && DoubleEpsilonComparer.NearlyEqual(vec2[i], 0.0, tinyDouble))
                {
                    d++;
                }
                else if (DoubleEpsilonComparer.NearlyEqual(vec1[i], 0.0, tinyDouble))
                {
                    b++;
                }
                else if (DoubleEpsilonComparer.NearlyEqual(vec2[i], 0.0, tinyDouble))
                {
                    a++;
                }
                else
                {
                    c++;
                }
            }

            return new int[] { a, b, c, d };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="vec1"></param>
        /// <param name="vec2"></param>
        /// <param name="weights"></param>
        /// <returns></returns>
        private static double[] BinaryAssociationDouble(double[] vec1, double[] vec2, double[] weights)
        {
            double a = 0; //0
            double b = 0; //1
            double c = 0; //2
            double d = 0; //3            

            for (int i = 0; i < vec1.Length; i++)
            {
                if (DoubleEpsilonComparer.NearlyEqual(vec1[i], 0.0, tinyDouble)
                    && DoubleEpsilonComparer.NearlyEqual(vec2[i], 0.0, tinyDouble))
                {
                    d += weights[i];
                }
                else if (DoubleEpsilonComparer.NearlyEqual(vec1[i], 0.0, tinyDouble))
                {
                    b += weights[i];
                }
                else if (DoubleEpsilonComparer.NearlyEqual(vec2[i], 0.0, tinyDouble))
                {
                    a += weights[i];
                }
                else
                {
                    c += weights[i];
                }
            }

            return new double[] { a, b, c, d };
        }
    }
}
