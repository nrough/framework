﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GenericParsing;
using Infovision.Data;
using Infovision.Datamining;
using Infovision.Datamining.Filters.Unsupervised.Attribute;
using Infovision.Datamining.Roughset;
using Infovision.Utils;

namespace DisesorTest
{
    public class Program
    {
        #region Member variables
        static string trainfile = @"c:\data\disesor\trainingData.csv";
        static string trainfile_merge = @"c:\data\disesor\trainingData_merge.csv";
        static string testfile = @"c:\data\disesor\testData.csv";
        static string testfile_merge = @"c:\data\disesor\testData_merge.csv";
        static string labelfile = @"c:\data\disesor\trainingLabels.csv";
        static string outputfile = @"c:\data\disesor\result.csv";
        private Dictionary<string, string> metadataDict;
        #endregion

        public static void Main(string[] args)
        {
            Program p = new Program();
            
            //p.DisesorDataStoreLoadTest();
                                                
            string factoryKey = ReductFactoryKeyHelper.GeneralizedMajorityDecisionApproximate;            
            int numberOfPermutations = 200;
            decimal epsilon = 0.5m;
            RuleQualityFunction identificationFunction = RuleQuality.CoverageW;
            RuleQualityFunction voteFunction = RuleQuality.CoverageW;
            WeightGeneratorType weightGeneratorType = WeightGeneratorType.Relative;
            DiscretizationType discretizationType = DiscretizationType.Entropy;

            //TODO Majority weighting most of objects are unclassified, empty reducts
            //TODO Boosting && CV
            //TODO Tunning            

            p.FinalTest(
                factoryKey, 
                numberOfPermutations, 
                epsilon, 
                identificationFunction, 
                voteFunction, 
                weightGeneratorType, 
                discretizationType);
            
            Console.Beep();
            Console.ReadKey();
        }

