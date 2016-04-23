using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using NUnit.Framework;
using Infovision.Data;
using Infovision.Datamining.Roughset;
using Infovision.Utils;
using Infovision.Math;
using Infovision.Datamining.Clustering.Hierarchical;
using System.IO;


namespace Infovision.Datamining.Roughset.UnitTests
{
    [TestFixture]
    public class ReductEnsembleBoostingTest
    {
        public ReductEnsembleBoostingTest()
        {
            Random randSeed = new Random();
            int seed = Guid.NewGuid().GetHashCode();
            //Console.WriteLine("class ReductEnsembleBoostingTest Seed: {0}", seed);
            RandomSingleton.Seed = seed;
        }
        
        public static IEnumerable<Dictionary<string, object>> GetGenerateTestArgs()
        {            
            DataStore data = DataStore.Load(@"Data\dna_modified.trn", FileFormat.Rses1);

            List<Dictionary<string, object>> argsList = new List<Dictionary<string, object>>();

            Args inner = new Args();
            inner.SetParameter(ReductGeneratorParamHelper.TrainData, data);
            inner.SetParameter(ReductGeneratorParamHelper.FactoryKey, ReductFactoryKeyHelper.GeneralizedMajorityDecisionApproximate);
            inner.SetParameter(ReductGeneratorParamHelper.NumberOfPermutations, 1);
            inner.SetParameter(ReductGeneratorParamHelper.Epsilon, 0.1m);
            inner.SetParameter(ReductGeneratorParamHelper.WeightGenerator, new WeightGeneratorMajority(data));

            Dictionary<string, object> argSet = new Dictionary<string, object>();
            argSet.Add(ReductGeneratorParamHelper.TrainData, data);
            argSet.Add(ReductGeneratorParamHelper.NumberOfThreads, 1);
            argSet.Add(ReductGeneratorParamHelper.FactoryKey, ReductFactoryKeyHelper.ReductEnsembleBoosting);
            argSet.Add(ReductGeneratorParamHelper.IdentificationType, (RuleQualityFunction)RuleQuality.Confidence);
            argSet.Add(ReductGeneratorParamHelper.VoteType, (RuleQualityFunction)RuleQuality.SingleVote);
            argSet.Add(ReductGeneratorParamHelper.MaxReductLength, 4);
            argSet.Add(ReductGeneratorParamHelper.Threshold, 0.5);
            argSet.Add(ReductGeneratorParamHelper.NumberOfReductsInWeakClassifier, 1);
            argSet.Add(ReductGeneratorParamHelper.MaxIterations, 1);
            argSet.Add(ReductGeneratorParamHelper.InnerParameters, inner);
            argsList.Add(argSet);

            argSet = new Dictionary<string, object>();
            argSet.Add(ReductGeneratorParamHelper.TrainData, data);
            argSet.Add(ReductGeneratorParamHelper.NumberOfThreads, 1);
            argSet.Add(ReductGeneratorParamHelper.FactoryKey, ReductFactoryKeyHelper.ReductEnsembleBoosting);
            argSet.Add(ReductGeneratorParamHelper.IdentificationType, (RuleQualityFunction)RuleQuality.Confidence);
            argSet.Add(ReductGeneratorParamHelper.VoteType, (RuleQualityFunction)RuleQuality.SingleVote);
            argSet.Add(ReductGeneratorParamHelper.MaxReductLength, 4);
            argSet.Add(ReductGeneratorParamHelper.Threshold, 0.5);
            argSet.Add(ReductGeneratorParamHelper.NumberOfReductsInWeakClassifier, 1);
            argSet.Add(ReductGeneratorParamHelper.MaxIterations, 3);
            argSet.Add(ReductGeneratorParamHelper.InnerParameters, inner);
            argsList.Add(argSet);

            argSet = new Dictionary<string, object>();
            argSet.Add(ReductGeneratorParamHelper.TrainData, data);
            argSet.Add(ReductGeneratorParamHelper.NumberOfThreads, 1);
            argSet.Add(ReductGeneratorParamHelper.FactoryKey, ReductFactoryKeyHelper.ReductEnsembleBoosting);
            argSet.Add(ReductGeneratorParamHelper.IdentificationType, (RuleQualityFunction)RuleQuality.Confidence);
            argSet.Add(ReductGeneratorParamHelper.VoteType, (RuleQualityFunction)RuleQuality.SingleVote);
            argSet.Add(ReductGeneratorParamHelper.MaxReductLength, 4);
            argSet.Add(ReductGeneratorParamHelper.Threshold, 0.5);
            argSet.Add(ReductGeneratorParamHelper.NumberOfReductsInWeakClassifier, 1);
            argSet.Add(ReductGeneratorParamHelper.MaxIterations, 5);
            argSet.Add(ReductGeneratorParamHelper.InnerParameters, inner);
            argsList.Add(argSet);


            argSet = new Dictionary<string, object>();
            argSet.Add(ReductGeneratorParamHelper.TrainData, data);
            argSet.Add(ReductGeneratorParamHelper.NumberOfThreads, 1);                                    
            argSet.Add(ReductGeneratorParamHelper.FactoryKey, ReductFactoryKeyHelper.ReductEnsembleBoosting);
            argSet.Add(ReductGeneratorParamHelper.IdentificationType, (RuleQualityFunction)RuleQuality.Confidence);
            argSet.Add(ReductGeneratorParamHelper.VoteType, (RuleQualityFunction)RuleQuality.SingleVote);
            argSet.Add(ReductGeneratorParamHelper.MaxReductLength, 4);
            argSet.Add(ReductGeneratorParamHelper.Threshold, 0.5);
            argSet.Add(ReductGeneratorParamHelper.NumberOfReductsInWeakClassifier, 1);
            argSet.Add(ReductGeneratorParamHelper.MaxIterations, 10);
            argSet.Add(ReductGeneratorParamHelper.InnerParameters, inner);
            argsList.Add(argSet);

            argSet = new Dictionary<string, object>();
            argSet.Add(ReductGeneratorParamHelper.TrainData, data);
            argSet.Add(ReductGeneratorParamHelper.NumberOfThreads, 1);
            argSet.Add(ReductGeneratorParamHelper.FactoryKey, ReductFactoryKeyHelper.ReductEnsembleBoosting);
            argSet.Add(ReductGeneratorParamHelper.IdentificationType, (RuleQualityFunction)RuleQuality.Confidence);
            argSet.Add(ReductGeneratorParamHelper.VoteType, (RuleQualityFunction)RuleQuality.SingleVote);
            argSet.Add(ReductGeneratorParamHelper.MaxReductLength, 4);
            argSet.Add(ReductGeneratorParamHelper.Threshold, 0.5);
            argSet.Add(ReductGeneratorParamHelper.NumberOfReductsInWeakClassifier, 1);
            argSet.Add(ReductGeneratorParamHelper.MaxIterations, 20);
            argSet.Add(ReductGeneratorParamHelper.InnerParameters, inner);
            argsList.Add(argSet);

            
            argSet = new Dictionary<string, object>();
            argSet.Add(ReductGeneratorParamHelper.TrainData, data);
            argSet.Add(ReductGeneratorParamHelper.FactoryKey, ReductFactoryKeyHelper.ReductEnsembleBoosting);
            argSet.Add(ReductGeneratorParamHelper.IdentificationType, (RuleQualityFunction)RuleQuality.Confidence);
            argSet.Add(ReductGeneratorParamHelper.VoteType, (RuleQualityFunction)RuleQuality.SingleVote);
            argSet.Add(ReductGeneratorParamHelper.MaxReductLength, 4);
            argSet.Add(ReductGeneratorParamHelper.Threshold, 0.5);
            argSet.Add(ReductGeneratorParamHelper.NumberOfReductsInWeakClassifier, 1);
            argSet.Add(ReductGeneratorParamHelper.MaxIterations, 30);
            argSet.Add(ReductGeneratorParamHelper.InnerParameters, inner);
            argsList.Add(argSet);

            argSet = new Dictionary<string, object>();
            argSet.Add(ReductGeneratorParamHelper.TrainData, data);
            argSet.Add(ReductGeneratorParamHelper.FactoryKey, ReductFactoryKeyHelper.ReductEnsembleBoosting);
            argSet.Add(ReductGeneratorParamHelper.IdentificationType, (RuleQualityFunction)RuleQuality.Confidence);
            argSet.Add(ReductGeneratorParamHelper.VoteType, (RuleQualityFunction)RuleQuality.SingleVote);
            argSet.Add(ReductGeneratorParamHelper.MaxReductLength, 4);
            argSet.Add(ReductGeneratorParamHelper.Threshold, 0.5);
            argSet.Add(ReductGeneratorParamHelper.NumberOfReductsInWeakClassifier, 1);
            argSet.Add(ReductGeneratorParamHelper.MaxIterations, 40);
            argSet.Add(ReductGeneratorParamHelper.InnerParameters, inner);
            argsList.Add(argSet);

            argSet = new Dictionary<string, object>();
            argSet.Add(ReductGeneratorParamHelper.TrainData, data);
            argSet.Add(ReductGeneratorParamHelper.FactoryKey, ReductFactoryKeyHelper.ReductEnsembleBoosting);
            argSet.Add(ReductGeneratorParamHelper.IdentificationType, (RuleQualityFunction)RuleQuality.Confidence);
            argSet.Add(ReductGeneratorParamHelper.VoteType, (RuleQualityFunction)RuleQuality.SingleVote);
            argSet.Add(ReductGeneratorParamHelper.MaxReductLength, 4);
            argSet.Add(ReductGeneratorParamHelper.Threshold, 0.5);
            argSet.Add(ReductGeneratorParamHelper.NumberOfReductsInWeakClassifier, 1);
            argSet.Add(ReductGeneratorParamHelper.MaxIterations, 50);
            argSet.Add(ReductGeneratorParamHelper.InnerParameters, inner);
            argsList.Add(argSet);

            argSet = new Dictionary<string, object>();
            argSet.Add(ReductGeneratorParamHelper.TrainData, data);
            argSet.Add(ReductGeneratorParamHelper.FactoryKey, ReductFactoryKeyHelper.ReductEnsembleBoosting);
            argSet.Add(ReductGeneratorParamHelper.IdentificationType, (RuleQualityFunction)RuleQuality.Confidence);
            argSet.Add(ReductGeneratorParamHelper.VoteType, (RuleQualityFunction)RuleQuality.SingleVote);
            argSet.Add(ReductGeneratorParamHelper.MaxReductLength, 4);
            argSet.Add(ReductGeneratorParamHelper.Threshold, 0.5);
            argSet.Add(ReductGeneratorParamHelper.NumberOfReductsInWeakClassifier, 1);
            argSet.Add(ReductGeneratorParamHelper.MaxIterations, 60);
            argSet.Add(ReductGeneratorParamHelper.InnerParameters, inner);
            argsList.Add(argSet);

            argSet = new Dictionary<string, object>();
            argSet.Add(ReductGeneratorParamHelper.TrainData, data);
            argSet.Add(ReductGeneratorParamHelper.FactoryKey, ReductFactoryKeyHelper.ReductEnsembleBoosting);
            argSet.Add(ReductGeneratorParamHelper.IdentificationType, (RuleQualityFunction)RuleQuality.Confidence);
            argSet.Add(ReductGeneratorParamHelper.VoteType, (RuleQualityFunction)RuleQuality.SingleVote);
            argSet.Add(ReductGeneratorParamHelper.MaxReductLength, 4);
            argSet.Add(ReductGeneratorParamHelper.Threshold, 0.5);
            argSet.Add(ReductGeneratorParamHelper.NumberOfReductsInWeakClassifier, 1);
            argSet.Add(ReductGeneratorParamHelper.MaxIterations, 70);
            argSet.Add(ReductGeneratorParamHelper.InnerParameters, inner);
            argsList.Add(argSet);

            argSet = new Dictionary<string, object>();
            argSet.Add(ReductGeneratorParamHelper.TrainData, data);
            argSet.Add(ReductGeneratorParamHelper.FactoryKey, ReductFactoryKeyHelper.ReductEnsembleBoosting);
            argSet.Add(ReductGeneratorParamHelper.IdentificationType, (RuleQualityFunction)RuleQuality.Confidence);
            argSet.Add(ReductGeneratorParamHelper.VoteType, (RuleQualityFunction)RuleQuality.SingleVote);
            argSet.Add(ReductGeneratorParamHelper.MaxReductLength, 4);
            argSet.Add(ReductGeneratorParamHelper.Threshold, 0.5);
            argSet.Add(ReductGeneratorParamHelper.NumberOfReductsInWeakClassifier, 1);
            argSet.Add(ReductGeneratorParamHelper.MaxIterations, 80);
            argSet.Add(ReductGeneratorParamHelper.InnerParameters, inner);
            argsList.Add(argSet);

            argSet = new Dictionary<string, object>();
            argSet.Add(ReductGeneratorParamHelper.TrainData, data);
            argSet.Add(ReductGeneratorParamHelper.FactoryKey, ReductFactoryKeyHelper.ReductEnsembleBoosting);
            argSet.Add(ReductGeneratorParamHelper.IdentificationType, (RuleQualityFunction)RuleQuality.Confidence);
            argSet.Add(ReductGeneratorParamHelper.VoteType, (RuleQualityFunction)RuleQuality.SingleVote);
            argSet.Add(ReductGeneratorParamHelper.MaxReductLength, 4);
            argSet.Add(ReductGeneratorParamHelper.Threshold, 0.5);
            argSet.Add(ReductGeneratorParamHelper.NumberOfReductsInWeakClassifier, 1);
            argSet.Add(ReductGeneratorParamHelper.MaxIterations, 90);
            argSet.Add(ReductGeneratorParamHelper.InnerParameters, inner);
            argsList.Add(argSet);

            argSet = new Dictionary<string, object>();
            argSet.Add(ReductGeneratorParamHelper.TrainData, data);
            argSet.Add(ReductGeneratorParamHelper.FactoryKey, ReductFactoryKeyHelper.ReductEnsembleBoosting);
            argSet.Add(ReductGeneratorParamHelper.IdentificationType, (RuleQualityFunction)RuleQuality.Confidence);
            argSet.Add(ReductGeneratorParamHelper.VoteType, (RuleQualityFunction)RuleQuality.SingleVote);
            argSet.Add(ReductGeneratorParamHelper.MaxReductLength, 4);
            argSet.Add(ReductGeneratorParamHelper.Threshold, 0.5);
            argSet.Add(ReductGeneratorParamHelper.NumberOfReductsInWeakClassifier, 1);
            argSet.Add(ReductGeneratorParamHelper.MaxIterations, 100);
            argSet.Add(ReductGeneratorParamHelper.InnerParameters, inner);
            argsList.Add(argSet);

            argSet = new Dictionary<string, object>();
            argSet.Add(ReductGeneratorParamHelper.TrainData, data);
            argSet.Add(ReductGeneratorParamHelper.NumberOfThreads, 1);
            argSet.Add(ReductGeneratorParamHelper.FactoryKey, ReductFactoryKeyHelper.ReductEnsembleBoosting);
            argSet.Add(ReductGeneratorParamHelper.IdentificationType, (RuleQualityFunction)RuleQuality.Confidence);
            argSet.Add(ReductGeneratorParamHelper.VoteType, (RuleQualityFunction)RuleQuality.SingleVote);
            argSet.Add(ReductGeneratorParamHelper.MaxReductLength, 4);
            argSet.Add(ReductGeneratorParamHelper.Threshold, 0.5);
            argSet.Add(ReductGeneratorParamHelper.NumberOfReductsInWeakClassifier, 1);
            argSet.Add(ReductGeneratorParamHelper.MaxIterations, 200);
            argSet.Add(ReductGeneratorParamHelper.InnerParameters, inner);
            argsList.Add(argSet);

            argSet = new Dictionary<string, object>();
            argSet.Add(ReductGeneratorParamHelper.TrainData, data);
            argSet.Add(ReductGeneratorParamHelper.NumberOfThreads, 1);
            argSet.Add(ReductGeneratorParamHelper.FactoryKey, ReductFactoryKeyHelper.ReductEnsembleBoosting);
            argSet.Add(ReductGeneratorParamHelper.IdentificationType, (RuleQualityFunction)RuleQuality.Confidence);
            argSet.Add(ReductGeneratorParamHelper.VoteType, (RuleQualityFunction)RuleQuality.SingleVote);
            argSet.Add(ReductGeneratorParamHelper.MaxReductLength, 4);
            argSet.Add(ReductGeneratorParamHelper.Threshold, 0.5);
            argSet.Add(ReductGeneratorParamHelper.NumberOfReductsInWeakClassifier, 1);
            argSet.Add(ReductGeneratorParamHelper.MaxIterations, 300);
            argSet.Add(ReductGeneratorParamHelper.InnerParameters, inner);
            argsList.Add(argSet);
            
            return argsList;
        }

