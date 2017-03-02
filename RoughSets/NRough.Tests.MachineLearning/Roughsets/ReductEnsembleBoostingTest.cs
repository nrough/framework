using System;
using System.Collections.Generic;
using System.Linq;
using NRough.Data;
using NRough.MachineLearning.Clustering.Hierarchical;
using NRough.Math;
using NRough.Core;
using NUnit.Framework;
using NRough.MachineLearning.Weighting;
using NRough.MachineLearning.Classification;
using NRough.MachineLearning.Roughsets;
using NRough.Core.Random;

namespace NRough.Tests.MachineLearning.Roughsets
{
    [TestFixture]
    public class ReductEnsembleBoostingTest
    {
        public ReductEnsembleBoostingTest()
        {
            RandomSingleton.Seed = Guid.NewGuid().GetHashCode();
        }

        public static IEnumerable<Dictionary<string, object>> GetGenerateTestArgs()
        {
            var data = Data.Benchmark.Factory.DnaModified();
            //DataStore data = DataStore.Load(@"Data\dna_modified.trn", DataFormat.RSES1);

            List<Dictionary<string, object>> argsList = new List<Dictionary<string, object>>();

            Args inner = new Args();
            inner.SetParameter(ReductFactoryOptions.DecisionTable, data);
            inner.SetParameter(ReductFactoryOptions.ReductType, ReductTypes.GeneralizedMajorityDecisionApproximate);
            inner.SetParameter(ReductFactoryOptions.NumberOfPermutations, 1);
            inner.SetParameter(ReductFactoryOptions.Epsilon, 0.1m);
            inner.SetParameter(ReductFactoryOptions.WeightGenerator, new WeightGeneratorMajority(data));

            Dictionary<string, object> argSet = new Dictionary<string, object>();
            argSet.Add(ReductFactoryOptions.DecisionTable, data);
            argSet.Add(ReductFactoryOptions.ReductType, ReductTypes.ReductEnsembleBoosting);
            argSet.Add(ReductFactoryOptions.IdentificationType, (RuleQualityMethod)RuleQualityMethods.Confidence);
            argSet.Add(ReductFactoryOptions.VoteType, (RuleQualityMethod)RuleQualityMethods.SingleVote);
            argSet.Add(ReductFactoryOptions.MaxReductLength, 4);
            argSet.Add(ReductFactoryOptions.Threshold, 0.5);
            argSet.Add(ReductFactoryOptions.NumberOfReductsInWeakClassifier, 1);
            argSet.Add(ReductFactoryOptions.MaxIterations, 1);
            argSet.Add(ReductFactoryOptions.InnerParameters, inner);
            argsList.Add(argSet);

            argSet = new Dictionary<string, object>();
            argSet.Add(ReductFactoryOptions.DecisionTable, data);
            argSet.Add(ReductFactoryOptions.ReductType, ReductTypes.ReductEnsembleBoosting);
            argSet.Add(ReductFactoryOptions.IdentificationType, (RuleQualityMethod)RuleQualityMethods.Confidence);
            argSet.Add(ReductFactoryOptions.VoteType, (RuleQualityMethod)RuleQualityMethods.SingleVote);
            argSet.Add(ReductFactoryOptions.MaxReductLength, 4);
            argSet.Add(ReductFactoryOptions.Threshold, 0.5);
            argSet.Add(ReductFactoryOptions.NumberOfReductsInWeakClassifier, 1);
            argSet.Add(ReductFactoryOptions.MaxIterations, 3);
            argSet.Add(ReductFactoryOptions.InnerParameters, inner);
            argsList.Add(argSet);

            argSet = new Dictionary<string, object>();
            argSet.Add(ReductFactoryOptions.DecisionTable, data);
            argSet.Add(ReductFactoryOptions.ReductType, ReductTypes.ReductEnsembleBoosting);
            argSet.Add(ReductFactoryOptions.IdentificationType, (RuleQualityMethod)RuleQualityMethods.Confidence);
            argSet.Add(ReductFactoryOptions.VoteType, (RuleQualityMethod)RuleQualityMethods.SingleVote);
            argSet.Add(ReductFactoryOptions.MaxReductLength, 4);
            argSet.Add(ReductFactoryOptions.Threshold, 0.5);
            argSet.Add(ReductFactoryOptions.NumberOfReductsInWeakClassifier, 1);
            argSet.Add(ReductFactoryOptions.MaxIterations, 5);
            argSet.Add(ReductFactoryOptions.InnerParameters, inner);
            argsList.Add(argSet);

            argSet = new Dictionary<string, object>();
            argSet.Add(ReductFactoryOptions.DecisionTable, data);
            argSet.Add(ReductFactoryOptions.ReductType, ReductTypes.ReductEnsembleBoosting);
            argSet.Add(ReductFactoryOptions.IdentificationType, (RuleQualityMethod)RuleQualityMethods.Confidence);
            argSet.Add(ReductFactoryOptions.VoteType, (RuleQualityMethod)RuleQualityMethods.SingleVote);
            argSet.Add(ReductFactoryOptions.MaxReductLength, 4);
            argSet.Add(ReductFactoryOptions.Threshold, 0.5);
            argSet.Add(ReductFactoryOptions.NumberOfReductsInWeakClassifier, 1);
            argSet.Add(ReductFactoryOptions.MaxIterations, 10);
            argSet.Add(ReductFactoryOptions.InnerParameters, inner);
            argsList.Add(argSet);

            argSet = new Dictionary<string, object>();
            argSet.Add(ReductFactoryOptions.DecisionTable, data);
            argSet.Add(ReductFactoryOptions.ReductType, ReductTypes.ReductEnsembleBoosting);
            argSet.Add(ReductFactoryOptions.IdentificationType, (RuleQualityMethod)RuleQualityMethods.Confidence);
            argSet.Add(ReductFactoryOptions.VoteType, (RuleQualityMethod)RuleQualityMethods.SingleVote);
            argSet.Add(ReductFactoryOptions.MaxReductLength, 4);
            argSet.Add(ReductFactoryOptions.Threshold, 0.5);
            argSet.Add(ReductFactoryOptions.NumberOfReductsInWeakClassifier, 1);
            argSet.Add(ReductFactoryOptions.MaxIterations, 20);
            argSet.Add(ReductFactoryOptions.InnerParameters, inner);
            argsList.Add(argSet);

            argSet = new Dictionary<string, object>();
            argSet.Add(ReductFactoryOptions.DecisionTable, data);
            argSet.Add(ReductFactoryOptions.ReductType, ReductTypes.ReductEnsembleBoosting);
            argSet.Add(ReductFactoryOptions.IdentificationType, (RuleQualityMethod)RuleQualityMethods.Confidence);
            argSet.Add(ReductFactoryOptions.VoteType, (RuleQualityMethod)RuleQualityMethods.SingleVote);
            argSet.Add(ReductFactoryOptions.MaxReductLength, 4);
            argSet.Add(ReductFactoryOptions.Threshold, 0.5);
            argSet.Add(ReductFactoryOptions.NumberOfReductsInWeakClassifier, 1);
            argSet.Add(ReductFactoryOptions.MaxIterations, 30);
            argSet.Add(ReductFactoryOptions.InnerParameters, inner);
            argsList.Add(argSet);

            argSet = new Dictionary<string, object>();
            argSet.Add(ReductFactoryOptions.DecisionTable, data);
            argSet.Add(ReductFactoryOptions.ReductType, ReductTypes.ReductEnsembleBoosting);
            argSet.Add(ReductFactoryOptions.IdentificationType, (RuleQualityMethod)RuleQualityMethods.Confidence);
            argSet.Add(ReductFactoryOptions.VoteType, (RuleQualityMethod)RuleQualityMethods.SingleVote);
            argSet.Add(ReductFactoryOptions.MaxReductLength, 4);
            argSet.Add(ReductFactoryOptions.Threshold, 0.5);
            argSet.Add(ReductFactoryOptions.NumberOfReductsInWeakClassifier, 1);
            argSet.Add(ReductFactoryOptions.MaxIterations, 40);
            argSet.Add(ReductFactoryOptions.InnerParameters, inner);
            argsList.Add(argSet);

            argSet = new Dictionary<string, object>();
            argSet.Add(ReductFactoryOptions.DecisionTable, data);
            argSet.Add(ReductFactoryOptions.ReductType, ReductTypes.ReductEnsembleBoosting);
            argSet.Add(ReductFactoryOptions.IdentificationType, (RuleQualityMethod)RuleQualityMethods.Confidence);
            argSet.Add(ReductFactoryOptions.VoteType, (RuleQualityMethod)RuleQualityMethods.SingleVote);
            argSet.Add(ReductFactoryOptions.MaxReductLength, 4);
            argSet.Add(ReductFactoryOptions.Threshold, 0.5);
            argSet.Add(ReductFactoryOptions.NumberOfReductsInWeakClassifier, 1);
            argSet.Add(ReductFactoryOptions.MaxIterations, 50);
            argSet.Add(ReductFactoryOptions.InnerParameters, inner);
            argsList.Add(argSet);

            argSet = new Dictionary<string, object>();
            argSet.Add(ReductFactoryOptions.DecisionTable, data);
            argSet.Add(ReductFactoryOptions.ReductType, ReductTypes.ReductEnsembleBoosting);
            argSet.Add(ReductFactoryOptions.IdentificationType, (RuleQualityMethod)RuleQualityMethods.Confidence);
            argSet.Add(ReductFactoryOptions.VoteType, (RuleQualityMethod)RuleQualityMethods.SingleVote);
            argSet.Add(ReductFactoryOptions.MaxReductLength, 4);
            argSet.Add(ReductFactoryOptions.Threshold, 0.5);
            argSet.Add(ReductFactoryOptions.NumberOfReductsInWeakClassifier, 1);
            argSet.Add(ReductFactoryOptions.MaxIterations, 60);
            argSet.Add(ReductFactoryOptions.InnerParameters, inner);
            argsList.Add(argSet);

            argSet = new Dictionary<string, object>();
            argSet.Add(ReductFactoryOptions.DecisionTable, data);
            argSet.Add(ReductFactoryOptions.ReductType, ReductTypes.ReductEnsembleBoosting);
            argSet.Add(ReductFactoryOptions.IdentificationType, (RuleQualityMethod)RuleQualityMethods.Confidence);
            argSet.Add(ReductFactoryOptions.VoteType, (RuleQualityMethod)RuleQualityMethods.SingleVote);
            argSet.Add(ReductFactoryOptions.MaxReductLength, 4);
            argSet.Add(ReductFactoryOptions.Threshold, 0.5);
            argSet.Add(ReductFactoryOptions.NumberOfReductsInWeakClassifier, 1);
            argSet.Add(ReductFactoryOptions.MaxIterations, 70);
            argSet.Add(ReductFactoryOptions.InnerParameters, inner);
            argsList.Add(argSet);

            argSet = new Dictionary<string, object>();
            argSet.Add(ReductFactoryOptions.DecisionTable, data);
            argSet.Add(ReductFactoryOptions.ReductType, ReductTypes.ReductEnsembleBoosting);
            argSet.Add(ReductFactoryOptions.IdentificationType, (RuleQualityMethod)RuleQualityMethods.Confidence);
            argSet.Add(ReductFactoryOptions.VoteType, (RuleQualityMethod)RuleQualityMethods.SingleVote);
            argSet.Add(ReductFactoryOptions.MaxReductLength, 4);
            argSet.Add(ReductFactoryOptions.Threshold, 0.5);
            argSet.Add(ReductFactoryOptions.NumberOfReductsInWeakClassifier, 1);
            argSet.Add(ReductFactoryOptions.MaxIterations, 80);
            argSet.Add(ReductFactoryOptions.InnerParameters, inner);
            argsList.Add(argSet);

            argSet = new Dictionary<string, object>();
            argSet.Add(ReductFactoryOptions.DecisionTable, data);
            argSet.Add(ReductFactoryOptions.ReductType, ReductTypes.ReductEnsembleBoosting);
            argSet.Add(ReductFactoryOptions.IdentificationType, (RuleQualityMethod)RuleQualityMethods.Confidence);
            argSet.Add(ReductFactoryOptions.VoteType, (RuleQualityMethod)RuleQualityMethods.SingleVote);
            argSet.Add(ReductFactoryOptions.MaxReductLength, 4);
            argSet.Add(ReductFactoryOptions.Threshold, 0.5);
            argSet.Add(ReductFactoryOptions.NumberOfReductsInWeakClassifier, 1);
            argSet.Add(ReductFactoryOptions.MaxIterations, 90);
            argSet.Add(ReductFactoryOptions.InnerParameters, inner);
            argsList.Add(argSet);

            argSet = new Dictionary<string, object>();
            argSet.Add(ReductFactoryOptions.DecisionTable, data);
            argSet.Add(ReductFactoryOptions.ReductType, ReductTypes.ReductEnsembleBoosting);
            argSet.Add(ReductFactoryOptions.IdentificationType, (RuleQualityMethod)RuleQualityMethods.Confidence);
            argSet.Add(ReductFactoryOptions.VoteType, (RuleQualityMethod)RuleQualityMethods.SingleVote);
            argSet.Add(ReductFactoryOptions.MaxReductLength, 4);
            argSet.Add(ReductFactoryOptions.Threshold, 0.5);
            argSet.Add(ReductFactoryOptions.NumberOfReductsInWeakClassifier, 1);
            argSet.Add(ReductFactoryOptions.MaxIterations, 100);
            argSet.Add(ReductFactoryOptions.InnerParameters, inner);
            argsList.Add(argSet);

            argSet = new Dictionary<string, object>();
            argSet.Add(ReductFactoryOptions.DecisionTable, data);
            argSet.Add(ReductFactoryOptions.ReductType, ReductTypes.ReductEnsembleBoosting);
            argSet.Add(ReductFactoryOptions.IdentificationType, (RuleQualityMethod)RuleQualityMethods.Confidence);
            argSet.Add(ReductFactoryOptions.VoteType, (RuleQualityMethod)RuleQualityMethods.SingleVote);
            argSet.Add(ReductFactoryOptions.MaxReductLength, 4);
            argSet.Add(ReductFactoryOptions.Threshold, 0.5);
            argSet.Add(ReductFactoryOptions.NumberOfReductsInWeakClassifier, 1);
            argSet.Add(ReductFactoryOptions.MaxIterations, 200);
            argSet.Add(ReductFactoryOptions.InnerParameters, inner);
            argsList.Add(argSet);

            argSet = new Dictionary<string, object>();
            argSet.Add(ReductFactoryOptions.DecisionTable, data);
            argSet.Add(ReductFactoryOptions.ReductType, ReductTypes.ReductEnsembleBoosting);
            argSet.Add(ReductFactoryOptions.IdentificationType, (RuleQualityMethod)RuleQualityMethods.Confidence);
            argSet.Add(ReductFactoryOptions.VoteType, (RuleQualityMethod)RuleQualityMethods.SingleVote);
            argSet.Add(ReductFactoryOptions.MaxReductLength, 4);
            argSet.Add(ReductFactoryOptions.Threshold, 0.5);
            argSet.Add(ReductFactoryOptions.NumberOfReductsInWeakClassifier, 1);
            argSet.Add(ReductFactoryOptions.MaxIterations, 300);
            argSet.Add(ReductFactoryOptions.InnerParameters, inner);
            argsList.Add(argSet);

            return argsList;
        }

