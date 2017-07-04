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
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NRough.Data.Readers
{
    public class DataTableReader : DataReaderBase
    {
        public DataTable DataTable { get; set; }

        private DataTableReader()
        {
        }

        public DataTableReader(DataTable dataTable)
            : this()
        {
            DataTable = dataTable;
        }

        public override DataStore Read()
        {
            int columns = DataTable.Columns.Count;
            int rows = DataTable.Rows.Count;

            var dataStoreInfo = new DataStoreInfo(columns);
            dataStoreInfo.NumberOfRecords = rows;

            for (int i = 1; i <= DataTable.Columns.Count; i++)
            {
                var column = DataTable.Columns[i - 1];
                var attributeInfo = new AttributeInfo(i, column.DataType);
                attributeInfo.Alias = column.ColumnName;
                attributeInfo.Name = column.ColumnName;
                attributeInfo.IsUnique = column.Unique;
                attributeInfo.IsIdentifier = column.AutoIncrement;
                attributeInfo.IsDecision = (i == DataTable.Columns.Count);
                attributeInfo.IsStandard = !(attributeInfo.IsDecision || column.Unique);
                attributeInfo.MissingValue = null;

                dataStoreInfo.AddFieldInfo(attributeInfo);
            }

            //TODO referenceDataStoreInfo
            //TODO coding internal values

            var dataStore = new DataStore(dataStoreInfo);
            //TODO add recrds

            return dataStore;
        }
    }
}
