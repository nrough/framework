﻿using System;
using System.Collections.Generic;
using System.Linq;
using Infovision.MachineLearning;
using Infovision.MachineLearning.Benchmark;
using Infovision.MachineLearning.Filters.Unsupervised.Attribute;
using Infovision.MachineLearning.Roughset;
using Infovision.Core;
using NUnit.Framework;
using Infovision.MachineLearning.Permutations;
using Infovision.MachineLearning.Classification;

namespace Infovision.Data.UnitTests
{
    [TestFixture]
    internal class DiscretizeTest
    {
        public static IEnumerable<KeyValuePair<string, BenchmarkData>> GetDataFiles()
        {
            return BenchmarkDataHelper.GetDataFiles("Data",
                new string[] {
                    "german",
                    "sat"
                });
        }

        [Test, TestCaseSource("GetDataFiles")]
        public void UpdateColumnTest(KeyValuePair<string, BenchmarkData> kvp)
        {
            double epsilon = 0.05;
            int numberOfPermutations = 20;

            BenchmarkData benchmark = kvp.Value;
            DataStore data = null, train = null, test = null;
            DataStoreSplitter splitter = null;
            DataFieldInfo localFieldInfoTrain, localFieldInfoTest;

            if (benchmark.CrossValidationActive)
            {
                data = DataStore.Load(benchmark.DataFile, benchmark.FileFormat);
                splitter = new DataStoreSplitter(data, benchmark.CrossValidationFolds);
            }
            else
            {
                train = DataStore.Load(benchmark.TrainFile, benchmark.FileFormat);
                test = DataStore.Load(benchmark.TestFile, benchmark.FileFormat, train.DataStoreInfo);
            }

            for (int i = 0; i < benchmark.CrossValidationFolds; i++)
            {
                if (splitter != null)                    
                    splitter.Split(ref train, ref test, i);

                //train.WriteToCSVFileExt(String.Format("disc_german_orig_{0}.trn", i), " ");
                //test.WriteToCSVFileExt(String.Format("disc_german_orig_{0}.tst", i), " ");

                Args args = new Args();
                args.SetParameter(ReductGeneratorParamHelper.TrainData, train);
                args.SetParameter(ReductGeneratorParamHelper.Epsilon, epsilon);
                args.SetParameter(ReductGeneratorParamHelper.FactoryKey, ReductFactoryKeyHelper.ApproximateReductMajorityWeights);
                PermutationCollection permutations = ReductFactory.GetPermutationGenerator(args).Generate(numberOfPermutations);
                args.SetParameter(ReductGeneratorParamHelper.PermutationCollection, permutations);

                IReductGenerator reductGenerator = ReductFactory.GetReductGenerator(args);
                reductGenerator.Run();

                IReductStoreCollection reductStoreCollection = reductGenerator.GetReductStoreCollection();
                Console.WriteLine("Average reduct length: {0}", reductStoreCollection.GetAvgMeasure(new ReductMeasureLength()));
                IReductStore reductStore = reductStoreCollection.FirstOrDefault();

                RoughClassifier classifier = new RoughClassifier(
                    reductStoreCollection,
                    RuleQuality.ConfidenceW,
                    RuleQuality.ConfidenceW,
                    train.DataStoreInfo.DecisionInfo.InternalValues());

                ClassificationResult classificationResult = classifier.Classify(test);
                Console.WriteLine("Accuracy: {0}", classificationResult.Accuracy);

                Console.WriteLine("+++++++++++++++++++++++++++++++++++++++++++++++++++");

                if (benchmark.CheckDiscretize())
                {
                    foreach (DataFieldInfo field in benchmark.GetNumericFields())
                    {
                        localFieldInfoTrain = train.DataStoreInfo.GetFieldInfo(field.Id);
                        localFieldInfoTrain.IsNumeric = false;

                        int[] newValues = new int[train.NumberOfRecords];

                        switch (Type.GetTypeCode(localFieldInfoTrain.FieldValueType))
                        {
                            case TypeCode.Int32:
                                Discretization<int> discretizeInt = new Discretization<int>();
                                discretizeInt.UseEntropy = benchmark.DiscretizeUsingEntropy;
                                discretizeInt.UseEqualFrequency = benchmark.DiscretizeUsingEqualFreq;
                                int[] oldValuesInt = train.GetColumn<int>(field.Id);
                                discretizeInt.Compute(oldValuesInt);
                                localFieldInfoTrain.Cuts = discretizeInt.Cuts;
                                for (int j = 0; j < train.NumberOfRecords; j++)
                                    newValues[j] = discretizeInt.Search(oldValuesInt[j]);
                                break;

                            case TypeCode.Double:
                                Discretization<double> discretizeDouble = new Discretization<double>();
                                discretizeDouble.UseEntropy = benchmark.DiscretizeUsingEntropy;
                                discretizeDouble.UseEqualFrequency = benchmark.DiscretizeUsingEqualFreq;
                                double[] oldValuesDouble = train.GetColumn<double>(field.Id);
                                discretizeDouble.Compute(oldValuesDouble);
                                localFieldInfoTrain.Cuts = discretizeDouble.Cuts;
                                for (int j = 0; j < train.NumberOfRecords; j++)
                                    newValues[j] = discretizeDouble.Search(oldValuesDouble[j]);
                                break;
                        }

                        localFieldInfoTrain.FieldValueType = typeof(int);
                        train.UpdateColumn(field.Id, Array.ConvertAll(newValues, x => (object)x));

                        localFieldInfoTest = test.DataStoreInfo.GetFieldInfo(field.Id);
                        localFieldInfoTest.IsNumeric = false;

                        newValues = new int[test.NumberOfRecords];

                        switch (Type.GetTypeCode(localFieldInfoTest.FieldValueType))
                        {
                            case TypeCode.Int32:
                                Discretization<int> discretizeInt = new Discretization<int>();
                                discretizeInt.Cuts = localFieldInfoTrain.Cuts;
                                int[] oldValuesInt = test.GetColumn<int>(field.Id);
                                for (int j = 0; j < test.NumberOfRecords; j++)
                                    newValues[j] = discretizeInt.Search(oldValuesInt[j]);
                                break;

                            case TypeCode.Double:
                                Discretization<double> discretizeDouble = new Discretization<double>();
                                discretizeDouble.Cuts = localFieldInfoTrain.Cuts;
                                double[] oldValuesDouble = test.GetColumn<double>(field.Id);
                                for (int j = 0; j < test.NumberOfRecords; j++)
                                    newValues[j] = discretizeDouble.Search(oldValuesDouble[j]);
                                break;
                        }

                        localFieldInfoTest.FieldValueType = typeof(int);
                        localFieldInfoTest.Cuts = localFieldInfoTrain.Cuts;
                        test.UpdateColumn(field.Id, Array.ConvertAll(newValues, x => (object)x), localFieldInfoTrain);
                    }
                }

                args = new Args();
                args.SetParameter(ReductGeneratorParamHelper.TrainData, train);
                args.SetParameter(ReductGeneratorParamHelper.Epsilon, epsilon);
                args.SetParameter(ReductGeneratorParamHelper.FactoryKey, ReductFactoryKeyHelper.ApproximateReductMajorityWeights);
                args.SetParameter(ReductGeneratorParamHelper.PermutationCollection, permutations);

                reductGenerator = ReductFactory.GetReductGenerator(args);
                reductGenerator.Run();

                reductStoreCollection = reductGenerator.GetReductStoreCollection();
                Console.WriteLine("Average reduct length: {0}", reductStoreCollection.GetAvgMeasure(new ReductMeasureLength()));

                reductStore = reductStoreCollection.FirstOrDefault();
                foreach (IReduct reduct in reductStore)
                {
                    reduct.EquivalenceClasses.ToString2();
                }

                classifier = new RoughClassifier(
                    reductStoreCollection,
                    RuleQuality.ConfidenceW,
                    RuleQuality.ConfidenceW,
                    train.DataStoreInfo.DecisionInfo.InternalValues());

                classificationResult = classifier.Classify(test);
                Console.WriteLine("Accuracy: {0}", classificationResult.Accuracy);

                //train.WriteToCSVFileExt(String.Format("disc_german_{0}.trn", i), " ");
                //test.WriteToCSVFileExt(String.Format("disc_german_{0}.tst", i), " ");

                Console.WriteLine("+++++++++++++++++++++++++++++++++++++++++++++++++++");
            }
        }
    }
}