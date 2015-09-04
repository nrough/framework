using System;
using Infovision.Data;
using Infovision.Utils;
using NUnit.Framework;

namespace Infovision.Datamining.Roughset.UnitTests
{
    [TestFixture]
    public class ApproximateReductTest
    {
        DataStore dataStoreTrain = null;
        DataStore dataStoreTest = null;

        DataStoreInfo dataStoreTrainInfo = null;

        public ApproximateReductTest()
        {
            string trainFileName = @"monks-1.train";
            string testFileName = @"monks-1.test";

            dataStoreTrain = DataStore.Load(trainFileName, FileFormat.Rses1);
            dataStoreTest = DataStore.Load(FileFormat.Rses1, testFileName, dataStoreTrain.DataStoreInfo);

            dataStoreTrainInfo = dataStoreTrain.DataStoreInfo;
        }

        [Test]
        public void ReductStatisticsTest()
        {
            ReductStore reductStore = new ReductStore();
            Reduct reduct = new Reduct(dataStoreTrain, new Int32[] { 1, 2 });
            reductStore.AddReduct(reduct);
            
            foreach (Reduct localReduct in reductStore)
            {
                EquivalenceClassMap result = localReduct.EquivalenceClassMap;
                
                DataVector dataVector = new DataVector(new Int64[] { dataStoreTrainInfo.GetFieldInfo(1).External2Internal(1), 
                                                                     dataStoreTrainInfo.GetFieldInfo(2).External2Internal(1) });
                EquivalenceClassInfo reductStat = result.GetStatistics(dataVector);
                Assert.AreEqual(9, reductStat.NumberOfObjects);
                Assert.AreEqual(1, reductStat.NumberOfDecisions);
                Assert.AreEqual(dataStoreTrainInfo.GetFieldInfo(dataStoreTrainInfo.DecisionFieldId).External2Internal(1), reductStat.MostFrequentDecision);
                Assert.AreEqual(9, reductStat.MostFrequentDecisionCount);
                Assert.AreEqual(9, reductStat.NumberOfObjectsWithDecision(dataStoreTrainInfo.GetFieldInfo(dataStoreTrainInfo.DecisionFieldId).External2Internal(1)));
                Assert.AreEqual(0, reductStat.NumberOfObjectsWithDecision(dataStoreTrainInfo.GetFieldInfo(dataStoreTrainInfo.DecisionFieldId).External2Internal(0)));

                dataVector = new DataVector(new Int64[] { dataStoreTrainInfo.GetFieldInfo(1).External2Internal(1), 
                                                          dataStoreTrainInfo.GetFieldInfo(2).External2Internal(2) });
                reductStat = result.GetStatistics(dataVector);
                Assert.AreEqual(17, reductStat.NumberOfObjects);
                Assert.AreEqual(2, reductStat.NumberOfDecisions);
                Assert.AreEqual(dataStoreTrainInfo.GetFieldInfo(dataStoreTrainInfo.DecisionFieldId).External2Internal(0), reductStat.MostFrequentDecision);
                Assert.AreEqual(15, reductStat.MostFrequentDecisionCount);
                Assert.AreEqual(2, reductStat.NumberOfObjectsWithDecision(dataStoreTrainInfo.GetFieldInfo(dataStoreTrainInfo.DecisionFieldId).External2Internal(1)));
                Assert.AreEqual(15, reductStat.NumberOfObjectsWithDecision(dataStoreTrainInfo.GetFieldInfo(dataStoreTrainInfo.DecisionFieldId).External2Internal(0)));

                dataVector = new DataVector(new Int64[] { dataStoreTrainInfo.GetFieldInfo(1).External2Internal(1), 
                                                          dataStoreTrainInfo.GetFieldInfo(2).External2Internal(3) });
                reductStat = result.GetStatistics(dataVector);
                Assert.AreEqual(19, reductStat.NumberOfObjects);
                Assert.AreEqual(2, reductStat.NumberOfDecisions);
                Assert.AreEqual(dataStoreTrainInfo.GetFieldInfo(dataStoreTrainInfo.DecisionFieldId).External2Internal(0), reductStat.MostFrequentDecision);
                Assert.AreEqual(16, reductStat.MostFrequentDecisionCount);
                Assert.AreEqual(3, reductStat.NumberOfObjectsWithDecision(dataStoreTrainInfo.GetFieldInfo(dataStoreTrainInfo.DecisionFieldId).External2Internal(1)));
                Assert.AreEqual(16, reductStat.NumberOfObjectsWithDecision(dataStoreTrainInfo.GetFieldInfo(dataStoreTrainInfo.DecisionFieldId).External2Internal(0)));

                dataVector = new DataVector(new Int64[] { dataStoreTrainInfo.GetFieldInfo(1).External2Internal(2), 
                                                          dataStoreTrainInfo.GetFieldInfo(2).External2Internal(1) });
                reductStat = result.GetStatistics(dataVector);
                Assert.AreEqual(16, reductStat.NumberOfObjects);
                Assert.AreEqual(2, reductStat.NumberOfDecisions);
                Assert.AreEqual(dataStoreTrainInfo.GetFieldInfo(dataStoreTrainInfo.DecisionFieldId).External2Internal(0), reductStat.MostFrequentDecision);
                Assert.AreEqual(14, reductStat.MostFrequentDecisionCount);
                Assert.AreEqual(2, reductStat.NumberOfObjectsWithDecision(dataStoreTrainInfo.GetFieldInfo(dataStoreTrainInfo.DecisionFieldId).External2Internal(1)));
                Assert.AreEqual(14, reductStat.NumberOfObjectsWithDecision(dataStoreTrainInfo.GetFieldInfo(dataStoreTrainInfo.DecisionFieldId).External2Internal(0)));

                dataVector = new DataVector(new Int64[] { dataStoreTrainInfo.GetFieldInfo(1).External2Internal(2), 
                                                          dataStoreTrainInfo.GetFieldInfo(2).External2Internal(2) });
                reductStat = result.GetStatistics(dataVector);
                Assert.AreEqual(15, reductStat.NumberOfObjects);
                Assert.AreEqual(1, reductStat.NumberOfDecisions);
                Assert.AreEqual(dataStoreTrainInfo.GetFieldInfo(dataStoreTrainInfo.DecisionFieldId).External2Internal(1), reductStat.MostFrequentDecision);
                Assert.AreEqual(15, reductStat.MostFrequentDecisionCount);
                Assert.AreEqual(15, reductStat.NumberOfObjectsWithDecision(dataStoreTrainInfo.GetFieldInfo(dataStoreTrainInfo.DecisionFieldId).External2Internal(1)));
                Assert.AreEqual(0, reductStat.NumberOfObjectsWithDecision(dataStoreTrainInfo.GetFieldInfo(dataStoreTrainInfo.DecisionFieldId).External2Internal(0)));

                dataVector = new DataVector(new Int64[] { dataStoreTrainInfo.GetFieldInfo(1).External2Internal(2), 
                                                          dataStoreTrainInfo.GetFieldInfo(2).External2Internal(3) });
                reductStat = result.GetStatistics(dataVector);
                Assert.AreEqual(11, reductStat.NumberOfObjects);
                Assert.AreEqual(2, reductStat.NumberOfDecisions);
                Assert.AreEqual(dataStoreTrainInfo.GetFieldInfo(dataStoreTrainInfo.DecisionFieldId).External2Internal(0), reductStat.MostFrequentDecision);
                Assert.AreEqual(6, reductStat.MostFrequentDecisionCount);
                Assert.AreEqual(5, reductStat.NumberOfObjectsWithDecision(dataStoreTrainInfo.GetFieldInfo(dataStoreTrainInfo.DecisionFieldId).External2Internal(1)));
                Assert.AreEqual(6, reductStat.NumberOfObjectsWithDecision(dataStoreTrainInfo.GetFieldInfo(dataStoreTrainInfo.DecisionFieldId).External2Internal(0)));

                dataVector = new DataVector(new Int64[] { dataStoreTrainInfo.GetFieldInfo(1).External2Internal(3), 
                                                          dataStoreTrainInfo.GetFieldInfo(2).External2Internal(1) });
                reductStat = result.GetStatistics(dataVector);
                Assert.AreEqual(10, reductStat.NumberOfObjects);
                Assert.AreEqual(2, reductStat.NumberOfDecisions);
                Assert.AreEqual(dataStoreTrainInfo.GetFieldInfo(dataStoreTrainInfo.DecisionFieldId).External2Internal(0), reductStat.MostFrequentDecision);
                Assert.AreEqual(6, reductStat.MostFrequentDecisionCount);
                Assert.AreEqual(4, reductStat.NumberOfObjectsWithDecision(dataStoreTrainInfo.GetFieldInfo(dataStoreTrainInfo.DecisionFieldId).External2Internal(1)));
                Assert.AreEqual(6, reductStat.NumberOfObjectsWithDecision(dataStoreTrainInfo.GetFieldInfo(dataStoreTrainInfo.DecisionFieldId).External2Internal(0)));

                dataVector = new DataVector(new Int64[] { dataStoreTrainInfo.GetFieldInfo(1).External2Internal(3), 
                                                          dataStoreTrainInfo.GetFieldInfo(2).External2Internal(2) });
                reductStat = result.GetStatistics(dataVector);
                Assert.AreEqual(10, reductStat.NumberOfObjects);
                Assert.AreEqual(2, reductStat.NumberOfDecisions);
                //Assert.AreEqual(2, reductStat.MostFrequentDecision);
                Assert.AreEqual(5, reductStat.MostFrequentDecisionCount);
                Assert.AreEqual(5, reductStat.NumberOfObjectsWithDecision(dataStoreTrainInfo.GetFieldInfo(dataStoreTrainInfo.DecisionFieldId).External2Internal(1)));
                Assert.AreEqual(5, reductStat.NumberOfObjectsWithDecision(dataStoreTrainInfo.GetFieldInfo(dataStoreTrainInfo.DecisionFieldId).External2Internal(0)));

                dataVector = new DataVector(new Int64[] { dataStoreTrainInfo.GetFieldInfo(1).External2Internal(3), 
                                                          dataStoreTrainInfo.GetFieldInfo(2).External2Internal(3) });
                reductStat = result.GetStatistics(dataVector);
                Assert.AreEqual(17, reductStat.NumberOfObjects);
                Assert.AreEqual(1, reductStat.NumberOfDecisions);
                Assert.AreEqual(dataStoreTrainInfo.GetFieldInfo(dataStoreTrainInfo.DecisionFieldId).External2Internal(1), reductStat.MostFrequentDecision);
                Assert.AreEqual(17, reductStat.MostFrequentDecisionCount);
                Assert.AreEqual(17, reductStat.NumberOfObjectsWithDecision(dataStoreTrainInfo.GetFieldInfo(dataStoreTrainInfo.DecisionFieldId).External2Internal(1)));
                Assert.AreEqual(0, reductStat.NumberOfObjectsWithDecision(dataStoreTrainInfo.GetFieldInfo(dataStoreTrainInfo.DecisionFieldId).External2Internal(0)));

            }
        }

