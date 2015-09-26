using System;
using System.Text;
using Infovision.Utils;

namespace Infovision.Data
{
    [Serializable]
    public class AttributeValueVector : ICloneable
    {
        #region Members

        private int[] attributes;
        private long[] values;
        
        #endregion

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

        #endregion

        #region Properties

        public long[] Values
        {
            get { return values; }
        }

        public int[] Attributes
        {
            get { return attributes; }
        }

        private long this[int index]
        {
            get { return this.values[index]; }
            set { this.values[index] = value; }
        }

        #endregion

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
            return HashHelper.GetHashCode<Int64>(values);
        }

        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            foreach (long element in values)
            {
                stringBuilder.Append(element).Append(' ');
            }

            return stringBuilder.ToString();
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

        #endregion

        #region ICloneable Members
        /// <summary>
        /// Clones the DataVector, performing a deep copy.
        /// </summary>
        /// <returns>A new instance of a RoughPartitionMap, using a deep copy.</returns>
        public virtual object Clone()
        {
            return new AttributeValueVector(attributes, values);
        }
        #endregion
        #endregion
    }
}
