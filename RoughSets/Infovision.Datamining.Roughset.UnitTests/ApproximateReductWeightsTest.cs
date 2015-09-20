using System;
using System.Collections.Generic;
using Infovision.Data;
using Infovision.Utils;
using NUnit.Framework;


namespace Infovision.Datamining.Roughset.UnitTests
{
    [TestFixture]
    public class ApproximateReductWeightsTest
    {
        DataStore dataStoreTrain = null;
        DataStore dataStoreTest = null;
        DataStoreInfo dataStoreTrainInfo = null;

        public ApproximateReductWeightsTest()
        {
            string trainFileName = @"Data\monks-1.train";
            string testFileName = @"Data\monks-1.test";

            dataStoreTrain = DataStore.Load(trainFileName, FileFormat.Rses1);
            dataStoreTest = DataStore.Load(testFileName, FileFormat.Rses1, dataStoreTrain.DataStoreInfo);

            dataStoreTrainInfo = dataStoreTrain.DataStoreInfo;
        }

        public Dictionary<string, BenchmarkData> GetDataFiles()
        {
            return BenchmarkDataHelper.GetDataFiles();
        }

        [Test, TestCaseSource("GetDataFiles")]
        public void GenerateZeroEpsilonTest(KeyValuePair<string, BenchmarkData> fileName)
        {            
            DataStore data = DataStore.Load(fileName.Value.TrainFile, FileFormat.Rses1);            
            
            Args parms = new Args();
            parms.AddParameter(ReductGeneratorParamHelper.DataStore, data);
            parms.AddParameter(ReductGeneratorParamHelper.NumberOfThreads, 1);
            parms.AddParameter(ReductGeneratorParamHelper.FactoryKey, ReductFactoryKeyHelper.ApproximateReductMajorityWeights);
            parms.AddParameter(ReductGeneratorParamHelper.NumberOfPermutations, 1);
            
            ReductGeneratorWeightsMajority reductGenerator = ReductFactory.GetReductGenerator(parms) as ReductGeneratorWeightsMajority;
            reductGenerator.Generate();

            Assert.NotNull(reductGenerator.GetReductStoreCollection());
            Assert.NotNull(reductGenerator.ReductPool);
            Assert.AreEqual(1, reductGenerator.ReductPool.Count);
            
            //string fn = String.Format("{0}_ApproximateReductWeightsTest_GenerateZeroEpsilonTest.txt", fileName.Key);
            //using (System.IO.StreamWriter file = new System.IO.StreamWriter(fn))
            //{
            //    foreach (IReduct reduct in reductGenerator.ReductPool)
            //        file.WriteLine(String.Format("{0} {1}", reduct, reduct.Attributes.Count));
            //}
        }
        