        [Test]
        public void EmptyReductStatisticsTest()
        {
            ReductStore reductStore = new ReductStore();
            Reduct reduct = new Reduct(dataStoreTrain);

            reductStore.AddReduct(reduct);

            foreach (Reduct localReduct in reductStore)
            {
                EquivalenceClassMap result = localReduct.EquivalenceClassMap;

                DataVector dataVector = new DataVector(new Int64[] {  });
                EquivalenceClassInfo reductStat = result.GetStatistics(dataVector);
                
                Assert.AreEqual(124, reductStat.NumberOfObjects);
                Assert.AreEqual(2, reductStat.NumberOfDecisions);
                Assert.AreEqual(62, reductStat.NumberOfObjectsWithDecision(dataStoreTrainInfo.GetFieldInfo(dataStoreTrainInfo.DecisionFieldId).External2Internal(1)));
                Assert.AreEqual(62, reductStat.NumberOfObjectsWithDecision(dataStoreTrainInfo.GetFieldInfo(dataStoreTrainInfo.DecisionFieldId).External2Internal(0)));
            }
        }

        [Test]
        public void FullAttributeSetReductStatisticsTest()
        {
            ReductStore reductStore = new ReductStore();
            Reduct reduct = new Reduct(dataStoreTrain, new Int32[] { 1, 2, 3, 4, 5, 6 });
            reductStore.AddReduct(reduct);

            foreach (Reduct localReduct in reductStore)
            {
                EquivalenceClassMap result = localReduct.EquivalenceClassMap;
                Assert.AreEqual(124, result.Count);

                DataVector dataVector = new DataVector(new Int64[] {1, 1, 1, 1, 1, 1});
                EquivalenceClassInfo reductStat = result.GetStatistics(dataVector);

                Assert.AreEqual(1, reductStat.NumberOfObjects);
            }
        }

