using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Common.Logging;
using Common.Logging.Configuration;
using NRough.Data;
using NRough.MachineLearning;
using NRough.MachineLearning.Benchmark;
using NRough.MachineLearning.Roughsets;
using NRough.Core;
using NRough.MachineLearning.Weighting;
using NRough.MachineLearning.Permutations;
using NRough.MachineLearning.Classification;
using NRough.Core.Random;

namespace ExceptionRulesTest
{
    public class Program
    {
        public void ExceptiodnRulesTest(KeyValuePair<string, BenchmarkData> kvp, int numberOfTests, int numberOfPermutations, int ensembleSize)
        {
            DataStore trainData = null, testData = null, data = null;
            string filename = Path.Combine(@"log", kvp.Value.Name + String.Format("-{0}", ensembleSize) + ".result");
            DataSplitter splitter = null;

            ClassificationResult[, ,] results1 = new ClassificationResult[numberOfTests, 100, kvp.Value.CrossValidationFolds];
            ClassificationResult[, ,] results2 = new ClassificationResult[numberOfTests, 100, kvp.Value.CrossValidationFolds];
            ClassificationResult[, ,] results3 = new ClassificationResult[numberOfTests, 100, kvp.Value.CrossValidationFolds];
            ClassificationResult[, ,] results4 = new ClassificationResult[numberOfTests, 100, kvp.Value.CrossValidationFolds];
            ClassificationResult[, ,] results5 = new ClassificationResult[numberOfTests, 100, kvp.Value.CrossValidationFolds];

            if (kvp.Value.CrossValidationActive)
            {
                data = DataStore.Load(kvp.Value.DataFile, kvp.Value.FileFormat);

                if (kvp.Value.DecisionFieldId != -1)
                    data.SetDecisionFieldId(kvp.Value.DecisionFieldId);

                splitter = new DataSplitter(data, kvp.Value.CrossValidationFolds);
            }

            for (int f = 0; f < kvp.Value.CrossValidationFolds; f++)
            {
                if (kvp.Value.CrossValidationActive)
                {
                    trainData = null;
                    testData = null;                    
                    splitter.Split(out trainData, out testData, f);
                }
                else if (f == 0)
                {
                    trainData = DataStore.Load(kvp.Value.TrainFile, kvp.Value.FileFormat);
                    if (kvp.Value.DecisionFieldId != -1)
                        trainData.SetDecisionFieldId(kvp.Value.DecisionFieldId);

                    testData = DataStore.Load(kvp.Value.TestFile, kvp.Value.FileFormat, trainData.DataStoreInfo);
                    if (kvp.Value.DecisionFieldId != -1)
                        testData.SetDecisionFieldId(kvp.Value.DecisionFieldId);
                }

                double mA = new InformationMeasureMajority().Calc(
                    new Reduct(
                        trainData,
                        trainData.DataStoreInfo.SelectAttributeIds(a => a.IsStandard),
                        0.0,
                        new WeightGeneratorMajority(trainData).Weights));

                for (int t = 0; t < numberOfTests; t++)
                {
                    var permGenerator = new PermutationGenerator(trainData);
                    var permList = permGenerator.Generate(numberOfPermutations);

                    log.InfoFormat(
                        "{0} Test:{1}/{2} Fold:{3}/{4} M(A)={5}",
                        trainData.Name,
                        t,
                        numberOfTests - 1,
                        f,
                        kvp.Value.CrossValidationFolds - 1,
                        mA);

                    ParallelOptions options = new ParallelOptions();
                    options.MaxDegreeOfParallelism = ConfigManager.MaxDegreeOfParallelism;

                    Parallel.For(0, 100, options, i =>
                    //for(int i=0; i<100; i++)
                    {
                        var accuracy = this.ExceptionRulesSingleRun(trainData, testData, permList, i, ensembleSize);

                        results1[t, i, f] = accuracy.Item1;
                        results2[t, i, f] = accuracy.Item2;
                        results3[t, i, f] = accuracy.Item3;
                        results4[t, i, f] = accuracy.Item4;
                        results5[t, i, f] = accuracy.Item5;

                        Console.WriteLine("GMDR|{0,2}|{1,2}|{2,3}|{3}", f, t, i, results1[t, i, f]);
                        Console.WriteLine("ARD|{0,2}|{1,2}|{2,3}|{3}", f, t, i, results2[t, i, f]);
                        Console.WriteLine("GAMDR+Ex|{0,2}|{1,2}|{2,3}|{3}", f, t, i, results3[t, i, f]);
                        Console.WriteLine("Random|{0,2}|{1,2}|{2,3}|{3}", f, t, i, results4[t, i, f]);
                        Console.WriteLine("GAMDR+Gaps|{0,2}|{1,2}|{2,3}|{3}", f, t, i, results5[t, i, f]);
                        Console.WriteLine();
                    }
                    );

                    this.SaveResults(filename, results1, results2, results3, results4, results5);
                }
            }
        }