        [Test, TestCaseSource("GetGenerateTestArgs"), Ignore("NoReason")]
        public void GenerateTest(Dictionary<string, object> args)
        {
            Args parms = new Args();
            foreach (KeyValuePair<string, object> kvp in args)
                parms.SetParameter(kvp.Key, kvp.Value);

            ReductEnsembleBoostingGenerator reductGenerator = ReductFactory.GetReductGenerator(parms) as ReductEnsembleBoostingGenerator;
            reductGenerator.Run();

            DataStore data = (DataStore)parms.GetParameter(ReductFactoryOptions.DecisionTable);

            RoughClassifier classifierTrn = new RoughClassifier(
                        reductGenerator.GetReductGroups(),
                        reductGenerator.IdentyficationType,
                        reductGenerator.VoteType,
                        data.DataStoreInfo.GetDecisionValues());
            ClassificationResult resultTrn = classifierTrn.Classify(data, null);
        }

        [TestCase(0.1, 100, 10)]
        [TestCase(0.2, 100, 10)]
        [TestCase(0.3, 100, 10)]
        [TestCase(0.4, 100, 10)]
        [TestCase(0.5, 100, 10)]
        [TestCase(0.6, 100, 10)]
        [TestCase(0.7, 100, 10)]
        [TestCase(0.8, 100, 10)]
        [TestCase(0.9, 100, 10)]
        [TestCase(0.0, 100, 10)]
        public void GenerateBoostingTestNullExceptionError(double epsilon, int iterations, int weak)
        {
            var train = Data.Benchmark.Factory.DnaModified();
            //DataStore train = DataStore.Load(@"Data\dna_modified.trn", DataFormat.RSES1);
            DataStore test = DataStore.Load(@"Data\dna_modified.tst", DataFormat.RSES1, train.DataStoreInfo);

            //-------------------------------------- Parameters --------------------------

            string factoryKey = ReductTypes.ReductEnsembleBoosting;
            int numberOfPermutations = 100;

            RuleQualityMethod identificationFunction = RuleQualityMethods.CoverageW;
            RuleQualityMethod voteFunction = RuleQualityMethods.CoverageW;
            WeightGeneratorType weightGeneratorType = WeightGeneratorType.Relative;

            string innerFactoryKey = ReductTypes.ApproximateReductRelativeWeights;
            double innerEpsilon = 0.1;

            RuleQualityMethod boostingIdentificationFunction = null;
            RuleQualityMethod boostingVoteFunction = null;
            UpdateWeightsDelegate boostingUpdateWeights = ReductEnsembleBoostingGenerator.UpdateWeightsAdaBoost_All;
            CalcModelConfidenceDelegate boostingCalcModelConfidence = ReductEnsembleBoostingGenerator.ModelConfidenceAdaBoostM1;
            bool boostingCheckEnsambleErrorDuringTraining = false;

            if (boostingIdentificationFunction == null)
                boostingIdentificationFunction = identificationFunction;

            if (boostingVoteFunction == null)
                boostingVoteFunction = voteFunction;

            //----------------------------------------------------------------------------

            WeightGenerator wGen = WeightGenerator.Construct(weightGeneratorType, train);

            Args innerArgs = new Args();
            innerArgs.SetParameter(ReductFactoryOptions.DecisionTable, train);
            innerArgs.SetParameter(ReductFactoryOptions.ReductType, innerFactoryKey);
            innerArgs.SetParameter(ReductFactoryOptions.Epsilon, innerEpsilon);
            innerArgs.SetParameter(ReductFactoryOptions.WeightGenerator, wGen);
            innerArgs.SetParameter(ReductFactoryOptions.ReductionStep, (int)(train.DataStoreInfo.CountAttributes(a => a.IsStandard) * 0.1)); //10% reduction step

            Args args = new Args();
            args.SetParameter(ReductFactoryOptions.DecisionTable, train);
            args.SetParameter(ReductFactoryOptions.ReductType, factoryKey);
            args.SetParameter(ReductFactoryOptions.Epsilon, epsilon);
            args.SetParameter(ReductFactoryOptions.NumberOfPermutations, numberOfPermutations);
            args.SetParameter(ReductFactoryOptions.WeightGenerator, wGen);

            args.SetParameter(ReductFactoryOptions.NumberOfReductsInWeakClassifier, weak);
            args.SetParameter(ReductFactoryOptions.IdentificationType, boostingIdentificationFunction);
            args.SetParameter(ReductFactoryOptions.VoteType, boostingVoteFunction);
            args.SetParameter(ReductFactoryOptions.UpdateWeights, boostingUpdateWeights);
            args.SetParameter(ReductFactoryOptions.CalcModelConfidence, boostingCalcModelConfidence);
            args.SetParameter(ReductFactoryOptions.MaxIterations, iterations);
            args.SetParameter(ReductFactoryOptions.CheckEnsembleErrorDuringTraining, boostingCheckEnsambleErrorDuringTraining);
            args.SetParameter(ReductFactoryOptions.InnerParameters, innerArgs);

            IReductGenerator generator = ReductFactory.GetReductGenerator(args);
            generator.Run();
            IReductStoreCollection reductStoreCollection = generator.GetReductStoreCollection();

            Assert.NotNull(reductStoreCollection);

            RoughClassifier classifier = new RoughClassifier(reductStoreCollection, identificationFunction, voteFunction, train.DataStoreInfo.GetDecisionValues());
            ClassificationResult result = classifier.Classify(test);
            Console.WriteLine(result.Accuracy);
        }

