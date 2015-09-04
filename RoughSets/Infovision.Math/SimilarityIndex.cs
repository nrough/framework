using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infovision.Math
{
    public static class SimilarityIndex
    {
        private static readonly double tinyDouble = 0.0000000001;

        
        public static double Tversky2(double[] prototype, double[] variant, double alpha, double beta)
        {
            int[] assoc = SimilarityIndex.BinaryAssociation(prototype, variant);
            int a = assoc[0];
            int b = assoc[1];
            int c = assoc[2];
            int d = assoc[3];

            double denominator = (alpha * a + beta * b + c);

            if (DoubleEpsilonComparer.NearlyEqual(denominator, 0.0, tinyDouble))
                return 0.0;

            return c / denominator;
        }        

        public static double Tversky(double[] prototype, double[] variant, double alpha, double beta)
        {
            return SimilarityIndex.TverskyDelegate(alpha, beta).Invoke(prototype, variant);
        }

        public static Func<double[], double[], double> TverskyDelegate(double alpha, double beta)
        {                       
            Func<double[], double[], double> tverskyDistance = (p, v) =>
            {
                int[] assoc = SimilarityIndex.BinaryAssociation(p, v);
                int a = assoc[0];
                int b = assoc[1];
                int c = assoc[2];
                int d = assoc[3];

                double denominator = (alpha * a + beta * b + c);

                if (DoubleEpsilonComparer.NearlyEqual(denominator, 0.0, tinyDouble))
                    return 0.0;

                return c / denominator;
            };

            return tverskyDistance;
        }

        public static double TverskySymetric(double[] prototype, double[] variant, double alpha, double beta)
        {
            int[] assoc = SimilarityIndex.BinaryAssociation(prototype, variant);
            int a = assoc[0];
            int b = assoc[1];
            int c = assoc[2];
            int d = assoc[3];

            int x = System.Math.Min(a, b);
            int y = System.Math.Max(a, b);

            double denominator = beta * (alpha * x + (1.0 - alpha) * y) + c;
            
            if (DoubleEpsilonComparer.NearlyEqual(denominator, 0.0, tinyDouble))
                return 0.0;

            return c / denominator;
        }

        public static double ReductSimilarity(double[] vec1, double[] vec2, double alpha)
        {
            //TODO implement ReductSImilarity
            return 0.0;
        }

        //TODO Implement Other SimilarityIndexes
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
            int a = 0; //0  //number od 
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
        /// Calculated four binary association factors and returns them in the form of an array int[] {a, b, c, d} where:
        /// a is the count of bits on in object A but not in object B.
        /// b is the count of bits on in object B but not in object A.      
        /// c is the count of the bits on in both object A and object B.
        /// d is the count of the bits off in both object A and object B.
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

            double tineDouble = 0.0000000001;

            for (int i = 0; i < vec1.Length; i++)
            {
                if (DoubleEpsilonComparer.NearlyEqual(vec1[i], 0.0, tineDouble)
                    && DoubleEpsilonComparer.NearlyEqual(vec2[i], 0.0, tineDouble))
                {
                    d++;
                }
                else if (DoubleEpsilonComparer.NearlyEqual(vec1[i], 0.0, tineDouble))
                {
                    b++;
                }
                else if (DoubleEpsilonComparer.NearlyEqual(vec2[i], 0.0, tineDouble))
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
    }
}
