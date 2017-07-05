// 
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
using NRough.Data;
using NRough.Core;
using NUnit.Framework;
using System.Collections.Generic;
using System.Diagnostics;
using NRough.MachineLearning.Weighting;
using NRough.MachineLearning.Permutations;
using NRough.MachineLearning.Classification;
using NRough.MachineLearning.Roughsets;
using NRough.MachineLearning;
using NRough.Core.Comparers;

namespace NRough.Tests.MachineLearning.Roughsets
{
    [TestFixture]
    public class WeightGeneratorTest
    {
        [Test]
        public void BalancedAccuracy()
        {
            var localDataStore = Data.Benchmark.Factory.DnaModifiedTrain();
            //string localFileName = @"Data\dna_modified.trn";
            //DataStore localDataStore = DataStore.Load(localFileName, DataFormat.RSES1);

            Args args = new Args();
            args.SetParameter(ReductFactoryOptions.DecisionTable, localDataStore);
            args.SetParameter(ReductFactoryOptions.Epsilon, 0.5);
            args.SetParameter(ReductFactoryOptions.ReductType, ReductTypes.ApproximateReductRelative);
            args.SetParameter(ReductFactoryOptions.PermutationCollection, ReductFactory.GetPermutationGenerator(args).Generate(10));

            IReductGenerator reductGenerator = ReductFactory.GetReductGenerator(args);
            reductGenerator.Run();

            RoughClassifier classifier = new RoughClassifier(
                reductGenerator.GetReductStoreCollection(),
                RuleQualityMethods.Confidence,
                RuleQualityMethods.SingleVote,
                localDataStore.DataStoreInfo.GetDecisionValues());

            string localFileNameTest = @"Data\dna_modified.tst";
            DataStore dataStoreTest = DataStore.Load(localFileNameTest, DataFormat.RSES1);

            ClassificationResult classificationResult = classifier.Classify(dataStoreTest, null);

            Assert.AreEqual(dataStoreTest.NumberOfRecords, classificationResult.Count);

            Assert.AreEqual(classificationResult.Count, classificationResult.Classified
                                                 + classificationResult.Misclassified
                                                 + classificationResult.Unclassified);


            DecisionDistribution aprioriDist = EquivalenceClassCollection.Create(new int[] { }, dataStoreTest).DecisionDistribution;

            double total = 0;
            double aprioriSum = 0;
            foreach (long decision in dataStoreTest.DataStoreInfo.GetDecisionValues())
            {
                aprioriSum += aprioriDist[decision];
                total += classificationResult.DecisionTotal(decision);
            }

            Assert.That(1.0, Is.EqualTo(aprioriSum).Using((IComparer<double>)ToleranceDoubleComparer.Instance));
            Assert.AreEqual(dataStoreTest.NumberOfRecords, total);

            Assert.That(classificationResult.BalancedAccuracy, Is.LessThanOrEqualTo(1.0).Using(ToleranceDoubleComparer.Instance));
            Assert.That(classificationResult.BalancedAccuracy, Is.GreaterThanOrEqualTo(0.0).Using(ToleranceDoubleComparer.Instance));            
        }

        [Test]
        public void CompareMajorityWeightVsMajority()
        {
            string localFileName = @"Data\dna.train";
            DataStore localDataStore = DataStore.Load(localFileName, DataFormat.RSES1);

            IReductGenerator redGenStd = new ReductGeneratorMajority();
            redGenStd.Epsilon = 0.1;

            IReductGenerator redGenWgh = new ReductGeneratorWeightsMajority();
            redGenWgh.Epsilon = 0.1;

            Args args = new Args(
                new string[] {
                    ReductFactoryOptions.ReductType,
                    ReductFactoryOptions.DecisionTable
                },
                new object[] {
                    ReductTypes.ApproximateReductMajority,
                    localDataStore
                }
            );

            IPermutationGenerator permGen = ReductFactory.GetPermutationGenerator(args);
            PermutationCollection permutationList = permGen.Generate(20);
            args.SetParameter(ReductFactoryOptions.PermutationCollection, permutationList);

            redGenStd.InitFromArgs(args);
            redGenStd.Run();
            IReductStore redStore = redGenStd.ReductPool;

            redGenWgh.InitFromArgs(args);
            redGenWgh.Run();
            IReductStore redStoreW = redGenWgh.ReductPool;

            Assert.IsTrue(CompareReductStores(redStore, redStoreW));            
        }

