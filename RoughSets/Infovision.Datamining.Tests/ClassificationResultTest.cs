using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Infovision.Utils;
using GenericParsing;
using Infovision.Utils.Data;

namespace Infovision.Datamining.Tests
{
    [TestFixture]
    class ClassificationResultTest
    {
        [Test]
        public void ReadResultsFromFileTest()
        {
            string fileName = @"mylogfile_CV_20161122212304.txt";
            DataTable dt;
            using (GenericParserAdapter gpa = new GenericParserAdapter(fileName))
            {
                gpa.ColumnDelimiter = '|';
                gpa.FirstRowHasHeader = true;
                gpa.IncludeFileLineNumber = false;
                gpa.FirstRowSetsExpectedColumnCount = true;
                gpa.TrimResults = true;

                dt = gpa.GetDataTable();
            }
        }


        [Test]
        public void ReadResultsTest()
        {
            string fileName = @"mylogfile_CV_20161122212304.txt";
            DataTable dt = ClassificationResult.ReadResults(fileName, '|');
            var results = ClassificationResult.AggregateResults(dt, "acc");
            results.WriteToCSVFile(@"mylogfile_CV_20161122212304b.txt", ";", true);
        }
    }
}
