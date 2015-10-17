using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Infovision.Data;
using Infovision.Utils;
using NUnit.Framework;

namespace Infovision.Datamining.Roughset.UnitTests
{
    [TestFixture]
    public class ReductGeneralizedMajorityDecisionApproximateTest
    {
        [Test, TestCaseSource("GetDataFiles")]
        public void CalculateReductTest(KeyValuePair<string, BenchmarkData> kvp)
        {
            DataStore data = DataStore.Load(kvp.Value.TrainFile, FileFormat.Rses1);
            
            foreach (int fieldId in data.DataStoreInfo.GetFieldIds(FieldTypes.Standard))
                data.DataStoreInfo.GetFieldInfo(fieldId).Alias = kvp.Value.GetFieldAlias(fieldId);

            DataStore test = DataStore.Load(kvp.Value.TestFile, FileFormat.Rses1, data.DataStoreInfo);
            
            Console.WriteLine(data.Name);

            PermutationGenerator permGenerator = new PermutationGenerator(data);
            int numberOfPermutations = 100;
            PermutationCollection permList = permGenerator.Generate(numberOfPermutations);
            
            Decimal dataQuality = new InformationMeasureWeights().Calc(
                new ReductGeneralizedMajorityDecision(
                    data, data.DataStoreInfo.GetFieldIds(FieldTypes.Standard), Decimal.Zero));

            Decimal dataQuality_2 = new InformationMeasureWeights().Calc(
                new ReductWeights(
                    data, data.DataStoreInfo.GetFieldIds(FieldTypes.Standard), Decimal.Zero));

            Assert.AreEqual(Decimal.Round(dataQuality, 17), Decimal.Round(dataQuality_2, 17));

            for (decimal eps = Decimal.Zero; eps <= Decimal.One; eps += 0.01m)
            //for (decimal eps = 0.15m; eps <= Decimal.One; eps += 0.01m)
            {
                long elapsed_sum_1 = 0;
                long elapsed_sum_2 = 0;
                int len_sum_1 = 0;
                int len_sum_2 = 0;
                decimal avg_quality_1 = Decimal.Zero;
                decimal avg_quality_2 = Decimal.Zero;
                decimal avg_accuracy_1 = Decimal.Zero;
                decimal avg_coverage_1 = Decimal.Zero;
                decimal avg_accuracy_2 = Decimal.Zero;
                decimal avg_coverage_2 = Decimal.Zero;

                foreach (Permutation permutation in permList)
                {
                    int[] attributes = permutation.ToArray();

                    var watch_1 = Stopwatch.StartNew();

                    IReduct reduct_1 = CalculateGeneralizedMajorityApproximateDecisionReduct(data, eps, attributes);

                    watch_1.Stop();

                    Assert.NotNull(reduct_1);
                    Decimal reductQuality_1 = new InformationMeasureWeights().Calc(reduct_1);
                    Assert.GreaterOrEqual(reductQuality_1, Decimal.Round(dataQuality * (Decimal.One - eps), 17));

                    elapsed_sum_1 += watch_1.ElapsedMilliseconds;
                    len_sum_1 += reduct_1.Attributes.Count;
                    avg_quality_1 += reductQuality_1;

                    RoughClassifier classifier_1 = new RoughClassifier();
                    classifier_1.Classify(test, reduct_1);
                    ClassificationResult result_1 = classifier_1.Vote(
                        test, 
                        IdentificationType.WeightConfidence, 
                        VoteType.WeightConfidence, 
                        reduct_1.Weights);

                    avg_accuracy_1 += (decimal)result_1.Accuracy;
                    avg_coverage_1 += (decimal)result_1.Coverage;
                    
                    Console.WriteLine("A: {0} |A|={3} M(B)={1} T={2}ms Acc={4} Cov={5}",
                        reduct_1.ToString(),
                        reductQuality_1,
                        watch_1.ElapsedMilliseconds,
                        reduct_1.Attributes.Count,
                        result_1.Accuracy,
                        result_1.Coverage);

                    var watch_2 = Stopwatch.StartNew();
                    IReduct reduct_2 = CalculateApproximateReductFromSubset(data, eps, attributes);
                    watch_2.Stop();

                    Assert.NotNull(reduct_2);
                    Decimal reductQuality_2 = new InformationMeasureWeights().Calc(reduct_2);
                    Assert.GreaterOrEqual(reductQuality_2, Decimal.Round(dataQuality * (Decimal.One - eps), 17));

                    elapsed_sum_2 += watch_2.ElapsedMilliseconds;
                    len_sum_2 += reduct_2.Attributes.Count;
                    avg_quality_2 += reductQuality_2;

                    RoughClassifier classifier_2 = new RoughClassifier();
                    classifier_2.Classify(test, reduct_2);
                    ClassificationResult result_2 = classifier_2.Vote(
                        test,
                        IdentificationType.WeightConfidence,
                        VoteType.WeightConfidence,
                        reduct_2.Weights);

                    avg_accuracy_2 += (decimal)result_2.Accuracy;
                    avg_coverage_2 += (decimal)result_2.Coverage;

                    Console.WriteLine("B: {0} |A|={3} M(B)={1} T={2}ms Acc={4} Cov={5} {6}",
                        reduct_2.ToString(),
                        reductQuality_2,
                        watch_2.ElapsedMilliseconds,
                        reduct_2.Attributes.Count,
                        result_2.Accuracy,
                        result_2.Coverage,
                    reductQuality_1 < reductQuality_2 ? "*" : String.Empty);
                }

                Console.WriteLine("==========================================");
                Console.WriteLine("Average reduct lenght of method A: {0}", (double)len_sum_1 / (double)permList.Count);
                Console.WriteLine("Average reduct lenght of method B: {0}", (double)len_sum_2 / (double)permList.Count);
                Console.WriteLine("Average computation time method A: {0}", (double)elapsed_sum_1 / (double)permList.Count);
                Console.WriteLine("Average computation time method B: {0}", (double)elapsed_sum_2 / (double)permList.Count);
                Console.WriteLine("Average reduct quality of method A: {0}", avg_quality_1 / (decimal)permList.Count);
                Console.WriteLine("Average reduct quality of method B: {0}", avg_quality_2 / (decimal)permList.Count);
                Console.WriteLine("Average accuracy of method A: {0}", avg_accuracy_1 / (decimal)permList.Count);
                Console.WriteLine("Average accuracy of method B: {0}", avg_accuracy_2 / (decimal)permList.Count);
                Console.WriteLine("Average coverage of method A: {0}", avg_coverage_1 / (decimal)permList.Count);
                Console.WriteLine("Average coverage of method B: {0}", avg_coverage_2 / (decimal)permList.Count);
                Console.WriteLine("==========================================");

                Console.WriteLine();
            }
        }
        
        
        public ReductGeneralizedMajorityDecisionApproximateTest()
        {
            Random randSeed = new Random();
            RandomSingleton.Seed = Guid.NewGuid().GetHashCode();
        }

