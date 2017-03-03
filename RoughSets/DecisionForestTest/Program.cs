using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using NRough.Data;
using NRough.MachineLearning.Roughsets;
using NRough.MachineLearning.Classification.DecisionTrees;
using NRough.MachineLearning.Permutations;
using NRough.MachineLearning.Classification;
using GenericParsing;
using NRough.Data.Benchmark;

namespace DecisionForestTest
{
    internal class Program
    {
        private DataStore trainData, testData, data;
        private StreamWriter fileStream;
        int[] sizes = new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 20, 30, 40, 50, 60, 70, 80, 90, 100 };

        private static void Main(string[] args)
        {
            string[] names = args;

            foreach (var kvp in BenchmarkDataHelper.GetDataFiles(names: names))
            {
                Program program = new Program();
                program.Run(kvp.Value);

                //program.LoadResults();

                GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
                GC.WaitForPendingFinalizers();
            }
        }

        private void Classify<T>(DecisionForestBase<T> forest, DataStore testData, string name, int test, int fold)
            where T : IDecisionTree, new()
        {
            int origSize = forest.Size;
            foreach (int size in sizes)
            {
                forest.Size = size;
                ClassificationResult result = Classifier.Default.Classify(forest, testData, null);

                result.ModelCreationTime = forest.LearningResult.ModelCreationTime;

                result.ModelName = name;
                result.TestNum = test;
                result.Fold = fold;
                
                this.WriteLine(result);
            }
            forest.Size = origSize;
        }

        public void Run(BenchmarkData benchmarkData)
        {
            this.OpenStream(Path.Combine(@"results", benchmarkData.Name + ".result"));

            int maxTest = 20;
            int size = 40;
            int numberOfTreeProbes = 10;            

            for (int t = 0; t < maxTest; t++)
            {
                int[] attributes = null;
                DataSplitter splitter = null;
                if (benchmarkData.CrossValidationActive)
                {
                    data = DataStore.Load(benchmarkData.DataFile, benchmarkData.FileFormat);
                    if (benchmarkData.DecisionFieldId > 0)
                        data.SetDecisionFieldId(benchmarkData.DecisionFieldId);
                    splitter = new DataSplitter(data, benchmarkData.CrossValidationFolds);
                    attributes = data.DataStoreInfo.SelectAttributeIds(a => a.IsStandard).ToArray();
                }
                else
                {
                    trainData = DataStore.Load(benchmarkData.TrainFile, benchmarkData.FileFormat);
                    if (benchmarkData.DecisionFieldId > 0)
                        trainData.SetDecisionFieldId(benchmarkData.DecisionFieldId);
                    testData = DataStore.Load(benchmarkData.TestFile, benchmarkData.FileFormat, trainData.DataStoreInfo);
                    attributes = trainData.DataStoreInfo.SelectAttributeIds(a => a.IsStandard).ToArray();
                }
                int numberOfAttributesToCheckForSplit = (int)((double)attributes.Length * 0.33);

                for (int fold = 0; fold < benchmarkData.CrossValidationFolds; fold++)
                {
                    if (splitter != null)                        
                        splitter.Split(out trainData, out testData, fold);

                    DataSampler sampler = new DataSampler(trainData, true);
                    sampler.BagSizePercent = 100;
                    PermutationCollection permutations = new PermutationCollection(size, attributes);
                    
                    for (double eps = 0; eps < 1.0; eps += 0.01)
                    {
                        DecisionForestDummy<DecisionTreeC45> dummyForest = new DecisionForestDummy<DecisionTreeC45>();
                        dummyForest.DataSampler = sampler;
                        dummyForest.Size = size;
                        dummyForest.Gamma = eps;
                        dummyForest.NumberOfTreeProbes = numberOfTreeProbes;
                        dummyForest.Learn(trainData, attributes);
                        this.Classify<DecisionTreeC45>(dummyForest, testData, "DummyC45", t, fold);

                        DecisionForestReduct<DecisionTreeC45> reductForestC45 = new DecisionForestReduct<DecisionTreeC45>();
                        reductForestC45.DataSampler = sampler;
                        reductForestC45.Size = size;
                        reductForestC45.Gamma = eps;
                        reductForestC45.NumberOfTreeProbes = numberOfTreeProbes;
                        reductForestC45.ReductGeneratorFactory = ReductTypes.ApproximateReductMajorityWeights;
                        reductForestC45.Learn(trainData, attributes);
                        this.Classify<DecisionTreeC45>(reductForestC45, testData, "ReductC45", t, fold);

                        DecisionForestReduct<DecisionTreeC45> roughForestGamma = new DecisionForestReduct<DecisionTreeC45>();
                        roughForestGamma.DataSampler = sampler;
                        roughForestGamma.Size = size;
                        roughForestGamma.NumberOfTreeProbes = numberOfTreeProbes;
                        roughForestGamma.Gamma = eps;
                        roughForestGamma.ReductGeneratorFactory = ReductTypes.GeneralizedMajorityDecisionApproximate;
                        roughForestGamma.Learn(trainData, attributes);
                        this.Classify<DecisionTreeC45>(roughForestGamma, testData, "RoughGammaC45", t, fold);

                        DecisionForestReduct<DecisionTreeRough> roughForestM = new DecisionForestReduct<DecisionTreeRough>();
                        roughForestM.DataSampler = sampler;
                        roughForestM.Size = size;
                        roughForestM.Gamma = eps;
                        roughForestM.NumberOfTreeProbes = numberOfTreeProbes;
                        roughForestM.ReductGeneratorFactory = ReductTypes.ApproximateReductMajorityWeights;
                        roughForestM.Learn(trainData, attributes);
                        this.Classify<DecisionTreeRough>(roughForestM, testData, "ReductRoughM", t, fold);

                        DecisionForestDummyRough<DecisionTreeC45> semiRoughForest = new DecisionForestDummyRough<DecisionTreeC45>();
                        semiRoughForest.DataSampler = sampler;
                        semiRoughForest.Size = size;
                        semiRoughForest.Gamma = eps;
                        semiRoughForest.NumberOfTreeProbes = numberOfTreeProbes;
                        semiRoughForest.ReductGeneratorFactory = ReductTypes.ApproximateReductMajorityWeights;
                        semiRoughForest.Learn(trainData, attributes);
                        this.Classify<DecisionTreeC45>(semiRoughForest, testData, "SemiRoughC45", t, fold);

                        DecisionForestDummyRough<DecisionTreeC45> semiRoughGammaForest = new DecisionForestDummyRough<DecisionTreeC45>();
                        semiRoughGammaForest.DataSampler = sampler;
                        semiRoughGammaForest.Size = size;
                        semiRoughGammaForest.Gamma = eps;
                        semiRoughGammaForest.NumberOfTreeProbes = numberOfTreeProbes;
                        semiRoughGammaForest.ReductGeneratorFactory = ReductTypes.GeneralizedMajorityDecisionApproximate;
                        semiRoughGammaForest.Learn(trainData, attributes);
                        this.Classify<DecisionTreeC45>(semiRoughGammaForest, testData, "SemiRoughGammaC45", t, fold);

                        DecisionForestRandom<DecisionTreeC45> randomForest = new DecisionForestRandom<DecisionTreeC45>();
                        randomForest.DataSampler = sampler;
                        randomForest.Size = size;
                        randomForest.Gamma = eps;
                        randomForest.NumberOfTreeProbes = numberOfTreeProbes;
                        randomForest.NumberOfAttributesToCheckForSplit = numberOfAttributesToCheckForSplit;
                        randomForest.Learn(trainData, attributes);
                        this.Classify<DecisionTreeC45>(randomForest, testData, "RandomC45", t, fold);

                        Console.WriteLine();
                    }
                }
            }

            this.CloseStream();
        }        

