using System;
using System.Collections.Generic;
using System.Linq;
using Infovision.Utils;

namespace Infovision.Data
{
    [Serializable]
    public class DataRecordInternal
    {
        #region Members
           
        private Dictionary<int, long> fields;

        #endregion

        #region Properties

        public long ObjectId { get; set; }
        public int ObjectIdx { get; set; }
        public long this[int fieldId]
        {
            get { return fields[fieldId]; }
            set { fields[fieldId] = value; }
        }

        public int Length
        {
            get { return fields.Count; }
        }        

        #endregion

        #region Constructors                

        public DataRecordInternal(int[] fieldIds, long[] fieldValues)
        {            
            this.fields = new Dictionary<int, long>(fieldIds.Length);

            if (fieldIds.Length != fieldValues.Length)
                throw new InvalidOperationException("Field and value lists must have the same length");

            for (int i = 0; i < fieldIds.Length; i++)
                fields.Add(fieldIds[i], fieldValues[i]);
        }

        #endregion

        #region Methods

        public IEnumerable<int> GetFields()
        {
            return fields.Keys;
        }

        public IEnumerable<long> GetValues()
        {
            return fields.Values;
        }

        #region System.Object Methods

        public override int GetHashCode()
        {
            //TODO to be optimized
            return HashHelper.GetHashCode<long>(fields.Values.Concat(fields.Keys.Cast<long>()));
        }
        
        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            DataRecordInternal p = obj as DataRecordInternal;
            if (p == null)
                return false;

            if (p.Length != this.Length)
                return false;

            try
            {
                foreach (KeyValuePair<int, long> kvp in this.fields)
                {
                    if (p[kvp.Key] != kvp.Value)
                        return false;
                }
            }
            catch (KeyNotFoundException e)
            {
                Console.WriteLine(e.Message);
                return false;
            }

            return true;
        }

        #endregion

        #endregion
    }
}
