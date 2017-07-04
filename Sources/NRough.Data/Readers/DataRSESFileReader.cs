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
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NRough.Data.Readers
{
    public class DataRSESFileReader : DataCSVFileReader
    {
        public DataRSESFileReader(string fileName)
            : base(fileName, 1)
        {
        }        

        public DataRSESFileReader(Stream stream)
            : base(stream)
        {
            Header = 1;
        }

        protected override void AnalyzeHeader(StreamReader streamReader)
        {
            string line = streamReader.ReadLine();
            if (!String.IsNullOrEmpty(line))
                this.DecodeFileInfo(line);
        }        

        protected override void DecodeFileInfo(string fileInfo)
        {
            string[] fields = fileInfo.Split(this.GetFieldDelimiter(), StringSplitOptions.RemoveEmptyEntries);

            int tmp;
            if (!Int32.TryParse(fields[1], out tmp))
                throw new InvalidDataException("FileFormat error");
            ExpectedColumns = tmp;

            if (!Int32.TryParse(fields[0], out tmp))
                throw new InvalidDataException("FileFormat error");
            ExpectedRows = tmp;

            if (this.ReferenceDataStoreInfo != null && this.ReferenceDataStoreInfo.DecisionFieldId > 0)
                this.DecisionId = this.ReferenceDataStoreInfo.DecisionFieldId;
            else
                this.DecisionId = this.ExpectedColumns;
        }
    }

    public class DataRSES11FileReader : DataRSESFileReader
    {
        public DataRSES11FileReader(string filePath)
            : base(filePath)
        {
        }

        public DataRSES11FileReader(Stream stream)
            : base(stream)
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