        [Test]
        public void CompareRelativeWeightVsRelative()
        {
            PermutationCollection permutationList = new PermutationCollection();
            permutationList.Add(new Permutation(new int[] { 34, 54, 4, 27, 29, 39, 33, 35, 10, 30, 36, 13, 49, 38, 8, 42, 1, 41, 19, 3, 7, 25, 2, 43, 31, 44, 14, 18, 32, 46, 48, 20, 17, 16, 55, 52, 53, 47, 45, 58, 28, 57, 6, 11, 26, 64, 12, 15, 59, 5, 51, 23, 61, 22, 62, 24, 40, 21, 63, 60, 37, 9, 50, 56 }));
            permutationList.Add(new Permutation(new int[] { 56, 50, 9, 37, 60, 63, 21, 40, 24, 62, 22, 61, 23, 51, 5, 59, 15, 12, 64, 26, 11, 6, 57, 28, 58, 45, 47, 53, 52, 55, 16, 17, 20, 48, 46, 32, 18, 14, 44, 31, 43, 2, 25, 7, 3, 19, 41, 1, 42, 8, 38, 49, 13, 36, 30, 10, 35, 33, 39, 29, 27, 4, 54, 34 }));
            permutationList.Add(new Permutation(new int[] { 38, 18, 48, 13, 59, 5, 52, 23, 16, 53, 9, 37, 33, 57, 42, 1, 56, 47, 14, 22, 34, 29, 19, 12, 11, 27, 43, 30, 24, 39, 51, 20, 17, 2, 40, 50, 28, 21, 4, 55, 44, 60, 10, 32, 58, 35, 8, 45, 62, 54, 49, 41, 46, 6, 7, 36, 25, 3, 63, 61, 31, 15, 64, 26 }));
            permutationList.Add(new Permutation(new int[] { 26, 64, 15, 31, 61, 63, 3, 25, 36, 7, 6, 46, 41, 49, 54, 62, 45, 8, 35, 58, 32, 10, 60, 44, 55, 4, 21, 28, 50, 40, 2, 17, 20, 51, 39, 24, 30, 43, 27, 11, 12, 19, 29, 34, 22, 14, 47, 56, 1, 42, 57, 33, 37, 9, 53, 16, 23, 52, 5, 59, 13, 48, 18, 38 }));
            permutationList.Add(new Permutation(new int[] { 57, 1, 34, 12, 23, 29, 45, 38, 5, 11, 4, 26, 33, 58, 43, 61, 42, 8, 53, 47, 36, 13, 50, 15, 40, 25, 30, 19, 55, 46, 22, 7, 14, 21, 27, 16, 49, 24, 9, 37, 51, 64, 20, 54, 32, 62, 28, 6, 59, 63, 48, 2, 17, 44, 56, 35, 52, 10, 60, 3, 31, 39, 18, 41 }));

            string localFileName = @"Data\optdigits.trn";
            DataStore localDataStore = DataStore.Load(localFileName, DataFormat.RSES1);

            Args args = new Args();
            args.SetParameter(ReductFactoryOptions.ReductType, ReductTypes.ApproximateReductRelative);
            args.SetParameter(ReductFactoryOptions.DecisionTable, localDataStore);
            args.SetParameter(ReductFactoryOptions.Epsilon, 0.1);
            args.SetParameter(ReductFactoryOptions.PermutationCollection, permutationList);

            IReductGenerator redGenStd = ReductFactory.GetReductGenerator(args);
            redGenStd.Run();

            args.SetParameter(ReductFactoryOptions.ReductType, ReductTypes.ApproximateReductRelativeWeights);

            IReductGenerator redGenWgh = ReductFactory.GetReductGenerator(args);
            redGenWgh.Run();

            IReductStore redStore = redGenStd.GetReductStoreCollection().FirstOrDefault();
            IReductStore redStoreW = redGenWgh.GetReductStoreCollection().FirstOrDefault();

            Assert.IsTrue(CompareReductStores(redStore, redStoreW));            
        }

