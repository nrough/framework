using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Infovision.Datamining.Clustering.Hierarchical;
using Infovision.Math;

namespace Infovision.Datamining.Tests.Clustering.Hierarchical
{
    [TestFixture]
    public class HierarchicalClusteringTest
    {        
        public static double[][] GetData()
        {
            double[][] data = 
            {
                new double[] {7, 8, 0, 1, 0, 7, 1}, //0
                new double[] {6, 7, 1, 1, 1, 7, 1}, //1
                new double[] {5, 6, 0, 0, 0, 7, 1}, //2
                new double[] {4, 1, 1, 3, 1, 7, 1}, //3
                new double[] {3, 2, 0, 0, 0, 7, 1}, //4
                new double[] {2, 6, 1, 2, 0, 7, 1}, //5
                new double[] {1, 2, 0, 0, 0, 7, 1}, //6
                new double[] {0, 9, 1, 2, 1, 7, 1}, //7
                new double[] {1, 5, 0, 0, 0, 7, 1}, //8
                new double[] {1, 5, 1, 2, 0, 7, 1}  //9                
            };

            return data;
        }

        [Test]
        public void ComputeTest()
        {                                    
            HierarchicalClustering hClustering = new HierarchicalClustering(Accord.Math.Distance.Euclidean, ClusteringLinkage.Min);
            hClustering.Compute(HierarchicalClusteringTest.GetData());            
            Assert.IsTrue(true);
        }

        [Test]
        public void ComputeLeafNodesTest()
        {
            HierarchicalClustering hClustering = new HierarchicalClustering(Accord.Math.Distance.Euclidean, ClusteringLinkage.Min);
            hClustering.Compute(HierarchicalClusteringTest.GetData());
            int[] leaves = hClustering.DendrogramLinkCollection.ComputeLeafNodes();
            Assert.IsTrue(true);
            foreach (int i in leaves)
            {
                Console.Write("{0} ", i);
            }
            Console.WriteLine();            
        }

        [Test]
        public void ComputeLeafNodesFromTreeTest()
        {
            HierarchicalClustering hClustering = new HierarchicalClustering(Accord.Math.Distance.Euclidean, ClusteringLinkage.Min);
            hClustering.Compute(HierarchicalClusteringTest.GetData());
            int[] leaves = hClustering.DendrogramLinkCollection.ComputeLeafNodesFromTree();
            Assert.IsTrue(true);
            foreach (int i in leaves)
            {
                Console.Write("{0} ", i);
            }
            Console.WriteLine();            
        }

        [Test]
        public void ComputeWithExternalDistanceMatrix()
        {            
            DistanceMatrix matrix = new DistanceMatrix();            
            double[][] data = HierarchicalClusteringTest.GetData();
            for (int i = 0; i < data.Length; i++)
            {
                for (int j = i + 1; j < data.Length; j++)
                {
                    double distance = Infovision.Math.Distance.Euclidean(data[i], data[j]);
                    matrix.Add(new MatrixKey(i, j), distance);
                }
            }
            HierarchicalClustering hClustering = new HierarchicalClustering(matrix, ClusteringLinkage.Min);
            hClustering.Compute(HierarchicalClusteringTest.GetData());
            int[] leaves = hClustering.DendrogramLinkCollection.ComputeLeafNodesFromTree();
            Assert.IsTrue(true);
            foreach (int i in leaves)
            {
                Console.Write("{0} ", i);
            }
            Console.WriteLine();            
        }

        [Test]
        public void GetDendrogramAsBitmapTest()
        {
            HierarchicalClustering hClustering = new HierarchicalClustering(Accord.Math.Distance.Manhattan, ClusteringLinkage.Max);
            hClustering.Compute(HierarchicalClusteringTest.GetData());
            Console.Write(hClustering.DendrogramLinkCollection.ToString());
            Bitmap bitmap = hClustering.GetDendrogramAsBitmap(640, 480);
            bitmap.Save(@"F:\test_euc_max.bmp", System.Drawing.Imaging.ImageFormat.Bmp);
            Assert.IsTrue(true);
        }

        [Test]
        public void GetDendrogramAsBitmapMinLinkageTest()
        {
            HierarchicalClustering hClustering = new HierarchicalClustering(Accord.Math.Distance.Manhattan, ClusteringLinkage.Min);
            hClustering.Compute(HierarchicalClusteringTest.GetData());
            Console.Write(hClustering.DendrogramLinkCollection.ToString());
            Bitmap bitmap = hClustering.GetDendrogramAsBitmap(640, 480);
            bitmap.Save(@"F:\test_euc_min.bmp", System.Drawing.Imaging.ImageFormat.Bmp);
            Assert.IsTrue(true);
        }

        [Test]
        public void GetClusterMembershipTest()
        {
            HierarchicalClustering hClustering = new HierarchicalClustering(Accord.Math.Distance.Euclidean, ClusteringLinkage.Min);
            hClustering.Compute(HierarchicalClusteringTest.GetData());            
            int[] membership2 = hClustering.GetClusterMembership(3.6);
            int[] membership = hClustering.GetClusterMembership(4);            
            
            Assert.AreEqual(membership.Length, membership2.Length);
            for (int i = 0; i < membership.Length; i++)
            {
                Assert.AreEqual(membership[i], membership2[i]);
            }
        }


    }
}
