using NRough.Core;
using NRough.Core.Helpers;
using NUnit.Framework;

namespace NRough.Tests.Core
{
    [TestFixture]
    internal class UtilsTest
    {
        [Test]
        public void IntArray2String()
        {
            int[] array = new int[] { 1, 2, 3, 4, 6, 7, 8, 10, 11, 12, 15, 16, 18, 19, 20, 21, 22, 23 };
            Assert.AreEqual("1..4 6..8 10..12 15 16 18..23", MiscHelper.IntArray2Ranges(array));

            array = new int[] { 1 };
            Assert.AreEqual("1", MiscHelper.IntArray2Ranges(array));

            array = new int[] { 1, 20 };
            Assert.AreEqual("1 20", MiscHelper.IntArray2Ranges(array));

            array = new int[] { 1, 2, 3, 4 };
            Assert.AreEqual("1..4", MiscHelper.IntArray2Ranges(array));
        }

        [Test]
        public void IsPowerOfTwo()
        {
            Assert.AreEqual(true, NRough.Core.Helpers.MiscHelper.IsPowerOfTwo(4));
            Assert.AreEqual(true, NRough.Core.Helpers.MiscHelper.IsPowerOfTwo(64));
            Assert.AreEqual(true, NRough.Core.Helpers.MiscHelper.IsPowerOfTwo(1024));
            Assert.AreEqual(true, NRough.Core.Helpers.MiscHelper.IsPowerOfTwo(4096));
            Assert.AreEqual(false, NRough.Core.Helpers.MiscHelper.IsPowerOfTwo(3));
            Assert.AreEqual(false, NRough.Core.Helpers.MiscHelper.IsPowerOfTwo(0));
            //Assert.AreEqual(false, NRough.Core.NRoughHelper.IsPowerOfTwo(9223372036854775809));
        }
    }
}