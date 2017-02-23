using System;
using System.Linq;
using NRough.Data;
using NRough.Core;
using NUnit.Framework;
using NRough.MachineLearning.Permutations;

namespace NRough.MachineLearning.Roughset.UnitTests
{
    [TestFixture]
    public class ApproximateReductTest
    {
        [Test]
        public void ReductStatisticsTest()
        {
            string trainFileName = @"Data\monks-1.train";
            string testFileName = @"Data\monks-1.test";

            DataStore dataStoreTrain = DataStore.Load(trainFileName, DataFormat.RSES1);
            DataStore dataStoreTest = DataStore.Load(testFileName, DataFormat.RSES1, dataStoreTrain.DataStoreInfo);

            DataStoreInfo dataStoreTrainInfo = dataStoreTrain.DataStoreInfo;

            ReductStore reductStore = new ReductStore(1);
            Reduct reduct = new Reduct(dataStoreTrain, new int[] { 1, 2 }, 0);
            reductStore.AddReduct(reduct);

            foreach (Reduct localReduct in reductStore)
            {
                EquivalenceClassCollection result = localReduct.EquivalenceClasses;

                var dataVector = new long[] { dataStoreTrainInfo.GetFieldInfo(1).External2Internal(1),
                                          dataStoreTrainInfo.GetFieldInfo(2).External2Internal(1) };

                EquivalenceClass reductStat = result.Find(dataVector);
                Assert.AreEqual(9, reductStat.NumberOfObjects);
                Assert.AreEqual(1, reductStat.NumberOfDecisions);                
                Assert.AreEqual(dataStoreTrainInfo.GetFieldInfo(dataStoreTrainInfo.DecisionFieldId).External2Internal(1), reductStat.DecisionWeight.FindMaxValueKey());
                Assert.AreEqual(9, reductStat.GetNumberOfObjectsWithDecision(reductStat.DecisionWeight.FindMaxValueKey()));
                Assert.AreEqual(9, reductStat.GetNumberOfObjectsWithDecision(dataStoreTrainInfo.GetFieldInfo(dataStoreTrainInfo.DecisionFieldId).External2Internal(1)));
                Assert.AreEqual(0, reductStat.GetNumberOfObjectsWithDecision(dataStoreTrainInfo.GetFieldInfo(dataStoreTrainInfo.DecisionFieldId).External2Internal(0)));

                dataVector = new long[] { dataStoreTrainInfo.GetFieldInfo(1).External2Internal(1),
                                          dataStoreTrainInfo.GetFieldInfo(2).External2Internal(2) };

                reductStat = result.Find(dataVector);
                Assert.AreEqual(17, reductStat.NumberOfObjects);
                Assert.AreEqual(2, reductStat.NumberOfDecisions);
                Assert.AreEqual(dataStoreTrainInfo.GetFieldInfo(dataStoreTrainInfo.DecisionFieldId).External2Internal(0), reductStat.DecisionWeight.FindMaxValueKey());
                Assert.AreEqual(15, reductStat.GetNumberOfObjectsWithDecision(reductStat.DecisionWeight.FindMaxValueKey()));
                Assert.AreEqual(2, reductStat.GetNumberOfObjectsWithDecision(dataStoreTrainInfo.GetFieldInfo(dataStoreTrainInfo.DecisionFieldId).External2Internal(1)));
                Assert.AreEqual(15, reductStat.GetNumberOfObjectsWithDecision(dataStoreTrainInfo.GetFieldInfo(dataStoreTrainInfo.DecisionFieldId).External2Internal(0)));

                dataVector = new long[] { dataStoreTrainInfo.GetFieldInfo(1).External2Internal(1),
                                          dataStoreTrainInfo.GetFieldInfo(2).External2Internal(3) };

                reductStat = result.Find(dataVector);
                Assert.AreEqual(19, reductStat.NumberOfObjects);
                Assert.AreEqual(2, reductStat.NumberOfDecisions);
                Assert.AreEqual(dataStoreTrainInfo.GetFieldInfo(dataStoreTrainInfo.DecisionFieldId).External2Internal(0), reductStat.DecisionWeight.FindMaxValueKey());
                Assert.AreEqual(16, reductStat.GetNumberOfObjectsWithDecision(reductStat.DecisionWeight.FindMaxValueKey()));
                Assert.AreEqual(3, reductStat.GetNumberOfObjectsWithDecision(dataStoreTrainInfo.GetFieldInfo(dataStoreTrainInfo.DecisionFieldId).External2Internal(1)));
                Assert.AreEqual(16, reductStat.GetNumberOfObjectsWithDecision(dataStoreTrainInfo.GetFieldInfo(dataStoreTrainInfo.DecisionFieldId).External2Internal(0)));

                dataVector = new long[] { dataStoreTrainInfo.GetFieldInfo(1).External2Internal(2),
                                          dataStoreTrainInfo.GetFieldInfo(2).External2Internal(1) };

                reductStat = result.Find(dataVector);
                Assert.AreEqual(16, reductStat.NumberOfObjects);
                Assert.AreEqual(2, reductStat.NumberOfDecisions);
                Assert.AreEqual(dataStoreTrainInfo.GetFieldInfo(dataStoreTrainInfo.DecisionFieldId).External2Internal(0), reductStat.DecisionWeight.FindMaxValueKey());
                Assert.AreEqual(14, reductStat.GetNumberOfObjectsWithDecision(reductStat.DecisionWeight.FindMaxValueKey()));
                Assert.AreEqual(2, reductStat.GetNumberOfObjectsWithDecision(dataStoreTrainInfo.GetFieldInfo(dataStoreTrainInfo.DecisionFieldId).External2Internal(1)));
                Assert.AreEqual(14, reductStat.GetNumberOfObjectsWithDecision(dataStoreTrainInfo.GetFieldInfo(dataStoreTrainInfo.DecisionFieldId).External2Internal(0)));

                dataVector = new long[] { dataStoreTrainInfo.GetFieldInfo(1).External2Internal(2),
                                          dataStoreTrainInfo.GetFieldInfo(2).External2Internal(2) };

                reductStat = result.Find(dataVector);
                Assert.AreEqual(15, reductStat.NumberOfObjects);
                Assert.AreEqual(1, reductStat.NumberOfDecisions);
                Assert.AreEqual(dataStoreTrainInfo.GetFieldInfo(dataStoreTrainInfo.DecisionFieldId).External2Internal(1), reductStat.DecisionWeight.FindMaxValueKey());
                Assert.AreEqual(15, reductStat.GetNumberOfObjectsWithDecision(reductStat.DecisionWeight.FindMaxValueKey()));
                Assert.AreEqual(15, reductStat.GetNumberOfObjectsWithDecision(dataStoreTrainInfo.GetFieldInfo(dataStoreTrainInfo.DecisionFieldId).External2Internal(1)));
                Assert.AreEqual(0, reductStat.GetNumberOfObjectsWithDecision(dataStoreTrainInfo.GetFieldInfo(dataStoreTrainInfo.DecisionFieldId).External2Internal(0)));

                dataVector = new long[] { dataStoreTrainInfo.GetFieldInfo(1).External2Internal(2),
                                          dataStoreTrainInfo.GetFieldInfo(2).External2Internal(3) };

                reductStat = result.Find(dataVector);
                Assert.AreEqual(11, reductStat.NumberOfObjects);
                Assert.AreEqual(2, reductStat.NumberOfDecisions);
                Assert.AreEqual(dataStoreTrainInfo.GetFieldInfo(dataStoreTrainInfo.DecisionFieldId).External2Internal(0), reductStat.DecisionWeight.FindMaxValueKey());
                Assert.AreEqual(6, reductStat.GetNumberOfObjectsWithDecision(reductStat.DecisionWeight.FindMaxValueKey()));
                Assert.AreEqual(5, reductStat.GetNumberOfObjectsWithDecision(dataStoreTrainInfo.GetFieldInfo(dataStoreTrainInfo.DecisionFieldId).External2Internal(1)));
                Assert.AreEqual(6, reductStat.GetNumberOfObjectsWithDecision(dataStoreTrainInfo.GetFieldInfo(dataStoreTrainInfo.DecisionFieldId).External2Internal(0)));

                dataVector = new long[] { dataStoreTrainInfo.GetFieldInfo(1).External2Internal(3),
                                          dataStoreTrainInfo.GetFieldInfo(2).External2Internal(1) };

                reductStat = result.Find(dataVector);
                Assert.AreEqual(10, reductStat.NumberOfObjects);
                Assert.AreEqual(2, reductStat.NumberOfDecisions);
                Assert.AreEqual(dataStoreTrainInfo.GetFieldInfo(dataStoreTrainInfo.DecisionFieldId).External2Internal(0), reductStat.DecisionWeight.FindMaxValueKey());
                Assert.AreEqual(6, reductStat.GetNumberOfObjectsWithDecision(reductStat.DecisionWeight.FindMaxValueKey()));
                Assert.AreEqual(4, reductStat.GetNumberOfObjectsWithDecision(dataStoreTrainInfo.GetFieldInfo(dataStoreTrainInfo.DecisionFieldId).External2Internal(1)));
                Assert.AreEqual(6, reductStat.GetNumberOfObjectsWithDecision(dataStoreTrainInfo.GetFieldInfo(dataStoreTrainInfo.DecisionFieldId).External2Internal(0)));

                dataVector = new long[] { dataStoreTrainInfo.GetFieldInfo(1).External2Internal(3),
                                          dataStoreTrainInfo.GetFieldInfo(2).External2Internal(2) };

                reductStat = result.Find(dataVector);
                Assert.AreEqual(10, reductStat.NumberOfObjects);
                Assert.AreEqual(2, reductStat.NumberOfDecisions);
                //Assert.AreEqual(2, reductStat.DecisionWeights.FindMaxValueKey());
                Assert.AreEqual(5, reductStat.GetNumberOfObjectsWithDecision(reductStat.DecisionWeight.FindMaxValueKey()));
                Assert.AreEqual(5, reductStat.GetNumberOfObjectsWithDecision(dataStoreTrainInfo.GetFieldInfo(dataStoreTrainInfo.DecisionFieldId).External2Internal(1)));
                Assert.AreEqual(5, reductStat.GetNumberOfObjectsWithDecision(dataStoreTrainInfo.GetFieldInfo(dataStoreTrainInfo.DecisionFieldId).External2Internal(0)));

                dataVector = new long[] { dataStoreTrainInfo.GetFieldInfo(1).External2Internal(3),
                                          dataStoreTrainInfo.GetFieldInfo(2).External2Internal(3) };

                reductStat = result.Find(dataVector);
                Assert.AreEqual(17, reductStat.NumberOfObjects);
                Assert.AreEqual(1, reductStat.NumberOfDecisions);
                Assert.AreEqual(dataStoreTrainInfo.GetFieldInfo(dataStoreTrainInfo.DecisionFieldId).External2Internal(1), reductStat.DecisionWeight.FindMaxValueKey());
                Assert.AreEqual(17, reductStat.GetNumberOfObjectsWithDecision(reductStat.DecisionWeight.FindMaxValueKey()));
                Assert.AreEqual(17, reductStat.GetNumberOfObjectsWithDecision(dataStoreTrainInfo.GetFieldInfo(dataStoreTrainInfo.DecisionFieldId).External2Internal(1)));
                Assert.AreEqual(0, reductStat.GetNumberOfObjectsWithDecision(dataStoreTrainInfo.GetFieldInfo(dataStoreTrainInfo.DecisionFieldId).External2Internal(0)));
            }
        }