        [Test]
        public void ReductStatisticsTest()
        {
            ReductStore reductStore = new ReductStore();
            ReductWeights reduct = new ReductWeights(dataStoreTrain, new int[] { 1, 2 }, new WeightGeneratorMajority(dataStoreTrain).Weights, 0);
            reductStore.AddReduct(reduct);

            foreach (ReductWeights localReduct in reductStore)
            {
                EquivalenceClassCollection result = localReduct.EquivalenceClasses;
                
                AttributeValueVector dataVector = new AttributeValueVector(new int[] { 1, 2 }, new long[] { dataStoreTrainInfo.GetFieldInfo(1).External2Internal(1), 
                                                                     dataStoreTrainInfo.GetFieldInfo(2).External2Internal(1) });

                EquivalenceClass reductStat = result.GetEquivalenceClass(dataVector);
                Assert.AreEqual(9, reductStat.NumberOfObjects);
                Assert.AreEqual(1, reductStat.NumberOfDecisions);
                Assert.AreEqual(dataStoreTrainInfo.GetFieldInfo(dataStoreTrainInfo.DecisionFieldId).External2Internal(1), reductStat.MajorDecision);
                Assert.AreEqual(9, reductStat.GetNumberOfObjectsWithDecision(reductStat.MajorDecision));
                Assert.AreEqual(9, reductStat.GetNumberOfObjectsWithDecision(dataStoreTrainInfo.GetFieldInfo(dataStoreTrainInfo.DecisionFieldId).External2Internal(1)));
                Assert.AreEqual(0, reductStat.GetNumberOfObjectsWithDecision(dataStoreTrainInfo.GetFieldInfo(dataStoreTrainInfo.DecisionFieldId).External2Internal(0)));

                dataVector = new AttributeValueVector(new int[] { 1, 2 }, new long[] { dataStoreTrainInfo.GetFieldInfo(1).External2Internal(1), 
                                                          dataStoreTrainInfo.GetFieldInfo(2).External2Internal(2) });
                reductStat = result.GetEquivalenceClass(dataVector);
                Assert.AreEqual(17, reductStat.NumberOfObjects);
                Assert.AreEqual(2, reductStat.NumberOfDecisions);
                Assert.AreEqual(dataStoreTrainInfo.GetFieldInfo(dataStoreTrainInfo.DecisionFieldId).External2Internal(0), reductStat.MajorDecision);
                Assert.AreEqual(15, reductStat.GetNumberOfObjectsWithDecision(reductStat.MajorDecision));
                Assert.AreEqual(2, reductStat.GetNumberOfObjectsWithDecision(dataStoreTrainInfo.GetFieldInfo(dataStoreTrainInfo.DecisionFieldId).External2Internal(1)));
                Assert.AreEqual(15, reductStat.GetNumberOfObjectsWithDecision(dataStoreTrainInfo.GetFieldInfo(dataStoreTrainInfo.DecisionFieldId).External2Internal(0)));

                dataVector = new AttributeValueVector(new int[] { 1, 2 }, new long[] { dataStoreTrainInfo.GetFieldInfo(1).External2Internal(1), 
                                                          dataStoreTrainInfo.GetFieldInfo(2).External2Internal(3) });
                reductStat = result.GetEquivalenceClass(dataVector);
                Assert.AreEqual(19, reductStat.NumberOfObjects);
                Assert.AreEqual(2, reductStat.NumberOfDecisions);
                Assert.AreEqual(dataStoreTrainInfo.GetFieldInfo(dataStoreTrainInfo.DecisionFieldId).External2Internal(0), reductStat.MajorDecision);
                Assert.AreEqual(16, reductStat.GetNumberOfObjectsWithDecision(reductStat.MajorDecision));
                Assert.AreEqual(3, reductStat.GetNumberOfObjectsWithDecision(dataStoreTrainInfo.GetFieldInfo(dataStoreTrainInfo.DecisionFieldId).External2Internal(1)));
                Assert.AreEqual(16, reductStat.GetNumberOfObjectsWithDecision(dataStoreTrainInfo.GetFieldInfo(dataStoreTrainInfo.DecisionFieldId).External2Internal(0)));

                dataVector = new AttributeValueVector(new int[] { 1, 2 }, new long[] { dataStoreTrainInfo.GetFieldInfo(1).External2Internal(2), 
                                                          dataStoreTrainInfo.GetFieldInfo(2).External2Internal(1) });
                reductStat = result.GetEquivalenceClass(dataVector);
                Assert.AreEqual(16, reductStat.NumberOfObjects);
                Assert.AreEqual(2, reductStat.NumberOfDecisions);
                Assert.AreEqual(dataStoreTrainInfo.GetFieldInfo(dataStoreTrainInfo.DecisionFieldId).External2Internal(0), reductStat.MajorDecision);
                Assert.AreEqual(14, reductStat.GetNumberOfObjectsWithDecision(reductStat.MajorDecision));
                Assert.AreEqual(2, reductStat.GetNumberOfObjectsWithDecision(dataStoreTrainInfo.GetFieldInfo(dataStoreTrainInfo.DecisionFieldId).External2Internal(1)));
                Assert.AreEqual(14, reductStat.GetNumberOfObjectsWithDecision(dataStoreTrainInfo.GetFieldInfo(dataStoreTrainInfo.DecisionFieldId).External2Internal(0)));

                dataVector = new AttributeValueVector(new int[] { 1, 2 }, new long[] { dataStoreTrainInfo.GetFieldInfo(1).External2Internal(2), 
                                                          dataStoreTrainInfo.GetFieldInfo(2).External2Internal(2) });
                reductStat = result.GetEquivalenceClass(dataVector);
                Assert.AreEqual(15, reductStat.NumberOfObjects);
                Assert.AreEqual(1, reductStat.NumberOfDecisions);
                Assert.AreEqual(dataStoreTrainInfo.GetFieldInfo(dataStoreTrainInfo.DecisionFieldId).External2Internal(1), reductStat.MajorDecision);
                Assert.AreEqual(15, reductStat.GetNumberOfObjectsWithDecision(reductStat.MajorDecision));
                Assert.AreEqual(15, reductStat.GetNumberOfObjectsWithDecision(dataStoreTrainInfo.GetFieldInfo(dataStoreTrainInfo.DecisionFieldId).External2Internal(1)));
                Assert.AreEqual(0, reductStat.GetNumberOfObjectsWithDecision(dataStoreTrainInfo.GetFieldInfo(dataStoreTrainInfo.DecisionFieldId).External2Internal(0)));

                dataVector = new AttributeValueVector(new int[] { 1, 2 }, new long[] { dataStoreTrainInfo.GetFieldInfo(1).External2Internal(2), 
                                                          dataStoreTrainInfo.GetFieldInfo(2).External2Internal(3) });
                reductStat = result.GetEquivalenceClass(dataVector);
                Assert.AreEqual(11, reductStat.NumberOfObjects);
                Assert.AreEqual(2, reductStat.NumberOfDecisions);
                Assert.AreEqual(dataStoreTrainInfo.GetFieldInfo(dataStoreTrainInfo.DecisionFieldId).External2Internal(0), reductStat.MajorDecision);
                Assert.AreEqual(6, reductStat.GetNumberOfObjectsWithDecision(reductStat.MajorDecision));
                Assert.AreEqual(5, reductStat.GetNumberOfObjectsWithDecision(dataStoreTrainInfo.GetFieldInfo(dataStoreTrainInfo.DecisionFieldId).External2Internal(1)));
                Assert.AreEqual(6, reductStat.GetNumberOfObjectsWithDecision(dataStoreTrainInfo.GetFieldInfo(dataStoreTrainInfo.DecisionFieldId).External2Internal(0)));

                dataVector = new AttributeValueVector(new int[] { 1, 2 }, new long[] { dataStoreTrainInfo.GetFieldInfo(1).External2Internal(3), 
                                                          dataStoreTrainInfo.GetFieldInfo(2).External2Internal(1) });
                reductStat = result.GetEquivalenceClass(dataVector);
                Assert.AreEqual(10, reductStat.NumberOfObjects);
                Assert.AreEqual(2, reductStat.NumberOfDecisions);
                Assert.AreEqual(dataStoreTrainInfo.GetFieldInfo(dataStoreTrainInfo.DecisionFieldId).External2Internal(0), reductStat.MajorDecision);
                Assert.AreEqual(6, reductStat.GetNumberOfObjectsWithDecision(reductStat.MajorDecision));
                Assert.AreEqual(4, reductStat.GetNumberOfObjectsWithDecision(dataStoreTrainInfo.GetFieldInfo(dataStoreTrainInfo.DecisionFieldId).External2Internal(1)));
                Assert.AreEqual(6, reductStat.GetNumberOfObjectsWithDecision(dataStoreTrainInfo.GetFieldInfo(dataStoreTrainInfo.DecisionFieldId).External2Internal(0)));

                dataVector = new AttributeValueVector(new int[] { 1, 2 }, new long[] { dataStoreTrainInfo.GetFieldInfo(1).External2Internal(3), 
                                                          dataStoreTrainInfo.GetFieldInfo(2).External2Internal(2) });
                reductStat = result.GetEquivalenceClass(dataVector);
                Assert.AreEqual(10, reductStat.NumberOfObjects);
                Assert.AreEqual(2, reductStat.NumberOfDecisions);
                //Assert.AreEqual(2, reductStat.MajorDecision);
                Assert.AreEqual(5, reductStat.GetNumberOfObjectsWithDecision(reductStat.MajorDecision));
                Assert.AreEqual(5, reductStat.GetNumberOfObjectsWithDecision(dataStoreTrainInfo.GetFieldInfo(dataStoreTrainInfo.DecisionFieldId).External2Internal(1)));
                Assert.AreEqual(5, reductStat.GetNumberOfObjectsWithDecision(dataStoreTrainInfo.GetFieldInfo(dataStoreTrainInfo.DecisionFieldId).External2Internal(0)));

                dataVector = new AttributeValueVector(new int[] { 1, 2 }, new long[] { dataStoreTrainInfo.GetFieldInfo(1).External2Internal(3), 
                                                          dataStoreTrainInfo.GetFieldInfo(2).External2Internal(3) });
                reductStat = result.GetEquivalenceClass(dataVector);
                Assert.AreEqual(17, reductStat.NumberOfObjects);
                Assert.AreEqual(1, reductStat.NumberOfDecisions);
                Assert.AreEqual(dataStoreTrainInfo.GetFieldInfo(dataStoreTrainInfo.DecisionFieldId).External2Internal(1), reductStat.MajorDecision);
                Assert.AreEqual(17, reductStat.GetNumberOfObjectsWithDecision(reductStat.MajorDecision));
                Assert.AreEqual(17, reductStat.GetNumberOfObjectsWithDecision(dataStoreTrainInfo.GetFieldInfo(dataStoreTrainInfo.DecisionFieldId).External2Internal(1)));
                Assert.AreEqual(0, reductStat.GetNumberOfObjectsWithDecision(dataStoreTrainInfo.GetFieldInfo(dataStoreTrainInfo.DecisionFieldId).External2Internal(0)));

            }
        }

