using NUnit.Framework;
using NRough.Data.Benchmark;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NRough.Data.Tests.Benchmark
{
    [TestFixture]
    public class FactoryTest
    {
        [Test]
        public void Mashroom()
        {
            DataStore train = Factory.Mashroom();
            Assert.IsNotNull(train);
            Assert.AreEqual(8124, train.NumberOfRecords);
            Assert.AreEqual(23, train.DataStoreInfo.NumberOfFields);
        }

        [Test]
        public void Audiolodgy()
        {
            DataStore train = Factory.Audiology();
            Assert.IsNotNull(train);
        }

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