        private void SaveResults(string filename,
            ClassificationResult[, ,] results1,
            ClassificationResult[, ,] results2,
            ClassificationResult[, ,] results3,
            ClassificationResult[, ,] results4,
            ClassificationResult[, ,] results5)
        {
            using (StreamWriter outputFile = new StreamWriter(filename, false))
            {
                outputFile.WriteLine("Method|Fold|Test|Epsilon|{0}", ClassificationResult.TableHeader());

                for (int t = 0; t < results1.GetLength(0); t++)
                {
                    for (int i = 0; i < results1.GetLength(1); i++)
                    {
                        for (int f = 0; f < results1.GetLength(2); f++)
                        {
                            outputFile.WriteLine("GMDR|{0,2}|{1,2}|{2,3}|{3}", f, t, i, results1[t, i, f]);
                            outputFile.WriteLine("ARD|{0,2}|{1,2}|{2,3}|{3}", f, t, i, results2[t, i, f]);
                            outputFile.WriteLine("GAMDR+Ex|{0,2}|{1,2}|{2,3}|{3}", f, t, i, results3[t, i, f]);
                            outputFile.WriteLine("Random|{0,2}|{1,2}|{2,3}|{3}", f, t, i, results4[t, i, f]);
                            outputFile.WriteLine("GAMDR+Gaps|{0,2}|{1,2}|{2,3}|{3}", f, t, i, results5[t, i, f]);
                        }
                    }
                }
            }
        }

