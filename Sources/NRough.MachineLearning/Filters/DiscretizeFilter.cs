// 
// NRough Framework
// http://www.nrough.net
// 
// Copyright (c) Sebastian Widz, 2017
// authors at nrough.net
// 
// This file is part of NRough Framework.
// 
// NRough Framework is free software; you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation; either version 3 of the License, or
// (at your option) any later version.
// 
// NRough Framework is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with NRough Framework; if not, write to the Free Software
// Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA  02110-1301  USA
//

using NRough.Core;
using NRough.Data;
using NRough.MachineLearning.Discretization;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using NRough.Core.CollectionExtensions;
using NRough.Data.Filters;

namespace NRough.MachineLearning.Filters
{
    [Serializable]
    public class DiscretizeFilter : FilterBase
    {        
        private object syncRoot = new object();
        private Dictionary<string, DataStore> cache;
        public DecisionTableDiscretizer TableDiscretizer { get; set; }       

        public DiscretizeFilter()
            : base()
        {
            cache = new Dictionary<string, DataStore>();
            this.TableDiscretizer = null;
        }

        public override void Compute(DataStore data)
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

                if (TableDiscretizer == null)
                    TableDiscretizer = new DecisionTableDiscretizer();

                discretizedData = (DataStore)data.Clone();
                TableDiscretizer.Discretize(discretizedData, discretizedData.Weights);

                cache.Add(GetCacheKey(data), discretizedData);

                TraceData(discretizedData, false);
            }
        }

        public override DataStore Apply(DataStore data)
        {
            TraceData(data, true);

            DataStore discretizedData = null;
            if (!cache.TryGetValue(GetCacheKey(data), out discretizedData))
                throw new InvalidOperationException("cannot find discretized data set");

            var dataToDiscretize = (DataStore)data.Clone();
            DecisionTableDiscretizer.Discretize(dataToDiscretize, discretizedData);

            TraceData(dataToDiscretize, false);

            return dataToDiscretize;
        }

        public IDiscretizer GetAttributeDiscretizer(int attributeId)
        {
            IDiscretizer result;
            if (TableDiscretizer.FieldDiscretizer.TryGetValue(attributeId, out result))
                return result;
            return null;
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
            Trace.WriteLine(data.DataStoreInfo.SelectAttributeIds().ToArray().ToStr());
        }
    }
}