        [Test]
        public void EmptyReductStatisticsTest()
        {
            string trainFileName = @"Data\monks-1.train";
            string testFileName = @"Data\monks-1.test";

            DataStore dataStoreTrain = DataStore.Load(trainFileName, DataFormat.RSES1);
            DataStore dataStoreTest = DataStore.Load(testFileName, DataFormat.RSES1, dataStoreTrain.DataStoreInfo);

            DataStoreInfo dataStoreTrainInfo = dataStoreTrain.DataStoreInfo;

            Reduct reduct = new Reduct(dataStoreTrain);
            EquivalenceClassCollection result = reduct.EquivalenceClasses;
            var dataVector = new long[] { };
            EquivalenceClass reductStat = result.Find(dataVector);
            Assert.NotNull(reductStat);
            Assert.AreEqual(124, reductStat.NumberOfObjects);
            Assert.AreEqual(2, reductStat.NumberOfDecisions);
            Assert.AreEqual(62, reductStat.GetNumberOfObjectsWithDecision(dataStoreTrainInfo.GetFieldInfo(dataStoreTrainInfo.DecisionFieldId).External2Internal(1)));
            Assert.AreEqual(62, reductStat.GetNumberOfObjectsWithDecision(dataStoreTrainInfo.GetFieldInfo(dataStoreTrainInfo.DecisionFieldId).External2Internal(0)));
        }

