using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Raccoon.Data
{
    public class DecisionTable<T>
        where T : IDataStore
    {
        IDataStore data;

        public DecisionTable(T dataStore)
        {
            data = dataStore;
        }
    }
}
