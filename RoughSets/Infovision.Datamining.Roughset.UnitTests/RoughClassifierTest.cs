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

                        RoughClassifier2_OLD classifier2 = new RoughClassifier2_OLD();
                        classifier2.ReductStoreCollection = reductStoreCollection;
                        classifier2.Classify(testData, identificationFunc, voteFunc);
                        ClassificationResult classificationResult2 = classifier2.Vote(
                            testData,
                            identificationFunc,
                            voteFunc,
                            null);

                        RoughClassifier classifier = new RoughClassifier(
                            reductStoreCollection,
                            identificationFunc,
                            voteFunc,
                            trainData.DataStoreInfo.DecisionInfo.InternalValues());
                        ClassificationResult classificationResult = classifier.Classify(
                            testData);


                        Assert.AreEqual(classificationResultOld.Accuracy, classificationResult.Accuracy);
                        Assert.AreEqual(classificationResultOld.Confidence, classificationResult.Confidence);
                        Assert.AreEqual(classificationResultOld.Coverage, classificationResult.Coverage);

                        Assert.AreEqual(classificationResultOld.Accuracy, classificationResult2.Accuracy);
                        Assert.AreEqual(classificationResultOld.Confidence, classificationResult2.Confidence);
                        Assert.AreEqual(classificationResultOld.Coverage, classificationResult2.Coverage);

                        foreach (long objectId in testData.GetObjectIds())
                        {
                            long predictionOld = classificationResultOld.GetResult(objectId);
                            long prediction2 = classificationResult2.GetResult(objectId);
                            long prediction = classificationResult.GetResult(objectId);

                            if (predictionOld != prediction)
                            {
                                DataRecordInternal record = testData.GetRecordByObjectId(objectId);

                                classifierOld.PrintClassification(record, identificationType, voteType);
                                Console.WriteLine();
                                Console.WriteLine();
                                classifier.PrintClassification(record);

                                Console.Beep();

                                long a = classifierOld.Classify(record, identificationType, voteType).FindMaxValue();
                                long b = classifier.Classify(record).FindMaxValue();
                            }

                            if (predictionOld != prediction)
                            {
                                DataRecordInternal record = testData.GetRecordByObjectId(objectId);

                                classifierOld.PrintClassification(record, identificationType, voteType);
                                Console.WriteLine();
                                Console.WriteLine();
                                classifier2.PrintClassification(record, identificationFunc, voteFunc);

                                Console.Beep();

                                long a = classifierOld.Classify(record, identificationType, voteType).FindMaxValue();
                                long b = classifier2.Classify(record, identificationFunc, voteFunc).FindMaxValue();
                            }


                            Assert.AreEqual(predictionOld, prediction,
                                String.Format(
                                "Old vs New objectid: {0} objectIdx: {1} ident {2} vote {3}", 
                                objectId, testData.ObjectId2ObjectIndex(objectId),
                                identType, vType));
                            Assert.AreEqual(predictionOld, prediction2,
                                String.Format(
                                "Old vs Current objectid: {0} objectIdx: {1} ident {2} vote {3}",
                                objectId, testData.ObjectId2ObjectIndex(objectId),
                                identType, vType));
                        }
                    }
                }
            }
        }


        [Test, Ignore]
        public void RoughClassifierNewPerformanceTest()
        {
            UInt64 sum_1 = 0, sum_2 = 0, sum_3 = 0;
            int loops = 100;
            int numberOfPerm = 10;

            decimal epsilon = 0.05m;
            string reductFactoryKey = ReductFactoryKeyHelper.ApproximateReductMajorityWeights;

            string trainFileName = @"Data\dna_modified.trn";
            string testFileName = @"Data\dna_modified.tst";

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

            for (int i = 0; i < loops; i++)
            {    
                args.AddParameter(ReductGeneratorParamHelper.PermutationCollection,
                            ReductFactory.GetPermutationGenerator(args).Generate(numberOfPerm));
                
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
                    classificationResultOld.GetResult(objectId);                                       
                watch_1.Stop();
                sum_1 += (ulong) watch_1.ElapsedMilliseconds;

                var watch_2 = Stopwatch.StartNew();
                RoughClassifier classifier = new RoughClassifier(
                    reductStoreCollection,
                    identificationFunc,
                    voteFunc,
                    trainData.DataStoreInfo.GetDecisionValues());
                ClassificationResult classificationResult = classifier.Classify(testData, null);
                foreach (long objectId in testData.GetObjectIds())
                    classificationResult.GetResult(objectId);
                watch_2.Stop();
                sum_2 += (ulong) watch_2.ElapsedMilliseconds;
                
                var watch_3 = Stopwatch.StartNew();
                RoughClassifier2_OLD classifier2 = new RoughClassifier2_OLD();
                classifier2.ReductStoreCollection = reductStoreCollection;
                classifier2.Classify(testData, null, 0, reductStoreCollection, identificationFunc, voteFunc);
                ClassificationResult classificationResult2 = classifier2.Vote(
                    testData,
                    identificationFunc,
                    voteFunc,
                    null);
                foreach (long objectId in testData.GetObjectIds())
                    classificationResult2.GetResult(objectId);
                watch_3.Stop();
                sum_3 += (ulong)watch_3.ElapsedMilliseconds;
            }

            Console.WriteLine("Classifier 1: {0}", (decimal)sum_1 / (decimal)loops);
            Console.WriteLine("Classifier 2: {0}", (decimal)sum_2 / (decimal)loops);
            Console.WriteLine("Classifier 3: {0}", (decimal)sum_3 / (decimal)loops);
        }
    }
}
