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
            int numberOfPermutations = 10;
            PermutationCollection permList = permGenerator.Generate(numberOfPermutations);

            WeightGeneratorMajority weightGenerator = new WeightGeneratorMajority(data);

            Args parms = new Args();
            parms.AddParameter(ReductGeneratorParamHelper.DataStore, data);
            parms.AddParameter(ReductGeneratorParamHelper.NumberOfThreads, 1);
            parms.AddParameter(ReductGeneratorParamHelper.FactoryKey, ReductFactoryKeyHelper.GeneralizedMajorityDecision);
            parms.AddParameter(ReductGeneratorParamHelper.PermutationCollection, permList);
            parms.AddParameter(ReductGeneratorParamHelper.WeightGenerator, weightGenerator);
            parms.AddParameter(ReductGeneratorParamHelper.ApproximationRatio, 0.2);


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
            DataStore data = DataStore.Load(@"Data\dna_modified.trn", FileFormat.Rses1);

            PermutationGenerator permGenerator = new PermutationGenerator(data);
            int numberOfPermutations = 1000;
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
                int cutoff = RandomSingleton.Random.Next(0, permutation.Length - 1);

                int[] attributes = new int[cutoff + 1];
                for (int i = 0; i <= cutoff; i++)
                    attributes[i] = permutation[i];

                ReductGeneralizedMajorityDecision reduct = reductGenerator.CalculateReduct(attributes);
                for (int i = 0; i < attributes.Length; i++)
                    Console.Write("{0} ", attributes[i]);
                Console.Write("({0}) -> ", attributes.Length);
                Console.WriteLine("{0} {1}", reduct, reduct.Attributes.Count);
            }
        }

        

    }
}
