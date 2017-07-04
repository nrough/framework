//
// NRough Framework
// http://www.nrough.net
//
// Copyright © Sebastian Widz, 2017
// authors at nrough.net

//This file is part of NRough.

//Foobar is free software; you can redistribute it and/or modify
//it under the terms of the GNU General Public License as published by
//the Free Software Foundation; either version 3 of the License, or
//(at your option) any later version.
//
//Foobar is distributed in the hope that it will be useful,
//but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
//GNU General Public License for more details.
//
//You should have received a copy of the GNU General Public License
//along with Foobar; if not, write to the Free Software
//Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA  02110-1301  USA
//
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