        [Test]
        public void WeightReductRelative2()
        {
            string localFileName = @"Data\dna.train";
            DataStore localDataStore = DataStore.Load(localFileName, DataFormat.RSES1);

            PermutationGenerator permGen = new PermutationGenerator(localDataStore);
            PermutationCollection permutationList = permGen.Generate(5);

            Args args = new Args();
            args.SetParameter(ReductFactoryOptions.DecisionTable, localDataStore);
            args.SetParameter(ReductFactoryOptions.Epsilon, 0.2);
            args.SetParameter(ReductFactoryOptions.PermutationCollection, permutationList);
            args.SetParameter(ReductFactoryOptions.ReductType, ReductTypes.ApproximateReductRelative);

            IReductGenerator reductGenerator = ReductFactory.GetReductGenerator(args);
            reductGenerator.Run();
            IReductStore reductStore = reductGenerator.GetReductStoreCollection().FirstOrDefault();

            Args args2 = new Args();
            args2.SetParameter(ReductFactoryOptions.DecisionTable, localDataStore);
            args2.SetParameter(ReductFactoryOptions.Epsilon, 0.2);
            args2.SetParameter(ReductFactoryOptions.PermutationCollection, permutationList);
            args2.SetParameter(ReductFactoryOptions.ReductType, ReductTypes.ApproximateReductRelativeWeights);

            IReductGenerator reductGenerator2 = ReductFactory.GetReductGenerator(args2);
            reductGenerator2.Run();
            IReductStore reductStore2 = reductGenerator2.GetReductStoreCollection().FirstOrDefault();

            Assert.IsTrue(CompareReductStores(reductStore, reductStore2));            
        }

        [Test]
        public void WeightReductMajority2()
        {
            string localFileName = @"Data\dna.train";
            DataStore localDataStore = DataStore.Load(localFileName, DataFormat.RSES1);

            PermutationGenerator permGen = new PermutationGenerator(localDataStore);
            PermutationCollection permutationList = permGen.Generate(5);

            Args args = new Args();
            args.SetParameter(ReductFactoryOptions.DecisionTable, localDataStore);
            args.SetParameter(ReductFactoryOptions.Epsilon, 0.2);
            args.SetParameter(ReductFactoryOptions.PermutationCollection, permutationList);
            args.SetParameter(ReductFactoryOptions.ReductType, ReductTypes.ApproximateReductMajority);

            IReductGenerator reductGenerator = ReductFactory.GetReductGenerator(args);
            reductGenerator.Run();
            IReductStore reductStore = reductGenerator.GetReductStoreCollection().FirstOrDefault();

            Args args2 = new Args();
            args2.SetParameter(ReductFactoryOptions.DecisionTable, localDataStore);
            args2.SetParameter(ReductFactoryOptions.Epsilon, 0.2);
            args2.SetParameter(ReductFactoryOptions.PermutationCollection, permutationList);
            args2.SetParameter(ReductFactoryOptions.ReductType, ReductTypes.ApproximateReductMajorityWeights);

            IReductGenerator reductGenerator2 = ReductFactory.GetReductGenerator(args2);
            reductGenerator2.Run();
            IReductStore reductStore2 = reductGenerator2.GetReductStoreCollection().FirstOrDefault();

            Assert.IsTrue(CompareReductStores(reductStore, reductStore2));            
        }