        public void OpenStream(string path)
        {
            fileStream = new StreamWriter(path, false);
            this.WriteLine(ClassificationResult.TableHeader());
        }

        public void CloseStream()
        {
            if (fileStream != null)
            {
                fileStream.Flush();
                fileStream.Close();
                fileStream = null;
            }
        }

        public void WriteLine(string format, params object[] paramteters)
        {
            if (fileStream != null)
            {
                fileStream.WriteLine(format, paramteters);
                fileStream.Flush();
            }

            Console.WriteLine(format, paramteters);
        }

        public void WriteLine(object parm)
        {
            if (fileStream != null)
            {
                fileStream.WriteLine(parm.ToString());
                fileStream.Flush();
            }

            Console.WriteLine(parm.ToString());
        }

        public void WriteLine()
        {
            if (fileStream != null)
            {
                fileStream.WriteLine();
                fileStream.Flush();
            }

            Console.WriteLine();
        }


        #region SQL

        private void LoadResults()
        {
            this.InsertDB(this.GetTableResult_MRIExceptionsTest(@"C:\Users\Sebastian\Source\Workspaces\RoughSets\RoughSets\DecisionForestTest\bin\x64\Release\results\breast.result"));
            this.InsertDB(this.GetTableResult_MRIExceptionsTest(@"C:\Users\Sebastian\Source\Workspaces\RoughSets\RoughSets\DecisionForestTest\bin\x64\Release\results\dna.result"));
            this.InsertDB(this.GetTableResult_MRIExceptionsTest(@"C:\Users\Sebastian\Source\Workspaces\RoughSets\RoughSets\DecisionForestTest\bin\x64\Release\results\monks-1.result"));
            this.InsertDB(this.GetTableResult_MRIExceptionsTest(@"C:\Users\Sebastian\Source\Workspaces\RoughSets\RoughSets\DecisionForestTest\bin\x64\Release\results\monks-2.result"));
            this.InsertDB(this.GetTableResult_MRIExceptionsTest(@"C:\Users\Sebastian\Source\Workspaces\RoughSets\RoughSets\DecisionForestTest\bin\x64\Release\results\monks-3.result"));
            this.InsertDB(this.GetTableResult_MRIExceptionsTest(@"C:\Users\Sebastian\Source\Workspaces\RoughSets\RoughSets\DecisionForestTest\bin\x64\Release\results\promoters.result"));
            this.InsertDB(this.GetTableResult_MRIExceptionsTest(@"C:\Users\Sebastian\Source\Workspaces\RoughSets\RoughSets\DecisionForestTest\bin\x64\Release\results\soybean-small.result"));
            this.InsertDB(this.GetTableResult_MRIExceptionsTest(@"C:\Users\Sebastian\Source\Workspaces\RoughSets\RoughSets\DecisionForestTest\bin\x64\Release\results\spect.result"));
            this.InsertDB(this.GetTableResult_MRIExceptionsTest(@"C:\Users\Sebastian\Source\Workspaces\RoughSets\RoughSets\DecisionForestTest\bin\x64\Release\results\zoo.result"));
        }

