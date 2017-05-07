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
