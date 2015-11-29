using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Infovision.Data;
using Infovision.Utils;
using NUnit.Framework;

namespace Infovision.Datamining.Roughset.UnitTests
{
    [TestFixture]
    public class RoughClassifierTest
    {
        [Test]
        public void RoughClassifierNewTest()
        {
            decimal epsilon = 0.1m;
            int numberOfPermutation = 10;
            string reductFactoryKey = ReductFactoryKeyHelper.ApproximateReductMajorityWeights;
            string trainFileName = @"Data\dna_modified.trn";
            string testFileName = @"Data\dna_modified.tst";
            RuleQualityFunction identificationFunc = RuleQuality.ConfidenceW;            
            RuleQualityFunction voteFunc = RuleQuality.CoverageW;
            IdentificationType identificationType = IdentificationType.WeightConfidence;
            VoteType voteType = VoteType.WeightCoverage;


            DataStore trainData = DataStore.Load(trainFileName, FileFormat.Rses1);            
            DataStore testData = DataStore.Load(testFileName, FileFormat.Rses1);            

            Args args = new Args();
            args.AddParameter(ReductGeneratorParamHelper.DataStore, trainData);
            args.AddParameter(ReductGeneratorParamHelper.Epsilon, epsilon);            

            IPermutationGenerator permGen = ReductFactory.GetReductFactory(reductFactoryKey).GetPermutationGenerator(args);
            PermutationCollection permutations = permGen.Generate(numberOfPermutation);
                                   
            args.AddParameter(ReductGeneratorParamHelper.PermutationCollection, permutations);
            args.AddParameter(ReductGeneratorParamHelper.FactoryKey, reductFactoryKey);

            IReductGenerator reductGenerator = ReductFactory.GetReductGenerator(args);
            reductGenerator.Epsilon = epsilon;
            reductGenerator.Generate();
                        
            IReductStoreCollection reductStoreCollection = reductGenerator.GetReductStoreCollection(Int32.MaxValue);
            
            RoughClassifier_OLD classifierOld = new RoughClassifier_OLD();
            classifierOld.ReductStoreCollection = reductStoreCollection;
            classifierOld.ReductStore = reductStoreCollection.FirstOrDefault();

            classifierOld.Classify(testData, null, 0, reductStoreCollection);
            ClassificationResult classificationResultOld = classifierOld.Vote(
                testData,
                identificationType,                
                voteType,
                null);

            RoughClassifier classifier = new RoughClassifier();
            classifier.ReductStoreCollection = reductStoreCollection;
            classifier.ReductStore = reductStoreCollection.FirstOrDefault();

            classifier.Classify(testData, identificationFunc, voteFunc);
            ClassificationResult classificationResult = classifier.Vote(
                testData,
                identificationFunc,
                voteFunc,
                null);

            RoughClassifierNew classifierNew = new RoughClassifierNew(
                reductStoreCollection,
                identificationFunc,
                voteFunc,
                trainData.DataStoreInfo.DecisionInfo.InternalValues());

            ClassificationResult classificationResultNew = classifierNew.Classify(
                testData);

            foreach (long objectId in testData.GetObjectIds())
            {
                long predictionOld = classificationResultOld.GetResult(objectId);
                long prediction = classificationResult.GetResult(objectId);
                long predictionNew = classificationResultNew.GetResult(objectId);

                Assert.AreEqual(predictionOld, prediction, "Old vs Current");
                Assert.AreEqual(predictionOld, predictionNew, "Old vs New");
            }

            Assert.True(true);

        }
    }
}
