using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Infovision.Data;
using Infovision.Utils;
using NUnit.Framework;

namespace Infovision.Datamining.Roughset.UnitTests
{
    [TestFixture]
    public class EquivalenceClassSortedMapTest
    {
        [Test]
        public void RemoveObjectsWithMinorDecisionsTest()
        {
            Random randSeed = new Random();
            int seed = randSeed.Next(Int32.MaxValue);
            RandomSingleton.Seed = seed;

            DataStore data = DataStore.Load(@"Data\dna_modified.trn", FileFormat.Rses1);

            PermutationGenerator permGenerator = new PermutationGenerator(data);
            int numberOfPermutations = 100;
            PermutationCollection permList = permGenerator.Generate(numberOfPermutations);

            WeightGeneratorMajority weightGenerator = new WeightGeneratorMajority(data);

            foreach (Permutation permutation in permList)
            {
                int cutoff = RandomSingleton.Random.Next(0, permutation.Length - 1);               

                int[] attributes = new int[cutoff + 1];
                for (int i = 0; i <= cutoff; i++)
                    attributes[i] = permutation[i];

                ReductCrisp reduct = new ReductCrisp(data, attributes, weightGenerator.Weights, 0.0);                                                    

                foreach (EquivalenceClass eq in reduct.EquivalenceClassMap)
                {
                    int origNumberOfDecisionValues = eq.DecisionValues.Count();
                    long origMajorDecision = eq.MajorDecision;

                    eq.RemoveObjectsWithMinorDecisions();

                    if (origNumberOfDecisionValues > 1)
                        Assert.LessOrEqual(eq.DecisionValues.Count(), origNumberOfDecisionValues);

                    Assert.GreaterOrEqual(eq.DecisionValues.Count(), 1);
                    Assert.AreEqual(origMajorDecision, eq.MajorDecision);

                    if (eq.DecisionValues.Count() > 1)
                    {
                        int count = -1;                        
                        foreach (long dec in eq.DecisionValues)
                        {
                            if (count < 0)
                            {
                                count = eq.GetNumberOfObjectsWithDecision(dec);                                
                            }

                            Assert.AreEqual(count, eq.GetNumberOfObjectsWithDecision(dec));

                        }
                    }
                }                
            }                                                
        }
    }
}
