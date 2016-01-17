using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Infovision.Data;
using Infovision.Utils;
using NUnit.Framework;
using Common.Logging;
using Common.Logging.Configuration;
using Infovision.Statistics;
using Common.Logging.NLog;
using NLog;
using System.IO;
using Infovision.Datamining.Benchmark;

namespace Infovision.Datamining.Roughset.UnitTests
{
    [TestFixture, Ignore]
    public class ReductGeneralizedMajorityDecisionApproximateTest
    {
        public IEnumerable<KeyValuePair<string, BenchmarkData>> GetDataFiles()
        {
            return BenchmarkDataHelper.GetDataFiles("Data",
                new string[] {                     
                    "zoo",
                    "semeion",
                    "opt", 
                    "dna", 
                    "letter", 
                    "monks-1", 
                    "monks-2", 
                    "monks-3", 
                    "spect", 
                    "pen"                     
                });
        }

        [Test, TestCaseSource("GetDataFiles")]
        public void CalculateReductTest(KeyValuePair<string, BenchmarkData> kvp)
        {
            DataStore data = DataStore.Load(kvp.Value.TrainFile, kvp.Value.FileFormat);
            
            foreach (int fieldId in data.DataStoreInfo.GetFieldIds(FieldTypes.Standard))
                data.DataStoreInfo.GetFieldInfo(fieldId).Alias = kvp.Value.GetFieldAlias(fieldId);

            DataStore test = DataStore.Load(kvp.Value.TestFile, FileFormat.Rses1, data.DataStoreInfo);
            
            log.InfoFormat(data.Name);

            PermutationGenerator permGenerator = new PermutationGenerator(data);
            int numberOfPermutations = 100;
            PermutationCollection permList = permGenerator.Generate(numberOfPermutations);

            WeightGeneratorMajority weightGenerator = new WeightGeneratorMajority(data);
            
            Decimal dataQuality = new InformationMeasureWeights().Calc(
                new ReductWeights(
                    data, data.DataStoreInfo.GetFieldIds(FieldTypes.Standard), weightGenerator.Weights, Decimal.Zero));

            Decimal dataQuality_2 = new InformationMeasureWeights().Calc(
                new ReductWeights(
                    data, data.DataStoreInfo.GetFieldIds(FieldTypes.Standard), weightGenerator.Weights, Decimal.Zero));

            Assert.AreEqual(Decimal.Round(dataQuality, 17), Decimal.Round(dataQuality_2, 17));

            for (decimal eps = Decimal.Zero; eps <= Decimal.One; eps += 0.01m)
            {
                long elapsed_sum_1 = 0;
                long elapsed_sum_2 = 0;
                
                int len_sum_1 = 0;
                int len_sum_2 = 0;
                
                decimal avg_quality_1 = Decimal.Zero;
                decimal avg_quality_2 = Decimal.Zero;                                          
                
                double[] accuracyResults_1 = new double[permList.Count];
                double[] accuracyResults_2 = new double[permList.Count];

                int i = 0;
                foreach (Permutation permutation in permList)
                {
                    int[] attributes = permutation.ToArray();

                    var watch_1 = Stopwatch.StartNew();
                    IReduct reduct_1 = CalculateGeneralizedMajorityApproximateDecisionReduct(data, eps, attributes);
                    watch_1.Stop();
                    
                    IReductStore store = new ReductStore(1);
                    store.AddReduct(reduct_1);
                    IReductStoreCollection reductStoreCollection = new ReductStoreCollection(1);
                    reductStoreCollection.AddStore(store);

                    Assert.NotNull(reduct_1);
                    Decimal reductQuality_1 = new InformationMeasureWeights().Calc(reduct_1);
                    Assert.GreaterOrEqual(reductQuality_1, Decimal.Round(dataQuality * (Decimal.One - eps), 17));

                    elapsed_sum_1 += watch_1.ElapsedMilliseconds;
                    len_sum_1 += reduct_1.Attributes.Count;
                    avg_quality_1 += reductQuality_1;

                    RoughClassifier classifier_1 = new RoughClassifier(
                        reductStoreCollection, 
                        RuleQuality.ConfidenceW, 
                        RuleQuality.ConfidenceW, 
                        data.DataStoreInfo.GetDecisionValues());

                    ClassificationResult result_1 = classifier_1.Classify(test, null);

                    accuracyResults_1[i] = result_1.Accuracy;                    
                    
                    log.InfoFormat("|A|{0}|{3}|{1}|{2}|{4}|",
                        reduct_1.ToString(),
                        reductQuality_1,
                        watch_1.ElapsedMilliseconds,
                        reduct_1.Attributes.Count,
                        result_1.Accuracy);

                    var watch_2 = Stopwatch.StartNew();
                    IReduct reduct_2 = CalculateApproximateReductFromSubset(data, eps, attributes);
                    watch_2.Stop();

                    IReductStore store2 = new ReductStore(1);
                    store2.AddReduct(reduct_2);
                    IReductStoreCollection reductStoreCollection2 = new ReductStoreCollection(1);
                    reductStoreCollection2.AddStore(store2);


                    Assert.NotNull(reduct_2);
                    Decimal reductQuality_2 = new InformationMeasureWeights().Calc(reduct_2);
                    Assert.GreaterOrEqual(reductQuality_2, Decimal.Round(dataQuality * (Decimal.One - eps), 17));

                    elapsed_sum_2 += watch_2.ElapsedMilliseconds;
                    len_sum_2 += reduct_2.Attributes.Count;
                    avg_quality_2 += reductQuality_2;

                    RoughClassifier classifier_2 = new RoughClassifier(
                        reductStoreCollection2, 
                        RuleQuality.ConfidenceW, 
                        RuleQuality.ConfidenceW,
                        data.DataStoreInfo.GetDecisionValues());
                    ClassificationResult result_2 = classifier_2.Classify(test, null); 

                    accuracyResults_2[i] = result_2.Accuracy;                    

                    log.InfoFormat("|B|{0}|{3}|{1}|{2}|{4}|",
                        reduct_2.ToString(),
                        reductQuality_2,
                        watch_2.ElapsedMilliseconds,
                        reduct_2.Attributes.Count,
                        result_2.Accuracy);

                    i++;
                }

                log.InfoFormat(Environment.NewLine);
                
                log.InfoFormat("==========================================");
                log.InfoFormat("Average reduct lenght of method A: {0}", (double)len_sum_1 / (double)permList.Count);
                log.InfoFormat("Average reduct lenght of method B: {0}", (double)len_sum_2 / (double)permList.Count);
                log.InfoFormat("Average computation time method A: {0}", (double)elapsed_sum_1 / (double)permList.Count);
                log.InfoFormat("Average computation time method B: {0}", (double)elapsed_sum_2 / (double)permList.Count);
                log.InfoFormat("Average reduct quality of method A: {0}", avg_quality_1 / (decimal)permList.Count);
                log.InfoFormat("Average reduct quality of method B: {0}", avg_quality_2 / (decimal)permList.Count);                
                                                             
                log.InfoFormat("Accuracy A Min: {0} Max: {1} Mean: {2} StdDev: {3}", 
                    Tools.Min(accuracyResults_1), Tools.Max(accuracyResults_1), Tools.Mean(accuracyResults_1), Tools.StdDev(accuracyResults_1));
                log.InfoFormat("Accuracy B Min: {0} Max: {1} Mean: {2} StdDev: {3}",
                    Tools.Min(accuracyResults_2), Tools.Max(accuracyResults_2), Tools.Mean(accuracyResults_2), Tools.StdDev(accuracyResults_2));

                log.InfoFormat("==========================================");

                log.InfoFormat(Environment.NewLine);
            }
        }

