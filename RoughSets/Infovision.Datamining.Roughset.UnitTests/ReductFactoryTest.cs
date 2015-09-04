using System;
using System.IO;
using System.Reflection;
using Infovision.Data;
using Infovision.Utils;
using NUnit.Framework;

namespace Infovision.Datamining.Roughset.UnitTests
{       
    [TestFixture]
    public class ReductFactoryTest
    {
        DataStore dataStoreTrain = null;
        DataStore dataStoreTest = null;

        public ReductFactoryTest()
        {
            string trainFileName = @"Data\monks-1.train";
            string testFileName = @"Data\monks-1.test";

            dataStoreTrain = DataStore.Load(trainFileName, FileFormat.Rses1);
            dataStoreTest = DataStore.Load(testFileName, FileFormat.Rses1, dataStoreTrain.DataStoreInfo);
        }

        [Test]
        public void CreateBireductTest()
        {
            Args args = new Args(new string[] { "FactoryKey", "DataStore", "NumberOfPermutations" }, 
                                  new Object[] { "Bireduct", dataStoreTrain, 100});

            BireductGenerator bireductGenerator = (BireductGenerator) ReductFactory.GetReductGenerator(args);
            bireductGenerator.Generate();
            IReductStore reductStore = bireductGenerator.ReductPool;
            Assert.AreEqual(true, true);
        }

        [Test]
        public void TestAssembly()
        {
            Assembly assembly = null;
            try
            {
                assembly = Assembly.Load("Infovision.TestAssembly");
            }
            catch (FileNotFoundException e)
            {
                Console.WriteLine(e.Message);
            }

            Type type = assembly.GetType("Infovision.TestAssembly.TestReductFactory");
            Assert.NotNull(type);
        }

        [Test]       
        public void TestFactoryRegister()
        {
            ReductFactory.Instance.RegisterFactory("Infovision.TestAssembly.TestReductFactory");
            Args parms = new Args(new string[] { "FactoryKey", "DataStore" }, new Object[] { "TestReduct", dataStoreTrain });
            IReductGenerator factory = ReductFactory.GetReductGenerator(parms);
            factory.Generate();
            IReductStore reductStore = factory.ReductPool;

            Assert.NotNull(reductStore);
        }        
    }
}
