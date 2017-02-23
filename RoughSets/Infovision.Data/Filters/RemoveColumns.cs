using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NRough.Data.Filters
{
    public class RemoveColumns : FilterBase
    {
        private IEnumerable<int> toRemove;

        public RemoveColumns(IEnumerable<int> columnsToRemove)
        {
            if (columnsToRemove == null) throw new ArgumentNullException("columnsToKeep");
            toRemove = columnsToRemove.ToArray();
        }        

        public override DataStore Apply(DataStore data)
        {
            if (data == null) throw new ArgumentNullException("data");
            var res = (DataStore)data.Clone();
            res.RemoveColumn(toRemove);
            return res;
        }
    }
}
