using System;
using System.IO;

namespace NRough.Data.Readers
{
    public abstract class DataFileReader : DataStreamReader, IDisposable
    {
        private FileStream filestream;
        private bool isDisposed;
        private bool isStreamOwner;

        public string FileName { get; set; }        

        public DataFileReader(string fileName)
            : base()
        {
            DataName = Path.GetFileName(fileName);
            FileName = fileName;
            isStreamOwner = true;
            Stream = new FileStream(this.FileName, FileMode.Open, FileAccess.Read);            
        }

        public DataFileReader(Stream stream)
            : base(stream)
        {
            FileName = null;
            isStreamOwner = false;
        }

        public static IDataReader Construct(DataFormat fileFormat, string fileName)
        {
            IDataReader dataReader;
            switch (fileFormat)
            {
                case DataFormat.CSV:
                    dataReader = new DataCSVFileReader(fileName);
                    break;

                case DataFormat.RSES1:
                    dataReader = new DataRSESFileReader(fileName);
                    break;

                case DataFormat.RSES1_1:
                    dataReader = new DataRSES11FileReader(fileName);
                    break;

                default:
                    throw new System.NotImplementedException("");
            }
            
            return dataReader;
        }        

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!isDisposed)
            {
                if (disposing)
                    filestream.Dispose();
                isDisposed = true;
            }
        }
    }
}