        [Test, TestCaseSource("GetGenerateTestArgs"), Ignore] 
        public void GenerateTest(Dictionary<string, object> args)
        {            
            Args parms = new Args();
            foreach (KeyValuePair<string, object> kvp in args)            
                parms.SetParameter(kvp.Key, kvp.Value);            

            ReductEnsembleBoostingGenerator reductGenerator = ReductFactory.GetReductGenerator(parms) as ReductEnsembleBoostingGenerator;
            reductGenerator.Run();

            DataStore data = (DataStore)parms.GetParameter(ReductGeneratorParamHelper.TrainData);

            RoughClassifier classifierTrn = new RoughClassifier(
                        reductGenerator.GetReductGroups(),
                        reductGenerator.IdentyficationType,
                        reductGenerator.VoteType,
                        data.DataStoreInfo.GetDecisionValues());
            ClassificationResult resultTrn = classifierTrn.Classify(data, null);
        }

        [TestCase(0.1, 100, 10)]
        [TestCase(0.2, 100, 10)]
        [TestCase(0.3, 100, 10)]
        [TestCase(0.4, 100, 10)]
        [TestCase(0.5, 100, 10)]
        [TestCase(0.6, 100, 10)]
        [TestCase(0.7, 100, 10)]
        [TestCase(0.8, 100, 10)]
        [TestCase(0.9, 100, 10)]
        [TestCase(0.0, 100, 10)]
        public void GenerateBoostingTestNullExceptionError(decimal epsilon, int iterations, int weak)
        {
            DataStore train = DataStore.Load(@"Data\dna_modified.trn", FileFormat.Rses1);
            DataStore test = DataStore.Load(@"Data\dna_modified.tst", FileFormat.Rses1, train.DataStoreInfo);

            //-------------------------------------- Parameters --------------------------

            string factoryKey = ReductFactoryKeyHelper.ReductEnsembleBoosting;
            int numberOfPermutations = 100;
        
            RuleQualityFunction identificationFunction = RuleQuality.CoverageW;
            RuleQualityFunction voteFunction = RuleQuality.CoverageW;
            WeightGeneratorType weightGeneratorType = WeightGeneratorType.Relative;

            string innerFactoryKey = ReductFactoryKeyHelper.ApproximateReductRelativeWeights;        
            decimal innerEpsilon = 0.1m;

            RuleQualityFunction boostingIdentificationFunction = null;
            RuleQualityFunction boostingVoteFunction = null;            
            UpdateWeightsDelegate boostingUpdateWeights = ReductEnsembleBoostingGenerator.UpdateWeightsAdaBoost_All;
            CalcModelConfidenceDelegate boostingCalcModelConfidence = ReductEnsembleBoostingGenerator.ModelConfidenceAdaBoostM1;
            bool boostingCheckEnsambleErrorDuringTraining = false;

            if (boostingIdentificationFunction == null)
                boostingIdentificationFunction = identificationFunction;

            if (boostingVoteFunction == null)
                boostingVoteFunction = voteFunction;

            //----------------------------------------------------------------------------

            WeightGenerator wGen = WeightGenerator.Construct(weightGeneratorType, train);

            Args innerArgs = new Args();
            innerArgs.SetParameter(ReductGeneratorParamHelper.TrainData, train);
            innerArgs.SetParameter(ReductGeneratorParamHelper.FactoryKey, innerFactoryKey);
            innerArgs.SetParameter(ReductGeneratorParamHelper.Epsilon, innerEpsilon);
            innerArgs.SetParameter(ReductGeneratorParamHelper.WeightGenerator, wGen);
            innerArgs.SetParameter(ReductGeneratorParamHelper.ReductionStep, (int)(train.DataStoreInfo.GetNumberOfFields(FieldTypes.Standard) * 0.1)); //10% reduction step

            Args args = new Args();
            args.SetParameter(ReductGeneratorParamHelper.TrainData, train);
            args.SetParameter(ReductGeneratorParamHelper.FactoryKey, factoryKey);
            args.SetParameter(ReductGeneratorParamHelper.Epsilon, epsilon);
            args.SetParameter(ReductGeneratorParamHelper.NumberOfPermutations, numberOfPermutations);
            args.SetParameter(ReductGeneratorParamHelper.WeightGenerator, wGen);

            args.SetParameter(ReductGeneratorParamHelper.NumberOfReductsInWeakClassifier, weak);
            args.SetParameter(ReductGeneratorParamHelper.IdentificationType, boostingIdentificationFunction);
            args.SetParameter(ReductGeneratorParamHelper.VoteType, boostingVoteFunction);
            args.SetParameter(ReductGeneratorParamHelper.UpdateWeights, boostingUpdateWeights);
            args.SetParameter(ReductGeneratorParamHelper.CalcModelConfidence, boostingCalcModelConfidence);
            args.SetParameter(ReductGeneratorParamHelper.MaxIterations, iterations);
            args.SetParameter(ReductGeneratorParamHelper.CheckEnsembleErrorDuringTraining, boostingCheckEnsambleErrorDuringTraining);
            args.SetParameter(ReductGeneratorParamHelper.InnerParameters, innerArgs);
            
            IReductGenerator generator = ReductFactory.GetReductGenerator(args);
            generator.Run();
            IReductStoreCollection reductStoreCollection = generator.GetReductStoreCollection();

            Assert.NotNull(reductStoreCollection);

            RoughClassifier classifier = new RoughClassifier(reductStoreCollection, identificationFunction, voteFunction, train.DataStoreInfo.GetDecisionValues());
            ClassificationResult result = classifier.Classify(test);
            Console.WriteLine(result.Accuracy);            
        }
        
