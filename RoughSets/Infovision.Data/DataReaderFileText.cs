using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Infovision.Utils;

namespace Infovision.Data
{
    public class DataReaderFileCsv : DataReaderFile
    {
        #region Globals
        
        private Char[] fieldDelimiter = new Char[] { ' ', ';', '\t' };
        private int header = 0;

        private int expectedColums = -1;
        private int expectedRows = -1;

        private Dictionary<int, Type> typePool = new Dictionary<int, Type>();

        #endregion

        #region Constructors

        public DataReaderFileCsv(string filePath)
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

        #region Methods

        public char[] GetFieldDelimiter()
        {
            return this.fieldDelimiter;
        }

        public override DataStoreInfo Analyze()
        {
            DataStoreInfo dataStoreInfo = new DataStoreInfo();
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

                dataStoreInfo.AddFieldInfo(new DataFieldInfo(i, this.AttributeType(i-1)), fieldType);
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

        public override void Load(DataStoreInfo dataStoreInfo, DataStore dataStore, DataStoreInfo referenceDataStoreInfo)
        {
            int numberOfFields = referenceDataStoreInfo != null ? referenceDataStoreInfo.NumberOfFields : dataStoreInfo.NumberOfFields;
            int[] fieldId = new int[numberOfFields];
            Int64[] fieldValue = new Int64[numberOfFields];
            string line = String.Empty;

            using (FileStream fileStream = new FileStream(this.FileName, FileMode.Open, FileAccess.Read))
            {
                using (StreamReader streamReader = new StreamReader(fileStream))
                {
                    this.SkipHeader(streamReader);        
                    while ((line = streamReader.ReadLine()) != null)
                    {
                        string[] fields = line.Split(fieldDelimiter, StringSplitOptions.RemoveEmptyEntries);

                        if (fields.Length != dataStoreInfo.NumberOfFields)
                            throw new System.InvalidOperationException();

                        object[] typedFieldValues = new Object[fields.Length];

                        Dictionary<Type, int> typeDict = new Dictionary<Type, int>
                        {
                            {typeof(int),  0},
                            {typeof(double), 1},
                            {typeof(string), 2}
                        };

                        for (int i = 0; i < fields.Length; i++)
                        {
                            switch (typeDict[dataStoreInfo.GetFieldInfo(i + 1).FieldValueType])
                            {
                                case 0:
                                    typedFieldValues[i] = Int32.Parse(fields[i], CultureInfo.InvariantCulture);
                                    break;

                                case 1:
                                    typedFieldValues[i] = Double.Parse(fields[i], CultureInfo.InvariantCulture);
                                    break;

                                case 2:
                                    typedFieldValues[i] = fields[i].Clone();
                                    break;

                                default:
                                    throw new System.InvalidOperationException();
                            }
                        }

                        for (int col = 0; col < dataStoreInfo.NumberOfFields; col++)
                        {
                            fieldId[col] = col + 1;

                            Int64 internalValue;
                            if (referenceDataStoreInfo != null)
                            {
                                internalValue = referenceDataStoreInfo.AddFieldValue(fieldId[col], typedFieldValues[col]);
                                dataStoreInfo.AddFieldInternalValue(fieldId[col], internalValue, typedFieldValues[col]);
                            }
                            else
                            {
                                internalValue = dataStoreInfo.AddFieldValue(fieldId[col], typedFieldValues[col]);
                            }

                            fieldValue[col] = internalValue;
                        }

                        dataStore.Insert(new DataRecordInternal(fieldId, fieldValue));
                    }
                }
            }

            dataStore.PostLoad();
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
                                                                     "Wrong fields number in row {0}",
                                                                     lineNum));
            }

            for (int i = 0; i < fields.Length; i++)
            {
                string value = fields[i];
                this.AddValue2TypePool(i, value);
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

            if (type == typeof(double))
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
            this.ExpectedColumns = Int32.Parse(fields[1], CultureInfo.InvariantCulture);
            this.ExpectedRows = Int32.Parse(fields[0], CultureInfo.InvariantCulture);
            this.DecisionId = this.ExpectedColumns;
        }
    }
}
