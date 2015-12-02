using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Infovision.Data;
using Infovision.Utils;
using NUnit.Framework;

namespace Infovision.Datamining.Roughset.UnitTests
{
    [TestFixture]
    public class RoughClassifierTest
    {
        [Test]
        public void RoughClassifierNewTest()
        {

            for (IdentificationType identType = IdentificationType.Support; identType <= IdentificationType.WeightCoverage; identType++)
            {
                for (VoteType vType = VoteType.Support; vType <= VoteType.ConfidenceRelative; vType++)
                {
                    for (int i = 0; i < 5; i++)
                    {
                        decimal epsilon = 0.1m;
                        int numberOfPermutations = 10;
                        string reductFactoryKey = ReductFactoryKeyHelper.ApproximateReductMajorityWeights;

                        string trainFileName = @"Data\dna_modified.trn";
                        string testFileName = @"Data\dna_modified.tst";

                        IdentificationType identificationType = identType;
                        VoteType voteType = vType;
                        RuleQualityFunction identificationFunc;
                        RuleQualityFunction voteFunc;

                        switch (identificationType)
                        {
                            case IdentificationType.Support: identificationFunc = RuleQuality.Support; break;
                            case IdentificationType.Confidence: identificationFunc = RuleQuality.Confidence; break;
                            case IdentificationType.Coverage: identificationFunc = RuleQuality.Coverage; break;
                            case IdentificationType.WeightSupport: identificationFunc = RuleQuality.SupportW; break;
                            case IdentificationType.WeightConfidence: identificationFunc = RuleQuality.ConfidenceW; break;
                            case IdentificationType.WeightCoverage: identificationFunc = RuleQuality.CoverageW; break;
                            default: throw new ArgumentException("Unknown value", "identificationType");
                        }

                        switch (voteType)
                        {
                            case VoteType.Support: voteFunc = RuleQuality.Support; break;
                            case VoteType.Confidence: voteFunc = RuleQuality.Confidence; break;
                            case VoteType.Coverage: voteFunc = RuleQuality.Coverage; break;
                            case VoteType.Ratio: voteFunc = RuleQuality.Ratio; break;
                            case VoteType.Strength: voteFunc = RuleQuality.Strength; break;
                            case VoteType.MajorDecision: voteFunc = RuleQuality.SingleVote; break;
                            case VoteType.WeightSupport: voteFunc = RuleQuality.SupportW; break;
                            case VoteType.WeightConfidence: voteFunc = RuleQuality.ConfidenceW; break;
                            case VoteType.WeightCoverage: voteFunc = RuleQuality.CoverageW; break;
                            case VoteType.WeightRatio: voteFunc = RuleQuality.RatioW; break;
                            case VoteType.WeightStrength: voteFunc = RuleQuality.StrengthW; break;
                            case VoteType.ConfidenceRelative: voteFunc = RuleQuality.ConfidenceRelative; break;
                            default: throw new ArgumentException("Unknown value", "voteType");
                        }

                        DataStore trainData = DataStore.Load(trainFileName, FileFormat.Rses1);
                        DataStore testData = DataStore.Load(testFileName, FileFormat.Rses1, trainData.DataStoreInfo);

                        Args args = new Args();
                        args.AddParameter(ReductGeneratorParamHelper.DataStore, trainData);
                        args.AddParameter(ReductGeneratorParamHelper.Epsilon, epsilon);
                        args.AddParameter(ReductGeneratorParamHelper.FactoryKey, reductFactoryKey);

                        args.AddParameter(ReductGeneratorParamHelper.PermutationCollection,
                            ReductFactory.GetPermutationGenerator(args).Generate(numberOfPermutations));

                        IReductGenerator reductGenerator = ReductFactory.GetReductGenerator(args);
                        reductGenerator.Generate();

                        IReductStoreCollection reductStoreCollection = reductGenerator.GetReductStoreCollection(Int32.MaxValue);

                        RoughClassifier_OLD classifierOld = new RoughClassifier_OLD();
                        classifierOld.ReductStoreCollection = reductStoreCollection;
                        classifierOld.Classify(testData, null, 0, reductStoreCollection);
                        ClassificationResult classificationResultOld = classifierOld.Vote(
                            testData,
                            identificationType,
                            voteType,
                            null);

                        RoughClassifier classifier = new RoughClassifier();
                        classifier.ReductStoreCollection = reductStoreCollection;
                        classifier.Classify(testData, identificationFunc, voteFunc);
                        ClassificationResult classificationResult = classifier.Vote(
                            testData,
                            identificationFunc,
                            voteFunc,
                            null);

                        RoughClassifierNew classifierNew = new RoughClassifierNew(
                            reductStoreCollection,
                            identificationFunc,
                            voteFunc,
                            trainData.DataStoreInfo.DecisionInfo.InternalValues());
                        ClassificationResult classificationResultNew = classifierNew.Classify(
                            testData);


                        //Assert.AreEqual(classificationResultOld.Accuracy, classificationResultNew.Accuracy);
                        //Assert.AreEqual(classificationResultOld.Confidence, classificationResultNew.Confidence);
                        //Assert.AreEqual(classificationResultOld.Coverage, classificationResultNew.Coverage);

                        //Assert.AreEqual(classificationResultOld.Accuracy, classificationResult.Accuracy);
                        //Assert.AreEqual(classificationResultOld.Confidence, classificationResult.Confidence);
                        //Assert.AreEqual(classificationResultOld.Coverage, classificationResult.Coverage);

                        foreach (long objectId in testData.GetObjectIds())
                        {
                            long predictionOld = classificationResultOld.GetResult(objectId);
                            long prediction = classificationResult.GetResult(objectId);
                            long predictionNew = classificationResultNew.GetResult(objectId);

                            if (predictionOld != predictionNew)
                            {
                                DataRecordInternal record = testData.GetRecordByObjectId(objectId);

                                classifierOld.PrintClassification(record, identificationType, voteType);
                                Console.WriteLine();
                                Console.WriteLine();
                                classifierNew.PrintClassification(record);

                                Console.Beep();

                                long a = classifierOld.Classify(record, identificationType, voteType).GetPrediction();
                                long b = classifierNew.Classify(record).GetPrediction();
                            }

                            Assert.AreEqual(predictionOld, predictionNew,
                                String.Format(
                                "Old vs New objectid: {0} objectIdx: {1} ident {2} vote {3}", 
                                objectId, testData.ObjectId2ObjectIndex(objectId),
                                identType, vType));
                            Assert.AreEqual(predictionOld, prediction,
                                String.Format(
                                "Old vs Current objectid: {0} objectIdx: {1} ident {2} vote {3}",
                                objectId, testData.ObjectId2ObjectIndex(objectId),
                                identType, vType));
                        }
                    }
                }
            }
        }


