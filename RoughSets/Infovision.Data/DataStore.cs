using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Raccoon.Math;
using Raccoon.Core;
using System.Data;

namespace Raccoon.Data
{
    [Serializable]
    public class DataStore : IDataTable, ICloneable
    {
        #region Members

        private long nextObjectId = 1;
        private long[] data;
        private long capacity;
        private int lastIndex;
        private double capacityFactor;

        private Dictionary<long, int> objectId2Index;
        
        private long[] index2ObjectId;
        private double[] weights;

        private long[] index2OrigObjectId; //used in case of bagging   

        private readonly object mutex = new object();

        #endregion

        #region Properties

        public string Name { get; set; }
        public int Fold { get; set; }
        public DataStoreInfo DataStoreInfo { get; set; }
        public bool IsBag { get { return this.index2OrigObjectId != null; } }
        public int NumberOfRecords { get { return this.DataStoreInfo.NumberOfRecords; } }
        public double[] Weights { get { return this.weights; } }

        #endregion

        #region Constructors

        public DataStore(DataStoreInfo dataStoreInfo)
        {
            this.DataStoreInfo = dataStoreInfo;
            this.InitStorage(dataStoreInfo.NumberOfRecords, dataStoreInfo.NumberOfFields, 0.1);
        }

        #endregion

        #region Methods

        private void InitStorage(int capacity, int attributeSize, double capacityFactor)
        {
            this.data = new long[capacity * attributeSize];
            this.capacity = capacity;
            this.capacityFactor = capacityFactor;
            this.lastIndex = -1;
            this.objectId2Index = new Dictionary<long, int>(capacity);
            this.index2ObjectId = new long[capacity];
            this.weights = new double[capacity];
        }        
       
        public int[] Sort(IComparer<int> comparer)
        {
            int[] sortedArray = Enumerable.Range(0, this.NumberOfRecords).ToArray();
            Array.Sort(sortedArray, comparer);
            return sortedArray;
        }        

        private bool CheckResize()
        {
            if (capacity == 0)
                return true;

            //if ((double)lastIndex > (double)capacity * (1.0M - capacityFactor) + 1.0M)
            //    return true;

            if (lastIndex >= capacity)
                return true;

            return false;
        }

        private void Resize()
        {
            long newCapacity = capacity != 0 ? Convert.ToInt64((double)capacity * (1 + capacityFactor)) + 1 : 1;
            long[] newStorage = new long[newCapacity * this.DataStoreInfo.NumberOfFields];
            Buffer.BlockCopy(data, 0, newStorage, 0, data.Length * sizeof(long));

            long[] newIndex2ObjectId = new long[newCapacity];
            Array.Copy(index2ObjectId, newIndex2ObjectId, this.index2ObjectId.Length);

            double[] newWeights = new double[newCapacity];
            Array.Copy(weights, newWeights, this.weights.Length);

            this.capacity = newCapacity;
            data = newStorage;
            index2ObjectId = newIndex2ObjectId;
        }

        public long AddRow(DataRecordInternal record)
        {
            long result = this.Insert(record);
            this.DataStoreInfo.NumberOfRecords++;
            //TODO Update other statistics

            return result;
        }

        public long GetOrigObjectId(int idx)
        {
            if (this.IsBag)
                return this.index2OrigObjectId[idx];
            return 0;
        }

        public long InsertBag(DataRecordInternal record)
        {
            if (record.ObjectId == 0)
                throw new ArgumentException("record", "record.ObjectId == 0");
            long origObjectId = record.ObjectId;
            record.ObjectId = 0;
            long newObjectId = this.Insert(record);            
            if (this.index2OrigObjectId == null)
                this.index2OrigObjectId = new long[capacity];
            this.index2OrigObjectId[record.ObjectIdx] = origObjectId;            
            return newObjectId;
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
            this.lastIndex++;

            if (this.CheckResize())
                this.Resize();

            foreach (int fieldId in record.GetFields())
            {
                long value = record[fieldId];
                this.data[lastIndex * this.DataStoreInfo.NumberOfFields + (fieldId - 1)] = value;
                var field = this.DataStoreInfo.GetFieldInfo(fieldId);
                field.IncreaseHistogramCount(value);
                field.IncreaseHistogramWeightsCount(value, 1);
            }

            //index2ObjectId.Add(lastIndex, record.ObjectId);
            this.index2ObjectId[lastIndex] = record.ObjectId;
            this.weights[lastIndex] = 1;

            this.objectId2Index.Add(record.ObjectId, lastIndex);
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
            Dictionary<int, long> valueMap = new Dictionary<int, long>(this.DataStoreInfo.NumberOfFields);

            foreach (int fieldId in this.DataStoreInfo.GetFieldIds())
                valueMap[fieldId] = this.GetFieldValue(objectIndex, fieldId);

            DataRecordInternal ret = new DataRecordInternal(valueMap);
            ret.ObjectIdx = objectIndex;

            if (setObjectId)
                ret.ObjectId = this.ObjectIndex2ObjectId(objectIndex);

            return ret;
        }