        [Test]
        public void GenerateExperimentBoostingStandard()
        {
            var trnData = Data.Benchmark.Factory.DnaModified();
            //DataStore trnData = DataStore.Load(@"Data\dna_modified.trn", DataFormat.RSES1);
            DataStore tstData = DataStore.Load(@"Data\dna_modified.tst", DataFormat.RSES1, trnData.DataStoreInfo);
            WeightingSchema weightingSchema = WeightingSchema.Majority;
            int numberOfTests = 1;
            int maxNumberOfIterations = 100;

            Console.WriteLine("{0} {1} {2} {3} {4} {5} {6} {7} {8} {9} {10} {11} {12}",
                                     "METHOD",
                                     "IDENTYFICATION",
                                     "VOTETYPE",
                //"MIN_LEN",
                                     "UPDATE_WEIGHTS",
                                     "WEIGHT_TYPE",
                                     "CHECK_ENSEBLE_ERROR",
                                     "TESTID",
                                     "MAXITER",
                                     "NOF_MODELS",
                                     "NOF_WRESET",
                                     "TRN_ERROR",
                                     "TST_ERROR",
                                     "AVG_REDUCT");

            for (int iter = 3; iter <= maxNumberOfIterations; iter++)
            {
                for (int t = 0; t < numberOfTests; t++)
                {
                    Args parms = new Args();
                    parms.SetParameter(ReductFactoryOptions.DecisionTable, trnData);
                    parms.SetParameter(ReductFactoryOptions.ReductType, ReductTypes.ReductEnsembleBoosting);
                    parms.SetParameter(ReductFactoryOptions.IdentificationType, (RuleQualityMethod)RuleQualityMethods.ConfidenceW);
                    parms.SetParameter(ReductFactoryOptions.VoteType, (RuleQualityMethod)RuleQualityMethods.ConfidenceW);
                    //args.SetParameter(ReductGeneratorParamHelper.MinReductLength, 1);
                    parms.SetParameter(ReductFactoryOptions.NumberOfReductsInWeakClassifier, 10);
                    parms.SetParameter(ReductFactoryOptions.MaxIterations, iter);

                    WeightGenerator weightGenerator;
                    switch (weightingSchema)
                    {
                        case WeightingSchema.Majority:
                            weightGenerator = new WeightGeneratorMajority(trnData);
                            break;

                        case WeightingSchema.Relative:
                            weightGenerator = new WeightGeneratorRelative(trnData);
                            break;

                        default:
                            weightGenerator = new WeightBoostingGenerator(trnData);
                            break;
                    }

                    parms.SetParameter(ReductFactoryOptions.WeightGenerator, weightGenerator);
                    parms.SetParameter(ReductFactoryOptions.CheckEnsembleErrorDuringTraining, false);

                    Args innerArgs = new Args();
                    innerArgs.SetParameter(ReductFactoryOptions.DecisionTable, trnData);
                    innerArgs.SetParameter(ReductFactoryOptions.ReductType, ReductTypes.ApproximateReductRelativeWeights);
                    innerArgs.SetParameter(ReductFactoryOptions.Epsilon, 0.2);
                    innerArgs.SetParameter(ReductFactoryOptions.WeightGenerator, weightGenerator);
                    innerArgs.SetParameter(ReductFactoryOptions.ReductionStep, (int)(trnData.DataStoreInfo.CountAttributes(a => a.IsStandard) * 0.1)); //10% reduction step

                    parms.SetParameter(ReductFactoryOptions.InnerParameters, innerArgs);

                    ReductEnsembleBoostingGenerator reductGenerator = ReductFactory.GetReductGenerator(parms) as ReductEnsembleBoostingGenerator;
                    reductGenerator.Run();

                    IReductStoreCollection reductStoreCollection = reductGenerator.GetReductStoreCollection();

                    RoughClassifier classifierTrn = new RoughClassifier(
                        reductStoreCollection,
                        reductGenerator.IdentyficationType,
                        reductGenerator.VoteType,
                        trnData.DataStoreInfo.GetDecisionValues());
                    ClassificationResult resultTrn = classifierTrn.Classify(trnData, null);

                    RoughClassifier classifierTst = new RoughClassifier(
                        reductStoreCollection,
                        reductGenerator.IdentyficationType,
                        reductGenerator.VoteType,
                        trnData.DataStoreInfo.GetDecisionValues());
                    ClassificationResult resultTst = classifierTst.Classify(tstData, null);

                    Console.WriteLine("{0} {1} {2} {3} {4} {5} {6} {7} {8} {9} {10} {11} {12}",
                                      parms.GetParameter(ReductFactoryOptions.ReductType),
                                      reductGenerator.IdentyficationType.Method.Name,
                                      reductGenerator.VoteType.Method.Name,
                        //reductGenerator.MinReductLength,
                                      reductGenerator.UpdateWeights.Method.Name,
                                      weightingSchema,
                                      reductGenerator.CheckEnsembleErrorDuringTraining,
                                      t + 1,
                                      reductGenerator.MaxIterations,
                                      reductGenerator.IterationsPassed,
                                      reductGenerator.NumberOfWeightResets,
                                      resultTrn.WeightMisclassified + resultTrn.WeightUnclassified,
                                      resultTst.WeightMisclassified + resultTst.WeightUnclassified,
                                      reductStoreCollection.First().GetAvgMeasure(new ReductMeasureLength()));
                }
            }
        }