        [Test]
        public void EmptyReductStatisticsTest()
        {
            ReductStore reductStore = new ReductStore();
            ReductWeights reduct = new ReductWeights(dataStoreTrain, new int[] { }, new WeightGeneratorMajority(dataStoreTrain).Weights, 0);

            reductStore.AddReduct(reduct);

            foreach (ReductWeights localReduct in reductStore)
            {
                EquivalenceClassCollection result = localReduct.EquivalenceClasses;

                AttributeValueVector dataVector = new AttributeValueVector(new int[] { }, new long[] {  });
                EquivalenceClass reductStat = result.GetEquivalenceClass(dataVector);
                
                Assert.AreEqual(124, reductStat.NumberOfObjects);
                Assert.AreEqual(2, reductStat.NumberOfDecisions);
                Assert.AreEqual(62, reductStat.GetNumberOfObjectsWithDecision(dataStoreTrainInfo.GetFieldInfo(dataStoreTrainInfo.DecisionFieldId).External2Internal(1)));
                Assert.AreEqual(62, reductStat.GetNumberOfObjectsWithDecision(dataStoreTrainInfo.GetFieldInfo(dataStoreTrainInfo.DecisionFieldId).External2Internal(0)));
            }
        }

        [Test]
        public void FullAttributeSetReductStatisticsTest()
        {
            ReductStore reductStore = new ReductStore();
            ReductWeights reduct = new ReductWeights(dataStoreTrain, new int[] { 1, 2, 3, 4, 5, 6 }, new WeightGeneratorMajority(dataStoreTrain).Weights, 0);
            reductStore.AddReduct(reduct);

            foreach (ReductWeights localReduct in reductStore)
            {
                EquivalenceClassCollection result = localReduct.EquivalenceClasses;
                Assert.AreEqual(124, result.NumberOfPartitions);

                AttributeValueVector dataVector = new AttributeValueVector(new int[] { 1, 2, 3, 4, 5, 6 }, new long[] {1, 1, 1, 1, 1, 1});
                EquivalenceClass reductStat = result.GetEquivalenceClass(dataVector);

                Assert.AreEqual(1, reductStat.NumberOfObjects);
            }
        }

