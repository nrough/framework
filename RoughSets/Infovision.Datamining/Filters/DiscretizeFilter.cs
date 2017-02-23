using NRough.Core;
using NRough.Data;
using NRough.MachineLearning.Discretization;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;

namespace NRough.MachineLearning.Filters
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
            TraceData(data, true);

            lock (syncRoot)
            {
                DataStore discretizedData = null;
                if (cache.TryGetValue(GetCacheKey(data), out discretizedData))
                {
                    TraceData(discretizedData, false);
                    return;
                }

                if (DataStoreDiscretizer == null)
                    DataStoreDiscretizer = new DataStoreDiscretizer();

                discretizedData = (DataStore)data.Clone();
                DataStoreDiscretizer.Discretize(discretizedData, discretizedData.Weights);

                cache.Add(GetCacheKey(data), discretizedData);

                TraceData(discretizedData, false);
            }
        }

        public DataStore Apply(DataStore data)
        {
            TraceData(data, true);

            DataStore discretizedData = null;
            if (!cache.TryGetValue(GetCacheKey(data), out discretizedData))
                throw new InvalidOperationException("cannot find discretized data set");

            var dataToDiscretize = (DataStore)data.Clone();
            DataStoreDiscretizer.Discretize(dataToDiscretize, discretizedData);

            TraceData(dataToDiscretize, false);

            return dataToDiscretize;
        }

        private string GetCacheKey(DataStore data)
        {
            return String.Format("{0}.{1}", data.TableId, data.Fold);
        }

        [Conditional("DEBUG")]
        private void TraceData(DataStore data, bool beforDisc)
        {
            Trace.WriteLine(beforDisc ? "Before discretization" : "After discretization");
            Trace.WriteLine(String.Format("Name : {0}", data.Name));
            Trace.WriteLine(String.Format("Fold : {0}", data.Fold.ToString()));
            Trace.WriteLine(String.Format("Id : {0}", data.TableId.ToString()));
            Trace.WriteLine(String.Format("Type : {0}", data.DatasetType.ToSymbol()));
            Trace.WriteLine(data.DataStoreInfo.GetFieldIds().ToArray().ToStr());
        }
    }
}
