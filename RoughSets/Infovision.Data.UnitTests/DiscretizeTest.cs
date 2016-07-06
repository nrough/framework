using System;
using System.Collections.Generic;
using System.Linq;
using Infovision.Datamining;
using Infovision.Datamining.Benchmark;
using Infovision.Datamining.Filters.Unsupervised.Attribute;
using Infovision.Datamining.Roughset;
using Infovision.Utils;
using NUnit.Framework;

namespace Infovision.Data.UnitTests
{
    [TestFixture]
    internal class DiscretizeTest
    {
        public IEnumerable<KeyValuePair<string, BenchmarkData>> GetDataFiles()
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
            decimal epsilon = 0.05m;
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
                {
                    splitter.ActiveFold = i;
                    splitter.Split(ref train, ref test);
                }

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
                            /*
                            case TypeCode.Decimal:
                                Discretization<decimal> discretizeDecimal = new Discretization<decimal>();
                                discretizeDecimal.UseEntropy = benchmark.DiscretizeUsingEntropy;
                                discretizeDecimal.UseEqualFrequency = benchmark.DiscretizeUsingEqualFreq;
                                decimal[] oldValuesDecimal = train.GetColumn<decimal>(field.Id);
                                discretizeDecimal.Compute(oldValuesDecimal);
                                localFieldInfoTrain.Cuts = discretizeDecimal.Cuts;
                                for (int j = 0; j < train.NumberOfRecords; j++)
                                    newValues[j] = discretizeDecimal.Search(oldValuesDecimal[j]);
                                break;
                            */

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

                            /*
                            case TypeCode.Decimal:
                                Discretization<decimal> discretizeDecimal = new Discretization<decimal>();
                                discretizeDecimal.Cuts = localFieldInfoTrain.Cuts;
                                decimal[] oldValuesDecimal = test.GetColumn<decimal>(field.Id);
                                for (int j = 0; j < test.NumberOfRecords; j++)
                                    newValues[j] = discretizeDecimal.Search(oldValuesDecimal[j]);
                                break;
                            */

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