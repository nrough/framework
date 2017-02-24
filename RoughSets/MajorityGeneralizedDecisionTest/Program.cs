using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NRough.Data;
using NRough.MachineLearning;
using NRough.MachineLearning.Roughsets;
using NRough.Core;
using NRough.MachineLearning.Benchmark;
using NRough.MachineLearning.Permutations;
using NRough.MachineLearning.Weighting;
using NRough.MachineLearning.Classification;

namespace MajorityGeneralizedDecisionTest
{
    internal class Program
    {
        private DataStore trainData, testData, data;
        private double eps;
        private int ensembleSize;
        private int ratio;
        private int permutationSize;
        private ReductLengthComparer reductLengthComparer;
        private ReductMeasureLength reductMeasureLength;

        private ReductStoreLengthComparer reductStoreLengthComparer;
        //ReductStoreLengthComparer reductStoreLengthComparerGaps;

        private PermutationCollection permList;
        private int t;
        private StreamWriter fileStream;
        private Dictionary<string, IReductGenerator> reductGeneratorCache;
        private int[] sizes;
        private int maxTest;
        private int fold;

        public void MajorityGeneralizedDecisionPerformanceTest(
            WeightGenerator weightGenerator,
            RuleQualityMethod identification,
            RuleQualityMethod voting)
        {
            ReductGeneralizedMajorityDecisionGenerator generator = null;
            if (reductGeneratorCache.ContainsKey(ReductTypes.GeneralizedMajorityDecision))
            {
                generator = reductGeneratorCache[ReductTypes.GeneralizedMajorityDecision] as ReductGeneralizedMajorityDecisionGenerator;
            }
            else
            {
                Args parms = new Args();
                parms.SetParameter(ReductFactoryOptions.DecisionTable, trainData);
                parms.SetParameter(ReductFactoryOptions.ReductType, ReductTypes.GeneralizedMajorityDecision);
                parms.SetParameter(ReductFactoryOptions.WeightGenerator, weightGenerator);
                parms.SetParameter(ReductFactoryOptions.Epsilon, eps);
                parms.SetParameter(ReductFactoryOptions.PermutationCollection, permList);
                parms.SetParameter(ReductFactoryOptions.UseExceptionRules, false);

                generator = ReductFactory.GetReductGenerator(parms) as ReductGeneralizedMajorityDecisionGenerator;
                generator.Run();

                reductGeneratorCache.Add(ReductTypes.GeneralizedMajorityDecision, generator);
            }

            IReductStoreCollection origReductStoreCollection = generator.GetReductStoreCollection();
            IReductStoreCollection filteredReductStoreCollection = origReductStoreCollection.Filter(ensembleSize, reductLengthComparer);

            RoughClassifier classifier = new RoughClassifier(
                filteredReductStoreCollection,
                identification,
                voting,
                trainData.DataStoreInfo.GetDecisionValues());
            classifier.UseExceptionRules = false;
            ClassificationResult result = classifier.Classify(testData);
            result.AvgNumberOfAttributes = filteredReductStoreCollection.GetAvgMeasure(reductMeasureLength, false);
            result.ModelCreationTime = generator.ReductGenerationTime;

            this.WriteLine("{0,5}|{1}|{2}|{3,2}|{4,4}|{5,14}|{6,22}|{7}|{8}", "GMDR", t, fold, ensembleSize, eps, identification.Method.Name, voting.Method.Name, weightGenerator.GetType().Name.Substring(15), result);
        }