        [Test]
        public void FullAttributeSetReductStatisticsTest()
        {
            string trainFileName = @"Data\monks-1.train";
            string testFileName = @"Data\monks-1.test";

            DataStore dataStoreTrain = DataStore.Load(trainFileName, DataFormat.RSES1);
            DataStore dataStoreTest = DataStore.Load(testFileName, DataFormat.RSES1, dataStoreTrain.DataStoreInfo);

            DataStoreInfo dataStoreTrainInfo = dataStoreTrain.DataStoreInfo;
            ReductStore reductStore = new ReductStore(1);
            Reduct reduct = new Reduct(dataStoreTrain, new int[] { 1, 2, 3, 4, 5, 6 }, 0);
            reductStore.AddReduct(reduct);

            foreach (Reduct localReduct in reductStore)
            {
                EquivalenceClassCollection result = localReduct.EquivalenceClasses;
                Assert.AreEqual(124, result.Count);

                //var dataVector = new long[] { 1, 1, 1, 1, 1, 1 };
                var dataVector = new long[] { 1, 1, 1, 1, 3, 1 };
                EquivalenceClass reductStat = result.Find(dataVector);

                Assert.AreEqual(1, reductStat.NumberOfObjects);
            }
        }

        [Test]
        public void ReductFiltering()
        {
            string trainFileName = @"Data\monks-1.train";
            string testFileName = @"Data\monks-1.test";

            DataStore dataStoreTrain = DataStore.Load(trainFileName, DataFormat.RSES1);
            DataStore dataStoreTest = DataStore.Load(testFileName, DataFormat.RSES1, dataStoreTrain.DataStoreInfo);

            DataStoreInfo dataStoreTrainInfo = dataStoreTrain.DataStoreInfo;

            ReductStore reductStore = new ReductStore(3);

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
            DataStore localDataStore = DataStore.Load(@"Data\letter.trn", DataFormat.RSES1);

            Reduct reduct = new Reduct(localDataStore, localDataStore.DataStoreInfo.GetFieldIds(FieldGroup.Standard), 0);
            double result = InformationMeasureRelative.Instance.Calc(reduct);

            Assert.AreEqual(result, 1.0, 0.0000001);
        }

