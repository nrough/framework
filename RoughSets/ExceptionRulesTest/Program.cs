using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Common.Logging;
using Common.Logging.Configuration;
using NRough.Data;
using NRough.MachineLearning.Roughsets;
using NRough.Core;
using NRough.MachineLearning.Weighting;
using NRough.MachineLearning.Permutations;
using NRough.MachineLearning.Classification;
using NRough.Core.Random;
using NRough.Data.Benchmark;
using NRough.MachineLearning.Roughsets.Reducts.Comparers;

namespace ExceptionRulesTest
{
    public class Program
    {
        public void ExceptiodnRulesTest(KeyValuePair<string, BenchmarkData> kvp, int numberOfTests, int numberOfPermutations, int ensembleSize)
        {
            DataStore trainData = null, testData = null, data = null;
            string filename = Path.Combine(@"log", kvp.Value.Name + String.Format("-{0}", ensembleSize) + ".result");
            DataSplitter splitter = null;

            ClassificationResult[, ,] results1 = new ClassificationResult[numberOfTests, 50, kvp.Value.CrossValidationFolds];
            ClassificationResult[, ,] results2 = new ClassificationResult[numberOfTests, 50, kvp.Value.CrossValidationFolds];
            ClassificationResult[, ,] results3 = new ClassificationResult[numberOfTests, 50, kvp.Value.CrossValidationFolds];
            ClassificationResult[, ,] results4 = new ClassificationResult[numberOfTests, 50, kvp.Value.CrossValidationFolds];
            ClassificationResult[, ,] results5 = new ClassificationResult[numberOfTests, 50, kvp.Value.CrossValidationFolds];

            if (kvp.Value.CrossValidationActive)
            {
                data = DataStore.Load(kvp.Value.DataFile, kvp.Value.DataFormat);
                foreach (var attribute in data.DataStoreInfo.Attributes)
                    attribute.IsNumeric = false;

                if (kvp.Value.DecisionFieldId != -1)
                    data.SetDecisionFieldId(kvp.Value.DecisionFieldId);

                splitter = new DataSplitter(data, kvp.Value.CrossValidationFolds);
            }

            Console.WriteLine(ClassificationResult.TableHeader());

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
                    trainData = DataStore.Load(kvp.Value.TrainFile, kvp.Value.DataFormat);
                    foreach (var attribute in trainData.DataStoreInfo.Attributes)
                        attribute.IsNumeric = false;
                    if (kvp.Value.DecisionFieldId != -1)
                        trainData.SetDecisionFieldId(kvp.Value.DecisionFieldId);

                    testData = DataStore.Load(kvp.Value.TestFile, kvp.Value.DataFormat, trainData.DataStoreInfo);
                    if (kvp.Value.DecisionFieldId != -1)
                        testData.SetDecisionFieldId(kvp.Value.DecisionFieldId);
                }                

