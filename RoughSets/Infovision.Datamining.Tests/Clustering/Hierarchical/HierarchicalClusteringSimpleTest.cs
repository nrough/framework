using System;
using System.Collections.Generic;
using System.Drawing;
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
        private static readonly Tuple<Func<double[], double[], double>, Func<int[], int[], DistanceMatrix, double>, int>[] DistancesAndLinkages =
        {            
            new Tuple<Func<double[], double[], double>, Func<int[], int[], DistanceMatrix, double>, int>(Accord.Math.Distance.Euclidean, ClusteringLinkage.Min, 1),
            new Tuple<Func<double[], double[], double>, Func<int[], int[], DistanceMatrix, double>, int>(Accord.Math.Distance.Euclidean, ClusteringLinkage.Max, 2),
            new Tuple<Func<double[], double[], double>, Func<int[], int[], DistanceMatrix, double>, int>(Accord.Math.Distance.Euclidean, ClusteringLinkage.Mean, 3),
            new Tuple<Func<double[], double[], double>, Func<int[], int[], DistanceMatrix, double>, int>(Accord.Math.Distance.SquareEuclidean, ClusteringLinkage.Min, 4),
            new Tuple<Func<double[], double[], double>, Func<int[], int[], DistanceMatrix, double>, int>(Accord.Math.Distance.SquareEuclidean, ClusteringLinkage.Max, 5),
            new Tuple<Func<double[], double[], double>, Func<int[], int[], DistanceMatrix, double>, int>(Accord.Math.Distance.SquareEuclidean, ClusteringLinkage.Mean, 6),
            new Tuple<Func<double[], double[], double>, Func<int[], int[], DistanceMatrix, double>, int>(Accord.Math.Distance.Manhattan, ClusteringLinkage.Min, 7),
            new Tuple<Func<double[], double[], double>, Func<int[], int[], DistanceMatrix, double>, int>(Accord.Math.Distance.Manhattan, ClusteringLinkage.Max, 8),
            new Tuple<Func<double[], double[], double>, Func<int[], int[], DistanceMatrix, double>, int>(Accord.Math.Distance.Manhattan, ClusteringLinkage.Mean, 9),
            new Tuple<Func<double[], double[], double>, Func<int[], int[], DistanceMatrix, double>, int>(Accord.Math.Distance.Euclidean, ClusteringLinkage.Average, 35),
            new Tuple<Func<double[], double[], double>, Func<int[], int[], DistanceMatrix, double>, int>(Accord.Math.Distance.SquareEuclidean, ClusteringLinkage.Average, 65),
            new Tuple<Func<double[], double[], double>, Func<int[], int[], DistanceMatrix, double>, int>(Accord.Math.Distance.Manhattan, ClusteringLinkage.Average, 95),
        }; 

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
            int[] leaves = hClustering.DendrogramLinkCollection.ComputeLeafNodes();                        
            
            foreach (int i in leaves)
            {
                Console.Write("{0} ", i);
            }
            Console.WriteLine();

            Assert.IsTrue(true);
        }

        [Test]
        public void ComputeLeafNodesFromTreeTest()
        {
            HierarchicalClusteringSimple hClustering = new HierarchicalClusteringSimple(Accord.Math.Distance.Euclidean, ClusteringLinkage.Min);
            hClustering.Compute(HierarchicalClusteringTest.GetData());
            int[] leaves = hClustering.DendrogramLinkCollection.ComputeLeafNodesFromTree();

            foreach (int i in leaves)
            {
                Console.Write("{0} ", i);
            }
            Console.WriteLine();

            Assert.IsTrue(true);
        }

        [Test, TestCaseSource("DistancesAndLinkages")]
        public void GetDendrogramAsBitmapTest(Tuple<Func<double[], double[], double>, Func<int[], int[], DistanceMatrix, double>, int> t)
        {
            Console.WriteLine("HierarcihcalClusteringSimpleTest.GetDendrogramAsBitmapTest()");
            
            Func<double[], double[], double> distance = t.Item1;
            Func<int[], int[], DistanceMatrix, double> linkage = t.Item2;
            int id = t.Item3;            

            HierarchicalClusteringSimple hClustering = new HierarchicalClusteringSimple(distance, linkage);
            hClustering.Compute(HierarchicalClusteringTest.GetData());

            

            Console.Write(hClustering.DendrogramLinkCollection.ToString());
            Console.WriteLine();

            Bitmap bitmap = hClustering.GetDendrogramAsBitmap(640, 480);
            string fileName = String.Format(@"F:\Dendrogram\DendrogramSimple_{0}.bmp", id);
            bitmap.Save(fileName, System.Drawing.Imaging.ImageFormat.Bmp);
            Assert.IsTrue(true);
        }

        [Test, TestCaseSource("DistancesAndLinkages")]
        public void GetDendrogramAsBitmapWithCustomeMatrixTest(Tuple<Func<double[], double[], double>, Func<int[], int[], DistanceMatrix, double>, int> t)
        {
            Console.WriteLine("HierarcihcalClusteringSimpleTest.GetDendrogramAsBitmapTest()");

            Func<double[], double[], double> distance = t.Item1;
            Func<int[], int[], DistanceMatrix, double> linkage = t.Item2;
            int id = t.Item3;

            DistanceMatrix matrix = new DistanceMatrix();
            double[][] data = HierarchicalClusteringTest.GetData();
            for (int i = 0; i < data.Length; i++)
            {
                for (int j = i + 1; j < data.Length; j++)
                {
                    double dist = Infovision.Math.Distance.Euclidean(data[i], data[j]);
                    matrix.Add(new MatrixKey(i, j), dist);
                }
            }

            Console.WriteLine(matrix.ToString());

            HierarchicalClusteringSimple hClustering = new HierarchicalClusteringSimple(matrix, linkage);
            hClustering.Compute(HierarchicalClusteringTest.GetData());

            Console.Write(hClustering.DendrogramLinkCollection.ToString());
            Console.WriteLine();

            Bitmap bitmap = hClustering.GetDendrogramAsBitmap(640, 480);
            string fileName = String.Format(@"F:\Dendrogram\DendrogramSimple_{0}.bmp", id);
            bitmap.Save(fileName, System.Drawing.Imaging.ImageFormat.Bmp);
            Assert.IsTrue(true);
        }
    }
}
