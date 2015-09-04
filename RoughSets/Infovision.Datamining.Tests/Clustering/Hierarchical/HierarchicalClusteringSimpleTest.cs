using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Infovision.Datamining.Clustering.Hierarchical;
using NUnit.Framework;
using Infovision.Math;

namespace Infovision.Datamining.Tests.Clustering.Hierarchical
{
    [TestFixture]
    public class HierarchicalClusteringSimpleTest
    {
        [Test]
        public void ComputeTest()
        {
            HierarchicalClusteringSimple hClustering = new HierarchicalClusteringSimple(Accord.Math.Distance.Euclidean, ClusteringLinkage.Min);
            hClustering.Compute(HierarchicalClusteringTest.GetData());

            Assert.IsTrue(true);
        }
    }
}
