using Raccoon.Data;
using Raccoon.Core;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Raccoon.MachineLearning.Classification.DecisionTables;
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
            DataStore data = DataStore.Load(trainFile, FileFormat.Rses1);
            foreach (var fieldInfo in data.DataStoreInfo.Fields) fieldInfo.IsNumeric = false;
            DataStore test = DataStore.Load(testFile, FileFormat.Rses1, data.DataStoreInfo);
            int[] attributes = data.DataStoreInfo.GetFieldIds(FieldTypes.Standard).ToArray();

            WeightGenerator weightGenerator = new WeightGeneratorMajority(data);
            PermutationCollection permutationCollection = new PermutationCollection(100, attributes);                

            Args parms = new Args();
            parms.SetParameter<DataStore>(ReductGeneratorParamHelper.TrainData, data);
            parms.SetParameter<string>(ReductGeneratorParamHelper.FactoryKey, ReductFactoryKeyHelper.ApproximateReductMajorityWeights);
            parms.SetParameter<WeightGenerator>(ReductGeneratorParamHelper.WeightGenerator, weightGenerator);
            parms.SetParameter<double>(ReductGeneratorParamHelper.Epsilon, 0.20);
            parms.SetParameter<PermutationCollection>(ReductGeneratorParamHelper.PermutationCollection, permutationCollection);
            parms.SetParameter<bool>(ReductGeneratorParamHelper.UseExceptionRules, false);
            parms.SetParameter<int>(ReductGeneratorParamHelper.NumberOfReducts, 100);
            parms.SetParameter<int>(ReductGeneratorParamHelper.NumberOfReductsToTest, 100);

            IReductGenerator generator = ReductFactory.GetReductGenerator(parms);
            generator.Run();

            foreach (var reductStore in generator.GetReductStoreCollection())
                foreach (var reduct in reductStore)
                {
                    DecisionTableMajority decTable = new DecisionTableMajority();
                    decTable.Learn(data, reduct.Attributes.ToArray());
                    ClassificationResult result = Classifier.DefaultClassifer.Classify(decTable, test);
                    Console.WriteLine("{0} {1} {2}", reduct, result.Error, result.NumberOfRules);
                }
        }
    }
}