        [Test]
        public void MeasureMajorityTest()
        {
            DataStore localDataStore = DataStore.Load(@"Data\letter.trn", DataFormat.RSES1);

            Reduct reduct = new Reduct(localDataStore, localDataStore.DataStoreInfo.GetFieldIds(FieldGroup.Standard), 0);
            double result = InformationMeasureMajority.Instance.Calc(reduct);

            Assert.AreEqual(result, 1.0, 0.0000001);
        }

        [Test]
        public void MeasurePositiveTest()
        {
            DataStore localDataStore = DataStore.Load(@"Data\letter.trn", DataFormat.RSES1);

            Reduct reduct = new Reduct(
                localDataStore,
                localDataStore.DataStoreInfo.GetFieldIds(FieldGroup.Standard), 0);

            InformationMeasurePositive roughMeasure = new InformationMeasurePositive();
            double result = roughMeasure.Calc(reduct);

            Assert.AreEqual(result, 1.0, 0.0000001);
        }

        [Test]
        public void EquivalenceClassMapTest()
        {
            string trainFileName = @"Data\monks-1.train";
            string testFileName = @"Data\monks-1.test";

            DataStore dataStoreTrain = DataStore.Load(trainFileName, DataFormat.RSES1);
            DataStore dataStoreTest = DataStore.Load(testFileName, DataFormat.RSES1, dataStoreTrain.DataStoreInfo);

            DataStoreInfo dataStoreTrainInfo = dataStoreTrain.DataStoreInfo;

            Args parms = new Args();
            parms.SetParameter(ReductFactoryOptions.DecisionTable, dataStoreTrain);
            parms.SetParameter(ReductFactoryOptions.ReductType,
                ReductTypes.ApproximateReductRelative);
            parms.SetParameter(ReductFactoryOptions.PermutationCollection,
                ReductFactory.GetPermutationGenerator(parms).Generate(1000));

            for (double epsilon = 0.0; epsilon < 1.0; epsilon += 0.01)
            {
                parms.SetParameter(ReductFactoryOptions.Epsilon, epsilon);

                IReductGenerator reductGenerator = ReductFactory.GetReductGenerator(parms);
                reductGenerator.Run();
                IReductStore reductStore = reductGenerator.GetReductStoreCollection().FirstOrDefault();

                foreach (Reduct reduct in reductStore)
                {
                    EquivalenceClassCollection partitionMap = new EquivalenceClassCollection(dataStoreTrain);
                    partitionMap.Calc(reduct.Attributes, dataStoreTrain);

                    Assert.AreEqual(partitionMap.Count, reduct.EquivalenceClasses.Count);

                    int objectCount = 0;
                    foreach (var eqClass in partitionMap)
                    {
                        Assert.AreEqual(eqClass.DecisionValues, reduct.EquivalenceClasses.Find(eqClass.Instance).DecisionValues, "Decision Values");
                        Assert.AreEqual(eqClass.NumberOfDecisions, reduct.EquivalenceClasses.Find(eqClass.Instance).NumberOfDecisions, "Number of Decisions");
                        Assert.AreEqual(eqClass.NumberOfObjects, reduct.EquivalenceClasses.Find(eqClass.Instance).NumberOfObjects, "Number of objects");
                        
                        EquivalenceClass eq2 = reduct.EquivalenceClasses.Find(eqClass.Instance);
                                                
                        if (eqClass.DecisionWeight.FindMaxValueKey() != eq2.DecisionWeight.FindMaxValueKey())
                        {
                            Assert.AreEqual(
                                eqClass.GetDecisionWeight(eqClass.DecisionWeight.FindMaxValueKey()),
                                eq2.GetDecisionWeight(eq2.DecisionWeight.FindMaxValueKey()),
                                    String.Format("Major Decision Weights eps = {0}", epsilon));
                        }                        
                        
                        EquivalenceClass reductEqClass = reduct.EquivalenceClasses.Find(eqClass.Instance);

                        
                        Assert.AreEqual(eqClass.GetNumberOfObjectsWithDecision(eqClass.DecisionWeight.FindMaxValueKey()),
                                        reductEqClass.GetNumberOfObjectsWithDecision(reductEqClass.DecisionWeight.FindMaxValueKey()),
                                        "Number of objects with major decisionInternalValue");

                        foreach (long decisionValue in dataStoreTrain.DataStoreInfo.DecisionInfo.InternalValues())
                        {
                            Assert.AreEqual(eqClass.GetNumberOfObjectsWithDecision(decisionValue),
                                            reduct.EquivalenceClasses.Find(eqClass.Instance).GetNumberOfObjectsWithDecision(decisionValue), "Numer of objects with decisionInternalValue");
                        }

                        objectCount += reduct.EquivalenceClasses.Find(eqClass.Instance).NumberOfObjects;
                    }

                    Assert.AreEqual(dataStoreTrain.NumberOfRecords, objectCount);
                }
            }
        }

