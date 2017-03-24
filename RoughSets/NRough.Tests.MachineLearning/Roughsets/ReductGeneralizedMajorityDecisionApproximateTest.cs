using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using NRough.Data;
using NRough.Core;
using NUnit.Framework;
using NRough.MachineLearning.Permutations;
using NRough.MachineLearning.Weighting;
using NRough.MachineLearning.Classification;
using System.Collections.Specialized;
using NRough.MachineLearning.Roughsets;
using NRough.Core.Random;
using NRough.Core.CollectionExtensions;
using NRough.Core.Comparers;
using NRough.Data.Benchmark;
using NRough.MachineLearning;

namespace NRough.Tests.MachineLearning.Roughsets
{
    [TestFixture]
    public class ReductGeneralizedMajorityDecisionApproximateTest 
    {
        [Test, Repeat(1)]
        public void TestExceptionCaseWithoutExceptions()
        {
            var trainData = Data.Benchmark.Factory.SoybeanSmall();
            var testData = Data.Benchmark.Factory.Monks2Test();


            double eps = 0.2;
            var weightGenerator = new WeightGeneratorMajority(trainData);

            var permList = new PermutationCollection(100, trainData.GetStandardFields());
            
            /*
            var permList = new PermutationCollection(new Permutation[] {
                new Permutation(new int[] { 4, 6, 5, 1, 3, 2 }),
                new Permutation(new int[] { 4, 6, 5, 1, 2, 3 }),
                new Permutation(new int[] { 4, 6, 5, 2, 1, 3 }),
                new Permutation(new int[] { 6, 1, 4, 5, 3, 2 }),
                new Permutation(new int[] { 6, 1, 4, 5, 2, 3 })
                });
            */

            Console.WriteLine(permList.ElementAt(0).ToArray().ToStr(' '));

            trainData.SetWeights(weightGenerator.Weights);

            Args parmsEx = new Args(6);
            parmsEx.SetParameter(ReductFactoryOptions.DecisionTable, trainData);
            parmsEx.SetParameter(ReductFactoryOptions.ReductType, ReductTypes.GeneralizedMajorityDecisionApproximate);
            parmsEx.SetParameter(ReductFactoryOptions.WeightGenerator, weightGenerator);
            parmsEx.SetParameter(ReductFactoryOptions.Epsilon, eps);
            parmsEx.SetParameter(ReductFactoryOptions.PermutationCollection, permList);
            parmsEx.SetParameter(ReductFactoryOptions.UseExceptionRules, true);
            parmsEx.SetParameter(ReductFactoryOptions.EquivalenceClassSortDirection, 
                SortDirection.Descending);
            
            ReductGeneralizedMajorityDecisionApproximateGenerator generatorEx =
                ReductFactory.GetReductGenerator(parmsEx) as ReductGeneralizedMajorityDecisionApproximateGenerator;
            generatorEx.Run();
            IReductStoreCollection origReductStoreCollectionEx = generatorEx.GetReductStoreCollection();


            int i = 0;
            foreach (var reductStore in origReductStoreCollectionEx)
            {
                Console.Write(permList.ElementAt(i).ToArray().ToStr(' '));
                Console.Write(" : ");
                foreach (var reduct in reductStore.Where(r => r.IsException == false))
                    Console.Write(reduct);
                Console.Write(" -> ");
                Console.Write(reductStore.Where(r => r.IsException).Count());

                var eqClasses = EquivalenceClassCollection.Create(
                    permList.ElementAt(i).ToArray(),
                    trainData,
                    weightGenerator.Weights);



                Console.WriteLine();
                foreach (var eqClass in eqClasses)
                {
                    Console.WriteLine("[{0}]{{{1}}}", eqClass.Instance.ToStr(' '), eqClass.DecisionSet.ToArray().ToStr(','));
                }

                i++;
                Console.WriteLine();
            }
        }





