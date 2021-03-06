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
using System.Linq;
using System.Threading;
using NRough.Data;
using NRough.Core;
using NUnit.Framework;
using System.Collections.Generic;
using NRough.MachineLearning.Permutations;
using NRough.MachineLearning.Roughsets;
using NRough.MachineLearning;

namespace NRough.Tests.MachineLearning.Roughsets
{
    [TestFixture]
    internal class ReductGeneratorTest
    {
        private DataStore dataStoreTrain = null;
        private DataStore dataStoreTest = null;
        private IReductGenerator reductGenerator = null;
        private IReductGenerator reductGeneratorMulti = null;
        private Args parms = new Args();

        private ManualResetEvent[] resetEvents;

        public ReductGeneratorTest()
        {
            string trainFileName = @"Data\dna.train";
            string testFileName = @"Data\dna.test";

            dataStoreTrain = DataStore.Load(trainFileName, DataFormat.RSES1);
            dataStoreTest = DataStore.Load(testFileName, DataFormat.RSES1, dataStoreTrain.DataStoreInfo);

            Args parms = new Args();
            parms.SetParameter(ReductFactoryOptions.DecisionTable, dataStoreTrain);
            parms.SetParameter(ReductFactoryOptions.ReductType, ReductTypes.ApproximateReductRelative);

            IPermutationGenerator permGen = ReductFactory.GetPermutationGenerator(parms);
            parms.SetParameter(ReductFactoryOptions.PermutationCollection, permGen.Generate(10));

            reductGenerator = ReductFactory.GetReductGenerator(parms);

            Args parmsMulti = new Args();
            parmsMulti.SetParameter(ReductFactoryOptions.DecisionTable, dataStoreTrain);
            parmsMulti.SetParameter(ReductFactoryOptions.PermutationCollection, (PermutationCollection)parms.GetParameter(ReductFactoryOptions.PermutationCollection));
            parmsMulti.SetParameter(ReductFactoryOptions.ReductType, ReductTypes.ApproximateReductRelative);

            reductGeneratorMulti = ReductFactory.GetReductGenerator(parmsMulti);
        }

        [Test]
        public void TestGolf([Values("GammaBireduct", "Bireduct", "BireductRelative")] string factoryKey)
        {
            bool showInfo = false;

            //if (showInfo)
            //    Console.WriteLine(factoryKey);

            DataStore localDataStore = DataStore.Load(@"Data\playgolf.train", DataFormat.RSES1);

            localDataStore.DataStoreInfo.GetFieldInfo(1).Alias = "O";
            localDataStore.DataStoreInfo.GetFieldInfo(2).Alias = "T";
            localDataStore.DataStoreInfo.GetFieldInfo(3).Alias = "H";
            localDataStore.DataStoreInfo.GetFieldInfo(4).Alias = "W";

            for (int i = 0; i <= 100; i += 1)
            {
                //if (showInfo)
                //    Console.WriteLine("Epsilon: {0}", i);

                Args args = new Args();
                args.SetParameter(ReductFactoryOptions.DecisionTable, localDataStore);
                args.SetParameter(ReductFactoryOptions.Epsilon, (double)(i / 100.0));
                args.SetParameter(ReductFactoryOptions.ReductType, factoryKey);
                IPermutationGenerator permGen = ReductFactory.GetPermutationGenerator(args);
                PermutationCollection permutations = permGen.Generate(100);
                args.SetParameter(ReductFactoryOptions.PermutationCollection, permutations);

                for (int j = 0; j < 100; j++)
                {
                    bool first = true;
                    if (showInfo)
                    {
                        foreach (int permElement in permutations[j].ToArray())
                        {
                            if (!first)
                            {
                                Console.Write(" ");
                            }
                            if (permElement < 0)
                            {
                                Console.Write("{0}", localDataStore.DataStoreInfo.GetFieldInfo(-permElement).Alias);
                            }
                            else
                            {
                                Console.Write("{0}", permElement + 1);
                            }
                            first = false;
                        }
                        Console.Write(" & ");
                    }

                    Args parms = new Args();
                    parms.SetParameter(ReductFactoryOptions.DecisionTable, localDataStore);
                    parms.SetParameter(ReductFactoryOptions.Epsilon, i / 100.0);
                    parms.SetParameter(ReductFactoryOptions.PermutationCollection, new PermutationCollection(permutations[j]));
                    parms.SetParameter(ReductFactoryOptions.ReductType, factoryKey);

                    IReductGenerator reductGenerator = ReductFactory.GetReductGenerator(parms);
                    reductGenerator.Run();

                    IReductStoreCollection reductStoreCollction = reductGenerator.GetReductStoreCollection();
                    IReductStore reductStore = reductStoreCollction.FirstOrDefault();
                    foreach (IReduct reduct in reductStore)
                    {
                        //if (showInfo)
                        //    Console.WriteLine(reduct);

                        EquivalenceClassCollection partitionMap = new EquivalenceClassCollection(localDataStore);
                        partitionMap.Calc(reduct.Attributes.ToArray(), localDataStore, reduct.SupportedObjects.ToArray(), reduct.Weights);

                        foreach (EquivalenceClass stats in partitionMap)
                        {
                            Assert.AreEqual(1, stats.NumberOfDecisions);
                        }
                    }
                }
            }
        }

