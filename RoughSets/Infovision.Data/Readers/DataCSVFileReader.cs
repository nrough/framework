using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using NRough.Core;

namespace NRough.Data.Readers
{
    public class DataCSVFileReader : DataFileReader
    {
        public DataCSVFileReader(string fileName)
            : base(fileName)
        {
        }

        public DataCSVFileReader(string fileName, int header)
            : this(fileName)
        {
            Header = header;
        }

        public DataCSVFileReader(string fileName, int header, char[] fieldDelimiter)
            : this(fileName, header)
        {
            FieldDelimiter = fieldDelimiter;
        }

        public DataCSVFileReader(Stream stream)
            : base(stream)
        {
        }
    }    
}