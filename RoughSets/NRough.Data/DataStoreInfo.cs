using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NRough.Core;

namespace NRough.Data
{
    /// <summary>
    /// Provides meta information about dataset
    /// </summary>
    [Serializable]
    public class DataStoreInfo
    {
        #region Members

        private int minFieldId = Int32.MaxValue;
        private int maxFieldId = Int32.MinValue;        
        private Dictionary<int, AttributeInfo> fields;
        private Dictionary<int, FieldGroup> fieldTypes;
        private Dictionary<FieldGroup, int> fieldTypeCount;
        private Dictionary<int, int> fieldId2Index;
        private int decisionFieldId;
        private int decisionFieldIdx;
        private AttributeInfo decisionFieldInfo;        
        private readonly object syncRoot = new object();

        #endregion Members

        #region Properties

        public int NumberOfRecords { get; set; }
        public int NumberOfFields { get; set; }

        public int NumberOfDecisionValues
        {
            get { return this.decisionFieldInfo.NumberOfValues; }
            //get { return this.GetFieldInfo(this.DecisionFieldId).NumberOfValues; }
        }

        public int MinFieldId
        {
            get { return this.minFieldId; }
        }

        public int MaxFieldId
        {
            get { return this.maxFieldId; }
        }

        public int DecisionFieldId
        {
            get { return decisionFieldId; }
            internal set
            {
                if (this.decisionFieldId != value)
                {
                    if (this.decisionFieldId > 0)                                             
                        fieldTypes[this.decisionFieldId] = FieldGroup.Standard;

                    this.decisionFieldId = value;

                    if (this.decisionFieldId > 0)
                    {
                        this.fieldTypes[this.decisionFieldId] = FieldGroup.Output;
                        this.decisionFieldInfo = fields[this.decisionFieldId];
                        this.decisionFieldIdx = this.GetDecisionFieldIndex();
                    }
                }
            }
        }

        public int DecisionFieldIndex
        {
            get { return this.decisionFieldIdx; }
        }

        public AttributeInfo DecisionInfo
        {
            get
            {
                return this.decisionFieldInfo;
                //return this.GetFieldInfo(this.DecisionFieldId);
            }
        }

        public IEnumerable<AttributeInfo> Attributes
        {
            get { return this.fields.Values; }
        }

        public bool HasMissingData { get; set; }
        public string MissingValue { get; set; }

        #endregion Properties

        #region Constructor

        public DataStoreInfo(int numberOfFields = 0)
        {
            if (numberOfFields != 0)
            {
                this.fields = new Dictionary<int, AttributeInfo>(numberOfFields);
                this.fieldTypes = new Dictionary<int, FieldGroup>(numberOfFields);
                this.fieldId2Index = new Dictionary<int, int>(numberOfFields);
                this.NumberOfFields = numberOfFields;
            }
            else
            {
                this.fields = new Dictionary<int, AttributeInfo>();
                this.fieldTypes = new Dictionary<int, FieldGroup>();
                this.fieldId2Index = new Dictionary<int, int>();
            }

            this.fieldTypeCount = new Dictionary<FieldGroup, int>(FieldTypesHelper.BasicFieldTypes.Count);
            foreach (FieldGroup ft in FieldTypesHelper.BasicFieldTypes)
                fieldTypeCount.Add(ft, 0);

            this.decisionFieldIdx = -1;
            this.decisionFieldId = -1;
        }

        #endregion Constructor

        #region Methods

        public ICollection<long> GetDecisionValues()
        {
            return this.DecisionInfo.InternalValues();
        }

        public int GetFieldIndex(int fieldId)
        {
            int index;
            if (!fieldId2Index.TryGetValue(fieldId, out index))
                throw new ArgumentException(String.Format("Field {0} does not exist", fieldId), "fieldId");
            return index;
        }        

        public IEnumerable<AttributeInfo> SelectAttributes()
        {
            return Attributes;
        }

