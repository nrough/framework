using System;
using System.Collections.Generic;
using System.Linq;
using NRough.Data;
using NRough.Core;
using NUnit.Framework;
using NRough.MachineLearning.Permutations;
using NRough.MachineLearning.Weighting;
using NRough.MachineLearning.Classification;
using NRough.MachineLearning.Roughsets;
using NRough.Core.Random;
using NRough.Core.CollectionExtensions;
using NRough.Core.Comparers;
using NRough.Data.Benchmark;
using NRough.MachineLearning.Roughsets.Reducts.Comparers;

namespace NRough.Tests.MachineLearning.Roughsets
{
    [TestFixture]
    internal class ReductGeneralizedMajorityDecisionTest
    {
        public ReductGeneralizedMajorityDecisionTest()
        {            
            RandomSingleton.Seed = Guid.NewGuid().GetHashCode();
        }

        public static IEnumerable<KeyValuePair<string, BenchmarkData>> GetDataFiles()
        {            
            return BenchmarkDataHelper.GetDataFiles("Data",
                new string[] {                    
                    //"zoo",
                    //"semeion",
                    //"opt",
                    "dna"
                    //"letter",
                    //"monks-1",
                    //"monks-2",
                    //"monks-3",
                    //"spect",
                    //"pen"
                    
                });
        }

        [Test]
        public void MajorityGeneralizedDecisionPerformanceTest()
        {
            DataStore trainData = DataStore.Load(@"Data\dna.train", DataFormat.RSES1);
            DataStore testData = DataStore.Load(@"Data\dna.test", DataFormat.RSES1, trainData.DataStoreInfo);

            WeightGeneratorRelative weightGenerator = new WeightGeneratorRelative(trainData);
            double eps = 0.0;
            int ensembleSize = 10;
            int ratio = 1;
            int permutationSize = ensembleSize * ratio;

            ReductLengthComparer reductLengthComparer = new ReductLengthComparer();
            ReductMeasureLength reductMeasureLength = new ReductMeasureLength();

            Args parms_GMDR = new Args();
            parms_GMDR.SetParameter(ReductFactoryOptions.DecisionTable, trainData);
            parms_GMDR.SetParameter(ReductFactoryOptions.ReductType, ReductTypes.GeneralizedMajorityDecision);
            parms_GMDR.SetParameter(ReductFactoryOptions.WeightGenerator, weightGenerator);
            parms_GMDR.SetParameter(ReductFactoryOptions.Epsilon, eps);

            IPermutationGenerator permGen = ReductFactory.GetPermutationGenerator(parms_GMDR);
            PermutationCollection permList = permGen.Generate(ensembleSize * 10);

            parms_GMDR.SetParameter(ReductFactoryOptions.PermutationCollection, permList);
            parms_GMDR.SetParameter(ReductFactoryOptions.UseExceptionRules, false);

            ReductGeneralizedMajorityDecisionGenerator generator_GMDR =
                ReductFactory.GetReductGenerator(parms_GMDR) as ReductGeneralizedMajorityDecisionGenerator;
            generator_GMDR.Run();
            IReductStoreCollection origReductStoreCollection_GMDR = generator_GMDR.GetReductStoreCollection();
            IReductStoreCollection filteredReductStoreCollection_GMDR = origReductStoreCollection_GMDR.Filter(ensembleSize, reductLengthComparer);

            RoughClassifier classifier_GMDR = new RoughClassifier(
                filteredReductStoreCollection_GMDR,
                RuleQualityMethods.CoverageW,
                RuleQualityMethods.CoverageW,
                trainData.DataStoreInfo.GetDecisionValues());
            classifier_GMDR.UseExceptionRules = false;
            ClassificationResult result_GMDR = classifier_GMDR.Classify(testData);
            result_GMDR.AvgNumberOfAttributes = filteredReductStoreCollection_GMDR.GetAvgMeasure(reductMeasureLength, false);
            result_GMDR.ModelCreationTime = generator_GMDR.ReductGenerationTime;
            result_GMDR.ClassificationTime = classifier_GMDR.ClassificationTime;

            //Console.WriteLine(ClassificationResult.ResultHeader());
            //Console.WriteLine(result_GMDR);

            Args parmsApprox = new Args();
            parmsApprox.SetParameter(ReductFactoryOptions.DecisionTable, trainData);
            parmsApprox.SetParameter(ReductFactoryOptions.ReductType, ReductTypes.ApproximateReductMajorityWeights);
            parmsApprox.SetParameter(ReductFactoryOptions.WeightGenerator, weightGenerator);
            parmsApprox.SetParameter(ReductFactoryOptions.Epsilon, eps);
            parmsApprox.SetParameter(ReductFactoryOptions.PermutationCollection, permList);
            parmsApprox.SetParameter(ReductFactoryOptions.UseExceptionRules, false);

            ReductGeneratorWeightsMajority generatorApprox =
                ReductFactory.GetReductGenerator(parmsApprox) as ReductGeneratorWeightsMajority;
            generatorApprox.Run();
            IReductStoreCollection origReductStoreCollectionApprox = generatorApprox.GetReductStoreCollection();
            IReductStoreCollection filteredReductStoreCollectionApprox = origReductStoreCollectionApprox.Filter(ensembleSize, reductLengthComparer);

            RoughClassifier classifierApprox = new RoughClassifier(
                filteredReductStoreCollectionApprox,
                RuleQualityMethods.CoverageW,
                RuleQualityMethods.CoverageW,
                trainData.DataStoreInfo.GetDecisionValues());
            classifierApprox.UseExceptionRules = false;
            ClassificationResult resultApprox = classifierApprox.Classify(testData);
            resultApprox.AvgNumberOfAttributes = filteredReductStoreCollectionApprox.GetAvgMeasure(reductMeasureLength, false);
            resultApprox.ModelCreationTime = generatorApprox.ReductGenerationTime;
            resultApprox.ClassificationTime = classifierApprox.ClassificationTime;

            //Console.WriteLine(resultApprox);
        }

