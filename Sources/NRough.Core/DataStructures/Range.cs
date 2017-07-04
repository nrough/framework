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
using NRough.Core.Helpers;
using System;
using System.Text;

namespace NRough.Core.DataStructures
{
    [Serializable]
    public class Range<T>
        where T : struct, IComparable, IFormattable, IComparable<T>, IEquatable<T>
    {
        public Range(T lowerBound, T upperBound)
        {
            this.LowerBound = lowerBound;
            this.UpperBound = upperBound;
        }

        #region Properties

        public T LowerBound
        {
            get;
            private set;
        }

        public T UpperBound
        {
            get;
            private set;
        }

        #endregion Properties        

        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append("[");
            stringBuilder.Append(this.LowerBound);
            stringBuilder.Append(", ");
            stringBuilder.Append(this.UpperBound);
            stringBuilder.Append("]");

            return stringBuilder.ToString();
        }

        public override int GetHashCode()
        {
            return HashHelper.GetHashCode(this.LowerBound, this.UpperBound);
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            Range<T> range = obj as Range<T>;
            if (range == null)
                return false;

            if (this.LowerBound.Equals(range.LowerBound) 
                && this.UpperBound.Equals(range.UpperBound))
                return true;

            return false;
        }
    }
}