        [Test]
        public void ReductFiltering()
        {
            ReductStore reductStore = new ReductStore();

            decimal[] weights = new WeightGeneratorMajority(dataStoreTrain).Weights;

            reductStore.AddReduct(new ReductWeights(dataStoreTrain, new int[] { 1, 2 }, weights, 0));
            reductStore.AddReduct(new ReductWeights(dataStoreTrain, new int[] { 1, 5, 6 }, weights, 0));
            reductStore.AddReduct(new ReductWeights(dataStoreTrain, new int[] { 1, 2, 3 }, weights, 0));

            //IReductStore filteredReductStore = reductStore.FilterReducts(1, new ReductMeasureNumberOfPartitions());
            IReductStore filteredReductStore = reductStore.FilterReducts(1, new ReductRuleNumberComparer());

            Assert.AreEqual(1, filteredReductStore.Count);

        }                

        [Test]
        public void MeasureRelativeTest()
        {
            DataStore localDataStore = DataStore.Load(@"Data\letter.trn", FileFormat.Rses1);
            decimal[] weights = new WeightGeneratorRelative(localDataStore).Weights;
            ReductWeights reduct = new ReductWeights(localDataStore, localDataStore.DataStoreInfo.GetFieldIds(FieldTypes.Standard), weights, 0);

            InformationMeasureWeights roughMeasure = new InformationMeasureWeights();
            decimal result = roughMeasure.Calc(reduct);

            Assert.AreEqual(Decimal.Round(result, 17), Decimal.One);
        }

