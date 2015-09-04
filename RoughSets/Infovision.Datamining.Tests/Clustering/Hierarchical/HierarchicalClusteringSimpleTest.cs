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
    public class HierarcihcalClusteringSimpleTest
    {
        [Test]
        public void ComputeTest()
        {
            HierarchicalClusteringSimple hClustering = new HierarchicalClusteringSimple(Accord.Math.Distance.Euclidean, ClusteringLinkage.Min);
            hClustering.Compute(HierarchicalClusteringTest.GetData());

            Assert.IsTrue(true);
        }

        [Test]
        public void ComputeLeafNodesTest()
        {
            HierarchicalClusteringSimple hClustering = new HierarchicalClusteringSimple(Accord.Math.Distance.Euclidean, ClusteringLinkage.Min);
            hClustering.Compute(HierarchicalClusteringTest.GetData());
            int[] leaves = hClustering.Dendrogram.ComputeLeafNodes();
            
            foreach (int i in leaves)
            {
                Console.Write("{0} ", i);
            }
            Console.WriteLine();

            Assert.IsTrue(true);
        }
    }
}
