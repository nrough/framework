using NUnit.Framework;

namespace NRough.Data.Tests
{
    [TestFixture]
    public class DataSamplerTest
    {
        [Test]
        public void GetDataTest()
        {
            DataStore data = DataStore.Load(@"Data\dna_modified.trn", DataFormat.RSES1);
            DataSampler sampler = new DataSampler(data);
            DataStore subData = null;
            for (int i = 0; i < 10; i++)
            {
                subData = sampler.GetData(i).Item1;
                foreach (var field in subData.DataStoreInfo.Fields)
                {
                    Assert.AreNotEqual(
                        field.Histogram,
                        data.DataStoreInfo.GetFieldInfo(field.Id).Histogram);
                }
            }
        }
    }
}