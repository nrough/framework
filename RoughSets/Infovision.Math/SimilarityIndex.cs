using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infovision.Math
{
    public static class SimilarityIndex
    {
        public static double Tversky(double[] prototype, double[] variant, double alpha, double beta)
        {
            int[] assoc = SimilarityIndex.BinaryAssociation(prototype, variant);
            int a = assoc[0];
            int b = assoc[1];
            int c = assoc[2];
            int d = assoc[3];

            return c / (alpha * a + beta * b + c);
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

            return c / beta * (alpha * x + (1.0 - alpha) * y) + c;
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