        public IEnumerable<AttributeInfo> SelectAttributes(Func<AttributeInfo, bool> selector)
        {
            return this.Attributes.Where(selector);            
        }

        public IEnumerable<int> SelectAttributeIds()
        {
            return Attributes.Select(f => f.Id);
        }

        public IEnumerable<int> SelectAttributeIds(Func<AttributeInfo, bool> selector)
        {
            return this.Attributes.Where(selector).Select(f => f.Id);
        }

        public int CountAttributes(Func<AttributeInfo, bool> selector)
        {
            return this.Attributes.Count(selector);
        }

        public void AddFieldInfo(AttributeInfo fieldInfo, FieldGroup fieldType = FieldGroup.None)
        {
            FieldGroup localFieldType = fieldType;
            if (localFieldType == FieldGroup.None)
            {
                if (fieldInfo.IsStandard)
                    localFieldType = FieldGroup.Standard;
                else if(fieldInfo.IsDecision)
                    localFieldType = FieldGroup.Output;
                else if(fieldInfo.IsIdentifier || fieldInfo.IsUnique)
                    localFieldType = FieldGroup.Id;
                else if (fieldInfo.IsSystem)
                    localFieldType = FieldGroup.Sys;
            }

            this.fieldId2Index.Add(fieldInfo.Id, this.fields.Count);

            this.fields.Add(fieldInfo.Id, fieldInfo);
            this.fieldTypes.Add(fieldInfo.Id, localFieldType);
            this.SetFieldMinMaxId(fieldInfo.Id);
            this.fieldTypeCount[localFieldType]++;

            if (localFieldType == FieldGroup.Output)
            {
                this.DecisionFieldId = fieldInfo.Id;
            }

            if (fieldInfo.HasMissingValues)
                this.HasMissingData = true;
        }

        public void RemoveFieldInfo(int fieldId)
        {                        
            this.fieldTypeCount[this.fieldTypes[fieldId]]--;
            this.fieldTypes.Remove(fieldId);
            this.fields.Remove(fieldId);
            this.NumberOfFields--;

            int fieldIndex = this.fieldId2Index[fieldId];
            fieldId2Index.Remove(fieldId);

            foreach (int idx in this.fieldId2Index.Keys.ToList())
            {
                if (this.fieldId2Index[idx] > fieldIndex)
                    this.fieldId2Index[idx]--;
            }

            if (this.decisionFieldIdx > fieldIndex)
                this.decisionFieldIdx--;
        }

        public void SetFieldIndex(int fieldId, int fieldIdx)
        {
            if (this.fieldId2Index.ContainsKey(fieldId))
            {
                this.fieldId2Index[fieldId] = fieldIdx;
            }
            else
            {
                this.fieldId2Index.Add(fieldId, fieldIdx);
            }
        }

        public AttributeInfo GetFieldInfo(int fieldId)
        {
            AttributeInfo res = null;
            if (fields.TryGetValue(fieldId, out res))
                return res;
            return null;
        }

        public long AddFieldValue(int fieldId, object externalValue, bool isMissing)
        {
            return this.GetFieldInfo(fieldId).Add(externalValue, isMissing);
        }

        public void AddFieldInternalValue(int fieldId, long internalValue, object externalValue, bool isMissing)
        {
            AttributeInfo attributeInfo = this.GetFieldInfo(fieldId);
            attributeInfo.AddInternal(internalValue, externalValue, isMissing);
        }

        private void SetFieldMinMaxId(int fieldId)
        {
            if (fieldId < minFieldId)
                minFieldId = fieldId;
            if (fieldId > maxFieldId)
                maxFieldId = fieldId;
        }

        public int NumberOfObjectsWithDecision(long decisionValue)
        {
            AttributeInfo decisionInfo;
            if (fields.TryGetValue(this.DecisionFieldId, out decisionInfo))
            {
                return (int)decisionInfo.Histogram.GetBinValue(decisionValue);
            }
            return 0;
        }

        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Digits(this.NumberOfFields);
            stringBuilder.Append('x');
            stringBuilder.Digits(this.NumberOfRecords);
            return stringBuilder.ToString();
        }

