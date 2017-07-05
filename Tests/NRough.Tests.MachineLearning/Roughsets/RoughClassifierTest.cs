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
using System.Diagnostics;
using NRough.Data;
using NRough.Core;
using NUnit.Framework;
using NRough.MachineLearning.Classification;
using NRough.MachineLearning.Roughsets;

namespace NRough.Tests.MachineLearning.Roughsets
{
    [TestFixture]
    public class RoughClassifierTest
    {
        [Test, Ignore("This is not a unit test")]
        public void RoughClassifierNewTest()
        {
            for (IdentificationType identType = IdentificationType.Support; identType <= IdentificationType.WeightCoverage; identType++)
            {
                for (VoteType vType = VoteType.Support; vType <= VoteType.ConfidenceRelative; vType++)
                {
                    for (int i = 0; i < 5; i++)
                    {
                        double epsilon = 0.1;
                        int numberOfPermutations = 10;
                        string reductFactoryKey = ReductTypes.ApproximateReductMajorityWeights;

                        //string trainFileName = @"Data\dna_modified.trn";
                        string testFileName = @"Data\dna_modified.tst";

                        IdentificationType identificationType = identType;
                        VoteType voteType = vType;
                        RuleQualityMethod identificationFunc;
                        RuleQualityMethod voteFunc;

                        switch (identificationType)
                        {
                            case IdentificationType.Support: identificationFunc = RuleQualityMethods.Support; break;
                            case IdentificationType.Confidence: identificationFunc = RuleQualityMethods.Confidence; break;
                            case IdentificationType.Coverage: identificationFunc = RuleQualityMethods.Coverage; break;
                            case IdentificationType.WeightSupport: identificationFunc = RuleQualityMethods.SupportW; break;
                            case IdentificationType.WeightConfidence: identificationFunc = RuleQualityMethods.ConfidenceW; break;
                            case IdentificationType.WeightCoverage: identificationFunc = RuleQualityMethods.CoverageW; break;
                            default: throw new ArgumentException("Unknown key", "identificationType");
                        }

                        switch (voteType)
                        {
                            case VoteType.Support: voteFunc = RuleQualityMethods.Support; break;
                            case VoteType.Confidence: voteFunc = RuleQualityMethods.Confidence; break;
                            case VoteType.Coverage: voteFunc = RuleQualityMethods.Coverage; break;
                            case VoteType.Ratio: voteFunc = RuleQualityMethods.Ratio; break;
                            case VoteType.Strength: voteFunc = RuleQualityMethods.Strength; break;
                            case VoteType.MajorDecision: voteFunc = RuleQualityMethods.SingleVote; break;
                            case VoteType.WeightSupport: voteFunc = RuleQualityMethods.SupportW; break;
                            case VoteType.WeightConfidence: voteFunc = RuleQualityMethods.ConfidenceW; break;
                            case VoteType.WeightCoverage: voteFunc = RuleQualityMethods.CoverageW; break;
                            case VoteType.WeightRatio: voteFunc = RuleQualityMethods.RatioW; break;
                            case VoteType.WeightStrength: voteFunc = RuleQualityMethods.StrengthW; break;
                            case VoteType.ConfidenceRelative: voteFunc = RuleQualityMethods.ConfidenceRelative; break;
                            default: throw new ArgumentException("Unknown key", "voteType");
                        }

                        var trainData = Data.Benchmark.Factory.DnaModifiedTrain();
                        //DataStore trainData = DataStore.Load(trainFileName, DataFormat.RSES1);
                        DataStore testData = DataStore.Load(testFileName, DataFormat.RSES1, trainData.DataStoreInfo);

                        Args args = new Args();
                        args.SetParameter(ReductFactoryOptions.DecisionTable, trainData);
                        args.SetParameter(ReductFactoryOptions.Epsilon, epsilon);
                        args.SetParameter(ReductFactoryOptions.ReductType, reductFactoryKey);

                        args.SetParameter(ReductFactoryOptions.PermutationCollection,
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

            double epsilon = 0.05;
            string reductFactoryKey = ReductTypes.ApproximateReductMajorityWeights;

            //string trainFileName = @"Data\dna_modified.trn";
            string testFileName = @"Data\dna_modified.tst";

            RuleQualityMethod identificationFunc = RuleQualityMethods.ConfidenceW;
            RuleQualityMethod voteFunc = RuleQualityMethods.ConfidenceW;

            var trainData = Data.Benchmark.Factory.DnaModifiedTrain();
            //DataStore trainData = DataStore.Load(trainFileName, DataFormat.RSES1);
            DataStore testData = DataStore.Load(testFileName, DataFormat.RSES1, trainData.DataStoreInfo);

            Args args = new Args();
            args.SetParameter(ReductFactoryOptions.DecisionTable, trainData);
            args.SetParameter(ReductFactoryOptions.Epsilon, epsilon);
            args.SetParameter(ReductFactoryOptions.ReductType, reductFactoryKey);

            for (int i = 0; i < loops; i++)
            {
                args.SetParameter(ReductFactoryOptions.PermutationCollection,
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
                sum_2 += (ulong)watch_2.ElapsedMilliseconds;

                Console.WriteLine("Accuracy 2: {0}", classificationResult.Accuracy);
                Console.WriteLine();
            }
            Console.WriteLine("Classifier 2: {0}", (double)sum_2 / (double)loops);
        }
    }
}