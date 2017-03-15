using NRough.Data.Pivot;
using NRough.Data.Writers;
using NRough.MachineLearning.Classification;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NRough.Tests.Data.Pivot
{
    [TestFixture]
    public class PivotServiceTest
    {
        [Test]
        public void Test()
        {
            string filename = @"C:\Users\Admin\Source\Workspaces\RoughSets\RoughSets\Infovision.UnitTest.Runner\bin\x64\Release\mylogfile_20170313021600911.txt";
            DataTable dtc = ClassificationResult.ReadResults(filename, '|');
            dtc.Columns.Remove("ds");
            

            var pivot = new PivotService();
            var pivotTable = pivot.Pivot(
                dtc, 
                dtc.Columns["model"], 
                new DataColumn[] {
                    dtc.Columns["acc"],
                    dtc.Columns["attr"],
                    dtc.Columns["numrul"],
                    dtc.Columns["dthm"],
                    dtc.Columns["dtha"]
                }, "-");

            Console.WriteLine(new DataTableLatexTabularFormatter().Format("G", pivotTable, null));
        }
    }
}
