﻿using System;
using Infovision.Data;
using Infovision.Utils;
using NUnit.Framework;

namespace Infovision.Datamining.Roughset.UnitTests
{
    [TestFixture]
    public class WeightGeneratorTest
    {
        DataStore dataStore = null;

        public WeightGeneratorTest()
        {
            string fileName = @"playgolf.train";
            dataStore = DataStore.Load(fileName, FileFormat.Rses1);
        }

        [Test]
        public void BalancedAccuracy()
        {
            string localFileName = @"dna_modified.trn";
            DataStore localDataStore = DataStore.Load(localFileName, FileFormat.Rses1);

            RoughClassifier classifier = new RoughClassifier();
            classifier.Train(localDataStore, "ApproximateReductRelative", 20, 10);

            string localFileNameTest = @"dna_modified.tst";
            DataStore dataStoreTest = DataStore.Load(localFileNameTest, FileFormat.Rses1);

            classifier.Classify(dataStoreTest);

            ClassificationResult classificationResult = classifier.Vote(dataStoreTest,
                                                                        IdentificationType.Confidence,
                                                                        VoteType.MajorDecision);

            Assert.AreEqual(dataStoreTest.NumberOfRecords, classificationResult.Count);

            Assert.AreEqual(classificationResult.Count, classificationResult.NumberOfClassified
                                                 + classificationResult.NumberOfMisclassified
                                                 + classificationResult.NumberOfUnclassifed);

            
            double total = 0;
            double aprioriSum = 0;
            foreach (Int64 decision in dataStoreTest.DataStoreInfo.GetDecisionValues())
            {
                aprioriSum += classificationResult.DecisionApriori(decision);
                total += classificationResult.DecisionTotal(decision);
            }

            Assert.AreEqual((double)1, aprioriSum);
            Assert.AreEqual(dataStoreTest.NumberOfRecords, total);
            Assert.LessOrEqual(classificationResult.BalancedAccuracy, 1);
            Assert.GreaterOrEqual(classificationResult.BalancedAccuracy, 0);
        }

        [Test]
        public void CompareMajorityWeightVsMajority()
        {
            string localFileName = @"dna.train";
            DataStore localDataStore = DataStore.Load(localFileName, FileFormat.Rses1);

            IReductGenerator redGenStd = new ReductGeneratorMajority(localDataStore);
            redGenStd.ApproximationLevel = 0.10;

            IReductGenerator redGenWgh = new ReductGeneratorWeightsMajority(localDataStore);
            redGenWgh.ApproximationLevel = 0.10;
 
            Args args = new Args(new string[] { "DataStore" }, new object[] { localDataStore });
            
            IPermutationGenerator permGen = ReductFactory.GetPermutationGenerator("ApproximateReductRelative", args);
            PermutationList permutationList = permGen.Generate(20);
            args.AddParameter("PermutationList", permutationList);
            
            IReductStore redStore = redGenStd.Generate(args);
            IReductStore redStoreW = redGenWgh.Generate(args);

            int i = 0;
            foreach (IReduct reduct in redStore)
            {
                Console.WriteLine(reduct);
                IReduct redW = redStoreW.GetReduct(i);
                i++;

                Assert.AreEqual(reduct, redW);
            }

        }

        [Test]
        public void CompareRelativeWeightVsRelative()
        {
            string localFileName = @"optdigits.trn";
            DataStore localDataStore = DataStore.Load(localFileName, FileFormat.Rses1);

            IReductGenerator redGenStd = new ReductGeneratorRelative(localDataStore);
            redGenStd.ApproximationLevel = 0.1;

            IReductGenerator redGenWgh = new ReductGeneratorWeightsRelative(localDataStore);
            redGenWgh.ApproximationLevel = 0.1;

            Args args = new Args(new string[] { "DataStore" }, new object[] { localDataStore });

            IPermutationGenerator permGen = ReductFactory.GetPermutationGenerator("ApproximateReductRelative", args);
            PermutationList permutationList = permGen.Generate(100);
            args.AddParameter("PermutationList", permutationList);

            IReductStore redStore = redGenStd.Generate(args);
            IReductStore redStoreW = redGenWgh.Generate(args);

            int i = 0;
            foreach (IReduct reduct in redStore)
            {
                Console.WriteLine(reduct);
                IReduct redW = redStoreW.GetReduct(i);
                i++;

                Assert.AreEqual(reduct, redW);
            }

        }

