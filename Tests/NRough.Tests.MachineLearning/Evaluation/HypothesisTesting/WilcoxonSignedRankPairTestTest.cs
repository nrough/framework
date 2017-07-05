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

using NRough.MachineLearning.Evaluation.HypothesisTesting;
using NRough.Tests.MachineLearning.Classification;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NRough.Tests.MachineLearning.Evaluation.HypothesisTesting
{
    [TextFixture]
    public class WilcoxonSignedRankPairTestTest
    {        
        [Test]
        public void ComputeTest()
        {
            double[] serie1 = new double[] {
                0.763, 0.599, 0.954, 0.628, 0.882, 0.936, 0.661, 0.583, 0.775, 1.000, 0.940, 0.619, 0.972, 0.957 };
            double[] serie2 = new double[] {
                0.768, 0.591, 0.971, 0.661, 0.888, 0.931, 0.668, 0.583, 0.838, 1.000, 0.962, 0.666, 0.981, 0.978 };

            var wilcoxon = new WilcoxonSignedRankPairTest();
            wilcoxon.AlternativeHypothesis = HypothesisType.FirstIsSmallerThanSecond;
            wilcoxon.ExactLimit = 8;
            wilcoxon.Compute(serie1, serie2);
            Console.WriteLine(wilcoxon.ToString("DEBUG", null));            

            var wilcoxonEx = new WilcoxonSignedRankPairTest();            
            wilcoxonEx.AlternativeHypothesis = HypothesisType.FirstIsSmallerThanSecond;
            wilcoxonEx.Exact = true;
            wilcoxonEx.Compute(serie1, serie2);
            Console.WriteLine(wilcoxonEx.ToString("DEBUG", null));

        }        

        [Test]
        public void Compute2Test()
        {
            double[] serie1 = new double[] {
                78, 24, 64, 45, 64, 52, 30, 50, 64, 50, 78, 22, 84, 40, 90, 72 };

            double[] serie2 = new double[] {
                78, 24, 62, 48, 68, 56, 25, 44, 56, 40, 68, 36, 68, 20, 58, 32 };

            var wilcoxon = new WilcoxonSignedRankPairTest();
            wilcoxon.AlternativeHypothesis = HypothesisType.FirstIsGreaterThanSecond;            
            wilcoxon.Alpha = 0.05;
            wilcoxon.Compute(serie1, serie2);
            Console.WriteLine(wilcoxon.ToString("DEBUG", null));
        }                       
    }
}
