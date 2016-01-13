using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Infovision.Utils;

namespace Infovision.Statistics
{
    public class Distribution
    {
        /// <summary>
        /// Returns random normal(0,1)
        /// </summary>
        /// <returns></returns>
        public static double BoxMullerTransform()
        {
            //these are uniform(0,1) random doubles
            double u1 = RandomSingleton.Random.NextDouble(); 
            double u2 = RandomSingleton.Random.NextDouble();
            double randStdNormal = System.Math.Sqrt(-2.0 * System.Math.Log(u1)) *
                         System.Math.Sin(2.0 * System.Math.PI * u2);
            return randStdNormal; 
        }

        /// <summary>
        /// Returns random normal(mean,stdDev^2)
        /// </summary>
        /// <param name="mean"></param>
        /// <param name="stdDev"></param>
        /// <returns></returns>
        public static double BoxMullerTransform(double mean, double stdDev)
        {
            return mean + stdDev * Distribution.BoxMullerTransform();
        }
    }
}
