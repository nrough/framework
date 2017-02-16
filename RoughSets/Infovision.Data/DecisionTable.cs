using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Raccoon.Data
{
    public class TmpDecisionTable<T>
        where T : IDataStore
    {
        IDataStore data;

        public TmpDecisionTable(T dataStore)
        {
            data = dataStore;
        }
    }
}