        [Test]
        public void TestBireductGolf_2()
        {
            DataStore localDataStore = DataStore.Load(@"Data\playgolf.train", DataFormat.RSES1);

            PermutationCollection permutationList = new PermutationCollection();
            permutationList.Add(new Permutation(new int[] { -3, 1, 6, 8, 0, -4, 12, 11, 5, 4, 7, 3, 10, 2, 13, 9, -1, -2 }));
            permutationList.Add(new Permutation(new int[] { 6, 10, -3, 4, -4, 11, 2, 12, 3, 8, 0, 9, 5, 13, 7, 1, -2, -1 }));
            permutationList.Add(new Permutation(new int[] { -4, -3, 9, 1, 7, 10, -1, 6, 0, 4, 8, 12, 11, -2, 3, 5, 2, 13 }));
            permutationList.Add(new Permutation(new int[] { 1, -3, 12, 8, 2, -4, 9, 13, 10, 11, 0, 3, 7, 4, 6, 5, -1, -2 }));
            permutationList.Add(new Permutation(new int[] { 0, 6, 2, 10, 3, 9, 5, -4, 8, 4, -2, 12, 11, -3, 1, 13, 7, -1 }));

            Args args = new Args();
            args.SetParameter(ReductFactoryOptions.DecisionTable, localDataStore);
            args.SetParameter(ReductFactoryOptions.Epsilon, 0.0);
            args.SetParameter(ReductFactoryOptions.PermutationCollection, permutationList);
            args.SetParameter(ReductFactoryOptions.ReductType, ReductTypes.Bireduct);

            IReductGenerator reductGenerator = ReductFactory.GetReductGenerator(args);
            reductGenerator.Run();

            IReductStore reductStore = reductGenerator.GetReductStoreCollection().FirstOrDefault();

            foreach (IReduct reduct in reductStore)
            {
                EquivalenceClassCollection eqClasses = new EquivalenceClassCollection(localDataStore);
                eqClasses.Calc(reduct.Attributes.ToArray(), localDataStore, reduct.SupportedObjects.ToArray(), reduct.Weights);

                foreach (EquivalenceClass eq in eqClasses)
                {
                    Assert.AreEqual(1, eq.NumberOfDecisions);
                }
            }
        }

        [Test, Ignore("NoReason")]
        public void IsSuperSetMultiThreadTiming()
        {
            ReductCache.Instance.Trim(100);
            for (int epsilon = 20; epsilon < 50; epsilon++)
            {
                reductGeneratorMulti.Epsilon = epsilon / 100.0;
                reductGeneratorMulti.Run();
                IReductStore reductStore = reductGeneratorMulti.ReductPool;
            }
        }

        [Test, Ignore("NoReason")]
        public void IsSuperSetSingleTiming()
        {
            ReductCache.Instance.Trim(100);
            for (int epsilon = 20; epsilon < 50; epsilon++)
            {
                reductGenerator.Epsilon = epsilon / 100.0;
                reductGenerator.Run();
                IReductStore reductStore = reductGenerator.ReductPool;
            }
        }

        [Test, Ignore("NoReason")]
        public void ThreadPoolTest()
        {
            HashSet<int>[] resource = new HashSet<int>[10000];
            for (int i = 0; i < 10000; i++)
            {
                resource[i] = new HashSet<int>(Enumerable.Range(0,10000));                
            }

            long[] results = new long[64];

            for (int t = 0; t < 100; t++)
            {
                for (int numberOfThreads = 1; numberOfThreads < 64; numberOfThreads++)
                {
                    resetEvents = new ManualResetEvent[numberOfThreads];

                    int start = Environment.TickCount;

                    for (int i = 0; i < numberOfThreads; i++)
                    {
                        resetEvents[i] = new ManualResetEvent(false);
                        ThreadPool.QueueUserWorkItem(new WaitCallback(this.ThreadPoolItemTest),
                                                     new WorkerTaskInfo(i, numberOfThreads, resource, resetEvents[i]));
                    }

                    ManualResetEvent.WaitAll(resetEvents);

                    int stop = Environment.TickCount;

                    results[numberOfThreads] += (stop - start);
                }
            }

            //for (int numberOfThreads = 1; numberOfThreads < 64; numberOfThreads++)
            //    Console.WriteLine("{0}: {1}", numberOfThreads, results[numberOfThreads] / 100);
        }

        private class WorkerTaskInfo
        {
            public WorkerTaskInfo(int threadIndex, int numberOfThreads, HashSet<int>[] array, ManualResetEvent resetEvent)
            {
                this.ThreadIndex = threadIndex;
                this.NumberOfThreads = numberOfThreads;
                this.Array = array;
                this.ResetEvent = resetEvent;
            }

            public int ThreadIndex { get; set; }
            public int NumberOfThreads { get; set; }
            public HashSet<int>[] Array { get; set; }
            public ManualResetEvent ResetEvent { get; set; }
        }

        public void ThreadPoolItemTest(Object obj)
        {
            WorkerTaskInfo task = (WorkerTaskInfo)obj;
            for (int i = 0; i < 10000; i += task.NumberOfThreads)
            {
                task.Array[task.ThreadIndex].IsSupersetOf(task.Array[i]);
            }

            task.ResetEvent.Set();
            //resetEvents[task.ThreadIndex].Set();
        }
    }
}