        public void SetWeights(double[] w)
        {
            if (w.Length != this.weights.Length)
                throw new ArgumentException("Invalid length of value vector", "w");

            Array.Copy(w, this.weights, w.Length);

            this.CreateWeightHistogramsOnFields();
        }

        internal void CreateWeightHistogramsOnFields()
        {
            foreach (var fieldInfo in this.DataStoreInfo.GetFields(FieldGroup.Standard))
                if (fieldInfo.HistogramWeights != null)
                    fieldInfo.CreateWeightHistogram(this, this.weights);

            foreach (var fieldInfo in this.DataStoreInfo.GetFields(FieldGroup.Output))
                fieldInfo.CreateWeightHistogram(this, this.weights);
        }

        public void NormalizeWeights()
        {
            Tools.Normalize(this.weights, this.weights.Sum());
        }

        public double GetWeight(int objectIdx)
        {
            return this.weights[objectIdx];
        }

        public void SetWeight(int objectIdx, double weight)
        {
            this.weights[objectIdx] = weight;
        }

        public double GetWeightByObjectId(long objectId)
        {
            return this.weights[this.objectId2Index[objectId]];
        }

        public void SetWeightByObjectId(long objectId, double weight)
        {
            this.weights[this.objectId2Index[objectId]] = weight;
        }

        public long[] GetColumnInternal(int fieldId)
        {
            int fieldIdx = this.DataStoreInfo.GetFieldIndex(fieldId);
            long[] result = new long[this.NumberOfRecords];            
            for (int i = 0; i < this.NumberOfRecords; i++)
                result[i] = this.GetFieldIndexValue(i, fieldIdx);
            return result;
        }

        public T[] GetColumn<T>(int fieldId)            
        {
            T[] result = new T[this.NumberOfRecords];
            DataFieldInfo field = this.DataStoreInfo.GetFieldInfo(fieldId);
            int fieldIdx = this.DataStoreInfo.GetFieldIndex(fieldId);
            try
            {
                for (int i = 0; i < this.NumberOfRecords; i++)
                    result[i] = (T) Convert.ChangeType(field.Internal2External(this.GetFieldIndexValue(i, fieldIdx)), typeof(T));
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
            catch (System.AggregateException aggregateException)
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
            int fieldIdx = this.DataStoreInfo.GetFieldIndex(fieldId);
            for (int i = 0; i < this.NumberOfRecords; i++)
                result[i] = field.Internal2External(this.GetFieldIndexValue(i, fieldIdx));
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
                this.data[i * this.DataStoreInfo.NumberOfFields + (fieldId - 1)] = internalValue;
                fieldInfo.IncreaseHistogramCount(internalValue);
                fieldInfo.IncreaseHistogramWeightsCount(internalValue, this.Weights[i]);
            }
        }

        public void SwitchColumns(int fieldId1, int fieldId2)
        {
            lock (mutex)
            {
                int fieldIdx1 = this.DataStoreInfo.GetFieldIndex(fieldId1);
                int fieldIdx2 = this.DataStoreInfo.GetFieldIndex(fieldId2);

                for (int i = 0; i < this.NumberOfRecords; i++)
                {
                    long tmp = this.GetFieldIndexValue(i, fieldIdx1);
                    this.SetFieldIndexValue(i, fieldIdx1, this.GetFieldIndexValue(i, fieldIdx2));
                    this.SetFieldIndexValue(i, fieldIdx2, tmp);
                }

                this.DataStoreInfo.SetFieldIndex(fieldId1, fieldIdx2);
                this.DataStoreInfo.SetFieldIndex(fieldId2, fieldIdx1);
            }
        }

