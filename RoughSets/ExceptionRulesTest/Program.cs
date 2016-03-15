using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Logging;
using Common.Logging.Configuration;
using Infovision.Data;
using Infovision.Datamining;
using Infovision.Datamining.Benchmark;
using Infovision.Datamining.Roughset;
using Infovision.Utils;

namespace ExceptionRulesTest
{
    public class Program
    {
        public void ExceptiodnRulesTest(KeyValuePair<string, BenchmarkData> kvp, int numberOfTests, int numberOfPermutations)
        {            
            DataStore trainData = null, testData = null, data = null;
            string filename = Path.Combine(@"log", kvp.Value.Name + String.Format("-{0}", numberOfPermutations) + ".result"); ;
            DataStoreSplitter splitter = null;

            ClassificationResult[, ,] results1 = new ClassificationResult[numberOfTests, 100, kvp.Value.CrossValidationFolds];
            ClassificationResult[, ,] results2 = new ClassificationResult[numberOfTests, 100, kvp.Value.CrossValidationFolds];
            ClassificationResult[, ,] results3 = new ClassificationResult[numberOfTests, 100, kvp.Value.CrossValidationFolds];
            ClassificationResult[, ,] results4 = new ClassificationResult[numberOfTests, 100, kvp.Value.CrossValidationFolds];

            if (kvp.Value.CrossValidationActive)
            {
                data = DataStore.Load(kvp.Value.DataFile, kvp.Value.FileFormat);

                if (kvp.Value.DecisionFieldId != -1)
                    data.SetDecisionFieldId(kvp.Value.DecisionFieldId);

                splitter = new DataStoreSplitter(data, kvp.Value.CrossValidationFolds);                
            }

            for (int f = 0; f < kvp.Value.CrossValidationFolds; f++)
            {
                if (kvp.Value.CrossValidationActive)
                {
                    trainData = null;
                    testData = null;
                    splitter.ActiveFold = f;
                    splitter.Split(ref trainData, ref testData);
                }
                else if(f == 0)
                {
                    trainData = DataStore.Load(kvp.Value.TrainFile, kvp.Value.FileFormat);
                    if (kvp.Value.DecisionFieldId != -1)
                        trainData.SetDecisionFieldId(kvp.Value.DecisionFieldId);

                    testData = DataStore.Load(kvp.Value.TestFile, kvp.Value.FileFormat, trainData.DataStoreInfo);                    
                    if (kvp.Value.DecisionFieldId != -1)
                        testData.SetDecisionFieldId(kvp.Value.DecisionFieldId);
                }

                for (int t = 0; t < numberOfTests; t++)
                {
                    var permGenerator = new PermutationGenerator(trainData);
                    var permList = permGenerator.Generate(numberOfPermutations);

                    log.InfoFormat("{0} Test:{1}/{2} Fold:{3}/{4}", trainData.Name, t, numberOfTests-1, f, kvp.Value.CrossValidationFolds-1);

                    ParallelOptions options = new ParallelOptions();
                    options.MaxDegreeOfParallelism = System.Math.Max(1, Environment.ProcessorCount / 2);
#if DEBUG
                    options.MaxDegreeOfParallelism = 1;
#endif
                    Parallel.For(0, 100, options, i =>
                    //for(int i = 0; i<100; i++)
                    {
                        var accuracy = this.ExceptionRulesSingleRun(trainData, testData, permList, i);

                        results1[t, i, f] = accuracy.Item1;
                        results2[t, i, f] = accuracy.Item2;
                        results3[t, i, f] = accuracy.Item3;
                        results4[t, i, f] = accuracy.Item4;

                        Console.WriteLine("GMDR|{0,2}|{1,2}|{2,3}|{3}", f, t, i, results1[t, i, f]);
                        Console.WriteLine("ARwOw|{0,2}|{1,2}|{2,3}|{3}", f, t, i, results2[t, i, f]);
                        Console.WriteLine("GAMDR+Ex|{0,2}|{1,2}|{2,3}|{3}", f, t, i, results3[t, i, f]);
                        Console.WriteLine("Random|{0,2}|{1,2}|{2,3}|{3}", f, t, i, results4[t, i, f]);
                        Console.WriteLine();
                    }
                    );

                    this.SaveResults(filename, results1, results2, results3, results4);
                }                
            }            
        }

