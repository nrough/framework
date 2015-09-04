﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infovision.Datamining.Experimenter.Parms
{
    [Serializable]
    public abstract class ParameterBase<T> : IParameter<T>
    {
        #region Global

        private readonly object syncRoot = new object();
        private string name;

        #endregion

        #region Properties

        public virtual string Name
        {
            get { return this.name; }
            set { this.name = value; }
        }

        public virtual object Current
        {
            get
            {
                throw new NotImplementedException("Property Current not implemented");
            }
        }

        /*
        public virtual int Count
        {
            get
            {
                throw new NotImplementedException("Property Count not implemented");
            }
        }
        */

        /// <summary>
        /// Returns an object that can be used to synchronize access to the ICollection.
        /// </summary>
        public virtual object SyncRoot
        {
            get { return syncRoot; }
        }

        public virtual Type Type
        {
            get { return typeof(T); }
        }

        /// <summary>
        /// Returns a value indicating whether access to the ICollection is synchronized (thread-safe).
        /// </summary>
        public bool IsSynchronized
        {
            get { return false; }
        }

        #endregion

        #region Methods

        public virtual bool InRange(object value)
        {
             throw new NotImplementedException("Method InRange() not implemented");
        }
        
        /*
        #region ICollection Members

        /// <summary>
        /// Copies the elements of the ICollection to an Array, starting at a particular Array index.
        /// </summary>
        public virtual void CopyTo(Array array, int index)
        {
            throw new NotImplementedException("Method CopyTo(Array, Int32) not implemented");
        }
        #endregion
        */

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
}