        [Test]
        public void ReductPositiveTest()
        {
            string trainFileName = @"Data\monks-1.train";
            string testFileName = @"Data\monks-1.test";

            DataStore dataStoreTrain = DataStore.Load(trainFileName, DataFormat.RSES1);
            DataStore dataStoreTest = DataStore.Load(testFileName, DataFormat.RSES1, dataStoreTrain.DataStoreInfo);

            DataStoreInfo dataStoreTrainInfo = dataStoreTrain.DataStoreInfo;

            Args parms = new Args(new string[] { ReductFactoryOptions.ReductType,
                                                 ReductFactoryOptions.DecisionTable },
                                  new Object[] { ReductTypes.ApproximateReductPositive, dataStoreTrain });

            IPermutationGenerator permGen = ReductFactory.GetPermutationGenerator(parms);
            PermutationCollection permutationList = permGen.Generate(1000);
            parms.SetParameter(ReductFactoryOptions.PermutationCollection, permutationList);
            parms.SetParameter(ReductFactoryOptions.ReductType, ReductTypes.ApproximateReductPositive);

            IReductGenerator reductGenerator = ReductFactory.GetReductGenerator(parms);
            reductGenerator.Run();
            IReductStore reductStore = reductGenerator.ReductPool;

            foreach (Reduct reduct in reductStore)
            {
                foreach (EquivalenceClass stat in reduct.EquivalenceClasses)
                {
                    Assert.AreEqual(1, stat.NumberOfDecisions);
                }
            }
        }