        [Test]
        public void WeightReductRelative()
        {
            string fileName = @"Data\playgolf.train";
            DataStore dataStore = DataStore.Load(fileName, DataFormat.RSES1);

            PermutationCollection permutationList = new PermutationCollection();
            permutationList.Add(new Permutation(new int[] { 3, 4, 1, 2 }));
            permutationList.Add(new Permutation(new int[] { 3, 4, 2, 1 }));
            permutationList.Add(new Permutation(new int[] { 4, 3, 1, 2 }));
            permutationList.Add(new Permutation(new int[] { 3, 4, 1, 2 }));
            permutationList.Add(new Permutation(new int[] { 4, 2, 3, 1 }));

            Args args = new Args();
            args.SetParameter(ReductFactoryOptions.DecisionTable, dataStore);
            args.SetParameter(ReductFactoryOptions.Epsilon, 0.0);
            args.SetParameter(ReductFactoryOptions.PermutationCollection, permutationList);
            args.SetParameter(ReductFactoryOptions.ReductType, ReductTypes.ApproximateReductRelative);

            IReductGenerator reductGenerator = ReductFactory.GetReductGenerator(args);
            reductGenerator.Run();
            IReductStore reductStore = reductGenerator.GetReductStoreCollection().FirstOrDefault();

            Args args2 = new Args();
            args2.SetParameter(ReductFactoryOptions.DecisionTable, dataStore);
            args2.SetParameter(ReductFactoryOptions.Epsilon, 0.0);
            args2.SetParameter(ReductFactoryOptions.PermutationCollection, permutationList);
            args2.SetParameter(ReductFactoryOptions.ReductType, ReductTypes.ApproximateReductRelativeWeights);

            IReductGenerator reductGenerator2 = ReductFactory.GetReductGenerator(args2);
            reductGenerator2.Run();
            IReductStore reductStore2 = reductGenerator2.GetReductStoreCollection().FirstOrDefault();

            Assert.IsTrue(CompareReductStores(reductStore, reductStore2));                        
        }

        [Test]
        public void WeightReductMajority()
        {
            string fileName = @"Data\playgolf.train";
            DataStore dataStore = DataStore.Load(fileName, DataFormat.RSES1);

            PermutationCollection permutationList = new PermutationCollection();
            permutationList.Add(new Permutation(new int[] { 3, 4, 1, 2 }));
            permutationList.Add(new Permutation(new int[] { 3, 4, 2, 1 }));
            permutationList.Add(new Permutation(new int[] { 4, 3, 1, 2 }));
            permutationList.Add(new Permutation(new int[] { 3, 4, 1, 2 }));
            permutationList.Add(new Permutation(new int[] { 4, 2, 3, 1 }));

            Args args = new Args();
            args.SetParameter(ReductFactoryOptions.DecisionTable, dataStore);
            args.SetParameter(ReductFactoryOptions.Epsilon, 0.0);
            args.SetParameter(ReductFactoryOptions.PermutationCollection, permutationList);
            args.SetParameter(ReductFactoryOptions.ReductType, ReductTypes.ApproximateReductMajority);

            IReductGenerator reductGenerator = ReductFactory.GetReductGenerator(args);
            reductGenerator.Run();
            IReductStore reductStore = reductGenerator.GetReductStoreCollection().FirstOrDefault();

            Args args2 = new Args();
            args2.SetParameter(ReductFactoryOptions.DecisionTable, dataStore);
            args2.SetParameter(ReductFactoryOptions.Epsilon, 0.0);
            args2.SetParameter(ReductFactoryOptions.PermutationCollection, permutationList);
            args2.SetParameter(ReductFactoryOptions.ReductType, ReductTypes.ApproximateReductMajorityWeights);

            IReductGenerator reductGenerator2 = ReductFactory.GetReductGenerator(args2);
            reductGenerator2.Run();
            IReductStore reductStore2 = reductGenerator2.GetReductStoreCollection().FirstOrDefault();

            Assert.IsTrue(CompareReductStores(reductStore, reductStore2));                       
        }

        [Test]
        public void WeightEqualConstructorTest()
        {
            string fileName = @"Data\playgolf.train";
            DataStore dataStore = DataStore.Load(fileName, DataFormat.RSES1);

            WeightGenerator weightGenerator = new WeightGeneratorMajority(dataStore);
            Assert.IsNotNull(weightGenerator);
        }

        [Test]
        public void WeightRelativeConstructorTest()
        {
            string fileName = @"Data\playgolf.train";
            DataStore dataStore = DataStore.Load(fileName, DataFormat.RSES1);

            WeightGenerator weightGenerator = new WeightGeneratorRelative(dataStore);
            Assert.IsNotNull(weightGenerator);
        }

