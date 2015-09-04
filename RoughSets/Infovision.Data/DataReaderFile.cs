using System;
using System.IO;

namespace Infovision.Data
{
    public interface IDataReader
    {
        DataStoreInfo Analyze();
        void Load(DataStoreInfo dataStoreInfo, DataStore dataStore, DataStoreInfo referenceDataStoreInfo);
        string DataName { get; }
        int DecisionId { get; set; }
    }
    
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

        public static IDataReader Construct(FileFormat fileFormat, string fileName)
        {
            IDataReader dataReader;
            switch (fileFormat)
            {
                case FileFormat.Csv :
                    dataReader = new DataReaderFileCsv(fileName);
                    break;

                case FileFormat.Rses1 :
                    dataReader = new DataReaderFileRses(fileName);
                    break;

                default:
                    throw new System.NotImplementedException("");
            }

            return dataReader;
        }

        public abstract DataStoreInfo Analyze();
        public abstract void Load(DataStoreInfo dataStoreInfo, DataStore dataStore, DataStoreInfo referenceDataStoreInfo);
    }
}