        public void MajorityGeneralizedDecisionNoExceptionsPerformanceTest(
            WeightGenerator weightGenerator,
            RuleQualityMethod identification,
            RuleQualityMethod voting)
        {
            ReductGeneralizedMajorityDecisionGenerator generator = null;
            if (reductGeneratorCache.ContainsKey(ReductTypes.GeneralizedMajorityDecisionApproximate + "NoExp"))
            {
                generator = reductGeneratorCache[ReductTypes.GeneralizedMajorityDecisionApproximate + "NoExp"] as ReductGeneralizedMajorityDecisionGenerator;
            }
            else
            {
                Args parms = new Args();
                parms.SetParameter(ReductFactoryOptions.DecisionTable, trainData);
                parms.SetParameter(ReductFactoryOptions.ReductType, ReductTypes.GeneralizedMajorityDecisionApproximate);
                parms.SetParameter(ReductFactoryOptions.WeightGenerator, weightGenerator);
                parms.SetParameter(ReductFactoryOptions.Epsilon, eps);
                parms.SetParameter(ReductFactoryOptions.PermutationCollection, permList);
                parms.SetParameter(ReductFactoryOptions.UseExceptionRules, false);

                generator = ReductFactory.GetReductGenerator(parms) as ReductGeneralizedMajorityDecisionGenerator;
                generator.Run();

                reductGeneratorCache.Add(ReductTypes.GeneralizedMajorityDecisionApproximate + "NoExp", generator);
            }

            IReductStoreCollection origReductStoreCollection = generator.GetReductStoreCollection();
            IReductStoreCollection filteredReductStoreCollection = origReductStoreCollection.Filter(ensembleSize, reductLengthComparer);

            RoughClassifier classifier = new RoughClassifier(
                filteredReductStoreCollection,
                identification,
                voting,
                trainData.DataStoreInfo.GetDecisionValues());

            classifier.UseExceptionRules = false;
            classifier.ExceptionRulesAsGaps = false;

            ClassificationResult result = classifier.Classify(testData);
            result.AvgNumberOfAttributes = filteredReductStoreCollection.GetAvgMeasure(reductMeasureLength, false);
            result.ModelCreationTime = generator.ReductGenerationTime;

            this.WriteLine("{0,5}|{1}|{2}|{3,2}|{4,4}|{5,14}|{6,22}|{7}|{8}", "NOEX", t, fold, ensembleSize, eps, identification.Method.Name, voting.Method.Name, weightGenerator.GetType().Name.Substring(15), result);
        }

        public void MajorityGeneralizedDecisionGapsPerformanceTest(
            WeightGenerator weightGenerator,
            RuleQualityMethod identification,
            RuleQualityMethod voting)
        {
            ReductGeneralizedMajorityDecisionGenerator generator = null;
            if (reductGeneratorCache.ContainsKey(ReductTypes.GeneralizedMajorityDecisionApproximate))
            {
                generator = reductGeneratorCache[ReductTypes.GeneralizedMajorityDecisionApproximate] as ReductGeneralizedMajorityDecisionGenerator;
            }
            else
            {
                Args parms = new Args();
                parms.SetParameter(ReductFactoryOptions.DecisionTable, trainData);
                parms.SetParameter(ReductFactoryOptions.ReductType, ReductTypes.GeneralizedMajorityDecisionApproximate);
                parms.SetParameter(ReductFactoryOptions.WeightGenerator, weightGenerator);
                parms.SetParameter<double>(ReductFactoryOptions.Epsilon, eps);
                //parms.SetParameter(ReductGeneratorParamHelper.EquivalenceClassSortDirection, SortDirection.Descending);

                parms.SetParameter(ReductFactoryOptions.PermutationCollection, permList);
                parms.SetParameter(ReductFactoryOptions.UseExceptionRules, true);

                generator = ReductFactory.GetReductGenerator(parms) as ReductGeneralizedMajorityDecisionGenerator;
                generator.Run();

                reductGeneratorCache.Add(ReductTypes.GeneralizedMajorityDecisionApproximate, generator);
            }

            IReductStoreCollection origReductStoreCollection = generator.GetReductStoreCollection();
            IReductStoreCollection filteredReductStoreCollection = origReductStoreCollection.FilterInEnsemble(ensembleSize, reductStoreLengthComparer);
            double avgLength = filteredReductStoreCollection.GetWeightedAvgMeasure(reductMeasureLength, true); ;

            RoughClassifier classifier = new RoughClassifier(
                filteredReductStoreCollection,
                identification,
                voting,
                trainData.DataStoreInfo.GetDecisionValues());

            classifier.UseExceptionRules = true;
            classifier.ExceptionRulesAsGaps = false;

            ClassificationResult result = classifier.Classify(testData);
            result.AvgNumberOfAttributes = avgLength;
            result.ModelCreationTime = generator.ReductGenerationTime;

            this.WriteLine("{0,5}|{1}|{2}|{3,2}|{4,4}|{5,14}|{6,22}|{7}|{8}", "EXEP", t, fold, ensembleSize, eps, identification.Method.Name, voting.Method.Name, weightGenerator.GetType().Name.Substring(15), result);
            
            classifier = new RoughClassifier(
                filteredReductStoreCollection,
                identification,
                voting,
                trainData.DataStoreInfo.GetDecisionValues());

            classifier.UseExceptionRules = true;
            classifier.ExceptionRulesAsGaps = true;

            result = classifier.Classify(testData);
            result.AvgNumberOfAttributes = avgLength;
            result.ModelCreationTime = generator.ReductGenerationTime;

            this.WriteLine("{0,5}|{1}|{2}|{3,2}|{4,4}|{5,14}|{6,22}|{7}|{8}", "GAPS", t, fold, ensembleSize, eps, identification.Method.Name, voting.Method.Name, weightGenerator.GetType().Name.Substring(15), result);
        }

