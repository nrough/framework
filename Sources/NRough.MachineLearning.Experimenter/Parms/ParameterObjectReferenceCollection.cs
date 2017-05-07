using System;
using System.Collections;
using System.Collections.Generic;

namespace NRough.MachineLearning.Experimenter.Parms
{
    [Serializable]
    public class ParameterObjectReferenceCollection<T> : ParameterBase<T>
    {
        #region Globals

        private T[] values;
        private int currentIndex;

        #endregion Globals

        #region Constructors

        public ParameterObjectReferenceCollection(string name, T value)
        {
            this.values = new T[1];
            this.values[0] = value;
            this.Name = name;
            this.ResetCurrent();
        }

        public ParameterObjectReferenceCollection(string name, T[] values)
        {
            this.values = new T[values.Length];
            values.CopyTo(this.values, 0);
            this.Name = name;
            this.ResetCurrent();
        }

        public ParameterObjectReferenceCollection(string name, IEnumerable<T> collection)
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

        public ParameterObjectReferenceCollection(ParameterObjectReferenceCollection<T> parameterObjectReferenceList)
        {
            this.values = new T[parameterObjectReferenceList.Count];
            this.Name = parameterObjectReferenceList.Name;
            int i = 0;
            foreach (T value in parameterObjectReferenceList)
            {
                this.values[i++] = (T)value;
            }
            this.ResetCurrent();
        }

        #endregion Constructors



        #region Methods

        public override bool InRange(object value)
        {
            if (Array.Exists(this.values, x => x.Equals(value)))
            {
                return true;
            }

            return false;
        }

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

        #endregion IEnumerable Members

        #region IEnumerator Members

        public override object Current
        {
            get
            {
                if (currentIndex >= this.values.Length || currentIndex < 0)
                    throw new InvalidOperationException();
                return this.values.GetValue(currentIndex);
            }
        }

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

        #endregion IEnumerator Members

        #region ICloneable Members

        public override object Clone()
        {
            return new ParameterObjectReferenceCollection<T>(this);
        }

        #endregion ICloneable Members

        #region ICollection Members

        public virtual int Count
        {
            get { return this.values.Length; }
        }

        /// <summary>
        /// Copies the elements of the ICollection to an Array, starting at a particular Array index.
        /// </summary>
        public virtual void CopyTo(Array array, int index)
        {
            this.values.CopyTo(array, index);
        }

        #endregion ICollection Members

        #endregion Methods
    }
}