using System;
using System.IO;

namespace Raccoon.Data
{
    public abstract class DataReaderFile : IDataReader
    {
        private string fileName = String.Empty;
        private int decisionId = -1;

        public string FileName
        {
            get { return this.fileName; }
            set { this.fileName = value; }
        }

        public string DataName
        {
            get { return Path.GetFileName(this.FileName); }
        }

        public int DecisionId
        {
            get { return this.decisionId; }
            set { this.decisionId = value; }
        }

        public bool HandleMissingData { get; set; }
        public string MissingValue { get; set; }
        public DataStoreInfo ReferenceDataStoreInfo { get; set; }

        public DataReaderFile()
        {
            this.MissingValue = "?";
            this.HandleMissingData = true;
        }

        public static IDataReader Construct(FileFormat fileFormat, string fileName)
        {
            IDataReader dataReader;
            switch (fileFormat)
            {
                case FileFormat.Csv:
                    dataReader = new DataReaderFileCsv(fileName);
                    break;

                case FileFormat.Rses1:
                    dataReader = new DataReaderFileRses(fileName);
                    break;

                case FileFormat.Rses1_1:
                    dataReader = new DataReaderFileRses11(fileName);
                    break;

                default:
                    throw new System.NotImplementedException("");
            }

            dataReader.HandleMissingData = true;
            dataReader.MissingValue = "?";
            return dataReader;
        }

        public abstract DataStoreInfo Analyze();

        public abstract void Load(DataStore dataStore, DataStoreInfo dataStoreInfo);
    }
}