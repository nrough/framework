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

using System.Runtime.Caching;

namespace NRough.MachineLearning.Roughsets
{
    public class ReductCache : MemoryCache
    {
        private static ReductCache reductCacheInstance = null;
        private static readonly object syncRoot = new object();
        private static CacheItemPolicy defaultCacheItemPolicy = null;

        private ReductCache(string name)
            : base(name)
        {
        }

        public static ReductCache Instance
        {
            get
            {
                if (reductCacheInstance == null)
                {
                    lock (syncRoot)
                    {
                        if (reductCacheInstance == null)
                        {
                            reductCacheInstance = new ReductCache("ReductCache");
                        }
                    }
                }

                return reductCacheInstance;
            }
        }

        private static CacheItemPolicy DefaultCacheItemPolicy
        {
            get
            {
                if (defaultCacheItemPolicy == null)
                {
                    lock (syncRoot)
                    {
                        if (defaultCacheItemPolicy == null)
                        {
                            defaultCacheItemPolicy = new CacheItemPolicy
                            {
                                AbsoluteExpiration = ObjectCache.InfiniteAbsoluteExpiration,
                                SlidingExpiration = ObjectCache.NoSlidingExpiration
                                //Priority = CacheItemPriority.NotRemovable
                            };
                        }
                    }
                }

                return defaultCacheItemPolicy;
            }
        }

        /*
        public override void Set(CacheItem item, CacheItemPolicy policy)
        {
            this.Set(item.Key, item.Value, policy, item.RegionName);
        }
        */

        /*
        public override void Set(string key, object key, DateTimeOffset absoluteExpiration, string regionName = null)
        {
            Set(key, key, new CacheItemPolicy { AbsoluteExpiration = absoluteExpiration }, regionName);
        }
        */

        /*
        public override void Set(string key, object key, CacheItemPolicy policy, string regionName)
        {
            base.Set(ReductCache.CreateKeyWithRegion(key, regionName), key, policy);
        }
        */

        public virtual void Set(string key, object value, string regionName = null)
        {
            this.Set(key, value, ReductCache.DefaultCacheItemPolicy, regionName);
        }

        /*
        public override CacheItem GetCacheItem(string key, string regionName = null)
        {
            CacheItem temporary = base.GetCacheItem(ReductCache.CreateKeyWithRegion(key, regionName));
            return new CacheItem(key, temporary.Value, regionName);
        }
        */

        /*
        >>>>
        public override object Get(string key, string regionName = null)
        {
            return base.Get(ReductCache.CreateKeyWithRegion(key, regionName));
        }
        <<<<
        */

        /*
        public override DefaultCacheCapabilities DefaultCacheCapabilities
        {
            get
            {
                // >>>> return (base.DefaultCacheCapabilities | System.Runtime.Caching.DefaultCacheCapabilities.CacheRegions);
                return (base.DefaultCacheCapabilities);
            }
        }
        */

        /*
        static string CreateKeyWithRegion(string key, string region)
        {
            return (region == null ? "region=DEFAULT;" : "region=" + region + ";") + key;
        }
        */
    }
}