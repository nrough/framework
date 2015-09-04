﻿using System;
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
            Console.WriteLine("class ReductEnsembleBoostingTest Seed: {0}", seed);
            RandomSingleton.Seed = seed;
        }
        
        public static IEnumerable<Dictionary<string, object>> GetGenerateTestArgs()
        {            
            DataStore data = DataStore.Load(@"Data\dna_modified.trn", FileFormat.Rses1);

            List<Dictionary<string, object>> argsList = new List<Dictionary<string, object>>();

            Dictionary<string, object> argSet = new Dictionary<string, object>();
            argSet.Add(ReductGeneratorParamHelper.DataStore, data);
            argSet.Add(ReductGeneratorParamHelper.NumberOfThreads, 1);
            argSet.Add(ReductGeneratorParamHelper.FactoryKey, ReductFactoryKeyHelper.ReductEnsembleBoosting);
            argSet.Add(ReductGeneratorParamHelper.IdentificationType, IdentificationType.Confidence);
            argSet.Add(ReductGeneratorParamHelper.VoteType, VoteType.MajorDecision);
            argSet.Add(ReductGeneratorParamHelper.MaxReductLength, 4);
            argSet.Add(ReductGeneratorParamHelper.Threshold, 0.5);
            argSet.Add(ReductGeneratorParamHelper.NumberOfReductsInWeakClassifier, 1);
            argSet.Add(ReductGeneratorParamHelper.MaxIterations, 1);
            argsList.Add(argSet);

            argSet = new Dictionary<string, object>();
            argSet.Add(ReductGeneratorParamHelper.DataStore, data);
            argSet.Add(ReductGeneratorParamHelper.NumberOfThreads, 1);
            argSet.Add(ReductGeneratorParamHelper.FactoryKey, ReductFactoryKeyHelper.ReductEnsembleBoosting);
            argSet.Add(ReductGeneratorParamHelper.IdentificationType, IdentificationType.Confidence);
            argSet.Add(ReductGeneratorParamHelper.VoteType, VoteType.MajorDecision);
            argSet.Add(ReductGeneratorParamHelper.MaxReductLength, 4);
            argSet.Add(ReductGeneratorParamHelper.Threshold, 0.5);
            argSet.Add(ReductGeneratorParamHelper.NumberOfReductsInWeakClassifier, 1);
            argSet.Add(ReductGeneratorParamHelper.MaxIterations, 3);
            argsList.Add(argSet);

            argSet = new Dictionary<string, object>();
            argSet.Add(ReductGeneratorParamHelper.DataStore, data);
            argSet.Add(ReductGeneratorParamHelper.NumberOfThreads, 1);
            argSet.Add(ReductGeneratorParamHelper.FactoryKey, ReductFactoryKeyHelper.ReductEnsembleBoosting);
            argSet.Add(ReductGeneratorParamHelper.IdentificationType, IdentificationType.Confidence);
            argSet.Add(ReductGeneratorParamHelper.VoteType, VoteType.MajorDecision);
            argSet.Add(ReductGeneratorParamHelper.MaxReductLength, 4);
            argSet.Add(ReductGeneratorParamHelper.Threshold, 0.5);
            argSet.Add(ReductGeneratorParamHelper.NumberOfReductsInWeakClassifier, 1);
            argSet.Add(ReductGeneratorParamHelper.MaxIterations, 5);
            argsList.Add(argSet);


            argSet = new Dictionary<string, object>();
            argSet.Add(ReductGeneratorParamHelper.DataStore, data);
            argSet.Add(ReductGeneratorParamHelper.NumberOfThreads, 1);                                    
            argSet.Add(ReductGeneratorParamHelper.FactoryKey, ReductFactoryKeyHelper.ReductEnsembleBoosting);
            argSet.Add(ReductGeneratorParamHelper.IdentificationType, IdentificationType.Confidence);
            argSet.Add(ReductGeneratorParamHelper.VoteType, VoteType.MajorDecision);
            argSet.Add(ReductGeneratorParamHelper.MaxReductLength, 4);
            argSet.Add(ReductGeneratorParamHelper.Threshold, 0.5);
            argSet.Add(ReductGeneratorParamHelper.NumberOfReductsInWeakClassifier, 1);
            argSet.Add(ReductGeneratorParamHelper.MaxIterations, 10);                        
            argsList.Add(argSet);

            argSet = new Dictionary<string, object>();
            argSet.Add(ReductGeneratorParamHelper.DataStore, data);
            argSet.Add(ReductGeneratorParamHelper.NumberOfThreads, 1);
            argSet.Add(ReductGeneratorParamHelper.FactoryKey, ReductFactoryKeyHelper.ReductEnsembleBoosting);
            argSet.Add(ReductGeneratorParamHelper.IdentificationType, IdentificationType.Confidence);
            argSet.Add(ReductGeneratorParamHelper.VoteType, VoteType.MajorDecision);
            argSet.Add(ReductGeneratorParamHelper.MaxReductLength, 4);
            argSet.Add(ReductGeneratorParamHelper.Threshold, 0.5);
            argSet.Add(ReductGeneratorParamHelper.NumberOfReductsInWeakClassifier, 1);
            argSet.Add(ReductGeneratorParamHelper.MaxIterations, 20);
            argsList.Add(argSet);

            
            argSet = new Dictionary<string, object>();
            argSet.Add(ReductGeneratorParamHelper.DataStore, data);
            argSet.Add(ReductGeneratorParamHelper.NumberOfThreads, 1);
            argSet.Add(ReductGeneratorParamHelper.FactoryKey, ReductFactoryKeyHelper.ReductEnsembleBoosting);
            argSet.Add(ReductGeneratorParamHelper.IdentificationType, IdentificationType.Confidence);
            argSet.Add(ReductGeneratorParamHelper.VoteType, VoteType.MajorDecision);
            argSet.Add(ReductGeneratorParamHelper.MaxReductLength, 4);
            argSet.Add(ReductGeneratorParamHelper.Threshold, 0.5);
            argSet.Add(ReductGeneratorParamHelper.NumberOfReductsInWeakClassifier, 1);
            argSet.Add(ReductGeneratorParamHelper.MaxIterations, 30);
            argsList.Add(argSet);

            argSet = new Dictionary<string, object>();
            argSet.Add(ReductGeneratorParamHelper.DataStore, data);
            argSet.Add(ReductGeneratorParamHelper.NumberOfThreads, 1);
            argSet.Add(ReductGeneratorParamHelper.FactoryKey, ReductFactoryKeyHelper.ReductEnsembleBoosting);
            argSet.Add(ReductGeneratorParamHelper.IdentificationType, IdentificationType.Confidence);
            argSet.Add(ReductGeneratorParamHelper.VoteType, VoteType.MajorDecision);
            argSet.Add(ReductGeneratorParamHelper.MaxReductLength, 4);
            argSet.Add(ReductGeneratorParamHelper.Threshold, 0.5);
            argSet.Add(ReductGeneratorParamHelper.NumberOfReductsInWeakClassifier, 1);
            argSet.Add(ReductGeneratorParamHelper.MaxIterations, 40);
            argsList.Add(argSet);

            argSet = new Dictionary<string, object>();
            argSet.Add(ReductGeneratorParamHelper.DataStore, data);
            argSet.Add(ReductGeneratorParamHelper.NumberOfThreads, 1);
            argSet.Add(ReductGeneratorParamHelper.FactoryKey, ReductFactoryKeyHelper.ReductEnsembleBoosting);
            argSet.Add(ReductGeneratorParamHelper.IdentificationType, IdentificationType.Confidence);
            argSet.Add(ReductGeneratorParamHelper.VoteType, VoteType.MajorDecision);
            argSet.Add(ReductGeneratorParamHelper.MaxReductLength, 4);
            argSet.Add(ReductGeneratorParamHelper.Threshold, 0.5);
            argSet.Add(ReductGeneratorParamHelper.NumberOfReductsInWeakClassifier, 1);
            argSet.Add(ReductGeneratorParamHelper.MaxIterations, 50);
            argsList.Add(argSet);

            argSet = new Dictionary<string, object>();
            argSet.Add(ReductGeneratorParamHelper.DataStore, data);
            argSet.Add(ReductGeneratorParamHelper.NumberOfThreads, 1);
            argSet.Add(ReductGeneratorParamHelper.FactoryKey, ReductFactoryKeyHelper.ReductEnsembleBoosting);
            argSet.Add(ReductGeneratorParamHelper.IdentificationType, IdentificationType.Confidence);
            argSet.Add(ReductGeneratorParamHelper.VoteType, VoteType.MajorDecision);
            argSet.Add(ReductGeneratorParamHelper.MaxReductLength, 4);
            argSet.Add(ReductGeneratorParamHelper.Threshold, 0.5);
            argSet.Add(ReductGeneratorParamHelper.NumberOfReductsInWeakClassifier, 1);
            argSet.Add(ReductGeneratorParamHelper.MaxIterations, 60);
            argsList.Add(argSet);

            argSet = new Dictionary<string, object>();
            argSet.Add(ReductGeneratorParamHelper.DataStore, data);
            argSet.Add(ReductGeneratorParamHelper.NumberOfThreads, 1);
            argSet.Add(ReductGeneratorParamHelper.FactoryKey, ReductFactoryKeyHelper.ReductEnsembleBoosting);
            argSet.Add(ReductGeneratorParamHelper.IdentificationType, IdentificationType.Confidence);
            argSet.Add(ReductGeneratorParamHelper.VoteType, VoteType.MajorDecision);
            argSet.Add(ReductGeneratorParamHelper.MaxReductLength, 4);
            argSet.Add(ReductGeneratorParamHelper.Threshold, 0.5);
            argSet.Add(ReductGeneratorParamHelper.NumberOfReductsInWeakClassifier, 1);
            argSet.Add(ReductGeneratorParamHelper.MaxIterations, 70);
            argsList.Add(argSet);

            argSet = new Dictionary<string, object>();
            argSet.Add(ReductGeneratorParamHelper.DataStore, data);
            argSet.Add(ReductGeneratorParamHelper.NumberOfThreads, 1);
            argSet.Add(ReductGeneratorParamHelper.FactoryKey, ReductFactoryKeyHelper.ReductEnsembleBoosting);
            argSet.Add(ReductGeneratorParamHelper.IdentificationType, IdentificationType.Confidence);
            argSet.Add(ReductGeneratorParamHelper.VoteType, VoteType.MajorDecision);
            argSet.Add(ReductGeneratorParamHelper.MaxReductLength, 4);
            argSet.Add(ReductGeneratorParamHelper.Threshold, 0.5);
            argSet.Add(ReductGeneratorParamHelper.NumberOfReductsInWeakClassifier, 1);
            argSet.Add(ReductGeneratorParamHelper.MaxIterations, 80);
            argsList.Add(argSet);

            argSet = new Dictionary<string, object>();
            argSet.Add(ReductGeneratorParamHelper.DataStore, data);
            argSet.Add(ReductGeneratorParamHelper.NumberOfThreads, 1);
            argSet.Add(ReductGeneratorParamHelper.FactoryKey, ReductFactoryKeyHelper.ReductEnsembleBoosting);
            argSet.Add(ReductGeneratorParamHelper.IdentificationType, IdentificationType.Confidence);
            argSet.Add(ReductGeneratorParamHelper.VoteType, VoteType.MajorDecision);
            argSet.Add(ReductGeneratorParamHelper.MaxReductLength, 4);
            argSet.Add(ReductGeneratorParamHelper.Threshold, 0.5);
            argSet.Add(ReductGeneratorParamHelper.NumberOfReductsInWeakClassifier, 1);
            argSet.Add(ReductGeneratorParamHelper.MaxIterations, 90);
            argsList.Add(argSet);

            argSet = new Dictionary<string, object>();
            argSet.Add(ReductGeneratorParamHelper.DataStore, data);
            argSet.Add(ReductGeneratorParamHelper.NumberOfThreads, 1);
            argSet.Add(ReductGeneratorParamHelper.FactoryKey, ReductFactoryKeyHelper.ReductEnsembleBoosting);
            argSet.Add(ReductGeneratorParamHelper.IdentificationType, IdentificationType.Confidence);
            argSet.Add(ReductGeneratorParamHelper.VoteType, VoteType.MajorDecision);
            argSet.Add(ReductGeneratorParamHelper.MaxReductLength, 4);
            argSet.Add(ReductGeneratorParamHelper.Threshold, 0.5);
            argSet.Add(ReductGeneratorParamHelper.NumberOfReductsInWeakClassifier, 1);
            argSet.Add(ReductGeneratorParamHelper.MaxIterations, 100);
            argsList.Add(argSet);

            argSet = new Dictionary<string, object>();
            argSet.Add(ReductGeneratorParamHelper.DataStore, data);
            argSet.Add(ReductGeneratorParamHelper.NumberOfThreads, 1);
            argSet.Add(ReductGeneratorParamHelper.FactoryKey, ReductFactoryKeyHelper.ReductEnsembleBoosting);
            argSet.Add(ReductGeneratorParamHelper.IdentificationType, IdentificationType.Confidence);
            argSet.Add(ReductGeneratorParamHelper.VoteType, VoteType.MajorDecision);
            argSet.Add(ReductGeneratorParamHelper.MaxReductLength, 4);
            argSet.Add(ReductGeneratorParamHelper.Threshold, 0.5);
            argSet.Add(ReductGeneratorParamHelper.NumberOfReductsInWeakClassifier, 1);
            argSet.Add(ReductGeneratorParamHelper.MaxIterations, 200);
            argsList.Add(argSet);

            argSet = new Dictionary<string, object>();
            argSet.Add(ReductGeneratorParamHelper.DataStore, data);
            argSet.Add(ReductGeneratorParamHelper.NumberOfThreads, 1);
            argSet.Add(ReductGeneratorParamHelper.FactoryKey, ReductFactoryKeyHelper.ReductEnsembleBoosting);
            argSet.Add(ReductGeneratorParamHelper.IdentificationType, IdentificationType.Confidence);
            argSet.Add(ReductGeneratorParamHelper.VoteType, VoteType.MajorDecision);
            argSet.Add(ReductGeneratorParamHelper.MaxReductLength, 4);
            argSet.Add(ReductGeneratorParamHelper.Threshold, 0.5);
            argSet.Add(ReductGeneratorParamHelper.NumberOfReductsInWeakClassifier, 1);
            argSet.Add(ReductGeneratorParamHelper.MaxIterations, 300);
            argsList.Add(argSet);
            
            return argsList;
        }

        [Test, TestCaseSource("GetGenerateTestArgs"), Ignore]        
        public void GenerateTest(Dictionary<string, object> args)
        {            
            Args parms = new Args();
            foreach (KeyValuePair<string, object> kvp in args)            
                parms.AddParameter(kvp.Key, kvp.Value);            

            ReductEnsembleBoostingGenerator reductGenerator = ReductFactory.GetReductGenerator(parms) as ReductEnsembleBoostingGenerator;
            reductGenerator.Generate();

            DataStore data = (DataStore)parms.GetParameter(ReductGeneratorParamHelper.DataStore);
            RoughClassifier classifierTrn = new RoughClassifier();
            classifierTrn.ReductStoreCollection = reductGenerator.GetReductGroups();
            classifierTrn.Classify(data);
            ClassificationResult resultTrn = classifierTrn.Vote(data, reductGenerator.IdentyficationType, reductGenerator.VoteType, null);            

            Console.WriteLine("{0} {1} {2}",
                              reductGenerator.MaxIterations,
                              resultTrn.WeightMisclassified + resultTrn.WeightUnclassified);
        }
        
        [Test]
        public void GenerateExperimentBoostingStandard()
        {
            Console.WriteLine("GenerateExperimentBoostingStandard");
            
            DataStore trnData = DataStore.Load(@"Data\dna_modified.trn", FileFormat.Rses1);
            DataStore tstData = DataStore.Load(@"Data\dna_modified.tst", FileFormat.Rses1, trnData.DataStoreInfo);

            Console.WriteLine("{0} {1} {2} {3} {4} {5} {6}",
                                     "TESTID",
                                     "MAXITER",
                                     "NOF_MODELS",
                                     "NOF_WRESET",
                                     "TRN_ERROR",
                                     "TST_ERROR",
                                     "AVG_REDUCT");

            for (int t = 0; t < 5; t++)
            {
                for (int iter = 1; iter <= 300; iter++)
                {
                    Args parms = new Args();
                    parms.AddParameter(ReductGeneratorParamHelper.DataStore, trnData);
                    parms.AddParameter(ReductGeneratorParamHelper.NumberOfThreads, 1);
                    parms.AddParameter(ReductGeneratorParamHelper.FactoryKey, ReductFactoryKeyHelper.ReductEnsembleBoosting);
                    parms.AddParameter(ReductGeneratorParamHelper.IdentificationType, IdentificationType.WeightConfidence);
                    parms.AddParameter(ReductGeneratorParamHelper.VoteType, VoteType.WeightConfidence);
                    parms.AddParameter(ReductGeneratorParamHelper.MinReductLength, 1);
                    parms.AddParameter(ReductGeneratorParamHelper.MaxReductLength, 5);                    
                    parms.AddParameter(ReductGeneratorParamHelper.NumberOfReductsInWeakClassifier, 1);
                    parms.AddParameter(ReductGeneratorParamHelper.MaxIterations, iter);

                    ReductEnsembleBoostingGenerator reductGenerator = ReductFactory.GetReductGenerator(parms) as ReductEnsembleBoostingGenerator;
                    reductGenerator.Generate();

                    RoughClassifier classifierTrn = new RoughClassifier();
                    classifierTrn.ReductStoreCollection = reductGenerator.GetReductGroups();
                    classifierTrn.Classify(trnData);
                    ClassificationResult resultTrn = classifierTrn.Vote(trnData, reductGenerator.IdentyficationType, reductGenerator.VoteType, null);

                    RoughClassifier classifierTst = new RoughClassifier();
                    classifierTst.ReductStoreCollection = reductGenerator.GetReductGroups();
                    classifierTst.Classify(tstData);
                    ClassificationResult resultTst = classifierTst.Vote(tstData, reductGenerator.IdentyficationType, reductGenerator.VoteType, null);

                    Console.WriteLine("{0} {1} {2} {3} {4} {5} {6}",
                                      t + 1,
                                      reductGenerator.MaxIterations,
                                      reductGenerator.IterationsPassed - reductGenerator.NumberOfWeightResets,
                                      reductGenerator.NumberOfWeightResets,
                                      resultTrn.WeightMisclassified + resultTrn.WeightUnclassified,
                                      resultTst.WeightMisclassified + resultTst.WeightUnclassified,
                                      reductGenerator.ReductPool.GetAvgMeasure(new ReductMeasureLength()));
                }
            }
        }

        [Test]
        public void GenerateExperimentBoostingWithDiversity()
        {
            Console.WriteLine("GenerateExperimentBoostingWithDiversity");

            DataStore trnData = DataStore.Load(@"Data\dna_modified.trn", FileFormat.Rses1);
            DataStore tstData = DataStore.Load(@"Data\dna_modified.tst", FileFormat.Rses1, trnData.DataStoreInfo);

            Console.WriteLine("{0} {1} {2} {3} {4} {5} {6}",
                                      "TESTID",
                                      "MAXITER",
                                      "NOF_MODELS",
                                      "NOF_WRESET",
                                      "TRN_ERROR",
                                      "TST_ERROR",
                                      "AVG_REDUCT");

            for (int t = 0; t < 5; t++)
            {
                for (int iter = 1; iter <= 300; iter++)
                {
                    Args parms = new Args();
                    parms.AddParameter(ReductGeneratorParamHelper.DataStore, trnData);
                    parms.AddParameter(ReductGeneratorParamHelper.NumberOfThreads, 1);
                    parms.AddParameter(ReductGeneratorParamHelper.FactoryKey, ReductFactoryKeyHelper.ReductEnsembleBoostingWithDiversity);
                    parms.AddParameter(ReductGeneratorParamHelper.IdentificationType, IdentificationType.WeightConfidence);
                    parms.AddParameter(ReductGeneratorParamHelper.ReconWeights, (Func<IReduct, double[], double[]>)ReductEnsembleReconWeightsHelper.GetCorrectReconWeights);
                    parms.AddParameter(ReductGeneratorParamHelper.Distance, (Func<double[], double[], double>)Similarity.Manhattan);
                    parms.AddParameter(ReductGeneratorParamHelper.Linkage, (Func<int[], int[], DistanceMatrix, double[][], double>)ClusteringLinkage.Complete);
                    parms.AddParameter(ReductGeneratorParamHelper.VoteType, VoteType.WeightConfidence);
                    parms.AddParameter(ReductGeneratorParamHelper.MinReductLength, 2);
                    parms.AddParameter(ReductGeneratorParamHelper.MaxReductLength, 5);                    
                    parms.AddParameter(ReductGeneratorParamHelper.NumberOfReductsInWeakClassifier, 1);
                    parms.AddParameter(ReductGeneratorParamHelper.MaxIterations, iter);
                    parms.AddParameter(ReductGeneratorParamHelper.NumberOfReductsToTest, 20);
                    parms.AddParameter(ReductGeneratorParamHelper.AgregateFunction, AgregateFunction.Max);


                    ReductEnsembleBoostingWithDiversityGenerator reductGenerator = ReductFactory.GetReductGenerator(parms) as ReductEnsembleBoostingWithDiversityGenerator;
                    reductGenerator.Generate();

                    RoughClassifier classifierTrn = new RoughClassifier();
                    classifierTrn.ReductStoreCollection = reductGenerator.GetReductGroups();
                    classifierTrn.Classify(trnData);
                    ClassificationResult resultTrn = classifierTrn.Vote(trnData, reductGenerator.IdentyficationType, reductGenerator.VoteType, null);

                    RoughClassifier classifierTst = new RoughClassifier();
                    classifierTst.ReductStoreCollection = reductGenerator.GetReductGroups();
                    classifierTst.Classify(tstData);
                    ClassificationResult resultTst = classifierTst.Vote(tstData, reductGenerator.IdentyficationType, reductGenerator.VoteType, null);

                    Console.WriteLine("{0} {1} {2} {3} {4} {5} {6}",
                                      t + 1,
                                      reductGenerator.MaxIterations,
                                      reductGenerator.IterationsPassed - reductGenerator.NumberOfWeightResets,
                                      reductGenerator.NumberOfWeightResets,
                                      resultTrn.WeightMisclassified + resultTrn.WeightUnclassified,
                                      resultTst.WeightMisclassified + resultTst.WeightUnclassified,
                                      reductGenerator.ReductPool.GetAvgMeasure(new ReductMeasureLength()));
                }
            }
        }

        [Test]
        public void GenerateExperimentBoostingWithAttributeDiversity()
        {
            Console.WriteLine("GenerateExperimentBoostingWithAttributeDiversity");

            DataStore trnData = DataStore.Load(@"Data\dna_modified.trn", FileFormat.Rses1);
            DataStore tstData = DataStore.Load(@"Data\dna_modified.tst", FileFormat.Rses1, trnData.DataStoreInfo);

            Console.WriteLine("{0} {1} {2} {3} {4} {5} {6}",
                                      "TESTID",
                                      "MAXITER",
                                      "NOF_MODELS",
                                      "NOF_WRESET",
                                      "TRN_ERROR",
                                      "TST_ERROR",
                                      "AVG_REDUCT");

            for (int t = 0; t < 5; t++)
            {
                for (int iter = 1; iter <= 300; iter++)
                {
                    Args parms = new Args();
                    parms.AddParameter(ReductGeneratorParamHelper.DataStore, trnData);
                    parms.AddParameter(ReductGeneratorParamHelper.NumberOfThreads, 1);
                    parms.AddParameter(ReductGeneratorParamHelper.FactoryKey, ReductFactoryKeyHelper.ReductEnsembleBoostingWithAttributeDiversity);
                    parms.AddParameter(ReductGeneratorParamHelper.IdentificationType, IdentificationType.WeightConfidence);                                                            
                    parms.AddParameter(ReductGeneratorParamHelper.VoteType, VoteType.WeightConfidence);
                    parms.AddParameter(ReductGeneratorParamHelper.MinReductLength, 2);
                    parms.AddParameter(ReductGeneratorParamHelper.MaxReductLength, 5);
                    parms.AddParameter(ReductGeneratorParamHelper.NumberOfReductsInWeakClassifier, 1);
                    parms.AddParameter(ReductGeneratorParamHelper.MaxIterations, iter);


                    ReductEnsembleBoostingWithAttributeDiversityGenerator reductGenerator = ReductFactory.GetReductGenerator(parms) as ReductEnsembleBoostingWithAttributeDiversityGenerator;
                    reductGenerator.Generate();

                    RoughClassifier classifierTrn = new RoughClassifier();
                    classifierTrn.ReductStoreCollection = reductGenerator.GetReductGroups();
                    classifierTrn.Classify(trnData);
                    ClassificationResult resultTrn = classifierTrn.Vote(trnData, reductGenerator.IdentyficationType, reductGenerator.VoteType, null);

                    RoughClassifier classifierTst = new RoughClassifier();
                    classifierTst.ReductStoreCollection = reductGenerator.GetReductGroups();
                    classifierTst.Classify(tstData);
                    ClassificationResult resultTst = classifierTst.Vote(tstData, reductGenerator.IdentyficationType, reductGenerator.VoteType, null);

                    Console.WriteLine("{0} {1} {2} {3} {4} {5} {6}",
                                      t + 1,
                                      reductGenerator.MaxIterations,
                                      reductGenerator.IterationsPassed - reductGenerator.NumberOfWeightResets,
                                      reductGenerator.NumberOfWeightResets,
                                      resultTrn.WeightMisclassified + resultTrn.WeightUnclassified,
                                      resultTst.WeightMisclassified + resultTst.WeightUnclassified,
                                      reductGenerator.ReductPool.GetAvgMeasure(new ReductMeasureLength()));
                }
            }
        }

        /*
        [Test]
        public void RandomSingletonTest()
        {
            Console.WriteLine(RandomSingleton.Random.Next(1, 5));
            Console.WriteLine(RandomSingleton.Random.Next(1, 5));
            Console.WriteLine(RandomSingleton.Random.Next(1, 5));
            Console.WriteLine(RandomSingleton.Random.Next(1, 5));
            Console.WriteLine(RandomSingleton.Random.Next(1, 5));
            Console.WriteLine(RandomSingleton.Random.Next(1, 5));
            Console.WriteLine(RandomSingleton.Random.Next(1, 5));
            Console.WriteLine(RandomSingleton.Random.Next(1, 5));            
        }
        */
    }
}