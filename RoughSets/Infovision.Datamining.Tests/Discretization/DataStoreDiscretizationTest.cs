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
    public class DataStoreDiscretizationTest
    {
        [TestCase(@"Data\german.data", FileFormat.Csv, null)]
        public virtual void DiscretizeTest(string filename, FileFormat fileFormat, IEnumerable<int> fields)
        {
            DataStore data = DataStore.Load(filename, fileFormat);

            IEnumerable<int> numericFields = fields == null ? data.DataStoreInfo
                .GetFields(FieldTypes.Standard)
                .Where(f => f.CanDiscretize())
                .Select(g => g.Id) : fields;

            var discretizer = new DataStoreDiscretizer();            
            discretizer.Discretize(data);
            
            foreach (int fieldId in numericFields)
                Assert.IsNotNull(data.DataStoreInfo.GetFieldInfo(fieldId).Cuts, "Training file {0} Field {1}", filename, fieldId);
        }
    }
}
