using System;
using Infovision.Core;
using System.Collections.Generic;
using System.Linq;
using MathNet.Numerics;
using MathNet.Numerics.Distributions;

namespace Infovision.Math
{
    public static class Tools
    {                                    
        /// <summary>
        /// Calculates a standard deviation for a sample of population
        /// </summary>
        /// <param name="sample"></param>
        /// <returns></returns>
        public static double StdDev(double[] sample)
        {
            if (sample.Length == 1)
                return 0.0;
            double mean = sample.Average();
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
            double mean = population.Average();
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
            double mean = sample.Average();
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
            double returnValue = 0, sum = sample.Sum();
            for (int i = 0; i < sample.Length; i++)
                if(sample[i] > 0)
                    returnValue -= (sample[i] / sum) * System.Math.Log(sample[i] / sum, 2);
            return returnValue;
        }

        public static double Entropy(double[] left, double[] right)
        {
            double entropyLeft = 0, entropyRight = 0, sumLeft = left.Sum(), sumRight = right.Sum();

            for (int i = 0; i < left.Length; i++)
                if(left[i] > 0)
                    entropyLeft -= (left[i] / sumLeft) * System.Math.Log(left[i] / sumLeft, 2);

            for (int i = 0; i < right.Length; i++)
                if (right[i] > 0)
                    entropyRight -= (right[i] / sumRight) * System.Math.Log(right[i] / sumRight, 2);

            return ((sumLeft / (sumLeft + sumRight)) * entropyLeft) + ((sumRight / (sumLeft + sumRight)) * entropyRight);
        }
        
        public static double Log2Binomial(double a, double b)
        {
            return SpecialFunctions.BinomialLn((int)a, (int)b) / System.Math.Log(2);
        }        

        public static double Log2Multinomial(double a, double[] bs)
        {
            return System.Math.Log(SpecialFunctions.Multinomial((int)a, bs.Select(d => (int)d).ToArray()), 2);
        }

        public static void Normalize(double[] values, double sum)
        {
            if (values == null)
                return;

            if (sum == 0.0)
                return;

            for (int i = 0; i < values.Length; i++)
                values[i] /= sum;
        }

        public static double MarginOfErrorUpper(double N, double e, double CF)
        {
            if (CF > 0.5)
                throw new ArgumentException("CF", "CF > 0.5");
            double z = Normal.InvCDF(0.0, 1.0, 1.0 - CF);
            return z * System.Math.Sqrt((e * (1 - e)) / N);
        }
    }
}