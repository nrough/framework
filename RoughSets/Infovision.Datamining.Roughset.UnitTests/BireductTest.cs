using System;
using System.Collections.Generic;

using Infovision.Data;
using Infovision.Utils;

using NUnit.Framework;

namespace Infovision.Datamining.Roughset.UnitTests
{
    [TestFixture]
    public class BireductTest
    {
        DataStore dataStoreTrain = null;
        DataStore dataStoreTest = null;

        DataStoreInfo dataStoreTrainInfo = null;

        public BireductTest()
        {
            String trainFileName = @"monks-1.train";
            String testFileName = @"monks-1.test";

            dataStoreTrain = DataStore.Load(trainFileName, FileFormat.Rses1);
            dataStoreTest = DataStore.Load(testFileName, FileFormat.Rses1, dataStoreTrain.DataStoreInfo);

            dataStoreTrainInfo = dataStoreTrain.DataStoreInfo;         
        }

        [Test]
        public void CalcBiReductPositive()
        {
            this.CheckBireductUnique("GammaBireduct");
        }

        [Test]
        public void CalcBiReductMajority()
        {
            this.CheckBireductUnique("Bireduct");
        }

        [Test]
        public void CalcBiReductRelative()
        {
            this.CheckBireductUnique("BireductRelative");
        }

        private void CheckBireductUnique(String reductGeneratorKey)
        {
            Args parms = new Args(new String[] { "DataStore", "NumberOfPermutations" }, 
                                  new Object[] { dataStoreTrain, 100 });
            
            IReductGenerator bireductGenerator = ReductFactory.GetReductGenerator(reductGeneratorKey, parms);
            IReductStore reductStore = bireductGenerator.Generate(parms);

            foreach (IReduct reduct in reductStore)
            {
                foreach (EquivalenceClassInfo reductStat in reduct.EquivalenceClassMap)
                {
                    Assert.AreEqual(1, reductStat.NumberOfDecisions);
                }
            }
        }

        [Test]
        public void RelativeMeasureTest()
        {
            Dictionary<int, double> elementWeights = new Dictionary<int, double>(dataStoreTrain.NumberOfRecords);
            double sumWeights = 0;

            int j = dataStoreTrain.DataStoreInfo.NumberOfFields - 1;

            foreach (int objectIdx in dataStoreTrain.GetObjectIndexes())
            {
                double p = 1 / dataStoreTrain.DataStoreInfo.PriorDecisionProbability(dataStoreTrain.GetDecisionValue(objectIdx));
                
                elementWeights[objectIdx] = p;
                sumWeights += p;
            }

            InformationMeasureRelative roughMeasure = new InformationMeasureRelative();
            Reduct reduct = new Reduct(dataStoreTrain, dataStoreTrainInfo.GetFieldIds(FieldTypes.Standard));

            double r = roughMeasure.Calc(reduct);
            double u = sumWeights / dataStoreTrainInfo.NumberOfRecords;

            //Assert.AreEqual(r, u);
            Assert.GreaterOrEqual(r, u - (0.00001 / (double)dataStoreTrain.DataStoreInfo.NumberOfRecords));
            Assert.LessOrEqual(r, u + (0.00001 / (double)dataStoreTrain.DataStoreInfo.NumberOfRecords));            

        }

        [Test]
        public void BireductMajorityClassifierTest()
        {
            this.Classify("Bireduct");
        }

        [Test]
        public void BireductPositiveClassifierTest()
        {
            this.Classify("GammaBireduct");
        }

        [Test]
        public void BireductRelativeClassifierTest()
        {
            this.Classify("BireductRelative");
        }

        private void Classify(String reductGeneratorKey)
        {
            RoughClassifier classifier = new RoughClassifier();
            classifier.Train(dataStoreTrain, reductGeneratorKey, 50, 10);
            classifier.Classify(dataStoreTest);
            
            ClassificationResult classificationResult = classifier.Vote(dataStoreTest,
                                                                        IdentificationType.Confidence,
                                                                        VoteType.MajorDecision);

            Assert.AreEqual(dataStoreTest.NumberOfRecords, classificationResult.NumberOfClassified
                                                            + classificationResult.NumberOfMisclassified
                                                            + classificationResult.NumberOfUnclassifed);
        }
    }
}