                for (int t = 0; t < numberOfTests; t++)
                {
                    var permGenerator = new PermutationGenerator(trainData);
                    var permList = permGenerator.Generate(numberOfPermutations);                    

                    ParallelOptions options = new ParallelOptions();
                    options.MaxDegreeOfParallelism = ConfigManager.MaxDegreeOfParallelism;

                    Parallel.For(0, 50, options, i =>
                    //for(int i=0; i<100; i++)
                    {
                        var accuracy = this.ExceptionRulesSingleRun(trainData, testData, permList, i, ensembleSize);

                        results1[t, i, f] = accuracy.Item1;
                        results2[t, i, f] = accuracy.Item2;
                        results3[t, i, f] = accuracy.Item3;
                        results4[t, i, f] = accuracy.Item4;
                        results5[t, i, f] = accuracy.Item5;

                        Console.WriteLine(results1[t, i, f]);
                        Console.WriteLine(results2[t, i, f]);
                        Console.WriteLine(results3[t, i, f]);
                        Console.WriteLine(results4[t, i, f]);
                        Console.WriteLine(results5[t, i, f]);
                    }
                    );

                    this.SaveResults(filename, results1, results2, results3, results4, results5);
                }
            }
        }

        private void SaveResults(string filename,
            ClassificationResult[,,] results1,
            ClassificationResult[,,] results2,
            ClassificationResult[,,] results3,
            ClassificationResult[,,] results4,
            ClassificationResult[,,] results5)
        {
            using (StreamWriter outputFile = new StreamWriter(filename, false))
            {                
                outputFile.WriteLine(ClassificationResult.TableHeader());

                for (int t = 0; t < results1.GetLength(0); t++)
                {
                    for (int i = 0; i < results1.GetLength(1); i++)
                    {
                        for (int f = 0; f < results1.GetLength(2); f++)
                        {
                            outputFile.WriteLine(results1[t, i, f]);
                            outputFile.WriteLine(results2[t, i, f]);
                            outputFile.WriteLine(results3[t, i, f]);
                            outputFile.WriteLine(results4[t, i, f]);
                            outputFile.WriteLine(results5[t, i, f]);
                        }
                    }
                }
            }
        }

        private Tuple<ClassificationResult, ClassificationResult, ClassificationResult, ClassificationResult, ClassificationResult>
            ExceptionRulesSingleRun(DataStore trainData, DataStore testData, PermutationCollection permList, int epsilon, int ensembleSize)
        {
            WeightGeneratorMajority weightGenerator = new WeightGeneratorMajority(trainData);
            double eps = (double)epsilon / 100;
            ReductMeasureLength reductMeasureLength = new ReductMeasureLength();
            ReductMeasureNumberOfPartitions reductMeasureRules = new ReductMeasureNumberOfPartitions();
            ReductLengthComparer reductRuleNumberComparer = new ReductLengthComparer();
            ReductStoreLengthComparer reductStoreLengthComparer = new ReductStoreLengthComparer(true);

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
            IReductStoreCollection filteredReductStoreCollectionApprox = origReductStoreCollectionApprox.Filter(ensembleSize, reductRuleNumberComparer);

            RoughClassifier classifierApprox = new RoughClassifier(
                filteredReductStoreCollectionApprox,
                RuleQualityMethods.ConfidenceW,
                RuleQualityMethods.SingleVote,
                trainData.DataStoreInfo.GetDecisionValues());

            classifierApprox.UseExceptionRules = false;
            ClassificationResult resultApprox = classifierApprox.Classify(testData);
            resultApprox.ModelName = "M-EPS";
            resultApprox.Epsilon = eps;
            resultApprox.EnsembleSize = ensembleSize;            
            resultApprox.AvgNumberOfAttributes = filteredReductStoreCollectionApprox.GetAvgMeasure(reductMeasureLength, false);
            resultApprox.NumberOfRules = filteredReductStoreCollectionApprox.GetAvgMeasure(reductMeasureRules, false);
            resultApprox.MaxTreeHeight = 0;
            resultApprox.AvgTreeHeight = resultApprox.AvgNumberOfAttributes;            
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
            IReductStoreCollection filteredReductStoreCollection_GMDR = origReductStoreCollection_GMDR.Filter(ensembleSize, reductRuleNumberComparer);

            RoughClassifier classifier_GMDR = new RoughClassifier(
                filteredReductStoreCollection_GMDR,
                RuleQualityAvgMethods.ConfidenceW,
                RuleQualityMethods.SingleVote,
                trainData.DataStoreInfo.GetDecisionValues());
            classifier_GMDR.UseExceptionRules = false;
            ClassificationResult result_GMDR = classifier_GMDR.Classify(testData);
            result_GMDR.Epsilon = eps;
            result_GMDR.ModelName = "m-EPS-CAP";
            result_GMDR.EnsembleSize = ensembleSize;

            result_GMDR.AvgNumberOfAttributes = filteredReductStoreCollection_GMDR.GetAvgMeasure(reductMeasureLength, false);
            result_GMDR.NumberOfRules = filteredReductStoreCollection_GMDR.GetAvgMeasure(reductMeasureRules, false);
            result_GMDR.MaxTreeHeight = 0;
            result_GMDR.AvgTreeHeight = result_GMDR.AvgNumberOfAttributes;            
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
                RuleQualityAvgMethods.ConfidenceW,
                RuleQualityMethods.SingleVote,
                trainData.DataStoreInfo.GetDecisionValues());
            classifierEx.UseExceptionRules = true;
            classifierEx.ExceptionRulesAsGaps = false;
            ClassificationResult resultEx = classifierEx.Classify(testData);
            resultEx.Epsilon = eps;
            resultEx.EnsembleSize = ensembleSize;
            resultEx.ModelName = "m-PHICAP-EXEP";
            resultEx.AvgNumberOfAttributes = filteredReductStoreCollectionEx.GetWeightedAvgMeasure(reductMeasureLength, true);
            resultEx.NumberOfRules = filteredReductStoreCollectionEx.GetAvgMeasure(reductMeasureRules, false);
            resultEx.MaxTreeHeight = filteredReductStoreCollectionEx.GetAvgMeasure(reductMeasureRules, true, true);
            resultEx.AvgTreeHeight = resultEx.AvgNumberOfAttributes;
            resultEx.ModelCreationTime = generatorEx.ReductGenerationTime;
            resultEx.ClassificationTime = classifierEx.ClassificationTime;            

            IReductStoreCollection origReductStoreCollectionGap = generatorEx.GetReductStoreCollection();
            IReductStoreCollection filteredReductStoreCollectionGap = origReductStoreCollectionGap.FilterInEnsemble(ensembleSize, reductStoreLengthComparer);

            var localPermGenerator = new PermutationGenerator(trainData);
            var localPermList = localPermGenerator.Generate(ensembleSize);
            

            RoughClassifier classifierGaps = new RoughClassifier(
                filteredReductStoreCollectionGap,
                RuleQualityAvgMethods.ConfidenceW,
                RuleQualityMethods.SingleVote,
                trainData.DataStoreInfo.GetDecisionValues());
            classifierGaps.UseExceptionRules = true;
            classifierGaps.ExceptionRulesAsGaps = true;
            ClassificationResult resultGaps = classifierGaps.Classify(testData);
            resultGaps.Epsilon = eps;
            resultGaps.EnsembleSize = ensembleSize;
            resultGaps.ModelName = "m-PHICAP-GAPS";
            resultGaps.AvgNumberOfAttributes = filteredReductStoreCollectionGap.GetWeightedAvgMeasure(reductMeasureLength, false);
            resultGaps.NumberOfRules = filteredReductStoreCollectionGap.GetAvgMeasure(reductMeasureRules, false);
            resultGaps.MaxTreeHeight = filteredReductStoreCollectionGap.GetAvgMeasure(reductMeasureRules, true, true);
            resultGaps.AvgTreeHeight = resultGaps.AvgNumberOfAttributes;           
            resultGaps.ModelCreationTime = generatorEx.ReductGenerationTime;
            resultGaps.ClassificationTime = classifierGaps.ClassificationTime;
            

            Args parmsNoEx = new Args();
            parmsNoEx.SetParameter(ReductFactoryOptions.DecisionTable, trainData);
            parmsNoEx.SetParameter(ReductFactoryOptions.ReductType, ReductTypes.GeneralizedMajorityDecisionApproximate);
            parmsNoEx.SetParameter(ReductFactoryOptions.WeightGenerator, weightGenerator);
            parmsNoEx.SetParameter(ReductFactoryOptions.Epsilon, eps);
            parmsNoEx.SetParameter(ReductFactoryOptions.PermutationCollection, permList);
            parmsNoEx.SetParameter(ReductFactoryOptions.UseExceptionRules, false);

            ReductGeneralizedMajorityDecisionApproximateGenerator generatorNoEx =
                ReductFactory.GetReductGenerator(parmsNoEx) as ReductGeneralizedMajorityDecisionApproximateGenerator;
            generatorNoEx.Run();
            IReductStoreCollection origReductStoreCollectionNoEx = generatorNoEx.GetReductStoreCollection();
            IReductStoreCollection filteredReductStoreCollectionNoEx = origReductStoreCollectionNoEx.FilterInEnsemble(ensembleSize, reductStoreLengthComparer);

            RoughClassifier classifierNoEx = new RoughClassifier(
                filteredReductStoreCollectionNoEx,
                RuleQualityAvgMethods.ConfidenceW,
                RuleQualityMethods.SingleVote,
                trainData.DataStoreInfo.GetDecisionValues());
            classifierNoEx.UseExceptionRules = false;
            classifierNoEx.ExceptionRulesAsGaps = false;
            ClassificationResult resultNoEx = classifierNoEx.Classify(testData);
            resultNoEx.Epsilon = eps;
            resultNoEx.EnsembleSize = ensembleSize;
            resultNoEx.ModelName = "m-PHICAP-NONE";
            resultNoEx.AvgNumberOfAttributes = filteredReductStoreCollectionNoEx.GetWeightedAvgMeasure(reductMeasureLength, true);
            resultNoEx.NumberOfRules = filteredReductStoreCollectionNoEx.GetAvgMeasure(reductMeasureRules, false);
            resultNoEx.MaxTreeHeight = 0;
            resultNoEx.AvgTreeHeight = resultNoEx.AvgNumberOfAttributes;            
            resultNoEx.ModelCreationTime = generatorNoEx.ReductGenerationTime;
            resultNoEx.ClassificationTime = classifierNoEx.ClassificationTime;

            return new Tuple<ClassificationResult, ClassificationResult, ClassificationResult, ClassificationResult, ClassificationResult>
                (resultApprox, result_GMDR, resultEx, resultGaps, resultNoEx);    
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

            ClassificationResult.OutputColumns = @"ds;model;eps;acc;attr;numrul;dtha;dthm;";

            var dta = BenchmarkDataHelper.GetDataFiles("Data", datasets);
            foreach (var kvp in dta)
            {
                int numberOfPermutation = ensembleSize * 20;
                program.ExceptiodnRulesTest(kvp, numberOfTests, numberOfPermutation, ensembleSize);
            }

            Console.ReadKey();
        }
    }
}