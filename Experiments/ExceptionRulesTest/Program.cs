//
// NRough Framework
// http://www.nrough.net
//
// Copyright © Sebastian Widz, 2017
// authors at nrough.net

//This file is part of NRough.

//Foobar is free software; you can redistribute it and/or modify
//it under the terms of the GNU General Public License as published by
//the Free Software Foundation; either version 3 of the License, or
//(at your option) any later version.
//
//Foobar is distributed in the hope that it will be useful,
//but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
//GNU General Public License for more details.
//
//You should have received a copy of the GNU General Public License
//along with Foobar; if not, write to the Free Software
//Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA  02110-1301  USA
//
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
using System.Diagnostics;

namespace ExceptionRulesTest
{
    public class Program
    {                
        private ReductRuleNumberComparer reductRuleNumberComparer = new ReductRuleNumberComparer();        
        private ReductStoreRuleNumberComparer reductStoreRuleNumComparer = new ReductStoreRuleNumberComparer(true);

        int minEpsilon = 0;
        int maxEpsilon = 100;

        SortDirection sortDirection = SortDirection.Descending;

        public void ExceptiodnRulesTest(KeyValuePair<string, BenchmarkData> kvp, int numberOfTests, int numberOfPermutations, int ensembleSize)
        {           
            DataStore trainData = null, testData = null, data = null;
            string filename = Path.Combine(@"log", kvp.Value.Name + String.Format("-{0}", ensembleSize) + ".result");
            DataSplitter splitter = null;

            ClassificationResult[, ,] results1 = new ClassificationResult[numberOfTests, maxEpsilon, kvp.Value.CrossValidationFolds];
            ClassificationResult[, ,] results2 = new ClassificationResult[numberOfTests, maxEpsilon, kvp.Value.CrossValidationFolds];
            ClassificationResult[, ,] results3 = new ClassificationResult[numberOfTests, maxEpsilon, kvp.Value.CrossValidationFolds];
            ClassificationResult[, ,] results4 = new ClassificationResult[numberOfTests, maxEpsilon, kvp.Value.CrossValidationFolds];            

            Console.WriteLine(ClassificationResult.TableHeader());

            for (int t = 0; t < numberOfTests; t++)
            {
                if (kvp.Value.CrossValidationActive)
                {
                    data = DataStore.Load(kvp.Value.DataFile, kvp.Value.DataFormat);
                    foreach (var attribute in data.DataStoreInfo.Attributes)
                        attribute.IsNumeric = false;

                    if (kvp.Value.DecisionFieldId != -1)
                        data.SetDecisionFieldId(kvp.Value.DecisionFieldId);

                    splitter = new DataSplitter(data, kvp.Value.CrossValidationFolds);
                }            

                for (int f = 0; f < kvp.Value.CrossValidationFolds; f++)
                {
                    if (kvp.Value.CrossValidationActive)
                    {                        
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
                
                    var permGenerator = new PermutationGenerator(trainData);
                    var permList = permGenerator.Generate(numberOfPermutations);                    

                    ParallelOptions options = new ParallelOptions();
                    options.MaxDegreeOfParallelism = ConfigManager.MaxDegreeOfParallelism;                    

                    Parallel.For(minEpsilon, maxEpsilon, options, i =>                    
                    {
                        var accuracy = this.ExceptionRulesSingleRun(trainData, testData, permList, i, ensembleSize);

                        results1[t, i, f] = accuracy.Item1;
                        results2[t, i, f] = accuracy.Item2;
                        results3[t, i, f] = accuracy.Item3;
                        results4[t, i, f] = accuracy.Item4;

                        Console.WriteLine(results1[t, i, f]);
                        Console.WriteLine(results2[t, i, f]);
                        Console.WriteLine(results3[t, i, f]);
                        Console.WriteLine(results4[t, i, f]); 
                    }
                    );

                    this.SaveResults(filename, results1, results2, results3, results4);
                }
            }
        }

        private void SaveResults(string filename,
            ClassificationResult[,,] results1,
            ClassificationResult[,,] results2,
            ClassificationResult[,,] results3,
            ClassificationResult[,,] results4)
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
                        }
                    }
                }
            }
        }

        private Tuple<ClassificationResult, ClassificationResult, ClassificationResult, ClassificationResult>
            ExceptionRulesSingleRun(DataStore trainData, DataStore testData, PermutationCollection permList, int epsilon, int ensembleSize)
        {
            WeightGeneratorMajority weightGenerator = new WeightGeneratorMajority(trainData);
            double eps = (double)epsilon / 100;            

            Args parmsApprox = new Args(6);
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
            IReductStoreCollection filteredReductStoreCollectionApprox
                = ensembleSize == 1
                ? origReductStoreCollectionApprox.Filter(ensembleSize, reductRuleNumberComparer)
                : origReductStoreCollectionApprox;

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

            resultApprox.AvgNumberOfAttributes = filteredReductStoreCollectionApprox.GetAvgMeasure(ReductMeasureLength.Instance, false, false);
            resultApprox.NumberOfRules = filteredReductStoreCollectionApprox.GetAvgMeasure(ReductMeasureNumberOfPartitions.Instance, true, false);
            resultApprox.AvgTreeHeight = filteredReductStoreCollectionApprox.GetAvgMeasure(ReductMeasureLength.Instance, true);
            resultApprox.MaxTreeHeight = 0;

            resultApprox.ModelCreationTime = generatorApprox.ReductGenerationTime;
            resultApprox.ClassificationTime = classifierApprox.ClassificationTime;

            TraceClassificationResult(resultApprox);            

            Args parmsEx = new Args(6);
            parmsEx.SetParameter(ReductFactoryOptions.DecisionTable, trainData);
            parmsEx.SetParameter(ReductFactoryOptions.ReductType, ReductTypes.GeneralizedMajorityDecisionApproximate);
            parmsEx.SetParameter(ReductFactoryOptions.WeightGenerator, weightGenerator);
            parmsEx.SetParameter(ReductFactoryOptions.Epsilon, eps);
            parmsEx.SetParameter(ReductFactoryOptions.PermutationCollection, permList);
            parmsEx.SetParameter(ReductFactoryOptions.UseExceptionRules, true);
            parmsEx.SetParameter(ReductFactoryOptions.EquivalenceClassSortDirection, sortDirection);

            ReductGeneralizedMajorityDecisionApproximateGenerator generatorEx =
                ReductFactory.GetReductGenerator(parmsEx) as ReductGeneralizedMajorityDecisionApproximateGenerator;
            generatorEx.Run();
            IReductStoreCollection origReductStoreCollectionEx = generatorEx.GetReductStoreCollection();
            IReductStoreCollection filteredReductStoreCollectionEx 
                = ensembleSize == 1
                ? origReductStoreCollectionEx.FilterInEnsemble(ensembleSize, reductStoreRuleNumComparer)
                : origReductStoreCollectionEx;

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

            resultEx.AvgNumberOfAttributes = filteredReductStoreCollectionEx.GetAvgMeasure(ReductMeasureLength.Instance, false, false);
            resultEx.NumberOfRules = filteredReductStoreCollectionEx.GetAvgSumPerStoreMeasure(ReductMeasureNumberOfPartitions.Instance, true, false);
            resultEx.AvgTreeHeight = filteredReductStoreCollectionEx.GetWeightedAvgPerEnsembleMeasure(ReductMeasureLength.Instance, true);
            resultEx.MaxTreeHeight = filteredReductStoreCollectionEx.GetAvgSumPerStoreMeasure(ReductMeasureNumberOfPartitions.Instance, true, true);

            resultEx.ModelCreationTime = generatorEx.ReductGenerationTime;
            resultEx.ClassificationTime = classifierEx.ClassificationTime;

            TraceClassificationResult(resultEx);            

            IReductStoreCollection origReductStoreCollectionGap = generatorEx.GetReductStoreCollection();
            IReductStoreCollection filteredReductStoreCollectionGap
                = ensembleSize == 1
                ? origReductStoreCollectionGap.FilterInEnsemble(ensembleSize, reductStoreRuleNumComparer)
                : origReductStoreCollectionGap;

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

            resultGaps.AvgNumberOfAttributes = filteredReductStoreCollectionGap.GetAvgMeasure(ReductMeasureLength.Instance, false, false);
            resultGaps.NumberOfRules = filteredReductStoreCollectionGap.GetAvgSumPerStoreMeasure(ReductMeasureNumberOfPartitions.Instance, true, false);
            resultGaps.AvgTreeHeight = filteredReductStoreCollectionGap.GetWeightedAvgPerEnsembleMeasure(ReductMeasureLength.Instance, true);
            resultGaps.MaxTreeHeight = filteredReductStoreCollectionGap.GetAvgSumPerStoreMeasure(ReductMeasureNumberOfPartitions.Instance, true, true);
            
            resultGaps.ModelCreationTime = generatorEx.ReductGenerationTime;
            resultGaps.ClassificationTime = classifierGaps.ClassificationTime;

            TraceClassificationResult(resultGaps);            

            Args parmsNoEx = new Args(6);
            parmsNoEx.SetParameter(ReductFactoryOptions.DecisionTable, trainData);
            parmsNoEx.SetParameter(ReductFactoryOptions.ReductType, ReductTypes.GeneralizedMajorityDecisionApproximate);
            parmsNoEx.SetParameter(ReductFactoryOptions.WeightGenerator, weightGenerator);
            parmsNoEx.SetParameter(ReductFactoryOptions.Epsilon, eps);
            parmsNoEx.SetParameter(ReductFactoryOptions.PermutationCollection, permList);
            parmsNoEx.SetParameter(ReductFactoryOptions.UseExceptionRules, false);
            parmsNoEx.SetParameter(ReductFactoryOptions.EquivalenceClassSortDirection, sortDirection);

            ReductGeneralizedMajorityDecisionApproximateGenerator generatorNoEx =
                ReductFactory.GetReductGenerator(parmsNoEx) as ReductGeneralizedMajorityDecisionApproximateGenerator;
            generatorNoEx.Run();
            IReductStoreCollection origReductStoreCollectionNoEx = generatorNoEx.GetReductStoreCollection();
            IReductStoreCollection filteredReductStoreCollectionNoEx
                = ensembleSize == 1 
                ? origReductStoreCollectionNoEx.Filter(ensembleSize, reductRuleNumberComparer)
                : origReductStoreCollectionNoEx;

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

            resultNoEx.AvgNumberOfAttributes = filteredReductStoreCollectionNoEx.GetAvgMeasure(ReductMeasureLength.Instance, false, false);
            
            //this is different (we do not calc avg sum per store) because all reducts are in single store!
            resultNoEx.NumberOfRules = filteredReductStoreCollectionNoEx.GetAvgMeasure(ReductMeasureNumberOfPartitions.Instance, true, false);

            resultNoEx.AvgTreeHeight = filteredReductStoreCollectionNoEx.GetAvgMeasure(ReductMeasureLength.Instance, true);
            resultNoEx.MaxTreeHeight = 0;
                             
            resultNoEx.ModelCreationTime = generatorNoEx.ReductGenerationTime;
            resultNoEx.ClassificationTime = classifierNoEx.ClassificationTime;

            TraceClassificationResult(resultNoEx);

            return new Tuple<ClassificationResult, ClassificationResult, ClassificationResult, ClassificationResult>
                (resultApprox, resultEx, resultGaps, resultNoEx);
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

            Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.BelowNormal;

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

            ClassificationResult.OutputColumns = @"ds;model;eps;acc;recallmacro;precisionmacro;attr;numrul;dtha;dthm;";

            var dta = BenchmarkDataHelper.GetDataFiles("Data", datasets);
            foreach (var kvp in dta)
            {                
                int numberOfPermutation = ensembleSize == 1 ? ensembleSize * 20 : ensembleSize;                
                program.ExceptiodnRulesTest(kvp, numberOfTests, numberOfPermutation, ensembleSize);
            }

            Pause();
        }

        [Conditional("DEBUG")]
        public static void TraceClassificationResult(ClassificationResult result)
        {
            Console.WriteLine(result.ConfusionMatrix);
        }

        [Conditional("DEBUG")]
        public static void Pause()
        {
            Console.ReadKey();
        }
    }
}