        [Test, TestCaseSource("GetDataFiles")]
        public void CalculateReductTest(KeyValuePair<string, BenchmarkData> kvp)
        {
            DataStore data = DataStore.Load(kvp.Value.TrainFile, kvp.Value.DataFormat);
            //Console.WriteLine(data.Name);

            PermutationGenerator permGenerator = new PermutationGenerator(data);
            int numberOfPermutations = 1;
            PermutationCollection permList = permGenerator.Generate(numberOfPermutations);

            for (double eps = 0.0; eps < 1.0; eps += 0.1)
            {
                Args parms = new Args();
                parms.SetParameter(ReductFactoryOptions.DecisionTable, data);
                parms.SetParameter(ReductFactoryOptions.ReductType, ReductTypes.GeneralizedMajorityDecision);
                parms.SetParameter(ReductFactoryOptions.PermutationCollection, permList);
                parms.SetParameter(ReductFactoryOptions.WeightGenerator, new WeightGeneratorMajority(data));
                parms.SetParameter(ReductFactoryOptions.Epsilon, eps);

                ReductGeneralizedMajorityDecisionGenerator reductGenerator = ReductFactory.GetReductGenerator(parms) as ReductGeneralizedMajorityDecisionGenerator;

                foreach (Permutation permutation in permList)
                {
                    IReduct reduct = reductGenerator.CalculateReduct(permutation.ToArray());
                    Assert.NotNull(reduct);
                    Assert.GreaterOrEqual(reduct.Attributes.Count, 0);

                    //Console.WriteLine(reduct);
                }
            }
        }

        [Test, TestCaseSource("GetDataFiles")]
        public void CalculateReductFromSubset(KeyValuePair<string, BenchmarkData> kvp)
        {
            int numberOfPermutations = 1;
            int cutoff = 2;
            DataStore data = DataStore.Load(kvp.Value.TrainFile, kvp.Value.DataFormat);
            PermutationGenerator permGenerator = new PermutationGenerator(data);
            PermutationCollection permutations = permGenerator.Generate(numberOfPermutations);

            foreach (Permutation permutation in permutations)
            {
                int[] attributes = new int[cutoff + 1];
                for (int i = 0; i <= cutoff; i++)
                    attributes[i] = permutation[i];

                CalculateGeneralizedDecisionReductFromSubset(data, 0.0, attributes);
                CalculateGeneralizedDecisionReductFromSubset(data, 0.1, attributes);
                CalculateGeneralizedDecisionReductFromSubset(data, 0.2, attributes);
                CalculateGeneralizedDecisionReductFromSubset(data, 0.3, attributes);
                CalculateGeneralizedDecisionReductFromSubset(data, 0.4, attributes);
                CalculateGeneralizedDecisionReductFromSubset(data, 0.5, attributes);
                CalculateGeneralizedDecisionReductFromSubset(data, 0.6, attributes);
                CalculateGeneralizedDecisionReductFromSubset(data, 0.7, attributes);
                CalculateGeneralizedDecisionReductFromSubset(data, 0.8, attributes);
                CalculateGeneralizedDecisionReductFromSubset(data, 0.9, attributes);
            }
        }

