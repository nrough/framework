﻿using Raccoon.Math;
using NUnit.Framework;

namespace Raccoon.MachineLearning.Tests.Clustering.Hierarchical
{
    [TestFixture]
    public class DistanceMatrixTest
    {       
        [Test]
        public void AddTest()
        {
            DistanceMatrix matrix = new DistanceMatrix();
            double[][] data = HierarchicalClusteringTest.GetData();
            for (int i = 0; i < data.Length; i++)
            {
                for (int j = i + 1; j < data.Length; j++)
                {
                    double distance = Raccoon.Math.Distance.SquaredEuclidean(data[i], data[j]);

                    //Console.WriteLine("{0}, {1}, {2}", i, j, distance);

                    matrix.Add(new MatrixKey(i, j), distance);
                }
            }

            Assert.IsTrue(true);

            //Console.Write(matrix.ToString());
            //Console.WriteLine();
        }

        [Test]
        public void InitializeTest()
        {
            double[][] data = HierarchicalClusteringTest.GetData();
            DistanceMatrix matrix = new DistanceMatrix(data.Length, Raccoon.Math.Distance.SquaredEuclidean);
            matrix.Initialize(data);

            for (int i = 0; i < data.Length; i++)
            {
                for (int j = i + 1; j < data.Length; j++)
                {
                    double distance = Raccoon.Math.Distance.SquaredEuclidean(data[i], data[j]);
                    Assert.AreEqual(distance, matrix.GetDistance(i, j));
                }
            }
        }
    }
}