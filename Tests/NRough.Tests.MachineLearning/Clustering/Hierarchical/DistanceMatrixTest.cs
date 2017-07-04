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
using NRough.Math;
using NUnit.Framework;

namespace NRough.Tests.MachineLearning.Clustering.Hierarchical
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
                    double distance = NRough.Math.Distance.SquaredEuclidean(data[i], data[j]);

                    //Console.WriteLine("{0}, {1}, {2}", i, j, distance);

                    matrix.Add(new SymetricPair<int, int>(i, j), distance);
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
            DistanceMatrix matrix = new DistanceMatrix(data.Length, NRough.Math.Distance.SquaredEuclidean);
            matrix.Initialize(data);

            for (int i = 0; i < data.Length; i++)
            {
                for (int j = i + 1; j < data.Length; j++)
                {
                    double distance = NRough.Math.Distance.SquaredEuclidean(data[i], data[j]);
                    Assert.AreEqual(distance, matrix.GetDistance(i, j));
                }
            }
        }
    }
}