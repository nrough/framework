using NUnit.Framework;
using NRough.Data.Benchmark;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NRough.Data;
using System.Reflection;

namespace NRough.Tests.Data.Benchmark
{
    [TestFixture]
    public class FactoryTest
    {

        [Test]
        public void TestAll()
        {
            DataStore data = null;
            Type factoryType = typeof(Factory);
            var methods = factoryType.GetMethods(
                BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
            foreach (var method in methods)
            {
                if (method.GetParameters().Length == 0 
                    && !method.IsSpecialName)
                {
                    data = method.Invoke(null, null) as DataStore;
                    Assert.NotNull(data, method.Name);
                    Assert.Greater(data.NumberOfRecords, 0, method.Name);
                }
            }
        }

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
            DataStore train = Factory.DnaTrain();
            Assert.IsNotNull(train);

            Console.WriteLine(train.GetStandardFields().Count());

            DataStore test = Factory.DnaTest();
            Assert.IsNotNull(test);
        }

        [Test]
        public void DnaModified()
        {
            DataStore train = Factory.DnaModifiedTrain();
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

        [Test]
        public void Dermatology()
        {
            DataStore data = Factory.Dermatology();
            Assert.IsNotNull(data);

            Console.WriteLine(data.DataStoreInfo.DecisionInfo.Histogram.ToString());
        }

        [Test]
        public void Lymphography()
        {
            DataStore data = Factory.Lymphography();
            Assert.IsNotNull(data);

            Console.WriteLine("Number of attributes: {0}", data.GetStandardFields().Count());
            Console.WriteLine("Number of records: {0}", data.NumberOfRecords);
            Console.WriteLine("Class distribution {0}", data.DataStoreInfo.DecisionInfo.Histogram.ToString());

        }

        [Test]
        public void Sat()
        {
            DataStore data = Factory.SatDiscTrain();
            Assert.IsNotNull(data);

            Console.WriteLine("Number of attributes: {0}", data.GetStandardFields().Count());
            Console.WriteLine("Number of records: {0}", data.NumberOfRecords);
            Console.WriteLine("Class distribution {0}", data.DataStoreInfo.DecisionInfo.Histogram.ToString());

            DataStore test = Factory.SatDiscTest();
            Assert.IsNotNull(data);

            Console.WriteLine("Number of attributes: {0}", test.GetStandardFields().Count());
            Console.WriteLine("Number of records: {0}", test.NumberOfRecords);
            Console.WriteLine("Class distribution {0}", test.DataStoreInfo.DecisionInfo.Histogram.ToString());
        }

        [Test]
        public void VowelDisc()
        {
            DataStore data = Factory.VowelDiscTrain();
            Assert.IsNotNull(data);

            Console.WriteLine("Number of attributes: {0}", data.GetStandardFields().Count());
            Console.WriteLine("Number of records: {0}", data.NumberOfRecords);
            Console.WriteLine("Class distribution {0}", data.DataStoreInfo.DecisionInfo.Histogram.ToString());

            DataStore test = Factory.VowelDiscTest();
            Assert.IsNotNull(data);

            Console.WriteLine("Number of attributes: {0}", test.GetStandardFields().Count());
            Console.WriteLine("Number of records: {0}", test.NumberOfRecords);
            Console.WriteLine("Class distribution {0}", test.DataStoreInfo.DecisionInfo.Histogram.ToString());
        }

        [Test]
        public void Vowel()
        {
            DataStore data = Factory.VowelTrain();
            Assert.IsNotNull(data);

            Console.WriteLine("Number of attributes: {0}", data.GetStandardFields().Count());
            Console.WriteLine("Number of records: {0}", data.NumberOfRecords);
            Console.WriteLine("Class distribution {0}", data.DataStoreInfo.DecisionInfo.Histogram.ToString());

            DataStore test = Factory.VowelTest();
            Assert.IsNotNull(data);

            Console.WriteLine("Number of attributes: {0}", test.GetStandardFields().Count());
            Console.WriteLine("Number of records: {0}", test.NumberOfRecords);
            Console.WriteLine("Class distribution {0}", test.DataStoreInfo.DecisionInfo.Histogram.ToString());
        }
    }
}
