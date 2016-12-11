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
                returnValue -= (sample[i] / sum) * System.Math.Log(sample[i] / sum, 2);
            return returnValue;
        }

        public static double EntropyConditionedOnRows(double[][] matrix)
        {
            double returnValue = 0, total = 0.0;
            double[][] localEntropy = new double[2][];
            for (int i = 0; i < 2; i++) localEntropy[i] = new double[matrix.Length];

            for (int i = 0; i < matrix.Length; i++)
            {
                localEntropy[0][i] = matrix[i].Sum();
                if (localEntropy[0][i] != 0)
                {
                    for (int j = 0; j < matrix[i].Length; j++)
                        localEntropy[1][i] -= (matrix[i][j] / localEntropy[0][i]) 
                            * System.Math.Log(matrix[i][j] / localEntropy[0][i], 2);
                    total += localEntropy[0][i];
                }
            }
            for(int i = 0; i < matrix.Length; i++)
                returnValue += (localEntropy[0][i] / total) * localEntropy[1][i];
            return returnValue;
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