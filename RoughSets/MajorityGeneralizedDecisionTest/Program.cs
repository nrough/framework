using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Infovision.Data;
using Infovision.Datamining;
using Infovision.Datamining.Roughset;
using Infovision.Utils;

namespace MajorityGeneralizedDecisionTest
{
    class Program
    {
        DataStore trainData;
        DataStore testData;
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

        
        static void Main(string[] args)
        {
            Program program = new Program();
            program.Init();

            Console.WriteLine("{0}|{1}|{2}|{3}|{4}", "Factory", "Test", "EnsembleSize", "Epsilon", ClassificationResult.ResultHeader());


            program.Run();

            Console.ReadKey();
        }

        public void Init()
        {                       
            trainData = DataStore.Load(@"Data\dna.train", FileFormat.Rses1);
            
            weightGenerator = new WeightGeneratorMajority(trainData);
            trainData.SetWeights(weightGenerator.Weights);
            
            testData = DataStore.Load(@"Data\dna.test", FileFormat.Rses1, trainData.DataStoreInfo);
                        
            eps = 0.17m;
            ensembleSize = 10;
            ratio = 10;
            permutationSize = ensembleSize * ratio;
            
            reductLengthComparer = new ReductLengthComparer();
            reductStoreLengthComparer = new ReductStoreLengthComparer(true);
            reductMeasureLength = new ReductMeasureLength();

            identification = RuleQuality.AvgConfidenceW; 
            identificationPlus = RuleQuality.AvgConfidenceW;
            voting =  RuleQuality.ConfidenceW;
            votingPlus =  RuleQuality.ConfidenceW;
        }

