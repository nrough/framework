// 
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

namespace NRough.MachineLearning.Experimenter.Parms
{
    [Serializable]
    public abstract class ParameterBase<T> : IParameter<T>
    {
        #region Global

        private readonly object syncRoot = new object();
        private string name;

        #endregion Global

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

        #endregion Properties

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

        #endregion ICollection Members

        */

        #region ICloneable

        public virtual object Clone()
        {
            throw new NotImplementedException("Method Clone() not implemented");
        }

        #endregion ICloneable

        #region IEnumerable Members

        public virtual IEnumerator GetEnumerator()
        {
            throw new NotImplementedException("Method GetEnumerator() not implemented");
        }

        #endregion IEnumerable Members

        #region IEnumerator Members

        public virtual void Reset()
        {
            throw new NotImplementedException("Method Reset() not implemented");
        }

        public virtual bool MoveNext()
        {
            throw new NotImplementedException("Method MoveNext() not implemented");
        }

        #endregion IEnumerator Members

        #endregion Methods
    }
}