        public IEnumerable<KeyValuePair<string, BenchmarkData>> GetDataFiles()
        {
            return BenchmarkDataHelper.GetDataFiles(new string[] {"dna"});
        }

        public IReduct CalculateGeneralizedMajorityApproximateDecisionReduct(DataStore data, decimal epsilon, int[] attributeSubset)
        {
            Args parms = new Args();
            parms.AddParameter(ReductGeneratorParamHelper.DataStore, data);
            parms.AddParameter(ReductGeneratorParamHelper.FactoryKey, ReductFactoryKeyHelper.GeneralizedMajorityDecisionApproximate);
            parms.AddParameter(ReductGeneratorParamHelper.WeightGenerator, new WeightGeneratorMajority(data));
            parms.AddParameter(ReductGeneratorParamHelper.Epsilon, epsilon);

            ReductGeneralizedMajorityDecisionApproximateGenerator reductGenerator =
                ReductFactory.GetReductGenerator(parms) as ReductGeneralizedMajorityDecisionApproximateGenerator;
            return reductGenerator.CalculateReduct(attributeSubset);
        }

        public IReduct CalculateApproximateReductFromSubset(DataStore data, decimal epsilon, int[] attributeSubset)
        {
            Args parms = new Args();
            parms.AddParameter(ReductGeneratorParamHelper.DataStore, data);
            parms.AddParameter(ReductGeneratorParamHelper.FactoryKey, ReductFactoryKeyHelper.ApproximateReductMajorityWeights);
            parms.AddParameter(ReductGeneratorParamHelper.WeightGenerator, new WeightGeneratorMajority(data));
            parms.AddParameter(ReductGeneratorParamHelper.Epsilon, epsilon);

            ReductGeneratorWeightsMajority reductGenerator =
                ReductFactory.GetReductGenerator(parms) as ReductGeneratorWeightsMajority;
            return reductGenerator.CalculateReduct(attributeSubset) as ReductWeights;
        }        
    }
}
