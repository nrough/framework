// 
// NRough Framework
// http://www.nrough.net
// 
// Copyright (c) Sebastian Widz, 2017
// authors at nrough.net
// 
// This file is part of NRough Framework.
// 
// NRough Framework is free software; you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation; either version 3 of the License, or
// (at your option) any later version.
// 
// NRough Framework is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with NRough Framework; if not, write to the Free Software
// Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA  02110-1301  USA
//

using System;
using System.Collections.Generic;
using System.Linq;
using NRough.Data;
using NRough.Core;
using NUnit.Framework;
using NRough.MachineLearning.Permutations;
using NRough.Core.Random;

namespace NRough.Tests.MachineLearning.Roughsets
{
    [TestFixture]
    public class PermutationTest
    {
        private DataStore dataStore = null;

        public PermutationTest()
        {
            dataStore = DataStore.Load(@"Data\monks-1.train", DataFormat.RSES1);            
            RandomSingleton.Seed = Guid.NewGuid().GetHashCode();
        }

        [Test]
        public void FieldObjectPermutationRatioTest()
        {
            PermutationAttributeObjectGenerator permGen = new PermutationAttributeObjectGenerator(dataStore, 0.5);
            PermutationCollection permList = permGen.Generate(100);
            this.CheckPermutationCompletness(permList,
                dataStore.GetStandardFields(),
                Enumerable.Range(0, dataStore.NumberOfRecords).ToArray());
        }

        [Test]
        public void RelativePermutationRatioTest()
        {
            PermutationGeneratorFieldObjectRelative permGen = new PermutationGeneratorFieldObjectRelative(dataStore, 0.5);
            PermutationCollection permList = permGen.Generate(100);
            this.CheckPermutationCompletness(permList,
                dataStore.GetStandardFields(),
                Enumerable.Range(0, dataStore.NumberOfRecords).ToArray());
        }

        [Test]
        public void FieldObjectPermutationTest()
        {
            PermutationAttributeObjectGenerator permGen = new PermutationAttributeObjectGenerator(dataStore);
            PermutationCollection permList = permGen.Generate(100);
            this.CheckPermutationCompletness(permList,
                dataStore.GetStandardFields(),
                Enumerable.Range(0, dataStore.NumberOfRecords).ToArray());
        }

        [Test]
        public void RelativePermutationTest()
        {
            PermutationGeneratorFieldObjectRelative permGen = new PermutationGeneratorFieldObjectRelative(dataStore);
            PermutationCollection permList = permGen.Generate(100);
            this.CheckPermutationCompletness(permList,
                dataStore.GetStandardFields(),
                Enumerable.Range(0, dataStore.NumberOfRecords).ToArray());
        }

        [Test]
        public void FieldGroupPermutationTest()
        {
            PermutatioGeneratorFieldGroup permGen = new PermutatioGeneratorFieldGroup(dataStore);
            PermutationCollection permList = permGen.Generate(10);
            this.CheckPermutationCompletness(permList, dataStore.GetStandardFields(), new int[] { });
        }

        private void CheckPermutationCompletness(PermutationCollection permList, int[] fields, int[] objects)
        {
            foreach (Permutation perm in permList)
            {
                //Console.WriteLine(perm);

                Assert.AreEqual(fields.Length + objects.Length, perm.Length);

                foreach (int field in fields)
                {
                    bool found = false;
                    for (int i = 0; i < perm.Length; i++)
                    {
                        if (perm[i] == field)
                        {
                            Assert.AreEqual(false, found);
                            found = true;
                        }
                    }
                    Assert.AreEqual(true, found);
                }

                foreach (int objectId in objects)
                {
                    bool found = false;
                    for (int i = 0; i < perm.Length; i++)
                    {
                        if ((perm[i] * -1) == objectId)
                        {
                            Assert.AreEqual(false, found);
                            found = true;
                        }
                    }
                    Assert.AreEqual(true, found);
                }

                for (int i = 0; i < perm.Length; i++)
                {
                    bool found = false;
                    if (perm[i] > 0)
                    {
                        for (int j = 0; j < fields.Length; j++)
                        {
                            if (perm[i] == fields[j])
                            {
                                Assert.AreEqual(false, found);
                                found = true;
                            }
                        }
                    }
                    else
                    {
                        for (int j = 0; j < objects.Length; j++)
                        {
                            if ((perm[i] * -1) == objects[j])
                            {
                                Assert.AreEqual(false, found);
                                found = true;
                            }
                        }
                    }

                    Assert.AreEqual(true, found);
                }
            }
        }

