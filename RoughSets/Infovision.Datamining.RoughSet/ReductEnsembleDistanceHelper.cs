using System;

namespace Infovision.Datamining.Roughset
{
    public static class ReductEnsembleDistanceHelper
    {
        /// <summary>
        /// |X| + |Y| - alpha * |X && Y|
        /// </summary>
        /// <param name="vec1"></param>
        /// <param name="vec2"></param>
        /// <param name="alpha"></param>
        /// <returns></returns>
        public static double ReductSim(double[] vec1, double[] vec2, double alpha)
        {
            return ReductEnsembleDistanceHelper.ReductSimDelegate(alpha).Invoke(vec1, vec2);
        }

        public static Func<double[], double[], double> ReductSimDelegate(double alpha)
        {
            Func<double[], double[], double> tverskyDistance = (p, v) =>
            {
                int[] assoc = Infovision.Math.Similarity.BinaryAssociation(p, v);
                int a = assoc[0];
                int b = assoc[1];
                int c = assoc[2];

                return (a + c) + (b + c) - (alpha * c);
            };

            return tverskyDistance;
        }
    }
}