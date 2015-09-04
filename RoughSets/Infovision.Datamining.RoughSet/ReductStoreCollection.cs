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
        IReductStore First();
    }

    public class ReductStoreCollection : IReductStoreCollection
    {
        List<IReductStore> stores;

        public ReductStoreCollection()
        {
            stores = new List<IReductStore>();
        }

        public void AddStore(IReductStore reductStore)
        {
            stores.Add(reductStore);
        }

        public IReductStore First()
        {
            return stores.First();
        }

        public IEnumerator<IReductStore> GetEnumerator()
        {
            return stores.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return stores.GetEnumerator();
        }
    }
}