        public void RemoveColumn(int fieldId)
        {
            lock (mutex)
            {
                long[] newArray = new long[(this.DataStoreInfo.NumberOfFields - 1) * this.NumberOfRecords];
                int j = 0;
                int fieldIdx = this.DataStoreInfo.GetFieldIndex(fieldId);
                for (int i = 0; i < this.DataStoreInfo.NumberOfFields * this.NumberOfRecords; i++)
                {
                    if (j != (fieldIdx % this.NumberOfRecords))
                        newArray[j++] = this.data[i];
                }
           
                this.DataStoreInfo.RemoveFieldInfo(fieldId);
            }
        }

        public void RemoveColumn(IEnumerable<int> fieldIds)
        {
            lock (mutex)
            {
                foreach (int fieldId in fieldIds)
                    RemoveColumn(fieldId);
            }
        }

        public int AddColumn<T>(T[] columnData, DataFieldInfo referenceFieldInfo = null)
        {
            if (columnData == null)
                throw new ArgumentNullException("columnData");
            if (columnData.Length != this.NumberOfRecords)
                throw new InvalidOperationException("columnData.Length != this.NumberOfRecords");
            
            long[] newData = new long[this.data.Length + columnData.Length];
            Parallel.For(0, this.NumberOfRecords, row =>
            {
                for (int col = 0; col < this.DataStoreInfo.NumberOfFields; col++)
                {
                    newData[(row * (this.DataStoreInfo.NumberOfFields + 1)) + col] 
                        = this.data[(row * this.DataStoreInfo.NumberOfFields) + col];
                }
            });

            DataFieldInfo newFieldInfo;
            long internalValue;
            bool isMissing;

            if (referenceFieldInfo == null)
            {
                newFieldInfo = new DataFieldInfo(this.DataStoreInfo.MaxFieldId + 1, typeof(T), 0);
                for (int row = 0; row < this.NumberOfRecords; row++)
                {
                    isMissing = this.DataStoreInfo.HasMissingData
                        && String.Equals(columnData[row], newFieldInfo.MissingValue);
                    internalValue = newFieldInfo.Add(columnData[row], isMissing);
                    newFieldInfo.AddInternal(internalValue, columnData[row], isMissing);
                    newFieldInfo.IncreaseHistogramCount(internalValue);
                    newFieldInfo.IncreaseHistogramWeightsCount(internalValue, this.Weights[row]);
                    newData[row * (this.DataStoreInfo.NumberOfFields + 1) + this.DataStoreInfo.NumberOfFields] = internalValue;
                }
            }
            else
            {
                newFieldInfo = new DataFieldInfo(referenceFieldInfo.Id, typeof(T), referenceFieldInfo.NumberOfValues);
                newFieldInfo.InitFromDataFieldInfo(referenceFieldInfo, true, true);
                for (int row = 0; row < this.NumberOfRecords; row++)
                {
                    isMissing = this.DataStoreInfo.HasMissingData
                        && String.Equals(columnData[row], newFieldInfo.MissingValue);
                    internalValue = referenceFieldInfo.Add(columnData[row], isMissing);
                    newFieldInfo.AddInternal(internalValue, columnData[row], isMissing);
                    newFieldInfo.IncreaseHistogramCount(internalValue);
                    newFieldInfo.IncreaseHistogramWeightsCount(internalValue, this.Weights[row]);
                    newData[row * (this.DataStoreInfo.NumberOfFields + 1) + this.DataStoreInfo.NumberOfFields] = internalValue;
                }
            }
            
            this.data = newData;
            this.DataStoreInfo.AddFieldInfo(newFieldInfo, FieldGroup.Standard);
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

        public AttributeValueVector GetDataVector(int objectIndex, HashSet<int> fieldSet)
        {
            return new AttributeValueVector(fieldSet.ToArray(), this.GetFieldValues(objectIndex, fieldSet), false);
        }

        public AttributeValueVector GetDataVector(int objectIdx)
        {
            return new AttributeValueVector(this.DataStoreInfo.GetFieldIds(FieldGroup.All).ToArray(), this.GetFieldValues(objectIdx), false);
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
            for (int i = 0; i < fieldIds.Length; i++)
            {
                result[i] = this.GetFieldValue(objectIndex, fieldIds[i]);
            }
            return result;
        }

        public long[] GetFieldValues(int objectIndex, HashSet<int> fieldSet)
        {
            long[] data = new long[fieldSet.Count];
            int j = 0;
            foreach (int fieldId in fieldSet)
                data[j++] = this.GetFieldValue(objectIndex, fieldId);
            return data;                
        }

        public long[] GetFieldValues(int objectIdx)
        {
            long[] result = new long[this.DataStoreInfo.NumberOfFields];
            for (int i = 0; i < this.DataStoreInfo.NumberOfFields; i++)
            {
                result[i] = data[objectIdx * this.DataStoreInfo.NumberOfFields + i];
            }
            return result;
        }

        public long[] GetFieldIndexValues(int objectIndex, int[] fieldIdxs)
        {
            long[] result = new long[fieldIdxs.Length];
            for (int i = 0; i < fieldIdxs.Length; i++)
                result[i] = this.GetFieldIndexValue(objectIndex, fieldIdxs[i]);
            return result;
        }

        public void GetFieldIndexValues(int objectIndex, int[] fieldIdxs, ref long[] cursor)
        {
            for (int i = 0; i < fieldIdxs.Length; i++)
                cursor[i] = this.GetFieldIndexValue(objectIndex, fieldIdxs[i]);
        }

        public long GetFieldValue(int objectIndex, int fieldId)
        {
            return this.GetFieldIndexValue(objectIndex, this.DataStoreInfo.GetFieldIndex(fieldId));
        }

        public long[] GetFieldValue(int[] objectIndices, int fieldId)
        {
            return this.GetFieldIndexValue(objectIndices, this.DataStoreInfo.GetFieldIndex(fieldId));
        }

        private void SetFieldIndexValue(int objectIdx, int fieldIdx, long internalValue)
        {
            data[objectIdx * this.DataStoreInfo.NumberOfFields + fieldIdx] = internalValue;
        }

        public long GetFieldIndexValue(int objectIdx, int fieldIdx)
        {
            return data[objectIdx * this.DataStoreInfo.NumberOfFields + fieldIdx];
        }

        public long[] GetFieldIndexValue(int[] objectIndices, int fieldIdx)
        {
            long[] values = new long[objectIndices.Length];
            for (int i = 0; i < objectIndices.Length; i++)
                values[i] = data[objectIndices[i] * this.DataStoreInfo.NumberOfFields + fieldIdx];
            return values;
        }

        public IEnumerable<long> GetOrigObjectIds()
        {
            return this.index2OrigObjectId != null ? this.index2OrigObjectId : new long[] { };
        }

        public IEnumerable<long> GetObjectIds()
        {
            return objectId2Index.Keys;
        }

        public void SetDecisionFieldId(int fieldId)
        {
            this.DataStoreInfo.DecisionFieldId = fieldId;

            //TODO this should be called inside setting this.DataStoreInfo.DecisionFieldId property but we miss reference to DataStore and its weights
            this.DataStoreInfo.DecisionInfo.CreateWeightHistogram(this, this.weights);
        }
        
        public long GetDecisionValue(int objectIndex)
        {
            return this.GetFieldIndexValue(objectIndex, this.DataStoreInfo.DecisionFieldIndex);
        }

        public long[] GetDecisionValue(int[] objectIndices)
        {
            long[] result = new long[objectIndices.Length];
            for (int i = 0; i < objectIndices.Length; i++)
                result[i] = this.GetFieldIndexValue(objectIndices[i], this.DataStoreInfo.DecisionFieldIndex);
            return result;
        }

        public long ObjectIndex2ObjectId(int objectIndex)
        {
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

                //TODO Extract this code to a method -->
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

                    if (position == this.DataStoreInfo.NumberOfFields)
                        stringBuilder.AppendFormat("{0}", externalValStr);
                    else
                        stringBuilder.AppendFormat("{0}{1}", externalValStr, separator);
                }
                //<--
                stringBuilder.Append(Environment.NewLine);
            }
            return stringBuilder.ToString();
        }