        private void SaveResults(string filename,
            ClassificationResult[, ,] results1,
            ClassificationResult[, ,] results2,
            ClassificationResult[, ,] results3,
            ClassificationResult[, ,] results4)
        {
            using (StreamWriter outputFile = new StreamWriter(filename, false))
            {
                outputFile.WriteLine("Method|Fold|Test|Epsilon|{0}", ClassificationResult.ResultHeader());
                
                for (int t = 0; t < results1.GetLength(0); t++)
                {
                    for (int i = 0; i < results1.GetLength(1); i++)
                    {
                        for (int f = 0; f < results1.GetLength(2); f++)
                        {
                            outputFile.WriteLine("GAMDR+Ex|{0,2}|{1,2}|{2,3}|{3}", f, t, i, results1[t, i, f]);
                            outputFile.WriteLine("ARwOw|{0,2}|{1,2}|{2,3}|{3}", f, t, i, results2[t, i, f]);
                            outputFile.WriteLine("GAMDR-Ex|{0,2}|{1,2}|{2,3}|{3}", f, t, i, results3[t, i, f]);
                            outputFile.WriteLine("Random|{0,2}|{1,2}|{2,3}|{3}", f, t, i, results4[t, i, f]);
                        }
                    }
                }
            }
        }

        private Tuple<ClassificationResult, ClassificationResult, ClassificationResult, ClassificationResult> 
            ExceptionRulesSingleRun(DataStore trainData, DataStore testData, PermutationCollection permList, int epsilon)
        {
            WeightGeneratorMajority weightGenerator = new WeightGeneratorMajority(trainData);
            decimal eps = Decimal.Divide(epsilon, 100);

            Args parms_GMDR = new Args();
            parms_GMDR.SetParameter(ReductGeneratorParamHelper.DataStore, trainData);
            parms_GMDR.SetParameter(ReductGeneratorParamHelper.FactoryKey, ReductFactoryKeyHelper.GeneralizedMajorityDecision);
            parms_GMDR.SetParameter(ReductGeneratorParamHelper.WeightGenerator, weightGenerator);
            parms_GMDR.SetParameter(ReductGeneratorParamHelper.Epsilon, eps);
            parms_GMDR.SetParameter(ReductGeneratorParamHelper.PermutationCollection, permList);
            parms_GMDR.SetParameter(ReductGeneratorParamHelper.UseExceptionRules, true);

            ReductGeneralizedMajorityDecisionGenerator generator_GMDR =
                ReductFactory.GetReductGenerator(parms_GMDR) as ReductGeneralizedMajorityDecisionGenerator;
            generator_GMDR.Generate();

            RoughClassifier classifier_GMDR = new RoughClassifier(
                generator_GMDR.GetReductStoreCollection(),
                RuleQuality.ConfidenceW,
                RuleQuality.ConfidenceW,
                trainData.DataStoreInfo.GetDecisionValues());
            classifier_GMDR.UseExceptionRules = true;
            ClassificationResult result_GMDR = classifier_GMDR.Classify(testData, null);
            result_GMDR.QualityRatio = generator_GMDR.GetReductStoreCollection().GetAvgMeasure(new ReductMeasureLength(), true);
            
            
            /*
            Args parms = new Args();
            parms.SetParameter(ReductGeneratorParamHelper.DataStore, trainData);
            parms.SetParameter(ReductGeneratorParamHelper.FactoryKey, ReductFactoryKeyHelper.GeneralizedMajorityDecisionApproximate);
            parms.SetParameter(ReductGeneratorParamHelper.WeightGenerator, weightGenerator);
            parms.SetParameter(ReductGeneratorParamHelper.Epsilon, eps);
            parms.SetParameter(ReductGeneratorParamHelper.PermutationCollection, permList);
            parms.SetParameter(ReductGeneratorParamHelper.UseExceptionRules, true);

            ReductGeneralizedMajorityDecisionApproximateGenerator generator =
                ReductFactory.GetReductGenerator(parms) as ReductGeneralizedMajorityDecisionApproximateGenerator;
            generator.Generate();

            RoughClassifier classifier = new RoughClassifier(
                generator.GetReductStoreCollection(),
                RuleQuality.ConfidenceW, 
                RuleQuality.ConfidenceW,
                trainData.DataStoreInfo.GetDecisionValues());
            classifier.UseExceptionRules = true;
            ClassificationResult result = classifier.Classify(testData, null);
            result.QualityRatio = generator.GetReductStoreCollection().GetAvgMeasure(new ReductMeasureLength(), true);
            */

            Args parmsEx = new Args();
            parmsEx.SetParameter(ReductGeneratorParamHelper.DataStore, trainData);
            parmsEx.SetParameter(ReductGeneratorParamHelper.FactoryKey, ReductFactoryKeyHelper.GeneralizedMajorityDecisionApproximate);
            parmsEx.SetParameter(ReductGeneratorParamHelper.WeightGenerator, weightGenerator);
            parmsEx.SetParameter(ReductGeneratorParamHelper.Epsilon, eps);
            parmsEx.SetParameter(ReductGeneratorParamHelper.PermutationCollection, permList);
            parmsEx.SetParameter(ReductGeneratorParamHelper.UseExceptionRules, true);

            ReductGeneralizedMajorityDecisionApproximateGenerator generatorEx =
                ReductFactory.GetReductGenerator(parmsEx) as ReductGeneralizedMajorityDecisionApproximateGenerator;
            generatorEx.Generate();

            RoughClassifier classifierEx = new RoughClassifier(
                generatorEx.GetReductStoreCollection(), 
                RuleQuality.ConfidenceW, 
                RuleQuality.ConfidenceW,
                trainData.DataStoreInfo.GetDecisionValues());
            classifierEx.UseExceptionRules = true;
            ClassificationResult resultEx = classifierEx.Classify(testData, null);
            resultEx.QualityRatio = generatorEx.GetReductStoreCollection().GetAvgMeasure(new ReductMeasureLength(), false);
            

            Args parms2 = new Args();
            parms2.SetParameter(ReductGeneratorParamHelper.DataStore, trainData);
            parms2.SetParameter(ReductGeneratorParamHelper.FactoryKey, ReductFactoryKeyHelper.ApproximateReductMajorityWeights);
            parms2.SetParameter(ReductGeneratorParamHelper.WeightGenerator, weightGenerator);
            parms2.SetParameter(ReductGeneratorParamHelper.Epsilon, eps);
            parms2.SetParameter(ReductGeneratorParamHelper.PermutationCollection, permList);
            parms2.SetParameter(ReductGeneratorParamHelper.UseExceptionRules, false);

            ReductGeneratorWeightsMajority generator2 =
                ReductFactory.GetReductGenerator(parms2) as ReductGeneratorWeightsMajority;
            generator2.Generate();

            RoughClassifier classifier2 = new RoughClassifier(
                generator2.GetReductStoreCollection(),
                RuleQuality.ConfidenceW, 
                RuleQuality.ConfidenceW,
                trainData.DataStoreInfo.GetDecisionValues());
            classifier2.UseExceptionRules = false;
            ClassificationResult result2 = classifier2.Classify(testData, null);
            result2.QualityRatio = generator2.GetReductStoreCollection().GetAvgMeasure(new ReductMeasureLength(), false);

            Args parms4 = new Args();
            parms4.SetParameter(ReductGeneratorParamHelper.DataStore, trainData);
            parms4.SetParameter(ReductGeneratorParamHelper.FactoryKey, ReductFactoryKeyHelper.RandomSubset);
            parms4.SetParameter(ReductGeneratorParamHelper.WeightGenerator, weightGenerator);
            parms4.SetParameter(ReductGeneratorParamHelper.Epsilon, eps);
            parms4.SetParameter(ReductGeneratorParamHelper.PermutationCollection, permList);
            parms4.SetParameter(ReductGeneratorParamHelper.UseExceptionRules, false);
            //parms4.SetParameter(ReductGeneratorParamHelper.MinReductLength, (int)resultEx.QualityRatio); //Avg Reduct Length in method C
            //parms4.SetParameter(ReductGeneratorParamHelper.MaxReductLength, (int)resultEx.QualityRatio); //Avg Reduct Length in method C

            ReductRandomSubsetGenerator generator4 =
                ReductFactory.GetReductGenerator(parms4) as ReductRandomSubsetGenerator;
            generator4.Generate();

            RoughClassifier classifier4 = new RoughClassifier(
                generator4.GetReductStoreCollection(),
                RuleQuality.ConfidenceW, 
                RuleQuality.ConfidenceW,
                trainData.DataStoreInfo.GetDecisionValues());
            classifier4.UseExceptionRules = false;
            ClassificationResult result4 = classifier4.Classify(testData, null);
            result4.QualityRatio = generator4.GetReductStoreCollection().GetAvgMeasure(new ReductMeasureLength(), false);

            return new Tuple<ClassificationResult, ClassificationResult, ClassificationResult, ClassificationResult>
                (result_GMDR,  //1
                result2,  //2
                resultEx, //3
                result4);  //4
        }

        private static ILog log;

        static void Main(string[] args)
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

            foreach (var kvp in BenchmarkDataHelper.GetDataFiles("Data", datasets))
            {
                program.ExceptiodnRulesTest(kvp, numberOfTests, ensembleSize);
            }

            Console.ReadKey();
        }
    }
}