        public static IEnumerable<KeyValuePair<string, BenchmarkData>> GetDataFiles()
        {
            return BenchmarkDataHelper.GetDataFiles("Data",
                new string[] {
                    //"zoo",
                    //"semeion",
                    //"opt",
                    "dna"
                    //"letter",
                    //"monks-1",
                    //"monks-2",
                    //"monks-3",
                    //"spect",
                    //"pen"
                });
        }

        [Test, TestCaseSource("GetDataFiles")]
        public void EmptyReductsTest(KeyValuePair<string, BenchmarkData> kvp)
        {
            DataStore data = DataStore.Load(kvp.Value.TrainFile, kvp.Value.DataFormat);
            DataStore test = DataStore.Load(kvp.Value.TestFile, DataFormat.RSES1, data.DataStoreInfo);

            Args args = new Args();
            args.SetParameter(ReductFactoryOptions.DecisionTable, data);
            args.SetParameter(ReductFactoryOptions.ReductType, ReductTypes.GeneralizedMajorityDecisionApproximate);
            args.SetParameter(ReductFactoryOptions.WeightGenerator, new WeightGeneratorRelative(data));
            args.SetParameter(ReductFactoryOptions.Epsilon, 0.1);

            Permutation p = ReductFactory.GetPermutationGenerator(args).Generate(1).FirstOrDefault();
            Permutation p2 = new Permutation(p.ToArray().SubArray(0, p.Length / 4));
            PermutationCollection permCollection = new PermutationCollection();
            for (int i = 0; i < 100; i++)
                permCollection.Add(p2);

            args.SetParameter(ReductFactoryOptions.PermutationCollection, permCollection);

            IReductGenerator gen = ReductFactory.GetReductGenerator(args);
            gen.Run();
            IReductStoreCollection model = gen.GetReductStoreCollection();
        }

