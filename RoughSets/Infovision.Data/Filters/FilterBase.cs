using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NRough.Data.Filters
{
    public abstract class FilterBase : IFilter
    {
        public abstract DataStore Apply(DataStore data);
        public void Compute(DataStore data) { }
    }
}
