using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infovision.Datamining.Experimenter.Parms
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

        #endregion

        #region Constructors

        public ParameterNumericRange(string name, T lowerBound, T upperBound, T step)
        {
            Type type = typeof(T);
            if( ! ( type == typeof(int)
                    || type == typeof(short)
                    || type == typeof(byte)
                    || type == typeof(long)
                    || type == typeof(decimal)
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

        #endregion

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
        
        #endregion

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
        #endregion

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

        #endregion

        #region ICloneable Members

        public override object Clone()
        {
            return new ParameterNumericRange<T>(this);
        }
        #endregion

        
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

        #endregion       

        #endregion
    }
}
