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
