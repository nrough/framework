using Raccoon.Data;
using Raccoon.MachineLearning.Discretization;
using System;

namespace Raccoon.MachineLearning.Filters
{
    [Serializable]
    public class DiscretizeFilter : IFilter
    {
        public DataStoreDiscretizer DataStoreDiscretizer { get; set; } = null;
        private DataStore discretizedData;

        public DiscretizeFilter()
        {
        }

        public void Compute(DataStore data)
        {
            if(DataStoreDiscretizer == null)
                DataStoreDiscretizer = new DataStoreDiscretizer();
            discretizedData = (DataStore) data.Clone();
            DataStoreDiscretizer.Discretize(discretizedData, discretizedData.Weights);
        }

        public DataStore Apply(DataStore data)
        {
            var dataToDiscretize = (DataStore) data.Clone();
            DataStoreDiscretizer.Discretize(dataToDiscretize, discretizedData);
            return dataToDiscretize;
        }
    }
}