        public string ToStringInfo()
        {
            StringBuilder stringBuilder = new StringBuilder();
            foreach (AttributeInfo fieldInfo in this.Attributes)
            {
                stringBuilder.AppendFormat("Attribute {0} is {1} and has {2} distinct values",
                                           fieldInfo.Name,
                                           fieldInfo.DataType,
                                           fieldInfo.NumberOfValues); //fieldInfo.Histogram.Elements);
                stringBuilder.Append(Environment.NewLine);

                stringBuilder.AppendFormat("{0,-10}{1,-10}{2,10}", "Value", "Internal", "Count");
                stringBuilder.Append(Environment.NewLine);
                var histogram = fieldInfo.Histogram;
                foreach (long internalValue in fieldInfo.InternalValues())
                {
                    stringBuilder.AppendFormat("{0,-10}{1,-10}{2,10}",
                                                    fieldInfo.Internal2External(internalValue),
                                                    internalValue,
                                                    histogram.GetBinValue(internalValue));
                    stringBuilder.Append(Environment.NewLine);
                }
                stringBuilder.Append(Environment.NewLine);
            }
            stringBuilder.Append(Environment.NewLine);
            return stringBuilder.ToString();
        }

        public void InitFromDataStoreInfo(DataStoreInfo dataStoreInfo, bool initValues, bool initMissingValues)
        {
            this.NumberOfRecords = 0;
            this.NumberOfFields = dataStoreInfo.NumberOfFields;

            if (initValues)
            {
                this.minFieldId = dataStoreInfo.MinFieldId;
                this.maxFieldId = dataStoreInfo.MaxFieldId;
            }

            this.fields = new Dictionary<int, AttributeInfo>(dataStoreInfo.NumberOfFields);
            foreach (KeyValuePair<int, AttributeInfo> kvp in dataStoreInfo.fields)
            {
                AttributeInfo dfi = new AttributeInfo(kvp.Key, kvp.Value.DataType);
                dfi.InitFromDataFieldInfo(kvp.Value, initValues, initMissingValues);
                this.AddFieldInfo(dfi, dataStoreInfo.fieldTypes[kvp.Key]);
            }

            this.DecisionFieldId = dataStoreInfo.DecisionFieldId;

            if (initMissingValues)
                this.HasMissingData = dataStoreInfo.HasMissingData;
        }

        public void CreateWeightHistogram(DataStore data, double[] weights, int fieldId)
        {
            this.CreateWeightHistogram(data, weights, new int[] { fieldId });
        }

        public void CreateWeightHistogram(DataStore data, double[] weights, IEnumerable<int> fieldIds)
        {
            foreach (int fieldId in fieldIds)
            {
                AttributeInfo fieldInfo = this.GetFieldInfo(fieldId);
                fieldInfo.CreateWeightHistogram(data, weights);
            }
        }

        public void SetFieldType(int fieldId, FieldGroup fieldType)
        {
            if (this.fields.ContainsKey(fieldId))
            {
                FieldGroup oldFieldType = this.fieldTypes[fieldId];
                this.fieldTypes[fieldId] = fieldType;
                this.fieldTypeCount[oldFieldType]--;

                int count = 0;
                if (this.fieldTypeCount.TryGetValue(fieldType, out count))
                    this.fieldTypeCount[fieldType] = count + 1;
                else
                    this.fieldTypeCount.Add(fieldType, 1);
            }
        }

        public FieldGroup GetFieldType(int fieldId)
        {
            return this.fieldTypes[fieldId];
        }

        private int GetDecisionFieldIndex()
        {
            return this.GetFieldIndex(this.DecisionFieldId);
        }

        #endregion Methods
    }
}