﻿using Raccoon.Data;
using Raccoon.MachineLearning.Clustering;
using Raccoon.Math;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Raccoon.MachineLearning.Tests.Clustering
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
            var data = DataStore.Load(@"data\german.data", FileFormat.CSV);
            Assert.NotNull(data);

            KMeans kmeans = new KMeans();
            kmeans.K = 3;
            kmeans.Distance = Distance.Euclidean;
            var result = kmeans.Learn(data, data.DataStoreInfo.GetFieldIds(FieldGroup.Standard).ToArray());

            Assert.NotNull(result);
        }
    }
}
