using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infovision.Math
{
    public class DoubleEpsilonComparer : IEqualityComparer<double>
    {
        private readonly double epsilon;

        public DoubleEpsilonComparer(double epsilon)            
        {
            this.epsilon = epsilon;
        }

        public DoubleEpsilonComparer()
            : this (0.00000001)
        {             
        }        

        public bool Equals(double a, double b)
        {            
            return NearlyEqual(a, b, this.epsilon);
        }

        public int GetHashCode(double a)
        {
            return a.GetHashCode();
        }

        public static bool NearlyEqual(double a, double b, double epsilon)
        {
            double absA = System.Math.Abs(a);
            double absB = System.Math.Abs(b);
            double diff = System.Math.Abs(a - b);

            if (a == b)
            {
                // shortcut, handles infinities
                return true;
            }
            else if (a == 0 || b == 0 || diff < Double.MinValue)
            {
                // a or b is zero or both are extremely close to it
                // relative error is less meaningful here
                return diff < (epsilon * Double.MinValue);
            }
            else
            {
                // use relative error
                return diff / (absA + absB) < epsilon;
            }
        }
    }
}
