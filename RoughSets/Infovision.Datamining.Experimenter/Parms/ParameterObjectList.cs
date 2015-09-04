using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infovision.Datamining.Experimenter.Parms
{
    //TODO Change "List" to "Collection" in class name

    [Serializable]
    public class ParameterObjectList<T> : ParameterBase<T>
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
            this.values[0] = (T)value.Clone();
            this.Name = name;
            this.ResetCurrent();
        }

        public ParameterObjectList(string name, T[] values)
        {
            this.values = new T[values.Length];
            for (int i = 0; i < values.Length; i++)
            {
                this.values[i] = (T)values[i].Clone();
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
                this.values[count++] = (T)element.Clone();
            }
            this.ResetCurrent();
        }

        private ParameterObjectList(ParameterObjectList<T> parameterObjectList)
        {
            this.values = new T[parameterObjectList.Count];
            this.Name = parameterObjectList.Name;
            int i = 0;
            foreach (T value in parameterObjectList)
            {
                this.values[i++] = (T)value.Clone();
            }
            this.ResetCurrent();
        }

        #endregion

        #region Properties

        
        public virtual int Count
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
        public virtual void CopyTo(Array array, int index)
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
}
