using NRough.Data;
using NRough.MachineLearning.Clustering;
using NRough.Math;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NRough.MachineLearning.Tests.Clustering
{
    [TestFixture]
    public class KMeansTest
    {
        [Test]
        public void ConstructorTest()
        {
            KMeans kmeans = new KMeans();
            Assert.IsNotNull(kmeans);
        }

        [Test]
        public void LearnTest()
        {
            var data = DataStore.Load(@"data\german.data", DataFormat.CSV);
            Assert.NotNull(data);

            KMeans kmeans = new KMeans();
            kmeans.K = 3;
            kmeans.Distance = Distance.Euclidean;
            var result = kmeans.Learn(data, data.DataStoreInfo.GetFieldIds(FieldGroup.Standard).ToArray());

            Assert.NotNull(result);
        }
    }
}
