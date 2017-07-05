using NRough.Data;
using NRough.MachineLearning.Discretization;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NRough.Tests.MachineLearning.Discretization
{
    [TestFixture]
    public class DiscretizeFayyadTest : DiscretizeSupervisedBaseTest
    {
        public override IDiscretizer GetDiscretizer()
        {
            return new DiscretizeFayyad();
        }

        [TestCase(@"Data\german.data", DataFormat.CSV, new int[] { 2, 5, 8, 11, 13, 16, 18 })]
        [TestCase(@"Data\german.data", DataFormat.CSV, new int[] { 2, 5, 8 })]
        [TestCase(@"Data\german.data", DataFormat.CSV, new int[] { 2, 5 })]
        public override void CreateDiscretizedDataTableTest(string filename, DataFormat fileFormat, IEnumerable<int> fields)
        {
            base.CreateDiscretizedDataTableTest(filename, fileFormat, fields);
        }

        [TestCase(@"Data\german.data", DataFormat.CSV, new int[] { 2, 5})]        
        public override void DiscretizeTest(string filename, DataFormat fileFormat, IEnumerable<int> fields)
        {            
            base.DiscretizeTest(filename, fileFormat, fields);
        }
    }
}
