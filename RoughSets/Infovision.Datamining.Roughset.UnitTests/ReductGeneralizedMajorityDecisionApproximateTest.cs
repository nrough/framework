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

namespace Infovision.Datamining.Roughset.UnitTests
{
    [TestFixture]
    public class ReductGeneralizedMajorityDecisionApproximateTest
    {
        public IEnumerable<KeyValuePair<string, BenchmarkData>> GetDataFiles()
        {
            return BenchmarkDataHelper.GetDataFiles(
                new string[] { 
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
            DataStore data = DataStore.Load(kvp.Value.TrainFile, FileFormat.Rses1);
            
            foreach (int fieldId in data.DataStoreInfo.GetFieldIds(FieldTypes.Standard))
                data.DataStoreInfo.GetFieldInfo(fieldId).Alias = kvp.Value.GetFieldAlias(fieldId);

            DataStore test = DataStore.Load(kvp.Value.TestFile, FileFormat.Rses1, data.DataStoreInfo);
            
            log.InfoFormat(data.Name);

            PermutationGenerator permGenerator = new PermutationGenerator(data);
            int numberOfPermutations = 100;
            PermutationCollection permList = permGenerator.Generate(numberOfPermutations);
            
            Decimal dataQuality = new InformationMeasureWeights().Calc(
                new ReductGeneralizedMajorityDecision(
                    data, data.DataStoreInfo.GetFieldIds(FieldTypes.Standard), Decimal.Zero));

            Decimal dataQuality_2 = new InformationMeasureWeights().Calc(
                new ReductWeights(
                    data, data.DataStoreInfo.GetFieldIds(FieldTypes.Standard), Decimal.Zero));

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

                    Assert.NotNull(reduct_1);
                    Decimal reductQuality_1 = new InformationMeasureWeights().Calc(reduct_1);
                    Assert.GreaterOrEqual(reductQuality_1, Decimal.Round(dataQuality * (Decimal.One - eps), 17));

                    elapsed_sum_1 += watch_1.ElapsedMilliseconds;
                    len_sum_1 += reduct_1.Attributes.Count;
                    avg_quality_1 += reductQuality_1;

                    RoughClassifier classifier_1 = new RoughClassifier();
                    classifier_1.Classify(test, reduct_1);
                    ClassificationResult result_1 = classifier_1.Vote(
                        test, 
                        IdentificationType.WeightConfidence, 
                        VoteType.WeightConfidence, 
                        null);

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

                    Assert.NotNull(reduct_2);
                    Decimal reductQuality_2 = new InformationMeasureWeights().Calc(reduct_2);
                    Assert.GreaterOrEqual(reductQuality_2, Decimal.Round(dataQuality * (Decimal.One - eps), 17));

                    elapsed_sum_2 += watch_2.ElapsedMilliseconds;
                    len_sum_2 += reduct_2.Attributes.Count;
                    avg_quality_2 += reductQuality_2;

                    RoughClassifier classifier_2 = new RoughClassifier();
                    classifier_2.Classify(test, reduct_2);
                    ClassificationResult result_2 = classifier_2.Vote(
                        test,
                        IdentificationType.WeightConfidence,
                        VoteType.WeightConfidence,
                        null);

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

            DataStore data = DataStore.Load(kvp.Value.TrainFile, FileFormat.Rses1);
            foreach (int fieldId in data.DataStoreInfo.GetFieldIds(FieldTypes.Standard))
                data.DataStoreInfo.GetFieldInfo(fieldId).Alias = kvp.Value.GetFieldAlias(fieldId);
            DataStore test = DataStore.Load(kvp.Value.TestFile, FileFormat.Rses1, data.DataStoreInfo);
            log.InfoFormat(data.Name);                        
            WeightGeneratorMajority weightGenerator = new WeightGeneratorMajority(data);

            double[,] results = new double[numberOfTests, 100];

            for (int t = 0; t < numberOfTests; t++)
            {
                PermutationGenerator permGenerator = new PermutationGenerator(data);
                PermutationCollection permList = permGenerator.Generate(numberOfPermutations);

                for (int i = 0; i<100; i++)
                {
                    decimal eps = Decimal.Divide(i, 100);
                    
                    Args parms = new Args();
                    parms.AddParameter(ReductGeneratorParamHelper.DataStore, data);
                    parms.AddParameter(ReductGeneratorParamHelper.FactoryKey, ReductFactoryKeyHelper.GeneralizedMajorityDecisionApproximate);
                    parms.AddParameter(ReductGeneratorParamHelper.WeightGenerator, weightGenerator);
                    parms.AddParameter(ReductGeneratorParamHelper.Epsilon, eps);
                    parms.AddParameter(ReductGeneratorParamHelper.PermutationCollection, permList);
                    parms.AddParameter(ReductGeneratorParamHelper.UseExceptionRules, true);

                    ReductGeneralizedMajorityDecisionApproximateGenerator generator =
                        ReductFactory.GetReductGenerator(parms) as ReductGeneralizedMajorityDecisionApproximateGenerator;
                    generator.Generate();

                    RoughClassifier classifier = new RoughClassifier();
                    classifier.Classify(test, generator.GetReductStoreCollection());
                    ClassificationResult result = classifier.Vote(
                        test, IdentificationType.WeightConfidence, VoteType.WeightConfidence, null);

                    results[t, i] = result.Accuracy;
                    log.InfoFormat("{0}|{1}|{2}", t, eps, result.Accuracy);
                }
            }
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
            parms.AddParameter(ReductGeneratorParamHelper.DataStore, data);
            parms.AddParameter(ReductGeneratorParamHelper.FactoryKey, ReductFactoryKeyHelper.GeneralizedMajorityDecisionApproximate);
            parms.AddParameter(ReductGeneratorParamHelper.WeightGenerator, new WeightGeneratorMajority(data));
            parms.AddParameter(ReductGeneratorParamHelper.Epsilon, epsilon);

            ReductGeneralizedMajorityDecisionApproximateGenerator reductGenerator =
                ReductFactory.GetReductGenerator(parms) as ReductGeneralizedMajorityDecisionApproximateGenerator;
            return reductGenerator.CalculateReduct(attributeSubset);
        }

        public IReduct CalculateApproximateReductFromSubset(DataStore data, decimal epsilon, int[] attributeSubset)
        {
            Args parms = new Args();
            parms.AddParameter(ReductGeneratorParamHelper.DataStore, data);
            parms.AddParameter(ReductGeneratorParamHelper.FactoryKey, ReductFactoryKeyHelper.ApproximateReductMajorityWeights);
            parms.AddParameter(ReductGeneratorParamHelper.WeightGenerator, new WeightGeneratorMajority(data));
            parms.AddParameter(ReductGeneratorParamHelper.Epsilon, epsilon);

            ReductGeneratorWeightsMajority reductGenerator =
                ReductFactory.GetReductGenerator(parms) as ReductGeneratorWeightsMajority;
            return reductGenerator.CalculateReduct(attributeSubset) as ReductWeights;
        }        
    }
}
