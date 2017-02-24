using System;
using System.Linq;
using NRough.Data;
using NRough.Core;
using NUnit.Framework;
using System.Diagnostics;
using System.Collections.Specialized;
using NRough.MachineLearning.Permutations;
using NRough.MachineLearning.Weighting;
using NRough.MachineLearning;
using NRough.MachineLearning.Roughsets;

namespace NRough.Tests.MachineLearning.Roughsets
{
    [TestFixture]
    public class EquivalenceClassTest
    {
        [Test]
        public void CreateTest()
        {
            DataStore data = DataStore.Load(@"Data\dna_modified.trn", DataFormat.RSES1);
            DataStore test = DataStore.Load(@"Data\dna_modified.tst", DataFormat.RSES1, data.DataStoreInfo);
            int[] attributes = data.GetStandardFields();

            EquivalenceClassCollection eqClasses = EquivalenceClassCollection.Create(attributes, data);
            Assert.NotNull(eqClasses);
        }

        [Test]
        public void CreatePerformanceTest()
        {
            long sum = 0;
            long total = 30;
            for (int i = 0; i < total; i++)
            {
                DataStore data = DataStore.Load(@"Data\letter.trn", DataFormat.RSES1);
                int[] attributes = data.DataStoreInfo.GetFieldIds(FieldGroup.Standard).ToArray();
                attributes = attributes.RemoveAt(attributes.Length / 2, (attributes.Length / 2) - 1);

                Stopwatch s = new Stopwatch();
                s.Start();
                EquivalenceClassCollection eqClasses = EquivalenceClassCollection.Create(attributes, data);
                s.Stop();

                sum += s.ElapsedMilliseconds;
            }

            Console.WriteLine("EquivalenceClassCollection.Create(attributes, data) took {0}ms", sum / total);
        }

        [Test]
        public void RemoveObjectsWithMinorDecisionsTest()
        {
            DataStore data = DataStore.Load(@"Data\dna_modified.trn", DataFormat.RSES1);

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

                IReduct reduct = new ReductWeights(data, attributes, 0.0, weightGenerator.Weights);

                foreach (EquivalenceClass eq in reduct.EquivalenceClasses)
                {
                    int origNumberOfDecisionValues = eq.DecisionValues.Count();
                    eq.KeepMajorDecisions(0.0);

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