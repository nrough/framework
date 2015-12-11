using System;
using System.Linq;
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
                EquivalenceClassCollection result = localReduct.EquivalenceClasses;
                               
                var dataVector = new long[] { dataStoreTrainInfo.GetFieldInfo(1).External2Internal(1), 
                                          dataStoreTrainInfo.GetFieldInfo(2).External2Internal(1) };                

                EquivalenceClass reductStat = result.GetEquivalenceClass(dataVector);
                Assert.AreEqual(9, reductStat.NumberOfObjects);
                Assert.AreEqual(1, reductStat.NumberOfDecisions);
                Assert.AreEqual(dataStoreTrainInfo.GetFieldInfo(dataStoreTrainInfo.DecisionFieldId).External2Internal(1), reductStat.MajorDecision);
                Assert.AreEqual(9, reductStat.GetNumberOfObjectsWithDecision(reductStat.MajorDecision));
                Assert.AreEqual(9, reductStat.GetNumberOfObjectsWithDecision(dataStoreTrainInfo.GetFieldInfo(dataStoreTrainInfo.DecisionFieldId).External2Internal(1)));
                Assert.AreEqual(0, reductStat.GetNumberOfObjectsWithDecision(dataStoreTrainInfo.GetFieldInfo(dataStoreTrainInfo.DecisionFieldId).External2Internal(0)));

                dataVector = new long[] { dataStoreTrainInfo.GetFieldInfo(1).External2Internal(1), 
                                          dataStoreTrainInfo.GetFieldInfo(2).External2Internal(2) };

                reductStat = result.GetEquivalenceClass(dataVector);
                Assert.AreEqual(17, reductStat.NumberOfObjects);
                Assert.AreEqual(2, reductStat.NumberOfDecisions);
                Assert.AreEqual(dataStoreTrainInfo.GetFieldInfo(dataStoreTrainInfo.DecisionFieldId).External2Internal(0), reductStat.MajorDecision);
                Assert.AreEqual(15, reductStat.GetNumberOfObjectsWithDecision(reductStat.MajorDecision));
                Assert.AreEqual(2, reductStat.GetNumberOfObjectsWithDecision(dataStoreTrainInfo.GetFieldInfo(dataStoreTrainInfo.DecisionFieldId).External2Internal(1)));
                Assert.AreEqual(15, reductStat.GetNumberOfObjectsWithDecision(dataStoreTrainInfo.GetFieldInfo(dataStoreTrainInfo.DecisionFieldId).External2Internal(0)));

                dataVector = new long[] { dataStoreTrainInfo.GetFieldInfo(1).External2Internal(1), 
                                          dataStoreTrainInfo.GetFieldInfo(2).External2Internal(3) };

                reductStat = result.GetEquivalenceClass(dataVector);
                Assert.AreEqual(19, reductStat.NumberOfObjects);
                Assert.AreEqual(2, reductStat.NumberOfDecisions);
                Assert.AreEqual(dataStoreTrainInfo.GetFieldInfo(dataStoreTrainInfo.DecisionFieldId).External2Internal(0), reductStat.MajorDecision);
                Assert.AreEqual(16, reductStat.GetNumberOfObjectsWithDecision(reductStat.MajorDecision));
                Assert.AreEqual(3, reductStat.GetNumberOfObjectsWithDecision(dataStoreTrainInfo.GetFieldInfo(dataStoreTrainInfo.DecisionFieldId).External2Internal(1)));
                Assert.AreEqual(16, reductStat.GetNumberOfObjectsWithDecision(dataStoreTrainInfo.GetFieldInfo(dataStoreTrainInfo.DecisionFieldId).External2Internal(0)));

                dataVector = new long[] { dataStoreTrainInfo.GetFieldInfo(1).External2Internal(2), 
                                          dataStoreTrainInfo.GetFieldInfo(2).External2Internal(1) };

                reductStat = result.GetEquivalenceClass(dataVector);
                Assert.AreEqual(16, reductStat.NumberOfObjects);
                Assert.AreEqual(2, reductStat.NumberOfDecisions);
                Assert.AreEqual(dataStoreTrainInfo.GetFieldInfo(dataStoreTrainInfo.DecisionFieldId).External2Internal(0), reductStat.MajorDecision);
                Assert.AreEqual(14, reductStat.GetNumberOfObjectsWithDecision(reductStat.MajorDecision));
                Assert.AreEqual(2, reductStat.GetNumberOfObjectsWithDecision(dataStoreTrainInfo.GetFieldInfo(dataStoreTrainInfo.DecisionFieldId).External2Internal(1)));
                Assert.AreEqual(14, reductStat.GetNumberOfObjectsWithDecision(dataStoreTrainInfo.GetFieldInfo(dataStoreTrainInfo.DecisionFieldId).External2Internal(0)));

                dataVector = new long[] { dataStoreTrainInfo.GetFieldInfo(1).External2Internal(2), 
                                          dataStoreTrainInfo.GetFieldInfo(2).External2Internal(2) };

                reductStat = result.GetEquivalenceClass(dataVector);
                Assert.AreEqual(15, reductStat.NumberOfObjects);
                Assert.AreEqual(1, reductStat.NumberOfDecisions);
                Assert.AreEqual(dataStoreTrainInfo.GetFieldInfo(dataStoreTrainInfo.DecisionFieldId).External2Internal(1), reductStat.MajorDecision);
                Assert.AreEqual(15, reductStat.GetNumberOfObjectsWithDecision(reductStat.MajorDecision));
                Assert.AreEqual(15, reductStat.GetNumberOfObjectsWithDecision(dataStoreTrainInfo.GetFieldInfo(dataStoreTrainInfo.DecisionFieldId).External2Internal(1)));
                Assert.AreEqual(0, reductStat.GetNumberOfObjectsWithDecision(dataStoreTrainInfo.GetFieldInfo(dataStoreTrainInfo.DecisionFieldId).External2Internal(0)));

                dataVector = new long[] { dataStoreTrainInfo.GetFieldInfo(1).External2Internal(2), 
                                          dataStoreTrainInfo.GetFieldInfo(2).External2Internal(3) };

                reductStat = result.GetEquivalenceClass(dataVector);
                Assert.AreEqual(11, reductStat.NumberOfObjects);
                Assert.AreEqual(2, reductStat.NumberOfDecisions);
                Assert.AreEqual(dataStoreTrainInfo.GetFieldInfo(dataStoreTrainInfo.DecisionFieldId).External2Internal(0), reductStat.MajorDecision);
                Assert.AreEqual(6, reductStat.GetNumberOfObjectsWithDecision(reductStat.MajorDecision));
                Assert.AreEqual(5, reductStat.GetNumberOfObjectsWithDecision(dataStoreTrainInfo.GetFieldInfo(dataStoreTrainInfo.DecisionFieldId).External2Internal(1)));
                Assert.AreEqual(6, reductStat.GetNumberOfObjectsWithDecision(dataStoreTrainInfo.GetFieldInfo(dataStoreTrainInfo.DecisionFieldId).External2Internal(0)));

                dataVector = new long[] { dataStoreTrainInfo.GetFieldInfo(1).External2Internal(3), 
                                          dataStoreTrainInfo.GetFieldInfo(2).External2Internal(1) };

                reductStat = result.GetEquivalenceClass(dataVector);
                Assert.AreEqual(10, reductStat.NumberOfObjects);
                Assert.AreEqual(2, reductStat.NumberOfDecisions);
                Assert.AreEqual(dataStoreTrainInfo.GetFieldInfo(dataStoreTrainInfo.DecisionFieldId).External2Internal(0), reductStat.MajorDecision);
                Assert.AreEqual(6, reductStat.GetNumberOfObjectsWithDecision(reductStat.MajorDecision));
                Assert.AreEqual(4, reductStat.GetNumberOfObjectsWithDecision(dataStoreTrainInfo.GetFieldInfo(dataStoreTrainInfo.DecisionFieldId).External2Internal(1)));
                Assert.AreEqual(6, reductStat.GetNumberOfObjectsWithDecision(dataStoreTrainInfo.GetFieldInfo(dataStoreTrainInfo.DecisionFieldId).External2Internal(0)));

                dataVector = new long[] { dataStoreTrainInfo.GetFieldInfo(1).External2Internal(3), 
                                          dataStoreTrainInfo.GetFieldInfo(2).External2Internal(2) };

                reductStat = result.GetEquivalenceClass(dataVector);
                Assert.AreEqual(10, reductStat.NumberOfObjects);
                Assert.AreEqual(2, reductStat.NumberOfDecisions);
                //Assert.AreEqual(2, reductStat.MajorDecision);
                Assert.AreEqual(5, reductStat.GetNumberOfObjectsWithDecision(reductStat.MajorDecision));
                Assert.AreEqual(5, reductStat.GetNumberOfObjectsWithDecision(dataStoreTrainInfo.GetFieldInfo(dataStoreTrainInfo.DecisionFieldId).External2Internal(1)));
                Assert.AreEqual(5, reductStat.GetNumberOfObjectsWithDecision(dataStoreTrainInfo.GetFieldInfo(dataStoreTrainInfo.DecisionFieldId).External2Internal(0)));

                dataVector = new long[] { dataStoreTrainInfo.GetFieldInfo(1).External2Internal(3), 
                                          dataStoreTrainInfo.GetFieldInfo(2).External2Internal(3) };

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
            Reduct reduct = new Reduct(dataStoreTrain);

            reductStore.AddReduct(reduct);

            foreach (Reduct localReduct in reductStore)
            {
                EquivalenceClassCollection result = localReduct.EquivalenceClasses;

                var dataVector = new long[] { };
                EquivalenceClass reductStat = result.GetEquivalenceClass(dataVector);

                Assert.NotNull(reductStat);
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
            Reduct reduct = new Reduct(dataStoreTrain, new int[] { 1, 2, 3, 4, 5, 6 }, 0);
            reductStore.AddReduct(reduct);

            foreach (Reduct localReduct in reductStore)
            {
                EquivalenceClassCollection result = localReduct.EquivalenceClasses;
                Assert.AreEqual(124, result.NumberOfPartitions);

                var dataVector = new long[] { 1, 1, 1, 1, 1, 1 };
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
            decimal result = roughMeasure.Calc(reduct);


            Assert.AreEqual(Decimal.Round(result, 17), localDataStore.DataStoreInfo.NumberOfDecisionValues);
        }

        [Test]
        public void MeasureMajorityTest()
        {
            DataStore localDataStore = DataStore.Load(@"Data\letter.trn", FileFormat.Rses1);

            Reduct reduct = new Reduct(localDataStore, localDataStore.DataStoreInfo.GetFieldIds(FieldTypes.Standard), 0);
            InformationMeasureMajority roughMeasure = new InformationMeasureMajority();
            decimal result = roughMeasure.Calc(reduct);

            Assert.AreEqual(Decimal.Round(result, 17), Decimal.One);
        }

        [Test]
        public void MeasurePositiveTest()
        {
            DataStore localDataStore = DataStore.Load(@"Data\letter.trn", FileFormat.Rses1);

            Reduct reduct = new Reduct(
                localDataStore, 
                localDataStore.DataStoreInfo.GetFieldIds(FieldTypes.Standard), 0);

            InformationMeasurePositive roughMeasure = new InformationMeasurePositive();
            decimal result = roughMeasure.Calc(reduct);

            Assert.AreEqual(Decimal.Round(result, 17), Decimal.One);
        }

        [Test]
        public void EquivalenceClassMapTest()
        {
            Args parms = new Args();
            parms.AddParameter(ReductGeneratorParamHelper.DataStore, dataStoreTrain);
            parms.AddParameter(ReductGeneratorParamHelper.FactoryKey, 
                ReductFactoryKeyHelper.ApproximateReductRelative);
            parms.AddParameter(ReductGeneratorParamHelper.PermutationCollection, 
                ReductFactory.GetPermutationGenerator(parms).Generate(1000));
            
            for (decimal epsilon = Decimal.Zero; epsilon < Decimal.One; epsilon += 0.01m)
            {
                parms.AddParameter(ReductGeneratorParamHelper.Epsilon, epsilon);

                IReductGenerator reductGenerator = ReductFactory.GetReductGenerator(parms);
                reductGenerator.Generate();
                IReductStore reductStore = reductGenerator.GetReductStoreCollection(Int32.MaxValue).FirstOrDefault();

                foreach (Reduct reduct in reductStore)
                {
                    EquivalenceClassCollection partitionMap = new EquivalenceClassCollection(dataStoreTrain);
                    partitionMap.Calc(reduct.Attributes, dataStoreTrain);

                    Assert.AreEqual(partitionMap.NumberOfPartitions, reduct.EquivalenceClasses.NumberOfPartitions);

                    int objectCount = 0;
                    foreach (var dataVector in partitionMap.Partitions.Keys)
                    {
                        Assert.AreEqual(partitionMap.GetEquivalenceClass(dataVector).DecisionValues, reduct.EquivalenceClasses.GetEquivalenceClass(dataVector).DecisionValues, "Decision Values");
                        Assert.AreEqual(partitionMap.GetEquivalenceClass(dataVector).NumberOfDecisions, reduct.EquivalenceClasses.GetEquivalenceClass(dataVector).NumberOfDecisions, "Number of Decisions");
                        Assert.AreEqual(partitionMap.GetEquivalenceClass(dataVector).NumberOfObjects, reduct.EquivalenceClasses.GetEquivalenceClass(dataVector).NumberOfObjects, "Number of objects");

                        EquivalenceClass eq1 = partitionMap.GetEquivalenceClass(dataVector);
                        EquivalenceClass eq2 = reduct.EquivalenceClasses.GetEquivalenceClass(dataVector);

                        if (eq1.MajorDecision != eq2.MajorDecision)
                        {                                                        
                            Assert.AreEqual(
                                eq1.GetDecisionWeigth(eq1.MajorDecision),
                                eq2.GetDecisionWeigth(eq2.MajorDecision), 
                                    String.Format("Major Decision Weights eps = {0}", epsilon));
                        
                        }
                        
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
        public void ReductPositiveTest()
        {
            Args parms = new Args(new string[] { ReductGeneratorParamHelper.FactoryKey, 
                                                 ReductGeneratorParamHelper.DataStore }, 
                                  new Object[] { ReductFactoryKeyHelper.ApproximateReductPositive, dataStoreTrain });

            IPermutationGenerator permGen = ReductFactory.GetPermutationGenerator(parms);
            PermutationCollection permutationList = permGen.Generate(1000);
            parms.AddParameter(ReductGeneratorParamHelper.PermutationCollection, permutationList);
            parms.AddParameter(ReductGeneratorParamHelper.FactoryKey, ReductFactoryKeyHelper.ApproximateReductPositive);

            IReductGenerator reductGenerator = ReductFactory.GetReductGenerator(parms);
            reductGenerator.Generate();
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
            Reduct allAttributes = new Reduct(dataStoreTrain, dataStoreTrain.DataStoreInfo.GetFieldIds(FieldTypes.Standard), 0);
            decimal allAttrMeasue = InformationMeasureBase.Construct(InformationMeasureType.Majority).Calc(allAttributes);

            Args parms = new Args(new string[] { ReductGeneratorParamHelper.FactoryKey, 
                                                 ReductGeneratorParamHelper.DataStore }, 
                                  new Object[] { ReductFactoryKeyHelper.ApproximateReductMajority, 
                                                 dataStoreTrain });
            
            IPermutationGenerator permGen = ReductFactory.GetPermutationGenerator(parms);
            PermutationCollection permutationList = permGen.Generate(1000);
            parms.AddParameter(ReductGeneratorParamHelper.PermutationCollection, permutationList);
            parms.AddParameter(ReductGeneratorParamHelper.FactoryKey, ReductFactoryKeyHelper.ApproximateReductMajority);

            IReductGenerator reductGenerator = ReductFactory.GetReductGenerator(parms);
            reductGenerator.Generate();
            IReductStore reductStore = reductGenerator.ReductPool;

            foreach (IReduct reduct in reductStore)
            {
                decimal localAttrMeasue = InformationMeasureBase.Construct(InformationMeasureType.Majority).Calc(reduct);
                Assert.GreaterOrEqual(1.0M / dataStoreTrainInfo.NumberOfRecords, allAttrMeasue - localAttrMeasue);
                Assert.LessOrEqual( -1.0M / dataStoreTrainInfo.NumberOfRecords, allAttrMeasue - localAttrMeasue);
            }
        }

        [Test]
        public void ReductRelativeTest()
        {
            Reduct allAttributes = new Reduct(dataStoreTrain, dataStoreTrain.DataStoreInfo.GetFieldIds(FieldTypes.Standard), 0);
            decimal allAttrMeasue = InformationMeasureBase.Construct(InformationMeasureType.Relative).Calc(allAttributes);

            Args parms = new Args(new string[] { ReductGeneratorParamHelper.FactoryKey, 
                                                 ReductGeneratorParamHelper.DataStore }, 
                                  new Object[] { ReductFactoryKeyHelper.ApproximateReductRelative, dataStoreTrain });

            IPermutationGenerator permGen = ReductFactory.GetPermutationGenerator(parms);
            PermutationCollection permutationList = permGen.Generate(1000);
            parms.AddParameter(ReductGeneratorParamHelper.PermutationCollection, permutationList);
            parms.AddParameter(ReductGeneratorParamHelper.FactoryKey, ReductFactoryKeyHelper.ApproximateReductRelative);

            IReductGenerator reductGenerator = ReductFactory.GetReductGenerator(parms);
            reductGenerator.Generate();
            IReductStore reductStore = reductGenerator.ReductPool;

            foreach (IReduct reduct in reductStore)
            {
                decimal localAttrMeasue = InformationMeasureBase.Construct(InformationMeasureType.Relative).Calc(reduct);
                Assert.GreaterOrEqual(1.0M/dataStoreTrainInfo.NumberOfRecords, allAttrMeasue - localAttrMeasue);
                Assert.LessOrEqual(-1.0M/dataStoreTrainInfo.NumberOfRecords, allAttrMeasue - localAttrMeasue);
            }
        }
    }
}
