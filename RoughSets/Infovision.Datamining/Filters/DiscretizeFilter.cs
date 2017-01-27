using Raccoon.Data;
using Raccoon.MachineLearning.Discretization;
using System;
using System.Collections.Generic;

namespace Raccoon.MachineLearning.Filters
{
    [Serializable]
    public class DiscretizeFilter : IFilter
    {        
        private object syncRoot = new object();
        private Dictionary<string, DataStore> cache;
        public DataStoreDiscretizer DataStoreDiscretizer { get; set; }

        public DiscretizeFilter()
        {
            cache = new Dictionary<string, DataStore>();
            this.DataStoreDiscretizer = null;
        }

        public void Compute(DataStore data)
        {
            lock (syncRoot)
            {
                DataStore discretizedData = null;
                if (cache.TryGetValue(GetCacheKey(data), out discretizedData))
                    return;

                if (DataStoreDiscretizer == null)
                    DataStoreDiscretizer = new DataStoreDiscretizer();

                discretizedData = (DataStore)data.Clone();
                DataStoreDiscretizer.Discretize(discretizedData, discretizedData.Weights);

                cache.Add(GetCacheKey(data), discretizedData);
            }
        }

        public DataStore Apply(DataStore data)
        {
            DataStore discretizedData = null;
            if (!cache.TryGetValue(GetCacheKey(data), out discretizedData))
                throw new InvalidOperationException("cannot find discretized data set");

            var dataToDiscretize = (DataStore)data.Clone();
            DataStoreDiscretizer.Discretize(dataToDiscretize, discretizedData);
            return dataToDiscretize;
        }

        private string GetCacheKey(DataStore data)
        {
            return String.Format("{0}.{1}", data.TableId, data.Fold);
        }
    }
}
