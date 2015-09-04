using System;
using System.Collections.Generic;
using System.Text;

using Infovision.Utils;

namespace Infovision.Data
{
    /// <summary>
    /// Provides meta-data information about dataset
    /// </summary>
    [Serializable]
    public class DataStoreInfo : Infovision.Data.IObjectSetInfo
    {
        #region Globals
        
        private int recordCount;
        private int fieldCount;
        private int minFieldId = Int32.MaxValue;
        private int maxFieldId = Int32.MinValue;
        private int decisionFieldId;
        private Dictionary<int, DataFieldInfo> fields = new Dictionary<int, DataFieldInfo>();
        private Dictionary<int, FieldTypes> fieldTypes = new Dictionary<int, FieldTypes>();
        private Dictionary<FieldTypes, int> fieldTypeCount = new Dictionary<FieldTypes, int>();
        private double[] recordWeights;
        
        #endregion

        #region Properties

        public int NumberOfRecords
        {
            get { return this.recordCount; }
            set { this.recordCount = value; }
        }

        public int NumberOfFields
        {
            get { return this.fieldCount; }
            set { this.fieldCount = value; }
        }

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
            set { this.decisionFieldId = value; }
        }

        public DataFieldInfo DecisionInfo
        {
            get { return this.GetFieldInfo(this.DecisionFieldId); }
        }

        public IEnumerable<DataFieldInfo> Fields
        {
            get { return this.fields.Values; }
        }

        public double[] RecordWeights
        {
            get { return this.recordWeights; }
            set { this.recordWeights = value; }
        }

        #region Constructor

        public DataStoreInfo()
        {
            foreach (FieldTypes ft in FieldTypesHelper.BasicFieldTypes)
            {
                fieldTypeCount.Add(ft, 0);
            }
        }

        #endregion

        #endregion

        #region Methods

        public double GetRecordWeight(int objectIdx)
        {
            return this.recordWeights[objectIdx];
        }

        public ICollection<long> GetDecisionValues()
        {
            return this.DecisionInfo.InternalValues();
        }

        public int[] GetFieldIds(FieldTypes fieldTypeFlags)
        {
            int size = this.GetNumberOfFields(fieldTypeFlags);

            int[] fieldIds = new int[size];
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

        public virtual int GetNumberOfFields(FieldTypes fieldTypeFlags)
        {

            if (fieldTypeFlags == FieldTypes.All)
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
            this.fields.Add(fieldInfo.Id, fieldInfo);
            this.fieldTypes.Add(fieldInfo.Id, fieldType);
            this.SetFieldMinMaxId(fieldInfo.Id);
            this.fieldTypeCount[fieldType]++;

            if (fieldType == FieldTypes.Decision)
            {
                this.decisionFieldId = fieldInfo.Id;
            }
        }

        public DataFieldInfo GetFieldInfo(int fieldId)
        {
            return fields[fieldId];
        }

        public DataFieldInfo GetDecisionFieldInfo()
        {
            return this.DecisionInfo;
        }

        public long AddFieldValue(int fieldId, object externalValue)
        {
            return this.GetFieldInfo(fieldId).Add(externalValue);
        }

        public void AddFieldInternalValue(int fieldId, long internalValue, object externalValue)
        {
            DataFieldInfo attributeInfo = this.GetFieldInfo(fieldId);
            attributeInfo.AddInternal(internalValue, externalValue);
        }

        /*

        public void SetDecisionField(int fieldId, bool isDecision)
        {
            if (this.fields.ContainsKey(fieldId))
            {
                DataFieldInfo attribute = fields[fieldId];
                attribute.IsDecision = isDecision;
                decisionFieldId = isDecision ? fieldId : 0;
            }
            else
            {
                throw new ArgumentOutOfRangeException("fieldId", String.Format("FieldId {0} does not exist in DataStore", fieldId));
            }
        }
        */

        private void SetFieldMinMaxId(int fieldId)
        {
            if (fieldId < minFieldId)
                minFieldId = fieldId;
            if (fieldId > maxFieldId)
                maxFieldId = fieldId;
        }

        public double PriorDecisionProbability(long decisionValue)
        {
            return (double) this.NumberOfObjectsWithDecision(decisionValue) / (double)this.NumberOfRecords;
        }

        public int NumberOfObjectsWithDecision(long decisionValue)
        {
            DataFieldInfo decisionInfo;
            if (fields.TryGetValue(this.DecisionFieldId, out decisionInfo))
            {
                return decisionInfo.Histogram.GetBinValue(decisionValue);
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
                Histogram histogram = fieldInfo.Histogram;
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

        public void InitFromDataStoreInfo(DataStoreInfo dataStoreInfo)
        {
            this.recordCount            = 0;
            this.decisionFieldId        = dataStoreInfo.DecisionFieldId;
            this.fieldCount             = dataStoreInfo.NumberOfFields;    
            this.minFieldId             = dataStoreInfo.MinFieldId;
            this.maxFieldId             = dataStoreInfo.MaxFieldId;

            this.fields = new Dictionary<int, DataFieldInfo>(dataStoreInfo.NumberOfFields);
            foreach(KeyValuePair<int, DataFieldInfo > kvp in dataStoreInfo.fields)
            {
                DataFieldInfo dfi = new DataFieldInfo(kvp.Key, kvp.Value.FieldValueType);
                dfi.InitFromDataFieldInfo(kvp.Value);
                this.AddFieldInfo(dfi, dataStoreInfo.fieldTypes[kvp.Key]);
            }         
        }

        #endregion

    }
}
