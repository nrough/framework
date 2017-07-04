//
// NRough Framework
// http://www.nrough.net
//
// Copyright © Sebastian Widz, 2017
// authors at nrough.net

//This file is part of NRough.

//Foobar is free software; you can redistribute it and/or modify
//it under the terms of the GNU General Public License as published by
//the Free Software Foundation; either version 3 of the License, or
//(at your option) any later version.
//
//Foobar is distributed in the hope that it will be useful,
//but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
//GNU General Public License for more details.
//
//You should have received a copy of the GNU General Public License
//along with Foobar; if not, write to the Free Software
//Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA  02110-1301  USA
//
using System;
using System.Drawing;
using NRough.MachineLearning.Clustering.Hierarchical;
using NRough.Math;
using NUnit.Framework;

namespace NRough.Tests.MachineLearning.Clustering.Hierarchical
{
    [TestFixture]
    public class HierarcihcalClusteringSimpleTest
    {
        private static readonly Tuple<Func<double[], double[], double>, Func<int[], int[], DistanceMatrix, double[][], double>, int>[] DistancesAndLinkages =
        {
            new Tuple<Func<double[], double[], double>, Func<int[], int[], DistanceMatrix, double[][], double>, int>(Distance.Euclidean, ClusteringLinkage.Single, 1),
            new Tuple<Func<double[], double[], double>, Func<int[], int[], DistanceMatrix, double[][], double>, int>(Distance.Euclidean, ClusteringLinkage.Complete, 2),
            new Tuple<Func<double[], double[], double>, Func<int[], int[], DistanceMatrix, double[][], double>, int>(Distance.Euclidean, ClusteringLinkage.Mean, 3),
            new Tuple<Func<double[], double[], double>, Func<int[], int[], DistanceMatrix, double[][], double>, int>(Distance.SquaredEuclidean, ClusteringLinkage.Single, 4),
            new Tuple<Func<double[], double[], double>, Func<int[], int[], DistanceMatrix, double[][], double>, int>(Distance.SquaredEuclidean, ClusteringLinkage.Complete, 5),
            new Tuple<Func<double[], double[], double>, Func<int[], int[], DistanceMatrix, double[][], double>, int>(Distance.SquaredEuclidean, ClusteringLinkage.Mean, 6),
            new Tuple<Func<double[], double[], double>, Func<int[], int[], DistanceMatrix, double[][], double>, int>(Distance.Manhattan, ClusteringLinkage.Single, 7),
            new Tuple<Func<double[], double[], double>, Func<int[], int[], DistanceMatrix, double[][], double>, int>(Distance.Manhattan, ClusteringLinkage.Complete, 8),
            new Tuple<Func<double[], double[], double>, Func<int[], int[], DistanceMatrix, double[][], double>, int>(Distance.Manhattan, ClusteringLinkage.Mean, 9),
            new Tuple<Func<double[], double[], double>, Func<int[], int[], DistanceMatrix, double[][], double>, int>(Distance.Euclidean, ClusteringLinkage.Average, 35),
            new Tuple<Func<double[], double[], double>, Func<int[], int[], DistanceMatrix, double[][], double>, int>(Distance.SquaredEuclidean, ClusteringLinkage.Average, 65),
            new Tuple<Func<double[], double[], double>, Func<int[], int[], DistanceMatrix, double[][], double>, int>(Distance.Manhattan, ClusteringLinkage.Average, 95),

            //can only be used if Distance method is set, not to use when only distance matrix is passed to the histogram
            //new Tuple<Func<double[], double[], double>, Func<int[], int[], DistanceMatrix, double[][], double>, int>(Accord.Math.Distance.Manhattan, ClusteringLinkage.Ward, 10),
        };

        [Test]
        public void ComputeTest()
        {
            HierarchicalClusteringSimple hClustering = new HierarchicalClusteringSimple(Distance.Euclidean, ClusteringLinkage.Single);
            hClustering.Instances = HierarchicalClusteringTest.GetDataAsDict();
            hClustering.Compute();
            Assert.IsTrue(true);
        }

        [Test, TestCaseSource("DistancesAndLinkages")]
        public void GetDendrogramAsBitmapTest(Tuple<Func<double[], double[], double>, Func<int[], int[], DistanceMatrix, double[][], double>, int> t)
        {
            //Console.WriteLine("HierarchicalClusteringSimpleTest.GetDendrogramAsBitmapTest()");

            Func<double[], double[], double> distance = t.Item1;
            Func<int[], int[], DistanceMatrix, double[][], double> linkage = t.Item2;
            int id = t.Item3;

            HierarchicalClusteringSimple hClustering = new HierarchicalClusteringSimple(distance, linkage);
            hClustering.Instances = HierarchicalClusteringTest.GetDataAsDict();
            hClustering.Compute();

            //Console.Write(hClustering.ToString());
            //Console.WriteLine();

            DendrogramChart dc = new DendrogramChart(hClustering, 640, 480);
            Bitmap bitmap = dc.GetAsBitmap();
            string fileName = String.Format(@"DendrogramSimple_{0}.bmp", id);
            bitmap.Save(fileName, System.Drawing.Imaging.ImageFormat.Bmp);
            Assert.IsTrue(true);
        }

        [Test, TestCaseSource("DistancesAndLinkages")]
        public void GetDendrogramAsBitmapWithCustomeMatrixTest(Tuple<Func<double[], double[], double>, Func<int[], int[], DistanceMatrix, double[][], double>, int> t)
        {
            //Console.WriteLine("HierarcihcalClusteringSimpleTest.GetDendrogramAsBitmapTest()");

            Func<double[], double[], double> distance = t.Item1;
            Func<int[], int[], DistanceMatrix, double[][], double> linkage = t.Item2;
            int id = t.Item3;

            DistanceMatrix matrix = new DistanceMatrix();
            //matrix.Distance = NRough.Math.Similarity.Euclidean;
            double[][] data = HierarchicalClusteringTest.GetData();
            for (int i = 0; i < data.Length; i++)
            {
                for (int j = i + 1; j < data.Length; j++)
                {
                    double dist = NRough.Math.Distance.Euclidean(data[i], data[j]);
                    matrix.Add(new SymetricPair<int, int>(i, j), dist);
                }
            }

            //Console.WriteLine(matrix.ToString());

            HierarchicalClusteringSimple hClustering = new HierarchicalClusteringSimple(matrix, linkage);
            hClustering.Instances = HierarchicalClusteringTest.GetDataAsDict();
            hClustering.Compute();

            //Console.Write(hClustering.ToString());
            //Console.WriteLine();

            DendrogramChart dc = new DendrogramChart(hClustering, 640, 480);
            Bitmap bitmap = dc.GetAsBitmap();
            string fileName = String.Format(@"DendrogramSimple_{0}.bmp", id);
            bitmap.Save(fileName, System.Drawing.Imaging.ImageFormat.Bmp);
            Assert.IsTrue(true);
        }
    }
}