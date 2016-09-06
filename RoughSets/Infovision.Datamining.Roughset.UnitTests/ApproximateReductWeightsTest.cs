using System;
using System.Collections.Generic;
using System.Linq;
using Infovision.Data;
using Infovision.Datamining.Benchmark;
using Infovision.Utils;
using NUnit.Framework;

namespace Infovision.Datamining.Roughset.UnitTests
{
    [TestFixture]
    public class ApproximateReductWeightsTest
    {
        private DataStore dataStoreTrain = null;
        private DataStore dataStoreTest = null;
        private DataStoreInfo dataStoreTrainInfo = null;

        public ApproximateReductWeightsTest()
        {
            string trainFileName = @"Data\monks-1.train";
            string testFileName = @"Data\monks-1.test";

            dataStoreTrain = DataStore.Load(trainFileName, FileFormat.Rses1);
            dataStoreTest = DataStore.Load(testFileName, FileFormat.Rses1, dataStoreTrain.DataStoreInfo);

            dataStoreTrainInfo = dataStoreTrain.DataStoreInfo;
        }

        public IEnumerable<KeyValuePair<string, BenchmarkData>> GetDataFiles()
        {
            return BenchmarkDataHelper.GetDataFiles("Data");
        }

        [Test, TestCaseSource("GetDataFiles")]
        public void GenerateZeroEpsilonTest(KeyValuePair<string, BenchmarkData> fileName)
        {
            DataStore data = DataStore.Load(fileName.Value.TrainFile, fileName.Value.FileFormat);

            Args parms = new Args();
            parms.SetParameter(ReductGeneratorParamHelper.TrainData, data);
            parms.SetParameter(ReductGeneratorParamHelper.FactoryKey, ReductFactoryKeyHelper.ApproximateReductMajorityWeights);
            parms.SetParameter(ReductGeneratorParamHelper.NumberOfPermutations, 1);

            ReductGeneratorWeightsMajority reductGenerator = ReductFactory.GetReductGenerator(parms) as ReductGeneratorWeightsMajority;
            reductGenerator.Run();

            Assert.NotNull(reductGenerator.GetReductStoreCollection());
            Assert.NotNull(reductGenerator.ReductPool);
            Assert.AreEqual(1, reductGenerator.ReductPool.Count);
        }

