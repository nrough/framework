using NUnit.Framework;
using Raccoon.Data.Benchmark;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Raccoon.Data.Tests.Examples
{
    [TestFixture]
    public class FactoryTest
    {
        [Test]
        public void Dna()
        {
            DataStore train = Factory.Dna();
            Assert.IsNotNull(train);

            DataStore test = Factory.DnaTest();
            Assert.IsNotNull(test);
        }

        [Test]
        public void DnaModified()
        {
            DataStore train = Factory.DnaModified();
            Assert.IsNotNull(train);

            DataStore test = Factory.DnaModifiedTest();
            Assert.IsNotNull(test);
        }

        [Test]
        public void German()
        {
            DataStore data = Factory.German();
            Assert.IsNotNull(data);            
        }
    }
}
