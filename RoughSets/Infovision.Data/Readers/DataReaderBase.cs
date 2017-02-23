using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NRough.Data.Readers
{
    public abstract class DataReaderBase : IDataReader
    {
        public string DataName { get; set; }
        public int DecisionId { get; set; }
        public int ExpectedColumns { get; set; }
        public int ExpectedRows { get; set; }
        public bool HandleMissingData { get; set; }
        public string MissingValue { get; set; }        
        public DataStoreInfo ReferenceDataStoreInfo { get; set; }                

        public DataReaderBase()
        {
            DataName = String.Empty;
            DecisionId = -1;
            ExpectedColumns = -1;
            ExpectedRows = -1;
            HandleMissingData = true;
            MissingValue = "?";
            ReferenceDataStoreInfo = null;
        }
        

        public abstract DataStore Read();
    }
}