        [Test]
        public void ReductFiltering()
        {
            ReductStore reductStore = new ReductStore();

            reductStore.AddReduct(new Reduct(dataStoreTrain, new Int32[] { 1, 2 }));
            reductStore.AddReduct(new Reduct(dataStoreTrain, new Int32[] { 1, 5, 6 }));
            reductStore.AddReduct(new Reduct(dataStoreTrain, new Int32[] { 1, 2, 3 }));

            //IReductStore filteredReductStore = reductStore.FilterReducts(1, new ReductMeasureNumberOfPartitions());
            IReductStore filteredReductStore = reductStore.FilterReducts(1, new ReductRuleNumberComparer());

            Assert.AreEqual(1, filteredReductStore.Count);

        }

        [Test]
        public void MeasureRelativeTest()
        {
            DataStore localDataStore = DataStore.Load(@"letter.trn", FileFormat.Rses1);

            Reduct reduct = new Reduct(localDataStore, localDataStore.DataStoreInfo.GetFieldIds(FieldTypes.Standard));
            InformationMeasureRelative roughMeasure = new InformationMeasureRelative();
            double result = roughMeasure.Calc(reduct);

            Assert.GreaterOrEqual(result, (double)localDataStore.DataStoreInfo.NumberOfDecisionValues - (0.00001 / (double)localDataStore.DataStoreInfo.NumberOfRecords));
            Assert.LessOrEqual(result, (double)localDataStore.DataStoreInfo.NumberOfDecisionValues + (0.00001 / (double)localDataStore.DataStoreInfo.NumberOfRecords));
        }

