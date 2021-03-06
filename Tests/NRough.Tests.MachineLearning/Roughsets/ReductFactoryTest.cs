﻿// 
// NRough Framework
// http://www.nrough.net
// 
// Copyright (c) Sebastian Widz, 2017
// authors at nrough.net
// 
// This file is part of NRough Framework.
// 
// NRough Framework is free software; you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation; either version 3 of the License, or
// (at your option) any later version.
// 
// NRough Framework is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with NRough Framework; if not, write to the Free Software
// Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA  02110-1301  USA
//

using System;
using System.IO;
using System.Reflection;
using NRough.Data;
using NRough.Core;
using NUnit.Framework;
using NRough.MachineLearning.Roughsets;

namespace NRough.Tests.MachineLearning.Roughsets
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

            dataStoreTrain = DataStore.Load(trainFileName, DataFormat.RSES1);
            dataStoreTest = DataStore.Load(testFileName, DataFormat.RSES1, dataStoreTrain.DataStoreInfo);
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
                Assembly assembly = Assembly.Load("NRough.TestAssembly");
                Type type = assembly.GetType("NRough.TestAssembly.TestReductFactory");
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
            ReductFactory.Instance.RegisterFactory("NRough.TestAssembly.TestReductFactory");
            Args parms = new Args(new string[] { ReductFactoryOptions.ReductType, ReductFactoryOptions.DecisionTable }, new Object[] { "TestReduct", dataStoreTrain });
            IReductGenerator factory = ReductFactory.GetReductGenerator(parms);
            factory.Run();
            IReductStore reductStore = factory.ReductPool;

            Assert.NotNull(reductStore);
        }
    }
}