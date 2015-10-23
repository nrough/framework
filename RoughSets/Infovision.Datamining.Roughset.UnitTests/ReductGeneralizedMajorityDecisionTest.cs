﻿using System;
using System.Collections.Generic;
using Infovision.Data;
using Infovision.Utils;
using NUnit.Framework;

namespace Infovision.Datamining.Roughset.UnitTests
{
    [TestFixture]
    class ReductGeneralizedMajorityDecisionTest
    {
        public ReductGeneralizedMajorityDecisionTest()
        {
            Random randSeed = new Random();
            RandomSingleton.Seed = Guid.NewGuid().GetHashCode();
        }

        public IEnumerable<KeyValuePair<string, BenchmarkData>> GetDataFiles()
        {
            return BenchmarkDataHelper.GetDataFiles();
        }

        [Test, TestCaseSource("GetDataFiles")]
        public void CalculateReductTest(KeyValuePair<string, BenchmarkData> kvp)
        {
            DataStore data = DataStore.Load(kvp.Value.TrainFile, FileFormat.Rses1);
            Console.WriteLine(data.Name);

            PermutationGenerator permGenerator = new PermutationGenerator(data);
            int numberOfPermutations = 1;
            PermutationCollection permList = permGenerator.Generate(numberOfPermutations);

            for (decimal eps = Decimal.Zero; eps < Decimal.One; eps += 0.1m)
            {
                Args parms = new Args();
                parms.AddParameter(ReductGeneratorParamHelper.DataStore, data);
                parms.AddParameter(ReductGeneratorParamHelper.FactoryKey, ReductFactoryKeyHelper.GeneralizedMajorityDecision);
                parms.AddParameter(ReductGeneratorParamHelper.PermutationCollection, permList);
                parms.AddParameter(ReductGeneratorParamHelper.WeightGenerator, new WeightGeneratorMajority(data));
                parms.AddParameter(ReductGeneratorParamHelper.Epsilon, eps);

                ReductGeneralizedMajorityDecisionGenerator reductGenerator = ReductFactory.GetReductGenerator(parms) as ReductGeneralizedMajorityDecisionGenerator;

                foreach (Permutation permutation in permList)
                {
                    ReductGeneralizedMajorityDecision reduct = reductGenerator.CalculateReduct(permutation.ToArray());
                    Assert.NotNull(reduct);
                    Assert.GreaterOrEqual(reduct.Attributes.Count, 0);

                    Console.WriteLine(reduct);
                }
            }
        }

        [Test, TestCaseSource("GetDataFiles")]
        public void CalculateReductFromSubset(KeyValuePair<string, BenchmarkData> kvp)
        {
            int numberOfPermutations = 1;
            int cutoff = 2;
            DataStore data = DataStore.Load(kvp.Value.TrainFile, FileFormat.Rses1);
            PermutationGenerator permGenerator = new PermutationGenerator(data);
            PermutationCollection permutations = permGenerator.Generate(numberOfPermutations);

            foreach (Permutation permutation in permutations)
            {                
                int[] attributes = new int[cutoff + 1];
                for (int i = 0; i <= cutoff; i++)
                    attributes[i] = permutation[i];
                
                CalculateGeneralizedDecisionReductFromSubset(data, 0.0M, attributes);
                CalculateGeneralizedDecisionReductFromSubset(data, 0.1M, attributes);
                CalculateGeneralizedDecisionReductFromSubset(data, 0.2M, attributes);
                CalculateGeneralizedDecisionReductFromSubset(data, 0.3M, attributes);
                CalculateGeneralizedDecisionReductFromSubset(data, 0.4M, attributes);
                CalculateGeneralizedDecisionReductFromSubset(data, 0.5M, attributes);
                CalculateGeneralizedDecisionReductFromSubset(data, 0.6M, attributes);
                CalculateGeneralizedDecisionReductFromSubset(data, 0.7M, attributes);
                CalculateGeneralizedDecisionReductFromSubset(data, 0.8M, attributes);
                CalculateGeneralizedDecisionReductFromSubset(data, 0.9M, attributes);
            }
        }

