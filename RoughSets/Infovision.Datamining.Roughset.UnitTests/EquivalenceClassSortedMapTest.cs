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
    public class EquivalenceClassTest
    {
        public EquivalenceClassTest()
        {
            Random randSeed = new Random();
            int seed = Guid.NewGuid().GetHashCode();
            RandomSingleton.Seed = seed;
        }
        
        [Test]
        public void RemoveObjectsWithMinorDecisionsTest()
        {
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

                IReduct reduct = new ReductWeights(data, attributes, 0.0M, weightGenerator.Weights);                                                    

                foreach (EquivalenceClass eq in reduct.EquivalenceClasses)
                {
                    int origNumberOfDecisionValues = eq.DecisionValues.Count();                    
                    eq.KeepMajorDecisions(Decimal.Zero);

                    if (origNumberOfDecisionValues > 1)
                        Assert.LessOrEqual(eq.DecisionValues.Count(), origNumberOfDecisionValues);

                    Assert.GreaterOrEqual(eq.DecisionValues.Count(), 1);                    
                    
                    if (eq.DecisionValues.Count() > 1)
                    {
                        int count = -1;                        
                        foreach (long dec in eq.DecisionValues)
                        {
                            if (count < 0)
                                count = eq.GetNumberOfObjectsWithDecision(dec);                                

                            Assert.AreEqual(count, eq.GetNumberOfObjectsWithDecision(dec));
                        }
                    }
                }                
            }                                                
        }
    }
}
