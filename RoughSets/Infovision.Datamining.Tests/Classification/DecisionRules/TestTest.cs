using Raccoon.Data;
using Raccoon.Core;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Raccoon.MachineLearning.Classification.DecisionLookup;
using Raccoon.MachineLearning.Permutations;
using Raccoon.MachineLearning.Weighting;
using Raccoon.MachineLearning.Roughset;
using Raccoon.MachineLearning.Classification;

namespace Raccoon.MachineLearning.Tests.Classification.UnitTests.DecisionRules
{
    [TestFixture]
    class TestTest
    {
        [Test, Repeat(1)]
        [TestCase(@"Data\monks-2.train", @"Data\monks-2.test")]
        public void NumberOfRulesVsAccuracy(string trainFile, string testFile)
        {
            DataStore data = DataStore.Load(trainFile, FileFormat.RSES1);
            foreach (var fieldInfo in data.DataStoreInfo.Fields) fieldInfo.IsNumeric = false;
            DataStore test = DataStore.Load(testFile, FileFormat.RSES1, data.DataStoreInfo);
            int[] attributes = data.DataStoreInfo.GetFieldIds(FieldGroup.Standard).ToArray();

            WeightGenerator weightGenerator = new WeightGeneratorMajority(data);
            PermutationCollection permutationCollection = new PermutationCollection(100, attributes);                

            Args parms = new Args();
            parms.SetParameter<DataStore>(ReductFactoryOptions.DecisionTable, data);
            parms.SetParameter<string>(ReductFactoryOptions.ReductType, ReductTypes.ApproximateReductMajorityWeights);
            parms.SetParameter<WeightGenerator>(ReductFactoryOptions.WeightGenerator, weightGenerator);
            parms.SetParameter<double>(ReductFactoryOptions.Epsilon, 0.20);
            parms.SetParameter<PermutationCollection>(ReductFactoryOptions.PermutationCollection, permutationCollection);
            parms.SetParameter<bool>(ReductFactoryOptions.UseExceptionRules, false);
            parms.SetParameter<int>(ReductFactoryOptions.NumberOfReducts, 100);
            parms.SetParameter<int>(ReductFactoryOptions.NumberOfReductsToTest, 100);

            IReductGenerator generator = ReductFactory.GetReductGenerator(parms);
            generator.Run();

            foreach (var reductStore in generator.GetReductStoreCollection())
                foreach (var reduct in reductStore)
                {
                    DecisionLookupMajority decTable = new DecisionLookupMajority();
                    decTable.Learn(data, reduct.Attributes.ToArray());
                    ClassificationResult result = Classifier.Default.Classify(decTable, test);
                    Console.WriteLine("{0} {1} {2}", reduct, result.Error, result.NumberOfRules);
                }
        }
    }
}
