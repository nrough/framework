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
        public void ExceptiodnRulesTest(KeyValuePair<string, BenchmarkData> kvp)
        {
            int numberOfPermutations = 10;
            int numberOfTests = 10;

            DataStore trainData = null, testData = null, data = null;

            double[, ,] results = new double[numberOfTests, 100, kvp.Value.CrossValidationFolds];
            double[, ,] results2 = new double[numberOfTests, 100, kvp.Value.CrossValidationFolds];

            string name;

            if (kvp.Value.CrossValidationActive)
            {
                data = DataStore.Load(kvp.Value.DataFile, FileFormat.Rses1);
                name = data.Name;
                DataStoreSplitter splitter = new DataStoreSplitter(data, kvp.Value.CrossValidationFolds);                

                for (int f = 0; f < kvp.Value.CrossValidationFolds; f++)
                {
                    splitter.ActiveFold = f;
                    splitter.Split(ref trainData, ref testData);

                    for (int t = 0; t < numberOfTests; t++)
                    {
                        PermutationGenerator permGenerator = new PermutationGenerator(trainData);
                        PermutationCollection permList = permGenerator.Generate(numberOfPermutations);

                        Parallel.For(0, 100, new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount }, i =>
                        //for(int i = 0; i<100; i++)
                        {
                            var accuracy = ExceptionRulesSingleRun(trainData, testData, permList, i);

                            results[t, i, f] = accuracy.Item1;
                            results2[t, i, f] = accuracy.Item2;

                            Console.WriteLine("A|{0}|{1}|{2}|{3}", f, t, i, results[t, i, f]);
                            Console.WriteLine("B|{0}|{1}|{2}|{3}", f, t, i, results2[t, i, f]);
                        }
                        );
                    }

                    trainData = null;
                    testData = null;
                }
            }
            else
            {
                int f = 0;
                trainData = DataStore.Load(kvp.Value.TrainFile, FileFormat.Rses1);
                foreach (int fieldId in trainData.DataStoreInfo.GetFieldIds(FieldTypes.Standard))
                    trainData.DataStoreInfo.GetFieldInfo(fieldId).Alias = kvp.Value.GetFieldAlias(fieldId);
                testData = DataStore.Load(kvp.Value.TestFile, FileFormat.Rses1, trainData.DataStoreInfo);
                name = trainData.Name;
                log.InfoFormat(trainData.Name);

                for (int t = 0; t < numberOfTests; t++)
                {
                    PermutationGenerator permGenerator = new PermutationGenerator(trainData);
                    PermutationCollection permList = permGenerator.Generate(numberOfPermutations);

                    Parallel.For(0, 100, new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount }, i =>
                    //for(int i = 0; i<100; i++)
                    {
                        var accuracy = ExceptionRulesSingleRun(trainData, testData, permList, i);

                        results[t, i, f] = accuracy.Item1;
                        results2[t, i, f] = accuracy.Item2;

                        Console.WriteLine("A|{0}|{1}|{2}|{3}", f, t, i, results[t, i, f]);
                        Console.WriteLine("B|{0}|{1}|{2}|{3}", f, t, i, results2[t, i, f]);
                    }
                    );
                }
            }

            string filename = name + ".result";
            SaveResults(filename, results, results2);
        }

        private void SaveResults(string filename, double[, ,] res1, double[, ,] res2)
        {
            using (StreamWriter outputFile = new StreamWriter(filename))
            {
                for (int t = 0; t < res1.GetLength(0); t++)
                {
                    for (int i = 0; i < res1.GetLength(1); i++)
                    {
                        for (int f = 0; f < res1.GetLength(2); f++)
                        {
                            outputFile.WriteLine("A|{0}|{1}|{2}|{3}", f, t, i, res1[t, i, f]);
                            outputFile.WriteLine("B|{0}|{1}|{2}|{3}", f, t, i, res2[t, i, f]);
                        }
                    }
                }
            }

        }

        private Tuple<double, double> ExceptionRulesSingleRun(DataStore trainData, DataStore testData, PermutationCollection permList, int epsilon)
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

            Args parms2 = new Args();
            parms2.AddParameter(ReductGeneratorParamHelper.DataStore, trainData);
            parms2.AddParameter(ReductGeneratorParamHelper.FactoryKey, ReductFactoryKeyHelper.ApproximateReductMajorityWeights);
            parms2.AddParameter(ReductGeneratorParamHelper.WeightGenerator, weightGenerator);
            parms2.AddParameter(ReductGeneratorParamHelper.Epsilon, eps);
            parms2.AddParameter(ReductGeneratorParamHelper.PermutationCollection, permList);
            parms2.AddParameter(ReductGeneratorParamHelper.UseExceptionRules, true);

            ReductGeneratorWeightsMajority generator2 =
                ReductFactory.GetReductGenerator(parms2) as ReductGeneratorWeightsMajority;
            generator2.Generate();

            RoughClassifier classifier = new RoughClassifier();
            classifier.Classify(testData, generator.GetReductStoreCollection());
            ClassificationResult result = classifier.Vote(
                testData, IdentificationType.WeightConfidence, VoteType.WeightConfidence, null);

            RoughClassifier classifier2 = new RoughClassifier();
            classifier2.Classify(testData, generator2.GetReductStoreCollection());
            ClassificationResult result2 = classifier2.Vote(
                testData, IdentificationType.WeightConfidence, VoteType.WeightConfidence, null);

            return new Tuple<double, double>(result.Accuracy, result2.Accuracy);
        }

        private static ILog log;

        static void Main(string[] args)
        {
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
            
            foreach(var kvp in BenchmarkDataHelper.GetDataFiles(
                new string[] {                     
                    "zoo",
                    "semeion",
                    "opt", 
                    //"dna", 
                    "letter", 
                    "monks-1", 
                    "monks-2", 
                    "monks-3", 
                    //"spect", 
                    "pen"                     
                }))
            {
                program.ExceptiodnRulesTest(kvp);
            }
        }
    }
}
