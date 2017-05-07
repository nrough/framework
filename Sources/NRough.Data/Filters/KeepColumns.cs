using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NRough.Data.Filters
{
    public class KeepColumns : FilterBase
    {
        private IEnumerable<int> toKeep;
        public KeepColumns(IEnumerable<int> columnsToKeep)
            : base()
        {
            this.toKeep = columnsToKeep.ToArray();
        }        

        public override DataStore Apply(DataStore data)
        {            
            if (data == null) throw new ArgumentNullException("data");            
            var columns = new HashSet<int>(data.DataStoreInfo.SelectAttributeIds());
            foreach (int k in toKeep)
                columns.Remove(k);
            return new RemoveColumns(columns).Apply(data);
        }
    }
}