        [Test]
        public void WeightReductRelative2()
        {
            string localFileName = @"optdigits.trn";
            DataStore localDataStore = DataStore.Load(localFileName, FileFormat.Rses1);

            RoughClassifier roughClassifier = new RoughClassifier();

            PermutationGenerator permGen = new PermutationGenerator(localDataStore);
            PermutationList permutationList = permGen.Generate(5);

            roughClassifier.Train(localDataStore, "ApproximateReductRelative", 20, permutationList);

            RoughClassifier roughClassifierWeight = new RoughClassifier();
            roughClassifierWeight.Train(localDataStore, "ApproximateReductRelativeWeights", 20, permutationList);
            for (int i = 0; i < roughClassifier.ReductStore.Count; i++)
            {
                Reduct r1 = roughClassifier.ReductStore.GetReduct(i) as Reduct;
                Reduct r2 = roughClassifierWeight.ReductStore.GetReduct(i) as Reduct;

                Console.WriteLine("{0} | {1}", r1, r2);

                Assert.NotNull(r1);
                Assert.NotNull(r2);
                Assert.AreEqual(r1, r2);
            }

        }

        [Test]
        public void WeightReductMajority2()
        {
            string localFileName = @"optdigits.trn";
            DataStore localDataStore = DataStore.Load(localFileName, FileFormat.Rses1);
            
            RoughClassifier roughClassifier = new RoughClassifier();

            PermutationGenerator permGen = new PermutationGenerator(localDataStore);
            PermutationList permutationList = permGen.Generate(5);

            roughClassifier.Train(localDataStore, "ApproximateReductMajority", 20, permutationList);

            RoughClassifier roughClassifierWeight = new RoughClassifier();
            roughClassifierWeight.Train(localDataStore, "ApproximateReductMajorityWeights", 20, permutationList);
            for (int i = 0; i < roughClassifier.ReductStore.Count; i++)
            {
                Reduct r1 = roughClassifier.ReductStore.GetReduct(i) as Reduct;
                Reduct r2 = roughClassifierWeight.ReductStore.GetReduct(i) as Reduct;

                Console.WriteLine("{0} | {1}", r1, r2);

                Assert.NotNull(r1);
                Assert.NotNull(r2);
                Assert.AreEqual(r1, r2);
            }

        }

        [Test]
        public void WeightReductRelative()
        {
            RoughClassifier roughClassifier = new RoughClassifier();

            PermutationList permutationList = new PermutationList();
            permutationList.Add(new Permutation(new Int32[] { 3, 4, 1, 2 }));
            permutationList.Add(new Permutation(new Int32[] { 3, 4, 2, 1 }));
            permutationList.Add(new Permutation(new Int32[] { 4, 3, 1, 2 }));
            permutationList.Add(new Permutation(new Int32[] { 3, 4, 1, 2 }));
            permutationList.Add(new Permutation(new Int32[] { 4, 2, 3, 1 }));

            roughClassifier.Train(dataStore, "ApproximateReductRelative", 0, permutationList);

            RoughClassifier roughClassifierWeight = new RoughClassifier();
            roughClassifierWeight.Train(dataStore, "ApproximateReductRelativeWeights", 0, permutationList);
            for (int i = 0; i < roughClassifier.ReductStore.Count; i++)
            {
                Reduct r1 = roughClassifier.ReductStore.GetReduct(i) as Reduct;
                Reduct r2 = roughClassifierWeight.ReductStore.GetReduct(i) as Reduct;

                Console.WriteLine("{0} | {1}", r1, r2);

                Assert.NotNull(r1);
                Assert.NotNull(r2);
                Assert.AreEqual(r1, r2);
            }

        }

        [Test]
        public void WeightReductMajority()
        {
            RoughClassifier roughClassifier = new RoughClassifier();

            PermutationList permutationList = new PermutationList();
            permutationList.Add(new Permutation(new Int32[] { 3, 4, 1, 2 }));
            permutationList.Add(new Permutation(new Int32[] { 3, 4, 2, 1 }));
            permutationList.Add(new Permutation(new Int32[] { 4, 3, 1, 2 }));
            permutationList.Add(new Permutation(new Int32[] { 3, 4, 1, 2 }));
            permutationList.Add(new Permutation(new Int32[] { 4, 2, 3, 1 }));
            
            roughClassifier.Train(dataStore, "ApproximateReductMajority", 0, permutationList);

            RoughClassifier roughClassifierWeight = new RoughClassifier();
            roughClassifierWeight.Train(dataStore, "ApproximateReductMajorityWeights", 0, permutationList);
            for (int i = 0; i < roughClassifier.ReductStore.Count; i++)
            {
                Reduct r1 = roughClassifier.ReductStore.GetReduct(i) as Reduct;
                Reduct r2 = roughClassifierWeight.ReductStore.GetReduct(i) as Reduct;

                Console.WriteLine("{0} | {1}", r1, r2);

                Assert.NotNull(r1);
                Assert.NotNull(r2);
                Assert.AreEqual(r1, r2);
            }
                
        }

