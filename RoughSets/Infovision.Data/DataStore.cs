﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infovision.Data
{
    [Serializable]
    public class DataStore : IDataTable
    {
        #region Members
        
        private long nextObjectId = 1;
        private long[] data;
        private long capacity;
        private int lastIndex;
        private double capacityFactor;

        private Dictionary<long, int> objectId2Index;
        //private Dictionary<int, long> index2ObjectId;
        
        private long[] index2ObjectId;
        private DataStoreInfo dataStoreInfo;
        private Dictionary<long, List<int>> decisionValue2ObjectIndex;
        private bool isDecisionMapCalculated = false;
 
        private object mutex = new object();

        #endregion        

        #region Properties

        public string Name { get; set; }

        public DataStoreInfo DataStoreInfo
        {
            get { return dataStoreInfo; }
            set { DataStoreInfo = value; }
        }

        public int NumberOfRecords 
        {
            get { return this.DataStoreInfo.NumberOfRecords; } 
        }        
        
        #endregion

        #region Constructors

        public DataStore(DataStoreInfo dataStoreInfo)
        {
            this.dataStoreInfo = dataStoreInfo;
            this.InitStorage(dataStoreInfo.NumberOfRecords, dataStoreInfo.NumberOfFields, 0.1);            
        }

        private void InitStorage(int capacity, int attributeSize, double capacityFactor)
        {
            this.data = new long[capacity * attributeSize];
            this.capacity = capacity;
            this.capacityFactor = capacityFactor;
            this.lastIndex = -1;
            this.objectId2Index = new Dictionary<long, int>(capacity);
            //index2ObjectId = new Dictionary<int, long>(capacity);
            this.index2ObjectId = new long[capacity];
        }

        #endregion

        #region Methods

        public int[] Sort(IComparer<int> comparer)
        {
            int[] sortedArray = new int[this.NumberOfRecords];
            for (int i = 0; i < sortedArray.Length; i++)            
                sortedArray[i] = i;

            Array.Sort(sortedArray, comparer);
            return sortedArray;
        }

        public int[] Sort(ObjectSet objectSet, IComparer<int> comparer)
        {
            int[] objects = objectSet.ToArray();
            int[] sortedArray = new int[objects.Length];
            
            for (int i = 0; i < objects.Length; i++)            
                sortedArray[i] = objects[i];            

            Array.Sort<int>(sortedArray, comparer);
            return sortedArray;
        }

        private bool CheckResize()
        {
            if (capacity == 0)
                return true;

            //if ((decimal)lastIndex > (decimal)capacity * (1.0M - capacityFactor) + 1.0M)
            //    return true;

            if (lastIndex >= capacity)
                return true;

            return false;

        }

        private void Resize()
        {
            long newCapacity = capacity != 0 ? Convert.ToInt64((double)capacity * (1 + capacityFactor)) + 1 : 1;
            long[] newStorage = new long[newCapacity * this.dataStoreInfo.NumberOfFields];
            Buffer.BlockCopy(data, 0, newStorage, 0, data.Length * sizeof(long));
            
            long[] newIndex2ObjectId = new long[newCapacity];
            Array.Copy(index2ObjectId, newIndex2ObjectId, this.index2ObjectId.Length);
            
            this.capacity = newCapacity;
            data = newStorage;
            index2ObjectId = newIndex2ObjectId;
        }

        public long Insert(DataRecordInternal record)
        {
            if (record.ObjectId == 0)
            {                
                record.ObjectId = nextObjectId;
                nextObjectId++;
            }
            this.InsertRecord(record);
            return record.ObjectId;
        }

        private void InsertRecord(DataRecordInternal record)
        {
            lastIndex++;

            if (this.CheckResize())
                this.Resize();
            
            foreach (int fieldId in record.GetFields())
            {
                long value = record[fieldId];
                data[lastIndex * this.dataStoreInfo.NumberOfFields + (fieldId - 1)] = value;
                this.dataStoreInfo.GetFieldInfo(fieldId).IncreaseHistogramCount(value);
            }

            //index2ObjectId.Add(lastIndex, record.ObjectId);
            index2ObjectId[lastIndex] = record.ObjectId;

            objectId2Index.Add(record.ObjectId, lastIndex);
            record.ObjectIdx = lastIndex;            
        }

        public DataRecordInternal GetRecordByObjectId(long objectId)
        {
            int objectIndex;
            if (objectId2Index.TryGetValue(objectId, out objectIndex) == false)
                throw new ArgumentOutOfRangeException("objectId", "Object with specified Id does not exist.");

            DataRecordInternal result = this.GetRecordByIndex(objectIndex, false);
            result.ObjectId = objectId;
            return result;
        }

        public DataRecordInternal GetRecordByIndex(int objectIndex, bool setObjectId = true)
        {
            Dictionary<int, long> valueMap = new Dictionary<int, long>(this.dataStoreInfo.NumberOfFields);
            for (int i = 1; i <= this.dataStoreInfo.NumberOfFields; i++)
                valueMap[i] = this.GetFieldValue(objectIndex, i);
            DataRecordInternal ret = new DataRecordInternal(valueMap);
            ret.ObjectIdx = objectIndex;

            if(setObjectId)
                ret.ObjectId = this.ObjectIndex2ObjectId(objectIndex);

            return ret;
        }

        public long[] GetColumnInternal(int fieldId)
        {
            long[] result = new long[this.NumberOfRecords];
            Parallel.For(0, this.NumberOfRecords, i =>
            {
                result[i] = this.GetFieldValue(i, fieldId);
            });
            return result;
        }

        public T[] GetColumn<T>(int fieldId)
        {
            T[] result = new T[this.NumberOfRecords];
            DataFieldInfo field = this.DataStoreInfo.GetFieldInfo(fieldId);
            try
            {                
                Parallel.For(0, this.NumberOfRecords, i =>
                {
                    result[i] = (T)field.Internal2External(this.GetFieldValue(i, fieldId));
                });
            }
            catch (InvalidCastException castException)
            {
                throw new InvalidCastException(
                    String.Format("Invalid cast exception for field {0} (Discovered type was: {1}, Expected type was {2}", 
                        fieldId,
                        field.FieldValueType.ToString(), 
                        typeof(T).ToString()),
                    castException);
            }
            catch(System.AggregateException aggregateException)
            {
                throw new AggregateException(
                    String.Format("Exception was thrown for field {0} (Discovered type was: {1}, Expected type was {2}",
                        fieldId,
                        field.FieldValueType.ToString(),
                        typeof(T).ToString()),
                    aggregateException);
            }

            return result;
        }

        public object[] GetColumn(int fieldId)
        {
            object[] result = new object[this.NumberOfRecords];
            DataFieldInfo field = this.DataStoreInfo.GetFieldInfo(fieldId);
            Parallel.For(0, this.NumberOfRecords, i =>
            {
                result[i] = field.Internal2External(this.GetFieldValue(i, fieldId));
            });
            return result;
        }

        public void UpdateColumn(int fieldId, object[] data, DataFieldInfo referenceFieldInfo = null)
        {
            DataFieldInfo fieldInfo = this.DataStoreInfo.GetFieldInfo(fieldId);
            fieldInfo.Reset();
            long internalValue;
            bool isMissing;
            for (int i = 0; i < this.NumberOfRecords; i++)
            {
                isMissing = this.DataStoreInfo.HasMissingData && String.Equals(data[i], fieldInfo.MissingValue);                
                if (referenceFieldInfo != null)
                {
                    internalValue = referenceFieldInfo.Add(data[i], isMissing);
                    fieldInfo.AddInternal(internalValue, data[i], isMissing);
                }
                else
                {
                    internalValue = fieldInfo.Add(data[i], isMissing);
                }
                this.data[i * this.dataStoreInfo.NumberOfFields + (fieldId - 1)] = internalValue;
                fieldInfo.IncreaseHistogramCount(internalValue);
            }
        }

        public int AddColumn<T>(T[] columnData, DataFieldInfo referenceFieldInfo = null)
        {
            if (columnData.Length != this.NumberOfRecords)
                throw new InvalidOperationException("Column data have different size than this dataset");

            long internalValue;
            bool isMissing;
            long[] newData = new long[this.data.Length + columnData.Length];
            Parallel.For(0, this.NumberOfRecords, row =>
            {
                for (int col = 0; col < this.DataStoreInfo.NumberOfFields; col++)
                {
                    newData[(row * (this.dataStoreInfo.NumberOfFields + 1)) + col] = this.data[(row * this.dataStoreInfo.NumberOfFields) + col];
                }
            });

            int newFieldId = this.DataStoreInfo.MaxFieldId + 1;
            DataFieldInfo newFieldInfo = new DataFieldInfo(newFieldId, typeof(T), referenceFieldInfo != null ? referenceFieldInfo.Values().Count : 0);
            for (int row = 0; row < this.NumberOfRecords; row++)
            {
                isMissing = this.DataStoreInfo.HasMissingData && String.Equals(columnData[row], newFieldInfo.MissingValue);
                internalValue = referenceFieldInfo != null
                    ? referenceFieldInfo.Add(columnData[row], isMissing)
                    : newFieldInfo.Add(columnData[row], isMissing);
                newFieldInfo.AddInternal(internalValue, columnData[row], isMissing);
                newFieldInfo.IncreaseHistogramCount(internalValue);
                newData[row * (this.dataStoreInfo.NumberOfFields + 1) + this.DataStoreInfo.NumberOfFields] = internalValue;
            }

            this.data = newData;
            this.DataStoreInfo.AddFieldInfo(newFieldInfo, FieldTypes.Standard);
            this.DataStoreInfo.NumberOfFields++;
            
            return newFieldInfo.Id;
        }

        public bool Exists(long objectId)
        {
            if (objectId2Index.ContainsKey(objectId))
                return true;
            return false;
        }

        public AttributeValueVector GetDataVector(int objectIndex, int[] fieldIds)
        {
            return new AttributeValueVector(fieldIds, this.GetFieldValues(objectIndex, fieldIds), false);
        }

        public AttributeValueVector GetDataVector(int objectIndex, FieldSet fieldSet)
        {
            return new AttributeValueVector(fieldSet.ToArray(), this.GetFieldValues(objectIndex, fieldSet), false);
        }

        public AttributeValueVector GetDataVector(int objectIdx)
        {
            return new AttributeValueVector(this.DataStoreInfo.GetFieldIds(FieldTypes.All).ToArray(), this.GetFieldValues(objectIdx), false);
        }

        public void GetFieldValues(int objectIndex, int[] fieldIds, ref long[] cursor)
        {                        
            for (int i = 0; i < fieldIds.Length; i++)
            {
                cursor[i] = this.GetFieldValue(objectIndex, fieldIds[i]);
            }
        }

        public long[] GetFieldValues(int objectIndex, int[] fieldIds)
        {
            long[] result = new long[fieldIds.Length];            
            for(int i = 0; i<fieldIds.Length; i++)
            {
                result[i] = this.GetFieldValue(objectIndex, fieldIds[i]);
            }            
            return result;
        }

        public long[] GetFieldValues(int objectIndex, FieldSet fieldSet)
        {
            long[] data = new long[fieldSet.Count];
            int j = 0;
            for (int i = 0; i < fieldSet.Data.Length; i++)
            {
                if(fieldSet.Data.Get(i))
                    data[j++] = this.GetFieldValue(objectIndex, i + fieldSet.LowerBound); 
            }
            return data;
        }

        public long[] GetFieldValues(int objectIdx)
        {
            long[] result = new long[this.DataStoreInfo.NumberOfFields];            
            for(int i = 0; i<this.DataStoreInfo.NumberOfFields; i++)
            {
                result[i] = data[objectIdx * this.dataStoreInfo.NumberOfFields + i];
            }            
            return result;
        }

        public long GetFieldValue(int objectIndex, int fieldId)
        {
            /*
            if(fieldId < this.DataStoreInfo.MinFieldId)
                throw new ArgumentOutOfRangeException("fieldIds", "Value is out of range");
            if(fieldId > this.DataStoreInfo.MaxFieldId)
                throw new ArgumentOutOfRangeException("fieldIds", "Value is out of range");
            if(objectIndex < 0)
                throw new ArgumentOutOfRangeException("objectIndex", "Value is out of range");
            if(objectIndex > this.NumberOfRecords-1)
                throw new ArgumentOutOfRangeException("objectIndex", "Value is out of range");
            */ 

            return data[objectIndex * this.dataStoreInfo.NumberOfFields + (fieldId - 1)];
        }

        public IEnumerable<long> GetObjectIds()
        {
            return objectId2Index.Keys;
        }

        /*
        public IEnumerable<int> GetObjectIndexes()
        {
            //return index2ObjectId.Keys;
            return Enumerable.Range(0, index2ObjectId.Length);
        }
        */

        public IEnumerable<int> GetObjectIndexes(long decisionValue)
        {
            List<int> result;
            if (this.isDecisionMapCalculated == false)
            {
                lock (mutex)
                {
                    if (this.isDecisionMapCalculated == false)
                    {
                        this.decisionValue2ObjectIndex = new Dictionary<long, List<int>>(this.DataStoreInfo.NumberOfDecisionValues);

                        for (int objectIdx = 0; objectIdx < this.NumberOfRecords; objectIdx++)
                        {
                            long decision = this.GetDecisionValue(objectIdx);
                            result = null;
                            if (!this.decisionValue2ObjectIndex.TryGetValue(decision, out result))
                            {
                                result = new List<int>(this.DataStoreInfo.NumberOfObjectsWithDecision(decision));
                                this.decisionValue2ObjectIndex.Add(decision, result);
                            }
                            result.Add(objectIdx);
                        }
                        this.isDecisionMapCalculated = true;
                    }
                }
            }

            result = null;
            if (!this.decisionValue2ObjectIndex.TryGetValue(decisionValue, out result))
                return new int[] { };

            return result;
        }

        protected void DecisionChanged()
        {
            this.isDecisionMapCalculated = false;
        }

        public void SetDecisionFieldId(int fieldId)
        {
            this.DataStoreInfo.DecisionFieldId = fieldId;
            this.DecisionChanged();
        }

        public long GetDecisionValue(int objectIndex)
        {
            return this.GetFieldValue(objectIndex, this.DataStoreInfo.DecisionFieldId);
        }

        public long ObjectIndex2ObjectId(int objectIndex)
        {
            /*
            long objectId;
            if (index2ObjectId.TryGetValue(objectIndex, out objectId))
                return objectId;
            return 0;
            */
            return index2ObjectId[objectIndex];
        }

        public int ObjectId2ObjectIndex(long objectId)
        {
            int objectIndex;
            if (objectId2Index.TryGetValue(objectId, out objectIndex))
            {
                return objectIndex;
            }
            return -1;
        }

        public decimal PriorDecisionProbability(long decisionValue)
        {
            return this.DataStoreInfo.PriorDecisionProbability(decisionValue);
        }

        public string ToStringInternal(string separator, bool includeHeader = false)
        {
            StringBuilder stringBuilder = new StringBuilder();

            if (includeHeader)
                stringBuilder.Append(this.ToStringHeader(separator));
            
            for (int objectIndex = 0; objectIndex < this.DataStoreInfo.NumberOfRecords; objectIndex++)
            {
                DataRecordInternal record = this.GetRecordByIndex(objectIndex, false);
                //sb.AppendFormat("{0}: ", objectIndex + 1);
                int position = 0;
                foreach (int fieldId in record.GetFields())
                {
                    position++;
                    if (position == this.DataStoreInfo.NumberOfFields)
                        stringBuilder.AppendFormat("{0}", record[fieldId]);
                    else
                        stringBuilder.AppendFormat("{0}{1}", record[fieldId], separator);
                }
                stringBuilder.Append(Environment.NewLine);
            }            
            return stringBuilder.ToString();
        }

        public string ToStringExternal(string separator, bool includeHeader = false)
        {
            StringBuilder stringBuilder = new StringBuilder();

            if (includeHeader)
                stringBuilder.Append(this.ToStringHeader(separator));

            for (int objectIndex = 0; objectIndex < this.DataStoreInfo.NumberOfRecords; objectIndex++)
            {
                DataRecordInternal record = this.GetRecordByIndex(objectIndex, false);
                int position = 0;
                foreach (int fieldId in record.GetFields())
                {
                    position++;
                    DataFieldInfo attr = this.DataStoreInfo.GetFieldInfo(fieldId);
                    object externalVal = attr.Internal2External(record[fieldId]);
                    string externalValStr = String.Empty;
                    
                    if (attr.HasMissingValues && record[fieldId] == attr.MissingValueInternal)
                        externalValStr = this.DataStoreInfo.MissingValue;
                    else
                        externalValStr = (externalVal != null) ? externalVal.ToString() : "?";
                    
                    if(position == this.DataStoreInfo.NumberOfFields)
                        stringBuilder.AppendFormat("{0}", externalValStr);
                    else
                        stringBuilder.AppendFormat("{0}{1}", externalValStr, separator);
                }
                stringBuilder.Append(Environment.NewLine);                
            }            
            return stringBuilder.ToString();
        }

        public string ToStringHeader(string separator)
        {
            StringBuilder stringBuilder = new StringBuilder();
            int position = 0;
            foreach (DataFieldInfo field in this.DataStoreInfo.Fields)
            {
                position++;
                if(position == this.DataStoreInfo.NumberOfFields)
                    stringBuilder.AppendFormat("{0}", field.Name);
                else
                    stringBuilder.AppendFormat("{0}{1}", field.Name, separator);
            }
            stringBuilder.Append(Environment.NewLine);
            return stringBuilder.ToString();
        }

        public static DataStore Load(string fileName, FileFormat fileFormat, DataStoreInfo referenceDataStoreInfo)
        {
            IDataReader fileReader = DataReaderFile.Construct(fileFormat, fileName);
            fileReader.HandleMissingData = true;
            fileReader.MissingValue = "?";

            fileReader.ReferenceDataStoreInfo = referenceDataStoreInfo;
            DataStore dataStore = DataStore.Load(fileReader);
            return dataStore;
        }

        public static DataStore Load(IDataReader dataReader)
        {
            DataStoreInfo dataStoreInfo = dataReader.Analyze();
            DataStore dataStore = new DataStore(dataStoreInfo);
            dataStore.Name = dataReader.DataName;
            dataReader.Load(dataStoreInfo, dataStore);            
            return dataStore;
        }              

        public static DataStore Load(string fileName, FileFormat fileFormat)
        {
            return DataStore.Load(fileName, fileFormat, null);
        }

        public void WriteToCSVFile(string filePath, string separator, bool includeHeader = false)
        {            
            //System.IO.File.WriteAllText(filePath, this.ToStringInternal(separator));

            StringBuilder sb;
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(filePath))
            {
                for (int objectIndex = 0; objectIndex < this.DataStoreInfo.NumberOfRecords; objectIndex++)
                {
                    if (objectIndex == 0 && includeHeader)
                        file.WriteLine(this.ToStringHeader(separator));

                    DataRecordInternal record = this.GetRecordByIndex(objectIndex, false);
                    int position = 0;
                    sb = new StringBuilder();
                    foreach (int fieldId in record.GetFields())
                    {
                        position++;
                        if (position == this.DataStoreInfo.NumberOfFields)
                            sb.AppendFormat("{0}", record[fieldId]);
                        else
                            sb.AppendFormat("{0}{1}", record[fieldId], separator);
                    }

                    file.WriteLine(sb.ToString());
                }
            }
        }

        public void WriteToCSVFileExt(string filePath, string separator, bool includeHeader = false)
        {
            //System.IO.File.WriteAllText(filePath, this.ToStringExternal(separator));
            
            StringBuilder sb;
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(filePath))
            {
                for (int objectIndex = 0; objectIndex < this.DataStoreInfo.NumberOfRecords; objectIndex++)
                {
                    if (objectIndex == 0 && includeHeader)
                        file.WriteLine(this.ToStringHeader(separator));

                    DataRecordInternal record = this.GetRecordByIndex(objectIndex, false);
                    int position = 0;
                    sb = new StringBuilder();
                    foreach (int fieldId in record.GetFields())
                    {
                        position++;
                        DataFieldInfo attr = this.DataStoreInfo.GetFieldInfo(fieldId);
                        object externalVal = attr.Internal2External(record[fieldId]);
                        string externalValStr = String.Empty;

                        if (attr.HasMissingValues && record[fieldId] == attr.MissingValueInternal)
                            externalValStr = this.DataStoreInfo.MissingValue;
                        else
                            externalValStr = (externalVal != null) ? externalVal.ToString() : "?";

                        if (position == this.DataStoreInfo.NumberOfFields)
                            sb.AppendFormat("{0}", externalValStr);
                        else
                            sb.AppendFormat("{0}{1}", externalValStr, separator);
                    }

                    file.WriteLine(sb.ToString());
                }
            }
        }

        #region System.Object Methods

        public override string ToString()
        {
            return this.Name;
        }

        #endregion

        #endregion
    }        
}
