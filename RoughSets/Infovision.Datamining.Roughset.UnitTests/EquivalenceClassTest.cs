﻿using System;
using System.Linq;
using Infovision.Data;
using Infovision.Utils;
using NUnit.Framework;
using System.Diagnostics;
using System.Collections.Specialized;

namespace Infovision.Datamining.Roughset.UnitTests
{
    [TestFixture]
    public class EquivalenceClassTest
    {
        [Test]
        public void CreateTest()
        {
            DataStore data = DataStore.Load(@"Data\dna_modified.trn", FileFormat.Rses1);
            DataStore test = DataStore.Load(@"Data\dna_modified.tst", FileFormat.Rses1, data.DataStoreInfo);
            int[] attributes = data.DataStoreInfo.GetFieldIds(FieldTypes.Standard).ToArray();

            EquivalenceClassCollection eqClasses = EquivalenceClassCollection.Create(attributes, data);
            Assert.NotNull(eqClasses);
        }

        [Test]
        public void CreatePerformanceTest()
        {
            DataStore data = DataStore.Load(@"Data\dna_modified.trn", FileFormat.Rses1);
            int[] attributes = data.DataStoreInfo.GetFieldIds(FieldTypes.Standard).ToArray();

            Stopwatch s = new Stopwatch();
            s.Start(); 
            EquivalenceClassCollection eqClasses = EquivalenceClassCollection.Create(attributes, data);
            s.Stop();

            Console.WriteLine("Hashing: {0}ms", s.ElapsedMilliseconds);
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