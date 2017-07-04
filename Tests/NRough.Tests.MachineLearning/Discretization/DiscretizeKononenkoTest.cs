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
using NRough.Data;
using NRough.MachineLearning.Discretization;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NRough.Tests.MachineLearning.Discretization
{
    [TestFixture]
    public class DiscretizeKononenkoTest : DiscretizeSupervisedBaseTest
    {
        public override IDiscretizer GetDiscretizer()
        {
            return new DiscretizeKononenko();
        }

        [TestCase(@"Data\german.data", DataFormat.CSV, null)]
        public override void CreateDiscretizedDataTableTest(string filename, DataFormat fileFormat, IEnumerable<int> fields)
        {
            base.CreateDiscretizedDataTableTest(filename, fileFormat, fields);
        }

        [TestCase(@"Data\german.data", DataFormat.CSV, new int[] { 2, 5 })]
        public override void DiscretizeTest(string filename, DataFormat fileFormat, IEnumerable<int> fields)
        {
            base.DiscretizeTest(filename, fileFormat, fields);
        }
    }
}
