using NRough.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NRough.MachineLearning.Roughsets
{
    public class ReductWithExceptions : Reduct
    {
        public IList<IReduct> Exceptions { get; set; }

        public ReductWithExceptions(DataStore dataStore, IEnumerable<int> fieldIds, double epsilon, double[] weights)
            : base(dataStore, fieldIds, epsilon, weights)
        {            
        }
    }
}
