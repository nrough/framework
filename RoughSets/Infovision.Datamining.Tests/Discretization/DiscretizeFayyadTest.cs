using Infovision.Data;
using Infovision.MachineLearning.Discretization;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infovision.MachineLearning.Tests.Discretization
{
    [TestFixture]
    public class DiscretizeFayyadTest : DiscretizeSupervisedBaseTest
    {
        public override IDiscretizer GetDiscretizer()
        {
            return new DiscretizeFayyad();
        }

        [TestCase(@"Data\german.data", FileFormat.Csv, new int[] { 2, 5, 8, 11, 13, 16, 18 })]
        [TestCase(@"Data\german.data", FileFormat.Csv, new int[] { 2, 5, 8 })]
        public override void CreateDiscretizedDataTableTest(string filename, FileFormat fileFormat, IEnumerable<int> fields)
        {
            base.CreateDiscretizedDataTableTest(filename, fileFormat, fields);
        }

        [TestCase(@"Data\german.data", FileFormat.Csv, null)]
        public override void DiscretizeTest(string filename, FileFormat fileFormat, IEnumerable<int> fields)
        {
            base.DiscretizeTest(filename, fileFormat, fields);
        }
    }
}
