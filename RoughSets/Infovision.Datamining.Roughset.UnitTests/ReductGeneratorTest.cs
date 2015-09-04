using System;
using System.Threading;
using Infovision.Data;
using Infovision.Utils;
using NUnit.Framework;

namespace Infovision.Datamining.Roughset.UnitTests
{
    [TestFixture]
    class ReductGeneratorTest
    {
        private DataStore dataStoreTrain = null;
        private DataStore dataStoreTest = null;
        private IReductGenerator reductGenerator = null;
        private IReductGenerator reductGeneratorMulti = null;
        private Args parms = new Args();

        ManualResetEvent[] resetEvents;

        public ReductGeneratorTest()
        {
            string trainFileName = @"dna.train";
            string testFileName = @"dna.test";

            dataStoreTrain = DataStore.Load(trainFileName, FileFormat.Rses1);
            dataStoreTest = DataStore.Load(testFileName, FileFormat.Rses1, dataStoreTrain.DataStoreInfo);

            Args parms = new Args();
            parms.AddParameter("DataStore", dataStoreTrain);
            parms.AddParameter("NumberOfThreads", 1);

            IPermutationGenerator permGen = ReductFactory.GetPermutationGenerator("ApproximateReductRelative", parms);
            parms.AddParameter("PermutationList", permGen.Generate(100));
            
            reductGenerator = ReductFactory.GetReductGenerator("ApproximateReductRelative", parms);

            Args parmsMulti = new Args();
            parmsMulti.AddParameter("DataStore", dataStoreTrain);
            parmsMulti.AddParameter("NumberOfThreads", InfovisionHelper.NumberOfCores());
            parmsMulti.AddParameter("PermutationList", (PermutationList)parms.GetParameter("PermutationList"));

            reductGeneratorMulti = ReductFactory.GetReductGenerator("ApproximateReductRelative", parmsMulti);
        }