        private Tuple<ClassificationResult, ClassificationResult, ClassificationResult, ClassificationResult, ClassificationResult>
            ExceptionRulesSingleRun(DataStore trainData, DataStore testData, PermutationCollection permList, int epsilon, int ensembleSize)
        {
            WeightGeneratorRelative weightGenerator = new WeightGeneratorRelative(trainData);
            double eps = (double)epsilon / 100;
            ReductMeasureLength reductMeasureLength = new ReductMeasureLength();
            ReductLengthComparer reductLengthComparer = new ReductLengthComparer();
            ReductStoreLengthComparer reductStoreLengthComparer = new ReductStoreLengthComparer(false);

            Args parmsApprox = new Args();
            parmsApprox.SetParameter(ReductFactoryOptions.DecisionTable, trainData);
            parmsApprox.SetParameter(ReductFactoryOptions.ReductType, ReductTypes.ApproximateReductMajorityWeights);
            parmsApprox.SetParameter(ReductFactoryOptions.WeightGenerator, weightGenerator);
            parmsApprox.SetParameter(ReductFactoryOptions.Epsilon, eps);
            parmsApprox.SetParameter(ReductFactoryOptions.PermutationCollection, permList);
            parmsApprox.SetParameter(ReductFactoryOptions.UseExceptionRules, false);

            ReductGeneratorWeightsMajority generatorApprox =
                ReductFactory.GetReductGenerator(parmsApprox) as ReductGeneratorWeightsMajority;
            generatorApprox.Run();
            IReductStoreCollection origReductStoreCollectionApprox = generatorApprox.GetReductStoreCollection();
            IReductStoreCollection filteredReductStoreCollectionApprox = origReductStoreCollectionApprox.Filter(ensembleSize, reductLengthComparer);

            RoughClassifier classifierApprox = new RoughClassifier(
                filteredReductStoreCollectionApprox,
                RuleQualityMethods.CoverageW,
                RuleQualityMethods.CoverageW,
                trainData.DataStoreInfo.GetDecisionValues());
            classifierApprox.UseExceptionRules = false;
            ClassificationResult resultApprox = classifierApprox.Classify(testData);
            resultApprox.AvgNumberOfAttributes = filteredReductStoreCollectionApprox.GetAvgMeasure(reductMeasureLength, false);
            resultApprox.ModelCreationTime = generatorApprox.ReductGenerationTime;
            resultApprox.ClassificationTime = classifierApprox.ClassificationTime;

            Args parms_GMDR = new Args();
            parms_GMDR.SetParameter(ReductFactoryOptions.DecisionTable, trainData);
            parms_GMDR.SetParameter(ReductFactoryOptions.ReductType, ReductTypes.GeneralizedMajorityDecision);
            parms_GMDR.SetParameter(ReductFactoryOptions.WeightGenerator, weightGenerator);
            parms_GMDR.SetParameter(ReductFactoryOptions.Epsilon, eps);
            parms_GMDR.SetParameter(ReductFactoryOptions.PermutationCollection, permList);
            parms_GMDR.SetParameter(ReductFactoryOptions.UseExceptionRules, false);
            //parms_GMDR.SetParameter(ReductGeneratorParamHelper.MaxReductLength, (int) resultApprox.QualityRatio > 0 ? (int) resultApprox.QualityRatio : 1);

            ReductGeneralizedMajorityDecisionGenerator generator_GMDR =
                ReductFactory.GetReductGenerator(parms_GMDR) as ReductGeneralizedMajorityDecisionGenerator;
            generator_GMDR.Run();
            IReductStoreCollection origReductStoreCollection_GMDR = generator_GMDR.GetReductStoreCollection();
            IReductStoreCollection filteredReductStoreCollection_GMDR = origReductStoreCollection_GMDR.Filter(ensembleSize, reductLengthComparer);

            RoughClassifier classifier_GMDR = new RoughClassifier(
                filteredReductStoreCollection_GMDR,
                RuleQualityMethods.CoverageW,
                RuleQualityMethods.CoverageW,
                trainData.DataStoreInfo.GetDecisionValues());
            classifier_GMDR.UseExceptionRules = false;
            ClassificationResult result_GMDR = classifier_GMDR.Classify(testData);
            result_GMDR.AvgNumberOfAttributes = filteredReductStoreCollection_GMDR.GetAvgMeasure(reductMeasureLength, false);
            result_GMDR.ModelCreationTime = generator_GMDR.ReductGenerationTime;
            result_GMDR.ClassificationTime = classifier_GMDR.ClassificationTime;

            Args parmsEx = new Args();
            parmsEx.SetParameter(ReductFactoryOptions.DecisionTable, trainData);
            parmsEx.SetParameter(ReductFactoryOptions.ReductType, ReductTypes.GeneralizedMajorityDecisionApproximate);
            parmsEx.SetParameter(ReductFactoryOptions.WeightGenerator, weightGenerator);
            parmsEx.SetParameter(ReductFactoryOptions.Epsilon, eps);
            parmsEx.SetParameter(ReductFactoryOptions.PermutationCollection, permList);
            parmsEx.SetParameter(ReductFactoryOptions.UseExceptionRules, true);

            ReductGeneralizedMajorityDecisionApproximateGenerator generatorEx =
                ReductFactory.GetReductGenerator(parmsEx) as ReductGeneralizedMajorityDecisionApproximateGenerator;
            generatorEx.Run();
            IReductStoreCollection origReductStoreCollectionEx = generatorEx.GetReductStoreCollection();
            IReductStoreCollection filteredReductStoreCollectionEx = origReductStoreCollectionEx.FilterInEnsemble(ensembleSize, reductStoreLengthComparer);

            RoughClassifier classifierEx = new RoughClassifier(
                filteredReductStoreCollectionEx,
                RuleQualityMethods.CoverageW,
                RuleQualityMethods.CoverageW,
                trainData.DataStoreInfo.GetDecisionValues());
            classifierEx.UseExceptionRules = true;
            classifierEx.ExceptionRulesAsGaps = false;
            ClassificationResult resultEx = classifierEx.Classify(testData);
            resultEx.AvgNumberOfAttributes = filteredReductStoreCollectionEx.GetWeightedAvgMeasure(reductMeasureLength, true);
            resultEx.ModelCreationTime = generatorEx.ReductGenerationTime;
            resultEx.ClassificationTime = classifierEx.ClassificationTime;

            /*
            Args parmsGaps = new Args();
            parmsGaps.SetParameter(ReductGeneratorParamHelper.TrainData, trainData);
            parmsGaps.SetParameter(ReductGeneratorParamHelper.FactoryKey, ReductFactoryKeyHelper.GeneralizedMajorityDecisionApproximate);
            parmsGaps.SetParameter(ReductGeneratorParamHelper.WeightGenerator, weightGenerator);
            parmsGaps.SetParameter(ReductGeneratorParamHelper.Epsilon, eps);
            parmsGaps.SetParameter(ReductGeneratorParamHelper.PermutationCollection, permList);
            parmsGaps.SetParameter(ReductGeneratorParamHelper.UseExceptionRules, true);
            parmsGaps.SetParameter(ReductGeneratorParamHelper.ExceptionRulesAsGaps, true);
            //parmsGaps.SetParameter(ReductGeneratorParamHelper.MaxReductLength, (int)resultApprox.QualityRatio > 0 ? (int)resultApprox.QualityRatio : 1);

            ReductGeneralizedMajorityDecisionApproximateGenerator generatorGaps =
                ReductFactory.GetReductGenerator(parmsGaps) as ReductGeneralizedMajorityDecisionApproximateGenerator;
            generatorGaps.Generate();
            */

            IReductStoreCollection origReductStoreCollectionGap = generatorEx.GetReductStoreCollection();
            IReductStoreCollection filteredReductStoreCollectionGap = origReductStoreCollectionGap.FilterInEnsemble(ensembleSize, reductStoreLengthComparer);

            var localPermGenerator = new PermutationGenerator(trainData);
            var localPermList = localPermGenerator.Generate(ensembleSize);

            RoughClassifier classifierGaps = new RoughClassifier(
                filteredReductStoreCollectionGap,
                RuleQualityMethods.CoverageW,
                RuleQualityMethods.CoverageW,
                trainData.DataStoreInfo.GetDecisionValues());
            classifierGaps.UseExceptionRules = true;
            classifierGaps.ExceptionRulesAsGaps = true;
            ClassificationResult resultGaps = classifierGaps.Classify(testData);
            resultGaps.AvgNumberOfAttributes = filteredReductStoreCollectionGap.GetWeightedAvgMeasure(reductMeasureLength, false);
            resultGaps.ModelCreationTime = generatorEx.ReductGenerationTime;
            resultGaps.ClassificationTime = classifierGaps.ClassificationTime;

            Args parmsRandom = new Args();
            parmsRandom.SetParameter(ReductFactoryOptions.DecisionTable, trainData);
            parmsRandom.SetParameter(ReductFactoryOptions.ReductType, ReductTypes.RandomSubset);
            parmsRandom.SetParameter(ReductFactoryOptions.WeightGenerator, weightGenerator);
            parmsRandom.SetParameter(ReductFactoryOptions.Epsilon, eps);
            parmsRandom.SetParameter(ReductFactoryOptions.PermutationCollection, localPermList);
            parmsRandom.SetParameter(ReductFactoryOptions.UseExceptionRules, false);

            parmsRandom.SetParameter(ReductFactoryOptions.MinReductLength, (int)resultApprox.AvgNumberOfAttributes);
            parmsRandom.SetParameter(ReductFactoryOptions.MaxReductLength, (int)resultApprox.AvgNumberOfAttributes);

            ReductRandomSubsetGenerator generatorRandom =
                ReductFactory.GetReductGenerator(parmsRandom) as ReductRandomSubsetGenerator;
            generatorRandom.Run();

            RoughClassifier classifierRandom = new RoughClassifier(
                generatorRandom.GetReductStoreCollection(),
                RuleQualityMethods.CoverageW,
                RuleQualityMethods.CoverageW,
                trainData.DataStoreInfo.GetDecisionValues());
            classifierRandom.UseExceptionRules = false;
            ClassificationResult resultRandom = classifierRandom.Classify(testData);
            resultRandom.AvgNumberOfAttributes = generatorRandom.GetReductStoreCollection().GetAvgMeasure(reductMeasureLength, false);
            resultRandom.ModelCreationTime = generatorRandom.ReductGenerationTime;
            resultRandom.ClassificationTime = classifierRandom.ClassificationTime;

            return new Tuple<ClassificationResult, ClassificationResult, ClassificationResult, ClassificationResult, ClassificationResult>
                (result_GMDR,   //1
                resultApprox,   //2
                resultEx,       //3
                resultRandom,   //5
                resultGaps);    //4
        }

