﻿// 
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

using NRough.Core;
using NRough.Core.BaseTypeExtensions;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace NRough.Tests.Core
{
    [TestFixture]
    class StringExtensionsTest
    {
        [TestCase("1234,12345,12", 0)]
        [TestCase("1234,12345", 0)]
        [TestCase("1234.12345", 5)]
        [TestCase("1234.0", 1)]
        [TestCase("1234", 0)]
        [TestCase("ABC", 0)]
        [TestCase(".1234", 4)]
        [TestCase("", 0)]
        public void GetNumberOfDigits(string num, int dec)
        {
            Assert.AreEqual(dec, num.GetNumberOfDecimals(), num);
        }
    }
}