        public void ApproximateDecisionReduct(
            WeightGenerator weightGenerator,
            RuleQualityMethod identification,
            RuleQualityMethod voting)
        {
            ReductGeneratorWeightsMajority generator = null;
            if (reductGeneratorCache.ContainsKey(ReductTypes.ApproximateReductMajorityWeights))
            {
                generator = reductGeneratorCache[ReductTypes.ApproximateReductMajorityWeights] as ReductGeneratorWeightsMajority;
            }
            else
            {
                Args parms = new Args();
                parms.SetParameter(ReductFactoryOptions.DecisionTable, trainData);
                parms.SetParameter(ReductFactoryOptions.ReductType, ReductTypes.ApproximateReductMajorityWeights);
                parms.SetParameter(ReductFactoryOptions.WeightGenerator, weightGenerator);
                parms.SetParameter(ReductFactoryOptions.Epsilon, eps);
                parms.SetParameter(ReductFactoryOptions.PermutationCollection, permList);
                parms.SetParameter(ReductFactoryOptions.UseExceptionRules, false);

                generator = ReductFactory.GetReductGenerator(parms) as ReductGeneratorWeightsMajority;
                generator.UsePerformanceImprovements = true;
                generator.Run();

                reductGeneratorCache.Add(ReductTypes.ApproximateReductMajorityWeights, generator);
            }

            IReductStoreCollection origReductStoreCollection = generator.GetReductStoreCollection();
            IReductStoreCollection filteredReductStoreCollectionApprox = origReductStoreCollection.Filter(ensembleSize, reductLengthComparer);

            RoughClassifier classifier = new RoughClassifier(
                filteredReductStoreCollectionApprox,
                identification,
                voting,
                trainData.DataStoreInfo.GetDecisionValues());
            classifier.UseExceptionRules = false;
            ClassificationResult result = classifier.Classify(testData);
            result.AvgNumberOfAttributes = filteredReductStoreCollectionApprox.GetAvgMeasure(reductMeasureLength, false);
            result.ModelCreationTime = generator.ReductGenerationTime;

            this.WriteLine(String.Format("{0,5}|{1}|{2}|{3,2}|{4,4}|{5,14}|{6,22}|{7}|{8}", "ADR", t, fold, ensembleSize, eps, identification.Method.Name, voting.Method.Name, weightGenerator.GetType().Name.Substring(15), result));
        }

        private static void Main(string[] args)
        {
            string[] names = args;

            foreach (var kvp in BenchmarkDataHelper.GetDataFiles(names: names))
            {
                Program program = new Program();
                program.Init(kvp.Value);
                program.Run(kvp.Value);

                GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
                GC.WaitForPendingFinalizers();
            }
        }

        public void Init(BenchmarkData benchmarkData)
        {
            ensembleSize = 10;
            ratio = 5;
            permutationSize = ensembleSize * ratio;

            reductLengthComparer = new ReductLengthComparer();
            reductStoreLengthComparer = new ReductStoreLengthComparer(true);
            //reductStoreLengthComparerGaps = new ReductStoreLengthComparer(false);
            reductMeasureLength = new ReductMeasureLength();

            this.ClearCache();

            //in decreasing order!
            sizes = new int[] { 20 };
            maxTest = 10;
        }

        public void ClearCache()
        {
            reductGeneratorCache = new Dictionary<string, IReductGenerator>();
        }