        [Test]
        public void MeasureMajorityTest()
        {
            DataStore localDataStore = DataStore.Load(@"letter.trn", FileFormat.Rses1);

            Reduct reduct = new Reduct(localDataStore, localDataStore.DataStoreInfo.GetFieldIds(FieldTypes.Standard));
            InformationMeasureMajority roughMeasure = new InformationMeasureMajority();
            Double result = roughMeasure.Calc(reduct);

            Assert.GreaterOrEqual(result, (Double)1.0 - (0.00001 / (Double)localDataStore.DataStoreInfo.NumberOfRecords));
            Assert.LessOrEqual(result, (Double)1.0 + (0.00001 / (Double)localDataStore.DataStoreInfo.NumberOfRecords));
        }

        [Test]
        public void MeasurePositiveTest()
        {
            DataStore localDataStore = DataStore.Load(@"letter.trn", FileFormat.Rses1);

            Reduct reduct = new Reduct(localDataStore, localDataStore.DataStoreInfo.GetFieldIds(FieldTypes.Standard));
            InformationMeasurePositive roughMeasure = new InformationMeasurePositive();
            Double result = roughMeasure.Calc(reduct);

            Assert.GreaterOrEqual(result, (Double)1.0 - (0.00001 / (Double)localDataStore.DataStoreInfo.NumberOfRecords));
            Assert.LessOrEqual(result, (Double)1.0 + (0.00001 / (Double)localDataStore.DataStoreInfo.NumberOfRecords));
        }

        [Test]
        public void EquivalenceClassMapTest()
        {
            Args parms = new Args(new String[] { "DataStore" }, new Object[] { dataStoreTrain });
            IPermutationGenerator permGen = ReductFactory.GetPermutationGenerator("ApproximateReductRelative", parms);
            PermutationList permutationList = permGen.Generate(1000);
            parms.AddParameter("PermutationList", permutationList);

            IReductGenerator reductGenerator = ReductFactory.GetReductGenerator("ApproximateReductRelative", parms);

            for (Int32 epsilon = 0; epsilon < 100; epsilon+= 11)
            {
                reductGenerator.ApproximationLevel = (double)epsilon/(double)100;

                IReductStore reductStore = reductGenerator.Generate(parms);

                foreach (Reduct reduct in reductStore)
                {
                    EquivalenceClassMap partitionMap = new EquivalenceClassMap(dataStoreTrain.DataStoreInfo);
                    partitionMap.Calc(reduct.AttributeSet, dataStoreTrain);

                    Assert.AreEqual(partitionMap.Count, reduct.EquivalenceClassMap.Count);

                    Int32 objectCount = 0;
                    foreach (DataVector dataVector in partitionMap.Partitions.Keys)
                    {
                        Assert.AreEqual(partitionMap.GetStatistics(dataVector).DecisionValues, reduct.EquivalenceClassMap.GetStatistics(dataVector).DecisionValues);
                        Assert.AreEqual(partitionMap.GetStatistics(dataVector).NumberOfDecisions, reduct.EquivalenceClassMap.GetStatistics(dataVector).NumberOfDecisions);
                        Assert.AreEqual(partitionMap.GetStatistics(dataVector).NumberOfObjects, reduct.EquivalenceClassMap.GetStatistics(dataVector).NumberOfObjects);
                        Assert.AreEqual(partitionMap.GetStatistics(dataVector).MostFrequentDecision, reduct.EquivalenceClassMap.GetStatistics(dataVector).MostFrequentDecision);
                        Assert.AreEqual(partitionMap.GetStatistics(dataVector).MostFrequentDecisionCount, reduct.EquivalenceClassMap.GetStatistics(dataVector).MostFrequentDecisionCount);

                        foreach (Int64 decisionValue in dataStoreTrain.DataStoreInfo.DecisionInfo.InternalValues())
                        {
                            Assert.AreEqual(partitionMap.GetStatistics(dataVector).NumberOfObjectsWithDecision(decisionValue),
                                            reduct.EquivalenceClassMap.GetStatistics(dataVector).NumberOfObjectsWithDecision(decisionValue));
                        }

                        objectCount += reduct.EquivalenceClassMap.GetStatistics(dataVector).NumberOfObjects;
                    }

                    Assert.AreEqual(dataStoreTrain.NumberOfRecords, objectCount);
                }
            }
        }

