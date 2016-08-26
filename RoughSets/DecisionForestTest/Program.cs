using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GenericParsing;
using Infovision.Data;
using Infovision.Datamining;
using Infovision.Datamining.Benchmark;
using Infovision.Datamining.Roughset;
using Infovision.Utils;
using Infovision.Datamining.Roughset.DecisionTrees;

namespace DecisionForestTest
{
    internal class Program
    {
        private DataStore trainData, testData, data;
        private StreamWriter fileStream;
        int[] sizes = new int[] { 1, 10, 20, 30, 40, 50, 60, 70, 80, 90, 100 };

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

        private void ProcessResult<T>(DecisionForestRandom<T> forest, DataStore testData, string name, int test, int fold, decimal epsilon)
            where T : IDecisionTree, new()
        {
            int origSize = forest.Size;
            foreach (int size in sizes)
            {
                forest.Size = size;
                ClassificationResult result = forest.Classify(testData, null);

                result.ModelName = name;
                result.TestNum = test;
                result.Fold = fold;
                result.Epsilon = epsilon;
                result.QualityRatio = forest.AverageNumberOfAttributes;
                result.EnsembleSize = size;
                
                this.WriteLine(result);
            }
            forest.Size = origSize;
        }

        public void Run(BenchmarkData benchmarkData)
        {
            
            this.OpenStream(Path.Combine(@"results", benchmarkData.Name + ".result"));
            
            for (int t = 0; t < 20; t++)
            {
                DataStoreSplitter splitter = null;
                if (benchmarkData.CrossValidationActive)
                {
                    data = DataStore.Load(benchmarkData.DataFile, benchmarkData.FileFormat);
                    if (benchmarkData.DecisionFieldId > 0)
                        data.SetDecisionFieldId(benchmarkData.DecisionFieldId);
                    splitter = new DataStoreSplitter(data, benchmarkData.CrossValidationFolds);
                }
                else
                {
                    trainData = DataStore.Load(benchmarkData.TrainFile, benchmarkData.FileFormat);
                    if (benchmarkData.DecisionFieldId > 0)
                        trainData.SetDecisionFieldId(benchmarkData.DecisionFieldId);
                    testData = DataStore.Load(benchmarkData.TestFile, benchmarkData.FileFormat, trainData.DataStoreInfo);
                }

                for (int fold = 0; fold < benchmarkData.CrossValidationFolds; fold++)
                {
                    if (splitter != null)
                    {
                        splitter.ActiveFold = fold;
                        splitter.Split(ref trainData, ref testData);
                    }

                    DataSampler sampler = new DataSampler(trainData, true);
                    int size = 100;

                    PermutationCollection permutations = new PermutationCollection(size, trainData.DataStoreInfo.GetFieldIds(FieldTypes.Standard).ToArray());
                    
                    for (int e = 0; e < 100; e++)
                    {
                        decimal eps = (decimal)e / (decimal)100;

                        // ###################### Rough Forest Var Eps ######################
                        DecisionForestReduct<DecisionTreeC45> roughForestNoEps = new DecisionForestReduct<DecisionTreeC45>();
                        roughForestNoEps.DataSampler = sampler;
                        roughForestNoEps.Size = size;
                        roughForestNoEps.NumberOfPermutationsPerTree = 20;
                        roughForestNoEps.ReductGeneratorFactory = ReductFactoryKeyHelper.ApproximateReductRelativeWeights;
                        roughForestNoEps.NumberOfAttributesToCheckForSplit = (int)((1 - eps) * trainData.DataStoreInfo.GetNumberOfFields(FieldTypes.Standard));
                        roughForestNoEps.Learn(trainData, trainData.DataStoreInfo.GetFieldIds(FieldTypes.Standard).ToArray());
                        this.ProcessResult<DecisionTreeC45>(roughForestNoEps, testData, "RoughVarEps", t, fold, Decimal.Zero);

                        // ###################### Rough Forest Measure M ######################
                        DecisionForestReduct<DecisionTreeRough> roughForestRough = new DecisionForestReduct<DecisionTreeRough>();
                        roughForestRough.DataSampler = sampler;
                        roughForestRough.Size = size;
                        roughForestRough.NumberOfPermutationsPerTree = 20;
                        roughForestRough.ReductGeneratorFactory = ReductFactoryKeyHelper.ApproximateReductRelativeWeights;
                        roughForestRough.NumberOfAttributesToCheckForSplit = (int)((1 - eps) * trainData.DataStoreInfo.GetNumberOfFields(FieldTypes.Standard));
                        roughForestRough.Learn(trainData, trainData.DataStoreInfo.GetFieldIds(FieldTypes.Standard).ToArray());
                        this.ProcessResult<DecisionTreeRough>(roughForestRough, testData, "RoughForestM", t, fold, Decimal.Zero);

                        // ###################### Dummy Forest ######################
                        DecisionForestDummy<DecisionTreeC45> dummyForest = new DecisionForestDummy<DecisionTreeC45>();
                        dummyForest.DataSampler = sampler;
                        dummyForest.Size = size;
                        dummyForest.PermutationCollection = permutations;
                        dummyForest.NumberOfAttributesToCheckForSplit = (int)((1 - eps) * trainData.DataStoreInfo.GetNumberOfFields(FieldTypes.Standard));
                        dummyForest.Learn(trainData, trainData.DataStoreInfo.GetFieldIds(FieldTypes.Standard).ToArray());
                        this.ProcessResult<DecisionTreeC45>(dummyForest, testData, "Dummy", t, fold, Decimal.Zero);

                        // ###################### Random Forest ######################
                        DecisionForestRandom<DecisionTreeC45> randomForest = new DecisionForestRandom<DecisionTreeC45>();
                        randomForest.DataSampler = sampler;
                        randomForest.Size = size;
                        randomForest.NumberOfAttributesToCheckForSplit = (int)((1 - eps) * trainData.DataStoreInfo.GetNumberOfFields(FieldTypes.Standard));
                        randomForest.Learn(trainData, trainData.DataStoreInfo.GetFieldIds(FieldTypes.Standard).ToArray());
                        this.ProcessResult<DecisionTreeC45>(randomForest, testData, "RandomC45", t, fold, eps);

                        // ###################### Reducted Random subsets ######################
                        DecisionForestDummyRough<DecisionTreeC45> semiRoughForest = new DecisionForestDummyRough<DecisionTreeC45>();
                        semiRoughForest.DataSampler = sampler;
                        semiRoughForest.Size = size;
                        semiRoughForest.Epsilon = eps;
                        semiRoughForest.PermutationCollection = permutations;
                        semiRoughForest.ReductGeneratorFactory = ReductFactoryKeyHelper.ApproximateReductRelativeWeights;
                        semiRoughForest.Learn(trainData, trainData.DataStoreInfo.GetFieldIds(FieldTypes.Standard).ToArray());
                        this.ProcessResult<DecisionTreeC45>(semiRoughForest, testData, "SemiRough", t, fold, eps);

                        // ###################### Rough Forest ######################
                        DecisionForestReduct<DecisionTreeC45> roughForest = new DecisionForestReduct<DecisionTreeC45>();
                        roughForest.DataSampler = sampler;
                        roughForest.Size = size;
                        roughForest.NumberOfPermutationsPerTree = 20;
                        roughForest.Epsilon = eps;
                        roughForest.ReductGeneratorFactory = ReductFactoryKeyHelper.ApproximateReductRelativeWeights;
                        roughForest.Learn(trainData, trainData.DataStoreInfo.GetFieldIds(FieldTypes.Standard).ToArray());
                        this.ProcessResult<DecisionTreeC45>(roughForest, testData, "Rough", t, fold, eps);

                        // ###################### Rough Forest Measure M ######################
                        DecisionForestReduct<DecisionTreeRough> roughForestM = new DecisionForestReduct<DecisionTreeRough>();
                        roughForestM.DataSampler = sampler;
                        roughForestM.Size = size;
                        roughForestM.NumberOfPermutationsPerTree = 20;
                        roughForestM.Epsilon = eps;
                        roughForestM.ReductGeneratorFactory = ReductFactoryKeyHelper.ApproximateReductRelativeWeights;
                        roughForestM.Learn(trainData, trainData.DataStoreInfo.GetFieldIds(FieldTypes.Standard).ToArray());
                        this.ProcessResult<DecisionTreeRough>(roughForestM, testData, "RoughM", t, fold, eps);

                        // ###################### Rough Forest Measure M ######################
                        DecisionForestReduct<DecisionTreeRough> roughForestGamma = new DecisionForestReduct<DecisionTreeRough>();
                        roughForestGamma.DataSampler = sampler;
                        roughForestGamma.Size = size;
                        roughForestGamma.NumberOfPermutationsPerTree = 20;
                        roughForestGamma.Epsilon = eps;
                        roughForestGamma.ReductGeneratorFactory = ReductFactoryKeyHelper.ApproximateReductRelativeWeights;
                        roughForestGamma.Learn(trainData, trainData.DataStoreInfo.GetFieldIds(FieldTypes.Standard).ToArray());
                        this.ProcessResult<DecisionTreeRough>(roughForestGamma, testData, "RoughGamma", t, fold, eps);

                        this.WriteLine();
                    }
                }
            }

            this.CloseStream();
        }        

        public void OpenStream(string path)
        {
            fileStream = new StreamWriter(path, false);
            this.WriteLine(ClassificationResult.ResultHeader());
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


        //###################################  SQL RESULT LOAD ######################################
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
    }
}