        public IReduct CalculateGeneralizedDecisionReductFromSubset(DataStore data, decimal epsilon, int[] attributeSubset)
        {                                    
            Args parms = new Args();
            parms.AddParameter(ReductGeneratorParamHelper.DataStore, data);
            parms.AddParameter(ReductGeneratorParamHelper.FactoryKey, ReductFactoryKeyHelper.GeneralizedMajorityDecision);
            parms.AddParameter(ReductGeneratorParamHelper.WeightGenerator, new WeightGeneratorMajority(data));
            parms.AddParameter(ReductGeneratorParamHelper.Epsilon, epsilon);

            ReductGeneralizedMajorityDecisionGenerator reductGenerator = 
                ReductFactory.GetReductGenerator(parms) as ReductGeneralizedMajorityDecisionGenerator;                               
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

        [Test, TestCaseSource("GetDataFiles")]
        public void CheckIfApproximateReductASupersetOGeneralizedDecisionReduct(KeyValuePair<string, BenchmarkData> kvp)
        {
            int numberOfPermutations = 2;
            DataStore data = DataStore.Load(kvp.Value.TrainFile, FileFormat.Rses1);
            PermutationGenerator permGenerator = new PermutationGenerator(data);
            PermutationCollection permutations = permGenerator.Generate(numberOfPermutations);

            for (decimal eps = Decimal.Zero; eps < Decimal.One; eps += 0.001M)
            {
                foreach (Permutation permutation in permutations)
                {
                    int[] gd_attributes = CalculateGeneralizedDecisionReductFromSubset(
                        data, eps, permutation.ToArray()).Attributes.ToArray();
                    int[] ar_attributes = CalculateApproximateReductFromSubset(
                        data, eps, permutation.ToArray()).Attributes.ToArray();

                    Assert.IsTrue(IsSupersetOf(data, gd_attributes, ar_attributes));
                }
            }
        }

        public bool IsSupersetOf(DataStore data, int[] setToCheck, int[] set)
        {
            FieldSet fieldSetToCheck = new FieldSet(data.DataStoreInfo, setToCheck);
            FieldSet fielsSet = new FieldSet(data.DataStoreInfo, set);
            return fieldSetToCheck.Superset(set);
        }

        [Test, TestCaseSource("GetDataFiles")]
        public void CheckIfGeneralizedDecisionIsMoreStrictThanApproximateReduct(KeyValuePair<string, BenchmarkData> kvp)
        {
            int numberOfPermutations = 100;
            DataStore data = DataStore.Load(kvp.Value.TrainFile, FileFormat.Rses1);
            PermutationGenerator permGenerator = new PermutationGenerator(data);
            PermutationCollection permutations = permGenerator.Generate(numberOfPermutations);

            IReduct allAttributes = 
                new ReductWeights(
                    data, 
                    data.DataStoreInfo.GetFieldIds(FieldTypes.Standard), 
                    new WeightGeneratorMajority(data).Weights, 
                    Decimal.Zero);

            IInformationMeasure measure = new InformationMeasureWeights();
            decimal dataQuality = measure.Calc(allAttributes);

            for (decimal eps = Decimal.Zero; eps < Decimal.One; eps += 0.01M)
            {
                foreach (Permutation permutation in permutations)
                {    
                    IReduct gdReduct = CalculateGeneralizedDecisionReductFromSubset(data, eps, permutation.ToArray());
                    decimal gdQuality = measure.Calc(gdReduct);

                    Assert.GreaterOrEqual(
                        Decimal.Round(gdQuality, 17), 
                        Decimal.Round(((Decimal.One - eps) * dataQuality), 17),
                        String.Format("{0} M(B)={1}", gdReduct, gdQuality));        
                }
            }
        }

        [Test, TestCaseSource("GetDataFiles")]
        public void CheckIfGeneralizedDecisionIsMoreStrictThanApproximateReduct2(KeyValuePair<string, BenchmarkData> kvp)
        {   
            int numberOfPermutations = 100;
            DataStore data = DataStore.Load(kvp.Value.TrainFile, FileFormat.Rses1);
            PermutationGenerator permGenerator = new PermutationGenerator(data);
            PermutationCollection permutations = permGenerator.Generate(numberOfPermutations);

            IReduct allAttributes = 
                new ReductWeights(
                    data, 
                    data.DataStoreInfo.GetFieldIds(FieldTypes.Standard), 
                    new WeightGeneratorMajority(data).Weights, 
                    Decimal.Zero);

            IInformationMeasure measure = new InformationMeasureWeights();
            decimal dataQuality = measure.Calc(allAttributes);

            for (decimal eps = Decimal.Zero; eps < Decimal.One; eps+=0.01m)
            {
                foreach (Permutation permutation in permutations)
                {
                    IReduct gdReduct = CalculateGeneralizedDecisionReductFromSubset(data, eps, permutation.ToArray());
                    decimal gdQuality = measure.Calc(gdReduct);

                    Assert.LessOrEqual(Decimal.Round(dataQuality - gdQuality, 17), eps);
                }
            }
        }
    }
}