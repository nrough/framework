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
using System;
using System.Collections.Generic;

namespace NRough.Core.Comparers
{
    public class EpsilonComparer<T> : EqualityComparer<T>, IComparer<T>
        where T : struct, IComparable, IFormattable, IComparable<T>, IEquatable<T>
    {
        public T Epsilon
        {
            get;
            protected set;
        }

        public EpsilonComparer(T epsilon)
        {
            this.Epsilon = epsilon;
        }

        public override bool Equals(T a, T b)
        {
            return NearlyEqual(a, b, this.Epsilon);
        }

        public virtual int Compare(T a, T b)
        {
            return a.CompareTo(b);
        }
        
        public override int GetHashCode(T a)
        {
            return a.GetHashCode();
        }

        public virtual bool NearlyEqual(T a, T b, T epsilon)
        {
            return a.Equals(b);
        }
    }

    public class DoubleEpsilonComparer : EpsilonComparer<double>
    {
        private static volatile DoubleEpsilonComparer instance;
        private static readonly object syncRoot = new Object();
        private static double DEFAULT_EPSILON = 1E-9;

        public static DoubleEpsilonComparer Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                            instance = new DoubleEpsilonComparer(DoubleEpsilonComparer.DEFAULT_EPSILON);
                    }
                }
                return instance;
            }
        }

        public DoubleEpsilonComparer(double epsilon)
            : base(epsilon)
        {
        }

        public override int Compare(double a, double b)
        {
            double diff = a - b;

            if (System.Math.Abs(diff) < this.Epsilon)
                return 0;

            if (diff > this.Epsilon)
                return 1;
            else if (diff < this.Epsilon)
                return -1;
            return 0;
        }

        public override bool Equals(double a, double b)
        {
            return NearlyEqual(a, b, this.Epsilon);
        }

        public override bool NearlyEqual(double a, double b, double epsilon)
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

        public override int GetHashCode(double a)
        {
            throw new NotSupportedException("GetHashCode(double a) is not supported method in DoubleEpsilonComparer class");            
        }
    }

    public class DecimalEpsilonComparer : EpsilonComparer<decimal>
    {
        private static volatile DecimalEpsilonComparer instance;
        private static readonly object syncRoot = new Object();
        private static decimal DefaultEpsilon = 0.00000000000000001m;

        public static DecimalEpsilonComparer Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                            instance = new DecimalEpsilonComparer(DecimalEpsilonComparer.DefaultEpsilon);
                    }
                }
                return instance;
            }
        }

        public DecimalEpsilonComparer(decimal epsilon)
            : base(epsilon)
        {
        }

        public override int Compare(decimal a, decimal b)
        {
            decimal dif = a - b;
            if (dif > this.Epsilon)
                return 1;
            else if (dif < this.Epsilon)
                return -1;
            return 0;
        }

        public override bool Equals(decimal a, decimal b)
        {
            return NearlyEqual(a, b, this.Epsilon);
        }

        public override bool NearlyEqual(decimal a, decimal b, decimal epsilon)
        {
            decimal absA = System.Math.Abs(a);
            decimal absB = System.Math.Abs(b);
            decimal diff = System.Math.Abs(a - b);

            if (a == b)
            {
                // shortcut, handles infinities
                return true;
            }
            else if (a == 0 || b == 0 || diff < Decimal.MinValue)
            {
                // a or b is zero or both are extremely close to it
                // relative error is less meaningful here
                return diff < (epsilon * Decimal.MinValue);
            }
            else
            {
                // use relative error
                return diff / (absA + absB) < epsilon;
            }
        }

        public override int GetHashCode(decimal a)
        {
            throw new NotSupportedException("GetHashCode(decimal a) is not supported method in DecimalEpsilonComparer class");
        }
    }
}