        [Test]
        public void GenerateExperimentBoostingStandard()
        {
            DataStore trnData = DataStore.Load(@"Data\dna_modified.trn", FileFormat.Rses1);
            DataStore tstData = DataStore.Load(@"Data\dna_modified.tst", FileFormat.Rses1, trnData.DataStoreInfo);
            WeightingSchema weightingSchema = WeightingSchema.Majority;
            int numberOfTests = 1;
            int maxNumberOfIterations = 100;

            Console.WriteLine("{0} {1} {2} {3} {4} {5} {6} {7} {8} {9} {10} {11} {12}",
                                     "METHOD",
                                     "IDENTYFICATION",
                                     "VOTETYPE",
                                     //"MIN_LEN",
                                     "UPDATE_WEIGHTS",
                                     "WEIGHT_TYPE",
                                     "CHECK_ENSEBLE_ERROR",
                                     "TESTID",
                                     "MAXITER",
                                     "NOF_MODELS",
                                     "NOF_WRESET",
                                     "TRN_ERROR",
                                     "TST_ERROR",
                                     "AVG_REDUCT");

            for (int iter = 3; iter <= maxNumberOfIterations; iter++)
            {
                for (int t = 0; t < numberOfTests; t++)
                {
                    Args parms = new Args();
                    parms.SetParameter(ReductGeneratorParamHelper.TrainData, trnData);
                    parms.SetParameter(ReductGeneratorParamHelper.NumberOfThreads, 1);
                    parms.SetParameter(ReductGeneratorParamHelper.FactoryKey, ReductFactoryKeyHelper.ReductEnsembleBoosting);
                    parms.SetParameter(ReductGeneratorParamHelper.IdentificationType, (RuleQualityFunction)RuleQuality.ConfidenceW);
                    parms.SetParameter(ReductGeneratorParamHelper.VoteType, (RuleQualityFunction)RuleQuality.ConfidenceW);
                    //args.SetParameter(ReductGeneratorParamHelper.MinReductLength, 1);
                    parms.SetParameter(ReductGeneratorParamHelper.NumberOfReductsInWeakClassifier, 10);
                    parms.SetParameter(ReductGeneratorParamHelper.MaxIterations, iter);
                    
                    WeightGenerator weightGenerator;
                    switch (weightingSchema)
                    {
                        case WeightingSchema.Majority:
                            weightGenerator = new WeightGeneratorMajority(trnData);
                            break;

                        case WeightingSchema.Relative:
                            weightGenerator = new WeightGeneratorRelative(trnData);
                            break;

                        default:
                            weightGenerator = new WeightBoostingGenerator(trnData);
                            break;
                    }

                    parms.SetParameter(ReductGeneratorParamHelper.WeightGenerator, weightGenerator);      
                    parms.SetParameter(ReductGeneratorParamHelper.CheckEnsembleErrorDuringTraining, false);

                    Args innerArgs = new Args();
                    innerArgs.SetParameter(ReductGeneratorParamHelper.TrainData, trnData);
                    innerArgs.SetParameter(ReductGeneratorParamHelper.FactoryKey, ReductFactoryKeyHelper.ApproximateReductRelativeWeights);
                    innerArgs.SetParameter(ReductGeneratorParamHelper.Epsilon, 0.2m);
                    innerArgs.SetParameter(ReductGeneratorParamHelper.WeightGenerator, weightGenerator);
                    innerArgs.SetParameter(ReductGeneratorParamHelper.ReductionStep, (int)(trnData.DataStoreInfo.GetNumberOfFields(FieldTypes.Standard) * 0.1)); //10% reduction step
                    
                    parms.SetParameter(ReductGeneratorParamHelper.InnerParameters, innerArgs);

                    ReductEnsembleBoostingGenerator reductGenerator = ReductFactory.GetReductGenerator(parms) as ReductEnsembleBoostingGenerator;
                    reductGenerator.Run();

                    IReductStoreCollection reductStoreCollection = reductGenerator.GetReductStoreCollection();

                    RoughClassifier classifierTrn = new RoughClassifier(
                        reductStoreCollection,
                        reductGenerator.IdentyficationType,
                        reductGenerator.VoteType,
                        trnData.DataStoreInfo.GetDecisionValues());
                    ClassificationResult resultTrn = classifierTrn.Classify(trnData, null);


                    RoughClassifier classifierTst = new RoughClassifier(
                        reductStoreCollection,
                        reductGenerator.IdentyficationType,
                        reductGenerator.VoteType,
                        trnData.DataStoreInfo.GetDecisionValues());
                    ClassificationResult resultTst = classifierTst.Classify(tstData, null);

                    
                    Console.WriteLine("{0} {1} {2} {3} {4} {5} {6} {7} {8} {9} {10} {11} {12}",
                                      parms.GetParameter(ReductGeneratorParamHelper.FactoryKey),
                                      reductGenerator.IdentyficationType.Method.Name,
                                      reductGenerator.VoteType.Method.Name,
                                      //reductGenerator.MinReductLength,
                                      reductGenerator.UpdateWeights.Method.Name,
                                      weightingSchema,
                                      reductGenerator.CheckEnsembleErrorDuringTraining,
                                      t + 1,
                                      reductGenerator.MaxIterations,
                                      reductGenerator.IterationsPassed,
                                      reductGenerator.NumberOfWeightResets,
                                      resultTrn.WeightMisclassified + resultTrn.WeightUnclassified,
                                      resultTst.WeightMisclassified + resultTst.WeightUnclassified,
                                      reductStoreCollection.First().GetAvgMeasure(new ReductMeasureLength()));
                    
                }
            }
        }