        private void TestWeightsNormalized(WeightGenerator weightGenerator)
        {
            string fileName = @"Data\playgolf.train";
            DataStore dataStore = DataStore.Load(fileName, DataFormat.RSES1);
            double weightSum = 0;
            for (int i = 0; i < dataStore.NumberOfRecords; i++)
            {
                weightSum += weightGenerator.Weights[i];
            }

            Assert.That(weightSum, Is.EqualTo(1.0).Using((IComparer<double>)ToleranceDoubleComparer.Instance));            
        }

        private void CheckWeightsEqual(WeightGenerator weightGenerator)
        {
            string fileName = @"Data\playgolf.train";
            DataStore dataStore = DataStore.Load(fileName, DataFormat.RSES1);

            double weight = 0;
            for (int i = 0; i < dataStore.NumberOfRecords; i++)
            {
                if (i == 0)
                {
                    weight = weightGenerator.Weights[i];
                }
                else
                {
                    Assert.AreEqual(weight, weightGenerator.Weights[i]);
                }
            }
        }

        [Test]
        public void WeightEqualGenerateTest()
        {
            string fileName = @"Data\playgolf.train";
            DataStore dataStore = DataStore.Load(fileName, DataFormat.RSES1);

            WeightGenerator weightGenerator = new WeightGeneratorMajority(dataStore);
            this.CheckWeightsEqual(weightGenerator);
            this.TestWeightsNormalized(weightGenerator);
        }

        [Test]
        public void WeightRelativeGenerateTest()
        {
            string fileName = @"Data\playgolf.train";
            DataStore dataStore = DataStore.Load(fileName, DataFormat.RSES1);

            WeightGenerator weightGenerator = new WeightGeneratorRelative(dataStore);
            this.TestWeightsNormalized(weightGenerator);
        }

        [Test]
        public void InformationMeasureMajorityTest()
        {
            string fileName = @"Data\playgolf.train";
            DataStore dataStore = DataStore.Load(fileName, DataFormat.RSES1);

            WeightGenerator weightGenerator = new WeightGeneratorMajority(dataStore);

            ReductWeights reduct = new ReductWeights(dataStore, dataStore.DataStoreInfo.SelectAttributeIds(a => a.IsStandard), 0, weightGenerator.Weights);
            IInformationMeasure infoMeasure = InformationMeasureBase.Construct(InformationMeasureType.Majority);
            double infoMeasureResult = infoMeasure.Calc(reduct);

            IInformationMeasure infoMeasureWeights = InformationMeasureBase.Construct(InformationMeasureType.ObjectWeights);
            double infoMeasureWeightsResult = infoMeasureWeights.Calc(reduct);

            Assert.AreEqual(infoMeasureResult, infoMeasureWeightsResult, 0.0000001);
            
        }

        [Test]
        public void ReductMajorityTest()
        {
            var localDataStore = Data.Benchmark.Factory.DnaModifiedTrain();
            //string localFileName = @"Data\dna_modified.trn";
            //DataStore localDataStore = DataStore.Load(localFileName, DataFormat.RSES1);
            int numberOfPermutations = 10;

            Args args = new Args();
            args.SetParameter(ReductFactoryOptions.DecisionTable, localDataStore);
            args.SetParameter(ReductFactoryOptions.Epsilon, 0.1);
            args.SetParameter(ReductFactoryOptions.ReductType, ReductTypes.ApproximateReductMajority);

            PermutationCollection permutationList = ReductFactory.GetPermutationGenerator(args).Generate(numberOfPermutations);

            args.SetParameter(ReductFactoryOptions.PermutationCollection, permutationList);

            IReductGenerator reductGenerator1 = ReductFactory.GetReductGenerator(args);
            reductGenerator1.Run();
            IReductStore reductStore1 = reductGenerator1.ReductPool;

            Args args2 = new Args();
            args2.SetParameter(ReductFactoryOptions.DecisionTable, localDataStore);
            args2.SetParameter(ReductFactoryOptions.PermutationCollection, permutationList);
            args2.SetParameter(ReductFactoryOptions.Epsilon, 0.1);
            args2.SetParameter(ReductFactoryOptions.ReductType, ReductTypes.ApproximateReductMajorityWeights);

            IReductGenerator reductGenerator2 = ReductFactory.GetReductGenerator(args2);
            reductGenerator2.Run();
            IReductStore reductStore2 = reductGenerator2.ReductPool;

            Assert.IsTrue(CompareReductStores(reductStore1, reductStore2));                        
        }

