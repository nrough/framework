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
            argSet.Add(ReductGeneratorParamHelper.VoteType, IdentificationType.Coverage);
            argSet.Add(ReductGeneratorParamHelper.MaxReductLength, 3);
            argSet.Add(ReductGeneratorParamHelper.Threshold, 0.5);
            argSet.Add(ReductGeneratorParamHelper.NumberOfReductsInWeakClassifier, 1);
            argSet.Add(ReductGeneratorParamHelper.MaxIterations, 1);
            argsList.Add(argSet);

            argSet = new Dictionary<string, object>();
            argSet.Add(ReductGeneratorParamHelper.DataStore, data);
            argSet.Add(ReductGeneratorParamHelper.NumberOfThreads, 1);
            argSet.Add(ReductGeneratorParamHelper.FactoryKey, ReductFactoryKeyHelper.ReductEnsembleBoosting);
            argSet.Add(ReductGeneratorParamHelper.IdentificationType, IdentificationType.Confidence);
            argSet.Add(ReductGeneratorParamHelper.VoteType, IdentificationType.Coverage);
            argSet.Add(ReductGeneratorParamHelper.MaxReductLength, 3);
            argSet.Add(ReductGeneratorParamHelper.Threshold, 0.5);
            argSet.Add(ReductGeneratorParamHelper.NumberOfReductsInWeakClassifier, 1);
            argSet.Add(ReductGeneratorParamHelper.MaxIterations, 3);
            argsList.Add(argSet);

            argSet = new Dictionary<string, object>();
            argSet.Add(ReductGeneratorParamHelper.DataStore, data);
            argSet.Add(ReductGeneratorParamHelper.NumberOfThreads, 1);
            argSet.Add(ReductGeneratorParamHelper.FactoryKey, ReductFactoryKeyHelper.ReductEnsembleBoosting);
            argSet.Add(ReductGeneratorParamHelper.IdentificationType, IdentificationType.Confidence);
            argSet.Add(ReductGeneratorParamHelper.VoteType, IdentificationType.Coverage);
            argSet.Add(ReductGeneratorParamHelper.MaxReductLength, 3);
            argSet.Add(ReductGeneratorParamHelper.Threshold, 0.5);
            argSet.Add(ReductGeneratorParamHelper.NumberOfReductsInWeakClassifier, 1);
            argSet.Add(ReductGeneratorParamHelper.MaxIterations, 5);
            argsList.Add(argSet);


            argSet = new Dictionary<string, object>();
            argSet.Add(ReductGeneratorParamHelper.DataStore, data);
            argSet.Add(ReductGeneratorParamHelper.NumberOfThreads, 1);                                    
            argSet.Add(ReductGeneratorParamHelper.FactoryKey, ReductFactoryKeyHelper.ReductEnsembleBoosting);
            argSet.Add(ReductGeneratorParamHelper.IdentificationType, IdentificationType.Confidence);
            argSet.Add(ReductGeneratorParamHelper.VoteType, IdentificationType.Coverage);
            argSet.Add(ReductGeneratorParamHelper.MaxReductLength, 3);
            argSet.Add(ReductGeneratorParamHelper.Threshold, 0.5);
            argSet.Add(ReductGeneratorParamHelper.NumberOfReductsInWeakClassifier, 1);
            argSet.Add(ReductGeneratorParamHelper.MaxIterations, 10);                        
            argsList.Add(argSet);

            argSet = new Dictionary<string, object>();
            argSet.Add(ReductGeneratorParamHelper.DataStore, data);
            argSet.Add(ReductGeneratorParamHelper.NumberOfThreads, 1);
            argSet.Add(ReductGeneratorParamHelper.FactoryKey, ReductFactoryKeyHelper.ReductEnsembleBoosting);
            argSet.Add(ReductGeneratorParamHelper.IdentificationType, IdentificationType.Confidence);
            argSet.Add(ReductGeneratorParamHelper.VoteType, IdentificationType.Coverage);
            argSet.Add(ReductGeneratorParamHelper.MaxReductLength, 3);
            argSet.Add(ReductGeneratorParamHelper.Threshold, 0.5);
            argSet.Add(ReductGeneratorParamHelper.NumberOfReductsInWeakClassifier, 1);
            argSet.Add(ReductGeneratorParamHelper.MaxIterations, 20);
            argsList.Add(argSet);

            argSet = new Dictionary<string, object>();
            argSet.Add(ReductGeneratorParamHelper.DataStore, data);
            argSet.Add(ReductGeneratorParamHelper.NumberOfThreads, 1);
            argSet.Add(ReductGeneratorParamHelper.FactoryKey, ReductFactoryKeyHelper.ReductEnsembleBoosting);
            argSet.Add(ReductGeneratorParamHelper.IdentificationType, IdentificationType.Confidence);
            argSet.Add(ReductGeneratorParamHelper.VoteType, IdentificationType.Coverage);
            argSet.Add(ReductGeneratorParamHelper.MaxReductLength, 3);
            argSet.Add(ReductGeneratorParamHelper.Threshold, 0.5);
            argSet.Add(ReductGeneratorParamHelper.NumberOfReductsInWeakClassifier, 1);
            argSet.Add(ReductGeneratorParamHelper.MaxIterations, 30);
            argsList.Add(argSet);

            argSet = new Dictionary<string, object>();
            argSet.Add(ReductGeneratorParamHelper.DataStore, data);
            argSet.Add(ReductGeneratorParamHelper.NumberOfThreads, 1);
            argSet.Add(ReductGeneratorParamHelper.FactoryKey, ReductFactoryKeyHelper.ReductEnsembleBoosting);
            argSet.Add(ReductGeneratorParamHelper.IdentificationType, IdentificationType.Confidence);
            argSet.Add(ReductGeneratorParamHelper.VoteType, IdentificationType.Coverage);
            argSet.Add(ReductGeneratorParamHelper.MaxReductLength, 3);
            argSet.Add(ReductGeneratorParamHelper.Threshold, 0.5);
            argSet.Add(ReductGeneratorParamHelper.NumberOfReductsInWeakClassifier, 1);
            argSet.Add(ReductGeneratorParamHelper.MaxIterations, 40);
            argsList.Add(argSet);

            argSet = new Dictionary<string, object>();
            argSet.Add(ReductGeneratorParamHelper.DataStore, data);
            argSet.Add(ReductGeneratorParamHelper.NumberOfThreads, 1);
            argSet.Add(ReductGeneratorParamHelper.FactoryKey, ReductFactoryKeyHelper.ReductEnsembleBoosting);
            argSet.Add(ReductGeneratorParamHelper.IdentificationType, IdentificationType.Confidence);
            argSet.Add(ReductGeneratorParamHelper.VoteType, IdentificationType.Coverage);
            argSet.Add(ReductGeneratorParamHelper.MaxReductLength, 3);
            argSet.Add(ReductGeneratorParamHelper.Threshold, 0.5);
            argSet.Add(ReductGeneratorParamHelper.NumberOfReductsInWeakClassifier, 1);
            argSet.Add(ReductGeneratorParamHelper.MaxIterations, 50);
            argsList.Add(argSet);

            argSet = new Dictionary<string, object>();
            argSet.Add(ReductGeneratorParamHelper.DataStore, data);
            argSet.Add(ReductGeneratorParamHelper.NumberOfThreads, 1);
            argSet.Add(ReductGeneratorParamHelper.FactoryKey, ReductFactoryKeyHelper.ReductEnsembleBoosting);
            argSet.Add(ReductGeneratorParamHelper.IdentificationType, IdentificationType.Confidence);
            argSet.Add(ReductGeneratorParamHelper.VoteType, IdentificationType.Coverage);
            argSet.Add(ReductGeneratorParamHelper.MaxReductLength, 3);
            argSet.Add(ReductGeneratorParamHelper.Threshold, 0.5);
            argSet.Add(ReductGeneratorParamHelper.NumberOfReductsInWeakClassifier, 1);
            argSet.Add(ReductGeneratorParamHelper.MaxIterations, 60);
            argsList.Add(argSet);

            argSet = new Dictionary<string, object>();
            argSet.Add(ReductGeneratorParamHelper.DataStore, data);
            argSet.Add(ReductGeneratorParamHelper.NumberOfThreads, 1);
            argSet.Add(ReductGeneratorParamHelper.FactoryKey, ReductFactoryKeyHelper.ReductEnsembleBoosting);
            argSet.Add(ReductGeneratorParamHelper.IdentificationType, IdentificationType.Confidence);
            argSet.Add(ReductGeneratorParamHelper.VoteType, IdentificationType.Coverage);
            argSet.Add(ReductGeneratorParamHelper.MaxReductLength, 3);
            argSet.Add(ReductGeneratorParamHelper.Threshold, 0.5);
            argSet.Add(ReductGeneratorParamHelper.NumberOfReductsInWeakClassifier, 1);
            argSet.Add(ReductGeneratorParamHelper.MaxIterations, 70);
            argsList.Add(argSet);

            argSet = new Dictionary<string, object>();
            argSet.Add(ReductGeneratorParamHelper.DataStore, data);
            argSet.Add(ReductGeneratorParamHelper.NumberOfThreads, 1);
            argSet.Add(ReductGeneratorParamHelper.FactoryKey, ReductFactoryKeyHelper.ReductEnsembleBoosting);
            argSet.Add(ReductGeneratorParamHelper.IdentificationType, IdentificationType.Confidence);
            argSet.Add(ReductGeneratorParamHelper.VoteType, IdentificationType.Coverage);
            argSet.Add(ReductGeneratorParamHelper.MaxReductLength, 3);
            argSet.Add(ReductGeneratorParamHelper.Threshold, 0.5);
            argSet.Add(ReductGeneratorParamHelper.NumberOfReductsInWeakClassifier, 1);
            argSet.Add(ReductGeneratorParamHelper.MaxIterations, 80);
            argsList.Add(argSet);

            argSet = new Dictionary<string, object>();
            argSet.Add(ReductGeneratorParamHelper.DataStore, data);
            argSet.Add(ReductGeneratorParamHelper.NumberOfThreads, 1);
            argSet.Add(ReductGeneratorParamHelper.FactoryKey, ReductFactoryKeyHelper.ReductEnsembleBoosting);
            argSet.Add(ReductGeneratorParamHelper.IdentificationType, IdentificationType.Confidence);
            argSet.Add(ReductGeneratorParamHelper.VoteType, IdentificationType.Coverage);
            argSet.Add(ReductGeneratorParamHelper.MaxReductLength, 3);
            argSet.Add(ReductGeneratorParamHelper.Threshold, 0.5);
            argSet.Add(ReductGeneratorParamHelper.NumberOfReductsInWeakClassifier, 1);
            argSet.Add(ReductGeneratorParamHelper.MaxIterations, 90);
            argsList.Add(argSet);

            argSet = new Dictionary<string, object>();
            argSet.Add(ReductGeneratorParamHelper.DataStore, data);
            argSet.Add(ReductGeneratorParamHelper.NumberOfThreads, 1);
            argSet.Add(ReductGeneratorParamHelper.FactoryKey, ReductFactoryKeyHelper.ReductEnsembleBoosting);
            argSet.Add(ReductGeneratorParamHelper.IdentificationType, IdentificationType.Confidence);
            argSet.Add(ReductGeneratorParamHelper.VoteType, IdentificationType.Coverage);
            argSet.Add(ReductGeneratorParamHelper.MaxReductLength, 3);
            argSet.Add(ReductGeneratorParamHelper.Threshold, 0.5);
            argSet.Add(ReductGeneratorParamHelper.NumberOfReductsInWeakClassifier, 1);
            argSet.Add(ReductGeneratorParamHelper.MaxIterations, 100);
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
            RoughClassifier classifier = new RoughClassifier();
            classifier.ReductStoreCollection = reductGenerator.GetReductGroups();
            classifier.Classify(data);
            ClassificationResult result = classifier.Vote(data, reductGenerator.IdentyficationType, reductGenerator.VoteType);

            Console.WriteLine("{0} {1}", reductGenerator.MaxIterations, result.Accuracy);
        }
    }
}
