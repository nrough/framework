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

using NRough.MachineLearning.Experimenter.Parms;
using NUnit.Framework;

namespace NRough.Tests.MachineLearning.Experimenter
{
    [TestFixture]
    internal class ValueInRangeFixture
    {
        [Test]
        public void ValueInNumericRange()
        {
            IParameter parmA = new ParameterNumericRange<int>("A", 0, 10, 1);

            Assert.IsTrue(parmA.InRange(0));
            Assert.IsTrue(parmA.InRange(5));
            Assert.IsTrue(parmA.InRange(10));

            Assert.IsFalse(parmA.InRange(-1));
            Assert.IsFalse(parmA.InRange(11));

            Assert.IsTrue(parmA.InRange(0.1));
            Assert.IsTrue(parmA.InRange(9.99999));

            Assert.IsFalse(parmA.InRange(-0.1));
            Assert.IsFalse(parmA.InRange(-10));
        }

        [Test]
        public void ValueInListRange()
        {
            IParameter parmA = new ParameterValueCollection<int>("A", new int[] { 1, 2, 3, 4, 7, 8, 9 });

            Assert.IsTrue(parmA.InRange(1));
            Assert.IsTrue(parmA.InRange(4));
            Assert.IsTrue(parmA.InRange(9));

            Assert.IsFalse(parmA.InRange(-1));
            Assert.IsFalse(parmA.InRange(11));
            Assert.IsFalse(parmA.InRange(5));

            Assert.IsFalse(parmA.InRange(-0.1));
            Assert.IsFalse(parmA.InRange(-10));
        }
    }
}