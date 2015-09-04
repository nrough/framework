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
            string trainFileName = @"Data\dna.train";
            string testFileName = @"Data\dna.test";

            dataStoreTrain = DataStore.Load(trainFileName, FileFormat.Rses1);
            dataStoreTest = DataStore.Load(testFileName, FileFormat.Rses1, dataStoreTrain.DataStoreInfo);

            Args parms = new Args();
            parms.AddParameter("DataStore", dataStoreTrain);
            parms.AddParameter("NumberOfThreads", 1);
            parms.AddParameter("FactoryKey", "ApproximateReductRelative");

            IPermutationGenerator permGen = ReductFactory.GetPermutationGenerator(parms);
            parms.AddParameter("PermutationCollection", permGen.Generate(100));
            
            reductGenerator = ReductFactory.GetReductGenerator(parms);

            Args parmsMulti = new Args();
            parmsMulti.AddParameter("DataStore", dataStoreTrain);
            parmsMulti.AddParameter("NumberOfThreads", InfovisionHelper.NumberOfCores());
            parmsMulti.AddParameter("PermutationCollection", (PermutationCollection)parms.GetParameter("PermutationCollection"));
            parmsMulti.AddParameter("FactoryKey", "ApproximateReductRelative");

            reductGeneratorMulti = ReductFactory.GetReductGenerator(parmsMulti);
        }

        [Test]
        public void TestGolf([Values("GammaBireduct", "Bireduct", "BireductRelative")] string reductType)
        {
            bool showInfo = false;
            
            if (showInfo)
            {
                Console.WriteLine(reductType);
            }

            DataStore localDataStore = DataStore.Load(@"Data\playgolf.train", FileFormat.Rses1);

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
                args.AddParameter("ApproximationRatio", i / 100.0);
                IPermutationGenerator permGen = ReductFactory.GetReductFactory(reductType).GetPermutationGenerator(args);
                PermutationCollection permutations = permGen.Generate(100);

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

                    roughClassifier.Train(localDataStore, reductType, i, new PermutationCollection(permutations[j]));

                    foreach (IReduct reduct in roughClassifier.ReductStore)
                    {
                        if (showInfo)
                        {
                            Console.WriteLine(reduct);
                        }

                        EquivalenceClassMap partitionMap = new EquivalenceClassMap(localDataStore);
                        partitionMap.Calc(reduct.Attributes, localDataStore, reduct.ObjectSet, reduct.Weights);

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
            DataStore localDataStore = DataStore.Load(@"Data\playgolf.train", FileFormat.Rses1);
            RoughClassifier roughClassifier = new RoughClassifier();

            PermutationCollection permutationList = new PermutationCollection();
            permutationList.Add(new Permutation(new int[] { -3, 1, 6, 8, 0, -4, 12, 11, 5, 4, 7, 3, 10, 2, 13, 9, -1, -2 }));
            permutationList.Add(new Permutation(new int[] { 6, 10, -3, 4, -4, 11, 2, 12, 3, 8, 0, 9, 5, 13, 7, 1, -2, -1 }));
            permutationList.Add(new Permutation(new int[] { -4, -3, 9, 1, 7, 10, -1, 6, 0, 4, 8, 12, 11, -2, 3, 5, 2, 13 }));
            permutationList.Add(new Permutation(new int[] { 1, -3, 12, 8, 2, -4, 9, 13, 10, 11, 0, 3, 7, 4, 6, 5, -1, -2 }));
            permutationList.Add(new Permutation(new int[] { 0, 6, 2, 10, 3, 9, 5, -4, 8, 4, -2, 12, 11, -3, 1, 13, 7, -1 }));
            
            roughClassifier.Train(localDataStore, "Bireduct", 0, permutationList);
            foreach (IReduct reduct in roughClassifier.ReductStore)
            {
                EquivalenceClassMap partitionMap = new EquivalenceClassMap(localDataStore);
                partitionMap.Calc(reduct.Attributes, localDataStore, reduct.ObjectSet, reduct.Weights);

                foreach (EquivalenceClass stats in partitionMap)
                {
                    Assert.AreEqual(1, stats.NumberOfDecisions);
                }
            }
        }

        [Test]        
        public void IsSuperSetMultiThreadTiming()
        {
            ReductCache.Instance.Trim(100);
            int start = Environment.TickCount;

            for (int epsilon = 20; epsilon < 50; epsilon++)
            {
                reductGeneratorMulti.Epsilon = epsilon / 100.0;
                reductGeneratorMulti.Generate();
                IReductStore reductStore = reductGeneratorMulti.ReductPool;
            }

            int stop = Environment.TickCount;

            Console.WriteLine("Multi-thread timing is {0}", stop - start);
        }

        [Test]        
        public void IsSuperSetSingleTiming()
        {
            ReductCache.Instance.Trim(100);
            int start = Environment.TickCount;

            for (int epsilon = 20; epsilon < 50; epsilon++)
            {
                reductGenerator.Epsilon = epsilon / 100.0;
                reductGenerator.Generate();
                IReductStore reductStore = reductGenerator.ReductPool;
            }

            int stop = Environment.TickCount;

            Console.WriteLine("Single-thread timing is {0}", stop - start);
        }

        [Test]        
        public void ThreadPoolTest()
        {
            PascalSet[] resource = new PascalSet[10000];
            for (int i = 0; i < 10000; i++)
            {
                resource[i] = new PascalSet(0, 1000);
                for (int j = 0; j < 1000; j++)
                {
                    resource[i].AddElement(j);
                }
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

            for (int numberOfThreads = 1; numberOfThreads < 64; numberOfThreads++)
            {
                Console.WriteLine("{0}: {1}", numberOfThreads, results[numberOfThreads] / 100);
            }
        }

        private class WorkerTaskInfo
        {
            public WorkerTaskInfo(int threadIndex, int numberOfThreads, PascalSet[] array, ManualResetEvent resetEvent)
            {
                this.ThreadIndex = threadIndex;
                this.NumberOfThreads = numberOfThreads;
                this.Array = array;
                this.ResetEvent = resetEvent;
            }

            public int ThreadIndex { get; set; }
            public int NumberOfThreads { get; set; }
            public PascalSet[] Array { get; set; }
            public ManualResetEvent ResetEvent { get; set; }
        }

        public void ThreadPoolItemTest(Object obj)
        {
            WorkerTaskInfo task = (WorkerTaskInfo) obj;
            for (int i = 0; i < 10000; i += task.NumberOfThreads)
            {
                task.Array[task.ThreadIndex].Superset(task.Array[i]);
            }
            
            task.ResetEvent.Set();
            //resetEvents[task.ThreadIndex].Set();
        }
    }
}
