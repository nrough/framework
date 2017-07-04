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
using System.Text;
using NRough.Core;
using NRough.Core.Helpers;
using NRough.Core.CollectionExtensions;

namespace NRough.Data
{
    [Serializable]
    public class AttributeValueVector : ICloneable
    {
        #region Members

        private int[] attributes;
        private long[] values;

        #endregion Members

        #region Constructors

        public AttributeValueVector(int size)
        {
            this.values = new long[size];
            this.attributes = new int[size];
        }

        public AttributeValueVector(int[] attributes, long[] values, bool deepCopy)
        {
            if (deepCopy)
            {
                this.values = new long[values.Length];
                Buffer.BlockCopy(values, 0, this.values, 0, values.Length * sizeof(long));

                this.attributes = new int[attributes.Length];
                Buffer.BlockCopy(attributes, 0, this.attributes, 0, attributes.Length * sizeof(int));
            }
            else
            {
                this.values = values;
                this.attributes = attributes;
            }
        }

        public AttributeValueVector(int[] attributes, long[] values)
        {
            this.values = new long[values.Length];
            Buffer.BlockCopy(values, 0, this.values, 0, values.Length * sizeof(long));

            this.attributes = new int[attributes.Length];
            Buffer.BlockCopy(attributes, 0, this.attributes, 0, attributes.Length * sizeof(int));
        }

        #endregion Constructors

        #region Properties

        public long[] Values
        {
            get { return values; }
        }

        public int[] Attributes
        {
            get { return attributes; }
        }

        public long this[int index]
        {
            get { return this.values[index]; }
            set { this.values[index] = value; }
        }

        public int Length
        {
            get
            {
                return this.attributes.Length;
            }
        }

        #endregion Properties

        #region Methods

        public AttributeValueVector RemoveAttribute(int attributeId)
        {
            int[] newAttributes = new int[attributes.Length - 1];
            long[] newValues = new long[values.Length - 1];

            int k = 0;
            for (int i = 0; i < attributes.Length; i++)
            {
                if (attributes[i] != attributeId)
                {
                    newAttributes[k] = attributes[i];
                    newValues[k] = values[i];
                    k++;
                }
            }

            return new AttributeValueVector(newAttributes, newValues, false);
        }

        public AttributeValueVector RemoveAt(int index)
        {
            return new AttributeValueVector(
                attributes.RemoveAt<int>(index),
                values.RemoveAt<long>(index), false);
        }

        public void Set(int index, int attribute, long value)
        {
            this.attributes[index] = attribute;
            this.values[index] = value;
        }

        #region System.Object Methods

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            AttributeValueVector p = obj as AttributeValueVector;
            if (p == null)
                return false;

            if (p.attributes.Length != attributes.Length)
                return false;

            for (int i = 0; i < attributes.Length; i++)
            {
                if (this.values[i] != p.values[i] || this.attributes[i] != p.attributes[i])
                    return false;
            }

            return true;
        }

        public override int GetHashCode()
        {
            return HashHelper.GetHashCode<long>(values);
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < attributes.Length; i++)
            {
                sb.AppendFormat("{0}={1}", attributes[i], values[i]);
                if (i < attributes.Length - 1)
                    sb.Append(" & ");
            }

            return sb.ToString();
        }

        public string ToString2(DataStoreInfo dataStoreInfo)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < attributes.Length; i++)
            {
                sb.Append(String.Format("{0}={1}",
                        dataStoreInfo.GetFieldInfo(attributes[i]).Alias,
                        dataStoreInfo.GetFieldInfo(attributes[i]).Internal2External(values[i])));
                if (i != attributes.Length - 1)
                    sb.Append(" & ");
            }
            return sb.ToString();
        }

        #endregion System.Object Methods

        #region ICloneable Members

        /// <summary>
        /// Clones the DataVector, performing a deep copy.
        /// </summary>
        /// <returns>A new instance of a RoughPartitionMap, using a deep copy.</returns>
        public virtual object Clone()
        {
            return new AttributeValueVector(attributes, values);
        }

        #endregion ICloneable Members

        #endregion Methods
    }
}