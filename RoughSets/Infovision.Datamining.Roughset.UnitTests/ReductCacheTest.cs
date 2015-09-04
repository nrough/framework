﻿using System;
using System.Linq;
using Infovision.Data;
using NUnit.Framework;

namespace Infovision.Datamining.Roughset.UnitTests
{
    [TestFixture]
    public class ReductCacheTest
    {
        DataStore dataStoreTrain = null;
        DataStore dataStoreTest = null;

        DataStoreInfo dataStoreTrainInfo = null;

        public ReductCacheTest()
        {
            string trainFileName = @"Data\monks-1.train";
            string testFileName = @"Data\monks-1.test";

            dataStoreTrain = DataStore.Load(trainFileName, FileFormat.Rses1);
            dataStoreTest = DataStore.Load(testFileName, FileFormat.Rses1, dataStoreTrain.DataStoreInfo);

            dataStoreTrainInfo = dataStoreTrain.DataStoreInfo;
        }               

        [Test]
        public void CacheReductFirstFoundTrue()
        {
            FieldSet attributeSet = new FieldSet(dataStoreTrainInfo, new int[] { 1, 2, 3 });
            string key = "#$#$#$" + attributeSet.ToString();
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
            FieldSet attributeSet = new FieldSet(dataStoreTrainInfo, new int[] { 1, 2, 3 });
            string key = "#$#$#$" + attributeSet.ToString();
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

        [Test, ExpectedException("System.InvalidOperationException")]
        public void EpsilonRangeException()
        {
            FieldSet attributeSet = new FieldSet(dataStoreTrainInfo, new int[] { 1, 2, 3 });
            string key = "#$#$#$" + attributeSet.ToString();
            ReductCache.Instance.Set(key, new ReductCacheInfo(false, 8), null);

            ReductCacheInfo reductInfo = ReductCache.Instance.Get(key, null) as ReductCacheInfo;

            reductInfo.SetApproximationRanges(true, 9);
            reductInfo.SetApproximationRanges(true, 10);
            reductInfo.SetApproximationRanges(true, 11);
            reductInfo.SetApproximationRanges(true, 12);
            reductInfo.SetApproximationRanges(true, 20);
            reductInfo.SetApproximationRanges(true, 50);

            //Should throw exception
            reductInfo.SetApproximationRanges(false, 60);
        }
    }
}
