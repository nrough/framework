using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Infovision.Datamining;
using Infovision.Datamining.Benchmark;
using Infovision.Datamining.Filters.Unsupervised.Attribute;
using Infovision.Datamining.Roughset;
using Infovision.Utils;
using NUnit.Framework;

namespace Infovision.Data.UnitTests
{
    [TestFixture]
    class DiscretizeTest
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

            decimal epsilon = 0m;
            int numberOfPermutations = 10;
            
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
                test = DataStore.Load(benchmark.TestFile, benchmark.FileFormat);
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
                args.AddParameter(ReductGeneratorParamHelper.DataStore, train);
                args.AddParameter(ReductGeneratorParamHelper.Epsilon, epsilon);
                args.AddParameter(ReductGeneratorParamHelper.FactoryKey, ReductFactoryKeyHelper.ApproximateReductMajorityWeights);
                args.AddParameter(ReductGeneratorParamHelper.PermutationCollection, ReductFactory.GetPermutationGenerator(args).Generate(numberOfPermutations));

                IReductGenerator reductGenerator = ReductFactory.GetReductGenerator(args);
                reductGenerator.Generate();

                IReductStoreCollection reductStoreCollection = reductGenerator.GetReductStoreCollection(Int32.MaxValue);
                Console.WriteLine("Average reduct length: {0}", reductStoreCollection.GetAvgMeasure(new ReductMeasureLength()));

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
                        //Console.WriteLine("FieldId: {0}", field.Id);
                        Discretization<int> discretize = new Discretization<int>();
                        discretize.UseEntropy = benchmark.DiscretizeUsingEntropy;
                        discretize.UseEqualFrequency = benchmark.DiscretizeUsingEqualFreq;

                        int[] oldColumnValues = train.GetColumn<int>(field.Id);
                        discretize.Compute(oldColumnValues);

                        //Console.WriteLine(discretize.ToString());

                        localFieldInfoTrain = train.DataStoreInfo.GetFieldInfo(field.Id);
                        localFieldInfoTrain.IsNumeric = false;
                        localFieldInfoTrain.Cuts = discretize.Cuts;

                        int[] newColumnValues = new int[oldColumnValues.Length];
                        for (int j = 0; j < oldColumnValues.Length; j++)
                            newColumnValues[j] = discretize.Search(oldColumnValues[j]);

                        train.UpdateColumn(field.Id, Array.ConvertAll(newColumnValues, x => (object)x));
                        
                        oldColumnValues = test.GetColumn<int>(field.Id);
                        newColumnValues = new int[oldColumnValues.Length];
                        for (int j = 0; j < oldColumnValues.Length; j++)
                            newColumnValues[j] = discretize.Search(oldColumnValues[j]);

                        test.UpdateColumn(field.Id, Array.ConvertAll(newColumnValues, x => (object)x), localFieldInfoTrain);

                        localFieldInfoTest = test.DataStoreInfo.GetFieldInfo(field.Id);
                        localFieldInfoTest.IsNumeric = false;
                        localFieldInfoTest.Cuts = discretize.Cuts;

                    }
                }
                

                args = new Args();
                args.AddParameter(ReductGeneratorParamHelper.DataStore, train);
                args.AddParameter(ReductGeneratorParamHelper.Epsilon, epsilon);
                args.AddParameter(ReductGeneratorParamHelper.FactoryKey, ReductFactoryKeyHelper.ApproximateReductMajorityWeights);
                args.AddParameter(ReductGeneratorParamHelper.PermutationCollection, ReductFactory.GetPermutationGenerator(args).Generate(numberOfPermutations));

                reductGenerator = ReductFactory.GetReductGenerator(args);
                reductGenerator.Generate();

                reductStoreCollection = reductGenerator.GetReductStoreCollection(Int32.MaxValue);
                Console.WriteLine("Average reduct length: {0}", reductStoreCollection.GetAvgMeasure(new ReductMeasureLength()));                
                
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
