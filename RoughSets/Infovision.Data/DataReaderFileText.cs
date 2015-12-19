﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Infovision.Utils;

namespace Infovision.Data
{
    public class DataReaderFileCsv : DataReaderFile
    {
        #region Members
        
        private Char[] fieldDelimiter = new Char[] { ' ', ';', '\t', ',' };
        private int header = 0;
        private int expectedColums = -1;
        private int expectedRows = -1;        
        private Dictionary<int, Type> typePool = new Dictionary<int, Type>();
        private Dictionary<int, int> missingValuesCount = new Dictionary<int, int>();
        #endregion

        #region Properties

        public int ExpectedColumns
        {
            get { return this.expectedColums; }
            set { this.expectedColums = value; }
        }

        public int ExpectedRows
        {
            get { return this.expectedRows; }
            set { this.expectedRows = value; }
        }

        #endregion

        #region Constructors

        public DataReaderFileCsv(string filePath)
            : base()
        {
            this.FileName = filePath;
        }

        public DataReaderFileCsv(string fileName, int header)
            : this(fileName)
        {
            this.header = header;
        }

        public DataReaderFileCsv(string fileName, int header, char[] fieldDelimiter)
            : this(fileName, header)
        {
            this.fieldDelimiter = fieldDelimiter;        
        }

        #endregion        

        #region Methods

        public char[] GetFieldDelimiter()
        {
            return this.fieldDelimiter;
        }

