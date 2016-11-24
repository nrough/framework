using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Infovision.Utils;
using GenericParsing;

namespace Infovision.Datamining.Roughset.UnitTests
{
    [TestFixture]
    class DataTableClassificationResultsTest
    {
        [Test]
        public void ReadResultsGenericParserTest()
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
            var results = ClassificationResult.AggregateResults(ClassificationResult.ReadResults(fileName, '|'), "acc");
            results.WriteToCSVFile(@"mylogfile_CV_20161122212304b.txt", ";", true);
        }
    }
}
