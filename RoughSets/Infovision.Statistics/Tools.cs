using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Infovision.Math;
using Infovision.Utils;

namespace Infovision.Statistics
{
    public static class Tools
    {
        static double LOGPI = 1.14472988584940017414;
        static double LOG2 = System.Math.Log(2.0);        
        static double MAX_INT_FOR_CACHE_PLUS_ONE = 10000;
        static double[] INT_N_LOG_N_CACHE = new double[(int)MAX_INT_FOR_CACHE_PLUS_ONE];

        static Tools()
        {
            for (int i = 1; i < MAX_INT_FOR_CACHE_PLUS_ONE; i++)
            {
                double d = (double)i;
                INT_N_LOG_N_CACHE[i] = d * System.Math.Log(d);
            }
        }


        /// <summary>
        /// Finds minimum key
        /// </summary>
        /// <param name="sample"></param>
        /// <returns></returns>
        public static double Min(double[] sample)
        {
            double min = Double.MaxValue;
            for (int i = 0; i < sample.Length; i++)
                if (min > sample[i])
                    min = sample[i];
            return min;
        }

        /// <summary>
        /// Finds minimum key
        /// </summary>
        /// <param name="sample"></param>
        /// <returns></returns>
        public static double Max(double[] sample)
        {
            double max = Double.MinValue;
            for (int i = 0; i < sample.Length; i++)
                if (max < sample[i])
                    max = sample[i];
            return max;
        }

        /// <summary>
        /// Calculated a mean
        /// </summary>
        /// <param name="sample"></param>
        /// <returns></returns>
        public static double Mean(double[] sample)
        {
            double sum = 0.0;
            for (int i = 0; i < sample.Length; i++)
                sum += sample[i];
            return sum / sample.Length;
        }

        /// <summary>
        /// Calculates a standard deviation for a sample of population
        /// </summary>
        /// <param name="sample"></param>
        /// <returns></returns>
        public static double StdDev(double[] sample)
        {
            if (sample.Length == 1)
                return 0.0;
            double mean = Tools.Mean(sample);
            double sum = 0.0;
            for (int i = 0; i < sample.Length; i++)
                sum += ((sample[i] - mean) * (sample[i] - mean));
            return System.Math.Sqrt(sum / (sample.Length - 1));
        }

        /// <summary>
        /// Calculates a standard deviation for a sample of population, given a sample mean
        /// </summary>
        /// <param name="sample"></param>
        /// <param name="mean"></param>
        /// <returns></returns>
        public static double StdDev(double[] sample, double mean)
        {            
            if (sample.Length == 1)
                return 0.0;
            double sum = 0.0;
            for (int i = 0; i < sample.Length; i++)
                sum += ((sample[i] - mean) * (sample[i] - mean));
            return System.Math.Sqrt(sum / (sample.Length - 1));
        }

        /// <summary>
        /// Calculates a standard deviation for a whole population
        /// </summary>
        /// <param name="population"></param>
        /// <returns></returns>
        public static double StdDevP(double[] population)
        {
            if (population.Length == 1)
                return 0.0;
            double mean = Tools.Mean(population);
            double sum = 0.0;
            for (int i = 0; i < population.Length; i++)
                sum += (population[i] - mean) * (population[i] - mean);
            return System.Math.Sqrt(sum / population.Length);
        }

        /// <summary>
        /// Calculates the average of the absolute deviations of data points from their mean
        /// </summary>
        /// <param name="sample"></param>
        /// <returns>Returns the average of the absolute deviations of data points from their mean.</returns>
        public static double AveDev(double[] sample)
        {
            double mean = Tools.Mean(sample);
            double sum = 0.0;
            for (int i = 0; i < sample.Length; i++)
                sum += System.Math.Abs(sample[i] - mean);
            return sum / sample.Length;
        }

        /// <summary>
        /// Calculates the average of the absolute deviations of data points from their mean
        /// </summary>
        /// <param name="sample"></param>
        /// <param name="mean"></param>
        /// <returns>Returns the average of the absolute deviations of data points from their mean.</returns>
        public static double AveDev(double[] sample, double mean)
        {            
            double sum = 0.0;
            for (int i = 0; i < sample.Length; i++)
                sum += System.Math.Abs(sample[i] - mean);
            return sum / sample.Length;
        }

        public static double Entropy(double[] sample)
        {
            double returnValue = 0, sum = 0;
            for (int i = 0; i < sample.Length; i++)
            {
                returnValue -= LnFunc(sample[i]);
                sum += sample[i];
            }
            return DoubleEpsilonComparer.Instance.Equals(sum, 0.0) ? 0.0 : (returnValue + LnFunc(sum)) / (sum * LOG2);
        }

        public static double LnFunc(double num)
        {
            if (num <= 0)
            {
                return 0;
            }
            else
            {
                // Use cache if we have a sufficiently small integer
                if (num < MAX_INT_FOR_CACHE_PLUS_ONE)
                {
                    int n = (int)num;
                    if ((double)n == num)
                    {
                        return INT_N_LOG_N_CACHE[n];
                    }
                }
                return num * System.Math.Log(num);
            }
        }

        public static double EntropyConditionedOnRows(double[][] matrix)
        {
            double returnValue = 0, sumForRow, total = 0;
            for (int i = 0; i < matrix.Length; i++)
            {
                sumForRow = 0;
                for (int j = 0; j < matrix[0].Length; j++)
                {
                    returnValue = returnValue + LnFunc(matrix[i][j]);
                    sumForRow += matrix[i][j];
                }
                returnValue = returnValue - LnFunc(sumForRow);
                total += sumForRow;
            }
            return DoubleEpsilonComparer.Instance.Equals(total, 0.0) ? 0.0 : -returnValue / (total * LOG2);            
        }

