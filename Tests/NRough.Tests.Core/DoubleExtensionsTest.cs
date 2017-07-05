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