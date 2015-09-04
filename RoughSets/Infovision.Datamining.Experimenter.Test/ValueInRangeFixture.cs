using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Infovision.Datamining.Experimenter.Parms;
using NUnit.Framework;

namespace Infovision.Datamining.Experimenter.Test
{
    [TestFixture]
    class ValueInRangeFixture
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
            IParameter parmA = new ParameterValueCollection<int>("A", new int[] {1, 2, 3, 4, 7, 8, 9});

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
