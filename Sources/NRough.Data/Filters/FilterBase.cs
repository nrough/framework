using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NRough.Data.Filters
{
    public abstract class FilterBase : IFilter, ICloneable
    {
        public bool Enabled { get; set; }

        public FilterBase()
        {
            Enabled = true;
        }

        public abstract DataStore Apply(DataStore data);
        public virtual void Compute(DataStore data) { }

        public object Clone()
        {
            var clone = (FilterBase)this.MemberwiseClone();
            this.HandleCloned(clone);
            return clone;
        }

        protected virtual void HandleCloned(FilterBase clone)
        {
        }
    }
}
