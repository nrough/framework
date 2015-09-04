using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Infovision.Datamining.Clustering.Hierarchical;
using Infovision.Math;
using Infovision.Datamining.Visualization;

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

        public static Dictionary<int, double[]> GetDataAsDict()
        {
            Dictionary<int, double[]> result = new Dictionary<int, double[]>();
            double[][] data = HierarchicalClusteringTest.GetData();
            for (int i = 0; i < data.Length; i++)
            {
                result.Add(i, data[i]);
            }
            return result;
        }


        private static readonly Tuple<Func<double[], double[], double>, Func<int[], int[], DistanceMatrix, double[][], double>, int>[] DistancesAndLinkages =
        {            
            new Tuple<Func<double[], double[], double>, Func<int[], int[], DistanceMatrix, double[][], double>, int>(Accord.Math.Distance.Euclidean, ClusteringLinkage.Single, 1),
            new Tuple<Func<double[], double[], double>, Func<int[], int[], DistanceMatrix, double[][], double>, int>(Accord.Math.Distance.Euclidean, ClusteringLinkage.Complete, 2),
            new Tuple<Func<double[], double[], double>, Func<int[], int[], DistanceMatrix, double[][], double>, int>(Accord.Math.Distance.Euclidean, ClusteringLinkage.Mean, 3),
            new Tuple<Func<double[], double[], double>, Func<int[], int[], DistanceMatrix, double[][], double>, int>(Accord.Math.Distance.SquareEuclidean, ClusteringLinkage.Single, 4),
            new Tuple<Func<double[], double[], double>, Func<int[], int[], DistanceMatrix, double[][], double>, int>(Accord.Math.Distance.SquareEuclidean, ClusteringLinkage.Complete, 5),
            new Tuple<Func<double[], double[], double>, Func<int[], int[], DistanceMatrix, double[][], double>, int>(Accord.Math.Distance.SquareEuclidean, ClusteringLinkage.Mean, 6),
            new Tuple<Func<double[], double[], double>, Func<int[], int[], DistanceMatrix, double[][], double>, int>(Accord.Math.Distance.Manhattan, ClusteringLinkage.Single, 7),
            new Tuple<Func<double[], double[], double>, Func<int[], int[], DistanceMatrix, double[][], double>, int>(Accord.Math.Distance.Manhattan, ClusteringLinkage.Complete, 8),
            new Tuple<Func<double[], double[], double>, Func<int[], int[], DistanceMatrix, double[][], double>, int>(Accord.Math.Distance.Manhattan, ClusteringLinkage.Mean, 9),
            new Tuple<Func<double[], double[], double>, Func<int[], int[], DistanceMatrix, double[][], double>, int>(Accord.Math.Distance.Euclidean, ClusteringLinkage.Average, 35),
            new Tuple<Func<double[], double[], double>, Func<int[], int[], DistanceMatrix, double[][], double>, int>(Accord.Math.Distance.SquareEuclidean, ClusteringLinkage.Average, 65),
            new Tuple<Func<double[], double[], double>, Func<int[], int[], DistanceMatrix, double[][], double>, int>(Accord.Math.Distance.Manhattan, ClusteringLinkage.Average, 95),
        };        

        [Test]
        public void ComputeTest()
        {                                    
            HierarchicalClustering hClustering = new HierarchicalClustering(Accord.Math.Distance.Euclidean, ClusteringLinkage.Single);
            hClustering.Instances = HierarchicalClusteringTest.GetDataAsDict();
            hClustering.Compute();            
            Assert.IsTrue(true);
        }

        [Test]
        public void CalcLCATest()
        {
            HierarchicalClustering hClustering = new HierarchicalClustering(Accord.Math.Distance.Euclidean, ClusteringLinkage.Single);
            hClustering.Instances = HierarchicalClusteringTest.GetDataAsDict();
            hClustering.Compute();

            //DendrogramChart dc = new DendrogramChart(hClustering, 640, 480);
            //Bitmap bitmap = dc.GetAsBitmap();
            //string fileName = @"F:\Dendrogram\Dendrogram_LCA.bmp";
            //bitmap.Save(fileName, System.Drawing.Imaging.ImageFormat.Bmp);

            Assert.AreEqual(8, hClustering.GetLeafDistance(1, 5));
            Assert.AreEqual(2, hClustering.GetLeafDistance(5, 9));
            Assert.AreEqual(2, hClustering.GetLeafDistance(9, 5));
            //Assert.AreEqual(1, hClustering.GetLeafDistance(5, -1));
            //Assert.AreEqual(1, hClustering.GetLeafDistance(-1, 5));
            Assert.AreEqual(4, hClustering.GetLeafDistance(4, 3));
            Assert.AreEqual(4, hClustering.GetLeafDistance(3, 4));
            //Assert.AreEqual(5, hClustering.GetLeafDistance(-9, 6));
            //Assert.AreEqual(5, hClustering.GetLeafDistance(6, -9));
            Assert.AreEqual(7, hClustering.GetLeafDistance(7, 5));
            Assert.AreEqual(7, hClustering.GetLeafDistance(5, 7));            
        }

        [Test]
        public void AvgNodeLevelDistanceTest()
        {
            HierarchicalClustering hClustering = new HierarchicalClustering(Accord.Math.Distance.Euclidean, ClusteringLinkage.Single);
            hClustering.Instances = HierarchicalClusteringTest.GetDataAsDict();
            hClustering.Compute();

            Assert.AreEqual(238.0 / 45, hClustering.AvgNodeLevelDistance);
        }


        [Test, TestCaseSource("DistancesAndLinkages")]
        public void ComputeSimpleVsAgregativeTest(Tuple<Func<double[], double[], double>, Func<int[], int[], DistanceMatrix, double[][], double>, int> t)
        {
            Func<double[], double[], double> distance = t.Item1;
            Func<int[], int[], DistanceMatrix, double[][], double> linkage = t.Item2;
            int id = t.Item3;

            HierarchicalClustering aggregativeVersion = new HierarchicalClustering(distance, linkage);
            aggregativeVersion.Instances = HierarchicalClusteringTest.GetDataAsDict();
            aggregativeVersion.Compute();
            
            HierarchicalClusteringSimple simpleVersion = new HierarchicalClusteringSimple(distance, linkage);
            simpleVersion.Instances = HierarchicalClusteringTest.GetDataAsDict();
            simpleVersion.Compute();

            DendrogramChart dc1 = new DendrogramChart(aggregativeVersion, 640, 480);
            Bitmap bitmap = dc1.GetAsBitmap();
            string fileName = String.Format(@"F:\Dendrogram\DndrA_{0}.bmp", id);
            bitmap.Save(fileName, System.Drawing.Imaging.ImageFormat.Bmp);

            DendrogramChart dc2 = new DendrogramChart(simpleVersion, 640, 480);
            bitmap = dc2.GetAsBitmap();
            fileName = String.Format(@"F:\Dendrogram\DndrS_{0}.bmp", id);
            bitmap.Save(fileName, System.Drawing.Imaging.ImageFormat.Bmp);

            Assert.AreEqual(simpleVersion.DendrogramLinkCollection.Count,
                            aggregativeVersion.DendrogramLinkCollection.Count,
                            String.Format("DendrogramLinkCollections have different sizes: {0} {1}", simpleVersion.DendrogramLinkCollection, aggregativeVersion.DendrogramLinkCollection));
            
            Console.WriteLine(simpleVersion.DendrogramLinkCollection.ToString());
            Console.WriteLine(aggregativeVersion.DendrogramLinkCollection.ToString());

            for (int i = 0; i < simpleVersion.DendrogramLinkCollection.Count; i++ )
            {
                DendrogramLink simpleLink = simpleVersion.DendrogramLinkCollection[i];
                DendrogramLink aggregativeLink = aggregativeVersion.DendrogramLinkCollection[i];

                Assert.AreEqual(simpleLink.Id, aggregativeLink.Id, String.Format("Simple: {0}; Aggregative: {1}", simpleLink, aggregativeLink));
                //The ordering of items with the same distance might be different
                //Assert.AreEqual(simpleLink.Cluster1, aggregativeLink.Cluster1, String.Format("Simple: {0}; Aggregative: {1}", simpleLink, aggregativeLink));
                //Assert.AreEqual(simpleLink.Cluster2, aggregativeLink.Cluster2, String.Format("Simple: {0}; Aggregative: {1}", simpleLink, aggregativeLink));
                Assert.AreEqual(System.Math.Round(simpleLink.Distance, 11), System.Math.Round(aggregativeLink.Distance, 11), String.Format("Simple: {0}; Aggregative: {1}", simpleLink, aggregativeLink));
            }
        }       

        [Test]
        public void ComputeLeafNodesFromTreeTest()
        {
            HierarchicalClustering hClustering = new HierarchicalClustering(Accord.Math.Distance.Euclidean, ClusteringLinkage.Single);
            hClustering.Instances = HierarchicalClusteringTest.GetDataAsDict();
            hClustering.Compute();

            int[] leaves = hClustering.GetLeaves();
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
                    double distance = Infovision.Math.Similarity.Euclidean(data[i], data[j]);
                    matrix.Add(new MatrixKey(i, j), distance);
                }
            }
            HierarchicalClustering hClustering = new HierarchicalClustering(matrix, ClusteringLinkage.Single);
            hClustering.Instances = HierarchicalClusteringTest.GetDataAsDict();
            hClustering.Compute();

            int[] leaves = hClustering.GetLeaves();
            Assert.IsTrue(true);
            foreach (int i in leaves)
            {
                Console.Write("{0} ", i);
            }
            Console.WriteLine();            
        }


        [Test, TestCaseSource("DistancesAndLinkages")]
        public void GetDendrogramAsBitmapTest(Tuple<Func<double[], double[], double>, Func<int[], int[], DistanceMatrix, double[][], double>, int> t)
        {
            Func<double[], double[], double> distance = t.Item1;
            Func<int[], int[], DistanceMatrix, double[][], double> linkage = t.Item2;
            int id = t.Item3;

            HierarchicalClustering hClustering = new HierarchicalClustering(distance, linkage);
            hClustering.Instances = HierarchicalClusteringTest.GetDataAsDict();
            hClustering.Compute();
            
            Console.Write(hClustering.DendrogramLinkCollection.ToString());
            Console.WriteLine();

            DendrogramChart dc = new DendrogramChart(hClustering, 640, 480);
            Bitmap bitmap = dc.GetAsBitmap();
            string fileName = String.Format(@"F:\Dendrogram\Dendrogram_{0}.bmp", id);
            bitmap.Save(fileName, System.Drawing.Imaging.ImageFormat.Bmp);
            Assert.IsTrue(true);
        }       

        [Test]
        public void GetClusterMembershipTest()
        {
            HierarchicalClustering hClustering = new HierarchicalClustering(Accord.Math.Distance.Euclidean, ClusteringLinkage.Single);
            hClustering.Instances = HierarchicalClusteringTest.GetDataAsDict();
            hClustering.Compute();

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
