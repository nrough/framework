using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infovision.Statistics
{
    public static class Tools
    {
        /// <summary>
        /// Finds minimum value
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
        /// Finds minimum value
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
    }
}