        public string ToStringHeader(string separator, bool decisionAsLastField = false)
        {
            StringBuilder stringBuilder = new StringBuilder();
            int position = 0;

            if (decisionAsLastField && this.DataStoreInfo.DecisionFieldId > 0)
            {
                foreach (DataFieldInfo field in this.DataStoreInfo.Fields)
                {
                    if (field.Id != this.DataStoreInfo.DecisionFieldId)
                    {
                        position++;
                        if (position == this.DataStoreInfo.NumberOfFields)
                            stringBuilder.AppendFormat("{0}", field.Name);
                        else
                            stringBuilder.AppendFormat("{0}{1}", field.Name, separator);
                    }
                }
                stringBuilder.AppendFormat("{0}", this.DataStoreInfo.DecisionInfo.Name);
            }
            else
            {
                foreach (DataFieldInfo field in this.DataStoreInfo.Fields)
                {
                    position++;
                    if (position == this.DataStoreInfo.NumberOfFields)
                        stringBuilder.AppendFormat("{0}", field.Name);
                    else
                        stringBuilder.AppendFormat("{0}{1}", field.Name, separator);
                }
            }
            stringBuilder.Append(Environment.NewLine);
            return stringBuilder.ToString();
        }

        public static DataStore Load(string fileName, FileFormat fileFormat, DataStoreInfo referenceDataStoreInfo)
        {
            IDataReader fileReader = DataReaderFile.Construct(fileFormat, fileName);
            fileReader.ReferenceDataStoreInfo = referenceDataStoreInfo;
            DataStore dataStore = DataStore.Load(fileReader);
            return dataStore;
        }

