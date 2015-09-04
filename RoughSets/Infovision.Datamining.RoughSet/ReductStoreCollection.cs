using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infovision.Datamining.Roughset
{
    public interface IReductStoreCollection : IEnumerable<IReductStore>
    {
        void AddStore(IReductStore reductStore);
        int Count { get; }
        //List<IReductStore> ActiveModels();
    }

    public class ReductStoreCollection : IReductStoreCollection
    {
        List<IReductStore> stores;

        public int Count { get { return this.stores.Count; } }

        public ReductStoreCollection()
        {
            this.stores = new List<IReductStore>();            
        }
        
        public void AddStore(IReductStore reductStore)
        {
            this.stores.Add(reductStore);            
        }        

        public IEnumerator<IReductStore> GetEnumerator()
        {
            return stores.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return stores.GetEnumerator();
        }

        public List<IReductStore> ActiveModels()
        {
            return stores.FindAll(x => x.IsActive == true);
        }
    }
}
