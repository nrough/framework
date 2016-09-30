using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infovision.Utils
{
    public class ToleranceDoubleComparer : EqualityComparer<double>, IComparer<double>        
    {
        public long Tolerance { get; set; }
        public double MaxDifference { get; set; }
        private static long DEFAULT_TOLERANCE = 10;
        private static double DEFAULT_MAX_DIFF = 1E-9;
        private static object syncRoot = new object();
        private static volatile ToleranceDoubleComparer instance;

        public ToleranceDoubleComparer()
            : this(DEFAULT_MAX_DIFF, DEFAULT_TOLERANCE) { }

        public ToleranceDoubleComparer(long tolerance)
            : this(DEFAULT_MAX_DIFF, tolerance) { }

        public ToleranceDoubleComparer(double maxDifference, long tolerance)
        {
            this.Tolerance = tolerance;
            this.MaxDifference = maxDifference;
        }

        public static ToleranceDoubleComparer Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                        {
                            instance = new ToleranceDoubleComparer();
                        }
                    }
                }

                return instance;
            }
        }

        public int Compare(double a, double b)
        {

            if (this.Equals(a, b))
                return 0;

            return a.CompareTo(b);
        }

        public override bool Equals(double a, double b)
        {
            return a.AlmostEquals(b, this.MaxDifference, this.Tolerance);
        }

        public override int GetHashCode(double a)
        {
            throw new NotSupportedException("GetHashCode(double a) is not supported method in ToleranceDoubleComparer class");
        }
    }
}