        public static DataStore Load(IDataReader dataReader)
        {
            DataStoreInfo dataStoreInfo = dataReader.Analyze();
            DataStore dataStore = new DataStore(dataStoreInfo);
            dataStore.Name = dataReader.DataName;
            dataReader.Load(dataStore, dataStoreInfo);
            return dataStore;
        }

        public static DataStore Load(string fileName, FileFormat fileFormat)
        {
            return DataStore.Load(fileName, fileFormat, null);
        }

        public void Dump(string filePath, string separator, bool includeHeader = false)
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

        public void DumpExt(string filePath, string separator, bool includeHeader = false, bool decisionAsLastField = false)
        {
            StringBuilder sb;
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(filePath))
            {
                for (int objectIndex = 0; objectIndex < this.DataStoreInfo.NumberOfRecords; objectIndex++)
                {
                    if (objectIndex == 0 && includeHeader)
                        file.Write(this.ToStringHeader(separator, decisionAsLastField));

                    DataRecordInternal record = this.GetRecordByIndex(objectIndex, false);
                    int position = 0;
                    sb = new StringBuilder();

                    if (decisionAsLastField && this.DataStoreInfo.DecisionFieldId > 0)
                    {
                        foreach (int fieldId in record.GetFields())
                        {
                            if (fieldId != this.DataStoreInfo.DecisionFieldId)
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
                        }
                        sb.AppendFormat("{0}",
                            this.DataStoreInfo.DecisionInfo.Internal2External(
                                record[this.DataStoreInfo.DecisionFieldId]));
                    }
                    else
                    {
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
                    }
                    file.WriteLine(sb.ToString());
                }
            }
        }

        public static DataStore Copy(DataStore dataToCopy, int startFromIdx, int recordCount)
        {
            int len = startFromIdx + recordCount;

            if (len > dataToCopy.NumberOfRecords)
                throw new ArgumentException("startFromIdx + recordCount > dataToCopy.NumberOfRecords ", "recordCount");
            if (startFromIdx < 0)
                throw new ArgumentException("startFromIdx < 0", "recordCount");

            DataStoreInfo localDataStoreInfo = new DataStoreInfo(dataToCopy.DataStoreInfo.NumberOfFields);
            localDataStoreInfo.InitFromDataStoreInfo(dataToCopy.DataStoreInfo, true, true);
            localDataStoreInfo.NumberOfRecords = recordCount;

            DataStore localDataStore = new DataStore(localDataStoreInfo);
            localDataStore.Name = dataToCopy.Name;

            if (dataToCopy.IsBag)
            {
                for (int i = startFromIdx, j = 0; i < len; i++)
                {
                    DataRecordInternal instance = dataToCopy.GetRecordByIndex(i, false);
                    instance.ObjectId = dataToCopy.GetOrigObjectId(i);
                    localDataStore.InsertBag(instance);
                    localDataStore.SetWeight(j++, dataToCopy.GetWeight(i));
                }
            }
            else
            {
                for (int i = startFromIdx, j = 0; i < len; i++)
                { 
                    DataRecordInternal instance = dataToCopy.GetRecordByIndex(i);
                    localDataStore.Insert(instance);
                    localDataStore.SetWeight(j++, dataToCopy.GetWeight(i));
                }
            }
            
            localDataStore.NormalizeWeights();
            localDataStore.CreateWeightHistogramsOnFields();

            return localDataStore;
        }

        public void Shuffle()
        {
            for (int j = this.NumberOfRecords - 1; j > 0; j--)
                this.Swap(j, RandomSingleton.Random.Next(j + 1));
        }

        public void Swap(int aidx, int bidx)
        {
            int numOfFields = this.DataStoreInfo.NumberOfFields;
            for (int i = 0; i < numOfFields; i++)
            {
                long tmpValue = this.GetFieldIndexValue(aidx, i);
                this.SetFieldIndexValue(aidx, i, this.GetFieldIndexValue(bidx, i));
                this.SetFieldIndexValue(bidx, i, tmpValue);
            }

            long aId = index2ObjectId[aidx];
            long bId = index2ObjectId[bidx];
            index2ObjectId[aidx] = index2ObjectId[bidx];
            index2ObjectId[bidx] = aId;

            objectId2Index[aId] = bidx;
            objectId2Index[bId] = aidx;

            if (weights != null)
            {
                double tmpWeigth = weights[aidx];
                weights[aidx] = weights[bidx];
                weights[bidx] = tmpWeigth;
            }
        }

        public int[] GetStandardFields()
        {
            return this.DataStoreInfo.GetFieldIds(FieldGroup.Standard).ToArray();
        }

        public T[][] ToArray<T>()            
        {
            return this.ToArray<T>(this.DataStoreInfo.GetFieldIds().ToArray());
        }

        public T[][] ToArray<T>(int[] fieldIds)
        {
            T[][] result = new T[this.NumberOfRecords][];
            for (int i = 0; i < result.Length; i++)
                result[i] = new T[fieldIds.Length];

            for (int i = 0; i < fieldIds.Length; i++)
            {
                DataFieldInfo fieldInfo = this.DataStoreInfo.GetFieldInfo(fieldIds[i]);
                if (fieldInfo.IsNumeric)
                {
                    T[] column = this.GetColumn<T>(fieldIds[i]);
                    for (int j = 0; j < this.NumberOfRecords; j++)
                        result[j][i] = column[j];
                }
                else
                {
                    long[] column = this.GetColumnInternal(fieldIds[i]);
                    for (int j = 0; j < this.NumberOfRecords; j++)        
                        result[j][i] = (T) Convert.ChangeType(column[j], typeof(T));
                }
            }

            return result;
        }

        public DataTable ToDataTable()
        {
            DataTable datatable = new DataTable(String.IsNullOrEmpty(this.Name) ? "" : this.Name);
            int[] fieldIds = this.DataStoreInfo.GetFieldIds().ToArray();
            for (int i = 0; i < fieldIds.Length; i++)
            {
                DataFieldInfo field = this.DataStoreInfo.GetFieldInfo(fieldIds[i]);
                datatable.Columns.Add(field.Name);
            }

            string[] recordStr = new string[fieldIds.Length];
            for (int i = 0; i < this.NumberOfRecords; i++)
            {
                AttributeValueVector record = this.GetDataVector(i, fieldIds);                
                for (int j = 0; j < fieldIds.Length; j++)
                {
                    DataFieldInfo field = this.DataStoreInfo.GetFieldInfo(fieldIds[j]);
                    object externalVal = field.Internal2External(record[j]);
                    if (field.HasMissingValues && record[j] == field.MissingValueInternal)
                        recordStr[j] = this.DataStoreInfo.MissingValue;
                    else
                        recordStr[j] = (externalVal != null) ? externalVal.ToString() : "?";
                }                
                datatable.Rows.Add(recordStr);
            }

            return datatable;
        }

        public object Clone()
        {
            DataStore data1 = null, data2 = null;
            new DataStoreSplitterRatio(this, 1.0).Split(ref data1, ref data2);
            return data1;
        }

        #region System.Object Methods

        public override string ToString()
        {
            return this.Name;
        }

        #endregion System.Object Methods

        #endregion Methods
    }
}