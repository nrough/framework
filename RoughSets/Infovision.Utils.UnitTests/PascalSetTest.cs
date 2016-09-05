using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Infovision.Utils.UnitTests
{
    [TestFixture]
    internal class PascalSetTest
    {
        [Test]
        public void GetCardinalityTest()
        {
            PascalSet<int> set1 = new PascalSet<int>(0, 10, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 });
            PascalSet<int> set2 = new PascalSet<int>(0, 10, new int[] { 5, 6, 7, 8, 9 });

            Assert.AreEqual(10, set1.GetCardinality());
            Assert.AreEqual(5, set2.GetCardinality());

            PascalSet<int> set3 = set1.Union(set2);
            Assert.AreEqual(10, set3.GetCardinality());

            set3 = set1 - 9;
            Assert.AreEqual(9, set3.GetCardinality());
            Assert.AreEqual(10, set1.GetCardinality());

            set3 = set1 - set2;
            Assert.AreEqual(5, set3.GetCardinality());

            set3 = set1.Intersection(set2);
            Assert.AreEqual(5, set3.GetCardinality());
        }

        [Test]
        public void HashSetTest()
        {
            Stopwatch s1 = new Stopwatch();
            s1.Start();            
            for (int i = 0; i < 10000; i++)
            {
                PascalSet<long> set1 = new PascalSet<long>(0, 10, new long[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 });
                PascalSet<long> set2 = new PascalSet<long>(0, 10, new long[] { 5, 6, 7, 8, 9 });

                //PascalSet<int> union = set1 + set2;
                //PascalSet<int> intersection = set1 * set2;
                PascalSet<long> difference = set1.Difference(set2);
                bool isSuperSet = set1.Superset(set2);
                bool isProperSuperSet = set1.ProperSubset(set2);
            }
            s1.Stop();
            Console.WriteLine("Pascal {0}", s1.ElapsedMilliseconds);


            Stopwatch s2 = new Stopwatch();
            s2.Start();
            for (int i = 0; i < 10000; i++)
            {
                HashSet<long> set3 = new HashSet<long>(new long[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 });
                HashSet<long> set4 = new HashSet<long>(new long[] { 5, 6, 7, 8, 9 });

                //set3.UnionWith(set4);
                //set4.IntersectWith(set3);
                //set3.ExceptWith(set4);

                bool isSuperSet = set3.IsSupersetOf(set3);
                bool isProperSuperSet = set3.IsProperSubsetOf(set3);
            }
            s2.Stop();
            Console.WriteLine("HashSet {0}", s2.ElapsedMilliseconds);

        }
    }
}