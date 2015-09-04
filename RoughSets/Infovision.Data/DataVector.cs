using System;
using System.Text;
using Infovision.Utils;

namespace Infovision.Data
{
    [Serializable]
    public class DataVector : ICloneable
    {
        #region Globals

        private Int64[] data;

        #endregion

        #region Constructors

        public DataVector(int size)
        {
            this.data = new Int64[size];
        }

        public DataVector(Int64[] initalData, bool deepCopy)
        {
            if (deepCopy)
            {
                this.data = new Int64[initalData.Length];
                Buffer.BlockCopy(initalData, 0, this.data, 0, initalData.Length * sizeof(Int64));
            }
            else
            {
                this.data = initalData;
            }
        }

        public DataVector(Int64[] initalData)
        {
            this.data = new Int64[initalData.Length];
            Buffer.BlockCopy(initalData, 0, this.data, 0, initalData.Length * sizeof(Int64));
        }

        #endregion

        #region Properties

        public Int64 this[int index]
        {
            get { return data[index]; }
        }

        #endregion

        #region Methods

        public Int64[] GetData()
        {
            return data;
        }

        #region System.Object Methods

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            DataVector p = obj as DataVector;
            if (p == null)
                return false;

            if (p.data.Length != data.Length)
                return false;

            for (int i = 0; i < data.Length; i++)
                if (this[i] != p[i])
                    return false;

            return true;
        }

        public override int GetHashCode()
        {
            return HashHelper.GetHashCode<Int64>(data);
        }

        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            foreach (Int64 element in data)
            {
                stringBuilder.Append(element).Append(' ');
            }

            return stringBuilder.ToString();
        }

        #endregion

        #region ICloneable Members
        /// <summary>
        /// Clones the DataVector, performing a deep copy.
        /// </summary>
        /// <returns>A new instance of a RoughPartitionMap, using a deep copy.</returns>
        public virtual object Clone()
        {
            return new DataVector(this.GetData());
        }
        #endregion
        #endregion
    }
}