        public override DataStoreInfo Analyze()
        {
            DataStoreInfo dataStoreInfo = new DataStoreInfo();
            dataStoreInfo.MissingValue = this.MissingValue;

            string line = String.Empty;
            int rows = 0;

            using (FileStream fileStream = new FileStream(this.FileName, FileMode.Open, FileAccess.Read))
            {
                using (StreamReader streamReader = new StreamReader(fileStream))
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

            if (!this.CheckNumberOfRows(rows))
            {
                throw new System.InvalidOperationException(String.Format(CultureInfo.InvariantCulture,
                                                                         "Wrong number of fields. Expected number: {0}, Actual number: {1}",
                                                                         this.expectedRows,
                                                                         rows));
            }
            else
            {
                this.expectedRows = rows;
            }

            dataStoreInfo.NumberOfFields = this.ExpectedColumns;
            dataStoreInfo.NumberOfRecords = rows;

            for (int i = 1; i <= dataStoreInfo.NumberOfFields; i++)
            {
                FieldTypes fieldType = FieldTypes.Standard;
                if (i == this.DecisionId)
                {
                    fieldType = FieldTypes.Decision;
                }

                DataFieldInfo fieldInfo = new DataFieldInfo(i, (this.ReferenceDataStoreInfo == null) ? this.AttributeType(i-1) : this.ReferenceDataStoreInfo.GetFieldInfo(i).FieldValueType);
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
            string[] fields = fileInfo.Split(fieldDelimiter, StringSplitOptions.RemoveEmptyEntries);
            this.expectedColums = fields.Length;
            this.DecisionId = fields.Length;
        }

        protected virtual bool CheckNumberOfColumns(int numberOfColumns)
        {
            if (this.expectedColums != -1)
            {
                if (numberOfColumns != this.expectedColums)
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
            if (this.expectedRows != -1)
            {
                if (numberOfRows != this.expectedRows)
                    return false;
            }
            else
            {
                if (numberOfRows <= 0)
                    return false;
            }

            return true;
        }

        public override void Load(DataStoreInfo dataStoreInfo, DataStore dataStore)
        {
            int numberOfFields = this.ReferenceDataStoreInfo != null ? this.ReferenceDataStoreInfo.NumberOfFields : dataStoreInfo.NumberOfFields;
            int[] fieldId = new int[numberOfFields];
            long[] fieldValue = new long[numberOfFields];
            string line = String.Empty;
            int linenum = 0; int i = 0;

            try
            {
                using (FileStream fileStream = new FileStream(this.FileName, FileMode.Open, FileAccess.Read))
                {
                    using (StreamReader streamReader = new StreamReader(fileStream))
                    {
                        this.SkipHeader(streamReader);
                        while ((line = streamReader.ReadLine()) != null)
                        {
                            linenum++;

                            string[] fields = line.Split(fieldDelimiter, StringSplitOptions.RemoveEmptyEntries);

                            if (fields.Length != dataStoreInfo.NumberOfFields)
                                throw new System.InvalidOperationException();

                            IComparable[] typedFieldValues = new IComparable[fields.Length];

                            //TODO Iterate over datafieldinfo with foreach loop
                            for (i = 0; i < fields.Length; i++)
                            {                                
                                if (this.HandleMissingData && String.Equals(fields[i], this.MissingValue))
                                {
                                    switch (Type.GetTypeCode(dataStoreInfo.GetFieldInfo(i + 1).FieldValueType))
                                    {
                                        case TypeCode.Int32:
                                            typedFieldValues[i] = Int32.MinValue;
                                            break;

                                        case TypeCode.Decimal:
                                            typedFieldValues[i] = Decimal.MinValue;
                                            break;

                                        case TypeCode.Double:
                                            typedFieldValues[i] = Double.MinValue;
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
                                    switch (Type.GetTypeCode(dataStoreInfo.GetFieldInfo(i + 1).FieldValueType))
                                    {
                                        case TypeCode.Int32:
                                            typedFieldValues[i] = Int32.Parse(fields[i], CultureInfo.InvariantCulture);
                                            break;

                                        case TypeCode.Decimal:
                                            typedFieldValues[i] = Decimal.Parse(fields[i], CultureInfo.InvariantCulture);
                                            break;

                                        case TypeCode.Double:
                                            typedFieldValues[i] = Double.Parse(fields[i], CultureInfo.InvariantCulture);
                                            break;

                                        case TypeCode.String:
                                            typedFieldValues[i] = (string)fields[i].Clone();
                                            break;

                                        default:
                                            throw new System.InvalidOperationException();
                                    }
                                }
                            }

                            //TODO implement foreach 
                            for (i = 0; i < dataStoreInfo.NumberOfFields; i++)
                            {
                                fieldId[i] = i + 1;

                                IComparable externalValue = typedFieldValues[i];
                                long internalValue;
                                bool isMissing = this.HandleMissingData && String.Equals(fields[i], this.MissingValue);

                                if (this.ReferenceDataStoreInfo != null)
                                {
                                    DataFieldInfo localFieldInfo = this.ReferenceDataStoreInfo.GetFieldInfo(fieldId[i]);
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
            }
            catch (Exception e)
            {
                throw new InvalidProgramException(String.Format("Error in line {0}, field {1}, exception message was: {2}", linenum, i + 1, e.Message));
            }
        }

        protected virtual void AnalyzeHeader(StreamReader streamReader)
        {
            int i = 0;
            while (i < this.header && streamReader.ReadLine() != null)
            { 
                i++; 
            }
        }

        protected virtual void SkipHeader(StreamReader streamReader)
        {
            int i = 0;
            while (i < this.header && streamReader.ReadLine() != null)
            {
                i++;
            }
        }

        protected virtual void AnalyzeLine(string line, int lineNum)
        {
            string[] fields = line.Split(fieldDelimiter, StringSplitOptions.RemoveEmptyEntries);
            if (lineNum == 1 && this.expectedColums == -1)
            {
                this.DecodeFileInfo(line);
            }
            else if (!this.CheckNumberOfColumns(fields.Length))
            {
                throw new System.MissingFieldException(String.Format(CultureInfo.InvariantCulture,
                                                                     "Wrong fields number in row {0} (Was: {1} Expected: {2}",
                                                                     lineNum,
                                                                     fields.Length,
                                                                     this.ExpectedColumns));
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
                }
            }
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

        private void AddValue2TypePool(int fieldIndex, string value)
        {            
            Type type = InfovisionHelper.String2Type(value);
            Type previousType = null;
            typePool.TryGetValue(fieldIndex, out previousType);

            if (type == previousType)
            {
                return;
            }

            if (previousType == null)
            {
                this.typePool[fieldIndex] = type;
                return;
            }

            if (type == typeof(string)
                || previousType == typeof(string))
            {
                this.typePool[fieldIndex] = typeof(string);
                return;
            }

            if (type == typeof(decimal) || type == typeof(double))
            {
                this.typePool[fieldIndex] = type;
                return;
            }

            return;
        }

        #endregion
    }

    public class DataReaderFileRses : DataReaderFileCsv
    {   
        public DataReaderFileRses(string filePath)
            : this(filePath, 1)
        {
        }

        public DataReaderFileRses(string filePath, int header)
            : base(filePath, 1)
        {
        }

        protected override void AnalyzeHeader(StreamReader streamReader)
        {
            string line = streamReader.ReadLine();
            if(! String.IsNullOrEmpty(line))
            {
                this.DecodeFileInfo(line);
            }
        }

        protected override void DecodeFileInfo(string fileInfo)
        {
            string[] fields = fileInfo.Split(this.GetFieldDelimiter(), StringSplitOptions.RemoveEmptyEntries);

            int tmp;
            if (!Int32.TryParse(fields[1], out tmp))
                throw new InvalidDataException("FileFormat error");
            this.ExpectedColumns = tmp;

            if (!Int32.TryParse(fields[0], out tmp))
                throw new InvalidDataException("FileFormat error");
            this.ExpectedRows = tmp;            

            this.DecisionId = this.ExpectedColumns;
        }
    }

    public class DataReaderFileRses11 : DataReaderFileRses
    {
        public DataReaderFileRses11(string filePath)
            : base(filePath, 1)
        {
        }        

        protected override void DecodeFileInfo(string fileInfo)
        {
            string[] fields = fileInfo.Split(this.GetFieldDelimiter(), StringSplitOptions.RemoveEmptyEntries);

            base.DecodeFileInfo(fileInfo);

            this.DecisionId = Int32.Parse(fields[2], CultureInfo.InvariantCulture);                        
        }
    }
}
