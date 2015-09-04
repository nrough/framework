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
    public class ReductGeneralDecisionGeneratorTest
    {
        [Test]
        public void TryRemoveAttribute()
        {
            Random randSeed = new Random();
            int seed = randSeed.Next(Int32.MaxValue);
            RandomSingleton.Seed = seed;

            DataStore data = DataStore.Load(@"Data\dna_modified.trn", FileFormat.Rses1);            

            PermutationGenerator permGenerator = new PermutationGenerator(data);
            int numberOfPermutations = 10;
            PermutationCollection permList = permGenerator.Generate(numberOfPermutations);

            WeightGeneratorMajority weightGenerator = new WeightGeneratorMajority(data);

            Args parms = new Args();
            parms.AddParameter("DataStore", data);
            parms.AddParameter("NumberOfThreads", 1);
            parms.AddParameter("FactoryKey", "ReductGeneralizedDecision");
            parms.AddParameter("PermutationCollection", permList);
            parms.AddParameter("WeightGenerator", weightGenerator);

            Dictionary<int, List<int>> results = new Dictionary<int, List<int>>();

            ReductGeneralDecisionGenerator reductGenerator = ReductFactory.GetReductGenerator(parms) as ReductGeneralDecisionGenerator;

            foreach (Permutation permutation in permList)
            {
                int cutoff = RandomSingleton.Random.Next(0, permutation.Length - 1);

                int[] attributes = new int[cutoff + 1];
                for (int i = 0; i <= cutoff; i++)
                    attributes[i] = permutation[i];

                ReductCrisp reduct = reductGenerator.CalculateReduct(attributes);
                for (int i = attributes.Length - 1; i >= 0; i--)
                {
                    if (reduct.TryRemoveAttribute(attributes[i]))
                    {
                        //TODO
                    }
                }
            }            
        }

        public Dictionary<string, BenchmarkData> GetDataFiles()
        {
            return BenchmarkDataHelper.GetDataFiles();
        }
        
        
        [Test, TestCaseSource("GetDataFiles")]
        public void ExperimentAvgReductLength(KeyValuePair<string, BenchmarkData> fileName)
        {
            Random randSeed = new Random();
            int seed = randSeed.Next(Int32.MaxValue);
            RandomSingleton.Seed = seed;

            DataStore data = DataStore.Load(fileName.Value.TrainFile, FileFormat.Rses1);            

            PermutationGenerator permGenerator = new PermutationGenerator(data);
            int numberOfPermutations = 1000;
            PermutationCollection permList = permGenerator.Generate(numberOfPermutations);

            WeightGeneratorMajority weightGenerator = new WeightGeneratorMajority(data);

            Args parms = new Args();
            parms.AddParameter("DataStore", data);
            parms.AddParameter("NumberOfThreads", 1);
            parms.AddParameter("FactoryKey", "ReductGeneralizedDecision");
            parms.AddParameter("PermutationCollection", permList);
            parms.AddParameter("WeightGenerator", weightGenerator);

            Dictionary<int, List<int>> results = new Dictionary<int, List<int>>();

            ReductGeneralDecisionGenerator reductGenerator = ReductFactory.GetReductGenerator(parms) as ReductGeneralDecisionGenerator;

            foreach (Permutation permutation in permList)
            {
                int cutoff = RandomSingleton.Random.Next(0, permutation.Length - 1);

                int[] attributes = new int[cutoff + 1];
                for (int i = 0; i <= cutoff; i++)
                    attributes[i] = permutation[i];

                ReductCrisp reduct = reductGenerator.CalculateReduct(attributes);
                List<int> cLen = null;
                if (results.TryGetValue(attributes.Length, out cLen))
                {
                    cLen.Add(reduct.Attributes.GetCardinality());
                }
                else
                {
                    cLen = new List<int>();
                    cLen.Add(reduct.Attributes.GetCardinality());
                    results.Add(attributes.Length, cLen);
                }
            }

            string fn = String.Format("F:\\Temp\\{0}_ReductGeneralDecisionGeneratorTest_ExperimentAvgReductLength.txt", fileName.Key);
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(fn))
            {
                foreach (KeyValuePair<int, List<int>> kvp in results)
                {
                    List<int> cLen = kvp.Value;
                    int sum = 0;
                    int min = Int32.MaxValue;
                    int max = Int32.MinValue;
                    foreach (int k in cLen)
                    {
                        sum += k;

                        if (min > k)
                            min = k;
                        if (max < k)
                            max = k;
                    }
                    double avgLen = (double)sum / (double)cLen.Count;

                    file.WriteLine(String.Format("{0} {1} {2} {3} {4}", kvp.Key, avgLen, cLen.Count, min, max));
                }
            }
        }
        
        
        [Test]
        public void GenerateRelativeTest()
        {            
            Random randSeed = new Random();
            int seed = randSeed.Next(Int32.MaxValue);
            RandomSingleton.Seed = seed;

            DataStore data = DataStore.Load(@"Data\dna_modified.trn", FileFormat.Rses1);

            PermutationGenerator permGenerator = new PermutationGenerator(data);
            int numberOfPermutations = 100;
            PermutationCollection permList = permGenerator.Generate(numberOfPermutations);

            WeightGeneratorRelative weightGenerator = new WeightGeneratorRelative(data);            

            Args parms = new Args();
            parms.AddParameter("DataStore", data);
            parms.AddParameter("NumberOfThreads", 1);
            parms.AddParameter("FactoryKey", "ReductGeneralizedDecision");
            parms.AddParameter("PermutationCollection", permList);
            parms.AddParameter("WeightGenerator", weightGenerator);                                                                       

            ReductGeneralDecisionGenerator reductGenerator = ReductFactory.GetReductGenerator(parms) as ReductGeneralDecisionGenerator;
            reductGenerator.Generate();
                        
            Args parms2 = new Args();
            parms2.AddParameter("DataStore", data);
            parms2.AddParameter("NumberOfThreads", 1);
            parms2.AddParameter("FactoryKey", "ApproximateReductRelativeWeights");
            parms2.AddParameter("PermutationCollection", permList);
            parms2.AddParameter("WeightGenerator", weightGenerator);

            ReductGeneratorWeightsRelative rGen2 = ReductFactory.GetReductGenerator(parms2) as ReductGeneratorWeightsRelative;            

            IReductStore reductPool = reductGenerator.ReductPool;
            foreach (IReduct reduct in reductPool)
            {                
                InformationMeasureWeights m_Weights = new InformationMeasureWeights();                
                double result_W = m_Weights.Calc(reduct);                                
                rGen2.Epsilon = 1.0 - result_W;

                ReductWeights approxReduct = new ReductWeights(data, reduct.Attributes.ToArray(), weightGenerator.Weights, rGen2.Epsilon);
                approxReduct.Id = reduct.Id;

                Console.WriteLine("{0} M(C)={1} eps={2}", approxReduct, result_W, rGen2.Epsilon);
                                
                //if(rGen2.CheckIsReduct(approxReduct) == false)
                Assert.IsTrue(rGen2.CheckIsReduct(approxReduct), String.Format("{0} is not a reduct for eps={1}", approxReduct, rGen2.Epsilon));                
                
                foreach (int attributeId in approxReduct.Attributes)
                {
                    approxReduct.TryRemoveAttribute(attributeId);
                    Assert.IsFalse(rGen2.CheckIsReduct(approxReduct), String.Format("Reduct should not be reducible. Attribute {0} can be removed", attributeId));                        
                    approxReduct.AddAttribute(attributeId);
                }                
            }            
        }

        [Test]
        public void GenerateMajorityTest()
        {
            Random randSeed = new Random();
            int seed = randSeed.Next(Int32.MaxValue);
            RandomSingleton.Seed = seed;

            DataStore data = DataStore.Load(@"Data\dna_modified.trn", FileFormat.Rses1);

            PermutationGenerator permGenerator = new PermutationGenerator(data);
            int numberOfPermutations = 100;
            PermutationCollection permList = permGenerator.Generate(numberOfPermutations);

            WeightGeneratorMajority weightGenerator = new WeightGeneratorMajority(data);

            Args parms = new Args();
            parms.AddParameter("DataStore", data);
            parms.AddParameter("NumberOfThreads", 1);
            parms.AddParameter("FactoryKey", "ReductGeneralizedDecision");
            parms.AddParameter("PermutationCollection", permList);
            parms.AddParameter("WeightGenerator", weightGenerator);

            ReductGeneralDecisionGenerator reductGenerator = ReductFactory.GetReductGenerator(parms) as ReductGeneralDecisionGenerator;
            reductGenerator.Generate();

            Args parms2 = new Args();
            parms2.AddParameter("DataStore", data);
            parms2.AddParameter("NumberOfThreads", 1);
            parms2.AddParameter("FactoryKey", "ApproximateReductMajorityWeights");
            parms2.AddParameter("PermutationCollection", permList);
            parms2.AddParameter("WeightGenerator", weightGenerator);

            ReductGeneratorWeightsMajority rGen2 = ReductFactory.GetReductGenerator(parms2) as ReductGeneratorWeightsMajority;

            IReductStore reductPool = reductGenerator.ReductPool;
            foreach (IReduct reduct in reductPool)
            {
                InformationMeasureWeights m_Weights = new InformationMeasureWeights();
                double result_W = m_Weights.Calc(reduct);
                
                rGen2.Epsilon = 1.0 - result_W;

                ReductWeights approxReduct = new ReductWeights(data, reduct.Attributes.ToArray(), weightGenerator.Weights, rGen2.Epsilon);
                approxReduct.Id = reduct.Id;

                Console.WriteLine("{0} M(C)={1} eps={2}", approxReduct, result_W, rGen2.Epsilon);

                //if(rGen2.CheckIsReduct(approxReduct) == false)
                Assert.IsTrue(rGen2.CheckIsReduct(approxReduct), String.Format("{0} is not a reduct for eps={1}", approxReduct, rGen2.Epsilon));

                foreach (int attributeId in approxReduct.Attributes)
                {
                    approxReduct.TryRemoveAttribute(attributeId);
                    Assert.IsFalse(rGen2.CheckIsReduct(approxReduct), String.Format("Reduct should not be reducible. Attribute {0} can be removed", attributeId));
                    approxReduct.AddAttribute(attributeId);
                }
            }
        }

        [Test]
        public void GenerateTest()
        {
            Random randSeed = new Random();
            int seed = randSeed.Next(Int32.MaxValue);
            RandomSingleton.Seed = seed;

            DataStore data = DataStore.Load(@"Data\dna_modified.trn", FileFormat.Rses1);

            PermutationGenerator permGenerator = new PermutationGenerator(data);
            int numberOfPermutations = 10;
            PermutationCollection permList = permGenerator.Generate(numberOfPermutations);

            WeightGeneratorRelative weightGenerator = new WeightGeneratorRelative(data);

            Args parms = new Args();
            parms.AddParameter("DataStore", data);
            parms.AddParameter("NumberOfThreads", 1);
            parms.AddParameter("FactoryKey", "ReductGeneralizedDecision");
            parms.AddParameter("PermutationCollection", permList);
            parms.AddParameter("WeightGenerator", weightGenerator);

            ReductGeneralDecisionGenerator reductGenerator = ReductFactory.GetReductGenerator(parms) as ReductGeneralDecisionGenerator;
            reductGenerator.Generate();

            Assert.True(true);
        }
    }
}