        public IReduct CalculateGeneralizedDecisionReductFromSubset(DataStore data, double epsilon, int[] attributeSubset)
        {
            Args parms = new Args();
            parms.SetParameter(ReductFactoryOptions.DecisionTable, data);
            parms.SetParameter(ReductFactoryOptions.ReductType, ReductTypes.GeneralizedMajorityDecision);
            parms.SetParameter(ReductFactoryOptions.WeightGenerator, new WeightGeneratorMajority(data));
            parms.SetParameter(ReductFactoryOptions.Epsilon, epsilon);
            parms.SetParameter(ReductFactoryOptions.UseExceptionRules, false);
            parms.SetParameter(ReductFactoryOptions.EquivalenceClassSortDirection, SortDirection.Descending);

            ReductGeneralizedMajorityDecisionGenerator reductGenerator =
                ReductFactory.GetReductGenerator(parms) as ReductGeneralizedMajorityDecisionGenerator;

            IReduct result = reductGenerator.CalculateReduct((int[])attributeSubset.Clone());
            result.EquivalenceClasses.RecalcEquivalenceClassStatistic(data);

            return result;
        }

        public IReduct CalculateApproximateReductFromSubset(DataStore data, double epsilon, int[] attributeSubset)
        {
            Args parms = new Args();
            parms.SetParameter(ReductFactoryOptions.DecisionTable, data);
            parms.SetParameter(ReductFactoryOptions.ReductType, ReductTypes.ApproximateReductMajorityWeights);
            parms.SetParameter(ReductFactoryOptions.WeightGenerator, new WeightGeneratorMajority(data));
            parms.SetParameter(ReductFactoryOptions.Epsilon, epsilon);            

            ReductGeneratorWeightsMajority reductGenerator =
                ReductFactory.GetReductGenerator(parms) as ReductGeneratorWeightsMajority;

            reductGenerator.UsePerformanceImprovements = false;

            IReduct result = reductGenerator.CalculateReduct((int[])attributeSubset.Clone());
            return result;
        }

        [Test, TestCaseSource("GetDataFiles")]
        public void CheckIfApproximateReductASupersetOGeneralizedDecisionReduct(KeyValuePair<string, BenchmarkData> kvp)
        {
            int numberOfPermutations = 20;
            DataStore trainData = DataStore.Load(kvp.Value.TrainFile, kvp.Value.DataFormat);
            PermutationGenerator permGenerator = new PermutationGenerator(trainData);
            PermutationCollection permutations = permGenerator.Generate(numberOfPermutations);

            //This test works only for eps = 0.0;
            //For greater epsilon values there are slight differentces in attribute removal.
            //For example some attribute are replace with other having the same quality
            //Thus IsSubsetOf test fails

            double eps = 0.0;
            //for (double eps = 0.2; eps < 0.5; eps += 0.1)
            //{

            foreach (Permutation permutation in permutations)
            {
                int[] attributes = (int[])permutation.ToArray().Clone();

                Console.WriteLine(attributes.ToStr(" "));

                int[] gd_attributes = CalculateGeneralizedDecisionReductFromSubset(
                    trainData, eps, attributes).Attributes.ToArray();

                int[] ar_attributes = CalculateApproximateReductFromSubset(
                    trainData, eps, attributes).Attributes.ToArray();

                Assert.IsTrue(
                    IsSupersetOf(
                        trainData, 
                        gd_attributes, 
                        ar_attributes), 
                    String.Format("{0} is not superset of {1} (eps={2})", 
                        gd_attributes.ToStr(), 
                        ar_attributes.ToStr(), 
                        eps));
            }
            //}
        }

