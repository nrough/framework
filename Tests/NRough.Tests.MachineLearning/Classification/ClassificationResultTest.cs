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
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NRough.Core.Data;
using NRough.MachineLearning.Classification;

namespace NRough.Tests.MachineLearning.Classification
{
    [TestFixture]
    class ClassificationResultTest
    {        
        [Test]
        public void ReadResultsTest()
        {
            string fileName = @"d:\temp\1\mylogfile_CV_20161128064439.txt";
            DataTable dt = ClassificationResult.ReadResults(fileName, '|');

            if (dt.Columns.Contains("Column1"))
                dt.Columns.Remove("Column1");

            dt.Columns.Add("pruning", typeof(string));
            foreach (DataRow row in dt.Rows)
            {
                string model = row.Field<string>("model");
                if (model.Substring(model.Length - 4, 4) == "NONE")
                {
                    row["pruning"] = "NONE";
                    row["model"] = model.Substring(0, model.Length - 5);
                }
                else if (model.Substring(model.Length - 3, 3) == "REP")
                {
                    row["pruning"] = "REP";
                    row["model"] = model.Substring(0, model.Length - 4);
                }
            }

            var results = ClassificationResult.AggregateResults(dt, "acc");
            results.Dumb(@"d:\temp\1\result.txt", ";", true);
            //ClassificationResult.PlotR(results);
        }
    }
}
