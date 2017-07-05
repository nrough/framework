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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NRough.Core;
using NRough.Core.Random;

namespace NRough.MachineLearning
{
    [Serializable]
    public class DecisionDistribution
    {
        private readonly object syncRoot = new object();

        private Dictionary<long, double> weightDistribution;
        private IDistibutionSelectStrategy selectStrategy;
        private long output;
        private bool isCached;

        public long Output
        {
            get
            {
                if (!this.isCached)
                {
                    lock (syncRoot)
                    {
                        if (!this.isCached)
                        {
                            this.output = selectStrategy.Select(this.weightDistribution);
                            this.isCached = true;
                        }
                    }
                }

                return this.output;
            }            
        }

        public double this[long decision]
        {
            get
            {
                double w = 0;
                if (this.weightDistribution.TryGetValue(decision, out w))
                    return w;
                return 0; 
            }
        }

        private DecisionDistribution()
        {
            this.isCached = false;
            this.output = -1;
        }

        public DecisionDistribution(IDictionary<long, double> distribution, IDistibutionSelectStrategy strategy)
            : this()
        {
            this.weightDistribution = new Dictionary<long, double>(distribution);
            this.selectStrategy = strategy;
        }

        public DecisionDistribution(IDictionary<long, double> distribution)
            : this(distribution, DistributionSelectMax.DefaultInstance)
        {            
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            bool first = true;
            foreach (var kvp in this.weightDistribution)
            {
                if (first)
                {
                    sb.AppendFormat("{0}:{1}", kvp.Key, kvp.Value);
                    first = false;
                }
                else
                {
                    sb.AppendFormat(" {0}:{1}", kvp.Key, kvp.Value);                    
                }
            }
            return sb.ToString();
        }
    }

    [Serializable]
    public class DistributionSelectMax : IDistibutionSelectStrategy
    {
        private static object syncRoot = new object();
        private static volatile DistributionSelectMax instance;

        private Comparer<double> comparer = Comparer<double>.Default;
        
        public static DistributionSelectMax DefaultInstance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                        {
                            instance = new DistributionSelectMax();
                        }
                    }
                }

                return instance;
            }
        }

        /// <summary>
        /// Select element with maximal weigth. In case two or more elements exists, returns a random element
        /// </summary>
        /// <param name="distribution"></param>
        /// <returns></returns>
        public long Select(IDictionary<long, double> distribution)
        {
            if (distribution == null || distribution.Count == 0)
                return -1;

            List<long> maxValues = new List<long>(distribution.Count);
                        
            KeyValuePair<long, double> result = new KeyValuePair<long, double>(-1, 0.0);
            foreach (var kvp in distribution)
                if (this.comparer.Compare(kvp.Value, result.Value) >= 0)
                {
                    if (this.comparer.Compare(kvp.Value, result.Value) > 0)
                    {
                        maxValues.Clear();
                        result = kvp;
                    }
                     
                    maxValues.Add(kvp.Key);
                }

            if (maxValues.Count > 1)
            {                
                int idx = RandomSingleton.Random.Next(maxValues.Count);
                return maxValues.ElementAt(idx);
            }

            return result.Key;
        }
    }
}
