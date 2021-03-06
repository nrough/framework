﻿// 
// NRough Framework
// http://www.nrough.net
// 
// Copyright (c) Sebastian Widz, 2017
// authors at nrough.net
// 
// This file is part of NRough Framework.
// 
// NRough Framework is free software; you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation; either version 3 of the License, or
// (at your option) any later version.
// 
// NRough Framework is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with NRough Framework; if not, write to the Free Software
// Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA  02110-1301  USA
//

using System;
using System.Collections;
using System.Globalization;

namespace NRough.MachineLearning.Experimenter.Parms
{
    [Serializable]
    public class ParameterNumericRange<T> : ParameterBase<T>
        where T : IConvertible
    {
        #region Globals

        private T lowerBound;
        private T upperBound;
        private T step;

        private T currentValue;

        #endregion Globals

        #region Constructors

        public ParameterNumericRange(string name, T lowerBound, T upperBound, T step)
        {
            Type type = typeof(T);
            if (!(type == typeof(int)
                    || type == typeof(short)
                    || type == typeof(byte)
                    || type == typeof(long)
                    || type == typeof(double)
                    || type == typeof(float)
                    || type == typeof(uint)
                    || type == typeof(ushort)
                    || type == typeof(sbyte)
                    || type == typeof(ulong)
                    || type == typeof(char)
                    || type == typeof(decimal)))
            {
                throw new ArgumentException(String.Format(CultureInfo.InvariantCulture, "The type {0} is not supported", type.FullName));
            }

            if (step.ToDouble(NumberFormatInfo.CurrentInfo).CompareTo(0) == 0)
            {
                throw new ArgumentException("Step argument can not be zero", "step");
            }

            this.lowerBound = lowerBound;
            this.upperBound = upperBound;
            this.step = step;
            this.Name = name;

            this.ResetCurrent();
        }

        public ParameterNumericRange(ParameterNumericRange<T> parameterNumericRange)
        {
            this.Name = parameterNumericRange.Name;
            this.lowerBound = parameterNumericRange.LowerBound;
            this.upperBound = parameterNumericRange.UpperBound;
            this.step = parameterNumericRange.Step;

            this.ResetCurrent();
        }

        #endregion Constructors

        #region Properties

        public T LowerBound
        {
            get { return lowerBound; }
        }

        public T UpperBound
        {
            get { return upperBound; }
        }

        public T Step
        {
            get { return step; }
        }

        public override object Current
        {
            get
            {
                return currentValue;
            }
        }

        #endregion Properties

        #region Methods

        private void ResetCurrent()
        {
            currentValue = (T)Convert.ChangeType((this.lowerBound.ToDouble(NumberFormatInfo.CurrentInfo)
                                                  - this.step.ToDouble(NumberFormatInfo.CurrentInfo)),
                                                  typeof(T),
                                                  CultureInfo.InvariantCulture);
        }

        public override bool InRange(object value)
        {
            IConvertible val;
            if (value is IConvertible)
            {
                val = value as IConvertible;
            }
            else
            {
                return false;
            }

            if (val.ToDouble(NumberFormatInfo.CurrentInfo).CompareTo(this.UpperBound.ToDouble(NumberFormatInfo.CurrentInfo)) <= 0
                    && val.ToDouble(NumberFormatInfo.CurrentInfo).CompareTo(this.LowerBound.ToDouble(NumberFormatInfo.CurrentInfo)) >= 0)
            {
                return true;
            }

            return false;
        }

        #region IEnumerable Members

        public override IEnumerator GetEnumerator()
        {
            return (IEnumerator)this;
        }

        #endregion IEnumerable Members

        #region IEnumerator Members

        public override void Reset()
        {
            try
            {
                this.ResetCurrent();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("The operation failed.", ex);
            }
        }

        public override bool MoveNext()
        {
            try
            {
                currentValue = (T)Convert.ChangeType((this.currentValue.ToDouble(NumberFormatInfo.CurrentInfo)
                                                    + this.step.ToDouble(NumberFormatInfo.CurrentInfo)), typeof(T), CultureInfo.InvariantCulture);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("The operation failed.", ex);
            }

            if (this.upperBound.ToDouble(NumberFormatInfo.CurrentInfo).CompareTo(this.lowerBound.ToDouble(NumberFormatInfo.CurrentInfo)) >= 0)
            {
                if (this.currentValue.ToDouble(NumberFormatInfo.CurrentInfo).CompareTo(this.upperBound.ToDouble(NumberFormatInfo.CurrentInfo)) > 0)
                    return false;

                if (this.currentValue.ToDouble(NumberFormatInfo.CurrentInfo).CompareTo(this.lowerBound.ToDouble(NumberFormatInfo.CurrentInfo)) < 0)
                    return false;
            }
            else if (this.currentValue.ToDouble(NumberFormatInfo.CurrentInfo).CompareTo(this.upperBound.ToDouble(NumberFormatInfo.CurrentInfo)) < 0)
            {
                if (this.currentValue.ToDouble(NumberFormatInfo.CurrentInfo).CompareTo(this.lowerBound.ToDouble(NumberFormatInfo.CurrentInfo)) > 0)
                    return false;

                if (this.currentValue.ToDouble(NumberFormatInfo.CurrentInfo).CompareTo(this.upperBound.ToDouble(NumberFormatInfo.CurrentInfo)) < 0)
                    return false;
            }

            return true;
        }

        #endregion IEnumerator Members

        #region ICloneable Members

        public override object Clone()
        {
            return new ParameterNumericRange<T>(this);
        }

        #endregion ICloneable Members

        #region ICollection Members

        public virtual int Count
        {
            get
            {
                return (int)Math.Abs(((this.upperBound.ToDouble(NumberFormatInfo.CurrentInfo) - this.lowerBound.ToDouble(NumberFormatInfo.CurrentInfo))
                                        / this.step.ToDouble(NumberFormatInfo.CurrentInfo)));
            }
        }

        /// <summary>
        /// Copies the elements of the ICollection to an Array, starting at a particular Array index.
        /// </summary>
        public virtual void CopyTo(Array array, int index)
        {
            T[] valueArray = new T[this.Count];
            int i = -1;
            foreach (T value in this)
            {
                i++;
                valueArray[i] = value;
            }

            valueArray.CopyTo(array, index);
        }

        #endregion ICollection Members

        #endregion Methods
    }
}