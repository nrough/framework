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
        [Test, Ignore]
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
                            case IdentificationType.Support: identificationFunc = RuleQuality_DEL.Support; break;
                            case IdentificationType.Confidence: identificationFunc = RuleQuality_DEL.Confidence; break;
                            case IdentificationType.Coverage: identificationFunc = RuleQuality_DEL.Coverage; break;
                            case IdentificationType.WeightSupport: identificationFunc = RuleQuality_DEL.SupportW; break;
                            case IdentificationType.WeightConfidence: identificationFunc = RuleQuality_DEL.ConfidenceW; break;
                            case IdentificationType.WeightCoverage: identificationFunc = RuleQuality_DEL.CoverageW; break;
                            default: throw new ArgumentException("Unknown value", "identificationType");
                        }

                        switch (voteType)
                        {
                            case VoteType.Support: voteFunc = RuleQuality_DEL.Support; break;
                            case VoteType.Confidence: voteFunc = RuleQuality_DEL.Confidence; break;
                            case VoteType.Coverage: voteFunc = RuleQuality_DEL.Coverage; break;
                            case VoteType.Ratio: voteFunc = RuleQuality_DEL.Ratio; break;
                            case VoteType.Strength: voteFunc = RuleQuality_DEL.Strength; break;
                            case VoteType.MajorDecision: voteFunc = RuleQuality_DEL.SingleVote; break;
                            case VoteType.WeightSupport: voteFunc = RuleQuality_DEL.SupportW; break;
                            case VoteType.WeightConfidence: voteFunc = RuleQuality_DEL.ConfidenceW; break;
                            case VoteType.WeightCoverage: voteFunc = RuleQuality_DEL.CoverageW; break;
                            case VoteType.WeightRatio: voteFunc = RuleQuality_DEL.RatioW; break;
                            case VoteType.WeightStrength: voteFunc = RuleQuality_DEL.StrengthW; break;
                            case VoteType.ConfidenceRelative: voteFunc = RuleQuality_DEL.ConfidenceRelative; break;
                            default: throw new ArgumentException("Unknown value", "voteType");
                        }

                        DataStore trainData = DataStore.Load(trainFileName, FileFormat.Rses1);
                        DataStore testData = DataStore.Load(testFileName, FileFormat.Rses1, trainData.DataStoreInfo);

                        Args args = new Args();
                        args.SetParameter(ReductGeneratorParamHelper.TrainData, trainData);
                        args.SetParameter(ReductGeneratorParamHelper.Epsilon, epsilon);
                        args.SetParameter(ReductGeneratorParamHelper.FactoryKey, reductFactoryKey);

                        args.SetParameter(ReductGeneratorParamHelper.PermutationCollection,
                            ReductFactory.GetPermutationGenerator(args).Generate(numberOfPermutations));

                        IReductGenerator reductGenerator = ReductFactory.GetReductGenerator(args);
                        reductGenerator.Run();

                        IReductStoreCollection reductStoreCollection = reductGenerator.GetReductStoreCollection();

                        RoughClassifier classifier = new RoughClassifier(
                            reductStoreCollection,
                            identificationFunc,
                            voteFunc,
                            trainData.DataStoreInfo.DecisionInfo.InternalValues());
                        ClassificationResult classificationResult = classifier.Classify(testData);

                        for (int objectIdx = 0; objectIdx < testData.NumberOfRecords; objectIdx++)
                        {
                            long prediction = classificationResult.GetPrediction(objectIdx);
                        }
                    }
                }
            }
        }


        [Test]
        public void RoughClassifierNewPerformanceTest()
        {
            UInt64 sum_2 = 0;
            int loops = 100;
            int numberOfPerm = 10;

            decimal epsilon = 0.05m;
            string reductFactoryKey = ReductFactoryKeyHelper.ApproximateReductMajorityWeights;

            string trainFileName = @"Data\dna_modified.trn";
            string testFileName = @"Data\dna_modified.tst";

            RuleQualityFunction identificationFunc = RuleQuality_DEL.ConfidenceW;
            RuleQualityFunction voteFunc = RuleQuality_DEL.ConfidenceW;

            DataStore trainData = DataStore.Load(trainFileName, FileFormat.Rses1);
            DataStore testData = DataStore.Load(testFileName, FileFormat.Rses1, trainData.DataStoreInfo);

            Args args = new Args();
            args.SetParameter(ReductGeneratorParamHelper.TrainData, trainData);
            args.SetParameter(ReductGeneratorParamHelper.Epsilon, epsilon);
            args.SetParameter(ReductGeneratorParamHelper.FactoryKey, reductFactoryKey);

            for (int i = 0; i < loops; i++)
            {    
                args.SetParameter(ReductGeneratorParamHelper.PermutationCollection,
                            ReductFactory.GetPermutationGenerator(args).Generate(numberOfPerm));
                
                IReductGenerator reductGenerator = ReductFactory.GetReductGenerator(args);
                reductGenerator.Run();

                IReductStoreCollection reductStoreCollection = reductGenerator.GetReductStoreCollection();

                var watch_2 = Stopwatch.StartNew();
                RoughClassifier classifier = new RoughClassifier(
                    reductStoreCollection,
                    identificationFunc,
                    voteFunc,
                    trainData.DataStoreInfo.GetDecisionValues());
                ClassificationResult classificationResult = classifier.Classify(testData, null);
                
                for (int objectIdx = 0; objectIdx < testData.NumberOfRecords; objectIdx++)
                    classificationResult.GetPrediction(objectIdx);
                watch_2.Stop();
                sum_2 += (ulong) watch_2.ElapsedMilliseconds;
                
                Console.WriteLine("Accuracy 2: {0}", classificationResult.Accuracy);
                Console.WriteLine();
            }
            Console.WriteLine("Classifier 2: {0}", (decimal)sum_2 / (decimal)loops);
        }
    }
}
