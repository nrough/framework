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
            string trainFileName = @"Data\monks-1.train";
            string testFileName = @"Data\monks-1.test";

            dataStoreTrain = DataStore.Load(trainFileName, FileFormat.Rses1);
            dataStoreTest = DataStore.Load(testFileName, FileFormat.Rses1, dataStoreTrain.DataStoreInfo);

            dataStoreTrainInfo = dataStoreTrain.DataStoreInfo;
        }

        [Test]
        public void ReductStatisticsTest()
        {
            ReductStore reductStore = new ReductStore();
            Reduct reduct = new Reduct(dataStoreTrain, new int[] { 1, 2 }, 0);
            reductStore.AddReduct(reduct);
            
            foreach (Reduct localReduct in reductStore)
            {
                EquivalenceClassMap result = localReduct.EquivalenceClassMap;
                
                DataVector dataVector = new DataVector(new Int64[] { dataStoreTrainInfo.GetFieldInfo(1).External2Internal(1), 
                                                                     dataStoreTrainInfo.GetFieldInfo(2).External2Internal(1) });
                EquivalenceClass reductStat = result.GetEquivalenceClass(dataVector);
                Assert.AreEqual(9, reductStat.NumberOfObjects);
                Assert.AreEqual(1, reductStat.NumberOfDecisions);
                Assert.AreEqual(dataStoreTrainInfo.GetFieldInfo(dataStoreTrainInfo.DecisionFieldId).External2Internal(1), reductStat.MajorDecision);
                Assert.AreEqual(9, reductStat.NumberOfObjectsWithDecision(reductStat.MajorDecision));
                Assert.AreEqual(9, reductStat.NumberOfObjectsWithDecision(dataStoreTrainInfo.GetFieldInfo(dataStoreTrainInfo.DecisionFieldId).External2Internal(1)));
                Assert.AreEqual(0, reductStat.NumberOfObjectsWithDecision(dataStoreTrainInfo.GetFieldInfo(dataStoreTrainInfo.DecisionFieldId).External2Internal(0)));

                dataVector = new DataVector(new Int64[] { dataStoreTrainInfo.GetFieldInfo(1).External2Internal(1), 
                                                          dataStoreTrainInfo.GetFieldInfo(2).External2Internal(2) });
                reductStat = result.GetEquivalenceClass(dataVector);
                Assert.AreEqual(17, reductStat.NumberOfObjects);
                Assert.AreEqual(2, reductStat.NumberOfDecisions);
                Assert.AreEqual(dataStoreTrainInfo.GetFieldInfo(dataStoreTrainInfo.DecisionFieldId).External2Internal(0), reductStat.MajorDecision);
                Assert.AreEqual(15, reductStat.NumberOfObjectsWithDecision(reductStat.MajorDecision));
                Assert.AreEqual(2, reductStat.NumberOfObjectsWithDecision(dataStoreTrainInfo.GetFieldInfo(dataStoreTrainInfo.DecisionFieldId).External2Internal(1)));
                Assert.AreEqual(15, reductStat.NumberOfObjectsWithDecision(dataStoreTrainInfo.GetFieldInfo(dataStoreTrainInfo.DecisionFieldId).External2Internal(0)));

                dataVector = new DataVector(new Int64[] { dataStoreTrainInfo.GetFieldInfo(1).External2Internal(1), 
                                                          dataStoreTrainInfo.GetFieldInfo(2).External2Internal(3) });
                reductStat = result.GetEquivalenceClass(dataVector);
                Assert.AreEqual(19, reductStat.NumberOfObjects);
                Assert.AreEqual(2, reductStat.NumberOfDecisions);
                Assert.AreEqual(dataStoreTrainInfo.GetFieldInfo(dataStoreTrainInfo.DecisionFieldId).External2Internal(0), reductStat.MajorDecision);
                Assert.AreEqual(16, reductStat.NumberOfObjectsWithDecision(reductStat.MajorDecision));
                Assert.AreEqual(3, reductStat.NumberOfObjectsWithDecision(dataStoreTrainInfo.GetFieldInfo(dataStoreTrainInfo.DecisionFieldId).External2Internal(1)));
                Assert.AreEqual(16, reductStat.NumberOfObjectsWithDecision(dataStoreTrainInfo.GetFieldInfo(dataStoreTrainInfo.DecisionFieldId).External2Internal(0)));

                dataVector = new DataVector(new Int64[] { dataStoreTrainInfo.GetFieldInfo(1).External2Internal(2), 
                                                          dataStoreTrainInfo.GetFieldInfo(2).External2Internal(1) });
                reductStat = result.GetEquivalenceClass(dataVector);
                Assert.AreEqual(16, reductStat.NumberOfObjects);
                Assert.AreEqual(2, reductStat.NumberOfDecisions);
                Assert.AreEqual(dataStoreTrainInfo.GetFieldInfo(dataStoreTrainInfo.DecisionFieldId).External2Internal(0), reductStat.MajorDecision);
                Assert.AreEqual(14, reductStat.NumberOfObjectsWithDecision(reductStat.MajorDecision));
                Assert.AreEqual(2, reductStat.NumberOfObjectsWithDecision(dataStoreTrainInfo.GetFieldInfo(dataStoreTrainInfo.DecisionFieldId).External2Internal(1)));
                Assert.AreEqual(14, reductStat.NumberOfObjectsWithDecision(dataStoreTrainInfo.GetFieldInfo(dataStoreTrainInfo.DecisionFieldId).External2Internal(0)));

                dataVector = new DataVector(new Int64[] { dataStoreTrainInfo.GetFieldInfo(1).External2Internal(2), 
                                                          dataStoreTrainInfo.GetFieldInfo(2).External2Internal(2) });
                reductStat = result.GetEquivalenceClass(dataVector);
                Assert.AreEqual(15, reductStat.NumberOfObjects);
                Assert.AreEqual(1, reductStat.NumberOfDecisions);
                Assert.AreEqual(dataStoreTrainInfo.GetFieldInfo(dataStoreTrainInfo.DecisionFieldId).External2Internal(1), reductStat.MajorDecision);
                Assert.AreEqual(15, reductStat.NumberOfObjectsWithDecision(reductStat.MajorDecision));
                Assert.AreEqual(15, reductStat.NumberOfObjectsWithDecision(dataStoreTrainInfo.GetFieldInfo(dataStoreTrainInfo.DecisionFieldId).External2Internal(1)));
                Assert.AreEqual(0, reductStat.NumberOfObjectsWithDecision(dataStoreTrainInfo.GetFieldInfo(dataStoreTrainInfo.DecisionFieldId).External2Internal(0)));

                dataVector = new DataVector(new Int64[] { dataStoreTrainInfo.GetFieldInfo(1).External2Internal(2), 
                                                          dataStoreTrainInfo.GetFieldInfo(2).External2Internal(3) });
                reductStat = result.GetEquivalenceClass(dataVector);
                Assert.AreEqual(11, reductStat.NumberOfObjects);
                Assert.AreEqual(2, reductStat.NumberOfDecisions);
                Assert.AreEqual(dataStoreTrainInfo.GetFieldInfo(dataStoreTrainInfo.DecisionFieldId).External2Internal(0), reductStat.MajorDecision);
                Assert.AreEqual(6, reductStat.NumberOfObjectsWithDecision(reductStat.MajorDecision));
                Assert.AreEqual(5, reductStat.NumberOfObjectsWithDecision(dataStoreTrainInfo.GetFieldInfo(dataStoreTrainInfo.DecisionFieldId).External2Internal(1)));
                Assert.AreEqual(6, reductStat.NumberOfObjectsWithDecision(dataStoreTrainInfo.GetFieldInfo(dataStoreTrainInfo.DecisionFieldId).External2Internal(0)));

                dataVector = new DataVector(new Int64[] { dataStoreTrainInfo.GetFieldInfo(1).External2Internal(3), 
                                                          dataStoreTrainInfo.GetFieldInfo(2).External2Internal(1) });
                reductStat = result.GetEquivalenceClass(dataVector);
                Assert.AreEqual(10, reductStat.NumberOfObjects);
                Assert.AreEqual(2, reductStat.NumberOfDecisions);
                Assert.AreEqual(dataStoreTrainInfo.GetFieldInfo(dataStoreTrainInfo.DecisionFieldId).External2Internal(0), reductStat.MajorDecision);
                Assert.AreEqual(6, reductStat.NumberOfObjectsWithDecision(reductStat.MajorDecision));
                Assert.AreEqual(4, reductStat.NumberOfObjectsWithDecision(dataStoreTrainInfo.GetFieldInfo(dataStoreTrainInfo.DecisionFieldId).External2Internal(1)));
                Assert.AreEqual(6, reductStat.NumberOfObjectsWithDecision(dataStoreTrainInfo.GetFieldInfo(dataStoreTrainInfo.DecisionFieldId).External2Internal(0)));

                dataVector = new DataVector(new Int64[] { dataStoreTrainInfo.GetFieldInfo(1).External2Internal(3), 
                                                          dataStoreTrainInfo.GetFieldInfo(2).External2Internal(2) });
                reductStat = result.GetEquivalenceClass(dataVector);
                Assert.AreEqual(10, reductStat.NumberOfObjects);
                Assert.AreEqual(2, reductStat.NumberOfDecisions);
                //Assert.AreEqual(2, reductStat.MajorDecision);
                Assert.AreEqual(5, reductStat.NumberOfObjectsWithDecision(reductStat.MajorDecision));
                Assert.AreEqual(5, reductStat.NumberOfObjectsWithDecision(dataStoreTrainInfo.GetFieldInfo(dataStoreTrainInfo.DecisionFieldId).External2Internal(1)));
                Assert.AreEqual(5, reductStat.NumberOfObjectsWithDecision(dataStoreTrainInfo.GetFieldInfo(dataStoreTrainInfo.DecisionFieldId).External2Internal(0)));

                dataVector = new DataVector(new Int64[] { dataStoreTrainInfo.GetFieldInfo(1).External2Internal(3), 
                                                          dataStoreTrainInfo.GetFieldInfo(2).External2Internal(3) });
                reductStat = result.GetEquivalenceClass(dataVector);
                Assert.AreEqual(17, reductStat.NumberOfObjects);
                Assert.AreEqual(1, reductStat.NumberOfDecisions);
                Assert.AreEqual(dataStoreTrainInfo.GetFieldInfo(dataStoreTrainInfo.DecisionFieldId).External2Internal(1), reductStat.MajorDecision);
                Assert.AreEqual(17, reductStat.NumberOfObjectsWithDecision(reductStat.MajorDecision));
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
                EquivalenceClass reductStat = result.GetEquivalenceClass(dataVector);
                
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
            Reduct reduct = new Reduct(dataStoreTrain, new int[] { 1, 2, 3, 4, 5, 6 }, 0);
            reductStore.AddReduct(reduct);

            foreach (Reduct localReduct in reductStore)
            {
                EquivalenceClassMap result = localReduct.EquivalenceClassMap;
                Assert.AreEqual(124, result.Count);

                DataVector dataVector = new DataVector(new Int64[] {1, 1, 1, 1, 1, 1});
                EquivalenceClass reductStat = result.GetEquivalenceClass(dataVector);

                Assert.AreEqual(1, reductStat.NumberOfObjects);
            }
        }

        [Test]
        public void ReductFiltering()
        {
            ReductStore reductStore = new ReductStore();

            reductStore.AddReduct(new Reduct(dataStoreTrain, new int[] { 1, 2 }, 0));
            reductStore.AddReduct(new Reduct(dataStoreTrain, new int[] { 1, 5, 6 }, 0));
            reductStore.AddReduct(new Reduct(dataStoreTrain, new int[] { 1, 2, 3 }, 0));

            //IReductStore filteredReductStore = reductStore.FilterReducts(1, new ReductMeasureNumberOfPartitions());
            IReductStore filteredReductStore = reductStore.FilterReducts(1, new ReductRuleNumberComparer());

            Assert.AreEqual(1, filteredReductStore.Count);

        }

        [Test]
        public void MeasureRelativeTest()
        {
            DataStore localDataStore = DataStore.Load(@"Data\letter.trn", FileFormat.Rses1);

            Reduct reduct = new Reduct(localDataStore, localDataStore.DataStoreInfo.GetFieldIds(FieldTypes.Standard), 0);
            InformationMeasureRelative roughMeasure = new InformationMeasureRelative();
            double result = roughMeasure.Calc(reduct);

            Assert.GreaterOrEqual(result, (double)localDataStore.DataStoreInfo.NumberOfDecisionValues - (0.00001 / (double)localDataStore.DataStoreInfo.NumberOfRecords));
            Assert.LessOrEqual(result, (double)localDataStore.DataStoreInfo.NumberOfDecisionValues + (0.00001 / (double)localDataStore.DataStoreInfo.NumberOfRecords));
        }

        [Test]
        public void MeasureMajorityTest()
        {
            DataStore localDataStore = DataStore.Load(@"Data\letter.trn", FileFormat.Rses1);

            Reduct reduct = new Reduct(localDataStore, localDataStore.DataStoreInfo.GetFieldIds(FieldTypes.Standard), 0);
            InformationMeasureMajority roughMeasure = new InformationMeasureMajority();
            double result = roughMeasure.Calc(reduct);

            Assert.GreaterOrEqual(result, (double)1.0 - (0.00001 / (double)localDataStore.DataStoreInfo.NumberOfRecords));
            Assert.LessOrEqual(result, (double)1.0 + (0.00001 / (double)localDataStore.DataStoreInfo.NumberOfRecords));
        }

        [Test]
        public void MeasurePositiveTest()
        {
            DataStore localDataStore = DataStore.Load(@"Data\letter.trn", FileFormat.Rses1);

            Reduct reduct = new Reduct(localDataStore, localDataStore.DataStoreInfo.GetFieldIds(FieldTypes.Standard), 0);
            InformationMeasurePositive roughMeasure = new InformationMeasurePositive();
            double result = roughMeasure.Calc(reduct);

            Assert.GreaterOrEqual(result, (double)1.0 - (0.00001 / (double)localDataStore.DataStoreInfo.NumberOfRecords));
            Assert.LessOrEqual(result, (double)1.0 + (0.00001 / (double)localDataStore.DataStoreInfo.NumberOfRecords));
        }

        [Test]
        public void EquivalenceClassMapTest()
        {
            Args parms = new Args(new string[] { "DataStore" }, new Object[] { dataStoreTrain });
            IPermutationGenerator permGen = ReductFactory.GetPermutationGenerator("ApproximateReductRelative", parms);
            PermutationCollection permutationList = permGen.Generate(1000);
            parms.AddParameter("PermutationCollection", permutationList);
            parms.AddParameter("FactoryKey", "ApproximateReductRelative");

            IReductGenerator reductGenerator = ReductFactory.GetReductGenerator(parms);

            for (int epsilon = 0; epsilon < 100; epsilon+= 11)
            {
                reductGenerator.ApproximationDegree = epsilon / 100.0;

                IReductStore reductStore = reductGenerator.Generate(parms).First();

                foreach (Reduct reduct in reductStore)
                {
                    EquivalenceClassMap partitionMap = new EquivalenceClassMap(dataStoreTrain.DataStoreInfo);
                    partitionMap.Calc(reduct.Attributes, dataStoreTrain);

                    Assert.AreEqual(partitionMap.Count, reduct.EquivalenceClassMap.Count);

                    int objectCount = 0;
                    foreach (DataVector dataVector in partitionMap.Partitions.Keys)
                    {
                        Assert.AreEqual(partitionMap.GetEquivalenceClass(dataVector).DecisionValues, reduct.EquivalenceClassMap.GetEquivalenceClass(dataVector).DecisionValues, "Decision Values");
                        Assert.AreEqual(partitionMap.GetEquivalenceClass(dataVector).NumberOfDecisions, reduct.EquivalenceClassMap.GetEquivalenceClass(dataVector).NumberOfDecisions, "Number of Decisions");
                        Assert.AreEqual(partitionMap.GetEquivalenceClass(dataVector).NumberOfObjects, reduct.EquivalenceClassMap.GetEquivalenceClass(dataVector).NumberOfObjects, "Number of objects");
                        Assert.AreEqual(partitionMap.GetEquivalenceClass(dataVector).MajorDecision, reduct.EquivalenceClassMap.GetEquivalenceClass(dataVector).MajorDecision, "Major Decision");
                        EquivalenceClass partitionEqClass = partitionMap.GetEquivalenceClass(dataVector);
                        EquivalenceClass reductEqClass = reduct.EquivalenceClassMap.GetEquivalenceClass(dataVector);
                        Assert.AreEqual(partitionEqClass.NumberOfObjectsWithDecision(partitionEqClass.MajorDecision),
                                        reductEqClass.NumberOfObjectsWithDecision(reductEqClass.MajorDecision), "Number of objects with major decision");

                        foreach (long decisionValue in dataStoreTrain.DataStoreInfo.DecisionInfo.InternalValues())
                        {
                            Assert.AreEqual(partitionMap.GetEquivalenceClass(dataVector).NumberOfObjectsWithDecision(decisionValue),
                                            reduct.EquivalenceClassMap.GetEquivalenceClass(dataVector).NumberOfObjectsWithDecision(decisionValue), "Numer of objects with decision");
                        }

                        objectCount += reduct.EquivalenceClassMap.GetEquivalenceClass(dataVector).NumberOfObjects;
                    }

                    Assert.AreEqual(dataStoreTrain.NumberOfRecords, objectCount);
                }
            }
        }

        [Test]
        public void ReductPositiveTest()
        {
            Args parms = new Args(new string[] { "DataStore" }, new Object[] { dataStoreTrain });
            IPermutationGenerator permGen = ReductFactory.GetPermutationGenerator("ApproximateReductPositive", parms);
            PermutationCollection permutationList = permGen.Generate(1000);
            parms.AddParameter("PermutationCollection", permutationList);
            parms.AddParameter("FactoryKey", "ApproximateReductPositive");

            IReductGenerator reductGenerator = ReductFactory.GetReductGenerator(parms);
            IReductStore reductStore = reductGenerator.Generate(parms).First();

            foreach (Reduct reduct in reductStore)
            {
                foreach (EquivalenceClass stat in reduct.EquivalenceClassMap)
                {
                    Assert.AreEqual(1, stat.NumberOfDecisions);
                }
            }
        }

        [Test]
        public void ReductMajorityTest()
        {
            Reduct allAttributes = new Reduct(dataStoreTrain, dataStoreTrain.DataStoreInfo.GetFieldIds(FieldTypes.Standard), 0);
            double allAttrMeasue = InformationMeasureBase.Construct(InformationMeasureType.Majority).Calc(allAttributes);

            Args parms = new Args(new string[] { "DataStore" }, new Object[] { dataStoreTrain });
            IPermutationGenerator permGen = ReductFactory.GetPermutationGenerator("ApproximateReductMajority", parms);
            PermutationCollection permutationList = permGen.Generate(1000);
            parms.AddParameter("PermutationCollection", permutationList);
            parms.AddParameter("FactoryKey", "ApproximateReductMajority");

            IReductGenerator reductGenerator = ReductFactory.GetReductGenerator(parms);
            IReductStore reductStore = reductGenerator.Generate(parms).First();

            foreach (IReduct reduct in reductStore)
            {
                double localAttrMeasue = InformationMeasureBase.Construct(InformationMeasureType.Majority).Calc(reduct);
                Assert.GreaterOrEqual((double)1 / (double)dataStoreTrainInfo.NumberOfRecords, allAttrMeasue - localAttrMeasue);
                Assert.LessOrEqual((double)(-1) / (double)dataStoreTrainInfo.NumberOfRecords, allAttrMeasue - localAttrMeasue);
            }
        }

        [Test]
        public void ReductRelativeTest()
        {
            Reduct allAttributes = new Reduct(dataStoreTrain, dataStoreTrain.DataStoreInfo.GetFieldIds(FieldTypes.Standard), 0);
            double allAttrMeasue = InformationMeasureBase.Construct(InformationMeasureType.Relative).Calc(allAttributes);

            Args parms = new Args(new string[] { "DataStore" }, new Object[] { dataStoreTrain });
            IPermutationGenerator permGen = ReductFactory.GetPermutationGenerator("ApproximateReductRelative", parms);
            PermutationCollection permutationList = permGen.Generate(1000);
            parms.AddParameter("PermutationCollection", permutationList);
            parms.AddParameter("FactoryKey", "ApproximateReductRelative");

            IReductGenerator reductGenerator = ReductFactory.GetReductGenerator(parms);
            IReductStore reductStore = reductGenerator.Generate(parms).First();

            foreach (IReduct reduct in reductStore)
            {
                double localAttrMeasue = InformationMeasureBase.Construct(InformationMeasureType.Relative).Calc(reduct);
                Assert.GreaterOrEqual((double)1/(double)dataStoreTrainInfo.NumberOfRecords, allAttrMeasue - localAttrMeasue);
                Assert.LessOrEqual((double)(-1)/(double)dataStoreTrainInfo.NumberOfRecords, allAttrMeasue - localAttrMeasue);
            }
        }
    }
}
