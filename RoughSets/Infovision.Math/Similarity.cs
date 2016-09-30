using System;
using Infovision.Utils;

namespace Infovision.Math
{
    /// <summary>
    /// http://www.daylight.com/dayhtml/doc/theory/theory.finger.html
    /// </summary>
    public static class Similarity
    {
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

        public static double Hamming(double[] v1, double[] v2)
        {
            if (v1.Length != v2.Length)
                throw new InvalidOperationException("Arrays have different length.");

            double sum = 0.0;
            for (int i = 0; i < v1.Length; i++)
                if (!DoubleEpsilonComparer.Instance.Equals(v1[i], v2[i]))
                    sum += System.Math.Max(v1[i], v2[i]);

            return v1.Length > 0 ? sum / (double)v1.Length : 0.0;
        }

        public static double JaccardFuzzy(double[] a, double[] b)
        {
            double minSum = 0.0;
            double maxSum = 0.0;

            for (int i = 0; i < a.Length; i++)
            {
                minSum += System.Math.Min(a[i], b[i]);
                maxSum += System.Math.Max(a[i], b[i]);
            }

            if (maxSum == 0)
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
                sum += (a[i] - b[i]) * (a[i] - b[i]); //System.Math.Pow((a[i] - b[i]), 2.0);
            return sum;
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

                if (DoubleEpsilonComparer.Instance.Equals(denominator, 0.0))
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

            if (DoubleEpsilonComparer.Instance.Equals(denominator, 0.0))
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

                if (DoubleEpsilonComparer.Instance.Equals(denominator, 0.0))
                    return 0.0;

                return c / denominator;
            };

            return tverskyDistance;
        }

        public static double Cosine(double[] v1, double[] v2)
        {
            double dot = 0.0, d1 = 0.0, d2 = 0.0;
            for (int i = 0; i < v1.Length; i++)
            {
                dot += v1[i] * v2[i];
                d1 += v1[i] * v1[i];
                d2 += v2[i] * v2[i];
            }
            return dot / (System.Math.Sqrt(d1) * System.Math.Sqrt(d2));
        }

        public static double CosineB(double[] v1, double[] v2, double[] w)
        {
            double[] assoc = Similarity.BinaryAssociationDouble(v1, v2, w);
            double a = assoc[0];
            double b = assoc[1];
            double c = assoc[2];

            double denominator = System.Math.Sqrt((a + c) * (b + c));
            return c / denominator;
        }

        public static double DiceB(double[] v1, double[] v2, double[] w)
        {
            double[] assoc = Similarity.BinaryAssociationDouble(v1, v2, w);
            double a = assoc[0];
            double b = assoc[1];
            double c = assoc[2];

            double denominator = (a + c) * (b + c);
            return (2.0 * c) / denominator;
        }

        public static double EuclideanB(double[] v1, double[] v2, double[] w)
        {
            double[] assoc = Similarity.BinaryAssociationDouble(v1, v2, w);
            double a = assoc[0];
            double b = assoc[1];
            double c = assoc[2];
            double d = assoc[3];

            double denominator = a + b + c + d;
            return System.Math.Sqrt((c + d) / denominator);
        }

        public static double ForbesB(double[] v1, double[] v2, double[] w)
        {
            double[] assoc = Similarity.BinaryAssociationDouble(v1, v2, w);
            double a = assoc[0];
            double b = assoc[1];
            double c = assoc[2];
            double d = assoc[3];

            double denominator = (a + c) * (b + c);
            return (c * (a + b + c + d)) / denominator;
        }

        public static double HammanB(double[] v1, double[] v2, double[] w)
        {
            double[] assoc = Similarity.BinaryAssociationDouble(v1, v2, w);
            double a = assoc[0];
            double b = assoc[1];
            double c = assoc[2];
            double d = assoc[3];

            double denominator = (a + b + c + d);
            return ((c + d) - (a + b)) / denominator;
        }

        public static double JaccardB(double[] v1, double[] v2, double[] w)
        {
            double[] assoc = Similarity.BinaryAssociationDouble(v1, v2, w);
            double a = assoc[0];
            double b = assoc[1];
            double c = assoc[2];

            double denominator = (a + b + c);
            return c / denominator;
        }

        public static double KulczynskiB(double[] v1, double[] v2, double[] w)
        {
            double[] assoc = Similarity.BinaryAssociationDouble(v1, v2, w);
            double a = assoc[0];
            double b = assoc[1];
            double c = assoc[2];

            return 0.5 * ((c / (a + c)) + (c / (b + c)));
        }

        public static double ManhattanB(double[] v1, double[] v2, double[] w)
        {
            double[] assoc = Similarity.BinaryAssociationDouble(v1, v2, w);
            double a = assoc[0];
            double b = assoc[1];
            double c = assoc[2];
            double d = assoc[3];

            return (a + b) / (a + b + c + d);
        }

        public static double MatchingB(double[] v1, double[] v2, double[] w)
        {
            double[] assoc = Similarity.BinaryAssociationDouble(v1, v2, w);
            double a = assoc[0];
            double b = assoc[1];
            double c = assoc[2];
            double d = assoc[3];

            return (c + d) / (a + b + c + d);
        }

        public static double PearsonB(double[] v1, double[] v2, double[] w)
        {
            double[] assoc = Similarity.BinaryAssociationDouble(v1, v2, w);
            double a = assoc[0];
            double b = assoc[1];
            double c = assoc[2];
            double d = assoc[3];

            return ((c * d) - (a * b)) / System.Math.Sqrt((a + c) * (b + c) * (a + d) * (b + d));
        }