        [Test]
        public void CheckIfApproximateReductASupersetOGeneralizedDecisionReduct2()
        {            
            DataStore trainData = DataStore.Load(@"data\playgolf.train", DataFormat.RSES1);            
            double eps = 0.0;
            //double eps = 0.2; This will not work

            int[] attributes = new int[] { 4, 3, 2, 1 };

            Console.WriteLine(attributes.ToStr(" "));

            int[] gd_attributes = CalculateGeneralizedDecisionReductFromSubset(
                trainData, eps, attributes).Attributes.ToArray();

            int[] ar_attributes = CalculateApproximateReductFromSubset(
                trainData, eps, attributes).Attributes.ToArray();

            Assert.IsTrue(
                IsSupersetOf(
                    trainData,
                    gd_attributes,
                    ar_attributes),
                String.Format("{0} is not superset of {1} (eps={2})",
                    gd_attributes.ToStr(),
                    ar_attributes.ToStr(),
                    eps));
        }

        public bool IsSupersetOf(DataStore data, int[] setToCheck, int[] set)
        {
            HashSet<int> fieldSetToCheck = new HashSet<int>(setToCheck);
            HashSet<int> fielsSet = new HashSet<int>(set);
            return fieldSetToCheck.IsSupersetOf(set);
        }

        [Test, TestCaseSource("GetDataFiles")]
        public void CheckIfGeneralizedDecisionIsMoreStrictThanApproximateReduct(KeyValuePair<string, BenchmarkData> kvp)
        {
            int numberOfPermutations = 10;
            DataStore data = DataStore.Load(kvp.Value.TrainFile, kvp.Value.DataFormat);
            PermutationGenerator permGenerator = new PermutationGenerator(data);
            PermutationCollection permutations = permGenerator.Generate(numberOfPermutations);

            IReduct allAttributes =
                new ReductWeights(data, data.DataStoreInfo.SelectAttributeIds(a => a.IsStandard), 0.0, new WeightGeneratorMajority(data).Weights);

            double dataQuality = InformationMeasureWeights.Instance.Calc(allAttributes);

            for (double eps = 0.0; eps < 0.5; eps += 0.1)
            {
                foreach (Permutation permutation in permutations)
                {
                    IReduct gdReduct = CalculateGeneralizedDecisionReductFromSubset(data, eps, permutation.ToArray());
                    double gdQuality = InformationMeasureWeights.Instance.Calc(gdReduct);

                    Assert.That(gdQuality,
                        Is.GreaterThanOrEqualTo((1.0 - eps) * dataQuality)
                        .Using(ToleranceDoubleComparer.Instance),
                        String.Format("{0} M(B)={1}", gdReduct, gdQuality));
                }
            }
        }

        [Test, TestCaseSource("GetDataFiles")]
        public void CheckIfGeneralizedDecisionIsMoreStrictThanApproximateReduct2(KeyValuePair<string, BenchmarkData> kvp)
        {
            int numberOfPermutations = 10;
            DataStore data = DataStore.Load(kvp.Value.TrainFile, kvp.Value.DataFormat);
            PermutationGenerator permGenerator = new PermutationGenerator(data);
            PermutationCollection permutations = permGenerator.Generate(numberOfPermutations);

            IReduct allAttributes =
                new ReductWeights(data, data.DataStoreInfo.SelectAttributeIds(a => a.IsStandard), 0.0, new WeightGeneratorMajority(data).Weights);

            IInformationMeasure measure = new InformationMeasureWeights();
            double dataQuality = measure.Calc(allAttributes);

            for (double eps = 0.0; eps < 0.5; eps += 0.1)
            {
                foreach (Permutation permutation in permutations)
                {
                    IReduct gdReduct = CalculateGeneralizedDecisionReductFromSubset(data, eps, permutation.ToArray());
                    double gdQuality = measure.Calc(gdReduct);

                    Assert.That(dataQuality - gdQuality, Is.LessThanOrEqualTo(eps).Using(ToleranceDoubleComparer.Instance));                    
                }
            }
        }
    }
}