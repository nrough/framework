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
    public class ParmListEnumeratorFixture
    {
        [Test]
        public void ParameterValueEnumerator_GenerateIntegerList()
        {
            //Console.WriteLine("*** ParameterValueEnumerator_GenerateIntegerList ****");

            IParameter parmA = new ParameterValueCollection<int>("A", new int[] { 0, 1 });
            IParameter parmB = new ParameterValueCollection<int>("B", new int[] { 0, 1 });
            IParameter parmC = new ParameterValueCollection<int>("C", new int[] { 0, 1 });

            ParameterCollection parmList = new ParameterCollection(new IParameter[] { parmA, parmB, parmC });

            int i = 0;
            foreach (object[] parms in parmList.Values())
            {
                //Console.WriteLine("{0} {1} {2}", parms[0], parms[1], parms[2]);
                i++;
            }

            Assert.AreEqual(8, i);
        }

        [Test]
        public void ParameterValueEnumerator_GenerateStringList()
        {
            //Console.WriteLine("*** ParameterValueEnumerator_GenerateStringList ****");

            IParameter parmA = new ParameterValueCollection<string>("A", new string[] { "1", "2", "3" });
            IParameter parmB = new ParameterValueCollection<string>("B", new string[] { "1", "2", "3" });
            IParameter parmC = new ParameterValueCollection<string>("C", new string[] { "1", "2", "3" });
            IParameter parmD = new ParameterValueCollection<string>("D", new string[] { "1", "2", "3" });

            ParameterCollection parmList = new ParameterCollection(new IParameter[] { parmA, parmB, parmC, parmD });

            int i = 0;
            foreach (object[] parms in parmList.Values())
            {
                //Console.WriteLine("{0} {1} {2} {3}", parms[0], parms[1], parms[2], parms[3]);
                i++;
            }

            Assert.AreEqual(81, i);
        }

        [Test]
        public void ParameterValueEnumerator_GenerateMixedList()
        {
            //Console.WriteLine("*** ParameterValueEnumerator_GenerateMixedList ****");

            IParameter parmA = new ParameterValueCollection<int>("A", new int[] { 1, 2, 3 });
            IParameter parmB = new ParameterValueCollection<string>("B", new string[] { "A", "B", "C" });
            IParameter parmC = new ParameterValueCollection<double>("C", new double[] { 0.1, 0.2, 0.3 });

            ParameterCollection parmList = new ParameterCollection(new IParameter[] { parmA, parmB, parmC });

            int i = 0;
            foreach (object[] parms in parmList.Values())
            {
                //Console.WriteLine("{0} {1} {2}", parms[0], parms[1], parms[2]);
                i++;
            }

            Assert.AreEqual(27, i);
        }

        [Test]
        public void ParameterValueEnumerator_GenerateIntegerRangeList()
        {
            //Console.WriteLine("*** ParameterValueEnumerator_GenerateIntegerRangeList ****");

            IParameter parmA = new ParameterNumericRange<int>("RangeA", 1, 3, 1);
            IParameter parmB = new ParameterNumericRange<int>("RangeB", 1, 3, 1);
            IParameter parmC = new ParameterNumericRange<int>("RangeC", 1, 3, 1);

            ParameterCollection parmList = new ParameterCollection(new IParameter[] { parmA, parmB, parmC });

            int i = 0;
            foreach (object[] parms in parmList.Values())
            {
                //Console.WriteLine("{0} {1} {2}", parms[0], parms[1], parms[2]);
                i++;
            }

            Assert.AreEqual(27, i);
        }

        [Test]
        public void ParameterValueEnumerator_GenerateDoubleRangeList()
        {
            //Console.WriteLine("*** ParameterValueEnumerator_GenerateIntegerRangeList ****");

            IParameter parmA = new ParameterNumericRange<double>("RangeA", 2.05, 3.07, 0.15);
            IParameter parmB = new ParameterNumericRange<double>("RangeB", 0.05, 3.46, 0.15);
            IParameter parmC = new ParameterNumericRange<double>("RangeC", 1.05, 2.1, 0.15);

            ParameterCollection parmList = new ParameterCollection(new IParameter[] { parmA, parmB, parmC });

            int i = 0;
            foreach (object[] parms in parmList.Values())
            {
                //Console.WriteLine("{0} {1} {2}", parms[0], parms[1], parms[2]);
                i++;
            }

            Assert.AreEqual(1288, i);
        }
    }
}