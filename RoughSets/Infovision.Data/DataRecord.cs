using System;
using System.Collections.Generic;
using System.Linq;
using Infovision.Utils;

namespace Infovision.Data
{
    [Serializable]
    public class DataRecordInternal
    {
        #region Globals

        private long objectId;
        private Dictionary<int, Int64> fields;

        #endregion

        #region Constructors

        public DataRecordInternal()
        {
            this.Init();
        }

        public DataRecordInternal(int capacity)
        {
            this.Init(capacity);
        }

        public DataRecordInternal(int[] fieldIds, Int64[] fieldValues)
            : this(fieldIds.Length)
        {
            if (fieldIds.Length != fieldValues.Length)
                throw new System.ArgumentException("Field and value lists must have the same length");

            for (int i = 0; i < fieldIds.Length; i++)
            {
                fields.Add(fieldIds[i], fieldValues[i]);
            }
        }

        protected void Init(int capacity)
        {
            objectId = 0;
            fields = new Dictionary<int, Int64>(capacity);
        }

        protected void Init()
        {
            objectId = 0;
            fields = new Dictionary<int, Int64>();
        }
        #endregion

        #region Properties

        public long this[int fieldId]
        {
            get { return fields[fieldId]; }
            set { fields[fieldId] = value; }
        }

        public int FieldsCount
        {
            get { return fields.Count; }
        }

        public long ObjectId
        {
            get { return objectId; }
            set { objectId = value; }
        }

        #endregion

        #region Methods        

        private Dictionary<int, Int64> GetFieldDictionary()
        {
            return this.fields;
        }

        public IEnumerable<int> GetFields()
        {
            return fields.Keys;
        }

        public IEnumerable<Int64> GetValues()
        {
            return fields.Values;
        }

        #region System.Object Methods

        public override int GetHashCode()
        {
            return HashHelper.GetHashCode<Int64>(this.GetValues());
        }

        //TODO if we could ensure the increasing order of field ids those two foreach loops could be optimized!
        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            DataRecordInternal p = obj as DataRecordInternal;
            if (p == null)
                return false;

            if (p.FieldsCount != this.FieldsCount)
                return false;

            //could we access fields and values by index ?
            foreach (var localField in this.fields)
            {
                if (p[localField.Key] != localField.Value)
                    return false;
            }

            foreach (var externalField in p.GetFieldDictionary())
            {
                if (this[externalField.Key] != externalField.Value)
                    return false;
            }

            foreach (var localField in this.fields)
            {
                if (!p.GetFields().Contains(localField.Key))
                    return false;
            }

            foreach (var externalField in p.GetFieldDictionary())
            {
                if (!this.GetFields().Contains(externalField.Key))
                    return false;
            }

            return true;
        }

        #endregion
        #endregion
    }
}