        [Test]
        public void ReductPositiveTest()
        {
            Args parms = new Args(new String[] { "DataStore" }, new Object[] { dataStoreTrain });
            IPermutationGenerator permGen = ReductFactory.GetPermutationGenerator("ApproximateReductPositive", parms);
            PermutationList permutationList = permGen.Generate(1000);
            parms.AddParameter("PermutationList", permutationList);

            IReductGenerator reductGenerator = ReductFactory.GetReductGenerator("ApproximateReductPositive", parms);
            IReductStore reductStore = reductGenerator.Generate(parms);

            foreach (Reduct reduct in reductStore)
            {
                foreach (EquivalenceClassInfo stat in reduct.EquivalenceClassMap)
                {
                    Assert.AreEqual(1, stat.NumberOfDecisions);
                }
            }
        }

        [Test]
        public void ReductMajorityTest()
        {
            Reduct allAttributes = new Reduct(dataStoreTrain, dataStoreTrain.DataStoreInfo.GetFieldIds(FieldTypes.Standard));
            double allAttrMeasue = InformationMeasureBase.Construct(InformationMeasureType.Majority).Calc(allAttributes);

            Args parms = new Args(new String[] { "DataStore" }, new Object[] { dataStoreTrain });
            IPermutationGenerator permGen = ReductFactory.GetPermutationGenerator("ApproximateReductMajority", parms);
            PermutationList permutationList = permGen.Generate(1000);
            parms.AddParameter("PermutationList", permutationList);

            IReductGenerator reductGenerator = ReductFactory.GetReductGenerator("ApproximateReductMajority", parms);
            IReductStore reductStore = reductGenerator.Generate(parms);

            foreach (IReduct reduct in reductStore)
            {
                Double localAttrMeasue = InformationMeasureBase.Construct(InformationMeasureType.Majority).Calc(reduct);
                Assert.GreaterOrEqual((Double)1 / (Double)dataStoreTrainInfo.NumberOfRecords, allAttrMeasue - localAttrMeasue);
                Assert.LessOrEqual((Double)(-1) / (Double)dataStoreTrainInfo.NumberOfRecords, allAttrMeasue - localAttrMeasue);
            }
        }

        [Test]
        public void ReductRelativeTest()
        {
            Reduct allAttributes = new Reduct(dataStoreTrain, dataStoreTrain.DataStoreInfo.GetFieldIds(FieldTypes.Standard));
            Double allAttrMeasue = InformationMeasureBase.Construct(InformationMeasureType.Relative).Calc(allAttributes);

            Args parms = new Args(new String[] { "DataStore" }, new Object[] { dataStoreTrain });
            IPermutationGenerator permGen = ReductFactory.GetPermutationGenerator("ApproximateReductRelative", parms);
            PermutationList permutationList = permGen.Generate(1000);
            parms.AddParameter("PermutationList", permutationList);

            IReductGenerator reductGenerator = ReductFactory.GetReductGenerator("ApproximateReductRelative", parms);
            IReductStore reductStore = reductGenerator.Generate(parms);

            foreach (IReduct reduct in reductStore)
            {
                Double localAttrMeasue = InformationMeasureBase.Construct(InformationMeasureType.Relative).Calc(reduct);
                Assert.GreaterOrEqual((Double)1/(Double)dataStoreTrainInfo.NumberOfRecords, allAttrMeasue - localAttrMeasue);
                Assert.LessOrEqual((Double)(-1)/(Double)dataStoreTrainInfo.NumberOfRecords, allAttrMeasue - localAttrMeasue);
            }
        }
    }
}
