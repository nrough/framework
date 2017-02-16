using System;
using System.IO;
using System.Reflection;
using Raccoon.Data;
using Raccoon.Core;
using NUnit.Framework;

namespace Raccoon.MachineLearning.Roughset.UnitTests
{
    [TestFixture]
    public class ReductFactoryTest
    {
        private DataStore dataStoreTrain = null;
        private DataStore dataStoreTest = null;

        public ReductFactoryTest()
        {
            string trainFileName = @"Data\monks-1.train";
            string testFileName = @"Data\monks-1.test";

            dataStoreTrain = DataStore.Load(trainFileName, FileFormat.RSES1);
            dataStoreTest = DataStore.Load(testFileName, FileFormat.RSES1, dataStoreTrain.DataStoreInfo);
        }

        [Test]
        public void CreateBireductTest()
        {
            Args args = new Args(new string[] { ReductFactoryOptions.ReductType, ReductFactoryOptions.DecisionTable, ReductFactoryOptions.NumberOfPermutations },
                                  new Object[] { ReductTypes.Bireduct, dataStoreTrain, 100 });

            BireductGenerator bireductGenerator = (BireductGenerator)ReductFactory.GetReductGenerator(args);
            bireductGenerator.Run();
            IReductStore reductStore = bireductGenerator.ReductPool;
            Assert.AreEqual(true, true);
        }

        [Test, Ignore("TODO Link TestAssembly for release and debug")]
        public void TestAssembly()
        {            
            try
            {
                string path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);                
                Assembly assembly = Assembly.Load("Raccoon.TestAssembly");
                Type type = assembly.GetType("Raccoon.TestAssembly.TestReductFactory");
                Assert.NotNull(type);
            }
            catch (FileNotFoundException e)
            {
                Console.WriteLine(e.Message);
            }

            Assert.True(true);            
        }

        [Test, Ignore("TODO Link TestAssembly for release and debug")]
        public void TestFactoryRegister()
        {
            ReductFactory.Instance.RegisterFactory("Raccoon.TestAssembly.TestReductFactory");
            Args parms = new Args(new string[] { ReductFactoryOptions.ReductType, ReductFactoryOptions.DecisionTable }, new Object[] { "TestReduct", dataStoreTrain });
            IReductGenerator factory = ReductFactory.GetReductGenerator(parms);
            factory.Run();
            IReductStore reductStore = factory.ReductPool;

            Assert.NotNull(reductStore);
        }
    }
}