        public static double RogersTanimotoB(double[] v1, double[] v2, double[] w)
        {
            double[] assoc = Similarity.BinaryAssociationDouble(v1, v2, w);
            double a = assoc[0];
            double b = assoc[1];
            double c = assoc[2];
            double d = assoc[3];

            return (2.0 * (a + b)) / (c + (2.0 * (a + b)) + d);
        }

        public static double RussellRaoB(double[] v1, double[] v2, double[] w)
        {
            double[] assoc = Similarity.BinaryAssociationDouble(v1, v2, w);
            double a = assoc[0];
            double b = assoc[1];
            double c = assoc[2];
            double d = assoc[3];

            return (a + b + d) / (a + b + c + d);  //denominator was v1.Length in the original formula
        }

        public static double SimpsonB(double[] v1, double[] v2, double[] w)
        {
            double[] assoc = Similarity.BinaryAssociationDouble(v1, v2, w);
            double a = assoc[0];
            double b = assoc[1];
            double c = assoc[2];

            return c / System.Math.Min((a + c), (b + c));
        }

        public static double TanimotoB(double[] v1, double[] v2, double[] w)
        {
            return Similarity.JaccardB(v1, v2, w);
        }

        public static double YuleB(double[] v1, double[] v2, double[] w)
        {
            double[] assoc = Similarity.BinaryAssociationDouble(v1, v2, w);
            double a = assoc[0];
            double b = assoc[1];
            double c = assoc[2];
            double d = assoc[3];

            return (2.0 * a * b) / ((c * d) + (a * b)); //Wolfram
            //return ((c * d) - (a * b)) / ((c * d) + (a * b)); //Chem
        }

        public static double SokalSneathB(double[] v1, double[] v2, double[] w)
        {
            double[] assoc = Similarity.BinaryAssociationDouble(v1, v2, w);
            double a = assoc[0];
            double b = assoc[1];
            double c = assoc[2];
            double d = assoc[3];

            return (2.0 * (a + b)) / (c + (2.0 * (a * b)));
        }

        public static double BrayCurtis(double[] v1, double[] v2)
        {
            double d1 = 0.0, d2 = 0.0;
            for (int i = 0; i < v1.Length; i++)
            {
                d1 += System.Math.Abs(v1[i] - v2[i]);
                d2 += System.Math.Abs(v1[i] + v2[i]);
            }
            return d1 / d2;
        }

        public static double Canberra(double[] v1, double[] v2)
        {
            double sum = 0;
            for (int i = 0; i < v1.Length; i++)
                if ((System.Math.Abs(v1[i]) + System.Math.Abs(v2[i])) > 0.0)
                    sum += System.Math.Abs(v1[i] - v2[i]) / (System.Math.Abs(v1[i]) + System.Math.Abs(v2[i]));
            return sum;
        }

        public static double KulsinskiB(double[] v1, double[] v2, double[] w)
        {
            double[] assoc = Similarity.BinaryAssociationDouble(v1, v2, w);
            double a = assoc[0];
            double b = assoc[1];
            double c = assoc[2];
            double d = assoc[3];

            return ((a + b - c) + v1.Length) / ((a + b) + v1.Length);
        }

        //mahalanobis
        //SokalMichener

        public static double Minkowski(double[] v1, double[] v2, double p)
        {
            double ex = 0.0;
            double min_d = Double.PositiveInfinity;
            double max_d = Double.NegativeInfinity;
            for (int i = 0; i < v1.Length; i++)
            {
                double d = System.Math.Abs(v1[i] - v2[i]);
                ex += System.Math.Pow(d, p);
                min_d = System.Math.Min(min_d, d);
                max_d = System.Math.Max(max_d, d);
            }

            return Double.IsNaN(ex) ? ex
                : !ex.IsNormal() && p.SignBit() ? min_d
                : !ex.IsNormal() && !p.SignBit() ? max_d
                : System.Math.Pow(ex, 1.0 / p);
        }

        public static int[] BinaryAssociation(int[] vec1, int[] vec2)
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
        /// <para>a is the count of bits on in object A but not in object B.</para>
        /// <para>b is the count of bits on in object B but not in object A. </para>
        /// <para>c is the count of the bits on in both object A and object B.</para>
        /// <para>d is the count of the bits off in both object A and object B.</para>
        /// </summary>
        /// <param name="vec1"></param>
        /// <param name="vec2"></param>
        /// <returns></returns>
        public static int[] BinaryAssociation(double[] vec1, double[] vec2)
        {
            int a = 0; //0
            int b = 0; //1
            int c = 0; //2
            int d = 0; //3

            for (int i = 0; i < vec1.Length; i++)
            {
                if (DoubleEpsilonComparer.Instance.Equals(vec1[i], 0.0)
                    && DoubleEpsilonComparer.Instance.Equals(vec2[i], 0.0))
                {
                    d++;
                }
                else if (DoubleEpsilonComparer.Instance.Equals(vec1[i], 0.0))
                {
                    b++;
                }
                else if (DoubleEpsilonComparer.Instance.Equals(vec2[i], 0.0))
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
        public static double[] BinaryAssociationDouble(double[] vec1, double[] vec2, double[] weights)
        {
            double a = 0; //0
            double b = 0; //1
            double c = 0; //2
            double d = 0; //3

            for (int i = 0; i < vec1.Length; i++)
            {
                if (DoubleEpsilonComparer.Instance.Equals(vec1[i], 0.0)
                    && DoubleEpsilonComparer.Instance.Equals(vec2[i], 0.0))
                {
                    d += weights[i];
                }
                else if (DoubleEpsilonComparer.Instance.Equals(vec1[i], 0.0))
                {
                    b += weights[i];
                }
                else if (DoubleEpsilonComparer.Instance.Equals(vec2[i], 0.0))
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