        [Test, TestCaseSource("GetDataFiles")]
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
                
                for (int f = 0; f <= kvp.Value.CrossValidationFolds; f++)
                {
                    splitter.ActiveFold = f;
                    splitter.Split(ref trainData, ref testData);

                    for (int t = 0; t < numberOfTests; t++)
                    {
                        PermutationGenerator permGenerator = new PermutationGenerator(trainData);
                        PermutationCollection permList = permGenerator.Generate(numberOfPermutations);
                        
                        Parallel.For(0, 100, i =>
                        {
                            var accuracy = ExceptionRulesSingleRun(trainData, testData, permList, i, t, f);
                            
                            results[t, i, f] = accuracy.Item1;
                            results2[t, i, f] = accuracy.Item2;

                            log.InfoFormat("A|{0}|{1}|{2}|{3}", f, t, i, accuracy.Item1);
                            log.InfoFormat("B|{0}|{1}|{2}|{3}", f, t, i, accuracy.Item2); 
                        });
                    }
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

                    Parallel.For(0, 100, i =>
                    {
                        var accuracy = ExceptionRulesSingleRun(trainData, testData, permList, i, t, f);

                        results[t, i, f] = accuracy.Item1;
                        results2[t, i, f] = accuracy.Item2;

                        log.InfoFormat("A|{0}|{1}|{2}|{3}", f, t, i, accuracy.Item1);
                        log.InfoFormat("B|{0}|{1}|{2}|{3}", f, t, i, accuracy.Item2); 
                    });
                }
            }

