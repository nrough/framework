// 
// NRough Framework
// http://www.nrough.net
// 
// Copyright (c) Sebastian Widz, 2017
// authors at nrough.net
// 
// This file is part of NRough Framework.
// 
// NRough Framework is free software; you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation; either version 3 of the License, or
// (at your option) any later version.
// 
// NRough Framework is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with NRough Framework; if not, write to the Free Software
// Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA  02110-1301  USA
//

using NRough.MachineLearning.Classification;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NRough.Tests.MachineLearning.Classification
{
    [TextFixture]
    public class ConfusionMatrixTest
    {
        [Test]
        public void ExampleTest()
        {
            long[] labels = new long[] { 1, 2, 3 };
            var confusionMatrix = new ConfusionMatrix(labels);

            confusionMatrix.AddResult(1, 1, 5);
            confusionMatrix.AddResult(1, 2, 3);
            confusionMatrix.AddResult(1, 3, 0);

            confusionMatrix.AddResult(2, 1, 2);
            confusionMatrix.AddResult(2, 2, 3);
            confusionMatrix.AddResult(2, 3, 1);

            confusionMatrix.AddResult(3, 1, 0);
            confusionMatrix.AddResult(3, 2, 2);
            confusionMatrix.AddResult(3, 3, 11);

            Console.WriteLine(confusionMatrix);

            Assert.AreEqual(5, confusionMatrix.TP(1));
            Assert.AreEqual(2, confusionMatrix.FP(1));
            Assert.AreEqual(3, confusionMatrix.FN(1));
            Assert.AreEqual(17, confusionMatrix.TN(1));

            Assert.AreEqual(3, confusionMatrix.TP(2));
            Assert.AreEqual(5, confusionMatrix.FP(2));
            Assert.AreEqual(3, confusionMatrix.FN(2));
            Assert.AreEqual(16, confusionMatrix.TN(2));

            Assert.AreEqual(11, confusionMatrix.TP(3));
            Assert.AreEqual(1, confusionMatrix.FP(3));
            Assert.AreEqual(2, confusionMatrix.FN(3));
            Assert.AreEqual(13, confusionMatrix.TN(3));

            Console.WriteLine(confusionMatrix.TruePositiveAvg);
            Console.WriteLine(confusionMatrix.FalsePositiveAvg);
            Console.WriteLine(confusionMatrix.FalseNegativeAvg);
            Console.WriteLine(confusionMatrix.TrueNegativeAvg);
        }

        public void Example2Test()
        {
            long[] labels = new long[] { 1, 2, 3 };
            var confusionMatrix = new ConfusionMatrix(labels);

            confusionMatrix.AddResult(1, 1, 30);
            confusionMatrix.AddResult(1, 2, 50);
            confusionMatrix.AddResult(1, 3, 20);

            confusionMatrix.AddResult(2, 1, 20);
            confusionMatrix.AddResult(2, 2, 60);
            confusionMatrix.AddResult(2, 3, 20);

            confusionMatrix.AddResult(3, 1, 10);
            confusionMatrix.AddResult(3, 2, 10);
            confusionMatrix.AddResult(3, 3, 80);

            Console.WriteLine(confusionMatrix);

            Assert.AreEqual(0.3, confusionMatrix.Recall(1));
            Assert.AreEqual(0.5, confusionMatrix.Precision(1));

            Assert.AreEqual(0.6, confusionMatrix.Recall(2));
            Assert.AreEqual(0.5, confusionMatrix.Precision(2));

            Console.WriteLine(confusionMatrix.Accuracy);

            Console.WriteLine(confusionMatrix.RecallMicro);
            Console.WriteLine(confusionMatrix.RecallMacro);

            Console.WriteLine(confusionMatrix.PrecisionMicro);
            Console.WriteLine(confusionMatrix.PrecisionMacro);
        }
    }
}
