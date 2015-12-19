using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GenericParsing;
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
                args.AddParameter(ReductGeneratorParamHelper.DataStore, train);
                args.AddParameter(ReductGeneratorParamHelper.Epsilon, epsilon);
                args.AddParameter(ReductGeneratorParamHelper.FactoryKey, ReductFactoryKeyHelper.ApproximateReductMajorityWeights);
                PermutationCollection permutations = ReductFactory.GetPermutationGenerator(args).Generate(numberOfPermutations);
                args.AddParameter(ReductGeneratorParamHelper.PermutationCollection, permutations);

                IReductGenerator reductGenerator = ReductFactory.GetReductGenerator(args);
                reductGenerator.Generate();

                IReductStoreCollection reductStoreCollection = reductGenerator.GetReductStoreCollection(Int32.MaxValue);
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
                            case TypeCode.Decimal:
                                Discretization<decimal> discretizeDecimal = new Discretization<decimal>();
                                discretizeDecimal.UseEntropy = benchmark.DiscretizeUsingEntropy;
                                discretizeDecimal.UseEqualFrequency = benchmark.DiscretizeUsingEqualFreq;
                                decimal[] oldValuesDecimal = train.GetColumn<decimal>(field.Id);
                                discretizeDecimal.Compute(oldValuesDecimal);
                                localFieldInfoTrain.Cuts = Array.ConvertAll(discretizeDecimal.Cuts, x => (IComparable)x);
                                for (int j = 0; j < train.NumberOfRecords; j++)
                                    newValues[j] = discretizeDecimal.Search(oldValuesDecimal[j]);
                                break;

                            case TypeCode.Int32:
                                Discretization<int> discretizeInt = new Discretization<int>();
                                discretizeInt.UseEntropy = benchmark.DiscretizeUsingEntropy;
                                discretizeInt.UseEqualFrequency = benchmark.DiscretizeUsingEqualFreq;
                                int[] oldValuesInt = train.GetColumn<int>(field.Id);
                                discretizeInt.Compute(oldValuesInt);
                                localFieldInfoTrain.Cuts = Array.ConvertAll(discretizeInt.Cuts, x => (IComparable)x);
                                for (int j = 0; j < train.NumberOfRecords; j++)
                                    newValues[j] = discretizeInt.Search(oldValuesInt[j]);
                                break;

                            case TypeCode.Double:
                                Discretization<double> discretizeDouble = new Discretization<double>();
                                discretizeDouble.UseEntropy = benchmark.DiscretizeUsingEntropy;
                                discretizeDouble.UseEqualFrequency = benchmark.DiscretizeUsingEqualFreq;
                                double[] oldValuesDouble = train.GetColumn<double>(field.Id);
                                discretizeDouble.Compute(oldValuesDouble);
                                localFieldInfoTrain.Cuts = Array.ConvertAll(discretizeDouble.Cuts, x => (IComparable)x);
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
                                discretizeInt.Cuts = Array.ConvertAll(localFieldInfoTrain.Cuts, x => (int) x);
                                int[] oldValuesInt = test.GetColumn<int>(field.Id);
                                for (int j = 0; j < test.NumberOfRecords; j++)
                                    newValues[j] = discretizeInt.Search(oldValuesInt[j]);
                                break;

                            case TypeCode.Decimal:
                                Discretization<decimal> discretizeDecimal = new Discretization<decimal>();
                                discretizeDecimal.Cuts = Array.ConvertAll(localFieldInfoTrain.Cuts, x => (decimal)x);
                                decimal[] oldValuesDecimal = test.GetColumn<decimal>(field.Id);
                                for (int j = 0; j < test.NumberOfRecords; j++)
                                    newValues[j] = discretizeDecimal.Search(oldValuesDecimal[j]);
                                break;

                            case TypeCode.Double:
                                Discretization<double> discretizeDouble = new Discretization<double>();
                                discretizeDouble.Cuts = Array.ConvertAll(localFieldInfoTrain.Cuts, x => (double)x);
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
                args.AddParameter(ReductGeneratorParamHelper.DataStore, train);
                args.AddParameter(ReductGeneratorParamHelper.Epsilon, epsilon);
                args.AddParameter(ReductGeneratorParamHelper.FactoryKey, ReductFactoryKeyHelper.ApproximateReductMajorityWeights);
                args.AddParameter(ReductGeneratorParamHelper.PermutationCollection, permutations);

                reductGenerator = ReductFactory.GetReductGenerator(args);
                reductGenerator.Generate();

                reductStoreCollection = reductGenerator.GetReductStoreCollection(Int32.MaxValue);
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

        [Test]
        public void DisesorDataStoreLoadTest()
        {
            int nFold = 4;
            int numberOfPermutations = 1;
            string trainfile = @"c:\data\disesor\trainingData.csv";
            string labelfile = @"c:\data\disesor\trainingLabels.csv";

            string trainfile_merge = @"c:\data\disesor\trainingData_merge.csv";            
            //string testfile = @"c:\data\disesor\testData.csv";
            //string columnfile = @"c:\data\disesor\testData.csv";          
            
            Dictionary<string, string> metadataDict = new Dictionary<string, string>();

            metadataDict.Add("146", "146,ściana 5,Partia F,416,ZZ,2,a");
            metadataDict.Add("149", "149,ściana 5,Partia F,418,ZZ,2.2,b");
            metadataDict.Add("155", "155,ściana 3,Partia H,502,ZZ,2.7,b");
            metadataDict.Add("171", "171,ściana 1,Partia F,409,ZZ,2,a");
            metadataDict.Add("264", "264,sc. i100,Z,405/2,ZZ,3.5,b");
            metadataDict.Add("373", "373,1_Ściana M-12,G-1,707/2,ZZ,1.6,b");
            metadataDict.Add("437", "437,1_Ściana M-5,G-1,712/1-2,ZZ,3,b");
            metadataDict.Add("470", "470,sc. i101,Z,405/2,ZZ,3.8,c");
            metadataDict.Add("479", "479,2_Ściana W-4,G - 2,505,ZZ,4,a");
            metadataDict.Add("490", "490,śc.h51,B,405/1,ZZ,1.9,a");
            metadataDict.Add("508", "508,śc.i61,B,405/2,ZZ,2.8,a");
            metadataDict.Add("541", "541,KG1 Sc_521,Dz,510,ZZ,4.4,b");
            metadataDict.Add("575", "575,1_Ściana M-4,G-1,712/1-2,ZZ,3,b");
            metadataDict.Add("583", "583,KG1 Sc_550,Dw,510,ZZ,4,b");
            metadataDict.Add("599", "599,3_Ściana C-2a,G-3,505,ZZ,3,a");
            metadataDict.Add("607", "607,KG2 Sc_510,Az,501,ZZ,3.8,b");
            metadataDict.Add("641", "641,2_Ściana C-3,G-2,503-504,ZZ,3.8,a");
            metadataDict.Add("689", "689,KG2 Sc_560,Dw,510,ZZ,3,b");
            metadataDict.Add("703", "703,1_Ściana M-3,G-1,712/1-2,ZZ,3,a");
            metadataDict.Add("725", "725,Ściana 2,12,506,ZZ,2.2,b");
            metadataDict.Add("765", "765,Ściana 713,13,401,ZZ,1.4,a");
            metadataDict.Add("777", "777,Ściana 003,9,504,ZZ,3.4,b");
            metadataDict.Add("793", "793,Ściana 839a,0,405,ZZ,3.4,b");
            metadataDict.Add("799", "799,Ściana 026,9,504,ZZ,3.2,a");

            foreach (var id in metadataDict.Keys.ToArray())
            {
                metadataDict[id] = metadataDict[id].Replace(' ', '_');
            }

            DataTable rawData;
            using (GenericParserAdapter gpa = new GenericParserAdapter(trainfile))
            {
                gpa.ColumnDelimiter = ",".ToCharArray()[0];
                gpa.FirstRowHasHeader = false;
                gpa.IncludeFileLineNumber = false;

                rawData = gpa.GetDataTable();
            }

            foreach (DataRow row in rawData.Rows)
            {
                string oldValue = row.Field<string>(0);
                string newValue = metadataDict[oldValue];
                row.SetField(0, newValue);
            }

            rawData.WriteToCSVFile(trainfile_merge, ",");

            DataStore data = DataStore.Load(trainfile_merge, FileFormat.Csv);
            DataStore labels = DataStore.Load(labelfile, FileFormat.Csv);
            int decisionFieldId = data.AddColumn<string>(labels.GetColumn<string>(1));
            labels = null;
            data.SetDecisionFieldId(decisionFieldId);

            long[] decisionValues = data.DataStoreInfo.GetDecisionValues().ToArray();

            DataStore train = null, test = null;
            DataStoreSplitter splitter = new DataStoreSplitter(data, nFold);

            for (int n = 0; n < nFold; n++)
            {
                splitter.ActiveFold = n;
                splitter.Split(ref train, ref test);

                new DataStoreDiscretizer().Discretize(ref train, ref test);

                train.WriteToCSVFileExt(String.Format("c:\\data\\disesor\\disesor-{0}.trn", n), ",");
                test.WriteToCSVFileExt(String.Format("c:\\data\\disesor\\disesor-{0}.tst", n), ",");
            }

            train = null; test = null; data = null;

            for (int n = 0; n < nFold; n++)
            {
                train = DataStore.Load(String.Format("c:\\data\\disesor\\disesor-{0}.trn", n), FileFormat.Csv);
                test = DataStore.Load(String.Format("c:\\data\\disesor\\disesor-{0}.tst", n), FileFormat.Csv);

                Args args = new Args();
                args.AddParameter(ReductGeneratorParamHelper.DataStore, train);
                args.AddParameter(ReductGeneratorParamHelper.FactoryKey, ReductFactoryKeyHelper.ApproximateReductRelative);
                args.AddParameter(ReductGeneratorParamHelper.Epsilon, 0.1m);
                args.AddParameter(ReductGeneratorParamHelper.PermutationCollection, ReductFactory.GetPermutationGenerator(args).Generate(numberOfPermutations));

                IReductGenerator generator = ReductFactory.GetReductGenerator(args);
                generator.Generate();

                IReductStoreCollection reductStoreCollection = generator.GetReductStoreCollection(Int32.MaxValue);

                RoughClassifier classifier = new RoughClassifier(
                    reductStoreCollection, 
                    RuleQuality.Coverage, 
                    RuleQuality.Coverage,
                    decisionValues);

                ClassificationResult result = classifier.Classify(test);

                Console.WriteLine(result.Accuracy);
            }
        }
    }
}
