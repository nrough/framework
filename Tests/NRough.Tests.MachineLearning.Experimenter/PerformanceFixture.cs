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
using NRough.MachineLearning.Experimenter.Parms;
using NUnit.Framework;

namespace NRough.Tests.MachineLearning.Experimenter
{
    [TestFixture]
    internal class PerformanceFixture
    {
        [Test]
        public void LongRunningLoop()
        {
            ParameterCollection parmList = new ParameterCollection(new IParameter[] {
                new ParameterNumericRange<int>("1", 0, 10, 1),
                new ParameterNumericRange<double>("2", 0, 10, 1),
                new ParameterNumericRange<double>("3", 0, 0.1, 0.01),
                new ParameterNumericRange<float>("4", 0f, 1f, 0.1f),
                new ParameterNumericRange<double>("5", 0, (double)11.5, 1),
                new ParameterNumericRange<int>("6", -10, 0, 1),
                new ParameterNumericRange<int>("7", 10, 0, -1),
                new ParameterNumericRange<double>("8", -1, 0, 0.1),
                new ParameterNumericRange<double>("9", 1, 0, -0.1),
                new ParameterNumericRange<short>("10", 0, 10, 1)});

            int i = 0;
            foreach (object[] p in parmList.Values())
            {
                //System.Console.WriteLine("{0} {1} {2} {3} {4} {5} {6} {7} {8} {9}", p[0], p[1], p[2], p[3], p[4], p[5], p[6], p[7], p[8], p[9]);

                i++;
                if (i > 10000)
                    break;
            }
        }
    }
}