        [Test]
        public void ReductMajorityTest()
        {
            string trainFileName = @"Data\monks-1.train";
            string testFileName = @"Data\monks-1.test";

            DataStore dataStoreTrain = DataStore.Load(trainFileName, DataFormat.RSES1);
            DataStore dataStoreTest = DataStore.Load(testFileName, DataFormat.RSES1, dataStoreTrain.DataStoreInfo);

            DataStoreInfo dataStoreTrainInfo = dataStoreTrain.DataStoreInfo;

            Reduct allAttributes = new Reduct(dataStoreTrain, dataStoreTrain.DataStoreInfo.GetFieldIds(FieldGroup.Standard), 0);
            double allAttrMeasure = InformationMeasureBase.Construct(InformationMeasureType.Majority).Calc(allAttributes);

            Args parms = new Args(new string[] { ReductFactoryOptions.ReductType,
                                                 ReductFactoryOptions.DecisionTable },
                                  new Object[] { ReductTypes.ApproximateReductMajority,
                                                 dataStoreTrain });

            IPermutationGenerator permGen = ReductFactory.GetPermutationGenerator(parms);
            PermutationCollection permutationList = permGen.Generate(1000);
            parms.SetParameter(ReductFactoryOptions.PermutationCollection, permutationList);
            parms.SetParameter(ReductFactoryOptions.ReductType, ReductTypes.ApproximateReductMajority);

            IReductGenerator reductGenerator = ReductFactory.GetReductGenerator(parms);
            reductGenerator.Run();
            IReductStore reductStore = reductGenerator.ReductPool;

            foreach (IReduct reduct in reductStore)
            {
                double localAttrMeasure = InformationMeasureBase.Construct(InformationMeasureType.Majority).Calc(reduct);

                Assert.That(1.0 / dataStoreTrainInfo.NumberOfRecords, Is.GreaterThanOrEqualTo(allAttrMeasure - localAttrMeasure).Using(ToleranceDoubleComparer.Instance));
                Assert.That(-1.0 / dataStoreTrainInfo.NumberOfRecords, Is.LessThanOrEqualTo(allAttrMeasure - localAttrMeasure).Using(ToleranceDoubleComparer.Instance));                
            }
        }

        [Test]
        public void ReductRelativeTest()
        {
            string trainFileName = @"Data\monks-1.train";
            string testFileName = @"Data\monks-1.test";

            DataStore dataStoreTrain = DataStore.Load(trainFileName, DataFormat.RSES1);
            DataStore dataStoreTest = DataStore.Load(testFileName, DataFormat.RSES1, dataStoreTrain.DataStoreInfo);

            DataStoreInfo dataStoreTrainInfo = dataStoreTrain.DataStoreInfo;

            Reduct allAttributes = new Reduct(dataStoreTrain, dataStoreTrain.DataStoreInfo.GetFieldIds(FieldGroup.Standard), 0);
            double allAttrMeasure = InformationMeasureBase.Construct(InformationMeasureType.Relative).Calc(allAttributes);

            Args parms = new Args(new string[] { ReductFactoryOptions.ReductType,
                                                 ReductFactoryOptions.DecisionTable },
                                  new Object[] { ReductTypes.ApproximateReductRelative, dataStoreTrain });

            IPermutationGenerator permGen = ReductFactory.GetPermutationGenerator(parms);
            PermutationCollection permutationList = permGen.Generate(1000);
            parms.SetParameter(ReductFactoryOptions.PermutationCollection, permutationList);
            parms.SetParameter(ReductFactoryOptions.ReductType, ReductTypes.ApproximateReductRelative);

            IReductGenerator reductGenerator = ReductFactory.GetReductGenerator(parms);
            reductGenerator.Run();
            IReductStore reductStore = reductGenerator.ReductPool;

            foreach (IReduct reduct in reductStore)
            {
                double localAttrMeasure = InformationMeasureBase.Construct(InformationMeasureType.Relative).Calc(reduct);

                Assert.That(1.0 / dataStoreTrainInfo.NumberOfRecords, Is.GreaterThanOrEqualTo(allAttrMeasure - localAttrMeasure).Using(ToleranceDoubleComparer.Instance));
                Assert.That(-1.0 / dataStoreTrainInfo.NumberOfRecords, Is.LessThanOrEqualTo(allAttrMeasure - localAttrMeasure).Using(ToleranceDoubleComparer.Instance));                
            }
        }
    }
}