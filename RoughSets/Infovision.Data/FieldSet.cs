using System;
using System.Collections;
using Infovision.Utils;

namespace Infovision.Data
{
    [Serializable]
    public class FieldSet : PascalSet
    {
        #region Contructors

        public FieldSet(DataStoreInfo dataStoreInfo)
            : base(dataStoreInfo.MinFieldId, dataStoreInfo.MaxFieldId)
        {
        }

        public FieldSet(DataStoreInfo dataStoreInfo, int[] initialData)
            : base(dataStoreInfo.MinFieldId, dataStoreInfo.MaxFieldId, initialData)
        {
        }

        public FieldSet(DataStoreInfo dataStoreInfo, BitArray data)
            : base(dataStoreInfo.MinFieldId, dataStoreInfo.MaxFieldId, data)
        {
        }

        public FieldSet(FieldSet attributeSet)
            : base(attributeSet.LowerBound, attributeSet.UpperBound, attributeSet.Data)
        {
        }

        public FieldSet(int lowerBound, int upperBound, int[] data)
            : base(lowerBound, upperBound, data)
        {
        }

        #endregion

        #region Properties

        public string CacheKey
        {
            get { return this.ToString(); }
        }

        #endregion

        #region Methods

        public static FieldSet ConstructFromArray(int[] fields)
        {
            int min = Int32.MaxValue;
            int max = Int32.MinValue;

            foreach (int i in fields)
            {
                if (i < min)
                    min = i;

                if (i > max)
                    max = i;
            }

            return new FieldSet(min, max, fields);
        }

        public static FieldSet ConstructEmptyAttributeSet(DataStoreInfo dataStoreInfo)
        {
            return new FieldSet(dataStoreInfo);
        }
        
        #region System.Object Methods

        public override string ToString()
        {
            return base.ToString();
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            FieldSet attr = obj as FieldSet;
            if (attr == null)
                return false;

            return base.Equals((PascalSet) obj);
        }
        #endregion

        #region ICloneable Members
        /// <summary>
        /// Clones the FieldSet, performing a deep copy.
        /// </summary>
        /// <returns>A new instance of a FieldSet, using a deep copy.</returns>
        public override Object Clone()
        {
            return new FieldSet(this);
        }
        
        #endregion

        #endregion
    }
}
