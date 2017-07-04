//
// NRough Framework
// http://www.nrough.net
//
// Copyright © Sebastian Widz, 2017
// authors at nrough.net

//This file is part of NRough.

//Foobar is free software; you can redistribute it and/or modify
//it under the terms of the GNU General Public License as published by
//the Free Software Foundation; either version 3 of the License, or
//(at your option) any later version.
//
//Foobar is distributed in the hope that it will be useful,
//but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
//GNU General Public License for more details.
//
//You should have received a copy of the GNU General Public License
//along with Foobar; if not, write to the Free Software
//Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA  02110-1301  USA
//
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
using NRough.Core.Random;
using NRough.Core.CollectionExtensions;

namespace NRough.Tests.MachineLearning.Roughsets
{
    [TestFixture]
    public class EquivalenceClassTest
    {
        [Test]
        public void CreateTest()
        {
            var data = Data.Benchmark.Factory.DnaModifiedTrain();
            //DataStore data = DataStore.Load(@"Data\dna_modified.trn", DataFormat.RSES1);

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
                int[] attributes = data.DataStoreInfo.SelectAttributeIds(a => a.IsStandard).ToArray();
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
            var data = Data.Benchmark.Factory.DnaModifiedTrain();
            //DataStore data = DataStore.Load(@"Data\dna_modified.trn", DataFormat.RSES1);

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