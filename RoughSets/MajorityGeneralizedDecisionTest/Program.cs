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
        DataStore trainData;
        DataStore testData;
        DataStore data;
        WeightGenerator weightGenerator;
        decimal eps;
        int ensembleSize;
        int ratio;
        int permutationSize;
        ReductLengthComparer reductLengthComparer;
        ReductMeasureLength reductMeasureLength;
        ReductStoreLengthComparer reductStoreLengthComparer;
        PermutationCollection permList;
        RuleQualityFunction identification, identificationPlus;
        RuleQualityFunction voting, votingPlus;
        int t;
        StreamWriter fileStream;
        Dictionary<string, IReductGenerator> reductGeneratorCache;
        int[] sizes;
        int maxTest;
        int fold;
        
        public void MajorityGeneralizedDecisionPerformanceTest()
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

            //Console.WriteLine(filteredReductStoreCollection.FirstOrDefault().FirstOrDefault());

            RoughClassifier classifier = new RoughClassifier(
                filteredReductStoreCollection,
                identification,
                identification,
                trainData.DataStoreInfo.GetDecisionValues());
            classifier.UseExceptionRules = false;
            ClassificationResult result = classifier.Classify(testData);
            result.QualityRatio = filteredReductStoreCollection.GetAvgMeasure(reductMeasureLength, false);
            result.ModelCreationTime = generator.ReductGenerationTime;
            result.ClassificationTime = classifier.ClassificationTime;

            this.WriteLine("{0,5}|{1}|{2}|{3,2}|{4,4}|{5}", "GMDR", t, fold, ensembleSize, eps, result);

            classifier = new RoughClassifier(
                filteredReductStoreCollection,
                identificationPlus,
                identificationPlus,
                trainData.DataStoreInfo.GetDecisionValues());
            classifier.UseExceptionRules = false;
            result = classifier.Classify(testData);
            result.QualityRatio = filteredReductStoreCollection.GetAvgMeasure(reductMeasureLength, false);
            result.ModelCreationTime = generator.ReductGenerationTime;
            result.ClassificationTime = classifier.ClassificationTime;

            this.WriteLine("{0,5}|{1}|{2}|{3,2}|{4,4}|{5}", "GMDR+", t, fold, ensembleSize, eps, result);
        }        

        public void MajorityGeneralizedDecisionNoExceptionsPerformanceTest()
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

            //Console.WriteLine(filteredReductStoreCollection.FirstOrDefault().FirstOrDefault());

            RoughClassifier classifier = new RoughClassifier(
                filteredReductStoreCollection,
                identification,
                identification,
                trainData.DataStoreInfo.GetDecisionValues());

            classifier.UseExceptionRules = false;
            classifier.ExceptionRulesAsGaps = false;

            ClassificationResult result = classifier.Classify(testData);
            result.QualityRatio = filteredReductStoreCollection.GetAvgMeasure(reductMeasureLength, false);
            result.ModelCreationTime = generator.ReductGenerationTime;
            result.ClassificationTime = classifier.ClassificationTime;

            this.WriteLine("{0,5}|{1}|{2}|{3,2}|{4,4}|{5}", "NOEX", t, fold, ensembleSize, eps, result);

        }

        public void MajorityGeneralizedDecisionGapsPerformanceTest()
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

            //Console.WriteLine(filteredReductStoreCollection.FirstOrDefault().FirstOrDefault());

            RoughClassifier classifier = new RoughClassifier(
                filteredReductStoreCollection,
                identification,
                identification,
                trainData.DataStoreInfo.GetDecisionValues());
            
            classifier.UseExceptionRules = true;
            classifier.ExceptionRulesAsGaps = true;

            ClassificationResult result = classifier.Classify(testData);
            result.QualityRatio = filteredReductStoreCollection.GetAvgMeasure(reductMeasureLength, true);
            result.ModelCreationTime = generator.ReductGenerationTime;
            result.ClassificationTime = classifier.ClassificationTime;

            this.WriteLine("{0,5}|{1}|{2}|{3,2}|{4,4}|{5}", "GAPS", t, fold, ensembleSize, eps, result);

            classifier = new RoughClassifier(
                filteredReductStoreCollection,
                identificationPlus,
                identificationPlus,
                trainData.DataStoreInfo.GetDecisionValues());

            classifier.UseExceptionRules = true;
            classifier.ExceptionRulesAsGaps = true;

            result = classifier.Classify(testData);
            result.QualityRatio = filteredReductStoreCollection.GetAvgMeasure(reductMeasureLength, true);
            result.ModelCreationTime = generator.ReductGenerationTime;
            result.ClassificationTime = classifier.ClassificationTime;

            this.WriteLine("{0,5}|{1}|{2}|{3,2}|{4,4}|{5}", "GAPS+", t, fold, ensembleSize, eps, result);

            /*
            foreach (var reductStore in filteredReductStoreCollection)
            {
                foreach (var reduct in reductStore)
                {
                    foreach (var eq in reduct.EquivalenceClasses)
                    {
                        if (eq.AvgConfidenceWeight != eq.DecisionWeights.FindMaxValuePair().Value)
                        {
                            Console.WriteLine("{0}={1}", "AvgConfidenceWeigth", eq.AvgConfidenceWeight);
                            foreach (long dec in eq.DecisionSet)
                            {
                                Console.WriteLine("{0}={1}", "Dec", eq.GetDecisionWeight(dec));
                            }
                        }
                    }
                }
            }
            */

            classifier = new RoughClassifier(
                filteredReductStoreCollection,
                identification,
                identification,
                trainData.DataStoreInfo.GetDecisionValues());

            classifier.UseExceptionRules = true;
            classifier.ExceptionRulesAsGaps = false;

            result = classifier.Classify(testData);
            result.QualityRatio = filteredReductStoreCollection.GetAvgMeasure(reductMeasureLength, true);
            result.ModelCreationTime = generator.ReductGenerationTime;
            result.ClassificationTime = classifier.ClassificationTime;

            this.WriteLine("{0,5}|{1}|{2}|{3,2}|{4,4}|{5}", "EXEP", t, fold, ensembleSize, eps, result);           

            classifier = new RoughClassifier(
                filteredReductStoreCollection,
                identificationPlus,
                identificationPlus,
                trainData.DataStoreInfo.GetDecisionValues());

            classifier.UseExceptionRules = true;
            classifier.ExceptionRulesAsGaps = false;

            result = classifier.Classify(testData);
            result.QualityRatio = filteredReductStoreCollection.GetAvgMeasure(reductMeasureLength, true);
            result.ModelCreationTime = generator.ReductGenerationTime;
            result.ClassificationTime = classifier.ClassificationTime;

            this.WriteLine("{0,5}|{1}|{2}|{3,2}|{4,4}|{5}", "EXEP+", t, fold, ensembleSize, eps, result);

        }

        public void ApproximateDecisionReduct()
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

            //Console.WriteLine(filteredReductStoreCollectionApprox.FirstOrDefault().FirstOrDefault());

            RoughClassifier classifier = new RoughClassifier(
                filteredReductStoreCollectionApprox,
                identificationPlus,
                identificationPlus,
                trainData.DataStoreInfo.GetDecisionValues());
            classifier.UseExceptionRules = false;
            ClassificationResult result = classifier.Classify(testData);
            result.QualityRatio = filteredReductStoreCollectionApprox.GetAvgMeasure(reductMeasureLength, false);
            result.ModelCreationTime = generator.ReductGenerationTime;
            result.ClassificationTime = classifier.ClassificationTime;

            this.WriteLine(String.Format("{0,5}|{1}|{2}|{3,2}|{4,4}|{5}", "ADR+", t, fold, ensembleSize, eps, result));
        }



        static void Main(string[] args)
        {
            string[] names = args;

            foreach (var kvp in BenchmarkDataHelper.GetDataFiles(names: names))
            {
                Program program = new Program();
                program.Init(kvp.Value);
                program.Run(kvp.Value);
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

            identification = RuleQuality.AvgConfidenceW;
            identificationPlus = RuleQuality.AvgConfidenceW;
            voting = RuleQuality.ConfidenceW;
            votingPlus = RuleQuality.ConfidenceW;

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
                    splitter = new DataStoreSplitter(data, benchmarkData.CrossValidationFolds);
                }

                for (int f = 0; f < benchmarkData.CrossValidationFolds; f++)
                {
                    fold = f;

                    if (splitter != null)
                    {
                        splitter.ActiveFold = f;
                        splitter.Split(ref trainData, ref testData);

                        weightGenerator = new WeightGeneratorMajority(trainData);
                        trainData.SetWeights(weightGenerator.Weights);
                    }
                    else
                    {
                        trainData = DataStore.Load(benchmarkData.TrainFile, benchmarkData.FileFormat);

                        weightGenerator = new WeightGeneratorMajority(trainData);
                        trainData.SetWeights(weightGenerator.Weights);

                        testData = DataStore.Load(benchmarkData.TestFile, benchmarkData.FileFormat, trainData.DataStoreInfo);

                    }

                    for (int e = 0; e < 100; e++)
                    {
                        ClearCache();

                        eps = (decimal)e / (decimal)100;

                        ensembleSize = sizes.First();
                        permutationSize = ensembleSize * ratio;

                        this.InitPermutation();

                        foreach (int size in sizes)
                        {
                            ensembleSize = size;
                            permutationSize = ensembleSize * ratio;

                            this.MajorityGeneralizedDecisionNoExceptionsPerformanceTest();
                            this.MajorityGeneralizedDecisionGapsPerformanceTest();
                            this.MajorityGeneralizedDecisionPerformanceTest();
                            this.ApproximateDecisionReduct();

                            Console.WriteLine();
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

            //Console.WriteLine(permList);
        }

        public void OpenStream(string path)
        {
            fileStream = new StreamWriter(path, false);

            this.WriteLine("{0}|{1}|{2}|{3}|{4}|{5}", "Factory", "Test", "Fold", "EnsembleSize", "Epsilon", ClassificationResult.ResultHeader());
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
