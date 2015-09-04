using System;
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
            string trainFileName = @"monks-1.train";
            string testFileName = @"monks-1.test";

            dataStoreTrain = DataStore.Load(trainFileName, FileFormat.Rses1);
            dataStoreTest = DataStore.Load(testFileName, FileFormat.Rses1, dataStoreTrain.DataStoreInfo);

            dataStoreTrainInfo = dataStoreTrain.DataStoreInfo;
        }
        
        [Test]
        [Ignore("We do not use RegionNames any more")]
        public void CacheRegion()
        {
            string key1 = "1#2#3#4#5#6#7#8#9#0#1#2#3#4#5#6#7#8#9#0";
            string key2 = "123#1#3#4|$$$|****";

            FieldSet attr1 = new FieldSet(dataStoreTrainInfo, new int[] { 1, 2, 3 });
            FieldSet attr2 = new FieldSet(dataStoreTrainInfo, new int[] { 1, 2, 3 });

            ReductCache.Instance.Set(key1, attr1, DateTime.MaxValue, "Region_A");
            ReductCache.Instance.Set(key1, attr1, DateTime.MaxValue, null);

            ReductCache.Instance.Set(key2, attr2, DateTime.MaxValue, "Region_B");
            ReductCache.Instance.Set(key2, attr2, DateTime.MaxValue, null);

            FieldSet attr11 = ReductCache.Instance.Get(key1, null) as FieldSet;
            Assert.AreEqual(attr11.ToArray().ElementAt<int>(0), attr1.ToArray().ElementAt<int>(0));

            FieldSet attr12 = ReductCache.Instance.Get(key1, "Region_A") as FieldSet;
            Assert.AreEqual(attr11.ToArray().ElementAt<int>(0), attr1.ToArray().ElementAt<int>(0));

            FieldSet attr21 = ReductCache.Instance.Get(key2, null) as FieldSet;
            Assert.AreEqual(attr21.ToArray().ElementAt<int>(1), attr2.ToArray().ElementAt<int>(1));

            FieldSet attr22 = ReductCache.Instance.Get(key2, "Region_B") as FieldSet;
            Assert.AreEqual(attr22.ToArray().ElementAt<int>(2), attr2.ToArray().ElementAt<int>(2));

            attr11 = ReductCache.Instance.Get(key2, null) as FieldSet;
            Assert.IsNotNull(attr11);

            attr12 = ReductCache.Instance.Get(key2, "Region_A") as FieldSet;
            Assert.IsNull(attr12);
        }

        [Test]
        public void CacheReductFirstFoundTrue()
        {
            FieldSet attributeSet = new FieldSet(dataStoreTrainInfo, new Int32[] { 1, 2, 3 });
            String key = "#$#$#$" + attributeSet.ToString();
            ReductCache.Instance.Set(key, new ReductCacheInfo(true, 0.1));

            ReductCacheInfo reductInfo = ReductCache.Instance.Get(key, null) as ReductCacheInfo;
            Assert.NotNull(reductInfo);

            Assert.AreEqual(NoYesUnknown.Yes, reductInfo.CheckIsReduct(0.1));
            Assert.AreEqual(NoYesUnknown.Yes, reductInfo.CheckIsReduct(0.11));
            Assert.AreEqual(NoYesUnknown.Unknown, reductInfo.CheckIsReduct(0.09));

            reductInfo.SetApproximationRanges(true, 0.09);
            reductInfo.SetApproximationRanges(false, 0.08);

            reductInfo = ReductCache.Instance.Get(key, null) as ReductCacheInfo;

            Assert.AreEqual(NoYesUnknown.Yes, reductInfo.CheckIsReduct(0.1));
            Assert.AreEqual(NoYesUnknown.Yes, reductInfo.CheckIsReduct(0.11));
            Assert.AreEqual(NoYesUnknown.Yes, reductInfo.CheckIsReduct(0.09));
            Assert.AreEqual(NoYesUnknown.No, reductInfo.CheckIsReduct(0.08));
            Assert.AreEqual(NoYesUnknown.No, reductInfo.CheckIsReduct(0.07));
            Assert.AreEqual(NoYesUnknown.Yes, reductInfo.CheckIsReduct(0.1101));
            Assert.AreEqual(NoYesUnknown.Yes, reductInfo.CheckIsReduct(0.12));
            Assert.AreEqual(NoYesUnknown.Yes, reductInfo.CheckIsReduct(0.20));
            Assert.AreEqual(NoYesUnknown.Yes, reductInfo.CheckIsReduct(0.30));
            Assert.AreEqual(NoYesUnknown.Yes, reductInfo.CheckIsReduct(0.99));
        }

        [Test]
        public void CacheReductFirstFoundFalse()
        {
            FieldSet attributeSet = new FieldSet(dataStoreTrainInfo, new Int32[] { 1, 2, 3 });
            string key = "#$#$#$" + attributeSet.ToString();
            ReductCache.Instance.Set(key, new ReductCacheInfo(false, 0.08), null);

            ReductCacheInfo reductInfo = ReductCache.Instance.Get(key, null) as ReductCacheInfo;
            Assert.NotNull(reductInfo);

            Assert.AreEqual(NoYesUnknown.No, reductInfo.CheckIsReduct(0.08));
            Assert.AreEqual(NoYesUnknown.Unknown, reductInfo.CheckIsReduct(0.1));
            Assert.AreEqual(NoYesUnknown.Unknown, reductInfo.CheckIsReduct(0.11));
            Assert.AreEqual(NoYesUnknown.Unknown, reductInfo.CheckIsReduct(0.09));
            Assert.AreEqual(NoYesUnknown.No, reductInfo.CheckIsReduct(0.07));

            reductInfo.SetApproximationRanges(false, 0.06);
            reductInfo.SetApproximationRanges(false, 0.05);

            reductInfo = ReductCache.Instance.Get(key, null) as ReductCacheInfo;

            Assert.AreEqual(NoYesUnknown.No, reductInfo.CheckIsReduct(0.05));
            Assert.AreEqual(NoYesUnknown.No, reductInfo.CheckIsReduct(0.06));
            Assert.AreEqual(NoYesUnknown.No, reductInfo.CheckIsReduct(0.07));
        }

        [Test, ExpectedException("System.InvalidOperationException")]
        public void EpsilonRangeException()
        {
            FieldSet attributeSet = new FieldSet(dataStoreTrainInfo, new int[] { 1, 2, 3 });
            String key = "#$#$#$" + attributeSet.ToString();
            ReductCache.Instance.Set(key, new ReductCacheInfo(false, 0.08), null);

            ReductCacheInfo reductInfo = ReductCache.Instance.Get(key, null) as ReductCacheInfo;

            reductInfo.SetApproximationRanges(true, 0.09);
            reductInfo.SetApproximationRanges(true, 0.10);
            reductInfo.SetApproximationRanges(true, 0.11);
            reductInfo.SetApproximationRanges(true, 0.12);
            reductInfo.SetApproximationRanges(true, 0.20);
            reductInfo.SetApproximationRanges(true, 0.50);

            //Should throw exception
            reductInfo.SetApproximationRanges(false, 0.60);
        }
    }
}
