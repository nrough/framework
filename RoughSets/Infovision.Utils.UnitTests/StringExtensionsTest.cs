using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Infovision.Core.Tests
{
    [TestFixture]
    class StringExtensionsTest
    {
        [TestCase("1234,12345,12", 0)]
        [TestCase("1234,12345", 0)]
        [TestCase("1234.12345", 5)]
        [TestCase("1234.0", 1)]
        [TestCase("1234", 0)]
        [TestCase("ABC", 0)]
        [TestCase(".1234", 4)]
        [TestCase("", 0)]
        public void GetNumberOfDigits(string num, int dec)
        {
            Assert.AreEqual(dec, num.GetNumberOfDecimals(), num);
        }
    }
}