        private void FinalTest(
            string factoryKey, 
            int numberOfPermutations, 
            decimal epsilon, 
            RuleQualityFunction identificationFunction, 
            RuleQualityFunction voteFunction, 
            WeightGeneratorType weightGeneratorType, 
            DiscretizationType discretizationType)
        {
            Console.WriteLine("Algorithm: {0}", factoryKey);
            Console.WriteLine("Number of permutations: {0}", numberOfPermutations);
            Console.WriteLine("Epsilon: {0}", epsilon);
            Console.WriteLine("Decision identification: {0}", identificationFunction.Method.Name);
            Console.WriteLine("Voting method: {0}", voteFunction.Method.Name);
            Console.WriteLine("Weighting generator: {0}", weightGeneratorType);
            Console.WriteLine("Discretization method: {0}", discretizationType);
            Console.WriteLine();
            
            this.LoadMetadata();

            Console.Write("Loading raw train data...");
            DataTable rawTrainData;
            using (GenericParserAdapter gpa = new GenericParserAdapter(trainfile))
            {
                gpa.ColumnDelimiter = ",".ToCharArray()[0];
                gpa.FirstRowHasHeader = false;
                gpa.IncludeFileLineNumber = false;

                rawTrainData = gpa.GetDataTable();
            }
            Console.WriteLine("Done");

            Console.Write("Updating raw train data...");
            foreach (DataRow row in rawTrainData.Rows)
            {
                string oldValue = row.Field<string>(0);
                string newValue = metadataDict[oldValue];
                row.SetField(0, newValue);
            }
            Console.WriteLine("Done");

            Console.Write("Saving merged raw train data...");
            rawTrainData.WriteToCSVFile(trainfile_merge, ",");
            rawTrainData = null;
            Console.WriteLine("Done");

            Console.Write("Loading raw test data...");
            DataTable rawTestData;
            using (GenericParserAdapter gpa = new GenericParserAdapter(testfile))
            {
                gpa.ColumnDelimiter = ",".ToCharArray()[0];
                gpa.FirstRowHasHeader = false;
                gpa.IncludeFileLineNumber = false;

                rawTestData = gpa.GetDataTable();
            }
            Console.WriteLine("Done");

            Console.Write("Updating raw test data...");
            foreach (DataRow row in rawTestData.Rows)
            {
                string oldValue = row.Field<string>(0);
                string newValue = metadataDict[oldValue];
                row.SetField(0, newValue);
            }
            Console.WriteLine("Done");

            Console.Write("Saving merged raw test data...");
            rawTestData.WriteToCSVFile(testfile_merge, ",");
            rawTestData = null;
            Console.WriteLine("Done");

            Console.Write("Loading training data store...");
            DataStore train = DataStore.Load(trainfile_merge, FileFormat.Csv);
            Console.WriteLine("Done");
            
            Console.Write("Loading test data...");
            DataStore test = DataStore.Load(testfile_merge, FileFormat.Csv, train.DataStoreInfo);
            test.SetDecisionFieldId(-1);
            Console.WriteLine("Done");

            Console.Write("Loading labels...");
            DataStore labels = DataStore.Load(labelfile, FileFormat.Csv);
            int decisionFieldId = train.AddColumn<string>(labels.GetColumn<string>(1));
            labels = null;
            train.SetDecisionFieldId(decisionFieldId);
            long[] decisionValues = train.DataStoreInfo.GetDecisionValues().ToArray();
            long warningLabel = train.DataStoreInfo.DecisionInfo.External2Internal("warning");
            long normalLabel = train.DataStoreInfo.DecisionInfo.External2Internal("normal");
            Console.WriteLine("Done");

            Console.Write("Discretizing data...");
            DataStoreDiscretizer.Construct(discretizationType).Discretize(ref train, ref test);
            Console.WriteLine("Done");

            Args args = new Args();
            args.AddParameter(ReductGeneratorParamHelper.DataStore, train);
            args.AddParameter(ReductGeneratorParamHelper.FactoryKey, factoryKey);
            args.AddParameter(ReductGeneratorParamHelper.Epsilon, epsilon);            
            args.AddParameter(ReductGeneratorParamHelper.PermutationCollection, ReductFactory.GetPermutationGenerator(args).Generate(numberOfPermutations));
            args.AddParameter(ReductGeneratorParamHelper.WeightGenerator, WeightGenerator.Construct(weightGeneratorType, train));

            Console.Write("Reduct generation...");
            IReductGenerator generator = ReductFactory.GetReductGenerator(args);
            generator.Generate();
            IReductStoreCollection reductStoreCollection = generator.GetReductStoreCollection(Int32.MaxValue);
            Console.WriteLine("Done");

            foreach (IReductStore reductStore in reductStoreCollection)
            {
                foreach (IReduct reduct in reductStore)
                {
                    Console.WriteLine(reduct);
                }
            }

            Console.Write("Classification...");            
            RoughClassifier classifier = new RoughClassifier(
                reductStoreCollection,
                identificationFunction,
                voteFunction,
                decisionValues);

            StreamWriter file = null;            
            int unclassified = 0;
            try
            {
                file = new System.IO.StreamWriter(outputfile);
                for (int i = 0; i < test.NumberOfRecords; i++)
                {
                    DataRecordInternal record = test.GetRecordByIndex(i);
                    var prediction = classifier.Classify(record);

                    decimal sum = Decimal.Zero;
                    foreach (var kvp in prediction)
                    {
                        if (kvp.Key != -1)
                            sum += kvp.Value;                        
                    }

                    if(prediction.Count == 0 || (prediction.Count == 1 && prediction.ContainsKey(-1)))
                        unclassified++;

                    decimal warning = prediction.ContainsKey(warningLabel) ? prediction[warningLabel] : Decimal.Zero;
                    decimal result = sum > 0 ? warning / sum : Decimal.Zero;
                    file.WriteLine(result.ToString(CultureInfo.InvariantCulture));
                }
            }
            catch
            {
                throw;
            }
            finally
            {
                if (file != null)
                {
                    file.Close();
                }
            }
            Console.WriteLine("Done");
            Console.WriteLine("Unclassified: {0}", unclassified);       
        }

