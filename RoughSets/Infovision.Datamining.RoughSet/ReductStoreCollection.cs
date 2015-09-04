using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infovision.Datamining.Roughset
{
    public interface IReductStoreCollection : IEnumerable<IReductStore>
    {
    }

    public class ReductStoreCollection : IReductStoreCollection
    {
        List<IReductStore> stores;

        public ReductStoreCollection()
    }
}