        [Test]
        public void GenerateExperimentBoostingVarEps()
        {
            //Console.WriteLine("GenerateExperimentBoostingStandard");

            //TrainData trnData = TrainData.Load(@"Data\dna_modified.trn", FileFormat.Rses1);
            //TrainData tstData = TrainData.Load(@"Data\dna_modified.tst", FileFormat.Rses1, trnData.DataStoreInfo);

            DataStore trnData = DataStore.Load(@"Data\optdigits.trn", DataFormat.RSES1);
            DataStore tstData = DataStore.Load(@"Data\optdigits.tst", DataFormat.RSES1, trnData.DataStoreInfo);

            WeightingSchema weightingSchema = WeightingSchema.Majority;
            int numberOfTests = 2;
            //int maxNumberOfIterations = 100;

            /*
            Console.WriteLine("{0} {1} {2} {3} {4} {5} {6} {7} {8} {9} {10} {11} {12} {13}",
                                     "METHOD",
                                     "IDENTYFICATION",
                                     "VOTETYPE",
                                     "MIN_LEN",
                                     "UPDATE_WEIGHTS",
                                     "WEIGHT_TYPE",
                                     "CHECK_ENSEBLE_ERROR",
                                     "TESTID",
                                     "MAXITER",
                                     "NOF_MODELS",
                                     "NOF_WRESET",
                                     "TRN_ERROR",
                                     "TST_ERROR",
                                     "AVG_REDUCT");
            */
            //List<int> iterList = new List<int>(new int[] { 1, 2, 5, 10, 20, 50, 100 });

            List<int> iterList = new List<int>(new int[] { 1, 2, 5, 10 });

            //for (int iter = 1; iter <= maxNumberOfIterations; iter++)
            foreach (int iter in iterList)
            {
                for (int t = 0; t < numberOfTests; t++)
                {
                    Args parms = new Args();
                    parms.SetParameter(ReductFactoryOptions.DecisionTable, trnData);
                    parms.SetParameter(ReductFactoryOptions.ReductType, ReductTypes.ReductEnsembleBoostingVarEps);
                    parms.SetParameter(ReductFactoryOptions.IdentificationType, (RuleQualityMethod)RuleQualityMethods.ConfidenceW);
                    parms.SetParameter(ReductFactoryOptions.VoteType, (RuleQualityMethod)RuleQualityMethods.ConfidenceW);
                    parms.SetParameter(ReductFactoryOptions.MaxIterations, iter);

                    WeightGenerator weightGenerator;
                    switch (weightingSchema)
                    {
                        case WeightingSchema.Majority:
                            weightGenerator = new WeightGeneratorMajority(trnData);
                            break;

                        case WeightingSchema.Relative:
                            weightGenerator = new WeightGeneratorRelative(trnData);
                            break;

                        default:
                            weightGenerator = new WeightBoostingGenerator(trnData);
                            break;
                    }

                    parms.SetParameter(ReductFactoryOptions.WeightGenerator, weightGenerator);
                    parms.SetParameter(ReductFactoryOptions.CheckEnsembleErrorDuringTraining, false);

                    Args innerArgs = new Args();
                    innerArgs.SetParameter(ReductFactoryOptions.DecisionTable, trnData);
                    innerArgs.SetParameter(ReductFactoryOptions.ReductType, ReductTypes.ApproximateReductRelativeWeights);
                    innerArgs.SetParameter(ReductFactoryOptions.Epsilon, 0.1);
                    innerArgs.SetParameter(ReductFactoryOptions.WeightGenerator, weightGenerator);
                    innerArgs.SetParameter(ReductFactoryOptions.ReductionStep, (int)(trnData.DataStoreInfo.CountAttributes(a => a.IsStandard) * 0.1)); //10% reduction step

                    parms.SetParameter(ReductFactoryOptions.InnerParameters, innerArgs);

                    ReductEnsembleBoostingVarEpsGenerator reductGenerator = ReductFactory.GetReductGenerator(parms) as ReductEnsembleBoostingVarEpsGenerator;
                    reductGenerator.Run();

                    RoughClassifier classifierTrn = new RoughClassifier(
                        reductGenerator.GetReductGroups(),
                        reductGenerator.IdentyficationType,
                        reductGenerator.VoteType,
                        trnData.DataStoreInfo.GetDecisionValues());
                    ClassificationResult resultTrn = classifierTrn.Classify(trnData, null);

                    RoughClassifier classifierTst = new RoughClassifier(
                        reductGenerator.GetReductGroups(),
                        reductGenerator.IdentyficationType,
                        reductGenerator.VoteType,
                        trnData.DataStoreInfo.GetDecisionValues());
                    ClassificationResult resultTst = classifierTst.Classify(tstData, null);

                    /*
                    Console.WriteLine("{0} {1} {2} {3} {4} {5} {6} {7} {8} {9} {10} {11} {12} {13}",
                                      args.GetParameter(ReductGeneratorParamHelper.FactoryKey),
                                      reductGenerator.IdentyficationType,
                                      reductGenerator.VoteType,
                                      reductGenerator.MinReductLength,
                                      reductGenerator.SetWeights,
                                      weightingSchema,
                                      reductGenerator.CheckEnsembleErrorDuringTraining,
                                      t + 1,
                                      reductGenerator.MaxIterations,
                                      reductGenerator.IterationsPassed,
                                      reductGenerator.NumberOfWeightResets,
                                      resultTrn.WeightMisclassified + resultTrn.WeightUnclassified,
                                      resultTst.WeightMisclassified + resultTst.WeightUnclassified,
                                      reductGenerator.ReductPool.GetAvgMeasure(new ReductMeasureLength()));
                    */
                }
            }
        }

