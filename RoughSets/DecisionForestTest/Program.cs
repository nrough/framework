using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Infovision.Data;
using Infovision.Datamining;
using Infovision.Datamining.Benchmark;
using Infovision.Datamining.Roughset;
using Infovision.Utils;

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

                GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
                GC.WaitForPendingFinalizers();
            }
        }

        private void ProcessResult<T>(RandomForest<T> forest, DataStore testData, string name, int test, int fold, decimal epsilon)
            where T : IRandomForestTree, new()
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

                    PermutationCollection permutations = new PermutationCollection();
                    for (int j = 0; j < size; j++)
                    {
                        int[] attributes = trainData.DataStoreInfo.GetFieldIds(FieldTypes.Standard).ToArray();
                        int len = attributes.Length;
                        attributes = attributes.RandomSubArray(RandomSingleton.Random.Next(1, len));
                        permutations.Add(new Permutation(attributes));
                    }

                    // ###################### Dummy Forest ######################
                    DummyForest<DecisionTreeC45> dummyForest = new DummyForest<DecisionTreeC45>();
                    dummyForest.DataSampler = sampler;
                    dummyForest.Size = size;
                    dummyForest.PermutationCollection = permutations;
                    dummyForest.Learn(trainData, trainData.DataStoreInfo.GetFieldIds(FieldTypes.Standard).ToArray());
                    this.ProcessResult<DecisionTreeC45>(dummyForest, testData, "Dummy", t, fold, Decimal.Zero);

                    // ###################### Rough Forest Var Eps ######################
                    RoughForest<DecisionTreeC45> roughForestNoEps = new RoughForest<DecisionTreeC45>();
                    roughForestNoEps.DataSampler = sampler;
                    roughForestNoEps.Size = size;
                    roughForestNoEps.NumberOfPermutationsPerTree = 20;
                    roughForestNoEps.ReductGeneratorFactory = ReductFactoryKeyHelper.ApproximateReductRelativeWeights;
                    roughForestNoEps.Learn(trainData, trainData.DataStoreInfo.GetFieldIds(FieldTypes.Standard).ToArray());
                    this.ProcessResult<DecisionTreeC45>(roughForestNoEps, testData, "RoughVarEps", t, fold, Decimal.Zero);

                    // ###################### Rough Forest Measure M ######################
                    RoughForest<DecisionTreeRough> roughForestRough = new RoughForest<DecisionTreeRough>();
                    roughForestRough.DataSampler = sampler;
                    roughForestRough.Size = size;
                    roughForestRough.NumberOfPermutationsPerTree = 20;
                    roughForestRough.ReductGeneratorFactory = ReductFactoryKeyHelper.ApproximateReductRelativeWeights;
                    roughForestRough.Learn(trainData, trainData.DataStoreInfo.GetFieldIds(FieldTypes.Standard).ToArray());
                    this.ProcessResult<DecisionTreeRough>(roughForestRough, testData, "RoughForestM", t, fold, Decimal.Zero);

                    for (int e = 0; e < 100; e++)
                    {
                        decimal eps = (decimal)e / (decimal)100;

                        // ###################### Random Forest ######################
                        RandomForest<DecisionTreeC45> randomForest = new RandomForest<DecisionTreeC45>();
                        randomForest.DataSampler = sampler;
                        randomForest.Size = size;
                        randomForest.NumberOfRandomAttributes = (int)((1 - eps) * trainData.DataStoreInfo.GetNumberOfFields(FieldTypes.Standard));
                        randomForest.Learn(trainData, trainData.DataStoreInfo.GetFieldIds(FieldTypes.Standard).ToArray());
                        this.ProcessResult<DecisionTreeC45>(randomForest, testData, "RandomC45", t, fold, eps);

                        // ###################### Reducted Random subsets ######################
                        SemiRoughForest<DecisionTreeC45> semiRoughForest = new SemiRoughForest<DecisionTreeC45>();
                        semiRoughForest.DataSampler = sampler;
                        semiRoughForest.Size = size;
                        semiRoughForest.Epsilon = eps;
                        semiRoughForest.PermutationCollection = permutations;
                        semiRoughForest.ReductGeneratorFactory = ReductFactoryKeyHelper.ApproximateReductRelativeWeights;
                        semiRoughForest.Learn(trainData, trainData.DataStoreInfo.GetFieldIds(FieldTypes.Standard).ToArray());
                        this.ProcessResult<DecisionTreeC45>(semiRoughForest, testData, "SemiRough", t, fold, eps);

                        // ###################### Rough Forest ######################
                        RoughForest<DecisionTreeC45> roughForest = new RoughForest<DecisionTreeC45>();
                        roughForest.DataSampler = sampler;
                        roughForest.Size = size;
                        roughForest.NumberOfPermutationsPerTree = 20;
                        roughForest.Epsilon = eps;
                        roughForest.ReductGeneratorFactory = ReductFactoryKeyHelper.ApproximateReductRelativeWeights;
                        roughForest.Learn(trainData, trainData.DataStoreInfo.GetFieldIds(FieldTypes.Standard).ToArray());
                        this.ProcessResult<DecisionTreeC45>(roughForest, testData, "Rough", t, fold, eps);

                        // ###################### Rough Forest Measure M ######################
                        RoughForest<DecisionTreeRough> roughForestM = new RoughForest<DecisionTreeRough>();
                        roughForestM.DataSampler = sampler;
                        roughForestM.Size = size;
                        roughForestM.NumberOfPermutationsPerTree = 20;
                        roughForestM.Epsilon = eps;
                        roughForestM.ReductGeneratorFactory = ReductFactoryKeyHelper.ApproximateReductRelativeWeights;
                        roughForestM.Learn(trainData, trainData.DataStoreInfo.GetFieldIds(FieldTypes.Standard).ToArray());
                        this.ProcessResult<DecisionTreeRough>(roughForestM, testData, "RoughM", t, fold, eps);

                        // ###################### Rough Forest Measure M ######################
                        RoughForest<DecisionTreeRough> roughForestGamma = new RoughForest<DecisionTreeRough>();
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
    }
}