        public static double EntropyConditionedOnColumns(double[][] matrix)
        {
            double returnValue = 0, sumForColumn, total = 0;
            for (int j = 0; j < matrix[0].Length; j++)
            {
                sumForColumn = 0;
                for (int i = 0; i < matrix.Length; i++)
                {
                    returnValue = returnValue + LnFunc(matrix[i][j]);
                    sumForColumn += matrix[i][j];
                }
                returnValue = returnValue - LnFunc(sumForColumn);
                total += sumForColumn;
            }
            return DoubleEpsilonComparer.Instance.Equals(total, 0) ? 0.0 : -returnValue / (total * LOG2);            
        }
        
        public static double Log2Binomial(double a, double b)
        {
            if (DoubleEpsilonComparer.Instance.Compare(b, a) > 0)
            {
                throw new ArithmeticException("Can't compute binomial coefficient.");
            }
            return (LnFactorial(a) - LnFactorial(b) - LnFactorial(a - b)) / LOG2;
        }

        
        public static double Log2Multinomial(double a, double[] bs)
        {            
            double sum = 0;
            int i;

            for (i = 0; i < bs.Length; i++)
            {
                if (DoubleEpsilonComparer.Instance.Compare(bs[i], a) > 0)
                {
                    throw new ArithmeticException("Can't compute multinomial coefficient.");
                }
                else
                {
                    sum = sum + LnFactorial(bs[i]);
                }
            }
            return (LnFactorial(a) - sum) / LOG2;
        }

        public static double LnFactorial(double x)
        {
            return LnGamma(x + 1);
        }

        public static double LnGamma(double x)
        {
            double p, q, w, z;

            double[] A = { 8.11614167470508450300E-4, -5.95061904284301438324E-4, 7.93650340457716943945E-4, -2.77777777730099687205E-3, 8.33333333333331927722E-2 };
            double[] B = { -1.37825152569120859100E3, -3.88016315134637840924E4, -3.31612992738871184744E5, -1.16237097492762307383E6, -1.72173700820839662146E6, -8.53555664245765465627E5 };
            double[] C = { /* 1.00000000000000000000E0, */ -3.51815701436523470549E2, -1.70642106651881159223E4, -2.20528590553854454839E5, -1.13933444367982507207E6, -2.53252307177582951285E6, -2.01889141433532773231E6 };

            if (x < -34.0)
            {
                q = -x;
                w = LnGamma(q);
                p = System.Math.Floor(q);
                if (p == q)
                {
                    throw new ArithmeticException("LnGamma: Overflow");
                }

                z = q - p;
                if (z > 0.5)
                {
                    p += 1.0;
                    z = p - q;
                }

                z = q * System.Math.Sin(System.Math.PI * z);
                if (z == 0.0)
                {
                    throw new ArithmeticException("lnGamma: Overflow");
                }
                z = LOGPI - System.Math.Log(z) - w;
                return z;
            }

            if (x < 13.0)
            {
                z = 1.0;
                while (x >= 3.0)
                {
                    x -= 1.0;
                    z *= x;
                }

                while (x < 2.0)
                {
                    if (x == 0.0)
                    {
                        throw new ArithmeticException("LnGamma: Overflow");
                    }
                    z /= x;
                    x += 1.0;
                }

                if (z < 0.0)
                {
                    z = -z;
                }

                if (x == 2.0)
                {
                    return System.Math.Log(z);
                }

                x -= 2.0;
                p = x * PolEvl(x, B, 5) / Pol1Evl(x, C, 6);
                return (System.Math.Log(z) + p);
            }

            if (x > 2.556348e305)
            {
                throw new ArithmeticException("LnGamma: Overflow");
            }

            q = (x - 0.5) * System.Math.Log(x) - x + 0.91893853320467274178;

            if (x > 1.0e8)
            {
                return (q);
            }

            p = 1.0 / (x * x);
            if (x >= 1000.0)
            {
                q += ((7.9365079365079365079365e-4 * p - 2.7777777777777777777778e-3) * p + 0.0833333333333333333333) / x;
            }
            else
            {
                q += PolEvl(p, A, 4) / x;
            }

            return q;
        }

        public static double PolEvl(double x, double[] coef, int N)
        {
            double ans = coef[0];
            for (int i = 1; i <= N; i++)
                ans = ans * x + coef[i];
            return ans;
        }

        public static double Pol1Evl(double x, double[] coef, int N)
        {
            double ans = x + coef[0];            
            for (int i = 1; i < N; i++)
                ans = ans * x + coef[i];
            return ans;
        }

        public static double Sum(double[] samples)
        {
            double sum = 0.0;
            for (int i = 0; i < samples.Length; i++)
            {
                sum += samples[i];
            }
            return sum;
        }

        public static double Log2(double a)
        {
            return System.Math.Log(a) / LOG2;
        }

        public static void Normalize(double[] values, double sum)
        {
            for (int i = 0; i < values.Length; i++)
                values[i] /= sum;
        }

        public static void Normalize(decimal[] values, decimal sum)
        {
            for (int i = 0; i < values.Length; i++)
                values[i] /= sum;
        }

    }
}