        [Test]
        public void FieldSelecitonRatio()
        {
            this.CalcAndPrintRatio(10, 10, 0);
            this.CalcAndPrintRatio(10, 10, 0.01);
            this.CalcAndPrintRatio(10, 10, 0.2);
            this.CalcAndPrintRatio(10, 10, 0.5);
            this.CalcAndPrintRatio(10, 10, 0.7);
            this.CalcAndPrintRatio(10, 10, 0.99);
            this.CalcAndPrintRatio(10, 10, 1);
            //Console.WriteLine();

            this.CalcAndPrintRatio(10, 20, 0);
            this.CalcAndPrintRatio(10, 20, 0.01);
            this.CalcAndPrintRatio(10, 20, 0.2);
            this.CalcAndPrintRatio(10, 20, 0.5);
            this.CalcAndPrintRatio(10, 20, 0.7);
            this.CalcAndPrintRatio(10, 20, 0.99);
            this.CalcAndPrintRatio(10, 20, 1);
            //Console.WriteLine();

            this.CalcAndPrintRatio(20, 10, 0);
            this.CalcAndPrintRatio(20, 10, 0.01);
            this.CalcAndPrintRatio(20, 10, 0.2);
            this.CalcAndPrintRatio(20, 10, 0.5);
            this.CalcAndPrintRatio(20, 10, 0.7);
            this.CalcAndPrintRatio(20, 10, 0.99);
            this.CalcAndPrintRatio(20, 10, 1);
            //Console.WriteLine();

            this.CalcAndPrintRatio(10, 1000, 0);
            this.CalcAndPrintRatio(10, 1000, 0.01);
            this.CalcAndPrintRatio(10, 1000, 0.2);
            this.CalcAndPrintRatio(10, 1000, 0.5);
            this.CalcAndPrintRatio(10, 1000, 0.7);
            this.CalcAndPrintRatio(10, 1000, 0.99);
            this.CalcAndPrintRatio(10, 1000, 1);
            //Console.WriteLine();

            this.CalcAndPrintRatio(1000, 10, 0);
            this.CalcAndPrintRatio(1000, 10, 0.01);
            this.CalcAndPrintRatio(1000, 10, 0.2);
            this.CalcAndPrintRatio(1000, 10, 0.5);
            this.CalcAndPrintRatio(1000, 10, 0.7);
            this.CalcAndPrintRatio(1000, 10, 0.99);
            this.CalcAndPrintRatio(1000, 10, 1);
            //Console.WriteLine();

            this.CalcAndPrintRatio(10000, 10, 0);
            this.CalcAndPrintRatio(10000, 10, 0.01);
            this.CalcAndPrintRatio(10000, 10, 0.2);
            this.CalcAndPrintRatio(10000, 10, 0.5);
            this.CalcAndPrintRatio(10000, 10, 0.7);
            this.CalcAndPrintRatio(10000, 10, 0.99);
            this.CalcAndPrintRatio(10000, 10, 1);
            //Console.WriteLine();

            Assert.AreEqual(true, true);
        }

        private double CalcAndPrintRatio(int numberOfFields, int numberOfObjects, double fieldSelectionRatio)
        {
            double result = PermutationAttributeObjectGenerator.CalcSelectionRatio(numberOfFields, numberOfObjects, fieldSelectionRatio);
            //Console.WriteLine("{0} {1} {2} {3}", numberOfFields, numberOfObjects, fieldSelectionRatio, result);
            return result;
        }

