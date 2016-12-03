using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Infovision.Core.Data;
using Infovision.MachineLearning.Classification;

namespace Infovision.MachineLearning.Tests.Classification
{
    [TestFixture]
    class ClassificationResultTest
    {        
        [Test]
        public void ReadResultsTest()
        {
            string fileName = @"d:\temp\1\mylogfile_CV_20161128064439.txt";
            DataTable dt = ClassificationResult.ReadResults(fileName, '|');
            var results = ClassificationResult.AggregateResults(dt, "acc");
            results.Dumb(@"d:\temp\1\result.txt", ";", true);
            //ClassificationResult.PlotR(results);
        }
    }
}
