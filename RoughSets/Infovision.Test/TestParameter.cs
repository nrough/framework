using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;

namespace Infovision.Test
{
    public interface ITestParameter 
        : IEnumerable, IEnumerator, ICloneable, ICollection
    {
        string Name { get; set; }
    }

    [Serializable]
    public abstract class TestParameter
        : ITestParameter
    {
        #region Global
        
        private readonly object syncRoot = new object();
        private string name;
        
        #endregion

        #region Properties

        public string Name
        {
            get { return this.name; }
            set { this.name = value; }
        }

        public virtual Object Current
        {
            get
            {
                throw new NotImplementedException("Property Current not implemented");
            }
        }

        public virtual int Count
        {
            get
            {
                throw new NotImplementedException("Property Count not implemented");
            }
        }

        /// <summary>
        /// Returns an object that can be used to synchronize access to the ICollection.
        /// </summary>
        public virtual Object SyncRoot
        {
            get { return syncRoot; }
        }

        /// <summary>
        /// Returns a value indicating whether access to the ICollection is synchronized (thread-safe).
        /// </summary>
        public Boolean IsSynchronized
        {
            get { return false; }
        }

        #endregion

        #region Methods

        #region ICollection Members

        /// <summary>
        /// Copies the elements of the ICollection to an Array, starting at a particular Array index.
        /// </summary>
        public virtual void CopyTo(Array array, int index)
        {
            throw new NotImplementedException("Method CopyTo(Array, Int32) not implemented");
        }

        #endregion

        #region ICloneable

        public virtual object Clone()
        {
            throw new NotImplementedException("Method Clone() not implemented");
        }

        #endregion

        #region IEnumerable Members

        public virtual IEnumerator GetEnumerator()
        {
            throw new NotImplementedException("Method GetEnumerator() not implemented");
        }
        #endregion

        #region IEnumerator Members

        public virtual void Reset()
        {
            throw new NotImplementedException("Method Reset() not implemented");
        }

        public virtual bool MoveNext()
        {
            throw new NotImplementedException("Method MoveNext() not implemented");
        }

        #endregion

        #endregion
    }

    [CLSCompliant(false)]
    [Serializable]
    public class ParameterNumericRange<T> 
        : TestParameter
        where T: IConvertible
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
            var type = typeof(T);

            if (type == typeof(string) || type == typeof(DateTime))
            {
                throw new ArgumentException(String.Format(CultureInfo.InvariantCulture, "The type {0} is not supported", type.FullName));
            }

