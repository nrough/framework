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

        private string name;
        private long nextObjectId = 1;
        private long[] data;
        private long capacity;
        private int lastIndex;
        private double capacityFactor;
        private Dictionary<long, int> objectId2Index;
        private Dictionary<int, long> index2ObjectId;
        private DataStoreInfo dataStoreInfo;
        private Dictionary<long, List<int>> decisionValue2ObjectIndex;

        #endregion        

        #region Properties

        public string Name
        {
            get { return name; }
            set { name = value; }
        }

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
            this.InitStorage(dataStoreInfo.NumberOfRecords, dataStoreInfo.NumberOfFields, 0);
        }

        private void InitStorage(int capacity, int attributeSize, double capacityFactor)
        {
            data = new long[capacity * attributeSize];
            this.capacity = capacity;
            this.capacityFactor = capacityFactor;
            lastIndex = -1;
            objectId2Index = new Dictionary<long, int>(capacity);
            index2ObjectId = new Dictionary<int, long>(capacity);
            decisionValue2ObjectIndex = new Dictionary<long, List<int>>();
        }

        #endregion

        #region Methods

        public int[] OrderBy(int[] orderBy, IComparer<int> comparer)
        {
            int[] sortedArray = new int[this.NumberOfRecords];
            for (int i = 0; i < sortedArray.Length; i++)            
                sortedArray[i] = i;            

            Array.Sort(sortedArray, comparer);
            return sortedArray;
        }

        public int[] OrderBy(int[] orderBy, ObjectSet objectSet, IComparer<int> comparer)
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
            if ((double)lastIndex + 0.0001 > (double)capacity * (1 - capacityFactor) + 1.0)
                return true;
            return false;
        }

        private void Resize()
        {
            long newCapacity = Convert.ToInt32((double)capacity * (1 + capacityFactor));
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

            long decisionValue = record[this.DataStoreInfo.DecisionFieldId];            
            List<int> objectList = null;
            if (!decisionValue2ObjectIndex.TryGetValue(decisionValue, out objectList))
            {
                objectList = new List<int>();
                decisionValue2ObjectIndex[decisionValue] = objectList;
            }
            objectList.Add(lastIndex);

            index2ObjectId.Add(lastIndex, record.ObjectId);
            objectId2Index.Add(record.ObjectId, lastIndex);
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
            long[] fieldValue = new Int64[this.dataStoreInfo.NumberOfFields];

            if (objectIndex < 0 || objectIndex > this.dataStoreInfo.NumberOfRecords)
                throw new ArgumentOutOfRangeException("objectIndex", "Index out of range.");

            for (int i = 0; i < this.dataStoreInfo.NumberOfFields; i++)
            {
                fieldId[i] = i + 1;
                fieldValue[i] = this.GetObjectField(objectIndex, fieldId[i]);
            }

            DataRecordInternal ret = new DataRecordInternal(fieldId, fieldValue);
            ret.ObjectId = this.ObjectIndex2ObjectId(objectIndex);
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
            return new AttributeValueVector(fieldIds, this.GetObjectFields(objectIndex, fieldIds), false);
        }

        public AttributeValueVector GetDataVector(int objectIndex, FieldSet fieldSet)
        {
            return new AttributeValueVector(fieldSet.ToArray(), this.GetObjectFields(objectIndex, fieldSet), false);
        }

        public long[] GetObjectFields(int objectIndex, int[] fieldIds)
        {
            long[] data = new long[fieldIds.Length];
            for (int i = 0; i < fieldIds.Length; i++)
            {
                data[i] = this.GetObjectField(objectIndex, fieldIds[i]);
            }
            return data;
        }

        public long[] GetObjectFields(int objectIndex, FieldSet fieldSet)
        {
            long[] data = new long[fieldSet.Count];
            int j = 0;
            for (int i = 0; i < fieldSet.Data.Length; i++)
            {
                if(fieldSet.Data.Get(i))
                    data[j++] = this.GetObjectField(objectIndex, i + fieldSet.LowerBound); 
            }
            return data;
        }

        public long GetObjectField(int objectIndex, int fieldId)
        {
            if(fieldId < this.DataStoreInfo.MinFieldId || fieldId > this.DataStoreInfo.MaxFieldId)
                throw new ArgumentOutOfRangeException("fieldId", "Value is out of range");

            if(objectIndex < 0 || objectIndex > this.NumberOfRecords-1)
                throw new ArgumentOutOfRangeException("objectIndex", "Value is out of range");
            
            int index = objectIndex * this.dataStoreInfo.NumberOfFields + (fieldId - 1);

            return data[index];
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
            List<int> result = null;
            if (!this.decisionValue2ObjectIndex.TryGetValue(decisionValue, out result))
            {
                return new int[] { };
            }

            return this.decisionValue2ObjectIndex[decisionValue].ToArray();
        }

        public long GetDecisionValue(int objectIndex)
        {
            return this.GetObjectField(objectIndex, this.DataStoreInfo.DecisionFieldId);
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

        public double PriorDecisionProbability(long decisionValue)
        {
            return this.DataStoreInfo.PriorDecisionProbability(decisionValue);
        }

        public string ToStringInternal(string separator)
        {
            StringBuilder stringBuilder = new StringBuilder();

            stringBuilder.Append(this.ToStringHeader(separator));
            
            for (int objectIndex = 0; objectIndex < this.DataStoreInfo.NumberOfRecords; objectIndex++)
            {
                DataRecordInternal record = this.GetRecordByIndex(objectIndex);
                //stringBuilder.AppendFormat("{0}: ", objectIndex + 1);
                foreach (int fieldId in record.GetFields())
                {
                    stringBuilder.AppendFormat("{0}{1}", record[fieldId], separator);
                }
                stringBuilder.Append(Environment.NewLine);
            }
            stringBuilder.Append(Environment.NewLine);
            return stringBuilder.ToString();
        }

        public string ToStringExternal(string separator)
        {
            StringBuilder stringBuilder = new StringBuilder();
            
            stringBuilder.Append(this.ToStringHeader(separator));

            for (int objectIndex = 0; objectIndex < this.DataStoreInfo.NumberOfRecords; objectIndex++)
            {
                DataRecordInternal record = this.GetRecordByIndex(objectIndex);
                //stringBuilder.AppendFormat("{0}: ", objectIndex + 1);
                foreach (int fieldId in record.GetFields())
                {
                    DataFieldInfo attr = this.DataStoreInfo.GetFieldInfo(fieldId);
                    object externalVal = attr.Internal2External(record[fieldId]);
                    string externalValStr = (externalVal != null) ? externalVal.ToString() : "?";
                    stringBuilder.AppendFormat("{0}{1}", externalValStr, separator);
                }
                stringBuilder.Append(Environment.NewLine);                
            }
            stringBuilder.Append(Environment.NewLine);
            return stringBuilder.ToString();
        }

        public string ToStringHeader(string separator)
        {
            StringBuilder stringBuilder = new StringBuilder();
            foreach (DataFieldInfo field in this.DataStoreInfo.Fields)
            {
                stringBuilder.AppendFormat("{0}{1}", field.Name, separator);
            }
            stringBuilder.Append(Environment.NewLine);
            return stringBuilder.ToString();
        }

        public static DataStore Load(string fileName, FileFormat fileFormat, DataStoreInfo referenceDataStoreInfo)
        {
            IDataReader fileReader = DataReaderFile.Construct(fileFormat, fileName);
            DataStore dataStore = DataStore.Load(fileReader, referenceDataStoreInfo);
            return dataStore;
        }

        public static DataStore Load(IDataReader dataReader, DataStoreInfo referenceDataStoreInfo)
        {
            DataStoreInfo dataStoreInfo = dataReader.Analyze();
            DataStore dataStore = new DataStore(dataStoreInfo);
            dataStore.Name = dataReader.DataName;

            dataReader.Load(dataStoreInfo, dataStore, referenceDataStoreInfo);

            return dataStore;
        }
        
        public static DataStore Load(IDataReader dataReader)
        {
            return DataStore.Load(dataReader, null);
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