        private void DisesorDataStoreLoadTest()
        {
            int nFold = 2;
            int numberOfPermutations = 10;
            string factoryKey = ReductFactoryKeyHelper.GeneralizedMajorityDecisionApproximate;
            decimal epsilon = 0.2m;
            RuleQualityFunction identificationFunction = RuleQuality.Coverage;
            RuleQualityFunction voteFunction = RuleQuality.Coverage;
            int attributeReductionStep = 5;

            this.LoadMetadata();            

            Console.Write("Loading raw data...");            
            DataTable rawData;
            using (GenericParserAdapter gpa = new GenericParserAdapter(trainfile))
            {
                gpa.ColumnDelimiter = ",".ToCharArray()[0];
                gpa.FirstRowHasHeader = false;
                gpa.IncludeFileLineNumber = false;

                rawData = gpa.GetDataTable();
            }
            Console.WriteLine("Done");

            Console.Write("Updating raw data...");
            foreach (DataRow row in rawData.Rows)
            {
                string oldValue = row.Field<string>(0);
                string newValue = metadataDict[oldValue];
                row.SetField(0, newValue);
            }
            Console.WriteLine("Done");

            Console.Write("Saving merged data...");
            rawData.WriteToCSVFile(trainfile_merge, ",");            
            Console.WriteLine("Done");

            Console.Write("Loading data store...");            
            DataStore data = DataStore.Load(trainfile_merge, FileFormat.Csv);
            Console.WriteLine("Done");

            Console.Write("Loading labels...");
            DataStore labels = DataStore.Load(labelfile, FileFormat.Csv);            
            int decisionFieldId = data.AddColumn<string>(labels.GetColumn<string>(1));
            labels = null;
            data.SetDecisionFieldId(decisionFieldId);
            long[] decisionValues = data.DataStoreInfo.GetDecisionValues().ToArray();
            Console.WriteLine("Done");

            Console.WriteLine("Generating cross validation data sets");            
            DataStore train = null, test = null;
            DataStoreSplitter splitter = new DataStoreSplitter(data, nFold);

            for (int n = 0; n < nFold; n++)
            {
                Console.Write("Split {0}/{1}...", n, nFold - 1);
                splitter.ActiveFold = n;
                splitter.Split(ref train, ref test);
                Console.WriteLine("Done");

                Console.Write("Discretizing split {0}/{1}...", n, nFold - 1);
                new DataStoreDiscretizer()
                {
                    DiscretizeUsingEntropy = true,
                    DiscretizeUsingEqualWidth = false,
                    DiscretizeUsingEqualFreq = false,

                }.Discretize(ref train, ref test);

                Console.WriteLine("Done");

                Console.Write("Saving train split {0}/{1}...", n, nFold - 1);                
                train.WriteToCSVFileExt(String.Format("c:\\data\\disesor\\disesor-{0}.trn", n), ",");
                Console.WriteLine("Done");

                Console.Write("Saving test split {0}/{1}...", n, nFold - 1);
                test.WriteToCSVFileExt(String.Format("c:\\data\\disesor\\disesor-{0}.tst", n), ",");
                Console.WriteLine("Done");
            }


            Console.WriteLine("Testing");
            
            train = null; test = null; data = null;

            for (int n = 0; n < nFold; n++)
            {
                Console.Write("Loading train data {0}/{1}...", n, nFold - 1);
                train = DataStore.Load(String.Format("c:\\data\\disesor\\disesor-{0}.trn", n), FileFormat.Csv);
                Console.WriteLine("Done");

                Console.Write("Loading test data {0}/{1}...", n, nFold - 1);                
                test = DataStore.Load(String.Format("c:\\data\\disesor\\disesor-{0}.tst", n), FileFormat.Csv);
                Console.WriteLine("Done");

                WeightGeneratorRelative weightGenerator = new WeightGeneratorRelative(train);

                Args args = new Args();
                args.AddParameter(ReductGeneratorParamHelper.DataStore, train);
                args.AddParameter(ReductGeneratorParamHelper.FactoryKey, factoryKey);
                args.AddParameter(ReductGeneratorParamHelper.Epsilon, epsilon);
                args.AddParameter(ReductGeneratorParamHelper.AttributeReductionStep, attributeReductionStep); //TODO
                args.AddParameter(ReductGeneratorParamHelper.PermutationCollection, ReductFactory.GetPermutationGenerator(args).Generate(numberOfPermutations));
                args.AddParameter(ReductGeneratorParamHelper.WeightGenerator, weightGenerator);

                Console.Write("Reduct generation {0}/{1}...", n, nFold - 1);                
                IReductGenerator generator = ReductFactory.GetReductGenerator(args);
                generator.Generate();
                IReductStoreCollection reductStoreCollection = generator.GetReductStoreCollection(Int32.MaxValue);
                Console.WriteLine("Done");

                foreach (IReductStore reductStore in reductStoreCollection)
                {
                    foreach(IReduct reduct in reductStore)
                    {
                        Console.WriteLine(reduct);
                    }
                }

                Console.Write("Classification {0}/{1}...", n, nFold - 1);
                RoughClassifier classifier = new RoughClassifier(
                    reductStoreCollection,
                    identificationFunction,
                    voteFunction,                    
                    decisionValues);                
                ClassificationResult result = classifier.Classify(test);
                Console.WriteLine("Done");

                Console.WriteLine("Accuracy: {0}", result.Accuracy);
                Console.WriteLine("Coverage: {0}", result.Coverage);
                Console.WriteLine("Confidence: {0}", result.Confidence);

                foreach (long actual in decisionValues)                
                {
                    foreach (long predicted in decisionValues)
                    {
                        Console.WriteLine("Predicted {0} Actual {1} Count {2}", predicted, actual, result.GetConfusionMatrix(predicted, actual));
                    }

                    Console.WriteLine("Predicted {0} Actual {1} Count {2}", -1, actual, result.GetConfusionMatrix(-1, actual));
                }
                
            }
        }

        private void LoadMetadata()
        {
            Console.Write("Loading metadata...");
            metadataDict = new Dictionary<string, string>();
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
            Console.WriteLine("Done");
        }
    }
}