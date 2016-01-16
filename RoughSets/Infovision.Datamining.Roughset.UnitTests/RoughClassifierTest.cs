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
                        args.SetParameter(ReductGeneratorParamHelper.DataStore, trainData);
                        args.SetParameter(ReductGeneratorParamHelper.Epsilon, epsilon);
                        args.SetParameter(ReductGeneratorParamHelper.FactoryKey, reductFactoryKey);

                        args.SetParameter(ReductGeneratorParamHelper.PermutationCollection,
                            ReductFactory.GetPermutationGenerator(args).Generate(numberOfPermutations));

                        IReductGenerator reductGenerator = ReductFactory.GetReductGenerator(args);
                        reductGenerator.Generate();

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

            RuleQualityFunction identificationFunc = RuleQuality.ConfidenceW;
            RuleQualityFunction voteFunc = RuleQuality.ConfidenceW;

            DataStore trainData = DataStore.Load(trainFileName, FileFormat.Rses1);
            DataStore testData = DataStore.Load(testFileName, FileFormat.Rses1, trainData.DataStoreInfo);

            Args args = new Args();
            args.SetParameter(ReductGeneratorParamHelper.DataStore, trainData);
            args.SetParameter(ReductGeneratorParamHelper.Epsilon, epsilon);
            args.SetParameter(ReductGeneratorParamHelper.FactoryKey, reductFactoryKey);

            for (int i = 0; i < loops; i++)
            {    
                args.SetParameter(ReductGeneratorParamHelper.PermutationCollection,
                            ReductFactory.GetPermutationGenerator(args).Generate(numberOfPerm));
                
                IReductGenerator reductGenerator = ReductFactory.GetReductGenerator(args);
                reductGenerator.Generate();

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