        [Test]
        public void TestGolf([Values("GammaBireduct", "Bireduct", "BireductRelative")] String reductType)
        {
            Boolean showInfo = false;
            
            if (showInfo)
            {
                Console.WriteLine(reductType);
            }

            DataStore localDataStore = DataStore.Load(@"playgolf.train", FileFormat.Rses1);

            localDataStore.DataStoreInfo.GetFieldInfo(1).NameAlias = "O";
            localDataStore.DataStoreInfo.GetFieldInfo(2).NameAlias = "T";
            localDataStore.DataStoreInfo.GetFieldInfo(3).NameAlias = "H";
            localDataStore.DataStoreInfo.GetFieldInfo(4).NameAlias = "W";

            RoughClassifier roughClassifier = new RoughClassifier();

            for (int i = 0; i <= 100; i += 1)
            {
                if (showInfo)
                {
                    Console.WriteLine("Epsilon: {0}", i);
                }

                Args args = new Args();
                args.AddParameter("DataStore", localDataStore);
                args.AddParameter("ApproximationRatio", i);
                IPermutationGenerator permGen = ReductFactory.GetReductFactory(reductType).GetPermutationGenerator(args);
                PermutationList permutations = permGen.Generate(100);

                for (int j = 0; j < 100; j++)
                {
                    Boolean first = true;
                    if (showInfo)
                    {
                        foreach (Int32 permElement in permutations[j].ToArray())
                        {
                            if (!first)
                            {
                                Console.Write(" ");
                            }
                            if (permElement < 0)
                            {
                                Console.Write("{0}", localDataStore.DataStoreInfo.GetFieldInfo(-permElement).NameAlias);
                            }
                            else
                            {
                                Console.Write("{0}", permElement + 1);
                            }
                            first = false;
                        }
                        Console.Write(" & ");
                    }

                    roughClassifier.Train(localDataStore, reductType, i, new PermutationList(permutations[j]));

                    foreach (IReduct reduct in roughClassifier.ReductStore)
                    {
                        if (showInfo)
                        {
                            Console.WriteLine(reduct);
                        }

                        EquivalenceClassMap partitionMap = new EquivalenceClassMap(localDataStore.DataStoreInfo);
                        partitionMap.Calc(reduct.AttributeSet, localDataStore, reduct.ObjectSet);

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
            DataStore localDataStore = DataStore.Load(@"playgolf.train", FileFormat.Rses1);
            RoughClassifier roughClassifier = new RoughClassifier();

            PermutationList permutationList = new PermutationList();
            permutationList.Add(new Permutation(new Int32[] { -3, 1, 6, 8, 0, -4, 12, 11, 5, 4, 7, 3, 10, 2, 13, 9, -1, -2 }));
            permutationList.Add(new Permutation(new Int32[] { 6, 10, -3, 4, -4, 11, 2, 12, 3, 8, 0, 9, 5, 13, 7, 1, -2, -1 }));
            permutationList.Add(new Permutation(new Int32[] { -4, -3, 9, 1, 7, 10, -1, 6, 0, 4, 8, 12, 11, -2, 3, 5, 2, 13 }));
            permutationList.Add(new Permutation(new Int32[] { 1, -3, 12, 8, 2, -4, 9, 13, 10, 11, 0, 3, 7, 4, 6, 5, -1, -2 }));
            permutationList.Add(new Permutation(new Int32[] { 0, 6, 2, 10, 3, 9, 5, -4, 8, 4, -2, 12, 11, -3, 1, 13, 7, -1 }));
            
            roughClassifier.Train(localDataStore, "Bireduct", 0, permutationList);
            foreach (IReduct reduct in roughClassifier.ReductStore)
            {
                EquivalenceClassMap partitionMap = new EquivalenceClassMap(localDataStore.DataStoreInfo);
                partitionMap.Calc(reduct.AttributeSet, localDataStore, reduct.ObjectSet);

                foreach (EquivalenceClass stats in partitionMap)
                {
                    Assert.AreEqual(1, stats.NumberOfDecisions);
                }
            }
        }

        [Test]
        [Ignore]
        public void IsSuperSetMultiThreadTiming()
        {
            ReductCache.Instance.Trim(100);
            Int32 start = Environment.TickCount;

            for (Int32 epsilon = 20; epsilon < 50; epsilon++)
            {
                reductGeneratorMulti.ApproximationLevel = (Double)epsilon / (Double)100;
                IReductStore reductStore = reductGeneratorMulti.Generate(parms);
            }

            Int32 stop = Environment.TickCount;

            Console.WriteLine("Multi-thread timing is {0}", stop - start);
        }

        [Test]
        [Ignore]
        public void IsSuperSetSingleTiming()
        {
            ReductCache.Instance.Trim(100);
            Int32 start = Environment.TickCount;

            for (Int32 epsilon = 20; epsilon < 50; epsilon++)
            {
                reductGenerator.ApproximationLevel = (double)epsilon / (double)100;
                IReductStore reductStore = reductGenerator.Generate(parms);
            }

            Int32 stop = Environment.TickCount;

            Console.WriteLine("Single-thread timing is {0}", stop - start);
        }

        [Test]
        [Ignore]
        public void ThreadPoolTest()
        {
            PascalSet[] resource = new PascalSet[10000];
            for (Int32 i = 0; i < 10000; i++)
            {
                resource[i] = new PascalSet(0, 1000);
                for (Int32 j = 0; j < 1000; j++)
                {
                    resource[i].AddElement(j);
                }
            }

            Int64[] results = new Int64[64];

            for (Int32 t = 0; t < 100; t++)
            {
                for (Int32 numberOfThreads = 1; numberOfThreads < 64; numberOfThreads++)
                {
                    resetEvents = new ManualResetEvent[numberOfThreads];

                    Int32 start = Environment.TickCount;

                    for (Int32 i = 0; i < numberOfThreads; i++)
                    {
                        resetEvents[i] = new ManualResetEvent(false);
                        ThreadPool.QueueUserWorkItem(new WaitCallback(this.ThreadPoolItemTest),
                                                     new WorkerTaskInfo(i, numberOfThreads, resource, resetEvents[i]));
                    }

                    ManualResetEvent.WaitAll(resetEvents);

                    Int32 stop = Environment.TickCount;

                    results[numberOfThreads] += (stop - start);
                }
            }

            for (Int32 numberOfThreads = 1; numberOfThreads < 64; numberOfThreads++)
            {
                Console.WriteLine("{0}: {1}", numberOfThreads, results[numberOfThreads] / 100);
            }
        }

        private class WorkerTaskInfo
        {
            public WorkerTaskInfo(Int32 threadIndex, Int32 numberOfThreads, PascalSet[] array, ManualResetEvent resetEvent)
            {
                this.ThreadIndex = threadIndex;
                this.NumberOfThreads = numberOfThreads;
                this.Array = array;
                this.ResetEvent = resetEvent;
            }

            public Int32 ThreadIndex { get; set; }
            public Int32 NumberOfThreads { get; set; }
            public PascalSet[] Array { get; set; }
            public ManualResetEvent ResetEvent { get; set; }
        }

        public void ThreadPoolItemTest(Object obj)
        {
            WorkerTaskInfo task = (WorkerTaskInfo) obj;   
            for (Int32 i = 0; i < 10000; i += task.NumberOfThreads)
            {
                task.Array[task.ThreadIndex].Superset(task.Array[i]);
            }
            
            task.ResetEvent.Set();
            //resetEvents[task.ThreadIndex].Set();
        }
    }
}
