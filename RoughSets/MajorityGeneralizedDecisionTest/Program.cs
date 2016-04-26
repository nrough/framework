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
        PermutationCollection permList;
        
        static void Main(string[] args)
        {
            Program program = new Program();
            program.Init();

            Console.WriteLine("{0}|{1}", "Factory", ClassificationResult.ResultHeader());

            for (int i = 0; i < 100; i++)
            {
                program.InitPermutation();
                
                program.ApproximateDecisionReduct();
                program.MajorityGeneralizedDecisionPerformanceTest();

                //Console.ReadKey();
            }            
              

            Console.ReadKey();
        }

        public void Init()
        {
            //trainData = DataStore.Load(@"Data\dna_modified.trn", FileFormat.Rses1);
            //testData = DataStore.Load(@"Data\dna_modified.tst", FileFormat.Rses1);
            
            trainData = DataStore.Load(@"Data\dna.test", FileFormat.Rses1);
            
            weightGenerator = new WeightGeneratorMajority(trainData);            
            trainData.SetWeights(weightGenerator.Weights);

            testData = DataStore.Load(@"Data\dna.train", FileFormat.Rses1);
            
            

            eps = 0;
            ensembleSize = 10;
            ratio = 10;
            permutationSize = ensembleSize * ratio;
            
            reductLengthComparer = new ReductLengthComparer();
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
            Args parms_GMDR = new Args();
            parms_GMDR.SetParameter(ReductGeneratorParamHelper.TrainData, trainData);
            parms_GMDR.SetParameter(ReductGeneratorParamHelper.FactoryKey, ReductFactoryKeyHelper.GeneralizedMajorityDecision);
            parms_GMDR.SetParameter(ReductGeneratorParamHelper.WeightGenerator, weightGenerator);
            parms_GMDR.SetParameter(ReductGeneratorParamHelper.Epsilon, eps);
            parms_GMDR.SetParameter(ReductGeneratorParamHelper.PermutationCollection, permList);
            parms_GMDR.SetParameter(ReductGeneratorParamHelper.UseExceptionRules, false);

            ReductGeneralizedMajorityDecisionGenerator generator_GMDR =
                ReductFactory.GetReductGenerator(parms_GMDR) as ReductGeneralizedMajorityDecisionGenerator;
            generator_GMDR.Run();
            IReductStoreCollection origReductStoreCollection_GMDR = generator_GMDR.GetReductStoreCollection();
            IReductStoreCollection filteredReductStoreCollection_GMDR = origReductStoreCollection_GMDR.Filter(ensembleSize, reductLengthComparer);

            //Console.WriteLine(filteredReductStoreCollection_GMDR.FirstOrDefault().FirstOrDefault());

            RoughClassifier classifier_GMDR = new RoughClassifier(
                filteredReductStoreCollection_GMDR,
                RuleQuality.CoverageW,
                RuleQuality.CoverageW,
                trainData.DataStoreInfo.GetDecisionValues());
            classifier_GMDR.UseExceptionRules = false;
            ClassificationResult result_GMDR = classifier_GMDR.Classify(testData);
            result_GMDR.QualityRatio = filteredReductStoreCollection_GMDR.GetAvgMeasure(reductMeasureLength, false);
            result_GMDR.ModelCreationTime = generator_GMDR.ReductGenerationTime;
            result_GMDR.ClassificationTime = classifier_GMDR.ClassificationTime;

            
            Console.WriteLine("{0,4}|{1}", "GMDR", result_GMDR);
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
            
            Console.WriteLine("{0,4}|{1}", "ADR", resultApprox);
        }
    }
}
