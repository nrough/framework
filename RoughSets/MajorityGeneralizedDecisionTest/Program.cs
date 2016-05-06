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

namespace MajorityGeneralizedDecisionTest
{
    class Program
    {
        DataStore trainData, testData, data;        
        decimal eps;
        int ensembleSize;
        int ratio;
        int permutationSize;
        ReductLengthComparer reductLengthComparer;
        ReductMeasureLength reductMeasureLength;
        ReductStoreLengthComparer reductStoreLengthComparer;
        PermutationCollection permList;
        int t;
        StreamWriter fileStream;
        Dictionary<string, IReductGenerator> reductGeneratorCache;
        int[] sizes;
        int maxTest;
        int fold;
        
        public void MajorityGeneralizedDecisionPerformanceTest(
            WeightGenerator weightGenerator, 
            RuleQualityFunction identification, 
            RuleQualityFunction voting)
        {
            ReductGeneralizedMajorityDecisionGenerator generator = null;
            if (reductGeneratorCache.ContainsKey(ReductFactoryKeyHelper.GeneralizedMajorityDecision))
            {
                generator = reductGeneratorCache[ReductFactoryKeyHelper.GeneralizedMajorityDecision] as ReductGeneralizedMajorityDecisionGenerator;
            }
            else
            {
                Args parms = new Args();
                parms.SetParameter(ReductGeneratorParamHelper.TrainData, trainData);
                parms.SetParameter(ReductGeneratorParamHelper.FactoryKey, ReductFactoryKeyHelper.GeneralizedMajorityDecision);
                parms.SetParameter(ReductGeneratorParamHelper.WeightGenerator, weightGenerator);
                parms.SetParameter(ReductGeneratorParamHelper.Epsilon, eps);
                parms.SetParameter(ReductGeneratorParamHelper.PermutationCollection, permList);
                parms.SetParameter(ReductGeneratorParamHelper.UseExceptionRules, false);

                generator = ReductFactory.GetReductGenerator(parms) as ReductGeneralizedMajorityDecisionGenerator;
                generator.Run();

                reductGeneratorCache.Add(ReductFactoryKeyHelper.GeneralizedMajorityDecision, generator);
                
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
            result.QualityRatio = filteredReductStoreCollection.GetAvgMeasure(reductMeasureLength, false);
            result.ModelCreationTime = generator.ReductGenerationTime;            

            this.WriteLine("{0,5}|{1}|{2}|{3,2}|{4,4}|{5,14}|{6,22}|{7}|{8}", "GMDR", t, fold, ensembleSize, eps, identification.Method.Name, voting.Method.Name, weightGenerator.GetType().Name.Substring(15), result);            
        }        

        public void MajorityGeneralizedDecisionNoExceptionsPerformanceTest(
            WeightGenerator weightGenerator, 
            RuleQualityFunction identification, 
            RuleQualityFunction voting)
        {
            ReductGeneralizedMajorityDecisionGenerator generator = null;
            if (reductGeneratorCache.ContainsKey(ReductFactoryKeyHelper.GeneralizedMajorityDecisionApproximate + "NoExp"))
            {
                generator = reductGeneratorCache[ReductFactoryKeyHelper.GeneralizedMajorityDecisionApproximate + "NoExp"] as ReductGeneralizedMajorityDecisionGenerator;
            }
            else
            {
                Args parms = new Args();
                parms.SetParameter(ReductGeneratorParamHelper.TrainData, trainData);
                parms.SetParameter(ReductGeneratorParamHelper.FactoryKey, ReductFactoryKeyHelper.GeneralizedMajorityDecisionApproximate);
                parms.SetParameter(ReductGeneratorParamHelper.WeightGenerator, weightGenerator);
                parms.SetParameter(ReductGeneratorParamHelper.Epsilon, eps);
                parms.SetParameter(ReductGeneratorParamHelper.PermutationCollection, permList);
                parms.SetParameter(ReductGeneratorParamHelper.UseExceptionRules, false);

                generator = ReductFactory.GetReductGenerator(parms) as ReductGeneralizedMajorityDecisionGenerator;
                generator.Run();

                reductGeneratorCache.Add(ReductFactoryKeyHelper.GeneralizedMajorityDecisionApproximate + "NoExp", generator);
                
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
            result.QualityRatio = filteredReductStoreCollection.GetAvgMeasure(reductMeasureLength, false);
            result.ModelCreationTime = generator.ReductGenerationTime;            

            this.WriteLine("{0,5}|{1}|{2}|{3,2}|{4,4}|{5,14}|{6,22}|{7}|{8}", "NOEX", t, fold, ensembleSize, eps, identification.Method.Name, voting.Method.Name, weightGenerator.GetType().Name.Substring(15), result);

        }

        public void MajorityGeneralizedDecisionGapsPerformanceTest(
            WeightGenerator weightGenerator, 
            RuleQualityFunction identification, 
            RuleQualityFunction voting)
        {
            ReductGeneralizedMajorityDecisionGenerator generator = null;
            if (reductGeneratorCache.ContainsKey(ReductFactoryKeyHelper.GeneralizedMajorityDecisionApproximate))
            {
                generator = reductGeneratorCache[ReductFactoryKeyHelper.GeneralizedMajorityDecisionApproximate] as ReductGeneralizedMajorityDecisionGenerator;
            }
            else
            {
                Args parms = new Args();
                parms.SetParameter(ReductGeneratorParamHelper.TrainData, trainData);
                parms.SetParameter(ReductGeneratorParamHelper.FactoryKey, ReductFactoryKeyHelper.GeneralizedMajorityDecisionApproximate);
                parms.SetParameter(ReductGeneratorParamHelper.WeightGenerator, weightGenerator);
                parms.SetParameter(ReductGeneratorParamHelper.Epsilon, eps);
                parms.SetParameter(ReductGeneratorParamHelper.PermutationCollection, permList);
                parms.SetParameter(ReductGeneratorParamHelper.UseExceptionRules, true);            

                generator = ReductFactory.GetReductGenerator(parms) as ReductGeneralizedMajorityDecisionGenerator;
                generator.Run();

                reductGeneratorCache.Add(ReductFactoryKeyHelper.GeneralizedMajorityDecisionApproximate, generator);
                
            }

            IReductStoreCollection origReductStoreCollection = generator.GetReductStoreCollection();
            IReductStoreCollection filteredReductStoreCollection = origReductStoreCollection.FilterInEnsemble(ensembleSize, reductStoreLengthComparer);            

            RoughClassifier classifier = new RoughClassifier(
                filteredReductStoreCollection,
                identification,
                voting,
                trainData.DataStoreInfo.GetDecisionValues());
            
            classifier.UseExceptionRules = true;
            classifier.ExceptionRulesAsGaps = true;

            ClassificationResult result = classifier.Classify(testData);
            result.QualityRatio = filteredReductStoreCollection.GetAvgMeasure(reductMeasureLength, true);
            result.ModelCreationTime = generator.ReductGenerationTime;            

            this.WriteLine("{0,5}|{1}|{2}|{3,2}|{4,4}|{5,14}|{6,22}|{7}|{8}", "GAPS", t, fold, ensembleSize, eps, identification.Method.Name, voting.Method.Name, weightGenerator.GetType().Name.Substring(15), result);            

            classifier = new RoughClassifier(
                filteredReductStoreCollection,
                identification,
                voting,
                trainData.DataStoreInfo.GetDecisionValues());

            classifier.UseExceptionRules = true;
            classifier.ExceptionRulesAsGaps = false;

            result = classifier.Classify(testData);
            result.QualityRatio = filteredReductStoreCollection.GetAvgMeasure(reductMeasureLength, true);
            result.ModelCreationTime = generator.ReductGenerationTime;            

            this.WriteLine("{0,5}|{1}|{2}|{3,2}|{4,4}|{5,14}|{6,22}|{7}|{8}", "EXEP", t, fold, ensembleSize, eps, identification.Method.Name, voting.Method.Name, weightGenerator.GetType().Name.Substring(15), result);                       
        }

        public void ApproximateDecisionReduct(
            WeightGenerator weightGenerator,
            RuleQualityFunction identification,
            RuleQualityFunction voting)
        {
            ReductGeneratorWeightsMajority generator = null;
            if (reductGeneratorCache.ContainsKey(ReductFactoryKeyHelper.ApproximateReductMajorityWeights))
            {
                generator = reductGeneratorCache[ReductFactoryKeyHelper.ApproximateReductMajorityWeights] as ReductGeneratorWeightsMajority;
            }
            else
            {
                Args parms = new Args();
                parms.SetParameter(ReductGeneratorParamHelper.TrainData, trainData);
                parms.SetParameter(ReductGeneratorParamHelper.FactoryKey, ReductFactoryKeyHelper.ApproximateReductMajorityWeights);
                parms.SetParameter(ReductGeneratorParamHelper.WeightGenerator, weightGenerator);
                parms.SetParameter(ReductGeneratorParamHelper.Epsilon, eps);
                parms.SetParameter(ReductGeneratorParamHelper.PermutationCollection, permList);
                parms.SetParameter(ReductGeneratorParamHelper.UseExceptionRules, false);

                generator = ReductFactory.GetReductGenerator(parms) as ReductGeneratorWeightsMajority;
                generator.Run();

                reductGeneratorCache.Add(ReductFactoryKeyHelper.ApproximateReductMajorityWeights, generator);
                
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
            result.QualityRatio = filteredReductStoreCollectionApprox.GetAvgMeasure(reductMeasureLength, false);
            result.ModelCreationTime = generator.ReductGenerationTime;            

            this.WriteLine(String.Format("{0,5}|{1}|{2}|{3,2}|{4,4}|{5,14}|{6,22}|{7}|{8}", "ADR", t, fold, ensembleSize, eps, identification.Method.Name, voting.Method.Name, weightGenerator.GetType().Name.Substring(15), result));
        }

        static void Main(string[] args)
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
            eps = 0.17m;
            ensembleSize = 10;
            ratio = 10;
            permutationSize = ensembleSize * ratio;

            reductLengthComparer = new ReductLengthComparer();
            reductStoreLengthComparer = new ReductStoreLengthComparer(true);
            reductMeasureLength = new ReductMeasureLength();                       

            this.ClearCache();

            sizes = new int[] { 20, 10, 2, 1 };
            maxTest = 20;

        }

        public void ClearCache()
        {
            reductGeneratorCache = new Dictionary<string, IReductGenerator>();
        }

        public void Run(BenchmarkData benchmarkData)
        {
            DataStoreSplitter splitter;

            this.OpenStream(Path.Combine(@"results", benchmarkData.Name + ".result"));
            
            for (int i = 0; i < maxTest; i++)
            {
                t = i;
                splitter = null;
                if (benchmarkData.CrossValidationActive)
                {
                    data = DataStore.Load(benchmarkData.DataFile, benchmarkData.FileFormat);

                    if (benchmarkData.DecisionFieldId > 0)
                        data.SetDecisionFieldId(benchmarkData.DecisionFieldId);

                    splitter = new DataStoreSplitter(data, benchmarkData.CrossValidationFolds);
                }

                for (int f = 0; f < benchmarkData.CrossValidationFolds; f++)
                {
                    fold = f;

                    if (splitter != null)
                    {
                        splitter.ActiveFold = f;
                        splitter.Split(ref trainData, ref testData);

                        WeightGenerator weightGenerator = new WeightGeneratorMajority(trainData);
                        trainData.SetWeights(weightGenerator.Weights);
                    }
                    else
                    {
                        trainData = DataStore.Load(benchmarkData.TrainFile, benchmarkData.FileFormat);

                        if (benchmarkData.DecisionFieldId > 0)
                            trainData.SetDecisionFieldId(benchmarkData.DecisionFieldId);

                        WeightGenerator weightGenerator = new WeightGeneratorMajority(trainData);
                        trainData.SetWeights(weightGenerator.Weights);

                        testData = DataStore.Load(benchmarkData.TestFile, benchmarkData.FileFormat, trainData.DataStoreInfo);

                    }

                    for (int e = 0; e < 100; e++)
                    {                       
                        eps = (decimal)e / (decimal)100;

                        ensembleSize = sizes.First();
                        permutationSize = ensembleSize * ratio;
                        this.InitPermutation();

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
                                
                                for (int j = 0; j < 10; j++)
                                {

                                    RuleQualityFunction v1, v2;
                                    switch (j)
                                    {
                                        case 0: v1 = RuleQualityAvg.Confidence; v2 = RuleQuality.Confidence; break;
                                        case 1: v1 = RuleQualityAvg.ConfidenceRelativeW; v2 = RuleQuality.ConfidenceRelativeW; break;
                                        case 2: v1 = RuleQualityAvg.ConfidenceW; v2 = RuleQuality.ConfidenceW; break;
                                        case 3: v1 = RuleQualityAvg.Coverage; v2 = RuleQuality.Coverage; break;
                                        case 4: v1 = RuleQualityAvg.CoverageW; v2 = RuleQuality.CoverageW; break;
                                        case 5: v1 = RuleQualityAvg.Ratio; v2 = RuleQuality.Ratio; break;
                                        case 6: v1 = RuleQualityAvg.RatioW; v2 = RuleQuality.RatioW; break;
                                        case 7: v1 = RuleQualityAvg.Support; v2 = RuleQuality.Support; break;
                                        case 8: v1 = RuleQualityAvg.SupportW; v2 = RuleQuality.SupportW; break;
                                        case 9: v1 = RuleQuality.SingleVote; v2 = RuleQuality.SingleVote; break;

                                        default: v1 = RuleQuality.SingleVote; v2 = RuleQuality.SingleVote; break;
                                    }                                                                                                           
                                    
                                    this.MajorityGeneralizedDecisionNoExceptionsPerformanceTest(weightGenerator, RuleQualityAvg.ConfidenceW, v1);
                                    this.MajorityGeneralizedDecisionGapsPerformanceTest(weightGenerator, RuleQualityAvg.ConfidenceW, v1);
                                    this.MajorityGeneralizedDecisionPerformanceTest(weightGenerator, RuleQualityAvg.ConfidenceW, v1);
                                    this.ApproximateDecisionReduct(weightGenerator, RuleQuality.ConfidenceW, v2);
                                }

                                Console.WriteLine();
                            }
                        }
                    }
                }
            }

            this.CloseStream();
        }

        public void InitPermutation()
        {
            Args permParm = new Args();
            permParm.SetParameter(ReductGeneratorParamHelper.TrainData, trainData);
            permParm.SetParameter(ReductGeneratorParamHelper.FactoryKey, ReductFactoryKeyHelper.ApproximateReductMajority);
            IPermutationGenerator permGen = ReductFactory.GetPermutationGenerator(permParm);
            permList = permGen.Generate(permutationSize);
        }

        public void OpenStream(string path)
        {
            fileStream = new StreamWriter(path, false);

            this.WriteLine("{0}|{1}|{2}|{3}|{4}|{5}|{6}|{7}|{8}", "Factory", "Test", "Fold", "EnsembleSize", "Epsilon", "Ident", "Vote", "Weight", ClassificationResult.ResultHeader());
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