        [Test]
        public void ReductStatisticsTest()
        {
            ReductStore reductStore = new ReductStore(1);
            ReductWeights reduct = new ReductWeights(dataStoreTrain, new int[] { 1, 2 }, 0, new WeightGeneratorMajority(dataStoreTrain).Weights);
            reductStore.AddReduct(reduct);

            foreach (ReductWeights localReduct in reductStore)
            {
                EquivalenceClassCollection result = localReduct.EquivalenceClasses;

                var dataVector = new long[] { dataStoreTrainInfo.GetFieldInfo(1).External2Internal(1),
                                              dataStoreTrainInfo.GetFieldInfo(2).External2Internal(1) };

                EquivalenceClass reductStat = result.GetEquivalenceClass(dataVector);
                Assert.AreEqual(9, reductStat.NumberOfObjects);
                Assert.AreEqual(1, reductStat.NumberOfDecisions);
                Assert.AreEqual(dataStoreTrainInfo.GetFieldInfo(dataStoreTrainInfo.DecisionFieldId).External2Internal(1), reductStat.DecisionWeights.FindMaxValueKey());
                Assert.AreEqual(9, reductStat.GetNumberOfObjectsWithDecision(reductStat.DecisionWeights.FindMaxValueKey()));
                Assert.AreEqual(9, reductStat.GetNumberOfObjectsWithDecision(dataStoreTrainInfo.GetFieldInfo(dataStoreTrainInfo.DecisionFieldId).External2Internal(1)));
                Assert.AreEqual(0, reductStat.GetNumberOfObjectsWithDecision(dataStoreTrainInfo.GetFieldInfo(dataStoreTrainInfo.DecisionFieldId).External2Internal(0)));

                dataVector = new long[] { dataStoreTrainInfo.GetFieldInfo(1).External2Internal(1),
                                          dataStoreTrainInfo.GetFieldInfo(2).External2Internal(2) };

                reductStat = result.GetEquivalenceClass(dataVector);
                Assert.AreEqual(17, reductStat.NumberOfObjects);
                Assert.AreEqual(2, reductStat.NumberOfDecisions);
                Assert.AreEqual(dataStoreTrainInfo.GetFieldInfo(dataStoreTrainInfo.DecisionFieldId).External2Internal(0), reductStat.DecisionWeights.FindMaxValueKey());
                Assert.AreEqual(15, reductStat.GetNumberOfObjectsWithDecision(reductStat.DecisionWeights.FindMaxValueKey()));
                Assert.AreEqual(2, reductStat.GetNumberOfObjectsWithDecision(dataStoreTrainInfo.GetFieldInfo(dataStoreTrainInfo.DecisionFieldId).External2Internal(1)));
                Assert.AreEqual(15, reductStat.GetNumberOfObjectsWithDecision(dataStoreTrainInfo.GetFieldInfo(dataStoreTrainInfo.DecisionFieldId).External2Internal(0)));

                dataVector = new long[] { dataStoreTrainInfo.GetFieldInfo(1).External2Internal(1),
                                          dataStoreTrainInfo.GetFieldInfo(2).External2Internal(3) };

                reductStat = result.GetEquivalenceClass(dataVector);
                Assert.AreEqual(19, reductStat.NumberOfObjects);
                Assert.AreEqual(2, reductStat.NumberOfDecisions);
                Assert.AreEqual(dataStoreTrainInfo.GetFieldInfo(dataStoreTrainInfo.DecisionFieldId).External2Internal(0), reductStat.DecisionWeights.FindMaxValueKey());
                Assert.AreEqual(16, reductStat.GetNumberOfObjectsWithDecision(reductStat.DecisionWeights.FindMaxValueKey()));
                Assert.AreEqual(3, reductStat.GetNumberOfObjectsWithDecision(dataStoreTrainInfo.GetFieldInfo(dataStoreTrainInfo.DecisionFieldId).External2Internal(1)));
                Assert.AreEqual(16, reductStat.GetNumberOfObjectsWithDecision(dataStoreTrainInfo.GetFieldInfo(dataStoreTrainInfo.DecisionFieldId).External2Internal(0)));

                dataVector = new long[] { dataStoreTrainInfo.GetFieldInfo(1).External2Internal(2),
                                          dataStoreTrainInfo.GetFieldInfo(2).External2Internal(1) };

                reductStat = result.GetEquivalenceClass(dataVector);
                Assert.AreEqual(16, reductStat.NumberOfObjects);
                Assert.AreEqual(2, reductStat.NumberOfDecisions);
                Assert.AreEqual(dataStoreTrainInfo.GetFieldInfo(dataStoreTrainInfo.DecisionFieldId).External2Internal(0), reductStat.DecisionWeights.FindMaxValueKey());
                Assert.AreEqual(14, reductStat.GetNumberOfObjectsWithDecision(reductStat.DecisionWeights.FindMaxValueKey()));
                Assert.AreEqual(2, reductStat.GetNumberOfObjectsWithDecision(dataStoreTrainInfo.GetFieldInfo(dataStoreTrainInfo.DecisionFieldId).External2Internal(1)));
                Assert.AreEqual(14, reductStat.GetNumberOfObjectsWithDecision(dataStoreTrainInfo.GetFieldInfo(dataStoreTrainInfo.DecisionFieldId).External2Internal(0)));

                dataVector = new long[] { dataStoreTrainInfo.GetFieldInfo(1).External2Internal(2),
                                          dataStoreTrainInfo.GetFieldInfo(2).External2Internal(2) };

                reductStat = result.GetEquivalenceClass(dataVector);
                Assert.AreEqual(15, reductStat.NumberOfObjects);
                Assert.AreEqual(1, reductStat.NumberOfDecisions);
                Assert.AreEqual(dataStoreTrainInfo.GetFieldInfo(dataStoreTrainInfo.DecisionFieldId).External2Internal(1), reductStat.DecisionWeights.FindMaxValueKey());
                Assert.AreEqual(15, reductStat.GetNumberOfObjectsWithDecision(reductStat.DecisionWeights.FindMaxValueKey()));
                Assert.AreEqual(15, reductStat.GetNumberOfObjectsWithDecision(dataStoreTrainInfo.GetFieldInfo(dataStoreTrainInfo.DecisionFieldId).External2Internal(1)));
                Assert.AreEqual(0, reductStat.GetNumberOfObjectsWithDecision(dataStoreTrainInfo.GetFieldInfo(dataStoreTrainInfo.DecisionFieldId).External2Internal(0)));

                dataVector = new long[] { dataStoreTrainInfo.GetFieldInfo(1).External2Internal(2),
                                          dataStoreTrainInfo.GetFieldInfo(2).External2Internal(3) };

                reductStat = result.GetEquivalenceClass(dataVector);
                Assert.AreEqual(11, reductStat.NumberOfObjects);
                Assert.AreEqual(2, reductStat.NumberOfDecisions);
                Assert.AreEqual(dataStoreTrainInfo.GetFieldInfo(dataStoreTrainInfo.DecisionFieldId).External2Internal(0), reductStat.DecisionWeights.FindMaxValueKey());
                Assert.AreEqual(6, reductStat.GetNumberOfObjectsWithDecision(reductStat.DecisionWeights.FindMaxValueKey()));
                Assert.AreEqual(5, reductStat.GetNumberOfObjectsWithDecision(dataStoreTrainInfo.GetFieldInfo(dataStoreTrainInfo.DecisionFieldId).External2Internal(1)));
                Assert.AreEqual(6, reductStat.GetNumberOfObjectsWithDecision(dataStoreTrainInfo.GetFieldInfo(dataStoreTrainInfo.DecisionFieldId).External2Internal(0)));

                dataVector = new long[] { dataStoreTrainInfo.GetFieldInfo(1).External2Internal(3),
                                          dataStoreTrainInfo.GetFieldInfo(2).External2Internal(1) };

                reductStat = result.GetEquivalenceClass(dataVector);
                Assert.AreEqual(10, reductStat.NumberOfObjects);
                Assert.AreEqual(2, reductStat.NumberOfDecisions);
                Assert.AreEqual(dataStoreTrainInfo.GetFieldInfo(dataStoreTrainInfo.DecisionFieldId).External2Internal(0), reductStat.DecisionWeights.FindMaxValueKey());
                Assert.AreEqual(6, reductStat.GetNumberOfObjectsWithDecision(reductStat.DecisionWeights.FindMaxValueKey()));
                Assert.AreEqual(4, reductStat.GetNumberOfObjectsWithDecision(dataStoreTrainInfo.GetFieldInfo(dataStoreTrainInfo.DecisionFieldId).External2Internal(1)));
                Assert.AreEqual(6, reductStat.GetNumberOfObjectsWithDecision(dataStoreTrainInfo.GetFieldInfo(dataStoreTrainInfo.DecisionFieldId).External2Internal(0)));

                dataVector = new long[] { dataStoreTrainInfo.GetFieldInfo(1).External2Internal(3),
                                          dataStoreTrainInfo.GetFieldInfo(2).External2Internal(2) };

                reductStat = result.GetEquivalenceClass(dataVector);
                Assert.AreEqual(10, reductStat.NumberOfObjects);
                Assert.AreEqual(2, reductStat.NumberOfDecisions);
                Assert.AreEqual(2, reductStat.DecisionWeights.FindMaxValueKey());
                Assert.AreEqual(5, reductStat.GetNumberOfObjectsWithDecision(reductStat.DecisionWeights.FindMaxValueKey()));
                Assert.AreEqual(5, reductStat.GetNumberOfObjectsWithDecision(dataStoreTrainInfo.GetFieldInfo(dataStoreTrainInfo.DecisionFieldId).External2Internal(1)));
                Assert.AreEqual(5, reductStat.GetNumberOfObjectsWithDecision(dataStoreTrainInfo.GetFieldInfo(dataStoreTrainInfo.DecisionFieldId).External2Internal(0)));

                dataVector = new long[] { dataStoreTrainInfo.GetFieldInfo(1).External2Internal(3),
                                          dataStoreTrainInfo.GetFieldInfo(2).External2Internal(3) };

                reductStat = result.GetEquivalenceClass(dataVector);
                Assert.AreEqual(17, reductStat.NumberOfObjects);
                Assert.AreEqual(1, reductStat.NumberOfDecisions);
                Assert.AreEqual(dataStoreTrainInfo.GetFieldInfo(dataStoreTrainInfo.DecisionFieldId).External2Internal(1), reductStat.DecisionWeights.FindMaxValueKey());
                Assert.AreEqual(17, reductStat.GetNumberOfObjectsWithDecision(reductStat.DecisionWeights.FindMaxValueKey()));
                Assert.AreEqual(17, reductStat.GetNumberOfObjectsWithDecision(dataStoreTrainInfo.GetFieldInfo(dataStoreTrainInfo.DecisionFieldId).External2Internal(1)));
                Assert.AreEqual(0, reductStat.GetNumberOfObjectsWithDecision(dataStoreTrainInfo.GetFieldInfo(dataStoreTrainInfo.DecisionFieldId).External2Internal(0)));
            }
        }