        private void InsertDB(DataTable table)
        {
            string connectionString = @"Server=HUJOLOPTER\SQL2014;Database=RoughDB;Integrated Security=True;";
            using (SqlConnection dbConnection = new SqlConnection(connectionString))
            {
                dbConnection.Open();
                using (SqlBulkCopy s = new SqlBulkCopy(dbConnection))
                {
                    s.DestinationTableName = table.TableName;
                    s.BulkCopyTimeout = 300; //5 minutes

                    foreach (var column in table.Columns)
                        s.ColumnMappings.Add(column.ToString(), column.ToString());

                    s.WriteToServer(table);
                }
            }
        }

        private DataTable DefineResultTable()
        {
            DataTable table = new DataTable("dbo.RESULTDECISIONFOREST");

            
            this.AddColumn(table, "DATASET", "System.String");
            this.AddColumn(table, "MODELTYPE", "System.String");
            this.AddColumn(table, "FOLDID", "System.Int32");
            this.AddColumn(table, "TESTID", "System.Int32");
            this.AddColumn(table, "ENSEMBLESIZE", "System.Int32");
            this.AddColumn(table, "EPSILON", "System.Double");            
            this.AddColumn(table, "ACCURACY", "System.Double");
            this.AddColumn(table, "BALANCEDACCURACY", "System.Double");
            this.AddColumn(table, "CONFIDENCE", "System.Double");
            this.AddColumn(table, "COVERAGE", "System.Double");
            this.AddColumn(table, "AVGREDUCTLENGTH", "System.Double");            

            return table;
        }

        private DataColumn AddColumn(DataTable table, string name, string type)
        {
            DataColumn c = new DataColumn();
            c.DataType = Type.GetType(type);
            c.ColumnName = name;
            table.Columns.Add(c);
            return c;
        }

        private DataTable GetTableResult_MRIExceptionsTest(string filename)
        {
            DataTable table = this.DefineResultTable();

            DataTable tmpTable;
            using (GenericParserAdapter gpa = new GenericParserAdapter(filename))
            {
                gpa.ColumnDelimiter = "|".ToCharArray()[0];
                gpa.FirstRowHasHeader = true;
                gpa.IncludeFileLineNumber = false;
                gpa.TrimResults = true;
                gpa.SkipEmptyRows = true;

                tmpTable = gpa.GetDataTable();
            }

            DataRow dataSetRow = null;
            foreach (DataRow row in tmpTable.Rows)
            {
                dataSetRow = table.NewRow();

                if (row["ModelName"].ToString() == "Dummy"
                    || row["ModelName"].ToString() == "RoughVarEps"
                    || row["ModelName"].ToString() == "RoughForestM")
                                
                dataSetRow["DATASET"] = row["Data"].ToString();
                dataSetRow["FOLDID"] = Int32.Parse(row["Fold"].ToString());
                dataSetRow["TESTID"] = Int32.Parse(row["TestNum"].ToString());
                dataSetRow["ENSEMBLESIZE"] = Int32.Parse(row["EnsembleSize"].ToString());
                dataSetRow["MODELTYPE"] = row["ModelName"].ToString();
                dataSetRow["EPSILON"] = Double.Parse(row["Epsilon"].ToString());                
                dataSetRow["ACCURACY"] = Double.Parse(row["Accuracy"].ToString());
                dataSetRow["BALANCEDACCURACY"] = Double.Parse(row["BalancedAccuracy"].ToString());
                dataSetRow["CONFIDENCE"] = Double.Parse(row["Confidence"].ToString());
                dataSetRow["COVERAGE"] = Double.Parse(row["Coverage"].ToString());
                dataSetRow["AVGREDUCTLENGTH"] = Double.Parse(row["AverageReductLength"].ToString());

                table.Rows.Add(dataSetRow);
            }

            return table;
        }

        #endregion
    }
}
