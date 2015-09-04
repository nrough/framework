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
            String trainFileName = @"monks-1.train";
            String testFileName = @"monks-1.test";

            dataStoreTrain = DataStore.Load(trainFileName, FileFormat.Rses1);
            dataStoreTest = DataStore.Load(testFileName, FileFormat.Rses1, dataStoreTrain.DataStoreInfo);
        }

        [Test]
        public void CreateBireductTest()
        {
            Args args = new Args(new String[] { "DataStore", "NumberOfPermutations" }, 
                                  new Object[] { dataStoreTrain, 100});

            BireductGenerator bireductGenerator = (BireductGenerator) ReductFactory.GetReductGenerator("Bireduct", args);
            IReductStore reductStore = bireductGenerator.Generate(args);                                                 
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
            Args parms = new Args(new String[] { "DataStore" }, new Object[] { dataStoreTrain });
            IReductGenerator factory = ReductFactory.GetReductGenerator("TestReduct", parms);
            IReductStore reductStore = factory.Generate(null);

            Assert.NotNull(reductStore);
        }        
    }
}