        [Test]
        public void MeasureMajorityTest()
        {
            DataStore localDataStore = DataStore.Load(@"Data\letter.trn", FileFormat.Rses1);
            decimal[] weights = new WeightGeneratorMajority(localDataStore).Weights;
            ReductWeights reduct = new ReductWeights(localDataStore, localDataStore.DataStoreInfo.GetFieldIds(FieldTypes.Standard), weights, 0);

            InformationMeasureWeights roughMeasure = new InformationMeasureWeights();
            decimal result = roughMeasure.Calc(reduct);

            Assert.AreEqual(Decimal.Round(result, 17), Decimal.One);
        }

        [Test]
        public void EquivalenceClassMapTest()
        {
            Args parms = new Args(new string[] { ReductGeneratorParamHelper.FactoryKey, 
                                                 ReductGeneratorParamHelper.DataStore }, 
                                  new Object[] { ReductFactoryKeyHelper.ApproximateReductMajorityWeights, dataStoreTrain });

            IPermutationGenerator permGen = ReductFactory.GetPermutationGenerator(parms);
            PermutationCollection permutationList = permGen.Generate(1000);
            parms.AddParameter(ReductGeneratorParamHelper.PermutationCollection, permutationList);
            parms.AddParameter(ReductGeneratorParamHelper.FactoryKey, ReductFactoryKeyHelper.ApproximateReductMajorityWeights);

            IReductGenerator reductGenerator = ReductFactory.GetReductGenerator(parms);

            for (int epsilon = 0; epsilon < 100; epsilon+= 11)
            {
                reductGenerator.Epsilon = epsilon / 100.0M;

                reductGenerator.Generate();
                IReductStore reductStore = reductGenerator.ReductPool;

                foreach (ReductWeights reduct in reductStore)
                {
                    EquivalenceClassCollection partitionMap = new EquivalenceClassCollection(dataStoreTrain);
                    partitionMap.Calc(reduct.Attributes, dataStoreTrain);

                    Assert.AreEqual(partitionMap.NumberOfPartitions, reduct.EquivalenceClasses.NumberOfPartitions);

                    int objectCount = 0;
                    foreach (AttributeValueVector dataVector in partitionMap.Partitions.Keys)
                    {
                        Assert.AreEqual(partitionMap.GetEquivalenceClass(dataVector).DecisionValues, reduct.EquivalenceClasses.GetEquivalenceClass(dataVector).DecisionValues, "Decision Values");
                        Assert.AreEqual(partitionMap.GetEquivalenceClass(dataVector).NumberOfDecisions, reduct.EquivalenceClasses.GetEquivalenceClass(dataVector).NumberOfDecisions, "Number of Decisions");
                        Assert.AreEqual(partitionMap.GetEquivalenceClass(dataVector).NumberOfObjects, reduct.EquivalenceClasses.GetEquivalenceClass(dataVector).NumberOfObjects, "Number of objects");
                        Assert.AreEqual(partitionMap.GetEquivalenceClass(dataVector).MajorDecision, reduct.EquivalenceClasses.GetEquivalenceClass(dataVector).MajorDecision, "Major Decision");
                        EquivalenceClass partitionEqClass = partitionMap.GetEquivalenceClass(dataVector);
                        EquivalenceClass reductEqClass = reduct.EquivalenceClasses.GetEquivalenceClass(dataVector);
                        Assert.AreEqual(partitionEqClass.GetNumberOfObjectsWithDecision(partitionEqClass.MajorDecision),
                                        reductEqClass.GetNumberOfObjectsWithDecision(reductEqClass.MajorDecision), "Number of objects with major decision");

                        foreach (long decisionValue in dataStoreTrain.DataStoreInfo.DecisionInfo.InternalValues())
                        {
                            Assert.AreEqual(partitionMap.GetEquivalenceClass(dataVector).GetNumberOfObjectsWithDecision(decisionValue),
                                            reduct.EquivalenceClasses.GetEquivalenceClass(dataVector).GetNumberOfObjectsWithDecision(decisionValue), "Numer of objects with decision");
                        }

                        objectCount += reduct.EquivalenceClasses.GetEquivalenceClass(dataVector).NumberOfObjects;
                    }

                    Assert.AreEqual(dataStoreTrain.NumberOfRecords, objectCount);
                }
            }
        }