        public void Run(BenchmarkData benchmarkData)
        {
            DataSplitter splitter;

            this.OpenStream(Path.Combine(@"results", benchmarkData.Name + ".result"));

            ensembleSize = sizes.First();
            permutationSize = ensembleSize * ratio;

            for (int i = 0; i < maxTest; i++)
            {
                t = i;
                splitter = null;
                if (benchmarkData.CrossValidationActive)
                {
                    data = DataStore.Load(benchmarkData.DataFile, benchmarkData.FileFormat);

                    if (benchmarkData.DecisionFieldId > 0)
                        data.SetDecisionFieldId(benchmarkData.DecisionFieldId);

                    splitter = new DataSplitter(data, benchmarkData.CrossValidationFolds);

                    this.InitPermutation(data);
                }
                else
                {
                    trainData = DataStore.Load(benchmarkData.TrainFile, benchmarkData.FileFormat);

                    if (benchmarkData.DecisionFieldId > 0)
                        trainData.SetDecisionFieldId(benchmarkData.DecisionFieldId);

                    WeightGenerator weightGenerator = new WeightGeneratorMajority(trainData);
                    trainData.SetWeights(weightGenerator.Weights);

                    testData = DataStore.Load(benchmarkData.TestFile, benchmarkData.FileFormat, trainData.DataStoreInfo);

                    this.InitPermutation(trainData);
                }

                for (int f = 0; f < benchmarkData.CrossValidationFolds; f++)
                {
                    fold = f;

                    if (splitter != null)
                    {                        
                        splitter.Split(out trainData, out testData, f);

                        WeightGenerator weightGenerator = new WeightGeneratorMajority(trainData);
                        trainData.SetWeights(weightGenerator.Weights);
                    }

                    for (int e = 0; e < 100; e++)
                    {
                        eps = (double)e / (double)100;

                        for (int w = 0; w < 2; w++)
                        {
                            WeightGenerator weightGenerator = (w == 0)
                                            ? new WeightGeneratorMajority(trainData) as WeightGenerator
                                            : new WeightGeneratorRelative(trainData) as WeightGenerator;

                            trainData.SetWeights(weightGenerator.Weights);

                            ClearCache();

                            foreach (int size in sizes)
                            {
                                ensembleSize = size;

                                //only confidenceW
                                for (int j = 2; j <= 2; j++)
                                {
                                    RuleQualityMethod v1, v2;
                                    switch (j)
                                    {
                                        case 0: v1 = RuleQualityAvgMethods.Confidence; v2 = RuleQualityMethods.Confidence; break;
                                        case 1: v1 = RuleQualityAvgMethods.ConfidenceRelativeW; v2 = RuleQualityMethods.ConfidenceRelativeW; break;
                                        case 2: v1 = RuleQualityAvgMethods.ConfidenceW; v2 = RuleQualityMethods.ConfidenceW; break;
                                        case 3: v1 = RuleQualityAvgMethods.Coverage; v2 = RuleQualityMethods.Coverage; break;
                                        case 4: v1 = RuleQualityAvgMethods.CoverageW; v2 = RuleQualityMethods.CoverageW; break;
                                        case 5: v1 = RuleQualityAvgMethods.Ratio; v2 = RuleQualityMethods.Ratio; break;
                                        case 6: v1 = RuleQualityAvgMethods.RatioW; v2 = RuleQualityMethods.RatioW; break;
                                        case 7: v1 = RuleQualityAvgMethods.Support; v2 = RuleQualityMethods.Support; break;
                                        case 8: v1 = RuleQualityAvgMethods.SupportW; v2 = RuleQualityMethods.SupportW; break;
                                        case 9: v1 = RuleQualityMethods.SingleVote; v2 = RuleQualityMethods.SingleVote; break;

                                        default: v1 = RuleQualityMethods.SingleVote; v2 = RuleQualityMethods.SingleVote; break;
                                    }

                                    this.MajorityGeneralizedDecisionGapsPerformanceTest(weightGenerator, RuleQualityAvgMethods.ConfidenceW, v1);
                                    this.MajorityGeneralizedDecisionNoExceptionsPerformanceTest(weightGenerator, RuleQualityAvgMethods.ConfidenceW, v1);
                                    //this.MajorityGeneralizedDecisionPerformanceTest(weightGenerator, RuleQualityAvg.ConfidenceW, v1);
                                    this.ApproximateDecisionReduct(weightGenerator, RuleQualityMethods.ConfidenceW, v2);
                                }

                                Console.WriteLine();
                            }
                        }
                    }
                }
            }

            this.CloseStream();
        }

        public void InitPermutation(DataStore d)
        {
            Args permParm = new Args();

            permParm.SetParameter(ReductFactoryOptions.DecisionTable, d);
            permParm.SetParameter(ReductFactoryOptions.ReductType, ReductTypes.ApproximateReductMajority);
            IPermutationGenerator permGen = ReductFactory.GetPermutationGenerator(permParm);
            permList = permGen.Generate(permutationSize);
        }

        public void OpenStream(string path)
        {
            fileStream = new StreamWriter(path, false);

            this.WriteLine("{0}|{1}|{2}|{3}|{4}|{5}|{6}|{7}|{8}", "Factory", "Test", "Fold", "EnsembleSize", "Epsilon", "Ident", "Vote", "Weight", ClassificationResult.TableHeader());
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