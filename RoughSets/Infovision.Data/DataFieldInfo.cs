using System;
using System.Collections.Generic;
using System.Globalization;
using Infovision.Utils;

namespace Infovision.Data
{
    [Serializable]
    public class DataFieldInfo
    {
        private string name;
        private string alias;
        private int id;
        private Type fieldValueType;
        private Dictionary<long, object> indexDictionary;
        private Dictionary<object, long> valueDictionary;
        private long maxValueInternalId;
        private Histogram histogram;

        #region Constuctors

        public DataFieldInfo(int attributeId, Type fieldValueType)
        {
            this.fieldValueType = fieldValueType;
            this.maxValueInternalId = 0;

            this.valueDictionary = new Dictionary<object, long>();
            this.indexDictionary = new Dictionary<long, object>();

            this.Id = attributeId;
            this.Name = String.Format(CultureInfo.InvariantCulture, "a{0}", attributeId);
            this.histogram = new Histogram();
        }

        #endregion

        #region Properties

        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        public string NameAlias
        {
            get { return alias; }
            set { alias = value; }
        }

        public int Id
        {
            get { return id; }
            set { id = value; }
        }

        public Type FieldValueType
        {
            get { return fieldValueType; }
        }

        public Histogram Histogram
        {
            get { return histogram; }
        }

        public long MinValue
        {
            get { return histogram.MinElement; }
        }

        public long MaxValue
        {
            get { return histogram.MaxElement; }
        }

        /*
        public FieldTypes FieldType
        {
            get
            {
                return this.fieldType;
            }

            protected set
            {
                if (fieldType == FieldTypes.All)
                    throw new InvalidOperationException("Field Type cannot be set to FieldTypes.All");
                    
                this.fieldType = value;
            }
        }

        #region Field type flags properties 
       
        public bool IsIdentifier
        {
            get
            {
                return this.FieldType.HasFlag(FieldTypes.Identifier);
            }

            protected set
            {
                if (value)
                {
                    this.FieldType |= FieldTypes.Identifier;
                }
                else
                {
                    this.FieldType &= ~FieldTypes.Identifier;
                }
            }
        }

        public bool IsDecision
        {
            get 
            { 
                return this.FieldType.HasFlag(FieldTypes.Decision); 
            }

            protected set
            {
                if (value)
                {
                    this.FieldType |= FieldTypes.Decision;
                }
                else
                {
                    this.FieldType &= ~ FieldTypes.Decision;
                }
            }
        }

        public bool IsTechnical
        {
            get
            {
                return this.FieldType.HasFlag(FieldTypes.Technical);
            }

            protected set
            {
                if (value)
                {
                    this.FieldType |= FieldTypes.Technical;
                }
                else
                {
                    this.FieldType &= ~FieldTypes.Technical;
                }
            }
        }
        

        #endregion
        */

        #endregion

        #region Methods

        public void InitFromDataFieldInfo(DataFieldInfo dataFieldInfo)
        {
            foreach (KeyValuePair<long, object> kvp in dataFieldInfo.indexDictionary)
            {
                this.AddInternal(kvp.Key, kvp.Value);
            }

            this.fieldValueType = dataFieldInfo.FieldValueType;
            this.Name = dataFieldInfo.Name;
            this.NameAlias = dataFieldInfo.NameAlias;
            this.Id = dataFieldInfo.Id;
            this.maxValueInternalId = dataFieldInfo.maxValueInternalId;
        }

        public long External2Internal(object externalValue)
        {
            long internalValue;
            if (valueDictionary.TryGetValue(externalValue, out internalValue))
            {
                return internalValue;
            }
            return -1;
        }

        public object Internal2External(long internalValue)
        {
            object externalValue;
            if (indexDictionary.TryGetValue(internalValue, out externalValue))
            {
                return externalValue;
            }
            return null;
        }

        public long Add(object value)
        {
            if (!valueDictionary.ContainsKey(value))
            {
                maxValueInternalId++;
                valueDictionary.Add(value, maxValueInternalId);
                indexDictionary.Add(maxValueInternalId, value);

                return maxValueInternalId;
            }
            
            return this.External2Internal(value);
        }

        public void AddInternal(long internalValue, object externalValue)
        {
            //TODO in case we just copy values from existing FieldInfo we need to increase maxValueInternalId
            if (!valueDictionary.ContainsKey(externalValue))
            {
                valueDictionary.Add(externalValue, internalValue);
                indexDictionary.Add(internalValue, externalValue);
            }
        }

        public ICollection<object> Values()
        {
            return indexDictionary.Values;
        }
        
        public ICollection<long> InternalValues()
        {
            return valueDictionary.Values;
        }

        #region Histogram Methods

        public void IncreaseHistogramCount(long value)
        {
            histogram.IncreaseCount(value);
        }

        public int GetAttribiteValueCount(long value)
        {
            return histogram.GetBinValue(value);
        }

        #endregion

        #endregion        
    }
}
