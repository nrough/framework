using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infovision.Datamining.Experimenter.Parms
{   
    [Serializable]
    public class ParameterValueCollection<T> : ParameterBase<T>
        where T : IConvertible
    {
        #region Globals

        private T[] values;
        private int currentIndex;

        #endregion

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

        #endregion

        #region Properties                        

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
        #endregion

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

        #endregion

        #region ICloneable Members

        public override object Clone()
        {
            return new ParameterValueCollection<T>(this);
        }
        #endregion

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

        #endregion

        #endregion
    }
}