        [Test]
        public void GenerateExperimentBoostingVarEps()
        {
            //Console.WriteLine("GenerateExperimentBoostingStandard");

            //TrainData trnData = TrainData.Load(@"Data\dna_modified.trn", FileFormat.Rses1);
            //TrainData tstData = TrainData.Load(@"Data\dna_modified.tst", FileFormat.Rses1, trnData.DataStoreInfo);

            DataStore trnData = DataStore.Load(@"Data\optdigits.trn", FileFormat.Rses1);
            DataStore tstData = DataStore.Load(@"Data\optdigits.tst", FileFormat.Rses1, trnData.DataStoreInfo);

            WeightingSchema weightingSchema = WeightingSchema.Majority;
            int numberOfTests = 2;
            //int maxNumberOfIterations = 100;

            /*
            Console.WriteLine("{0} {1} {2} {3} {4} {5} {6} {7} {8} {9} {10} {11} {12} {13}",
                                     "METHOD",
                                     "IDENTYFICATION",
                                     "VOTETYPE",
                                     "MIN_LEN",
                                     "UPDATE_WEIGHTS",
                                     "WEIGHT_TYPE",
                                     "CHECK_ENSEBLE_ERROR",
                                     "TESTID",
                                     "MAXITER",
                                     "NOF_MODELS",
                                     "NOF_WRESET",
                                     "TRN_ERROR",
                                     "TST_ERROR",
                                     "AVG_REDUCT");
            */
            //List<int> iterList = new List<int>(new int[] { 1, 2, 5, 10, 20, 50, 100 });

            List<int> iterList = new List<int>(new int[] { 1, 2, 5, 10 });

            //for (int iter = 1; iter <= maxNumberOfIterations; iter++)
            foreach(int iter in iterList)
            {
                for (int t = 0; t < numberOfTests; t++)
                {
                    Args parms = new Args();
                    parms.SetParameter(ReductGeneratorParamHelper.TrainData, trnData);
                    parms.SetParameter(ReductGeneratorParamHelper.FactoryKey, ReductFactoryKeyHelper.ReductEnsembleBoostingVarEps);
                    parms.SetParameter(ReductGeneratorParamHelper.IdentificationType, (RuleQualityFunction)RuleQuality.ConfidenceW);
                    parms.SetParameter(ReductGeneratorParamHelper.VoteType, (RuleQualityFunction)RuleQuality.ConfidenceW);
                    parms.SetParameter(ReductGeneratorParamHelper.MaxIterations, iter);
                    
                    WeightGenerator weightGenerator;
                    switch (weightingSchema)
                    {
                        case WeightingSchema.Majority:
                            weightGenerator = new WeightGeneratorMajority(trnData);
                            break;

                        case WeightingSchema.Relative:
                            weightGenerator = new WeightGeneratorRelative(trnData);
                            break;

                        default:
                            weightGenerator = new WeightBoostingGenerator(trnData);
                            break;
                    }

                    parms.SetParameter(ReductGeneratorParamHelper.WeightGenerator, weightGenerator);
                    parms.SetParameter(ReductGeneratorParamHelper.CheckEnsembleErrorDuringTraining, false);

                    Args innerArgs = new Args();
                    innerArgs.SetParameter(ReductGeneratorParamHelper.TrainData, trnData);
                    innerArgs.SetParameter(ReductGeneratorParamHelper.FactoryKey, ReductFactoryKeyHelper.ApproximateReductRelativeWeights);
                    innerArgs.SetParameter(ReductGeneratorParamHelper.Epsilon, 0.1m);
                    innerArgs.SetParameter(ReductGeneratorParamHelper.WeightGenerator, weightGenerator);
                    innerArgs.SetParameter(ReductGeneratorParamHelper.ReductionStep, (int)(trnData.DataStoreInfo.GetNumberOfFields(FieldTypes.Standard) * 0.1)); //10% reduction step
                    
                    parms.SetParameter(ReductGeneratorParamHelper.InnerParameters, innerArgs);
                    
                    ReductEnsembleBoostingVarEpsGenerator reductGenerator = ReductFactory.GetReductGenerator(parms) as ReductEnsembleBoostingVarEpsGenerator;
                    reductGenerator.Run();

                    RoughClassifier classifierTrn = new RoughClassifier(
                        reductGenerator.GetReductGroups(),
                        reductGenerator.IdentyficationType,
                        reductGenerator.VoteType,
                        trnData.DataStoreInfo.GetDecisionValues());
                    ClassificationResult resultTrn = classifierTrn.Classify(trnData, null);


                    RoughClassifier classifierTst = new RoughClassifier(
                        reductGenerator.GetReductGroups(),
                        reductGenerator.IdentyficationType,
                        reductGenerator.VoteType,
                        trnData.DataStoreInfo.GetDecisionValues());
                    ClassificationResult resultTst = classifierTst.Classify(tstData, null);

                    /*
                    Console.WriteLine("{0} {1} {2} {3} {4} {5} {6} {7} {8} {9} {10} {11} {12} {13}",
                                      args.GetParameter(ReductGeneratorParamHelper.FactoryKey),
                                      reductGenerator.IdentyficationType,
                                      reductGenerator.VoteType,
                                      reductGenerator.MinReductLength,
                                      reductGenerator.UpdateWeights,
                                      weightingSchema,
                                      reductGenerator.CheckEnsembleErrorDuringTraining,
                                      t + 1,
                                      reductGenerator.MaxIterations,
                                      reductGenerator.IterationsPassed,
                                      reductGenerator.NumberOfWeightResets,
                                      resultTrn.WeightMisclassified + resultTrn.WeightUnclassified,
                                      resultTst.WeightMisclassified + resultTst.WeightUnclassified,
                                      reductGenerator.ReductPool.GetAvgMeasure(new ReductMeasureLength()));
                    */
                }
            }
        }