        public void Run()
        {
            int[] sizes = new int[] {1, 2, 10, 20};

            for (int i = 0; i < 20; i++)
            {
                t = i;
                
                foreach (int size in sizes)
                {
                    ensembleSize = size;
                    permutationSize = ensembleSize * ratio;

                    this.InitPermutation();

                    for (int e = 0; e < 100; e++)
                    {
                        eps = (decimal)e / (decimal)100;

                        this.MajorityGeneralizedDecisionNoExceptionsPerformanceTest();
                        this.MajorityGeneralizedDecisionGapsPerformanceTest();
                        this.MajorityGeneralizedDecisionPerformanceTest();
                        this.ApproximateDecisionReduct();

                        //Console.WriteLine();
                        //Console.ReadKey();
                    }
                }
            }
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

        
        public void MajorityGeneralizedDecisionPerformanceTest()
        {            
            Args parms = new Args();
            parms.SetParameter(ReductGeneratorParamHelper.TrainData, trainData);
            parms.SetParameter(ReductGeneratorParamHelper.FactoryKey, ReductFactoryKeyHelper.GeneralizedMajorityDecision);
            parms.SetParameter(ReductGeneratorParamHelper.WeightGenerator, weightGenerator);
            parms.SetParameter(ReductGeneratorParamHelper.Epsilon, eps);
            parms.SetParameter(ReductGeneratorParamHelper.PermutationCollection, permList);
            parms.SetParameter(ReductGeneratorParamHelper.UseExceptionRules, false);

            ReductGeneralizedMajorityDecisionGenerator generator =
                ReductFactory.GetReductGenerator(parms) as ReductGeneralizedMajorityDecisionGenerator;
            generator.Run();
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

            Console.WriteLine("{0,5}|{1}|{2,2}|{3,4}|{4}", "GMDR", t, ensembleSize, eps, result);

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

            Console.WriteLine("{0,5}|{1}|{2,2}|{3,4}|{4}", "GMDR+", t, ensembleSize, eps, result);
        }        

        public void MajorityGeneralizedDecisionNoExceptionsPerformanceTest()
        {
            Args parms = new Args();
            parms.SetParameter(ReductGeneratorParamHelper.TrainData, trainData);
            parms.SetParameter(ReductGeneratorParamHelper.FactoryKey, ReductFactoryKeyHelper.GeneralizedMajorityDecisionApproximate);
            parms.SetParameter(ReductGeneratorParamHelper.WeightGenerator, weightGenerator);
            parms.SetParameter(ReductGeneratorParamHelper.Epsilon, eps);
            parms.SetParameter(ReductGeneratorParamHelper.PermutationCollection, permList);
            parms.SetParameter(ReductGeneratorParamHelper.UseExceptionRules, false);

            ReductGeneralizedMajorityDecisionGenerator generator =
                ReductFactory.GetReductGenerator(parms) as ReductGeneralizedMajorityDecisionGenerator;
            generator.Run();
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

            Console.WriteLine("{0,5}|{1}|{2,2}|{3,4}|{4}", "NOEX", t, ensembleSize, eps, result);

        }

        public void MajorityGeneralizedDecisionGapsPerformanceTest()
        {
            Args parms = new Args();
            parms.SetParameter(ReductGeneratorParamHelper.TrainData, trainData);
            parms.SetParameter(ReductGeneratorParamHelper.FactoryKey, ReductFactoryKeyHelper.GeneralizedMajorityDecisionApproximate);
            parms.SetParameter(ReductGeneratorParamHelper.WeightGenerator, weightGenerator);
            parms.SetParameter(ReductGeneratorParamHelper.Epsilon, eps);
            parms.SetParameter(ReductGeneratorParamHelper.PermutationCollection, permList);
            parms.SetParameter(ReductGeneratorParamHelper.UseExceptionRules, true);            

            ReductGeneralizedMajorityDecisionGenerator generator =
                ReductFactory.GetReductGenerator(parms) as ReductGeneralizedMajorityDecisionGenerator;
            generator.Run();
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
            result.QualityRatio = filteredReductStoreCollection.GetAvgMeasure(reductMeasureLength, false);
            result.ModelCreationTime = generator.ReductGenerationTime;
            result.ClassificationTime = classifier.ClassificationTime;

            Console.WriteLine("{0,5}|{1}|{2,2}|{3,4}|{4}", "GAPS", t, ensembleSize, eps, result);

            classifier = new RoughClassifier(
                filteredReductStoreCollection,
                identificationPlus,
                identificationPlus,
                trainData.DataStoreInfo.GetDecisionValues());

            classifier.UseExceptionRules = true;
            classifier.ExceptionRulesAsGaps = true;

            result = classifier.Classify(testData);
            result.QualityRatio = filteredReductStoreCollection.GetAvgMeasure(reductMeasureLength, false);
            result.ModelCreationTime = generator.ReductGenerationTime;
            result.ClassificationTime = classifier.ClassificationTime;

            Console.WriteLine("{0,5}|{1}|{2,2}|{3,4}|{4}", "GAPS+", t, ensembleSize, eps, result);

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

            classifier = new RoughClassifier(
                filteredReductStoreCollection,
                identification,
                identification,
                trainData.DataStoreInfo.GetDecisionValues());

            classifier.UseExceptionRules = true;
            classifier.ExceptionRulesAsGaps = false;

            result = classifier.Classify(testData);
            result.QualityRatio = filteredReductStoreCollection.GetAvgMeasure(reductMeasureLength, false);
            result.ModelCreationTime = generator.ReductGenerationTime;
            result.ClassificationTime = classifier.ClassificationTime;

            Console.WriteLine("{0,5}|{1}|{2,2}|{3,4}|{4}", "EXEP", t, ensembleSize, eps, result);           

            classifier = new RoughClassifier(
                filteredReductStoreCollection,
                identificationPlus,
                identificationPlus,
                trainData.DataStoreInfo.GetDecisionValues());

            classifier.UseExceptionRules = true;
            classifier.ExceptionRulesAsGaps = false;

            result = classifier.Classify(testData);
            result.QualityRatio = filteredReductStoreCollection.GetAvgMeasure(reductMeasureLength, false);
            result.ModelCreationTime = generator.ReductGenerationTime;
            result.ClassificationTime = classifier.ClassificationTime;

            Console.WriteLine("{0,5}|{1}|{2,2}|{3,4}|{4}", "EXEP+", t, ensembleSize, eps, result);
        }

        public void ApproximateDecisionReduct()
        {
            Args parms = new Args();
            parms.SetParameter(ReductGeneratorParamHelper.TrainData, trainData);
            parms.SetParameter(ReductGeneratorParamHelper.FactoryKey, ReductFactoryKeyHelper.ApproximateReductMajorityWeights);
            parms.SetParameter(ReductGeneratorParamHelper.WeightGenerator, weightGenerator);
            parms.SetParameter(ReductGeneratorParamHelper.Epsilon, eps);            
            parms.SetParameter(ReductGeneratorParamHelper.PermutationCollection, permList);
            parms.SetParameter(ReductGeneratorParamHelper.UseExceptionRules, false);

            ReductGeneratorWeightsMajority generator =
                ReductFactory.GetReductGenerator(parms) as ReductGeneratorWeightsMajority;
            generator.Run();
            IReductStoreCollection origReductStoreCollectionApprox = generator.GetReductStoreCollection();
            IReductStoreCollection filteredReductStoreCollectionApprox = origReductStoreCollectionApprox.Filter(ensembleSize, reductLengthComparer);

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

            Console.WriteLine("{0,5}|{1}|{2,2}|{3,4}|{4}", "ADR+", t, ensembleSize, eps, result);
        }
    }
}