            if (step.ToDouble(NumberFormatInfo.CurrentInfo).CompareTo(0) == 0)
            {
                throw new ArgumentException("Step argument can not be zero");
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
                if (lowerBound.ToDouble(NumberFormatInfo.CurrentInfo).CompareTo(upperBound.ToDouble(NumberFormatInfo.CurrentInfo)) <= 0)
                {
                    if (currentValue.ToDouble(NumberFormatInfo.CurrentInfo).CompareTo(lowerBound.ToDouble(NumberFormatInfo.CurrentInfo)) < 0)
                        throw new InvalidOperationException();
                    if (currentValue.ToDouble(NumberFormatInfo.CurrentInfo).CompareTo(upperBound.ToDouble(NumberFormatInfo.CurrentInfo)) > 0)
                        throw new InvalidOperationException();
                }
                else
                {
                    if (currentValue.ToDouble(NumberFormatInfo.CurrentInfo).CompareTo(upperBound.ToDouble(NumberFormatInfo.CurrentInfo)) < 0)
                        throw new InvalidOperationException();
                    if (currentValue.ToDouble(NumberFormatInfo.CurrentInfo).CompareTo(lowerBound.ToDouble(NumberFormatInfo.CurrentInfo)) > 0)
                        throw new InvalidOperationException();
                }

                return currentValue;
            }
        }

        public override int Count
        {
            get
            {
                return (int) Math.Abs(((this.upperBound.ToDouble(NumberFormatInfo.CurrentInfo) - this.lowerBound.ToDouble(NumberFormatInfo.CurrentInfo)) 
                                        / this.step.ToDouble(NumberFormatInfo.CurrentInfo)));
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

        /// <summary>
        /// Copies the elements of the ICollection to an Array, starting at a particular Array index.
        /// </summary>
        public override void CopyTo(Array array, int index)
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

    [CLSCompliant(false)]
    [Serializable]
    public class ParameterValue<T>
        : TestParameter
        where T : IConvertible
    {
        #region Globals

        private T value;
        private int currentIndex;
        
        #endregion

        #region Constructors

        public ParameterValue(String name, T value)
        {
            this.value = value;
            this.Name = name;
            this.ResetCurrent();
        }

        public ParameterValue(ParameterValue<T> parameterValue)
        {
            this.value = parameterValue.Value;
            this.Name = parameterValue.Name;
            this.ResetCurrent();
        }

        #endregion

        #region Properties

        public T Value
        {
            get { return this.value; }
        }

        public override object Current
        {
            get
            {
                if (currentIndex > 0)
                    throw new InvalidOperationException();
                if (currentIndex < 0)
                    throw new InvalidOperationException();
                return this.value;
            }
        }

        public override int Count
        {
            get { return 1; }
        }

        #endregion

        #region Methods

        private void ResetCurrent()
        {
            currentIndex = -1;
        }

        #region IEnumerable Members

        public override IEnumerator GetEnumerator()
        {
            return (IEnumerator) this;
        }
        #endregion

        #region IEnumerator Members

        public override void Reset()
        {
            this.ResetCurrent();
        }

        public override bool MoveNext()
        {
            currentIndex++;
            if (currentIndex > 0)
                return false;
            return true;
        }

        #endregion

        #region ICloneable Members

        public override object Clone()
        {
            return new ParameterValue<T>(this);
        }
        #endregion

        #region ICollection Members

        /// <summary>
        /// Copies the elements of the ICollection to an Array, starting at a particular Array index.
        /// </summary>
        public override void CopyTo(Array array, int index)
        {
            T[] valueArray = new T[1];
            valueArray[0] = this.value;
            valueArray.CopyTo(array, index);
        }

        #endregion

        #endregion
    }

    [CLSCompliant(false)] 
    [Serializable]
    public class ParameterValueList<T> 
        : TestParameter
        where T : IConvertible
    {
        #region Globals
        
        private T[] values;
        private int currentIndex;

        #endregion

        #region Constructors

        public ParameterValueList(string name, T value)
        {
            this.values = new T[1];
            this.values[0] = value;
            this.Name = name;
            this.ResetCurrent();
        }

        public ParameterValueList(string name, T[] values)
        {
            this.values = new T[values.Length];
            this.Name = name;
            values.CopyTo(this.values, 0);
            this.ResetCurrent();
        }

        public ParameterValueList(string name, IEnumerable<T> collection)
        {
            int count = 0;
            this.Name = name;
            foreach (T element in collection)
                count++;
            this.values = new T[count];
            count = 0;
            foreach (T element in collection)
            {
                this.values[count++] = element;
            }
            this.ResetCurrent();
        }

        public ParameterValueList(ParameterValueList<T> parameterValueList)
        {
            this.values = new T[parameterValueList.Count];
            this.Name = parameterValueList.Name;
            parameterValueList.values.CopyTo(this.values, 0); 
            this.ResetCurrent();
        }

        #endregion

        #region Properties

        public override int Count
        {
            get { return this.values.Length; }
        }

        public override object Current
        {
            get
            {
                if (currentIndex >= this.values.Length)
                    throw new InvalidOperationException();
                if (currentIndex < 0)
                    throw new InvalidOperationException();
                return this.values.GetValue(currentIndex);
            }
        }
        
        #endregion

        #region Methods

        public T GetValue(int index)
        {                
            return this.values[index];
        }

        private void ResetCurrent()
        {
            currentIndex = -1;
        }

        #region IEnumerable Members

        public override IEnumerator GetEnumerator()
        {
            return (IEnumerator) this;
        }
        #endregion

        #region IEnumerator Members

        public override void Reset()
        {
            this.ResetCurrent();
        }

        public override bool MoveNext()
        {
            currentIndex++;
            if (currentIndex >= this.values.Length)
                return false;
            return true;
        }

        #endregion

        #region ICloneable Members

        public override object Clone()
        {
            return new ParameterValueList<T>(this);
        }
        #endregion

        #region ICollection Members

        /// <summary>
        /// Copies the elements of the ICollection to an Array, starting at a particular Array index.
        /// </summary>
        public override void CopyTo(Array array, int index)
        {
            this.values.CopyTo(array, index);
        }

        #endregion

        #endregion
    }

    [Serializable]
    public class ParameterObject<T>
        : TestParameter
        where T : ICloneable
    {
        #region Globals
        
        private T value;
        private int currentIndex;
        
        #endregion

        #region Constructors

        public ParameterObject(String name, T value)
        {
            this.value = (T)value.Clone();
            this.Name = name;
            this.ResetCurrent();
        }

        public ParameterObject(ParameterObject<T> parameterObject)
        {
            this.value = (T) parameterObject.Value.Clone();
            this.Name = parameterObject.Name;
            this.ResetCurrent();
        }

        #endregion

        #region Properties

        public override Int32 Count
        {
            get { return 1; }
        }

        public T Value
        {
            get { return this.value; }
        }

        public override Object Current
        {
            get
            {
                if (currentIndex > 0)
                    throw new InvalidOperationException();
                if (currentIndex < 0)
                    throw new InvalidOperationException();
                return this.value;
            }
        }

        #endregion

        #region Methods

        private void ResetCurrent()
        {
            currentIndex = -1;
        }

        #region IEnumerable Members

        public override IEnumerator GetEnumerator()
        {
            return (IEnumerator) this;
        }
        #endregion

        #region IEnumerator Members

        public override void Reset()
        {
            this.ResetCurrent();
        }

        public override bool MoveNext()
        {
            currentIndex++;
            if (currentIndex > 0)
                return false;
            return true;
        }

        #endregion

        #region ICloneable Members

        public override object Clone()
        {
            return new ParameterObject<T>(this);
        }
        #endregion

        #region ICollection Members

        /// <summary>
        /// Copies the elements of the ICollection to an Array, starting at a particular Array index.
        /// </summary>
        public override void CopyTo(Array array, int index)
        {
            T[] valueArray = new T[1];
            valueArray[0] = (T) this.value.Clone();
            valueArray.CopyTo(array, index);
        }

        #endregion

        #endregion
    }

    [Serializable]
    public class ParameterObjectList<T>
        : TestParameter
        where T : ICloneable
    {
        #region Globals

        private T[] values;
        private int currentIndex;

        #endregion

        #region Constructors

        public ParameterObjectList(string name, T value)
        {
            this.values = new T[1];
            this.values[0] = (T) value.Clone();
            this.Name = name;
            this.ResetCurrent();
        }

        public ParameterObjectList(string name, T[] values)
        {
            this.values = new T[values.Length];
            for(Int32 i = 0; i<values.Length; i++)
            {
                this.values[i] = (T) values[i].Clone();
            }
            this.Name = name;
            this.ResetCurrent();
        }

        public ParameterObjectList(string name, IEnumerable<T> collection)
        {
            int count = 0;
            this.Name = name;
            foreach (T element in collection)
                count++;
            this.values = new T[count];
            count = 0;
            foreach (T element in collection)
            {
                this.values[count++] = (T) element.Clone();
            }
            this.ResetCurrent();
        }

        private ParameterObjectList(ParameterObjectList<T> parameterObjectList)
        {
            this.values = new T[parameterObjectList.Count];
            this.Name = parameterObjectList.Name;
            Int32 i = 0;
            foreach (T value in parameterObjectList)
            {
                this.values[i++] = (T) value.Clone();
            }
            this.ResetCurrent();
        }

        #endregion

        #region Properties

        public override int Count
        {
            get { return this.values.Length; }
        }

        public override object Current
        {
            get
            {
                if (currentIndex >= this.values.Length)
                    throw new InvalidOperationException();
                if (currentIndex < 0)
                    throw new InvalidOperationException();
                return this.values.GetValue(currentIndex);
            }
        }

        #endregion

        #region Methods

        public T GetValue(int index)
        {
            return this.values[index];
        }

        private void ResetCurrent()
        {
            currentIndex = -1;
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
            this.ResetCurrent();
        }

        public override bool MoveNext()
        {
            currentIndex++;
            if (currentIndex >= this.values.Length)
                return false;
            return true;
        }

        #endregion

        #region ICloneable Members

        public override object Clone()
        {
            return new ParameterObjectList<T>(this);
        }
        #endregion

        #region ICollection Members

        /// <summary>
        /// Copies the elements of the ICollection to an Array, starting at a particular Array index.
        /// </summary>
        public override void CopyTo(Array array, int index)
        {
            T[] valueArray = new T[this.values.Length];
            int i = 0;
            foreach (T val in this.values)
            {
                valueArray[i++] = (T)this.values.Clone();
            }

            valueArray.CopyTo(array, index);
        }

        #endregion

        #endregion
    }

    [Serializable]
    public class ParameterObjectReference<T>
        : TestParameter
    {
        #region Globals

        private T value;
        private int currentIndex;

        #endregion

        #region Constructors

        public ParameterObjectReference(String name, T value)
        {
            this.value = (T)value;
            this.Name = name;
            this.ResetCurrent();
        }

        private ParameterObjectReference(ParameterObjectReference<T> parameterObject)
        {
            this.value = (T)parameterObject.Value;
            this.Name = parameterObject.Name;
            this.ResetCurrent();
        }

        #endregion

        #region Properties

        public override Int32 Count
        {
            get { return 1; }
        }

        public T Value
        {
            get { return this.value; }
        }

        public override Object Current
        {
            get
            {
                if (currentIndex > 0)
                    throw new InvalidOperationException();
                if (currentIndex < 0)
                    throw new InvalidOperationException();
                return this.value;
            }
        }

        #endregion

        #region Methods

        private void ResetCurrent()
        {
            currentIndex = -1;
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
            this.ResetCurrent();
        }

        public override bool MoveNext()
        {
            currentIndex++;
            if (currentIndex > 0)
                return false;
            return true;
        }

        #endregion

        #region ICloneable Members

        public override object Clone()
        {
            return new ParameterObjectReference<T>(this);
        }
        #endregion

        #region ICollection Members

        /// <summary>
        /// Copies the elements of the ICollection to an Array, starting at a particular Array index.
        /// </summary>
        public override void CopyTo(Array array, int index)
        {
            T[] valueArray = new T[1];
            valueArray[0] = (T)this.value;
            valueArray.CopyTo(array, index);
        }

        #endregion

        #endregion
    }

    [Serializable]
    public class ParameterObjectReferenceList<T>
        : TestParameter
    {
        #region Globals

        private T[] values;
        private int currentIndex;

        #endregion

        #region Constructors

        public ParameterObjectReferenceList(string name, T value)
        {
            this.values = new T[1];
            this.values[0] = value;
            this.Name = name;
            this.ResetCurrent();
        }

        public ParameterObjectReferenceList(string name, T[] values)
        {
            this.values = new T[values.Length];
            values.CopyTo(this.values, 0);
            this.Name = name;
            this.ResetCurrent();
        }

        public ParameterObjectReferenceList(string name, IEnumerable<T> collection)
        {
            int count = 0;
            this.Name = name;
            foreach (T element in collection)
            {
                count++;
            }
            this.values = new T[count];
            count = 0;
            foreach (T element in collection)
            {
                this.values[count++] = element;
            }
            this.ResetCurrent();
        }

        public ParameterObjectReferenceList(ParameterObjectReferenceList<T> parameterObjectReferenceList)
        {
            this.values = new T[parameterObjectReferenceList.Count];
            this.Name = parameterObjectReferenceList.Name;
            int i = 0;
            foreach (T value in parameterObjectReferenceList)
            {
                this.values[i++] = (T) value;
            }
            this.ResetCurrent();
        }

        #endregion

        #region Properties

        public override int Count
        {
            get { return this.values.Length; }
        }

        public override object Current
        {
            get
            {
                if (currentIndex >= this.values.Length)
                    throw new InvalidOperationException();
                if (currentIndex < 0)
                    throw new InvalidOperationException();
                return this.values.GetValue(currentIndex);
            }
        }

        #endregion

        #region Methods

        public T GetValue(int index)
        {
            return this.values[index];
        }

        private void ResetCurrent()
        {
            this.currentIndex = -1;
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
            this.ResetCurrent();
        }

        public override bool MoveNext()
        {
            currentIndex++;
            if (currentIndex >= this.values.Length)
                return false;
            return true;
        }

        #endregion

        #region ICloneable Members

        public override object Clone()
        {
            return new ParameterObjectReferenceList<T>(this);
        }
        #endregion

        #region ICollection Members

        /// <summary>
        /// Copies the elements of the ICollection to an Array, starting at a particular Array index.
        /// </summary>
        public override void CopyTo(Array array, int index)
        {
            this.values.CopyTo(array, index);
        }

        #endregion

        #endregion
    }
}
