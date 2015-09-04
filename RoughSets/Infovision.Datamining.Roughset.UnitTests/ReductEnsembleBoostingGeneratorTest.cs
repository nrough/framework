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
    class ReductEnsembleBoostingGeneratorTest
    {
        public static IEnumerable<Dictionary<string, object>> GetGenerateTestArgs()
        {            
            Random randSeed = new Random();
            int seed = randSeed.Next(Int32.MaxValue);
            //int seed = 1814821014;
            RandomSingleton.Seed = seed;

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

        [Test, TestCaseSource("GetGenerateTestArgs")]        
        public void GenerateTest(Dictionary<string, object> args)
        {            
            Args parms = new Args();
            foreach (KeyValuePair<string, object> kvp in args)            
                parms.AddParameter(kvp.Key, kvp.Value);            

            ReductEnsembleBoostingGenerator reductGenerator = ReductFactory.GetReductGenerator(parms) as ReductEnsembleBoostingGenerator;
            reductGenerator.Generate();

            DataStore data = (DataStore)parms.GetParameter("DataStore");
            RoughClassifier classifierTrn = new RoughClassifier();
            classifierTrn.ReductStoreCollection = reductGenerator.GetReductGroups();
            classifierTrn.Classify(data);
            ClassificationResult resultTrn = classifierTrn.Vote(data, reductGenerator.IdentyficationType, reductGenerator.VoteType, null);            

            Console.WriteLine("{0} {1} {2}",
                              reductGenerator.MaxIterations,
                              resultTrn.WeightMisclassified + resultTrn.WeightUnclassified);
        }
        
        [Test]
        public void GenerateExperiqment()
        {
            Random randSeed = new Random();
            int seed = randSeed.Next(Int32.MaxValue);            
            RandomSingleton.Seed = seed;

            DataStore trnData = DataStore.Load(@"Data\dna_modified.trn", FileFormat.Rses1);
            DataStore tstData = DataStore.Load(@"Data\dna_modified.tst", FileFormat.Rses1, trnData.DataStoreInfo);

            for (int t = 0; t < 100; t++)
            {
                for (int iter = 1; iter <= 500; iter++)
                {
                    Args parms = new Args();
                    parms.AddParameter(ReductGeneratorParamHelper.DataStore, trnData);
                    parms.AddParameter(ReductGeneratorParamHelper.NumberOfThreads, 1);
                    parms.AddParameter(ReductGeneratorParamHelper.FactoryKey, ReductFactoryKeyHelper.ReductEnsembleBoosting);
                    parms.AddParameter(ReductGeneratorParamHelper.IdentificationType, IdentificationType.WeightConfidence);
                    parms.AddParameter(ReductGeneratorParamHelper.VoteType, VoteType.WeightConfidence);
                    parms.AddParameter(ReductGeneratorParamHelper.MinReductLength, 1);
                    parms.AddParameter(ReductGeneratorParamHelper.MaxReductLength, 5);
                    parms.AddParameter(ReductGeneratorParamHelper.Threshold, 0.5);
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

                    Console.WriteLine("{0} {1} {2} {3} {4}",
                                      t + 1,
                                      reductGenerator.MaxIterations,
                                      resultTrn.WeightMisclassified + resultTrn.WeightUnclassified,
                                      resultTst.WeightMisclassified + resultTst.WeightUnclassified,
                                      reductGenerator.ReductPool.GetAvgMeasure(new ReductMeasureLength()));
                }
            }
        }
    }
}
