﻿// 
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
using System.Linq;
using NRough.Data;
using NUnit.Framework;
using NRough.Core;
using System.Collections.Generic;
using NRough.MachineLearning.Roughsets;
using NRough.Core.CollectionExtensions;

namespace NRough.Tests.MachineLearning.Roughsets
{
    [TestFixture]
    public class ReductCacheTest
    {
        private DataStore dataStoreTrain = null;
        private DataStore dataStoreTest = null;

        private DataStoreInfo dataStoreTrainInfo = null;

        public ReductCacheTest()
        {
            string trainFileName = @"Data\monks-1.train";
            string testFileName = @"Data\monks-1.test";

            dataStoreTrain = DataStore.Load(trainFileName, DataFormat.RSES1);
            dataStoreTest = DataStore.Load(testFileName, DataFormat.RSES1, dataStoreTrain.DataStoreInfo);

            dataStoreTrainInfo = dataStoreTrain.DataStoreInfo;
        }

        [Test]
        public void CacheReductFirstFoundTrue()
        {
            HashSet<int> attributeSet = new HashSet<int>(new int[] { 1, 2, 3 });
            string key = "#$#$#$" + attributeSet.ToArray().ToStr(" ");
            ReductCache.Instance.Set(key, new ReductCacheInfo(true, 10));

            ReductCacheInfo reductInfo = ReductCache.Instance.Get(key, null) as ReductCacheInfo;
            Assert.NotNull(reductInfo);

            Assert.AreEqual(NoYesUnknown.Yes, reductInfo.CheckIsReduct(10));
            Assert.AreEqual(NoYesUnknown.Yes, reductInfo.CheckIsReduct(11));
            Assert.AreEqual(NoYesUnknown.Unknown, reductInfo.CheckIsReduct(9));

            reductInfo.SetApproximationRanges(true, 9);
            reductInfo.SetApproximationRanges(false, 8);

            reductInfo = ReductCache.Instance.Get(key, null) as ReductCacheInfo;

            Assert.AreEqual(NoYesUnknown.Yes, reductInfo.CheckIsReduct(10));
            Assert.AreEqual(NoYesUnknown.Yes, reductInfo.CheckIsReduct(11));
            Assert.AreEqual(NoYesUnknown.Yes, reductInfo.CheckIsReduct(9));
            Assert.AreEqual(NoYesUnknown.No, reductInfo.CheckIsReduct(8));
            Assert.AreEqual(NoYesUnknown.No, reductInfo.CheckIsReduct(7));
            Assert.AreEqual(NoYesUnknown.Yes, reductInfo.CheckIsReduct(12));
            Assert.AreEqual(NoYesUnknown.Yes, reductInfo.CheckIsReduct(20));
            Assert.AreEqual(NoYesUnknown.Yes, reductInfo.CheckIsReduct(30));
            Assert.AreEqual(NoYesUnknown.Yes, reductInfo.CheckIsReduct(99));
        }

        [Test]
        public void CacheReductFirstFoundFalse()
        {
            HashSet<int> attributeSet = new HashSet<int>(new int[] { 1, 2, 3 });
            string key = "#$#$#$" + attributeSet.ToArray().ToStr(" ");
            ReductCache.Instance.Set(key, new ReductCacheInfo(false, 8), null);

            ReductCacheInfo reductInfo = ReductCache.Instance.Get(key, null) as ReductCacheInfo;
            Assert.NotNull(reductInfo);

            Assert.AreEqual(NoYesUnknown.No, reductInfo.CheckIsReduct(8));
            Assert.AreEqual(NoYesUnknown.Unknown, reductInfo.CheckIsReduct(10));
            Assert.AreEqual(NoYesUnknown.Unknown, reductInfo.CheckIsReduct(11));
            Assert.AreEqual(NoYesUnknown.Unknown, reductInfo.CheckIsReduct(9));
            Assert.AreEqual(NoYesUnknown.No, reductInfo.CheckIsReduct(7));

            reductInfo.SetApproximationRanges(false, 6);
            reductInfo.SetApproximationRanges(false, 5);

            reductInfo = ReductCache.Instance.Get(key, null) as ReductCacheInfo;

            Assert.AreEqual(NoYesUnknown.No, reductInfo.CheckIsReduct(5));
            Assert.AreEqual(NoYesUnknown.No, reductInfo.CheckIsReduct(6));
            Assert.AreEqual(NoYesUnknown.No, reductInfo.CheckIsReduct(7));
        }

        [Test]
        public void EpsilonRangeException()
        {
            HashSet<int> attributeSet = new HashSet<int>(new int[] { 1, 2, 3 });
            string key = "#$#$#$" + attributeSet.ToArray().ToString();
            ReductCache.Instance.Set(key, new ReductCacheInfo(false, 8), null);

            ReductCacheInfo reductInfo = ReductCache.Instance.Get(key, null) as ReductCacheInfo;

            reductInfo.SetApproximationRanges(true, 9);
            reductInfo.SetApproximationRanges(true, 10);
            reductInfo.SetApproximationRanges(true, 11);
            reductInfo.SetApproximationRanges(true, 12);
            reductInfo.SetApproximationRanges(true, 20);
            reductInfo.SetApproximationRanges(true, 50);

            Assert.Throws<InvalidOperationException>(() => reductInfo.SetApproximationRanges(false, 60));
        }
    }
}