        [Test]
        public void WeightEqualConstructorTest()
        {
            WeightGenerator weightGenerator = new WeightGeneratorMajority(dataStore);
            Assert.IsNotNull(weightGenerator);
        }

        [Test]
        public void WeightRelativeConstructorTest()
        {
            WeightGenerator weightGenerator = new WeightGeneratorRelative(dataStore);
            Assert.IsNotNull(weightGenerator);
        }

        private void TestWeightsNormalized(WeightGenerator weightGenerator)
        {
            double weightSum = 0;
            for (int i = 0; i < dataStore.NumberOfRecords; i++)
            {
                weightSum += weightGenerator.Weights[i];
            }

            Assert.LessOrEqual(weightSum, 1.000001);
            Assert.GreaterOrEqual(weightSum, 0.999999);
        }

        private void CheckWeightsEqual(WeightGenerator weightGenerator)
        {
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
            WeightGenerator weightGenerator = new WeightGeneratorMajority(dataStore);
            this.CheckWeightsEqual(weightGenerator);
            this.TestWeightsNormalized(weightGenerator);
        }

        [Test]
        public void WeightRelativeGenerateTest()
        {
            WeightGenerator weightGenerator = new WeightGeneratorRelative(dataStore);
            this.TestWeightsNormalized(weightGenerator);
        }

        [Test]
        public void InformationMeasureMajorityTest()
        {
            WeightGenerator weightGenerator = new WeightGeneratorMajority(dataStore);

            ReductWeights reduct = new ReductWeights(dataStore, dataStore.DataStoreInfo.GetFieldIds(FieldTypes.Standard), weightGenerator.Weights);
            IInformationMeasure infoMeasure = InformationMeasureBase.Construct(InformationMeasureType.Majority);
            double infoMeasureResult = infoMeasure.Calc(reduct);

            IInformationMeasure infoMeasureWeights = InformationMeasureBase.Construct(InformationMeasureType.ObjectWeights);
            double infoMeasureWeightsResult = infoMeasureWeights.Calc(reduct);

            Assert.LessOrEqual(infoMeasureResult, infoMeasureWeightsResult + (0.00001 / (double)this.dataStore.NumberOfRecords));
            Assert.GreaterOrEqual(infoMeasureResult, infoMeasureWeightsResult - (0.00001 / (double)this.dataStore.NumberOfRecords));
        }

        public void CompareReductResult(string reductGeneratorKey1, string reductGeneratorKey2)
        {
            string localFileName = @"dna.train";
            DataStore localDataStore = DataStore.Load(localFileName, FileFormat.Rses1);
            
            Args args = new Args();
            args.AddParameter("DataStore", localDataStore);
            
            PermutationList permutationList = ReductFactory.GetPermutationGenerator(reductGeneratorKey1, args).Generate(10);
            
            args.AddParameter("PermutationList", permutationList);
            args.AddParameter("ApproximationRatio", 10);
            
            IReductGenerator reductGenerator1 = ReductFactory.GetReductGenerator(reductGeneratorKey1, args);
            IReductStore reductStore1 = reductGenerator1.Generate(args);
            
            IReductGenerator reductGenerator2 = ReductFactory.GetReductGenerator(reductGeneratorKey2, args);
            IReductStore reductStore2 = reductGenerator2.Generate(args);

            Assert.AreEqual(reductStore1.Count, reductStore2.Count);

            for (int i = 0; i < reductStore1.Count; i++)
            {
                IReduct reduct1 = reductStore1.GetReduct(i);
                IReduct reduct2 = reductStore2.GetReduct(i);

                //Console.WriteLine("Reduct A: {0}", reduct1);
                //Console.WriteLine("Reduct B: {0}", reduct2);

                Assert.AreEqual(reduct1, reduct2);             
            }
        }

        [Test]
        public void ReductMajorityTest()
        {
            this.CompareReductResult("ApproximateReductMajority", "ApproximateReductMajorityWeights");
        }

        [Test]
        public void ReductRelativeTest()
        {
            this.CompareReductResult("ApproximateReductRelative", "ApproximateReductRelativeWeights");
        }
    }
}