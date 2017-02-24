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
