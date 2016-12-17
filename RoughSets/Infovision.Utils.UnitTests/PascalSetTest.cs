using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Infovision.Core.Tests
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
        public void PerofrmanceTest()
        {
            Stopwatch s1 = new Stopwatch();
            s1.Start();            
            for (int i = 0; i < 100000; i++)
            {
                PascalSet<long> set1 = new PascalSet<long>(0, 10, new long[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 });
                PascalSet<long> set2 = new PascalSet<long>(0, 10, new long[] { 5, 6, 7, 8, 9 });

                long[] attr1 = set1.ToArray();
                long[] attr2 = set2.ToArray();

                PascalSet<long> union = set1 + set2;
                PascalSet<long> intersection = set1.Intersection(set2);
                PascalSet<long> difference = set1.Difference(set2);
                bool isSuperSet = set1.Superset(set2);
                bool isProperSuperSet = set1.ProperSubset(set2);


            }
            s1.Stop();
            Console.WriteLine("Pascal {0}", s1.ElapsedMilliseconds);


            Stopwatch s2 = new Stopwatch();
            s2.Start();
            for (int i = 0; i < 100000; i++)
            {
                HashSet<long> set3 = new HashSet<long>(new long[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 });
                HashSet<long> set4 = new HashSet<long>(new long[] { 5, 6, 7, 8, 9 });

                long[] attr3 = set3.ToArray();
                long[] attr4 = set4.ToArray();

                HashSet<long> union = new HashSet<long>(set3.Union(set4));
                HashSet<long> intersection = new HashSet<long>(set3.Intersect(set4));
                HashSet<long> difference = new HashSet<long>(set3.Except(set4));
                
                //set3.UnionWith(set4);
                //set4.IntersectWith(set3);
                //set3.ExceptWith(set4);

                bool isSuperSet = set3.IsSupersetOf(set4);
                bool isProperSuperSet = set3.IsProperSubsetOf(set3);

                
            }
            s2.Stop();
            Console.WriteLine("HashSet {0}", s2.ElapsedMilliseconds);

        }
    }
}