        [Test]
        public void EmptyReductStatisticsTest()
        {
            ReductStore reductStore = new ReductStore(1);
            ReductWeights reduct = new ReductWeights(dataStoreTrain, new int[] { }, 0, new WeightGeneratorMajority(dataStoreTrain).Weights);

            reductStore.AddReduct(reduct);

            foreach (ReductWeights localReduct in reductStore)
            {
                EquivalenceClassCollection result = localReduct.EquivalenceClasses;

                var dataVector = new long[] { };
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
            ReductStore reductStore = new ReductStore(1);
            ReductWeights reduct = new ReductWeights(dataStoreTrain, new int[] { 1, 2, 3, 4, 5, 6 }, 0, new WeightGeneratorMajority(dataStoreTrain).Weights);
            reductStore.AddReduct(reduct);

            foreach (ReductWeights localReduct in reductStore)
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
            ReductStore reductStore = new ReductStore(3);

            double[] weights = new WeightGeneratorMajority(dataStoreTrain).Weights;

            reductStore.AddReduct(new ReductWeights(dataStoreTrain, new int[] { 1, 2 }, 0, weights));
            reductStore.AddReduct(new ReductWeights(dataStoreTrain, new int[] { 1, 5, 6 }, 0, weights));
            reductStore.AddReduct(new ReductWeights(dataStoreTrain, new int[] { 1, 2, 3 }, 0, weights));

            //IReductStore filteredReductStore = reductStore.FilterReducts(1, new ReductMeasureNumberOfPartitions());
            IReductStore filteredReductStore = reductStore.FilterReducts(1, new ReductRuleNumberComparer());

            Assert.AreEqual(1, filteredReductStore.Count);
        }

        [Test]
        public void MeasureRelativeTest()
        {
            DataStore localDataStore = DataStore.Load(@"Data\letter.trn", FileFormat.Rses1);
            double[] weights = new WeightGeneratorRelative(localDataStore).Weights;
            ReductWeights reduct = new ReductWeights(localDataStore, localDataStore.DataStoreInfo.GetFieldIds(FieldTypes.Standard), 0, weights);

            InformationMeasureWeights roughMeasure = new InformationMeasureWeights();
            double result = roughMeasure.Calc(reduct);

            Assert.AreEqual(result, 1.0, 0.0000001);
        }

        [Test]
        public void MeasureMajorityTest()
        {
            DataStore localDataStore = DataStore.Load(@"Data\letter.trn", FileFormat.Rses1);
            double[] weights = new WeightGeneratorMajority(localDataStore).Weights;
            ReductWeights reduct = new ReductWeights(localDataStore, localDataStore.DataStoreInfo.GetFieldIds(FieldTypes.Standard), 0, weights);

            InformationMeasureWeights roughMeasure = new InformationMeasureWeights();
            double result = roughMeasure.Calc(reduct);

            Assert.AreEqual(result, 1.0, 0.0000001);
        }

        [Test]
        public void EquivalenceClassMapTest()
        {
            Args parms = new Args();
            parms.SetParameter(ReductGeneratorParamHelper.TrainData, dataStoreTrain);
            parms.SetParameter(ReductGeneratorParamHelper.FactoryKey, ReductFactoryKeyHelper.ApproximateReductMajorityWeights);
            parms.SetParameter(ReductGeneratorParamHelper.PermutationCollection, ReductFactory.GetPermutationGenerator(parms).Generate(1000));

            for (double epsilon = 0.0; epsilon < 1.0; epsilon += 0.11)
            {
                parms.SetParameter(ReductGeneratorParamHelper.Epsilon, epsilon);
                IReductGenerator reductGenerator = ReductFactory.GetReductGenerator(parms);
                reductGenerator.Run();
                IReductStoreCollection storeCollection = reductGenerator.GetReductStoreCollection();
                IReductStore reductStore = storeCollection.FirstOrDefault();

                foreach (ReductWeights reduct in reductStore)
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

                        EquivalenceClass partitionEqClass = partitionMap.GetEquivalenceClass(dataVector);
                        EquivalenceClass reductEqClass = reduct.EquivalenceClasses.GetEquivalenceClass(dataVector);

                        foreach (long decisionValue in dataStoreTrain.DataStoreInfo.DecisionInfo.InternalValues())
                        {
                            Assert.AreEqual(partitionMap.GetEquivalenceClass(dataVector).GetNumberOfObjectsWithDecision(decisionValue),
                                            reduct.EquivalenceClasses.GetEquivalenceClass(dataVector).GetNumberOfObjectsWithDecision(decisionValue), "Numer of objects with decisionInternalValue");
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
            double[] weights = new WeightGeneratorMajority(dataStoreTrain).Weights;
            ReductWeights allAttributes = new ReductWeights(dataStoreTrain, dataStoreTrain.DataStoreInfo.GetFieldIds(FieldTypes.Standard), 0, weights);
            double allAttrMeasure = InformationMeasureBase.Construct(InformationMeasureType.Majority).Calc(allAttributes);

            Args parms = new Args(new string[] { ReductGeneratorParamHelper.FactoryKey,
                                                 ReductGeneratorParamHelper.TrainData },
                                  new Object[] { ReductFactoryKeyHelper.ApproximateReductMajorityWeights, dataStoreTrain });
            IPermutationGenerator permGen = ReductFactory.GetPermutationGenerator(parms);
            PermutationCollection permutationList = permGen.Generate(1000);
            parms.SetParameter(ReductGeneratorParamHelper.PermutationCollection, permutationList);
            parms.SetParameter(ReductGeneratorParamHelper.FactoryKey, ReductFactoryKeyHelper.ApproximateReductMajorityWeights);

            IReductGenerator reductGenerator = ReductFactory.GetReductGenerator(parms);
            reductGenerator.Run();
            IReductStore reductStore = reductGenerator.ReductPool;

            foreach (IReduct reduct in reductStore)
            {
                double localAttrMeasure = InformationMeasureBase.Construct(InformationMeasureType.Majority).Calc(reduct);
                Assert.GreaterOrEqual(1.0 / (double)dataStoreTrainInfo.NumberOfRecords, allAttrMeasure - localAttrMeasure);
                Assert.LessOrEqual((-1.0) / (double)dataStoreTrainInfo.NumberOfRecords, allAttrMeasure - localAttrMeasure);
            }
        }

        [Test]
        public void ReductRelativeTest()
        {
            double[] weights = new WeightGeneratorMajority(dataStoreTrain).Weights;
            ReductWeights allAttributes = new ReductWeights(dataStoreTrain, dataStoreTrain.DataStoreInfo.GetFieldIds(FieldTypes.Standard), 0, weights);
            double allAttrMeasure = InformationMeasureBase.Construct(InformationMeasureType.Relative).Calc(allAttributes);

            Args parms = new Args(new string[] { ReductGeneratorParamHelper.FactoryKey, ReductGeneratorParamHelper.TrainData }, new Object[] { ReductFactoryKeyHelper.ApproximateReductRelativeWeights, dataStoreTrain });
            IPermutationGenerator permGen = ReductFactory.GetPermutationGenerator(parms);
            PermutationCollection permutationList = permGen.Generate(1000);
            parms.SetParameter(ReductGeneratorParamHelper.PermutationCollection, permutationList);
            parms.SetParameter(ReductGeneratorParamHelper.FactoryKey, ReductFactoryKeyHelper.ApproximateReductRelativeWeights);

            IReductGenerator reductGenerator = ReductFactory.GetReductGenerator(parms);
            reductGenerator.Run();
            IReductStore reductStore = reductGenerator.ReductPool;

            foreach (IReduct reduct in reductStore)
            {
                double localAttrMeasure = InformationMeasureBase.Construct(InformationMeasureType.Relative).Calc(reduct);
                Assert.GreaterOrEqual(1.0 / (double)dataStoreTrainInfo.NumberOfRecords, allAttrMeasure - localAttrMeasure);
                Assert.LessOrEqual((-1.0) / (double)dataStoreTrainInfo.NumberOfRecords, allAttrMeasure - localAttrMeasure);
            }
        }
    }
}