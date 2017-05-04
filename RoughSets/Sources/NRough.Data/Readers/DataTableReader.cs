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
