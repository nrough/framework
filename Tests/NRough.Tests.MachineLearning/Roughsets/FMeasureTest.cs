using NRough.Core.CollectionExtensions;
using NRough.MachineLearning;
using NRough.MachineLearning.Roughsets;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NRough.Tests.MachineLearning.Roughsets
{
    [TestFixture]
    class FMeasureTest
    {
        [Test]
        public void MajorityMeasureTest()
        {
            var data = Data.Benchmark.Factory.Golf();
            int size = (int) System.Math.Pow(2, data.NumberOfAttributes);
            List<int[]> attributes = new List<int[]>(size);

            attributes.Add(new int[] { 1, 2, 3, 4 });

            attributes.Add(new int[] { 1, 2, 3 });
            attributes.Add(new int[] { 1, 2, 4 });
            attributes.Add(new int[] { 1, 3, 4 });
            attributes.Add(new int[] { 2, 3, 4 });

            attributes.Add(new int[] { 1, 2 });
            attributes.Add(new int[] { 1, 3 });
            attributes.Add(new int[] { 1, 4 });
            attributes.Add(new int[] { 2, 3 });
            attributes.Add(new int[] { 2, 4 });
            attributes.Add(new int[] { 3, 4 });

            attributes.Add(new int[] { 1 });
            attributes.Add(new int[] { 2 });
            attributes.Add(new int[] { 3 });
            attributes.Add(new int[] { 4 });

            attributes.Add(new int[] { });

            foreach (var attr in attributes)
            {
                var eqClasses = EquivalenceClassCollection.Create(attr, data);
                Console.WriteLine("{0} {1}", attr.ToStr(), FMeasures.Majority(eqClasses).ToString("0.00", CultureInfo.InvariantCulture));
            }
        }
    }
}
