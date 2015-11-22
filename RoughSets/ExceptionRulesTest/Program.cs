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
using Infovision.Datamining.Roughset;
using Infovision.Utils;

namespace ExceptionRulesTest
{
    public class Program
    {
        public void ExceptiodnRulesTest(KeyValuePair<string, BenchmarkData> kvp, int numberOfTests, int numberOfPermutations)
        {            
            DataStore trainData = null, testData = null, data = null;
            string filename = Path.Combine(@"log", kvp.Value.Alias + String.Format("-{0}", numberOfPermutations) + ".result"); ;
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
                        
                    Parallel.For(0, 100, new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount }, i =>
                    //for(int i = 0; i<100; i++)
                    {
                        var accuracy = this.ExceptionRulesSingleRun(trainData, testData, permList, i);

                        results1[t, i, f] = accuracy.Item1;
                        results2[t, i, f] = accuracy.Item2;
                        results3[t, i, f] = accuracy.Item3;
                        results4[t, i, f] = accuracy.Item4;

                        Console.WriteLine("A|{0,2}|{1,2}|{2,3}|{3}", f, t, i, results1[t, i, f]);
                        Console.WriteLine("B|{0,2}|{1,2}|{2,3}|{3}", f, t, i, results2[t, i, f]);
                        Console.WriteLine("C|{0,2}|{1,2}|{2,3}|{3}", f, t, i, results3[t, i, f]);
                        Console.WriteLine("D|{0,2}|{1,2}|{2,3}|{3}", f, t, i, results4[t, i, f]);
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
                            outputFile.WriteLine("A|{0,2}|{1,2}|{2,3}|{3}", f, t, i, results1[t, i, f]);
                            outputFile.WriteLine("B|{0,2}|{1,2}|{2,3}|{3}", f, t, i, results2[t, i, f]);
                            outputFile.WriteLine("C|{0,2}|{1,2}|{2,3}|{3}", f, t, i, results3[t, i, f]);
                            outputFile.WriteLine("D|{0,2}|{1,2}|{2,3}|{3}", f, t, i, results4[t, i, f]);
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

            Args parms = new Args();
            parms.AddParameter(ReductGeneratorParamHelper.DataStore, trainData);
            parms.AddParameter(ReductGeneratorParamHelper.FactoryKey, ReductFactoryKeyHelper.GeneralizedMajorityDecisionApproximate);
            parms.AddParameter(ReductGeneratorParamHelper.WeightGenerator, weightGenerator);
            parms.AddParameter(ReductGeneratorParamHelper.Epsilon, eps);
            parms.AddParameter(ReductGeneratorParamHelper.PermutationCollection, permList);
            parms.AddParameter(ReductGeneratorParamHelper.UseExceptionRules, true);

            ReductGeneralizedMajorityDecisionApproximateGenerator generator =
                ReductFactory.GetReductGenerator(parms) as ReductGeneralizedMajorityDecisionApproximateGenerator;
            generator.Generate();

            RoughClassifier classifier = new RoughClassifier();
            classifier.UseExceptionRules = true;
            classifier.Classify(testData, generator.GetReductStoreCollection());
            ClassificationResult result = classifier.Vote(
                testData, IdentificationType.WeightConfidence, VoteType.WeightConfidence, null);
            result.QualityRatio = generator.GetReductStoreCollection().GetAvgMeasure(new ReductMeasureLength(), true);

            RoughClassifier classifierEx = new RoughClassifier();
            classifierEx.UseExceptionRules = false;
            classifierEx.Classify(testData, generator.GetReductStoreCollection());
            ClassificationResult resultEx = classifierEx.Vote(
                testData, IdentificationType.WeightConfidence, VoteType.WeightConfidence, null);
            resultEx.QualityRatio = generator.GetReductStoreCollection().GetAvgMeasure(new ReductMeasureLength(), false);

            Args parms2 = new Args();
            parms2.AddParameter(ReductGeneratorParamHelper.DataStore, trainData);
            parms2.AddParameter(ReductGeneratorParamHelper.FactoryKey, ReductFactoryKeyHelper.ApproximateReductMajorityWeights);
            parms2.AddParameter(ReductGeneratorParamHelper.WeightGenerator, weightGenerator);
            parms2.AddParameter(ReductGeneratorParamHelper.Epsilon, eps);
            parms2.AddParameter(ReductGeneratorParamHelper.PermutationCollection, permList);
            parms2.AddParameter(ReductGeneratorParamHelper.UseExceptionRules, false);

            ReductGeneratorWeightsMajority generator2 =
                ReductFactory.GetReductGenerator(parms2) as ReductGeneratorWeightsMajority;
            generator2.Generate();

            RoughClassifier classifier2 = new RoughClassifier();
            classifier2.UseExceptionRules = false;
            classifier2.Classify(testData, generator2.GetReductStoreCollection());
            ClassificationResult result2 = classifier2.Vote(
                testData, IdentificationType.WeightConfidence, VoteType.WeightConfidence, null);
            result2.QualityRatio = generator2.GetReductStoreCollection().GetAvgMeasure(new ReductMeasureLength(), false);

            Args parms4 = new Args();
            parms4.AddParameter(ReductGeneratorParamHelper.DataStore, trainData);
            parms4.AddParameter(ReductGeneratorParamHelper.FactoryKey, ReductFactoryKeyHelper.RandomSubset);
            parms4.AddParameter(ReductGeneratorParamHelper.WeightGenerator, weightGenerator);
            parms4.AddParameter(ReductGeneratorParamHelper.Epsilon, eps);
            parms4.AddParameter(ReductGeneratorParamHelper.PermutationCollection, permList);
            parms4.AddParameter(ReductGeneratorParamHelper.UseExceptionRules, false);
            parms4.AddParameter(ReductGeneratorParamHelper.MinReductLength, (int)resultEx.QualityRatio); //Avg Reduct Length in method C
            parms4.AddParameter(ReductGeneratorParamHelper.MaxReductLength, (int)resultEx.QualityRatio); //Avg Reduct Length in method C

            ReductRandomSubsetGenerator generator4 =
                ReductFactory.GetReductGenerator(parms4) as ReductRandomSubsetGenerator;
            generator4.Generate();

            RoughClassifier classifier4 = new RoughClassifier();
            classifier4.UseExceptionRules = false;
            classifier4.Classify(testData, generator4.GetReductStoreCollection());
            ClassificationResult result4 = classifier4.Vote(
                testData, IdentificationType.WeightConfidence, VoteType.WeightConfidence, null);
            result4.QualityRatio = generator4.GetReductStoreCollection().GetAvgMeasure(new ReductMeasureLength(), false);

            return new Tuple<ClassificationResult, ClassificationResult, ClassificationResult, ClassificationResult>
                (result, 
                result2, 
                resultEx,
                result4);
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

            Random randSeed = new Random();
            RandomSingleton.Seed = Guid.NewGuid().GetHashCode();

            NameValueCollection properties = new NameValueCollection();
            properties["showDateTime"] = "true";
            properties["showLogName"] = "false";
            properties["level"] = "All";

            properties["configType"] = "FILE";
            properties["configFile"] = "~/NLog.config";
            
            //Common.Logging.LogManager.Adapter = new Common.Logging.NLog.NLogLoggerFactoryAdapter(properties);
            Common.Logging.LogManager.Adapter = new Common.Logging.Simple.ConsoleOutLoggerFactoryAdapter(properties);
            log = Common.Logging.LogManager.GetLogger(program.GetType());

            foreach (var kvp in BenchmarkDataHelper.GetDataFiles(datasets))
            {
                program.ExceptiodnRulesTest(kvp, numberOfTests, ensembleSize);
            }

            Console.ReadKey();
        }
    }
}