        [Test, TestCaseSource("GetDataFiles")]
        public void CalculateReductTest(KeyValuePair<string, BenchmarkData> kvp)
        {
            DataStore data = DataStore.Load(kvp.Value.TrainFile, kvp.Value.DataFormat);

            foreach (int fieldId in data.DataStoreInfo.SelectAttributeIds(a => a.IsStandard))
                data.DataStoreInfo.GetFieldInfo(fieldId).Alias = kvp.Value.GetFieldAlias(fieldId);

            DataStore test = DataStore.Load(kvp.Value.TestFile, DataFormat.RSES1, data.DataStoreInfo);

            //log.InfoFormat(data.Name);

            PermutationGenerator permGenerator = new PermutationGenerator(data);
            int numberOfPermutations = 10;
            PermutationCollection permList = permGenerator.Generate(numberOfPermutations);

            WeightGeneratorMajority weightGenerator = new WeightGeneratorMajority(data);

            double dataQuality = new InformationMeasureWeights().Calc(
                new ReductWeights(data, data.DataStoreInfo.SelectAttributeIds(a => a.IsStandard), 0.0, weightGenerator.Weights));

            double dataQuality_2 = new InformationMeasureWeights().Calc(
                new ReductWeights(data, data.DataStoreInfo.SelectAttributeIds(a => a.IsStandard), 0.0, weightGenerator.Weights));
            
            Assert.That(dataQuality, Is.EqualTo(dataQuality_2).Using((IComparer<double>)ToleranceDoubleComparer.Instance));

            for (double eps = 0.0; eps <= 0.5; eps += 0.1)
            {
                long elapsed_sum_1 = 0;
                long elapsed_sum_2 = 0;

                int len_sum_1 = 0;
                int len_sum_2 = 0;

                double avg_quality_1 = 0.0;
                double avg_quality_2 = 0.0;

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

                    double reductQuality_1 = InformationMeasureWeights.Instance.Calc(reduct_1);

                    Assert.That(reductQuality_1, 
                        Is.GreaterThanOrEqualTo((1.0 - eps) * dataQuality)
                        .Using(ToleranceDoubleComparer.Instance));                    

                    elapsed_sum_1 += watch_1.ElapsedMilliseconds;
                    len_sum_1 += reduct_1.Attributes.Count;
                    avg_quality_1 += reductQuality_1;

                    RoughClassifier classifier_1 = new RoughClassifier(
                        reductStoreCollection,
                        RuleQualityMethods.ConfidenceW,
                        RuleQualityMethods.ConfidenceW,
                        data.DataStoreInfo.GetDecisionValues());

                    ClassificationResult result_1 = classifier_1.Classify(test, null);

                    accuracyResults_1[i] = result_1.Accuracy;

                    /*
                    log.InfoFormat("|A|{0}|{3}|{1}|{2}|{4}|",
                        reduct_1.ToString(),
                        reductQuality_1,
                        watch_1.ElapsedMilliseconds,
                        reduct_1.Attributes.Count,
                        result_1.Accuracy);
                    */

                    var watch_2 = Stopwatch.StartNew();
                    IReduct reduct_2 = CalculateApproximateReductFromSubset(data, eps, attributes);
                    watch_2.Stop();

                    IReductStore store2 = new ReductStore(1);
                    store2.AddReduct(reduct_2);
                    IReductStoreCollection reductStoreCollection2 = new ReductStoreCollection(1);
                    reductStoreCollection2.AddStore(store2);

                    Assert.NotNull(reduct_2);
                    double reductQuality_2 = new InformationMeasureWeights().Calc(reduct_2);

                    Assert.That(reductQuality_2, 
                        Is.GreaterThanOrEqualTo((1.0 - eps) * dataQuality)
                        .Using(ToleranceDoubleComparer.Instance));                    

                    elapsed_sum_2 += watch_2.ElapsedMilliseconds;
                    len_sum_2 += reduct_2.Attributes.Count;
                    avg_quality_2 += reductQuality_2;

                    RoughClassifier classifier_2 = new RoughClassifier(
                        reductStoreCollection2,
                        RuleQualityMethods.ConfidenceW,
                        RuleQualityMethods.ConfidenceW,
                        data.DataStoreInfo.GetDecisionValues());
                    ClassificationResult result_2 = classifier_2.Classify(test, null);

                    accuracyResults_2[i] = result_2.Accuracy;

                    /*
                    log.InfoFormat("|B|{0}|{3}|{1}|{2}|{4}|",
                        reduct_2.ToString(),
                        reductQuality_2,
                        watch_2.ElapsedMilliseconds,
                        reduct_2.Attributes.Count,
                        result_2.Accuracy);
                    */

                    i++;
                }

                /*
                log.InfoFormat(Environment.NewLine);

                log.InfoFormat("==========================================");
                log.InfoFormat("Average reduct lenght of method A: {0}", (double)len_sum_1 / (double)permList.Count);
                log.InfoFormat("Average reduct lenght of method B: {0}", (double)len_sum_2 / (double)permList.Count);
                log.InfoFormat("Average computation time method A: {0}", (double)elapsed_sum_1 / (double)permList.Count);
                log.InfoFormat("Average computation time method B: {0}", (double)elapsed_sum_2 / (double)permList.Count);
                log.InfoFormat("Average reduct quality of method A: {0}", avg_quality_1 / (double)permList.Count);
                log.InfoFormat("Average reduct quality of method B: {0}", avg_quality_2 / (double)permList.Count);

                log.InfoFormat("Accuracy A Min: {0} Max: {1} Mean: {2} StdDev: {3}",
                    Tools.Min(accuracyResults_1), Tools.Max(accuracyResults_1), Tools.Mean(accuracyResults_1), Tools.StdDev(accuracyResults_1));
                log.InfoFormat("Accuracy B Min: {0} Max: {1} Mean: {2} StdDev: {3}",
                    Tools.Min(accuracyResults_2), Tools.Max(accuracyResults_2), Tools.Mean(accuracyResults_2), Tools.StdDev(accuracyResults_2));

                log.InfoFormat("==========================================");

                log.InfoFormat(Environment.NewLine);
                */
            }
        }

