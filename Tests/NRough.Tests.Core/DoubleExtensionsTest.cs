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

using System;
using NUnit.Framework;
using NRough.Core;
using NRough.Core.BaseTypeExtensions;

namespace NRough.Tests.Core
{
    [TestFixture]
    internal class DoubleExtensionsTest
    {
        [Test]
        public void IsNormalTest()
        {
            double v;

            v = 10.0;
            Assert.IsTrue(v.IsNormal(), "10.0");

            v = 10.2;
            Assert.IsTrue(v.IsNormal(), "10.2");

            v = Double.NaN;
            Assert.IsFalse(v.IsNormal(), "NaN");

            v = Double.NegativeInfinity;
            Assert.IsFalse(v.IsNormal(), "-Inf");

            v = Double.PositiveInfinity;
            Assert.IsFalse(v.IsNormal(), "+Inf");

            v = Double.MinValue;
            Assert.IsTrue(v.IsNormal(), "MinValue");

            v = Double.MaxValue;
            Assert.IsTrue(v.IsNormal(), "MaxValue");

            v = 0.0;
            Assert.IsTrue(v.IsNormal(), "0.0");

            v = 0;
            Assert.IsTrue(v.IsNormal(), "0");

            v = Double.Epsilon;
            Assert.IsFalse(v.IsNormal(), "Epsilon");

            v = Double.Epsilon * 0.5;
            Assert.IsTrue(v.IsNormal(), "Epsilon * 0.5");
        }
    }
}