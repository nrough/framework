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
        
        static void Main(string[] args)
        {
            Program program = new Program();
            program.Init();

            Console.WriteLine("{0}|{1}", "Factory", ClassificationResult.ResultHeader());

            for (int i = 0; i < 100; i++)
            {
                program.InitPermutation();

                //program.MajorityGeneralizedDecisionNoExceptionsPerformanceTest();
                program.MajorityGeneralizedDecisionGapsPerformanceTest();                
                //program.MajorityGeneralizedDecisionPerformanceTest();                
                //program.ApproximateDecisionReduct();                

                //Console.ReadKey();
            }            
              

            Console.ReadKey();
        }

        public void Init()
        {                       
            trainData = DataStore.Load(@"Data\dna.train", FileFormat.Rses1);
            
            weightGenerator = new WeightGeneratorMajority(trainData);
            trainData.SetWeights(weightGenerator.Weights);
            
            testData = DataStore.Load(@"Data\dna.test", FileFormat.Rses1, trainData.DataStoreInfo);
                        
            eps = 0.05m;
            ensembleSize = 1;
            ratio = 1;
            permutationSize = ensembleSize * ratio;
            
            reductLengthComparer = new ReductLengthComparer();
            reductStoreLengthComparer = new ReductStoreLengthComparer(true);
            reductMeasureLength = new ReductMeasureLength();            
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
                RuleQuality.AvgCoverageW,
                RuleQuality.AvgCoverageW,
                trainData.DataStoreInfo.GetDecisionValues());
            classifier.UseExceptionRules = false;
            ClassificationResult result = classifier.Classify(testData);
            result.QualityRatio = filteredReductStoreCollection.GetAvgMeasure(reductMeasureLength, false);
            result.ModelCreationTime = generator.ReductGenerationTime;
            result.ClassificationTime = classifier.ClassificationTime;

            
            Console.WriteLine("{0,5}|{1}", "GMDR", result);

            classifier = new RoughClassifier(
                filteredReductStoreCollection,
                RuleQuality.CoverageW,
                RuleQuality.CoverageW,
                trainData.DataStoreInfo.GetDecisionValues());
            classifier.UseExceptionRules = false;
            result = classifier.Classify(testData);
            result.QualityRatio = filteredReductStoreCollection.GetAvgMeasure(reductMeasureLength, false);
            result.ModelCreationTime = generator.ReductGenerationTime;
            result.ClassificationTime = classifier.ClassificationTime;


            Console.WriteLine("{0,5}|{1}", "GMDR+", result);
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
                RuleQuality.AvgCoverageW,
                RuleQuality.AvgCoverageW,
                trainData.DataStoreInfo.GetDecisionValues());

            classifier.UseExceptionRules = false;
            classifier.ExceptionRulesAsGaps = false;

            ClassificationResult result = classifier.Classify(testData);
            result.QualityRatio = filteredReductStoreCollection.GetAvgMeasure(reductMeasureLength, false);
            result.ModelCreationTime = generator.ReductGenerationTime;
            result.ClassificationTime = classifier.ClassificationTime;


            Console.WriteLine("{0,5}|{1}", "NOEX", result);
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
                RuleQuality.AvgCoverageW,
                RuleQuality.AvgCoverageW,
                trainData.DataStoreInfo.GetDecisionValues());
            
            classifier.UseExceptionRules = true;
            classifier.ExceptionRulesAsGaps = true;

            ClassificationResult result = classifier.Classify(testData);
            result.QualityRatio = filteredReductStoreCollection.GetAvgMeasure(reductMeasureLength, false);
            result.ModelCreationTime = generator.ReductGenerationTime;
            result.ClassificationTime = classifier.ClassificationTime;


            Console.WriteLine("{0,5}|{1}", "GAPS", result);

            classifier = new RoughClassifier(
                filteredReductStoreCollection,
                RuleQuality.CoverageW,
                RuleQuality.CoverageW,
                trainData.DataStoreInfo.GetDecisionValues());

            classifier.UseExceptionRules = true;
            classifier.ExceptionRulesAsGaps = true;

            result = classifier.Classify(testData);
            result.QualityRatio = filteredReductStoreCollection.GetAvgMeasure(reductMeasureLength, false);
            result.ModelCreationTime = generator.ReductGenerationTime;
            result.ClassificationTime = classifier.ClassificationTime;


            Console.WriteLine("{0,5}|{1}", "GAPS+", result);

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
                RuleQuality.AvgCoverageW,
                RuleQuality.AvgCoverageW,
                trainData.DataStoreInfo.GetDecisionValues());

            classifier.UseExceptionRules = true;
            classifier.ExceptionRulesAsGaps = false;

            result = classifier.Classify(testData);
            result.QualityRatio = filteredReductStoreCollection.GetAvgMeasure(reductMeasureLength, false);
            result.ModelCreationTime = generator.ReductGenerationTime;
            result.ClassificationTime = classifier.ClassificationTime;


            Console.WriteLine("{0,5}|{1}", "EXEP", result);            

            classifier = new RoughClassifier(
                filteredReductStoreCollection,
                RuleQuality.CoverageW,
                RuleQuality.CoverageW,
                trainData.DataStoreInfo.GetDecisionValues());

            classifier.UseExceptionRules = true;
            classifier.ExceptionRulesAsGaps = false;

            result = classifier.Classify(testData);
            result.QualityRatio = filteredReductStoreCollection.GetAvgMeasure(reductMeasureLength, false);
            result.ModelCreationTime = generator.ReductGenerationTime;
            result.ClassificationTime = classifier.ClassificationTime;


            Console.WriteLine("{0,5}|{1}", "EXEP+", result);
        }        

        public void ApproximateDecisionReduct()
        {
            Args parmsApprox = new Args();
            parmsApprox.SetParameter(ReductGeneratorParamHelper.TrainData, trainData);
            parmsApprox.SetParameter(ReductGeneratorParamHelper.FactoryKey, ReductFactoryKeyHelper.ApproximateReductMajorityWeights);
            parmsApprox.SetParameter(ReductGeneratorParamHelper.WeightGenerator, weightGenerator);
            parmsApprox.SetParameter(ReductGeneratorParamHelper.Epsilon, eps);            
            parmsApprox.SetParameter(ReductGeneratorParamHelper.PermutationCollection, permList);
            parmsApprox.SetParameter(ReductGeneratorParamHelper.UseExceptionRules, false);

            ReductGeneratorWeightsMajority generatorApprox =
                ReductFactory.GetReductGenerator(parmsApprox) as ReductGeneratorWeightsMajority;
            generatorApprox.Run();
            IReductStoreCollection origReductStoreCollectionApprox = generatorApprox.GetReductStoreCollection();
            IReductStoreCollection filteredReductStoreCollectionApprox = origReductStoreCollectionApprox.Filter(ensembleSize, reductLengthComparer);

            //Console.WriteLine(filteredReductStoreCollectionApprox.FirstOrDefault().FirstOrDefault());

            RoughClassifier classifierApprox = new RoughClassifier(
                filteredReductStoreCollectionApprox,
                RuleQuality.CoverageW,
                RuleQuality.CoverageW,
                trainData.DataStoreInfo.GetDecisionValues());
            classifierApprox.UseExceptionRules = false;
            ClassificationResult resultApprox = classifierApprox.Classify(testData);
            resultApprox.QualityRatio = filteredReductStoreCollectionApprox.GetAvgMeasure(reductMeasureLength, false);
            resultApprox.ModelCreationTime = generatorApprox.ReductGenerationTime;
            resultApprox.ClassificationTime = classifierApprox.ClassificationTime;
            
            Console.WriteLine("{0,5}|{1}", "ADR", resultApprox);
        }
    }
}
