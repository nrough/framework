using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infovision.Math
{
    public static class Distance
    {
        public static double Euclidean(double[] a, double[] b)
        {
            return System.Math.Sqrt(Distance.SquaredEuclidean(a, b));
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

        public static double Tversky(double[] prototype, double[] variant, double alpha, double beta)
        {                                 
            return 0;
        }

        public static double TverskySymetric(double[] prototype, double[] variant, double alpha, double beta)
        {                    
            return 0;
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
                sum += System.Math.Pow((a[i] - b[i]), 2);
            return sum;
        }        
    }
}
