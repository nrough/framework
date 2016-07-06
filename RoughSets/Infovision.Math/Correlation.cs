using System;
using System.Collections.Generic;
using System.Linq;

namespace Infovision.Math
{
    public static class Correlation
    {
        //http://rysgsd.blogspot.com/2012/05/statistical-linq.html
        public static double SpearmansCoeff(this IEnumerable<int> current, IEnumerable<int> other)
        {
            if (current.Count() != other.Count())
                throw new ArgumentException("Both collections of data must contain an equal number of elements");

            double[] ranksX = GetRanking(current);
            double[] ranksY = GetRanking(other);

            var diffPair = ranksX.Zip(ranksY, (x, y) => new { x, y });
            double sigmaDiff = diffPair.Sum(s => System.Math.Pow(s.x - s.y, 2));
            int n = current.Count();

            // Spearman's Coefficient of Correlation
            // ρ = 1 - ((6 * sum of rank differences^2) / (n(n^2 - 1))
            double rho = 1.0 - ((6.0 * sigmaDiff) / (System.Math.Pow(n, 3) - n));

            return rho;
        }

        private static double[] GetRanking(IEnumerable<int> values)
        {
            var groupedValues = values.OrderByDescending(n => n)
                                      .Select((val, i) => new { Value = val, IndexedRank = i + 1 })
                                      .GroupBy(i => i.Value);

            double[] rankings = (from n in values
                                 join grp in groupedValues on n equals grp.Key
                                 select grp.Average(g => g.IndexedRank)).ToArray();

            return rankings;
        }

        //http://stackoverflow.com/questions/17447817/correlation-of-two-arrays-in-c-sharp
        public static double CorrelationCoef(double[] a, double[] b)
        {
            if (a.Length != b.Length)
                throw new ArgumentException("values must be the same length");

            var avg1 = a.Average();
            var avg2 = b.Average();

            var sum1 = a.Zip(b, (x1, y1) => (x1 - avg1) * (y1 - avg2)).Sum();

            var sumSqr1 = a.Sum(x => System.Math.Pow((x - avg1), 2.0));
            var sumSqr2 = b.Sum(y => System.Math.Pow((y - avg2), 2.0));

            var result = sum1 / System.Math.Sqrt(sumSqr1 * sumSqr2);

            return result;
        }

        //http://en.wikipedia.org/wiki/Pearson_product-moment_correlation_coefficient
        //http://stackoverflow.com/questions/17447817/correlation-of-two-arrays-in-c-sharp
        public static double Pearson(double[] Xs, double[] Ys)
        {
            double sumX = 0;
            double sumX2 = 0;
            double sumY = 0;
            double sumY2 = 0;
            double sumXY = 0;

            int n = Xs.Length < Ys.Length ? Xs.Length : Ys.Length;

            for (int i = 0; i < n; ++i)
            {
                double x = Xs[i];
                double y = Ys[i];

                sumX += x;
                sumX2 += x * x;
                sumY += y;
                sumY2 += y * y;
                sumXY += x * y;
            }

            double stdX = System.Math.Sqrt(sumX2 / n - sumX * sumX / n / n);
            double stdY = System.Math.Sqrt(sumY2 / n - sumY * sumY / n / n);
            double covariance = (sumXY / n - sumX * sumY / n / n);

            return covariance / stdX / stdY;
        }
    }
}