        [Test, Ignore("NoReason")]
        public void GenerateExperimentBoostingWithDiversity()
        {
            //Console.WriteLine("GenerateExperimentBoostingWithDiversity");

            var trnData = Data.Benchmark.Factory.DnaModified();
            //DataStore trnData = DataStore.Load(@"Data\dna_modified.trn", DataFormat.RSES1);
            DataStore tstData = DataStore.Load(@"Data\dna_modified.tst", DataFormat.RSES1, trnData.DataStoreInfo);

            /*
            Console.WriteLine("{0} {1} {2} {3} {4} {5} {6}",
                                      "TESTID",
                                      "MAXITER",
                                      "NOF_MODELS",
                                      "NOF_WRESET",
                                      "TRN_ERROR",
                                      "TST_ERROR",
                                      "AVG_REDUCT");
            */

            for (int iter = 1; iter <= 2; iter++)
            {
                for (int t = 0; t < 2; t++)
                {
                    Args parms = new Args();
                    parms.SetParameter(ReductFactoryOptions.DecisionTable, trnData);
                    parms.SetParameter(ReductFactoryOptions.ReductType, ReductTypes.ReductEnsembleBoostingWithDiversity);
                    parms.SetParameter(ReductFactoryOptions.IdentificationType, (RuleQualityMethod)RuleQualityMethods.ConfidenceW);
                    parms.SetParameter(ReductFactoryOptions.ReconWeights, (Func<IReduct, double[], RuleQualityMethod, double[]>)ReductToVectorConversionMethods.GetCorrectReconWeights);
                    parms.SetParameter(ReductFactoryOptions.Distance, (Func<double[], double[], double>)Distance.Manhattan);
                    parms.SetParameter(ReductFactoryOptions.Linkage, (Func<int[], int[], DistanceMatrix, double[][], double>)ClusteringLinkage.Complete);
                    parms.SetParameter(ReductFactoryOptions.VoteType, (RuleQualityMethod)RuleQualityMethods.ConfidenceW);
                    parms.SetParameter(ReductFactoryOptions.MinReductLength, 2);
                    //args.SetParameter(ReductGeneratorParamHelper.MaxReductLength, 5);
                    parms.SetParameter(ReductFactoryOptions.NumberOfReductsInWeakClassifier, 1);
                    parms.SetParameter(ReductFactoryOptions.MaxIterations, iter);
                    parms.SetParameter(ReductFactoryOptions.NumberOfReductsToTest, 20);
                    parms.SetParameter(ReductFactoryOptions.AgregateFunction, AgregateFunction.Max);

                    Args innerArgs = new Args();
                    innerArgs.SetParameter(ReductFactoryOptions.DecisionTable, trnData);
                    innerArgs.SetParameter(ReductFactoryOptions.ReductType, ReductTypes.ApproximateReductRelativeWeights);
                    innerArgs.SetParameter(ReductFactoryOptions.Epsilon, 0.1);
                    innerArgs.SetParameter(ReductFactoryOptions.WeightGenerator, new WeightGeneratorRelative(trnData));
                    innerArgs.SetParameter(ReductFactoryOptions.ReductionStep, (int)(trnData.DataStoreInfo.CountAttributes(a => a.IsStandard) * 0.1)); //10% reduction step

                    parms.SetParameter(ReductFactoryOptions.InnerParameters, innerArgs);

                    ReductEnsembleBoostingWithDiversityGenerator reductGenerator = ReductFactory.GetReductGenerator(parms) as ReductEnsembleBoostingWithDiversityGenerator;
                    reductGenerator.Run();

                    RoughClassifier classifierTrn = new RoughClassifier(
                        reductGenerator.GetReductGroups(),
                        reductGenerator.IdentyficationType,
                        reductGenerator.VoteType,
                        trnData.DataStoreInfo.GetDecisionValues());
                    ClassificationResult resultTrn = classifierTrn.Classify(trnData);

                    RoughClassifier classifierTst = new RoughClassifier(
                        reductGenerator.GetReductGroups(),
                        reductGenerator.IdentyficationType,
                        reductGenerator.VoteType,
                        trnData.DataStoreInfo.GetDecisionValues());
                    ClassificationResult resultTst = classifierTst.Classify(tstData, null);

                    /*
                    Console.WriteLine("{0} {1} {2} {3} {4} {5} {6}",
                                      t + 1,
                                      reductGenerator.MaxIterations,
                                      reductGenerator.IterationsPassed,
                                      reductGenerator.NumberOfWeightResets,
                                      resultTrn.WeightMisclassified + resultTrn.WeightUnclassified,
                                      resultTst.WeightMisclassified + resultTst.WeightUnclassified,
                                      reductGenerator.ReductPool.GetAvgMeasure(new ReductMeasureLength()));
                    */
                }
            }
        }

