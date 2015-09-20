using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Infovision.Data;
using Infovision.Datamining.Clustering.Hierarchical;
using Infovision.Math;
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
            int seed = Guid.NewGuid().GetHashCode();
            //Console.WriteLine("class ReductGeneralizedMajorityDecisionTest Seed: {0}", seed);
            RandomSingleton.Seed = seed;
        }
        
        [Test]
        public void CalculateReductTest()
        {
            DataStore data = DataStore.Load(@"Data\dna_modified.trn", FileFormat.Rses1);

            PermutationGenerator permGenerator = new PermutationGenerator(data);
            int numberOfPermutations = 10000;
            PermutationCollection permList = permGenerator.Generate(numberOfPermutations);

            WeightGeneratorMajority weightGenerator = new WeightGeneratorMajority(data);

            Args parms = new Args();
            parms.AddParameter(ReductGeneratorParamHelper.DataStore, data);
            parms.AddParameter(ReductGeneratorParamHelper.NumberOfThreads, 1);
            parms.AddParameter(ReductGeneratorParamHelper.FactoryKey, ReductFactoryKeyHelper.GeneralizedMajorityDecision);
            parms.AddParameter(ReductGeneratorParamHelper.PermutationCollection, permList);
            parms.AddParameter(ReductGeneratorParamHelper.WeightGenerator, weightGenerator);
            parms.AddParameter(ReductGeneratorParamHelper.ApproximationRatio, 0.9M);


            ReductGeneralizedMajorityDecisionGenerator reductGenerator = ReductFactory.GetReductGenerator(parms) as ReductGeneralizedMajorityDecisionGenerator;

            foreach (Permutation permutation in permList)
            {
                ReductGeneralizedMajorityDecision reduct = reductGenerator.CalculateReduct(permutation.ToArray());
                Assert.NotNull(reduct);
                Assert.GreaterOrEqual(reduct.Attributes.Count, 0);
                //Console.WriteLine("{0} {1}", reduct, reduct.Attributes.Count);
            }
        }

        [Test]
        public void CalculateReductFromSubset()
        {
            Console.WriteLine("ReductGeneralizedMajorityDecisionTest.CalculateGeneralizedDecisionReductFromSubset()");
            string fileName = @"data\SPECT.train";
            int numberOfPermutations = 1;
            int cutoff = 2;
            DataStore data = DataStore.Load(fileName, FileFormat.Rses1);
            PermutationGenerator permGenerator = new PermutationGenerator(data);
            PermutationCollection permutations = permGenerator.Generate(numberOfPermutations);

            foreach (Permutation permutation in permutations)
            {
                //Generate random subset from permutation
                //int cutoff = RandomSingleton.Random.Next(0, permutation.Length - 1);
                
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
            //Console.WriteLine("Filename: {0}", data.Name);
            //Console.WriteLine("Epsilon: {0}", epsilon);
            
            WeightGeneratorMajority weightGenerator = new WeightGeneratorMajority(data);

            Args parms = new Args();
            parms.AddParameter(ReductGeneratorParamHelper.DataStore, data);
            parms.AddParameter(ReductGeneratorParamHelper.NumberOfThreads, 1);
            parms.AddParameter(ReductGeneratorParamHelper.FactoryKey, ReductFactoryKeyHelper.GeneralizedMajorityDecision);
            parms.AddParameter(ReductGeneratorParamHelper.WeightGenerator, weightGenerator);
            parms.AddParameter(ReductGeneratorParamHelper.ApproximationRatio, epsilon);

            ReductGeneralizedMajorityDecisionGenerator reductGenerator = ReductFactory.GetReductGenerator(parms) as ReductGeneralizedMajorityDecisionGenerator;
                   
            //Calculate reduct
            ReductGeneralizedMajorityDecision reduct = reductGenerator.CalculateReduct(attributeSubset);
            decimal reductQuality = new InformationMeasureWeights().Calc(reduct);

            //Show reduction result
            //for (int i = 0; i < attributeSubset.Length; i++)
            //    Console.Write("{0} ", attributeSubset[i]);
            //Console.Write("({0}) -> ", attributeSubset.Length);
            //Console.WriteLine("{0} (size:{1}) Quality: {2}", reduct, reduct.Attributes.Count, reductQuality);

            return reduct;
        }

        public IReduct CalculateApproximateReductFromSubset(DataStore data, decimal epsilon, int[] attributeSubset)
        {            
            //Console.WriteLine("Epsilon: {0}", epsilon);

            WeightGeneratorMajority weightGenerator = new WeightGeneratorMajority(data);

            Args parms = new Args();
            parms.AddParameter(ReductGeneratorParamHelper.DataStore, data);
            parms.AddParameter(ReductGeneratorParamHelper.NumberOfThreads, 1);
            parms.AddParameter(ReductGeneratorParamHelper.FactoryKey, ReductFactoryKeyHelper.ApproximateReductMajorityWeights);
            parms.AddParameter(ReductGeneratorParamHelper.WeightGenerator, weightGenerator);
            parms.AddParameter(ReductGeneratorParamHelper.ApproximationRatio, epsilon);

            ReductGeneratorWeightsMajority reductGenerator = ReductFactory.GetReductGenerator(parms) as ReductGeneratorWeightsMajority;

            //Calculate reduct
            ReductWeights reduct = reductGenerator.CalculateReduct(attributeSubset) as ReductWeights;
            decimal reductQuality = new InformationMeasureWeights().Calc(reduct);

            //Show reduction result
            //for (int i = 0; i < attributeSubset.Length; i++)
            //    Console.Write("{0} ", attributeSubset[i]);
            //Console.Write("({0}) -> ", attributeSubset.Length);
            //Console.WriteLine("{0} (size:{1}) Quality: {2}", reduct, reduct.Attributes.Count, reductQuality);

            return reduct;
        }

        [Test, Ignore]
        public void CheckIfApproximateReductASupersetOGeneralizedDecisionReduct()
        {
            //Console.WriteLine("CheckIfApproximateReductASupersetOGeneralizedDecisionReduct()");
            string fileName = @"data\SPECT.train";
            int numberOfPermutations = 100;
            DataStore data = DataStore.Load(fileName, FileFormat.Rses1);
            PermutationGenerator permGenerator = new PermutationGenerator(data);
            PermutationCollection permutations = permGenerator.Generate(numberOfPermutations);

            int[] gd_attributes = null;
            int[] ar_attributes = null;
            foreach (Permutation permutation in permutations)
            {
                gd_attributes = CalculateGeneralizedDecisionReductFromSubset(data, 0.0M, permutation.ToArray()).Attributes.ToArray();
                ar_attributes = CalculateApproximateReductFromSubset(data, 0.0M, permutation.ToArray()).Attributes.ToArray();

                ShowInfoIsSupersetOf(data, gd_attributes, ar_attributes);

                gd_attributes = CalculateGeneralizedDecisionReductFromSubset(data, 0.1M, permutation.ToArray()).Attributes.ToArray();
                ar_attributes = CalculateApproximateReductFromSubset(data, 0.1M, permutation.ToArray()).Attributes.ToArray();
                ShowInfoIsSupersetOf(data, gd_attributes, ar_attributes);

                gd_attributes = CalculateGeneralizedDecisionReductFromSubset(data, 0.2M, permutation.ToArray()).Attributes.ToArray();
                ar_attributes = CalculateApproximateReductFromSubset(data, 0.2M, permutation.ToArray()).Attributes.ToArray();
                ShowInfoIsSupersetOf(data, gd_attributes, ar_attributes);

                gd_attributes = CalculateGeneralizedDecisionReductFromSubset(data, 0.3M, permutation.ToArray()).Attributes.ToArray();
                ar_attributes = CalculateApproximateReductFromSubset(data, 0.3M, permutation.ToArray()).Attributes.ToArray();
                ShowInfoIsSupersetOf(data, gd_attributes, ar_attributes);

                gd_attributes = CalculateGeneralizedDecisionReductFromSubset(data, 0.4M, permutation.ToArray()).Attributes.ToArray();
                ar_attributes = CalculateApproximateReductFromSubset(data, 0.4M, permutation.ToArray()).Attributes.ToArray();
                ShowInfoIsSupersetOf(data, gd_attributes, ar_attributes);

                gd_attributes = CalculateGeneralizedDecisionReductFromSubset(data, 0.5M, permutation.ToArray()).Attributes.ToArray();
                ar_attributes = CalculateApproximateReductFromSubset(data, 0.5M, permutation.ToArray()).Attributes.ToArray();
                ShowInfoIsSupersetOf(data, gd_attributes, ar_attributes);

                gd_attributes = CalculateGeneralizedDecisionReductFromSubset(data, 0.6M, permutation.ToArray()).Attributes.ToArray();
                ar_attributes = CalculateApproximateReductFromSubset(data, 0.6M, permutation.ToArray()).Attributes.ToArray();
                ShowInfoIsSupersetOf(data, gd_attributes, ar_attributes);

                gd_attributes = CalculateGeneralizedDecisionReductFromSubset(data, 0.7M, permutation.ToArray()).Attributes.ToArray();
                ar_attributes = CalculateApproximateReductFromSubset(data, 0.7M, permutation.ToArray()).Attributes.ToArray();
                ShowInfoIsSupersetOf(data, gd_attributes, ar_attributes);

                gd_attributes = CalculateGeneralizedDecisionReductFromSubset(data, 0.8M, permutation.ToArray()).Attributes.ToArray();
                ar_attributes = CalculateApproximateReductFromSubset(data, 0.8M, permutation.ToArray()).Attributes.ToArray();
                ShowInfoIsSupersetOf(data, gd_attributes, ar_attributes);

                gd_attributes = CalculateGeneralizedDecisionReductFromSubset(data, 0.9M, permutation.ToArray()).Attributes.ToArray();
                ar_attributes = CalculateApproximateReductFromSubset(data, 0.9M, permutation.ToArray()).Attributes.ToArray();
                ShowInfoIsSupersetOf(data, gd_attributes, ar_attributes);
            }
        }

        public void ShowInfoIsSupersetOf(DataStore data, int[] setToCheck, int[] set)
        {
            FieldSet fieldSetToCheck = new FieldSet(data.DataStoreInfo, setToCheck);
            FieldSet fielsSet = new FieldSet(data.DataStoreInfo, set);

            if (fieldSetToCheck.Superset(set))
                Console.WriteLine("Is superset");
            else
                Console.WriteLine("Is not superset");
        }

        [Test, Ignore]
        public void CheckIfGeneralizedDecisionIsMoreStrictThanApproximateReduct()
        {
            Console.WriteLine("CheckIfApproximateReductASupersetOGeneralizedDecisionReduct()");
            //string fileName = @"data\SPECT.train";
            string fileName = @"data\dna_modified.trn";
            int numberOfPermutations = 5;
            DataStore data = DataStore.Load(fileName, FileFormat.Rses1);
            PermutationGenerator permGenerator = new PermutationGenerator(data);
            PermutationCollection permutations = permGenerator.Generate(numberOfPermutations);
            
            WeightGeneratorMajority weightGenerator = new WeightGeneratorMajority(data);
            IReduct allAttributes = new ReductWeights(data, data.DataStoreInfo.GetFieldIds(FieldTypes.Standard), weightGenerator.Weights, 0.0M);
            IInformationMeasure measure = new InformationMeasureWeights();
            decimal dataQuality = measure.Calc(allAttributes);

            IReduct gdReduct = null;
            decimal gdQuality = 0.0M;

            Dictionary<decimal, Dictionary<string, int>> results = new Dictionary<decimal, Dictionary<string, int>>();
            foreach (Permutation permutation in permutations)
            {
                for (decimal eps = 0.0M; eps < 0.02M;)
                {
                    gdReduct = CalculateGeneralizedDecisionReductFromSubset(data, eps, permutation.ToArray());
                    gdQuality = measure.Calc(gdReduct);

                    //Assert.LessOrEqual(gdQuality, ((1.0M - eps) * dataQuality)); 

                    Dictionary<string, int> resultsCount = null;
                    if (! results.TryGetValue(eps, out resultsCount))
                    {
                        resultsCount = new Dictionary<string, int>();
                        results.Add(eps, resultsCount);
                    }
                    
                    int counter = 0;
                    decimal threshold = ((1.0M - eps) * dataQuality);
                    if (gdQuality < threshold)
                    {
                        Console.WriteLine("GD ({0}) is less than AR ({1})", gdQuality, threshold);
                        if (resultsCount.TryGetValue("lt", out counter))
                        {
                            resultsCount["lt"] = counter + 1; 
                        }
                        else
                        {
                            resultsCount.Add("lt", 1);
                        }
                    }
                    else if (gdQuality > threshold)
                    {
                        Console.WriteLine("GD ({0}) is greater than AR ({1})", gdQuality, threshold);
                        if (resultsCount.TryGetValue("gt", out counter))
                        {
                            resultsCount["gt"] = counter + 1;
                        }
                        else
                        {
                            resultsCount.Add("gt", 1);
                        }
                    }
                    else
                    {
                        Console.WriteLine("GD ({0}) is less or equal than AR ({1})", gdQuality, threshold);
                        if (resultsCount.TryGetValue("eq", out counter))
                        {
                            resultsCount["eq"] = counter + 1;
                        }
                        else
                        {
                            resultsCount.Add("eq", 1);
                        }
                    }

                    if (eps < .09M)
                    {
                        eps += .01M;
                    }
                    else
                    {
                        eps += .1M;
                    }
                }
            }

            foreach (var epsKvp in results)
            {
                foreach (var countKvp in epsKvp.Value)
                {
                    Console.WriteLine("{0} {1} {2}", epsKvp.Key, countKvp.Key, countKvp.Value);
                }
            }
        }

        [Test, Ignore]
        public void CheckIfGeneralizedDecisionIsMoreStrictThanApproximateReduct2()
        {
            Console.WriteLine("CheckIfGeneralizedDecisionIsMoreStrictThanApproximateReduct2()");
            
            string fileName = @"data\dna_modified.trn";
            int numberOfPermutations = 5;
            DataStore data = DataStore.Load(fileName, FileFormat.Rses1);
            PermutationGenerator permGenerator = new PermutationGenerator(data);
            PermutationCollection permutations = permGenerator.Generate(numberOfPermutations);

            WeightGeneratorMajority weightGenerator = new WeightGeneratorMajority(data);
            IReduct allAttributes = new ReductWeights(data, data.DataStoreInfo.GetFieldIds(FieldTypes.Standard), weightGenerator.Weights, 0.0M);
            IInformationMeasure measure = new InformationMeasureWeights();
            decimal dataQuality = measure.Calc(allAttributes);

            IReduct gdReduct = null;
            decimal gdQuality = 0.0M;

            Dictionary<double, Dictionary<string, int>> results = new Dictionary<double, Dictionary<string, int>>();
            foreach (Permutation permutation in permutations)
            {
                for (decimal eps = 0.0M; eps < 1.0M; )
                {
                    gdReduct = CalculateGeneralizedDecisionReductFromSubset(data, eps, permutation.ToArray());
                    gdQuality = measure.Calc(gdReduct);

                    Assert.LessOrEqual(dataQuality - gdQuality, eps);
                    
                    if (eps < 0.09999999M)
                        eps += 0.01M;
                    else
                        eps += 0.1M;
                }
            }
        }
    }
}
