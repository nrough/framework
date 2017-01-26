using Raccoon.Data;
using Raccoon.MachineLearning.Discretization;
using System;

namespace Raccoon.MachineLearning.Filters
{
    public class DiscretizeFilter : IFilter
    {
        public DataStoreDiscretizer DataStoreDiscretizer { get; set; }

        public DiscretizeFilter()
        {
        }

        public DataStore Apply(DataStore data)
        {
            throw new NotImplementedException();
        }
    }
}