            string filename = name + ".result";
            SaveResults(filename, results, results2);
        }

        private void SaveResults(string filename, double[,,] res1, double[,,] res2)
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
                            outputFile.WriteLine("B|{0}|{1}|{2}|{3}", f, t, i, res1[t, i, f]);
                        }
                    }
                }
            }

        }

        private Tuple<double, double> ExceptionRulesSingleRun(DataStore trainData, DataStore testData, PermutationCollection permList, int epsilon, int test, int fold)
        {
            WeightGeneratorMajority weightGenerator = new WeightGeneratorMajority(trainData);
            decimal eps = Decimal.Divide(epsilon, 100);

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

            Args parms2 = new Args();
            parms2.SetParameter(ReductGeneratorParamHelper.DataStore, trainData);
            parms2.SetParameter(ReductGeneratorParamHelper.FactoryKey, ReductFactoryKeyHelper.ApproximateReductMajorityWeights);
            parms2.SetParameter(ReductGeneratorParamHelper.WeightGenerator, weightGenerator);
            parms2.SetParameter(ReductGeneratorParamHelper.Epsilon, eps);
            parms2.SetParameter(ReductGeneratorParamHelper.PermutationCollection, permList);
            parms2.SetParameter(ReductGeneratorParamHelper.UseExceptionRules, true);

            ReductGeneratorWeightsMajority generator2 =
                ReductFactory.GetReductGenerator(parms2) as ReductGeneratorWeightsMajority;
            generator2.Generate();

            RoughClassifier classifier = new RoughClassifier(
                generator.GetReductStoreCollection(),
                RuleQuality.ConfidenceW, 
                RuleQuality.ConfidenceW,
                trainData.DataStoreInfo.GetDecisionValues());
            ClassificationResult result = classifier.Classify(testData, null);
                        
            RoughClassifier classifier2 = new RoughClassifier(
                generator2.GetReductStoreCollection(),
                RuleQuality.ConfidenceW, 
                RuleQuality.ConfidenceW,
                trainData.DataStoreInfo.GetDecisionValues());
            ClassificationResult result2 = classifier2.Classify(testData, null);
            
            return new Tuple<double, double>(result.Accuracy, result2.Accuracy);                                                                        
        }

        private ILog log;
                
        public ReductGeneralizedMajorityDecisionApproximateTest()
        {
            Random randSeed = new Random();
            RandomSingleton.Seed = Guid.NewGuid().GetHashCode();            

            NameValueCollection properties = new NameValueCollection();
            properties["showDateTime"] = "false";
            properties["showLogName"] = "false";
            properties["level"] = "All";

            properties["configType"] = "FILE";
            properties["configFile"] = "~/NLog.config";

            //Common.Logging.LogManager.Adapter = new Common.Logging.Simple.ConsoleOutLoggerFactoryAdapter(properties);
            Common.Logging.LogManager.Adapter = new Common.Logging.NLog.NLogLoggerFactoryAdapter(properties);

            log = Common.Logging.LogManager.GetLogger(this.GetType());            
        }        

        public IReduct CalculateGeneralizedMajorityApproximateDecisionReduct(DataStore data, decimal epsilon, int[] attributeSubset)
        {
            Args parms = new Args();
            parms.SetParameter(ReductGeneratorParamHelper.DataStore, data);
            parms.SetParameter(ReductGeneratorParamHelper.FactoryKey, ReductFactoryKeyHelper.GeneralizedMajorityDecisionApproximate);
            parms.SetParameter(ReductGeneratorParamHelper.WeightGenerator, new WeightGeneratorMajority(data));
            parms.SetParameter(ReductGeneratorParamHelper.Epsilon, epsilon);

            ReductGeneralizedMajorityDecisionApproximateGenerator reductGenerator =
                ReductFactory.GetReductGenerator(parms) as ReductGeneralizedMajorityDecisionApproximateGenerator;
            return reductGenerator.CalculateReduct(attributeSubset);
        }

        public IReduct CalculateApproximateReductFromSubset(DataStore data, decimal epsilon, int[] attributeSubset)
        {
            Args parms = new Args();
            parms.SetParameter(ReductGeneratorParamHelper.DataStore, data);
            parms.SetParameter(ReductGeneratorParamHelper.FactoryKey, ReductFactoryKeyHelper.ApproximateReductMajorityWeights);
            parms.SetParameter(ReductGeneratorParamHelper.WeightGenerator, new WeightGeneratorMajority(data));
            parms.SetParameter(ReductGeneratorParamHelper.Epsilon, epsilon);

            ReductGeneratorWeightsMajority reductGenerator =
                ReductFactory.GetReductGenerator(parms) as ReductGeneratorWeightsMajority;
            return reductGenerator.CalculateReduct(attributeSubset) as ReductWeights;
        }        
    }
}
