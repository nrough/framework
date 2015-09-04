﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Infovision.Datamining.Clustering.Hierarchical;

namespace Infovision.Datamining.Tests.Clustering.Hierarchical
{
    [TestFixture]
    public class AgnesTest
    {
                
        [Test]
        public void CalculateTest()
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

            
            
            Agnes agnes = new Agnes(Accord.Math.Distance.Euclidean);
            agnes.Compute(data);
            
            Assert.IsTrue(true);

        }
    }
}