        [Test]
        public void ReductMajorityTest()
        {
            decimal[] weights = new WeightGeneratorMajority(dataStoreTrain).Weights;
            ReductWeights allAttributes = new ReductWeights(dataStoreTrain, dataStoreTrain.DataStoreInfo.GetFieldIds(FieldTypes.Standard), weights, 0);
            decimal allAttrMeasure = InformationMeasureBase.Construct(InformationMeasureType.Majority).Calc(allAttributes);

            Args parms = new Args(new string[] { ReductGeneratorParamHelper.FactoryKey, 
                                                 ReductGeneratorParamHelper.DataStore }, 
                                  new Object[] { ReductFactoryKeyHelper.ApproximateReductMajorityWeights, dataStoreTrain });
            IPermutationGenerator permGen = ReductFactory.GetPermutationGenerator(parms);
            PermutationCollection permutationList = permGen.Generate(1000);
            parms.AddParameter(ReductGeneratorParamHelper.PermutationCollection, permutationList);
            parms.AddParameter(ReductGeneratorParamHelper.FactoryKey, ReductFactoryKeyHelper.ApproximateReductMajorityWeights);

            IReductGenerator reductGenerator = ReductFactory.GetReductGenerator(parms);
            reductGenerator.Generate();
            IReductStore reductStore = reductGenerator.ReductPool;

            foreach (IReduct reduct in reductStore)
            {
                decimal localAttrMeasure = InformationMeasureBase.Construct(InformationMeasureType.Majority).Calc(reduct);
                Assert.GreaterOrEqual(1.0M / (decimal)dataStoreTrainInfo.NumberOfRecords, allAttrMeasure - localAttrMeasure);
                Assert.LessOrEqual((-1.0M) / (decimal)dataStoreTrainInfo.NumberOfRecords, allAttrMeasure - localAttrMeasure);
            }
        }

        [Test]
        public void ReductRelativeTest()
        {
            decimal[] weights = new WeightGeneratorMajority(dataStoreTrain).Weights;
            ReductWeights allAttributes = new ReductWeights(dataStoreTrain, dataStoreTrain.DataStoreInfo.GetFieldIds(FieldTypes.Standard), weights, 0);
            decimal allAttrMeasure = InformationMeasureBase.Construct(InformationMeasureType.Relative).Calc(allAttributes);

            Args parms = new Args(new string[] { ReductGeneratorParamHelper.FactoryKey, ReductGeneratorParamHelper.DataStore }, new Object[] { ReductFactoryKeyHelper.ApproximateReductRelativeWeights, dataStoreTrain });
            IPermutationGenerator permGen = ReductFactory.GetPermutationGenerator(parms);
            PermutationCollection permutationList = permGen.Generate(1000);
            parms.AddParameter(ReductGeneratorParamHelper.PermutationCollection, permutationList);
            parms.AddParameter(ReductGeneratorParamHelper.FactoryKey, ReductFactoryKeyHelper.ApproximateReductRelativeWeights);

            IReductGenerator reductGenerator = ReductFactory.GetReductGenerator(parms);
            reductGenerator.Generate();
            IReductStore reductStore = reductGenerator.ReductPool;

            foreach (IReduct reduct in reductStore)
            {
                decimal localAttrMeasure = InformationMeasureBase.Construct(InformationMeasureType.Relative).Calc(reduct);
                Assert.GreaterOrEqual(1.0M / (decimal)dataStoreTrainInfo.NumberOfRecords, allAttrMeasure - localAttrMeasure);
                Assert.LessOrEqual((-1.0M) / (decimal)dataStoreTrainInfo.NumberOfRecords, allAttrMeasure - localAttrMeasure);
            }
        }
    }
}
