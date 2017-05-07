using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NRough.Data
{
    public class TmpDecisionTable<T>
        where T : IDataStorage
    {
        IDataStorage data;

        public TmpDecisionTable(T dataStore)
        {
            data = dataStore;
        }
    }
}