        [Test, Ignore]
        public void GenerateExperimentBoostingWithDiversity()
        {
            //Console.WriteLine("GenerateExperimentBoostingWithDiversity");

            DataStore trnData = DataStore.Load(@"Data\dna_modified.trn", FileFormat.Rses1);
            DataStore tstData = DataStore.Load(@"Data\dna_modified.tst", FileFormat.Rses1, trnData.DataStoreInfo);

            /*
            Console.WriteLine("{0} {1} {2} {3} {4} {5} {6}",
                                      "TESTID",
                                      "MAXITER",
                                      "NOF_MODELS",
                                      "NOF_WRESET",
                                      "TRN_ERROR",
                                      "TST_ERROR",
                                      "AVG_REDUCT");
            */

            for (int iter = 1; iter <= 2; iter++)
            {
                for (int t = 0; t < 2; t++)
                {
                    Args parms = new Args();
                    parms.SetParameter(ReductGeneratorParamHelper.TrainData, trnData);
                    parms.SetParameter(ReductGeneratorParamHelper.NumberOfThreads, 1);
                    parms.SetParameter(ReductGeneratorParamHelper.FactoryKey, ReductFactoryKeyHelper.ReductEnsembleBoostingWithDiversity);
                    parms.SetParameter(ReductGeneratorParamHelper.IdentificationType, (RuleQualityFunction)RuleQuality.ConfidenceW);
                    parms.SetParameter(ReductGeneratorParamHelper.ReconWeights, (Func<IReduct, decimal[], RuleQualityFunction, double[]>)ReductEnsembleReconWeightsHelper.GetCorrectReconWeights);
                    parms.SetParameter(ReductGeneratorParamHelper.Distance, (Func<double[], double[], double>)Similarity.Manhattan);
                    parms.SetParameter(ReductGeneratorParamHelper.Linkage, (Func<int[], int[], DistanceMatrix, double[][], double>)ClusteringLinkage.Complete);
                    parms.SetParameter(ReductGeneratorParamHelper.VoteType, (RuleQualityFunction)RuleQuality.ConfidenceW);
                    parms.SetParameter(ReductGeneratorParamHelper.MinReductLength, 2);
                    //args.SetParameter(ReductGeneratorParamHelper.MaxReductLength, 5);                    
                    parms.SetParameter(ReductGeneratorParamHelper.NumberOfReductsInWeakClassifier, 1);
                    parms.SetParameter(ReductGeneratorParamHelper.MaxIterations, iter);
                    parms.SetParameter(ReductGeneratorParamHelper.NumberOfReductsToTest, 20);
                    parms.SetParameter(ReductGeneratorParamHelper.AgregateFunction, AgregateFunction.Max);

                    Args innerArgs = new Args();
                    innerArgs.SetParameter(ReductGeneratorParamHelper.TrainData, trnData);
                    innerArgs.SetParameter(ReductGeneratorParamHelper.FactoryKey, ReductFactoryKeyHelper.ApproximateReductRelativeWeights);
                    innerArgs.SetParameter(ReductGeneratorParamHelper.Epsilon, 0.1);
                    innerArgs.SetParameter(ReductGeneratorParamHelper.WeightGenerator, new WeightGeneratorRelative(trnData));
                    innerArgs.SetParameter(ReductGeneratorParamHelper.ReductionStep, (int)(trnData.DataStoreInfo.GetNumberOfFields(FieldTypes.Standard) * 0.1)); //10% reduction step

                    parms.SetParameter(ReductGeneratorParamHelper.InnerParameters, innerArgs);
                    
                    ReductEnsembleBoostingWithDiversityGenerator reductGenerator = ReductFactory.GetReductGenerator(parms) as ReductEnsembleBoostingWithDiversityGenerator;
                    reductGenerator.Run();

                    RoughClassifier classifierTrn = new RoughClassifier(
                        reductGenerator.GetReductGroups(), 
                        reductGenerator.IdentyficationType, 
                        reductGenerator.VoteType,
                        trnData.DataStoreInfo.GetDecisionValues());
                    ClassificationResult resultTrn = classifierTrn.Classify(trnData);

                    RoughClassifier classifierTst = new RoughClassifier(
                        reductGenerator.GetReductGroups(), 
                        reductGenerator.IdentyficationType, 
                        reductGenerator.VoteType, 
                        trnData.DataStoreInfo.GetDecisionValues());
                    ClassificationResult resultTst = classifierTst.Classify(tstData, null);

                    /*
                    Console.WriteLine("{0} {1} {2} {3} {4} {5} {6}",
                                      t + 1,
                                      reductGenerator.MaxIterations,
                                      reductGenerator.IterationsPassed,
                                      reductGenerator.NumberOfWeightResets,
                                      resultTrn.WeightMisclassified + resultTrn.WeightUnclassified,
                                      resultTst.WeightMisclassified + resultTst.WeightUnclassified,
                                      reductGenerator.ReductPool.GetAvgMeasure(new ReductMeasureLength()));
                    */
                }
            }
        }

