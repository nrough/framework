using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

using Infovision.Utils;
using System.Threading.Tasks;
using System.Collections.ObjectModel;

namespace Infovision.Data
{
    /// <summary>
    /// Provides meta information about dataset
    /// </summary>
    [Serializable]
    public class DataStoreInfo : Infovision.Data.IObjectSetInfo
    {
        #region Members
               
        private int minFieldId = Int32.MaxValue;
        private int maxFieldId = Int32.MinValue;
        private int decisionFieldId;
        private Dictionary<int, DataFieldInfo> fields;
        private Dictionary<int, FieldTypes> fieldTypes;
        private Dictionary<FieldTypes, int> fieldTypeCount;
        private Dictionary<int, int> fieldId2Index;

        #endregion

        #region Properties

        public int NumberOfRecords { get; set; }        
        public int NumberOfFields { get; set; }
        
        public int NumberOfDecisionValues
        {
            get { return this.GetFieldInfo(this.DecisionFieldId).InternalValues().Count; }
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
                    if (this.decisionFieldId != 0)
                    {
                        int prevDecisionFieldId = this.decisionFieldId;
                        fieldTypes[prevDecisionFieldId] = FieldTypes.Standard;
                    }
                    
                    this.decisionFieldId = value;

                    if (this.decisionFieldId > 0)
                        fieldTypes[this.decisionFieldId] = FieldTypes.Decision;
                }
            }
        }

        public int DecisionFieldIndex
        {
            get { return this.GetFieldIndex(this.DecisionFieldId); }
        }

        public DataFieldInfo DecisionInfo
        {
            get { return this.GetFieldInfo(this.DecisionFieldId); }
        }

        public IEnumerable<DataFieldInfo> Fields
        {
            get { return this.fields.Values; }
        }

        public bool HasMissingData { get; set; }
        public string MissingValue { get; set; }

        #endregion

        #region Constructor

        public DataStoreInfo(int numberOfFields = 0)
        {
            if (numberOfFields != 0)
            {
                this.fields = new Dictionary<int, DataFieldInfo>(numberOfFields);
                this.fieldTypes = new Dictionary<int, FieldTypes>(numberOfFields);
                this.fieldId2Index = new Dictionary<int, int>(numberOfFields);
                this.NumberOfFields = numberOfFields;
            }
            else
            {
                this.fields = new Dictionary<int, DataFieldInfo>();
                this.fieldTypes = new Dictionary<int, FieldTypes>();
                this.fieldId2Index = new Dictionary<int, int>();
            }

            this.fieldTypeCount = new Dictionary<FieldTypes, int>(FieldTypesHelper.BasicFieldTypes.Count);
            foreach (FieldTypes ft in FieldTypesHelper.BasicFieldTypes)
                fieldTypeCount.Add(ft, 0);
        }

        #endregion        

        #region Methods        

        public ICollection<long> GetDecisionValues()
        {
            return this.DecisionInfo.InternalValues();
        }

        public int[] GetFieldIds_OLD(FieldTypes fieldTypeFlags)
        {            
            int[] fieldIds = new int[this.GetNumberOfFields(fieldTypeFlags)];
            int i = 0;
            foreach (DataFieldInfo field in this.Fields)
            {
                if (fieldTypeFlags == FieldTypes.All
                    || fieldTypeFlags == FieldTypes.None
                    || this.fieldTypes[field.Id].HasFlag(fieldTypeFlags))
                {
                    fieldIds[i++] = field.Id;
                }
            }
            return fieldIds;
        }

        public int GetFieldIndex(int fieldId)
        {
            return this.fieldId2Index[fieldId];
        }

        public IEnumerable<int> GetFieldIds(FieldTypes fieldTypeFlags = FieldTypes.All)
        {
            if (fieldTypeFlags == FieldTypes.All || fieldTypeFlags == FieldTypes.None)
                return this.Fields.Select(f => f.Id);                       
                        
            return this.Fields
                .Where(field => this.fieldTypes[field.Id].HasFlag(fieldTypeFlags))
                .Select(f => f.Id);
        }

        public IEnumerable<DataFieldInfo> GetFields(FieldTypes fieldTypeFlags)
        {
            if (fieldTypeFlags == FieldTypes.All || fieldTypeFlags == FieldTypes.None)
                return this.Fields;
            return this.Fields.Where(field => this.fieldTypes[field.Id].HasFlag(fieldTypeFlags));            
        }

        public virtual int GetNumberOfFields(FieldTypes fieldTypeFlags)
        {
            if (fieldTypeFlags == FieldTypes.All
                || fieldTypeFlags == FieldTypes.None)
                return this.NumberOfFields;

            if (fieldTypeFlags == FieldTypes.Decision)
                return 1;

            int numberOfFields = this.NumberOfFields;
            int numberOfNotIncludedFields = 0;

            foreach(FieldTypes ft in FieldTypesHelper.BasicFieldTypes)
            {
                if(!fieldTypeFlags.HasFlag(ft))
                {
                    numberOfNotIncludedFields += this.fieldTypeCount[ft];
                }
            }                       
             
            return numberOfFields - numberOfNotIncludedFields;
        }

        public void AddFieldInfo(DataFieldInfo fieldInfo, FieldTypes fieldType)
        {
            this.fieldId2Index.Add(fieldInfo.Id, this.fields.Count);

            this.fields.Add(fieldInfo.Id, fieldInfo);
            this.fieldTypes.Add(fieldInfo.Id, fieldType);
            this.SetFieldMinMaxId(fieldInfo.Id);
            this.fieldTypeCount[fieldType]++;

            if (fieldType == FieldTypes.Decision)
            {                
                this.DecisionFieldId = fieldInfo.Id;
            }

            if (fieldInfo.HasMissingValues)
                this.HasMissingData = true;
        }

        public void RemoveFieldInfo(int fieldId)
        {
            int fieldIndex = this.fieldId2Index[fieldId];
            DataFieldInfo fieldInfo2Remove = this.fields[fieldId];
            this.fieldTypeCount[this.fieldTypes[fieldId]]--;
            this.fieldTypes.Remove(fieldId);
            this.fields.Remove(fieldId);
            this.NumberOfFields--;
            List<int> fieldIndexes = new List<int>(this.fieldId2Index.Keys);
            foreach (int idx in fieldIndexes)
            {
                if (this.fieldId2Index[idx] > fieldIndex)
                    this.fieldId2Index[idx]--;
            }
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

        public DataFieldInfo GetFieldInfo(int fieldId)
        {            
            return fields[fieldId];
        }

        public long AddFieldValue(int fieldId, object externalValue, bool isMissing)
        {
            return this.GetFieldInfo(fieldId).Add(externalValue, isMissing);
        }

        public void AddFieldInternalValue(int fieldId, long internalValue, object externalValue, bool isMissing)
        {
            DataFieldInfo attributeInfo = this.GetFieldInfo(fieldId);
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
            DataFieldInfo decisionInfo;
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
            foreach (DataFieldInfo fieldInfo in this.Fields)
            {
                stringBuilder.AppendFormat("Attribute {0} is {1} and has {2} distinct values",
                                           fieldInfo.Name,
                                           fieldInfo.FieldValueType,
                                           fieldInfo.Values().Count); //fieldInfo.Histogram.Elements);
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
            
            this.decisionFieldId = dataStoreInfo.DecisionFieldId;
            this.NumberOfFields = dataStoreInfo.NumberOfFields;

            if (initValues)
            {
                this.minFieldId = dataStoreInfo.MinFieldId;
                this.maxFieldId = dataStoreInfo.MaxFieldId;
            }

            this.fields = new Dictionary<int, DataFieldInfo>(dataStoreInfo.NumberOfFields);
            foreach (KeyValuePair<int, DataFieldInfo> kvp in dataStoreInfo.fields)
            {
                DataFieldInfo dfi = new DataFieldInfo(kvp.Key, kvp.Value.FieldValueType);
                dfi.InitFromDataFieldInfo(kvp.Value, initValues, initMissingValues);
                this.AddFieldInfo(dfi, dataStoreInfo.fieldTypes[kvp.Key]);
            }

            if (initMissingValues)
                this.HasMissingData = dataStoreInfo.HasMissingData;            
        }

        public void CreateWeightHistogram(DataStore data, decimal[] weights, int fieldId)
        {
            this.CreateWeightHistogram(data, weights, new int[] { fieldId });
        }

        public void CreateWeightHistogram(DataStore data, decimal[] weights, IEnumerable<int> fieldIds)
        {
            foreach (int fieldId in fieldIds)
            {
                DataFieldInfo fieldInfo = this.GetFieldInfo(fieldId);
                fieldInfo.CreateWeightHistogram(data, weights);
            }
        }

        #endregion

    }
}
