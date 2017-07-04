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
using System.Collections;
using System.Collections.Generic;

namespace NRough.MachineLearning.Experimenter.Parms
{
    [Serializable]
    public class ParameterValueCollection<T> : ParameterBase<T>
    {
        #region Members

        private T[] values;
        private int currentIndex;

        #endregion Members

        #region Constructors

        public ParameterValueCollection(string name, T value)
        {
            this.values = new T[1];
            this.values[0] = value;
            this.Name = name;
            this.ResetCurrent();
        }

        public ParameterValueCollection(string name, T[] values)
        {
            this.values = new T[values.Length];
            this.Name = name;
            values.CopyTo(this.values, 0);
            this.ResetCurrent();
        }

        public ParameterValueCollection(string name, IEnumerable<T> collection)
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

        public ParameterValueCollection(ParameterValueCollection<T> parameterValueList)
        {
            this.values = new T[parameterValueList.Count];
            this.Name = parameterValueList.Name;
            parameterValueList.values.CopyTo(this.values, 0);
            this.ResetCurrent();
        }

        #endregion Constructors

        #region Methods

        public static ParameterValueCollection<T> CreateFromElements(string name, params T[] elements)
        {
            return new ParameterValueCollection<T>(name, elements);
        }

        public T GetValue(int index)
        {
            return this.values[index];
        }

        private void ResetCurrent()
        {
            currentIndex = -1;
        }

        public override bool InRange(object value)
        {
            if (Array.Exists(this.values, x => x.Equals(value)))
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
            return new ParameterValueCollection<T>(this);
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