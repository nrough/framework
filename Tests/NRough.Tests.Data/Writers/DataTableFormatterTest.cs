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
using NRough.Data.Writers;
using NRough.MachineLearning.Classification;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NRough.Tests.Data.Writers
{
    [TestFixture]
    class DataTableFormatterTest
    {
        [Test]
        public void ConstructorTest()
        {
            string filename = @"C:\Users\Admin\Source\Workspaces\RoughSets\RoughSets\Infovision.UnitTest.Runner\bin\x64\Release\mylogfile_20170313021600911.txt";
            DataTable dtc = ClassificationResult.ReadResults(filename, '|');
            dtc.Columns.Remove("ds");
            Console.WriteLine(new DataTableLatexTabularFormatter().Format("G", dtc, null));
        }
    }
}