        [Test]
        public void GenerateExperimentBoostingWithAttributeDiversity()
        {
            //Console.WriteLine("GenerateExperimentBoostingWithAttributeDiversity");

            DataStore trnData = DataStore.Load(@"Data\pendigits.trn", DataFormat.RSES1);
            DataStore tstData = DataStore.Load(@"Data\pendigits.tst", DataFormat.RSES1, trnData.DataStoreInfo);

            WeightingSchema weightingSchema = WeightingSchema.Majority;
            int numberOfTests = 1;
            int maxNumberOfIterations = 2;

            /*
            Console.WriteLine("{0} {1} {2} {3} {4} {5} {6} {7} {8} {9} {10} {11} {12} {13}",
                                     "METHOD",
                                     "IDENTYFICATION",
                                     "VOTETYPE",
                                     "MIN_LEN",
                                     "UPDATE_WEIGHTS",
                                     "WEIGHT_TYPE",
                                     "CHECK_ENSEBLE_ERROR",
                                     "TESTID",
                                     "MAXITER",
                                     "NOF_MODELS",
                                     "NOF_WRESET",
                                     "TRN_ERROR",
                                     "TST_ERROR",
                                     "AVG_REDUCT");
            */

            for (int iter = 1; iter <= maxNumberOfIterations; iter++)
            {
                for (int t = 0; t < numberOfTests; t++)
                {
                    Args parms = new Args();
                    parms.SetParameter(ReductFactoryOptions.DecisionTable, trnData);
                    parms.SetParameter(ReductFactoryOptions.ReductType, ReductTypes.ReductEnsembleBoostingWithAttributeDiversity);
                    parms.SetParameter(ReductFactoryOptions.IdentificationType, (RuleQualityMethod)RuleQualityMethods.ConfidenceW);
                    parms.SetParameter(ReductFactoryOptions.VoteType, (RuleQualityMethod)RuleQualityMethods.ConfidenceW);
                    parms.SetParameter(ReductFactoryOptions.NumberOfReductsInWeakClassifier, 1);
                    parms.SetParameter(ReductFactoryOptions.MaxIterations, iter);

                    WeightGenerator weightGenerator;
                    switch (weightingSchema)
                    {
                        case WeightingSchema.Majority:
                            weightGenerator = new WeightGeneratorMajority(trnData);
                            break;

                        case WeightingSchema.Relative:
                            weightGenerator = new WeightGeneratorRelative(trnData);
                            break;

                        default:
                            weightGenerator = new WeightBoostingGenerator(trnData);
                            break;
                    }

                    parms.SetParameter(ReductFactoryOptions.WeightGenerator, weightGenerator);
                    parms.SetParameter(ReductFactoryOptions.CheckEnsembleErrorDuringTraining, false);

                    Args innerArgs = new Args();
                    innerArgs.SetParameter(ReductFactoryOptions.DecisionTable, trnData);
                    innerArgs.SetParameter(ReductFactoryOptions.ReductType, ReductTypes.ApproximateReductRelativeWeights);
                    innerArgs.SetParameter(ReductFactoryOptions.Epsilon, 0.1);
                    innerArgs.SetParameter(ReductFactoryOptions.WeightGenerator, weightGenerator);
                    innerArgs.SetParameter(ReductFactoryOptions.ReductionStep, (int)(trnData.DataStoreInfo.CountAttributes(a => a.IsStandard) * 0.1)); //10% reduction step

                    parms.SetParameter(ReductFactoryOptions.InnerParameters, innerArgs);

                    ReductEnsembleBoostingWithAttributeDiversityGenerator reductGenerator = ReductFactory.GetReductGenerator(parms) as ReductEnsembleBoostingWithAttributeDiversityGenerator;
                    reductGenerator.Run();

                    RoughClassifier classifierTrn = new RoughClassifier(
                        reductGenerator.GetReductGroups(),
                        reductGenerator.IdentyficationType,
                        reductGenerator.VoteType,
                        trnData.DataStoreInfo.GetDecisionValues());
                    ClassificationResult resultTrn = classifierTrn.Classify(trnData, null);

                    RoughClassifier classifierTst = new RoughClassifier(
                        reductGenerator.GetReductGroups(),
                        reductGenerator.IdentyficationType,
                        reductGenerator.VoteType,
                        trnData.DataStoreInfo.GetDecisionValues());
                    ClassificationResult resultTst = classifierTst.Classify(tstData, null);

                    /*
                    Console.WriteLine("{0} {1} {2} {3} {4} {5} {6} {7} {8} {9} {10} {11} {12} {13}",
                                      args.GetParameter(ReductGeneratorParamHelper.FactoryKey),
                                      reductGenerator.IdentyficationType,
                                      reductGenerator.VoteType,
                                      reductGenerator.MinReductLength,
                                      reductGenerator.SetWeights,
                                      weightingSchema,
                                      reductGenerator.CheckEnsembleErrorDuringTraining,
                                      t + 1,
                                      reductGenerator.MaxIterations,
                                      reductGenerator.IterationsPassed,
                                      reductGenerator.NumberOfWeightResets,
                                      resultTrn.WeightMisclassified + resultTrn.WeightUnclassified,
                                      resultTst.WeightMisclassified + resultTst.WeightUnclassified,
                                      reductGenerator.ReductPool.GetAvgMeasure(new ReductMeasureLength()));
                    */
                }
            }
        }
    }
}