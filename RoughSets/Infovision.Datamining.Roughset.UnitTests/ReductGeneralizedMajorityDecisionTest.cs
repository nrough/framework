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
            Console.WriteLine("class ReductGeneralizedMajorityDecisionTest Seed: {0}", seed);
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
            parms.AddParameter(ReductGeneratorParamHelper.ApproximationRatio, 0.9);


            ReductGeneralizedMajorityDecisionGenerator reductGenerator = ReductFactory.GetReductGenerator(parms) as ReductGeneralizedMajorityDecisionGenerator;

            foreach (Permutation permutation in permList)
            {
                ReductGeneralizedMajorityDecision reduct = reductGenerator.CalculateReduct(permutation.ToArray());
                Console.WriteLine("{0} {1}", reduct, reduct.Attributes.Count);
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
            double reductQuality = new InformationMeasureWeights().Calc(reduct);

            //Show reduction result
            for (int i = 0; i < attributeSubset.Length; i++)
                Console.Write("{0} ", attributeSubset[i]);
            Console.Write("({0}) -> ", attributeSubset.Length);
            Console.WriteLine("{0} (size:{1}) Quality: {2}", reduct, reduct.Attributes.Count, reductQuality);

            return reduct;
        }

        public IReduct CalculateApproximateReductFromSubset(DataStore data, double epsilon, int[] attributeSubset)
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
            double reductQuality = new InformationMeasureWeights().Calc(reduct);

            //Show reduction result
            for (int i = 0; i < attributeSubset.Length; i++)
                Console.Write("{0} ", attributeSubset[i]);
            Console.Write("({0}) -> ", attributeSubset.Length);
            Console.WriteLine("{0} (size:{1}) Quality: {2}", reduct, reduct.Attributes.Count, reductQuality);

            return reduct;
        }

        [Test]
        public void CheckIfApproximateReductASupersetOGeneralizedDecisionReduct()
        {
            Console.WriteLine("CheckIfApproximateReductASupersetOGeneralizedDecisionReduct()");
            string fileName = @"data\SPECT.train";
            int numberOfPermutations = 100;
            DataStore data = DataStore.Load(fileName, FileFormat.Rses1);
            PermutationGenerator permGenerator = new PermutationGenerator(data);
            PermutationCollection permutations = permGenerator.Generate(numberOfPermutations);

            int[] gd_attributes = null;
            int[] ar_attributes = null;
            foreach (Permutation permutation in permutations)
            {
                gd_attributes = CalculateGeneralizedDecisionReductFromSubset(data, 0.0, permutation.ToArray()).Attributes.ToArray();
                ar_attributes = CalculateApproximateReductFromSubset(data, 0.0, permutation.ToArray()).Attributes.ToArray();

                ShowInfoIsSupersetOf(data, gd_attributes, ar_attributes);

                gd_attributes = CalculateGeneralizedDecisionReductFromSubset(data, 0.1, permutation.ToArray()).Attributes.ToArray();
                ar_attributes = CalculateApproximateReductFromSubset(data, 0.1, permutation.ToArray()).Attributes.ToArray();
                ShowInfoIsSupersetOf(data, gd_attributes, ar_attributes);

                gd_attributes = CalculateGeneralizedDecisionReductFromSubset(data, 0.2, permutation.ToArray()).Attributes.ToArray();
                ar_attributes = CalculateApproximateReductFromSubset(data, 0.2, permutation.ToArray()).Attributes.ToArray();
                ShowInfoIsSupersetOf(data, gd_attributes, ar_attributes);

                gd_attributes = CalculateGeneralizedDecisionReductFromSubset(data, 0.3, permutation.ToArray()).Attributes.ToArray();
                ar_attributes = CalculateApproximateReductFromSubset(data, 0.3, permutation.ToArray()).Attributes.ToArray();
                ShowInfoIsSupersetOf(data, gd_attributes, ar_attributes);

                gd_attributes = CalculateGeneralizedDecisionReductFromSubset(data, 0.4, permutation.ToArray()).Attributes.ToArray();
                ar_attributes = CalculateApproximateReductFromSubset(data, 0.4, permutation.ToArray()).Attributes.ToArray();
                ShowInfoIsSupersetOf(data, gd_attributes, ar_attributes);

                gd_attributes = CalculateGeneralizedDecisionReductFromSubset(data, 0.5, permutation.ToArray()).Attributes.ToArray();
                ar_attributes = CalculateApproximateReductFromSubset(data, 0.5, permutation.ToArray()).Attributes.ToArray();
                ShowInfoIsSupersetOf(data, gd_attributes, ar_attributes);

                gd_attributes = CalculateGeneralizedDecisionReductFromSubset(data, 0.6, permutation.ToArray()).Attributes.ToArray();
                ar_attributes = CalculateApproximateReductFromSubset(data, 0.6, permutation.ToArray()).Attributes.ToArray();
                ShowInfoIsSupersetOf(data, gd_attributes, ar_attributes);

                gd_attributes = CalculateGeneralizedDecisionReductFromSubset(data, 0.7, permutation.ToArray()).Attributes.ToArray();
                ar_attributes = CalculateApproximateReductFromSubset(data, 0.7, permutation.ToArray()).Attributes.ToArray();
                ShowInfoIsSupersetOf(data, gd_attributes, ar_attributes);

                gd_attributes = CalculateGeneralizedDecisionReductFromSubset(data, 0.8, permutation.ToArray()).Attributes.ToArray();
                ar_attributes = CalculateApproximateReductFromSubset(data, 0.8, permutation.ToArray()).Attributes.ToArray();
                ShowInfoIsSupersetOf(data, gd_attributes, ar_attributes);

                gd_attributes = CalculateGeneralizedDecisionReductFromSubset(data, 0.9, permutation.ToArray()).Attributes.ToArray();
                ar_attributes = CalculateApproximateReductFromSubset(data, 0.9, permutation.ToArray()).Attributes.ToArray();
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

        [Test]
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
            IReduct allAttributes = new ReductWeights(data, data.DataStoreInfo.GetFieldIds(FieldTypes.Standard), weightGenerator.Weights, 0.0);
            IInformationMeasure measure = new InformationMeasureWeights();
            double dataQuality = measure.Calc(allAttributes);

            IReduct gdReduct = null;
            double gdQuality = 0.0;

            Dictionary<double, Dictionary<string, int>> results = new Dictionary<double, Dictionary<string, int>>();
            foreach (Permutation permutation in permutations)
            {
                for (double eps = 0.0; eps < 0.02;)
                {
                    gdReduct = CalculateGeneralizedDecisionReductFromSubset(data, eps, permutation.ToArray());
                    gdQuality = measure.Calc(gdReduct);

                    //Assert.LessOrEqual(gdQuality, ((1.0 - eps) * dataQuality)); 

                    Dictionary<string, int> resultsCount = null;
                    if (! results.TryGetValue(eps, out resultsCount))
                    {
                        resultsCount = new Dictionary<string, int>();
                        results.Add(eps, resultsCount);
                    }
                    
                    int counter = 0;
                    double threshold = ((1.0 - eps) * dataQuality);
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

                    if (eps < 0.09999999)
                    {
                        eps += 0.01;
                    }
                    else
                    {
                        eps += 0.1;
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

        [Test]
        public void CheckIfGeneralizedDecisionIsMoreStrictThanApproximateReduct2()
        {
            Console.WriteLine("CheckIfApproximateReductASupersetOGeneralizedDecisionReduct()");
            //string fileName = @"data\SPECT.train";
            string fileName = @"data\dna_modified.trn";
            int numberOfPermutations = 5;
            DataStore data = DataStore.Load(fileName, FileFormat.Rses1);
            PermutationGenerator permGenerator = new PermutationGenerator(data);
            PermutationCollection permutations = permGenerator.Generate(numberOfPermutations);

            WeightGeneratorMajority weightGenerator = new WeightGeneratorMajority(data);
            IReduct allAttributes = new ReductWeights(data, data.DataStoreInfo.GetFieldIds(FieldTypes.Standard), weightGenerator.Weights, 0.0);
            IInformationMeasure measure = new InformationMeasureWeights();
            double dataQuality = measure.Calc(allAttributes);

            IReduct gdReduct = null;
            double gdQuality = 0.0;

            Dictionary<double, Dictionary<string, int>> results = new Dictionary<double, Dictionary<string, int>>();
            foreach (Permutation permutation in permutations)
            {
                for (double eps = 0.0; eps < 1.0; )
                {
                    gdReduct = CalculateGeneralizedDecisionReductFromSubset(data, eps, permutation.ToArray());
                    gdQuality = measure.Calc(gdReduct);

                    Assert.LessOrEqual(dataQuality - gdQuality, eps);
                    
                    if (eps < 0.09999999)
                        eps += 0.01;
                    else
                        eps += 0.1;
                }
            }
        }
    }
}