        [Test]
        public void ReductRelativeTest()
        {
            var localDataStore = Data.Benchmark.Factory.DnaModifiedTrain();
            //string localFileName = @"Data\dna_modified.trn";
            //DataStore localDataStore = DataStore.Load(localFileName, DataFormat.RSES1);
            int numberOfPermutations = 20;

            Args args = new Args();
            args.SetParameter(ReductFactoryOptions.DecisionTable, localDataStore);
            args.SetParameter(ReductFactoryOptions.Epsilon, 0.1);
            args.SetParameter(ReductFactoryOptions.ReductType, ReductTypes.ApproximateReductRelative);

            PermutationCollection permutationList = ReductFactory.GetPermutationGenerator(args).Generate(numberOfPermutations);

            args.SetParameter(ReductFactoryOptions.PermutationCollection, permutationList);

            IReductGenerator reductGenerator1 = ReductFactory.GetReductGenerator(args);
            reductGenerator1.Run();
            IReductStore reductStore1 = reductGenerator1.ReductPool;

            Args args2 = new Args();
            args2.SetParameter(ReductFactoryOptions.DecisionTable, localDataStore);
            args2.SetParameter(ReductFactoryOptions.PermutationCollection, permutationList);
            args2.SetParameter(ReductFactoryOptions.Epsilon, 0.1);
            args2.SetParameter(ReductFactoryOptions.ReductType, ReductTypes.ApproximateReductRelativeWeights);
            args2.SetParameter(ReductFactoryOptions.WeightGenerator, new WeightGeneratorRelative(localDataStore));

            IReductGenerator reductGenerator2 = ReductFactory.GetReductGenerator(args2);
            reductGenerator2.Run();
            IReductStore reductStore2 = reductGenerator2.ReductPool;

            Assert.IsTrue(CompareReductStores(reductStore1, reductStore2));            
        }

        public bool CompareReductStores(IReductStore reductStore1, IReductStore reductStore2)
        {           
            Assert.AreEqual(reductStore1.Count, reductStore2.Count);

            //Note that the order of reducts inside reduct store might differ when calculated in parallel
            //Reduct Ids also migth differ tu to race conditions on the GetNextReductId() method
            //We need to sort attributes inside each reduct and then sort reducts by attribute string.

            IReduct[] reducts1 = reductStore1.ToArray();
            int[][] redAttr1 = new int[reducts1.Length][];
            for(int i=0; i<reducts1.Length; i++)
            {
                redAttr1[i] = reducts1[i].Attributes.ToArray();
                Array.Sort(redAttr1[i]);
            }
           
            IReduct[] reducts2 = reductStore2.ToArray();
            int[][] redAttr2 = new int[reducts2.Length][];
            for (int i = 0; i < reducts2.Length; i++)
            {
                redAttr2[i] = reducts2[i].Attributes.ToArray();
                Array.Sort(redAttr2[i]);
            }

            Array.Sort(redAttr1, new ArrayComparer<int>());
            Array.Sort(redAttr2, new ArrayComparer<int>());

            for (int i = 0; i < redAttr1.Length; i++)
            {
                if (redAttr1[i].Length != redAttr2[i].Length)
                    Debugger.Break();

                Assert.AreEqual(redAttr1[i].Length, redAttr2[i].Length);
                for (int j = 0; j < redAttr1[i].Length; j++)
                {
                    if(redAttr1[i][j] != redAttr2[i][j])
                        Debugger.Break();

                    Assert.AreEqual(redAttr1[i][j], redAttr2[i][j]);
                }
            }

            return true;
        }
    }
}