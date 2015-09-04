using System;
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
            string fileName = @"Data\playgolf.train";
            dataStore = DataStore.Load(fileName, FileFormat.Rses1);
        }

        [Test]
        public void BalancedAccuracy()
        {
            string localFileName = @"Data\dna_modified.trn";
            DataStore localDataStore = DataStore.Load(localFileName, FileFormat.Rses1);

            RoughClassifier classifier = new RoughClassifier();
            classifier.Train(localDataStore, "ApproximateReductRelative", 20, 10);

            string localFileNameTest = @"Data\dna_modified.tst";
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
            foreach (long decision in dataStoreTest.DataStoreInfo.GetDecisionValues())
            {
                aprioriSum += classificationResult.DecisionApriori(decision);
                total += classificationResult.DecisionTotal(decision);
            }

            Assert.AreEqual(1.0, aprioriSum);
            Assert.AreEqual(dataStoreTest.NumberOfRecords, total);
            Assert.LessOrEqual(classificationResult.BalancedAccuracy, 1);
            Assert.GreaterOrEqual(classificationResult.BalancedAccuracy, 0);
        }

        [Test]
        public void CompareMajorityWeightVsMajority()
        {
            string localFileName = @"Data\dna.train";
            DataStore localDataStore = DataStore.Load(localFileName, FileFormat.Rses1);

            IReductGenerator redGenStd = new ReductGeneratorMajority();
            redGenStd.Epsilon = 0.1;

            IReductGenerator redGenWgh = new ReductGeneratorWeightsMajority();
            redGenWgh.Epsilon = 0.1;

            Args args = new Args(new string[] { "FactoryKey", "DataStore" }, new object[] { "ApproximateReductRelative", localDataStore });
            
            IPermutationGenerator permGen = ReductFactory.GetPermutationGenerator(args);
            PermutationCollection permutationList = permGen.Generate(20);
            args.AddParameter("PermutationCollection", permutationList);

            redGenStd.InitFromArgs(args);
            redGenStd.Generate();
            IReductStore redStore = redGenStd.ReductPool;

            redGenWgh.InitFromArgs(args);
            redGenWgh.Generate();
            IReductStore redStoreW = redGenWgh.ReductPool;

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
            string localFileName = @"Data\optdigits.trn";
            DataStore localDataStore = DataStore.Load(localFileName, FileFormat.Rses1);

            IReductGenerator redGenStd = new ReductGeneratorRelative();
            redGenStd.Epsilon = 0.1;

            IReductGenerator redGenWgh = new ReductGeneratorWeightsRelative();
            redGenWgh.Epsilon = 0.1;

            Args args = new Args(new string[] { "FactoryKey", "DataStore" }, new object[] { "ApproximateReductRelative", localDataStore });

            IPermutationGenerator permGen = ReductFactory.GetPermutationGenerator(args);
            PermutationCollection permutationList = permGen.Generate(5);
            args.AddParameter("PermutationCollection", permutationList);

            redGenStd.InitFromArgs(args);
            redGenStd.Generate();

            redGenWgh.InitFromArgs(args);
            redGenWgh.Generate();

            IReductStore redStore = redGenStd.ReductPool;
            IReductStore redStoreW = redGenWgh.ReductPool;

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
            string localFileName = @"Data\optdigits.trn";
            DataStore localDataStore = DataStore.Load(localFileName, FileFormat.Rses1);

            RoughClassifier roughClassifier = new RoughClassifier();

            PermutationGenerator permGen = new PermutationGenerator(localDataStore);
            PermutationCollection permutationList = permGen.Generate(5);

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
            string localFileName = @"Data\optdigits.trn";
            DataStore localDataStore = DataStore.Load(localFileName, FileFormat.Rses1);
            
            RoughClassifier roughClassifier = new RoughClassifier();

            PermutationGenerator permGen = new PermutationGenerator(localDataStore);
            PermutationCollection permutationList = permGen.Generate(5);

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

            PermutationCollection permutationList = new PermutationCollection();
            permutationList.Add(new Permutation(new int[] { 3, 4, 1, 2 }));
            permutationList.Add(new Permutation(new int[] { 3, 4, 2, 1 }));
            permutationList.Add(new Permutation(new int[] { 4, 3, 1, 2 }));
            permutationList.Add(new Permutation(new int[] { 3, 4, 1, 2 }));
            permutationList.Add(new Permutation(new int[] { 4, 2, 3, 1 }));

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

            PermutationCollection permutationList = new PermutationCollection();
            permutationList.Add(new Permutation(new int[] { 3, 4, 1, 2 }));
            permutationList.Add(new Permutation(new int[] { 3, 4, 2, 1 }));
            permutationList.Add(new Permutation(new int[] { 4, 3, 1, 2 }));
            permutationList.Add(new Permutation(new int[] { 3, 4, 1, 2 }));
            permutationList.Add(new Permutation(new int[] { 4, 2, 3, 1 }));
            
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

            ReductWeights reduct = new ReductWeights(dataStore, dataStore.DataStoreInfo.GetFieldIds(FieldTypes.Standard), weightGenerator.Weights, 0);
            IInformationMeasure infoMeasure = InformationMeasureBase.Construct(InformationMeasureType.Majority);
            double infoMeasureResult = infoMeasure.Calc(reduct);

            IInformationMeasure infoMeasureWeights = InformationMeasureBase.Construct(InformationMeasureType.ObjectWeights);
            double infoMeasureWeightsResult = infoMeasureWeights.Calc(reduct);

            Assert.LessOrEqual(infoMeasureResult, infoMeasureWeightsResult + (0.00001 / (double)this.dataStore.NumberOfRecords));
            Assert.GreaterOrEqual(infoMeasureResult, infoMeasureWeightsResult - (0.00001 / (double)this.dataStore.NumberOfRecords));
        }

        public void CompareReductResult(string reductGeneratorKey1, string reductGeneratorKey2)
        {
            string localFileName = @"Data\dna.train";
            DataStore localDataStore = DataStore.Load(localFileName, FileFormat.Rses1);
            
            Args args = new Args();
            args.AddParameter("DataStore", localDataStore);            
            args.AddParameter("ApproximationRatio", 10);
            args.AddParameter("FactoryKey", reductGeneratorKey1);

            PermutationCollection permutationList = ReductFactory.GetPermutationGenerator(args).Generate(10);

            args.AddParameter("PermutationCollection", permutationList);
            
            IReductGenerator reductGenerator1 = ReductFactory.GetReductGenerator(args);
            reductGenerator1.Generate();
            IReductStore reductStore1 = reductGenerator1.ReductPool;

            Args args2 = new Args();
            args2.AddParameter("DataStore", localDataStore);
            args2.AddParameter("PermutationCollection", permutationList);
            args2.AddParameter("ApproximationRatio", 10);
            args2.AddParameter("FactoryKey", reductGeneratorKey2);
            
            IReductGenerator reductGenerator2 = ReductFactory.GetReductGenerator(args2);
            reductGenerator2.Generate();
            IReductStore reductStore2 = reductGenerator2.ReductPool;

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