        [Test]
        public void RoughClassifierNewPerformanceTest()
        {
            UInt64 sum_1 = 0, sum_2 = 0;
            int loops = 100;
            for (int i = 0; i < loops; i++)
            {
                decimal epsilon = 0.1m;
                string reductFactoryKey = ReductFactoryKeyHelper.ApproximateReductMajorityWeights;

                string trainFileName    = @"Data\dna_modified.trn";
                string testFileName     = @"Data\dna_modified.tst";

                RuleQualityFunction identificationFunc = RuleQuality.ConfidenceW;
                RuleQualityFunction voteFunc = RuleQuality.ConfidenceW;

                IdentificationType identificationType = IdentificationType.WeightConfidence;
                VoteType voteType = VoteType.WeightConfidence;

                DataStore trainData = DataStore.Load(trainFileName, FileFormat.Rses1);
                DataStore testData = DataStore.Load(testFileName, FileFormat.Rses1, trainData.DataStoreInfo);

                Args args = new Args();
                args.AddParameter(ReductGeneratorParamHelper.DataStore, trainData);
                args.AddParameter(ReductGeneratorParamHelper.Epsilon, epsilon);
                args.AddParameter(ReductGeneratorParamHelper.FactoryKey, reductFactoryKey);                
                
                PermutationCollection permutations = new PermutationCollection();
                permutations.Add(new Permutation(new int[] { 9, 17, 15, 6, 8, 2, 4, 5, 13, 12, 3, 7, 10, 20, 14, 18, 11, 19, 1, 16 }));
                permutations.Add(new Permutation(new int[] { 16, 1, 19, 11, 18, 14, 20, 10, 7, 3, 12, 13, 5, 4, 2, 8, 6, 15, 17, 9 }));
                args.AddParameter(ReductGeneratorParamHelper.PermutationCollection, permutations);                

                IReductGenerator reductGenerator = ReductFactory.GetReductGenerator(args);
                reductGenerator.Generate();

                IReductStoreCollection reductStoreCollection = reductGenerator.GetReductStoreCollection(Int32.MaxValue);

                var watch_1 = Stopwatch.StartNew();

                RoughClassifier_OLD classifierOld = new RoughClassifier_OLD();
                classifierOld.ReductStoreCollection = reductStoreCollection;
                classifierOld.Classify(testData, null, 0, reductStoreCollection);
                ClassificationResult classificationResultOld = classifierOld.Vote(
                    testData,
                    identificationType,
                    voteType,
                    null);

                foreach (long objectId in testData.GetObjectIds())
                {
                    long predictionOld = classificationResultOld.GetResult(objectId);                                       
                }

                sum_1 += (ulong) watch_1.ElapsedMilliseconds;

                var watch_2 = Stopwatch.StartNew();
                
                RoughClassifierNew classifierNew = new RoughClassifierNew(
                    reductStoreCollection,
                    identificationFunc,
                    voteFunc,
                    trainData.DataStoreInfo.DecisionInfo.InternalValues());
                ClassificationResult classificationResultNew = classifierNew.Classify(
                    testData);                

                foreach (long objectId in testData.GetObjectIds())
                {                                    
                    long predictionNew = classificationResultNew.GetResult(objectId);                                       
                }

                sum_2 += (ulong) watch_2.ElapsedMilliseconds;
            }

            Console.WriteLine("Classifier 1: {0}", (decimal)sum_1 / (decimal)loops);
            Console.WriteLine("Classifier 2: {0}", (decimal)sum_2 / (decimal)loops);
        }
    }
}
