using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Infovision.Data;
using Infovision.Datamining.Filters.Unsupervised.Attribute;
using NUnit.Framework;

namespace Infovision.Datamining.Tests.Filters.Unsupervised.Attribute
{
    [TestFixture]
    public class ReplaceMissingValuesTest
    {
        [Test]
        public void ComputeTest()
        {
            Console.WriteLine("ReplaceMissingValuesTest.ComputeTest()");

            DataStore trnData = DataStore.Load(@"Data\soybean-large.data", FileFormat.Csv);            
            DataStore tstData = DataStore.Load(@"Data\soybean-large.test", FileFormat.Csv, trnData.DataStoreInfo);
            trnData.SetDecisionFieldId(1);
            tstData.SetDecisionFieldId(1);

            trnData.WriteToCSVFileExt(@"f:\temp\missingvalsorig.trn", ",");
            tstData.WriteToCSVFileExt(@"f:\temp\missingvalsorig.tst", ",");

            DataStore trnCompleteData = new ReplaceMissingValues().Compute(trnData);
            DataStore tstCompleteDate = new ReplaceMissingValues().Compute(tstData, trnData);

            trnCompleteData.WriteToCSVFileExt(@"f:\temp\missingvals.trn", ",");
            tstCompleteDate.WriteToCSVFileExt(@"f:\temp\missingvals.tst", ",");




        }
    }
}
