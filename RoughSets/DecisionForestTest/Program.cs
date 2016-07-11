﻿using System;
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

        public void Run(BenchmarkData benchmarkData)
        {
            DataStoreSplitter splitter;

            this.OpenStream(Path.Combine(@"results", benchmarkData.Name + ".result"));
            
            for (int t = 0; t < 20; t++)
            {
                splitter = null;
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

                        WeightGenerator weightGenerator = new WeightGeneratorMajority(trainData);
                        trainData.SetWeights(weightGenerator.Weights);
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

                    DummyForest<DecisionTreeC45> dummyForest = new DummyForest<DecisionTreeC45>();
                    dummyForest.DataSampler = sampler;
                    dummyForest.Size = size;
                    dummyForest.PermutationCollection = permutations;
                    dummyForest.Learn(trainData, trainData.DataStoreInfo.GetFieldIds(FieldTypes.Standard).ToArray());

                    ClassificationResult dummyForestResult = dummyForest.Classify(testData, null);
                    dummyForestResult.ModelName = "Dummy";
                    dummyForestResult.TestNum = t;
                    dummyForestResult.Fold = 0;
                    dummyForestResult.Epsilon = Decimal.Zero;
                    dummyForestResult.QualityRatio = dummyForest.AverageNumberOfAttributes;
                    dummyForestResult.EnsembleSize = size;
                    dummyForestResult.DatasetName = testData.Name;
                    Console.WriteLine(dummyForestResult);

                    DecisionTreeC45 c45tree = new DecisionTreeC45();
                    c45tree.Learn(trainData, trainData.DataStoreInfo.GetFieldIds(FieldTypes.Standard).ToArray());

                    
                    ClassificationResult c45treeResult = c45tree.Classify(testData, null);
                    c45treeResult.ModelName = "C45Tree";
                    c45treeResult.TestNum = t;
                    c45treeResult.Fold = 0;
                    c45treeResult.Epsilon = Decimal.Zero;
                    c45treeResult.QualityRatio = ((DecisionTreeNode)c45tree.Root)
                        .GroupBy(x => x.Key)
                        .Select(g => g.First().Key)
                        .Where(x => x != -1 && x != trainData.DataStoreInfo.DecisionFieldId)
                        .OrderBy(x => x).ToArray().Length;
                    c45treeResult.EnsembleSize = size;
                    c45treeResult.DatasetName = testData.Name;
                    Console.WriteLine(c45treeResult);

                    for (int e = 0; e < 100; e++)
                    {
                        decimal eps = (decimal)e / (decimal)100;

                        RandomForest<DecisionTreeC45> randomForest = new RandomForest<DecisionTreeC45>();
                        randomForest.DataSampler = sampler;
                        randomForest.Size = size;
                        randomForest.NumberOfRandomAttributes = (int)(eps * trainData.DataStoreInfo.GetNumberOfFields(FieldTypes.Standard));
                        randomForest.Learn(trainData, trainData.DataStoreInfo.GetFieldIds(FieldTypes.Standard).ToArray());

                        ClassificationResult randomForestResult = randomForest.Classify(testData, null);
                        randomForestResult.ModelName = "RandomC45";
                        randomForestResult.TestNum = t;
                        randomForestResult.Fold = 0;
                        randomForestResult.Epsilon = eps;
                        randomForestResult.QualityRatio = randomForest.AverageNumberOfAttributes;
                        randomForestResult.EnsembleSize = size;
                        randomForestResult.DatasetName = testData.Name;
                        Console.WriteLine(randomForestResult);
                        
                        SemiRoughForest<DecisionTreeC45> semiRoughForest = new SemiRoughForest<DecisionTreeC45>();
                        semiRoughForest.DataSampler = sampler;
                        semiRoughForest.Size = size;
                        semiRoughForest.Epsilon = eps;
                        semiRoughForest.PermutationCollection = permutations;
                        semiRoughForest.ReductGeneratorFactory = ReductFactoryKeyHelper.ApproximateReductRelativeWeights;
                        semiRoughForest.Learn(trainData, trainData.DataStoreInfo.GetFieldIds(FieldTypes.Standard).ToArray());

                        ClassificationResult semiRoughForestResult = semiRoughForest.Classify(testData, null);
                        semiRoughForestResult.ModelName = "SemiRough";
                        semiRoughForestResult.TestNum = t;
                        semiRoughForestResult.Fold = fold;
                        semiRoughForestResult.Epsilon = eps;
                        semiRoughForestResult.QualityRatio = semiRoughForest.AverageNumberOfAttributes;
                        semiRoughForestResult.EnsembleSize = size;
                        semiRoughForestResult.DatasetName = testData.Name;
                        Console.WriteLine(semiRoughForestResult);

                        RoughForest<DecisionTreeC45> roughForest = new RoughForest<DecisionTreeC45>();
                        roughForest.DataSampler = sampler;
                        roughForest.Size = size;
                        roughForest.NumberOfPermutationsPerTree = 20;
                        roughForest.ReductGeneratorFactory = ReductFactoryKeyHelper.ApproximateReductRelativeWeights;
                        roughForest.Learn(trainData, trainData.DataStoreInfo.GetFieldIds(FieldTypes.Standard).ToArray());

                        ClassificationResult roughForestResult = roughForest.Classify(testData, null);
                        roughForestResult.ModelName = "Rough";
                        roughForestResult.TestNum = fold;
                        roughForestResult.Fold = fold;
                        roughForestResult.Epsilon = Decimal.Zero;
                        roughForestResult.QualityRatio = roughForest.AverageNumberOfAttributes;
                        roughForestResult.EnsembleSize = size;
                        roughForestResult.DatasetName = testData.Name;
                        Console.WriteLine(roughForestResult);

                        Console.WriteLine(); 
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
    }
}
