using System;
using System.Collections.Generic;
using Infovision.Data;
using Infovision.Utils;
using NUnit.Framework;

namespace Infovision.Datamining.Roughset.UnitTests
{
    [TestFixture]
    public class PermutationTest
    {
        DataStore dataStore = null;

        public PermutationTest()
        {
            dataStore = DataStore.Load(@"Data\monks-1.train", FileFormat.Rses1);
        
            Random randSeed = new Random();
            int seed = Guid.NewGuid().GetHashCode();
            RandomSingleton.Seed = seed;
        }

        [Test]
        public void FieldObjectPermutationRatioTest()
        {
            PermutationGeneratorFieldObject permGen = new PermutationGeneratorFieldObject(dataStore, 0.5M);
            PermutationCollection permList = permGen.Generate(100);
            this.CheckPermutationCompletness(permList, dataStore.DataStoreInfo.GetFieldIds(FieldTypes.Standard), dataStore.GetObjectIndexes());
        }

        [Test]
        public void RelativePermutationRatioTest()
        {
            PermutationGeneratorFieldObjectRelative permGen = new PermutationGeneratorFieldObjectRelative(dataStore, 0.5M);
            PermutationCollection permList = permGen.Generate(100);
            this.CheckPermutationCompletness(permList, dataStore.DataStoreInfo.GetFieldIds(FieldTypes.Standard), dataStore.GetObjectIndexes());
        }

        [Test]
        public void FieldObjectPermutationTest()
        {
            PermutationGeneratorFieldObject permGen = new PermutationGeneratorFieldObject(dataStore);
            PermutationCollection permList = permGen.Generate(100);
            this.CheckPermutationCompletness(permList, dataStore.DataStoreInfo.GetFieldIds(FieldTypes.Standard), dataStore.GetObjectIndexes());
        }

        [Test]
        public void RelativePermutationTest()
        {
            PermutationGeneratorFieldObjectRelative permGen = new PermutationGeneratorFieldObjectRelative(dataStore);
            PermutationCollection permList = permGen.Generate(100);
            this.CheckPermutationCompletness(permList, dataStore.DataStoreInfo.GetFieldIds(FieldTypes.Standard), dataStore.GetObjectIndexes());
        }

        [Test]
        public void FieldGroupPermutationTest()
        {
            PermutatioGeneratorFieldGroup permGen = new PermutatioGeneratorFieldGroup(dataStore);
            PermutationCollection permList = permGen.Generate(10);
            this.CheckPermutationCompletness(permList, dataStore.DataStoreInfo.GetFieldIds(FieldTypes.Standard), new int[] { });
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
            this.CalcAndPrintRatio(10, 10, 0.01M);
            this.CalcAndPrintRatio(10, 10, 0.2M);
            this.CalcAndPrintRatio(10, 10, 0.5M);
            this.CalcAndPrintRatio(10, 10, 0.7M);
            this.CalcAndPrintRatio(10, 10, 0.99M);
            this.CalcAndPrintRatio(10, 10, 1);
            //Console.WriteLine();

            this.CalcAndPrintRatio(10, 20, 0);
            this.CalcAndPrintRatio(10, 20, 0.01M);
            this.CalcAndPrintRatio(10, 20, 0.2M);
            this.CalcAndPrintRatio(10, 20, 0.5M);
            this.CalcAndPrintRatio(10, 20, 0.7M);
            this.CalcAndPrintRatio(10, 20, 0.99M);
            this.CalcAndPrintRatio(10, 20, 1);
            //Console.WriteLine();

            this.CalcAndPrintRatio(20, 10, 0);
            this.CalcAndPrintRatio(20, 10, 0.01M);
            this.CalcAndPrintRatio(20, 10, 0.2M);
            this.CalcAndPrintRatio(20, 10, 0.5M);
            this.CalcAndPrintRatio(20, 10, 0.7M);
            this.CalcAndPrintRatio(20, 10, 0.99M);
            this.CalcAndPrintRatio(20, 10, 1);
            //Console.WriteLine();

            this.CalcAndPrintRatio(10, 1000, 0);
            this.CalcAndPrintRatio(10, 1000, 0.01M);
            this.CalcAndPrintRatio(10, 1000, 0.2M);
            this.CalcAndPrintRatio(10, 1000, 0.5M);
            this.CalcAndPrintRatio(10, 1000, 0.7M);
            this.CalcAndPrintRatio(10, 1000, 0.99M);
            this.CalcAndPrintRatio(10, 1000, 1);
            //Console.WriteLine();
            
            this.CalcAndPrintRatio(1000, 10, 0);
            this.CalcAndPrintRatio(1000, 10, 0.01M);
            this.CalcAndPrintRatio(1000, 10, 0.2M);
            this.CalcAndPrintRatio(1000, 10, 0.5M);
            this.CalcAndPrintRatio(1000, 10, 0.7M);
            this.CalcAndPrintRatio(1000, 10, 0.99M);
            this.CalcAndPrintRatio(1000, 10, 1);
            //Console.WriteLine();

            this.CalcAndPrintRatio(10000, 10, 0);
            this.CalcAndPrintRatio(10000, 10, 0.01M);
            this.CalcAndPrintRatio(10000, 10, 0.2M);
            this.CalcAndPrintRatio(10000, 10, 0.5M);
            this.CalcAndPrintRatio(10000, 10, 0.7M);
            this.CalcAndPrintRatio(10000, 10, 0.99M);
            this.CalcAndPrintRatio(10000, 10, 1);
            //Console.WriteLine();

            Assert.AreEqual(true, true);                                
        }

        private decimal CalcAndPrintRatio(int numberOfFields, int numberOfObjects, decimal fieldSelectionRatio)
        {
            decimal result = PermutationGeneratorFieldObject.CalcSelectionRatio(numberOfFields, numberOfObjects, fieldSelectionRatio);
            //Console.WriteLine("{0} {1} {2} {3}", numberOfFields, numberOfObjects, fieldSelectionRatio, result);
            return result;
        }

        [Test]
        public void FieldImportance()
        {
            PermutationGeneratorFieldObject permGen;
            PermutationCollection permList;
            int[] fields = dataStore.DataStoreInfo.GetFieldIds(FieldTypes.Standard);
            int[] objects = dataStore.GetObjectIndexes();

            permGen = new PermutationGeneratorFieldObject(objects, fields, 0);
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

            permGen = new PermutationGeneratorFieldObject(objects, fields, 1);
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

            int[] fields = dataStore.DataStoreInfo.GetFieldIds(FieldTypes.Standard);

            PermutatioGeneratorFieldGroup generator = new PermutatioGeneratorFieldGroup(new int[][] { 
                                                                                                        new int[] {},
                                                                                                        fields,
                                                                                                        new int[] {}});
            
            PermutationCollection permutationList = generator.Generate(10);
            this.CheckPermutationCompletness(permutationList, fields, new int[] { });


            generator = new PermutatioGeneratorFieldGroup(this.dataStore);

            permutationList = generator.Generate(10);
            this.CheckPermutationCompletness(permutationList, fields, new int[] { });

            generator = new PermutatioGeneratorFieldGroup(new int[][] { new int[] { 6, 4 }, new int[] { 5, 3 }, new int[] { }, new int[] { 2 }, new int[] { 1 }, new int[]{} });

            permutationList = generator.Generate(10);
            this.CheckPermutationCompletness(permutationList, fields, new int[] { });

        }

        [Test]
        public void PermutationGeneratorEnsemble()
        {
            int[] attribute = new int[] {1,2,3,4,5,6,7,8,9};
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
