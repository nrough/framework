//
// NRough Framework
// http://www.nrough.net
//
// Copyright © Sebastian Widz, 2017
// authors at nrough.net

//This file is part of NRough.

//Foobar is free software; you can redistribute it and/or modify
//it under the terms of the GNU General Public License as published by
//the Free Software Foundation; either version 3 of the License, or
//(at your option) any later version.
//
//Foobar is distributed in the hope that it will be useful,
//but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
//GNU General Public License for more details.
//
//You should have received a copy of the GNU General Public License
//along with Foobar; if not, write to the Free Software
//Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA  02110-1301  USA
//
using NRough.Core.BaseTypeExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NRough.Core.Comparers
{
    public class ToleranceDoubleComparer : EqualityComparer<double>, IComparer<double>        
    {
        public long Tolerance { get; set; }
        public double MaxDifference { get; set; }
        private static long DEFAULT_TOLERANCE = 10;
        private static double DEFAULT_MAX_DIFF = 1E-9;
        private static readonly object syncRoot = new object();
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
