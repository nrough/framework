using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infovision.Data
{
    [Serializable]
    public class DataStoreOrderByComparer : Comparer<int>
    {
        private DataStore dataStore;
        private int[] orderBy;

        public DataStoreOrderByComparer(DataStore dataStore, int[] orderBy)
            : base()
        {
            this.dataStore = dataStore;
            this.orderBy = orderBy;
        }

        public override int Compare(int x, int y)
        {
            int result;            
            for (int i = 0; i < orderBy.Length; i++)
            {
                result = dataStore.GetFieldValue(x, orderBy[i]).CompareTo(dataStore.GetFieldValue(y, orderBy[i]));
                if (result != 0)                                  
                    return result;                
            }            

            return 0;
        }
    }
}