        [Test]
        public void FieldImportance()
        {
            PermutationAttributeObjectGenerator permGen;
            PermutationCollection permList;
            int[] fields = dataStore.GetStandardFields();
            int[] objects = Enumerable.Range(0, dataStore.NumberOfRecords).ToArray();

            permGen = new PermutationAttributeObjectGenerator(objects, fields, 0);
            permList = permGen.Generate(100);

            foreach (Permutation perm in permList)
            {
                //Console.WriteLine(perm);

                Assert.AreEqual(fields.Length + objects.Length, perm.Length);
                for (int i = 0; i < perm.Length; i++)
                {
                    if (i < objects.Length)
                    {
                        Assert.LessOrEqual(perm[i], 0);
                    }
                    else
                    {
                        Assert.Greater(perm[i], 0);
                    }
                }
            }

            permGen = new PermutationAttributeObjectGenerator(objects, fields, 1);
            permList = permGen.Generate(100);

            foreach (Permutation perm in permList)
            {
                //Console.WriteLine(perm);
                Assert.AreEqual(fields.Length + objects.Length, perm.Length);
                for (int i = 0; i < perm.Length; i++)
                {
                    if (i < fields.Length)
                    {
                        Assert.Greater(perm[i], 0);
                    }
                    else
                    {
                        Assert.LessOrEqual(perm[i], 0);
                    }
                }
            }
        }

        [Test]
        public void GenNextObjectIdTest()
        {
            int[] array = new int[] { 1, 1, 1, 2, 2, 2, 3, 3, 3, 3, 3 };
            List<int> list = new List<Int32>(array);
            int sumArray = 24;

            for (int k = 0; k < array.Length; k++)
            {
                int number = RandomSingleton.Random.Next(sumArray) + 1;
                Assert.LessOrEqual(number, sumArray);
                int sum = 0;
                int i = -1;
                int j = -1;
                while (sum < number && i < array.Length - 1)
                {
                    i++;
                    sum += array[i];
                    if (array[i] > 0)
                        j++;
                }
                sumArray -= array[i];
                array[i] = 0;
                list.RemoveAt(j);
            }

            Assert.AreEqual(0, list.Count);
            Assert.AreEqual(0, sumArray);
        }

        [Test]
        public void PermutationFieldGroup()
        {
            int[] fields = dataStore.GetStandardFields();

            PermutatioGeneratorFieldGroup generator = new PermutatioGeneratorFieldGroup(new int[][] {
                                                                                                        new int[] {},
                                                                                                        fields,
                                                                                                        new int[] {}});

            PermutationCollection permutationList = generator.Generate(10);
            this.CheckPermutationCompletness(permutationList, fields, new int[] { });

            generator = new PermutatioGeneratorFieldGroup(this.dataStore);

            permutationList = generator.Generate(10);
            this.CheckPermutationCompletness(permutationList, fields, new int[] { });

            generator = new PermutatioGeneratorFieldGroup(new int[][] { new int[] { 6, 4 }, new int[] { 5, 3 }, new int[] { }, new int[] { 2 }, new int[] { 1 }, new int[] { } });

            permutationList = generator.Generate(10);
            this.CheckPermutationCompletness(permutationList, fields, new int[] { });
        }

        [Test]
        public void PermutationGeneratorEnsemble()
        {
            int[] attribute = new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            int[][] selectedAttributes = new int[][] {
                new int[] {1,2,3},
                new int[] {4,5,6},
                new int[] {1,2},
                new int[] {4,5,6}
            };

            PermutationGeneratorEnsemble permutationEnsemble = new PermutationGeneratorEnsemble(attribute, selectedAttributes);
            Permutation perm = permutationEnsemble.Generate(1)[0];
            //Console.WriteLine(perm);
        }
    }
}