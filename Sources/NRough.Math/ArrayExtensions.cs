using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NRough.Math
{
    public static class ArrayExtensions
    {
        public static void NormalizeSum(this double[] array)
        {
            if (array == null)
                return;
            double sum = array.Sum();
            array.NormalizeSum(sum);
        }

        public static void NormalizeSum(this double[] array, double sum)
        {
            if (array == null) throw new ArgumentNullException("array");
            if (sum == 0.0) return;
            for (int i = 0; i < array.Length; i++)
                array[i] /= sum;
        }
    }
}
