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
            throw new NotImplementedException();
        }
    }
}