        [Test]
        public void GenerateExperimentBoostingWithAttributeDiversity()
        {
            //Console.WriteLine("GenerateExperimentBoostingWithAttributeDiversity");

            DataStore trnData = DataStore.Load(@"Data\pendigits.trn", FileFormat.Rses1);
            DataStore tstData = DataStore.Load(@"Data\pendigits.tst", FileFormat.Rses1, trnData.DataStoreInfo);            

            WeightingSchema weightingSchema = WeightingSchema.Majority;
            int numberOfTests = 1;
            int maxNumberOfIterations = 2;

            /*
            Console.WriteLine("{0} {1} {2} {3} {4} {5} {6} {7} {8} {9} {10} {11} {12} {13}",
                                     "METHOD",
                                     "IDENTYFICATION",
                                     "VOTETYPE",
                                     "MIN_LEN",
                                     "UPDATE_WEIGHTS",
                                     "WEIGHT_TYPE",
                                     "CHECK_ENSEBLE_ERROR",
                                     "TESTID",                                     
                                     "MAXITER",
                                     "NOF_MODELS",
                                     "NOF_WRESET",
                                     "TRN_ERROR",
                                     "TST_ERROR",
                                     "AVG_REDUCT");
            */

            for (int iter = 1; iter <= maxNumberOfIterations; iter++)
            {
                for (int t = 0; t < numberOfTests; t++)
                {
                    Args parms = new Args();
                    parms.SetParameter(ReductGeneratorParamHelper.TrainData, trnData);
                    parms.SetParameter(ReductGeneratorParamHelper.NumberOfThreads, 1);
                    parms.SetParameter(ReductGeneratorParamHelper.FactoryKey, ReductFactoryKeyHelper.ReductEnsembleBoostingWithAttributeDiversity);
                    parms.SetParameter(ReductGeneratorParamHelper.IdentificationType, (RuleQualityFunction)RuleQuality.ConfidenceW);
                    parms.SetParameter(ReductGeneratorParamHelper.VoteType, (RuleQualityFunction)RuleQuality.ConfidenceW);                                      
                    parms.SetParameter(ReductGeneratorParamHelper.NumberOfReductsInWeakClassifier, 1);
                    parms.SetParameter(ReductGeneratorParamHelper.MaxIterations, iter);                    

                    WeightGenerator weightGenerator;
                    switch(weightingSchema)
                    {
                        case WeightingSchema.Majority :
                            weightGenerator = new WeightGeneratorMajority(trnData);
                            break;

                        case WeightingSchema.Relative:
                            weightGenerator = new WeightGeneratorRelative(trnData);
                            break;

                        default:
                            weightGenerator = new WeightBoostingGenerator(trnData);
                            break;
                    }

                    parms.SetParameter(ReductGeneratorParamHelper.WeightGenerator, weightGenerator);
                    parms.SetParameter(ReductGeneratorParamHelper.CheckEnsembleErrorDuringTraining, false);

                    Args innerArgs = new Args();
                    innerArgs.SetParameter(ReductGeneratorParamHelper.TrainData, trnData);
                    innerArgs.SetParameter(ReductGeneratorParamHelper.FactoryKey, ReductFactoryKeyHelper.ApproximateReductRelativeWeights);
                    innerArgs.SetParameter(ReductGeneratorParamHelper.Epsilon, 0.1m);
                    innerArgs.SetParameter(ReductGeneratorParamHelper.WeightGenerator, weightGenerator);
                    innerArgs.SetParameter(ReductGeneratorParamHelper.ReductionStep, (int)(trnData.DataStoreInfo.GetNumberOfFields(FieldTypes.Standard) * 0.1)); //10% reduction step

                    parms.SetParameter(ReductGeneratorParamHelper.InnerParameters, innerArgs);
                    
                    ReductEnsembleBoostingWithAttributeDiversityGenerator reductGenerator = ReductFactory.GetReductGenerator(parms) as ReductEnsembleBoostingWithAttributeDiversityGenerator;
                    reductGenerator.Run();

                    RoughClassifier classifierTrn = new RoughClassifier(
                        reductGenerator.GetReductGroups(),
                        reductGenerator.IdentyficationType,
                        reductGenerator.VoteType,
                        trnData.DataStoreInfo.GetDecisionValues());
                    ClassificationResult resultTrn = classifierTrn.Classify(trnData, null);


                    RoughClassifier classifierTst = new RoughClassifier(
                        reductGenerator.GetReductGroups(),
                        reductGenerator.IdentyficationType,
                        reductGenerator.VoteType,
                        trnData.DataStoreInfo.GetDecisionValues());
                    ClassificationResult resultTst = classifierTst.Classify(tstData, null);

                    /*
                    Console.WriteLine("{0} {1} {2} {3} {4} {5} {6} {7} {8} {9} {10} {11} {12} {13}",
                                      args.GetParameter(ReductGeneratorParamHelper.FactoryKey),
                                      reductGenerator.IdentyficationType,
                                      reductGenerator.VoteType,
                                      reductGenerator.MinReductLength,
                                      reductGenerator.UpdateWeights,
                                      weightingSchema,
                                      reductGenerator.CheckEnsembleErrorDuringTraining,
                                      t + 1,
                                      reductGenerator.MaxIterations,
                                      reductGenerator.IterationsPassed,
                                      reductGenerator.NumberOfWeightResets,
                                      resultTrn.WeightMisclassified + resultTrn.WeightUnclassified,
                                      resultTst.WeightMisclassified + resultTst.WeightUnclassified,
                                      reductGenerator.ReductPool.GetAvgMeasure(new ReductMeasureLength()));
                    */
                }
            }
        }        
    }
}