        private static ILog log;

        private static void Main(string[] args)
        {
            if (args.Length < 3)
                throw new InvalidProgramException("number of tests, ensemble size followed by name of dataset ");

            int numberOfTests = Int32.Parse(args[0]);
            int ensembleSize = Int32.Parse(args[1]);

            string[] datasets = new string[args.Length - 2];
            Array.Copy(args, 2, datasets, 0, args.Length - 2);

            Program program = new Program();

            RandomSingleton.Seed = Guid.NewGuid().GetHashCode();

            NameValueCollection properties = new NameValueCollection();

            properties["showDateTime"] = "true";
            properties["showLogName"] = "true";
            properties["level"] = "All";
            properties["configType"] = "FILE";
            properties["configFile"] = "~/NLog.config";

            //Common.Logging.LogManager.Adapter = new Common.Logging.NLog.NLogLoggerFactoryAdapter(properties);
            Common.Logging.LogManager.Adapter = new Common.Logging.Simple.ConsoleOutLoggerFactoryAdapter(properties);
            log = Common.Logging.LogManager.GetLogger(program.GetType());

            var dta = BenchmarkDataHelper.GetDataFiles("Data", datasets);
            foreach (var kvp in dta)
            {
                program.ExceptiodnRulesTest(kvp, numberOfTests, ensembleSize * 10, ensembleSize);
            }

            Console.ReadKey();
        }
    }
}