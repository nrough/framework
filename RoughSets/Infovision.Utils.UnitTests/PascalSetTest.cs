using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Infovision.Utils.UnitTests
{
    [TestFixture]
    class PascalSetTest
    {
        [Test]
        public void GetCardinalityTest()
        {
            PascalSet set1 = new PascalSet(0, 10, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 });
            PascalSet set2 = new PascalSet(0, 10, new int[] { 5, 6, 7, 8, 9 });

            Assert.AreEqual(10, set1.GetCardinality());
            Assert.AreEqual(5, set2.GetCardinality());

            PascalSet set3 = set1.Union(set2);
            Assert.AreEqual(10, set3.GetCardinality());

            set3 = set1 - 9;
            Assert.AreEqual(9, set3.GetCardinality());
            Assert.AreEqual(10, set1.GetCardinality());

            set3 = set1 - set2;
            Assert.AreEqual(5, set3.GetCardinality());

            set3 = set1.Intersection(set2);
            Assert.AreEqual(5, set3.GetCardinality());
        }
    }
}
