using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Common.Logging;
using Common.Logging.Configuration;
using NRough.Data;
using NRough.MachineLearning.Benchmark;
using NRough.MachineLearning.Roughset;
using NRough.Core;
using NRough.MachineLearning.Weighting;
using NRough.MachineLearning.Permutations;

namespace ExceptionRulesTiming
{
    public class Program
    {
        private static ILog log;

        private static void Main(string[] args)
        {
            if (args.Length < 1)
                throw new ArgumentException("Please provide number of tests.");

            int numberOfTests = Int32.Parse(args[0]);

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

            var dta = BenchmarkDataHelper.GetDataFiles("Data", null);
            foreach (var kvp in dta)
            {
                program.TimingTest(kvp, numberOfTests, 1, 1);
            }
        }

        public void TimingTest(KeyValuePair<string, BenchmarkData> kvp, int numberOfTests, int numberOfPermutations, int ensembleSize)
        {
            DataStore trainData = null, testData = null, data = null;
            string filename = Path.Combine(@"log", kvp.Value.Name + String.Format("-{0}", ensembleSize) + ".times");
            DataSplitter splitter = null;

            long[, ,] results1 = new long[numberOfTests, 100, kvp.Value.CrossValidationFolds];
            long[, ,] results2 = new long[numberOfTests, 100, kvp.Value.CrossValidationFolds];
            long[, ,] results3 = new long[numberOfTests, 100, kvp.Value.CrossValidationFolds];
            long[, ,] results4 = new long[numberOfTests, 100, kvp.Value.CrossValidationFolds];
            long[, ,] results5 = new long[numberOfTests, 100, kvp.Value.CrossValidationFolds];

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
                        trainData.DataStoreInfo.GetFieldIds(FieldGroup.Standard),
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

                    for (int i = 0; i < 100; i++)
                    {
                        var accuracy = this.ExceptionRulesSingleRun(trainData, testData, permList, 0, ensembleSize);

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

                    this.SaveResults(filename, results1, results2, results3, results4, results5);
                }
            }
        }

        private void SaveResults(string filename,
            long[, ,] results1,
            long[, ,] results2,
            long[, ,] results3,
            long[, ,] results4,
            long[, ,] results5)
        {
            using (StreamWriter outputFile = new StreamWriter(filename, false))
            {
                outputFile.WriteLine("Method|Fold|Test|Epsilon|Time");

                for (int t = 0; t < results1.GetLength(0); t++)
                    for (int i = 0; i < results1.GetLength(1); i++)
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

        private Tuple<long, long, long, long, long>
            ExceptionRulesSingleRun(DataStore trainData, DataStore testData, PermutationCollection permList, int epsilon, int ensembleSize)
        {
            Stopwatch t1, t2, t3, t4;
            WeightGeneratorRelative weightGenerator = new WeightGeneratorRelative(trainData);
            double eps = (double)epsilon / 100;

            Args parmsApprox = new Args();
            parmsApprox.SetParameter(ReductFactoryOptions.DecisionTable, trainData);
            parmsApprox.SetParameter(ReductFactoryOptions.ReductType, ReductTypes.ApproximateReductMajorityWeights);
            parmsApprox.SetParameter(ReductFactoryOptions.WeightGenerator, weightGenerator);
            parmsApprox.SetParameter(ReductFactoryOptions.Epsilon, eps);
            parmsApprox.SetParameter(ReductFactoryOptions.PermutationCollection, permList);
            parmsApprox.SetParameter(ReductFactoryOptions.UseExceptionRules, false);

            ReductGeneratorWeightsMajority generatorApprox =
                ReductFactory.GetReductGenerator(parmsApprox) as ReductGeneratorWeightsMajority;
            t1 = new Stopwatch();
            t1.Start();
            generatorApprox.Run();
            t1.Stop();

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
            t2 = new Stopwatch();
            t2.Start();
            generator_GMDR.Run();
            t2.Stop();

            Args parmsEx = new Args();
            parmsEx.SetParameter(ReductFactoryOptions.DecisionTable, trainData);
            parmsEx.SetParameter(ReductFactoryOptions.ReductType, ReductTypes.GeneralizedMajorityDecisionApproximate);
            parmsEx.SetParameter(ReductFactoryOptions.WeightGenerator, weightGenerator);
            parmsEx.SetParameter(ReductFactoryOptions.Epsilon, eps);
            parmsEx.SetParameter(ReductFactoryOptions.PermutationCollection, permList);
            parmsEx.SetParameter(ReductFactoryOptions.UseExceptionRules, true);

            ReductGeneralizedMajorityDecisionApproximateGenerator generatorEx =
                ReductFactory.GetReductGenerator(parmsEx) as ReductGeneralizedMajorityDecisionApproximateGenerator;
            t3 = new Stopwatch();
            t3.Start();
            generatorEx.Run();
            t3.Stop();

            double avgSize = generatorApprox.GetReductStoreCollection().Filter(ensembleSize, new ReductLengthComparer()).GetAvgMeasure(new ReductMeasureLength());

            var localPermGenerator = new PermutationGenerator(trainData);
            var localPermList = localPermGenerator.Generate(ensembleSize);

            Args parmsRandom = new Args();
            parmsRandom.SetParameter(ReductFactoryOptions.DecisionTable, trainData);
            parmsRandom.SetParameter(ReductFactoryOptions.ReductType, ReductTypes.RandomSubset);
            parmsRandom.SetParameter(ReductFactoryOptions.WeightGenerator, weightGenerator);
            parmsRandom.SetParameter(ReductFactoryOptions.Epsilon, eps);
            parmsRandom.SetParameter(ReductFactoryOptions.PermutationCollection, localPermList);
            parmsRandom.SetParameter(ReductFactoryOptions.UseExceptionRules, false);

            parmsRandom.SetParameter(ReductFactoryOptions.MinReductLength, (int)avgSize);
            parmsRandom.SetParameter(ReductFactoryOptions.MaxReductLength, (int)avgSize);

            ReductRandomSubsetGenerator generatorRandom =
                ReductFactory.GetReductGenerator(parmsRandom) as ReductRandomSubsetGenerator;
            t4 = new Stopwatch();
            t4.Start();
            generatorRandom.Run();
            t4.Start();

            return new Tuple<long, long, long, long, long>
                (t2.ElapsedMilliseconds,   //1
                t1.ElapsedMilliseconds,        //2
                t3.ElapsedMilliseconds,       //3
                t4.ElapsedMilliseconds,        //5
                t3.ElapsedMilliseconds);    //4
        }
    }
}