        [Test, TestCaseSource("GetDataFiles")]
        public void ExceptiodnRulesTest(KeyValuePair<string, BenchmarkData> kvp)
        {
            int numberOfPermutations = 10;
            int numberOfTests = 1;

            DataStore trainData = null, testData = null, data = null;

            double[, ,] results = new double[numberOfTests, 100, kvp.Value.CrossValidationFolds];
            double[, ,] results2 = new double[numberOfTests, 100, kvp.Value.CrossValidationFolds];

            string name;

            if (kvp.Value.CrossValidationActive)
            {
                data = DataStore.Load(kvp.Value.DataFile, DataFormat.RSES1);
                name = data.Name;
                DataSplitter splitter = new DataSplitter(data, kvp.Value.CrossValidationFolds);

                for (int f = 0; f <= kvp.Value.CrossValidationFolds; f++)
                {                    
                    splitter.Split(out trainData, out testData, f);

                    for (int t = 0; t < numberOfTests; t++)
                    {
                        PermutationGenerator permGenerator = new PermutationGenerator(trainData);
                        PermutationCollection permList = permGenerator.Generate(numberOfPermutations);

                        Parallel.For(0, 100, i =>
                        {
                            var accuracy = ExceptionRulesSingleRun(trainData, testData, permList, i, t, f);

                            results[t, i, f] = accuracy.Item1;
                            results2[t, i, f] = accuracy.Item2;

                            //log.InfoFormat("A|{0}|{1}|{2}|{3}", f, t, i, accuracy.Item1);
                            //log.InfoFormat("B|{0}|{1}|{2}|{3}", f, t, i, accuracy.Item2);
                        });
                    }
                }
            }
            else
            {
                int f = 0;
                trainData = DataStore.Load(kvp.Value.TrainFile, DataFormat.RSES1);
                foreach (int fieldId in trainData.DataStoreInfo.SelectAttributeIds(a => a.IsStandard))
                    trainData.DataStoreInfo.GetFieldInfo(fieldId).Alias = kvp.Value.GetFieldAlias(fieldId);
                testData = DataStore.Load(kvp.Value.TestFile, DataFormat.RSES1, trainData.DataStoreInfo);
                name = trainData.Name;
                Console.WriteLine(trainData.Name);

                for (int t = 0; t < numberOfTests; t++)
                {
                    PermutationGenerator permGenerator = new PermutationGenerator(trainData);
                    PermutationCollection permList = permGenerator.Generate(numberOfPermutations);

                    Parallel.For(0, 100, i =>
                    {
                        var accuracy = ExceptionRulesSingleRun(trainData, testData, permList, i, t, f);

                        results[t, i, f] = accuracy.Item1;
                        results2[t, i, f] = accuracy.Item2;

                        //log.InfoFormat("A|{0}|{1}|{2}|{3}", f, t, i, accuracy.Item1);
                        //log.InfoFormat("B|{0}|{1}|{2}|{3}", f, t, i, accuracy.Item2);
                    });
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
                            outputFile.WriteLine("B|{0}|{1}|{2}|{3}", f, t, i, res1[t, i, f]);
                        }
                    }
                }
            }
        }

        private Tuple<double, double> ExceptionRulesSingleRun(DataStore trainData, DataStore testData, PermutationCollection permList, int epsilon, int test, int fold)
        {
            WeightGeneratorMajority weightGenerator = new WeightGeneratorMajority(trainData);
            double eps = (double) epsilon / 100.0;

            Args parms = new Args();
            parms.SetParameter(ReductFactoryOptions.DecisionTable, trainData);
            parms.SetParameter(ReductFactoryOptions.ReductType, ReductTypes.GeneralizedMajorityDecisionApproximate);
            parms.SetParameter(ReductFactoryOptions.WeightGenerator, weightGenerator);
            parms.SetParameter(ReductFactoryOptions.Epsilon, eps);
            parms.SetParameter(ReductFactoryOptions.PermutationCollection, permList);
            parms.SetParameter(ReductFactoryOptions.UseExceptionRules, true);

            ReductGeneralizedMajorityDecisionApproximateGenerator generator =
                ReductFactory.GetReductGenerator(parms) as ReductGeneralizedMajorityDecisionApproximateGenerator;
            generator.Run();

            Args parms2 = new Args();
            parms2.SetParameter(ReductFactoryOptions.DecisionTable, trainData);
            parms2.SetParameter(ReductFactoryOptions.ReductType, ReductTypes.ApproximateReductMajorityWeights);
            parms2.SetParameter(ReductFactoryOptions.WeightGenerator, weightGenerator);
            parms2.SetParameter(ReductFactoryOptions.Epsilon, eps);
            parms2.SetParameter(ReductFactoryOptions.PermutationCollection, permList);
            parms2.SetParameter(ReductFactoryOptions.UseExceptionRules, true);

            ReductGeneratorWeightsMajority generator2 =
                ReductFactory.GetReductGenerator(parms2) as ReductGeneratorWeightsMajority;
            generator2.Run();

            RoughClassifier classifier = new RoughClassifier(
                generator.GetReductStoreCollection(),
                RuleQualityMethods.ConfidenceW,
                RuleQualityMethods.ConfidenceW,
                trainData.DataStoreInfo.GetDecisionValues());
            ClassificationResult result = classifier.Classify(testData, null);

            RoughClassifier classifier2 = new RoughClassifier(
                generator2.GetReductStoreCollection(),
                RuleQualityMethods.ConfidenceW,
                RuleQualityMethods.ConfidenceW,
                trainData.DataStoreInfo.GetDecisionValues());
            ClassificationResult result2 = classifier2.Classify(testData, null);

            return new Tuple<double, double>(result.Accuracy, result2.Accuracy);
        }        

        public ReductGeneralizedMajorityDecisionApproximateTest()
        {            
            RandomSingleton.Seed = Guid.NewGuid().GetHashCode();            
        }

        public IReduct CalculateGeneralizedMajorityApproximateDecisionReduct(DataStore data, double epsilon, int[] attributeSubset)
        {
            Args parms = new Args(5);
            parms.SetParameter(ReductFactoryOptions.DecisionTable, data);
            parms.SetParameter(ReductFactoryOptions.ReductType, ReductTypes.GeneralizedMajorityDecisionApproximate);
            parms.SetParameter(ReductFactoryOptions.WeightGenerator, new WeightGeneratorMajority(data));
            parms.SetParameter(ReductFactoryOptions.Epsilon, epsilon);
            parms.SetParameter(ReductFactoryOptions.UseExceptionRules, false);

            ReductGeneralizedMajorityDecisionApproximateGenerator reductGenerator =
                ReductFactory.GetReductGenerator(parms) as ReductGeneralizedMajorityDecisionApproximateGenerator;

            IReduct result = reductGenerator.CalculateReduct(attributeSubset);
            result.EquivalenceClasses.RecalcEquivalenceClassStatistic(data);

            return result;
        }

        public IReduct CalculateApproximateReductFromSubset(DataStore data, double epsilon, int[] attributeSubset)
        {
            Args parms = new Args();
            parms.SetParameter(ReductFactoryOptions.DecisionTable, data);
            parms.SetParameter(ReductFactoryOptions.ReductType, ReductTypes.ApproximateReductMajorityWeights);
            parms.SetParameter(ReductFactoryOptions.WeightGenerator, new WeightGeneratorMajority(data));
            parms.SetParameter(ReductFactoryOptions.Epsilon, epsilon);
            parms.SetParameter(ReductFactoryOptions.UseExceptionRules, false);

            ReductGeneratorWeightsMajority reductGenerator =
                ReductFactory.GetReductGenerator(parms) as ReductGeneratorWeightsMajority;

            reductGenerator.UsePerformanceImprovements = false;

            return reductGenerator.CalculateReduct(attributeSubset) as ReductWeights;
        }
    }
}