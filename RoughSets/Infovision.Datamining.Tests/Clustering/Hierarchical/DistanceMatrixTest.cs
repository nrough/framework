﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Infovision.Datamining.Clustering.Hierarchical;
using Accord.Math;
using Infovision.Math;

namespace Infovision.Datamining.Tests.Clustering.Hierarchical
{
    [TestFixture]
    public class DistanceMatrixTest
    {
        [Test]
        public void CalcDistanceMatrix()
        {            
            DistanceMatrix matrix = new DistanceMatrix(Accord.Math.Distance.SquareEuclidean);
            matrix.Initialize(HierarchicalClusteringTest.GetData());
            Assert.IsTrue(true);

            Console.Write(matrix.ToString());
            Console.WriteLine();
        }

        [Test]
        public void AddTest()
        {
            DistanceMatrix matrix = new DistanceMatrix();
            double[][] data = HierarchicalClusteringTest.GetData();
            for (int i = 0; i < data.Length; i++)
            {
                for (int j = i + 1; j < data.Length; j++)
                {
                    double distance = Infovision.Math.Distance.SquaredEuclidean(data[i], data[j]);
                    matrix.Add(new MatrixKey(i, j), distance);
                }
            }

            Assert.IsTrue(true);

            Console.Write(matrix.ToString());
            Console.WriteLine();

        }
    }
}
