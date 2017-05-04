using NRough.Core;
using NRough.Core.BaseTypeExtensions;
using NRough.Core.CollectionExtensions;
using NRough.Core.Helpers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NRough.Data.Readers
{
    public class DataStreamReader : DataReaderBase
    {        
        private Dictionary<int, Type> typePool = new Dictionary<int, Type>();
        private Dictionary<int, int> numberOfDecimals = new Dictionary<int, int>();
        private Dictionary<int, int> missingValuesCount = new Dictionary<int, int>();
        private Dictionary<int, HashSet<string>> valueCount = new Dictionary<int, HashSet<string>>();
        
        public char[] FieldDelimiter { get; set; }
        public Stream Stream { get; set; }
        public int Header { get; set; }

        public DataStreamReader()
        {
            FieldDelimiter = new char[] { ' ', ';', '\t', ',', '|' };
        }
        
        public DataStreamReader(Stream stream)
            : this()
        {
            Stream = stream;            
        }

        #region Methods

        public override DataStore Read()
        {            
            DataStoreInfo dataStoreInfo = Analyze();
            DataStore dataStore = new DataStore(dataStoreInfo);
            dataStore.Name = DataName;
            Load(dataStore, dataStoreInfo);
            return dataStore;
        }                

        public char[] GetFieldDelimiter()
        {
            return FieldDelimiter;
        }

        public virtual DataStoreInfo Analyze()
        {
            string line = String.Empty;
            int rows = 0;

            MemoryStream streamCopy = new MemoryStream();
            try
            {
                Stream.Position = 0;
                Stream.CopyTo(streamCopy);
                streamCopy.Position = 0;
                Stream.Position = 0;

                using (StreamReader streamReader = new StreamReader(streamCopy))
                {
                    this.AnalyzeHeader(streamReader);
                    line = streamReader.ReadLine();
                    while (!String.IsNullOrEmpty(line))
                    {
                        rows++;
                        this.AnalyzeLine(line, rows);
                        line = streamReader.ReadLine();
                    }
                }
            }
            finally
            {
                if (streamCopy != null)
                    streamCopy.Dispose();
            }

            if (!this.CheckNumberOfRows(rows))
            {
                throw new System.InvalidOperationException(String.Format(CultureInfo.InvariantCulture,
                                                                         "Wrong number of fileLine. Expected number: {0}, Actual number: {1}",
                                                                         ExpectedRows,
                                                                         rows));
            }
            else
            {
                ExpectedRows = rows;
            }

            DataStoreInfo dataStoreInfo = new DataStoreInfo(ExpectedColumns);
            dataStoreInfo.MissingValue = this.MissingValue;
            dataStoreInfo.NumberOfRecords = rows;

            for (int i = 1; i <= dataStoreInfo.NumberOfFields; i++)
            {
                FieldGroup fieldType = FieldGroup.Standard;
                if (i == this.DecisionId)
                {
                    fieldType = FieldGroup.Output;
                }

                AttributeInfo referenceFieldInfo = null;
                if (this.ReferenceDataStoreInfo != null)
                {
                    referenceFieldInfo = this.ReferenceDataStoreInfo.GetFieldInfo(i);
                }

                AttributeInfo fieldInfo =
                    new AttributeInfo(
                        i,
                        (referenceFieldInfo == null)
                            ? this.AttributeType(i - 1)
                            : referenceFieldInfo.DataType,
                        this.valueCount[i - 1].Count);

                if (referenceFieldInfo != null)
                {
                    fieldInfo.InitFromReferenceAttribute(referenceFieldInfo);
                                        
                    foreach (long internalValue in referenceFieldInfo.InternalValues())
                    {
                        object externalValue = referenceFieldInfo.Internal2External(internalValue);
                        bool isMissing = referenceFieldInfo.MissingValueInternal == internalValue;
                        fieldInfo.AddInternal(internalValue, externalValue, isMissing);
                    }

                }
                else
                {
                    fieldInfo.IsNumeric = AttributeInfo.IsNumericType(fieldInfo.DataType);
                    fieldInfo.IsSymbolic = !fieldInfo.IsNumeric;
                    fieldInfo.IsOrdered = fieldInfo.IsNumeric;
                    fieldInfo.NumberOfDecimals = this.GetNumberOfDecimals(i - 1);

                    fieldInfo.IsDecision = (i == dataStoreInfo.DecisionFieldId);
                    if (fieldInfo.IsDecision)
                        fieldInfo.IsNumeric = false;

                    fieldInfo.IsStandard = !fieldInfo.IsDecision;
                    
                }

                if (this.missingValuesCount.ContainsKey(i - 1))
                {
                    fieldInfo.HasMissingValues = true;
                }

                dataStoreInfo.AddFieldInfo(fieldInfo, fieldType);
            }

            return dataStoreInfo;
        }

        protected virtual void DecodeFileInfo(string fileInfo)
        {
            string[] fields = fileInfo.Split(FieldDelimiter, StringSplitOptions.RemoveEmptyEntries);
            ExpectedColumns = fields.Length;
            if (this.ReferenceDataStoreInfo != null && this.ReferenceDataStoreInfo.DecisionFieldId > 0)
            {
                this.DecisionId = this.ReferenceDataStoreInfo.DecisionFieldId;
            }
            else
            {
                this.DecisionId = fields.Length;
            }
        }

        protected virtual bool CheckNumberOfColumns(int numberOfColumns)
        {
            if (ExpectedColumns != -1)
            {
                if (numberOfColumns != this.ExpectedColumns)
                    return false;
            }
            else
            {
                if (numberOfColumns <= 0)
                    return false;
            }

            return true;
        }

        protected virtual bool CheckNumberOfRows(int numberOfRows)
        {
            if (ExpectedRows != -1)
            {
                if (numberOfRows != ExpectedRows)
                    return false;
            }
            else
            {
                if (numberOfRows <= 0)
                    return false;
            }

            return true;
        }

        public virtual void Load(DataStore dataStore, DataStoreInfo dataStoreInfo)
        {
            int numberOfFields = this.ReferenceDataStoreInfo != null 
                ? this.ReferenceDataStoreInfo.NumberOfFields 
                : dataStoreInfo.NumberOfFields;

            int[] fieldId = new int[numberOfFields];
            long[] fieldValue = new long[numberOfFields];
            string line = String.Empty;
            int linenum = 0;
            int i = 0;

            try
            {
                MemoryStream streamCopy = new MemoryStream(); ;
                try
                {
                    Stream.Position = 0;
                    Stream.CopyTo(streamCopy);
                    streamCopy.Position = 0;
                    Stream.Position = 0;

                    using (StreamReader streamReader = new StreamReader(streamCopy))
                    {
                        this.SkipHeader(streamReader);
                        while ((line = streamReader.ReadLine()) != null)
                        {
                            linenum++;

                            string[] fileLine = line.Split(FieldDelimiter, StringSplitOptions.RemoveEmptyEntries);

                            if (fileLine.Length != dataStoreInfo.NumberOfFields)
                                throw new System.InvalidOperationException();

                            IComparable[] typedFieldValues = new IComparable[fileLine.Length];

                            for (i = 0; i < fileLine.Length; i++)
                            {
                                if (this.HandleMissingData && String.Equals(fileLine[i], this.MissingValue))
                                {
                                    switch (Type.GetTypeCode(dataStoreInfo.GetFieldInfo(i + 1).DataType))
                                    {
                                        case TypeCode.Int32:
                                            typedFieldValues[i] = Int32.MaxValue;
                                            break;

                                        case TypeCode.Double:
                                            typedFieldValues[i] = Double.MaxValue;
                                            break;

                                        case TypeCode.Int64:
                                            typedFieldValues[i] = Int64.MaxValue;
                                            break;

                                        case TypeCode.String:
                                            typedFieldValues[i] = this.MissingValue;
                                            break;

                                        default:
                                            throw new System.InvalidOperationException();
                                    }
                                }
                                else
                                {
                                    switch (Type.GetTypeCode(dataStoreInfo.GetFieldInfo(i + 1).DataType))
                                    {
                                        case TypeCode.Int32:
                                            typedFieldValues[i] = Int32.Parse(fileLine[i], CultureInfo.InvariantCulture);
                                            break;

                                        case TypeCode.Int64:
                                            typedFieldValues[i] = Int64.Parse(fileLine[i], CultureInfo.InvariantCulture);
                                            break;

                                        case TypeCode.Double:
                                            typedFieldValues[i] = Double.Parse(fileLine[i], CultureInfo.InvariantCulture);
                                            break;

                                        case TypeCode.String:
                                            typedFieldValues[i] = (string)fileLine[i].Clone();
                                            break;

                                        default:
                                            throw new System.InvalidOperationException();
                                    }
                                }
                            }

                            for (i = 0; i < dataStoreInfo.NumberOfFields; i++)
                            {
                                fieldId[i] = i + 1;

                                IComparable externalValue = typedFieldValues[i];
                                long internalValue;
                                bool isMissing = this.HandleMissingData && String.Equals(fileLine[i], this.MissingValue);

                                if (this.ReferenceDataStoreInfo != null)
                                {
                                    AttributeInfo localFieldInfo = this.ReferenceDataStoreInfo.GetFieldInfo(fieldId[i]);
                                    if (localFieldInfo.IsNumeric && localFieldInfo.Cuts != null)
                                    {
                                        for (int j = 0; j < localFieldInfo.Cuts.Length; j++)
                                        {
                                            //TODO We need to provide a way to dynamically convert types
                                            if (externalValue.CompareTo(localFieldInfo.Cuts[j]) <= 0)
                                                externalValue = j;
                                        }
                                    }

                                    internalValue = this.ReferenceDataStoreInfo.AddFieldValue(fieldId[i], externalValue, isMissing);
                                    dataStoreInfo.AddFieldInternalValue(fieldId[i], internalValue, externalValue, isMissing);
                                }
                                else
                                {
                                    internalValue = dataStoreInfo.AddFieldValue(fieldId[i], externalValue, isMissing);
                                }

                                fieldValue[i] = internalValue;
                            }

                            dataStore.Insert(new DataRecordInternal(fieldId, fieldValue));
                        }
                    }
                }
                finally
                {
                    if (streamCopy != null)
                        streamCopy.Dispose();
                }
            }
            catch (Exception e)
            {
                throw new InvalidProgramException(
                    String.Format("Error in line {0}, field {1}, exception message was: {2}", linenum, i + 1, e.Message), e);
            }

            dataStore.Weights.SetAll(1);

            dataStore.NormalizeWeights();

            if (this.DecisionId != -1)
                dataStore.DataStoreInfo.CreateWeightHistogram(
                    dataStore,
                    dataStore.Weights,
                    this.DecisionId);
        }

        protected virtual void AnalyzeHeader(StreamReader streamReader)
        {
            SkipHeader(streamReader);
        }

        protected virtual void SkipHeader(StreamReader streamReader)
        {
            int i = 0;
            while (i < Header && streamReader.ReadLine() != null)
                i++;
        }

        protected virtual void AnalyzeLine(string line, int lineNum)
        {
            string[] fields = line.Split(FieldDelimiter, StringSplitOptions.RemoveEmptyEntries);
            if (lineNum == 1 && ExpectedColumns == -1)
            {
                this.DecodeFileInfo(line);
            }
            else if (!this.CheckNumberOfColumns(fields.Length))
            {
                throw new System.MissingFieldException(String.Format(CultureInfo.InvariantCulture,
                                                                     "Wrong number of columns in row {0} (was: {1} expected: {2}",
                                                                     lineNum,
                                                                     fields.Length,
                                                                     ExpectedColumns));
            }

            for (int i = 0; i < fields.Length; i++)
            {
                string value = fields[i];
                if (this.HandleMissingData && String.Equals(value, this.MissingValue))
                {
                    int count = 0;
                    if (missingValuesCount.TryGetValue(i, out count))
                        missingValuesCount[i] = count + 1;
                    else
                        missingValuesCount[i] = 1;
                }
                else if (this.ReferenceDataStoreInfo == null)
                {
                    this.AddValue2TypePool(i, value);
                    this.CheckMaxNumberOfDecimals(i, value);
                }

                HashSet<string> fieldValues = null;
                if (!valueCount.TryGetValue(i, out fieldValues))
                {
                    fieldValues = new HashSet<string>();
                    this.valueCount[i] = fieldValues;
                }

                fieldValues.Add(value);
            }
        }

        private int GetNumberOfDecimals(int fieldIndex)
        {
            int result;
            if (this.numberOfDecimals.TryGetValue(fieldIndex, out result))
                return result;
            return 0;
        }

        private Type AttributeType(int fieldIndex)
        {
            Type result = null;
            if (this.typePool.TryGetValue(fieldIndex, out result))
            {
                return result;
            }

            return typeof(string);
        }

        private void CheckMaxNumberOfDecimals(int fieldIndex, string value)
        {
            if (!AttributeInfo.IsNumericType(this.AttributeType(fieldIndex)))
                return;

            int count = value.GetNumberOfDecimals();
            int maxNumberOfDec = 0;

            if (numberOfDecimals.TryGetValue(fieldIndex, out maxNumberOfDec))
            {
                if (maxNumberOfDec < count)
                    numberOfDecimals[fieldIndex] = count;
            }
            else
            {
                numberOfDecimals[fieldIndex] = count;
            }
        }

        private void AddValue2TypePool(int fieldIndex, string value)
        {
            Type previousType = null;
            typePool.TryGetValue(fieldIndex, out previousType);

            if (previousType != null && previousType == typeof(string))
                return;

            Type type = MiscHelper.String2Type(value);

            if (previousType == null)
            {
                this.typePool[fieldIndex] = type;
                return;
            }

            if ((type == typeof(double)
                || type == typeof(string))
                && type != previousType)
            {
                this.typePool[fieldIndex] = type;
                return;
            }

            return;
        }

        #endregion Methods     
    }
}
