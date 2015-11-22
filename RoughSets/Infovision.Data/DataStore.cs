using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
        private decimal capacityFactor;
        private Dictionary<long, int> objectId2Index;
        private Dictionary<int, long> index2ObjectId;
        private DataStoreInfo dataStoreInfo;
        private Dictionary<long, List<int>> decisionValue2ObjectIndex;
        private bool isDecisionMapCalculated = false;
        
        public static object syncRoot = new object();

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
            this.InitStorage(dataStoreInfo.NumberOfRecords, dataStoreInfo.NumberOfFields, 0.2M);            
        }

        private void InitStorage(int capacity, int attributeSize, decimal capacityFactor)
        {
            data = new long[capacity * attributeSize];
            this.capacity = capacity;
            this.capacityFactor = capacityFactor;
            lastIndex = -1;
            objectId2Index = new Dictionary<long, int>(capacity);
            index2ObjectId = new Dictionary<int, long>(capacity);            
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
            long newCapacity = capacity != 0 ? Convert.ToInt32((decimal)capacity * (1 + capacityFactor)) + 1 : 1;
            long[] newStorage = new long[newCapacity * this.dataStoreInfo.NumberOfFields];
            Buffer.BlockCopy(data, 0, newStorage, 0, data.Length * sizeof(Int64));
            this.capacity = newCapacity;
            data = newStorage;
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

            index2ObjectId.Add(lastIndex, record.ObjectId);
            objectId2Index.Add(record.ObjectId, lastIndex);
            record.ObjectIdx = lastIndex;

            this.dataStoreInfo.RecordInserted(record);
        }

        public DataRecordInternal GetRecordByObjectId(long objectId)
        {
            int objectIndex = -1;
            if (!objectId2Index.TryGetValue(objectId, out objectIndex))
            {
                //return new DataRecordInternal(fieldId, fieldValue);
                throw new ArgumentOutOfRangeException("objectId", "Object with specified Id does not exist.");
            }

            return this.GetRecordByIndex(objectIndex);
        }

        public DataRecordInternal GetRecordByIndex(int objectIndex)
        {
            int[] fieldId = new int[this.dataStoreInfo.NumberOfFields];
            long[] fieldValue = new long[this.dataStoreInfo.NumberOfFields];

            if (objectIndex < 0 || objectIndex > this.dataStoreInfo.NumberOfRecords)
                throw new ArgumentOutOfRangeException("objectIndex", "Index out of range.");

            for (int i = 0; i < this.dataStoreInfo.NumberOfFields; i++)
            {
                fieldId[i] = i + 1;
                fieldValue[i] = this.GetFieldValue(objectIndex, fieldId[i]);
            }

            DataRecordInternal ret = new DataRecordInternal(fieldId, fieldValue);
            ret.ObjectId = this.ObjectIndex2ObjectId(objectIndex);
            ret.ObjectIdx = objectIndex;

            return ret;
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

        public long[] GetFieldValues(int objectIndex, int[] fieldIds)
        {
            long[] data = new long[fieldIds.Length];
            for (int i = 0; i < fieldIds.Length; i++)
                data[i] = this.GetFieldValue(objectIndex, fieldIds[i]);
            return data;
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

        public long GetFieldValue(int objectIndex, int fieldId)
        {
            if(fieldId < this.DataStoreInfo.MinFieldId)
                throw new ArgumentOutOfRangeException("fieldId", "Value is out of range");
            if(fieldId > this.DataStoreInfo.MaxFieldId)
                throw new ArgumentOutOfRangeException("fieldId", "Value is out of range");
            if(objectIndex < 0)
                throw new ArgumentOutOfRangeException("objectIndex", "Value is out of range");
            if(objectIndex > this.NumberOfRecords-1)
                throw new ArgumentOutOfRangeException("objectIndex", "Value is out of range");

            return data[objectIndex * this.dataStoreInfo.NumberOfFields + (fieldId - 1)];
        }

        public long[] GetObjectIds()
        {
            return objectId2Index.Keys.ToArray<long>();
        }

        public int[] GetObjectIndexes()
        {
            return index2ObjectId.Keys.ToArray<int>();
        }

        public int[] GetObjectIndexes(long decisionValue)
        {
            List<int> result;
            if (this.isDecisionMapCalculated == false)
            {
                lock (syncRoot)
                {
                    if (this.isDecisionMapCalculated == false)
                    {
                        this.decisionValue2ObjectIndex = new Dictionary<long, List<int>>();
                        foreach (int objectIdx in this.GetObjectIndexes())
                        {
                            long decision = this.GetDecisionValue(objectIdx);
                            result = null;
                            if (!this.decisionValue2ObjectIndex.TryGetValue(decisionValue, out result))
                            {
                                result = new List<int>();
                                this.decisionValue2ObjectIndex.Add(decisionValue, result);
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

            return this.decisionValue2ObjectIndex[decisionValue].ToArray();
        }

        public void DecisionChanged()
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
            long objectId;
            if (index2ObjectId.TryGetValue(objectIndex, out objectId))
            {
                return objectId;
            }
            return 0;
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
                DataRecordInternal record = this.GetRecordByIndex(objectIndex);
                //sb.AppendFormat("{0}: ", objectIndex + 1);
                int position = 0;
                foreach (int fieldId in record.GetFields())
                {
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
                DataRecordInternal record = this.GetRecordByIndex(objectIndex);
                //sb.AppendFormat("{0}: ", objectIndex + 1);
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
                        externalValStr = (externalVal != null) ? externalVal.ToString() : "N/A";
                    
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

        public void WriteToCSVFile(string filePath, string separator)
        {            
            System.IO.File.WriteAllText(filePath, this.ToStringInternal(separator));
        }

        public void WriteToCSVFileExt(string filePath, string separator)
        {
            System.IO.File.WriteAllText(filePath, this.ToStringExternal(separator));
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
