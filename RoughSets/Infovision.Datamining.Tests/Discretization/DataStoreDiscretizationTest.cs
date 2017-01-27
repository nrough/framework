using Raccoon.Data;
using Raccoon.MachineLearning.Discretization;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Raccoon.MachineLearning.Tests.Discretization
{
    [TestFixture]
    public class DataStoreDiscretizationTest
    {
        [TestCase(@"Data\german.data", FileFormat.Csv, new int[] { 2 })]
        public void DiscretizeTest(string filename, FileFormat fileFormat, IEnumerable<int> fields)
        {
            DataStore data = DataStore.Load(filename, fileFormat);

            IEnumerable<int> numericFields = fields == null ? data.DataStoreInfo
                .GetFields(FieldGroup.Standard)
                .Where(f => f.CanDiscretize())
                .Select(g => g.Id) : fields;

            var discretizer = new DataStoreDiscretizer();            
            discretizer.Discretize(data);
            
            foreach (int fieldId in numericFields)
                Assert.IsNotNull(data.DataStoreInfo.GetFieldInfo(fieldId).Cuts, "Training file {0} Field {1}", filename, fieldId);

            var discretizers = discretizer.FieldDiscretizer;
            foreach (int fieldId in numericFields)
            {
                Console.WriteLine("Field {0}", fieldId);
                Console.WriteLine(discretizers[fieldId]);
            }
        }

        [TestCase(@"Data\german.data", FileFormat.Csv, new int[] { 2 })]
        public void AddNewColumnsTest(string filename, FileFormat fileFormat, IEnumerable<int> fields)
        {
            DataStore data = DataStore.Load(filename, fileFormat);
            int numberOfFields = data.DataStoreInfo.NumberOfFields;

            IEnumerable<int> numericFields = fields == null ? data.DataStoreInfo
                .GetFields(FieldGroup.Standard)
                .Where(f => f.CanDiscretize())
                .Select(g => g.Id) : fields;

            var discretizer = new DataStoreDiscretizer();
            discretizer.Fields2Discretize = numericFields;
            discretizer.AddColumnsBasedOnCuts = true;
            discretizer.Discretize(data);

            Assert.AreEqual(numberOfFields + 1, data.DataStoreInfo.NumberOfFields);
        }

        [TestCase(@"Data\german.data", FileFormat.Csv, new int[] { 2 })]
        public void CreateDiscretizedDataTableTest(string filename, FileFormat fileFormat, int[] fields)
        {
            DataStore data = DataStore.Load(filename, fileFormat);
            DataStoreSplitter splitter = new DataStoreSplitter(data, 5);

            DataStore trainData = null, testData = null;
            splitter.Split(out trainData, out testData, 0);

            int numberOfFields = trainData.DataStoreInfo.NumberOfFields;
            IEnumerable<int> numericFields = fields == null ? trainData.DataStoreInfo
                .GetFields(FieldGroup.Standard)
                .Where(f => f.CanDiscretize())
                .Select(g => g.Id) : fields;

            var discretizer = new DataStoreDiscretizer();
            discretizer.Fields2Discretize = numericFields;
            discretizer.AddColumnsBasedOnCuts = true;
            discretizer.Discretize(trainData);
            
            DataStoreDiscretizer.Discretize(testData, trainData);

            Assert.AreEqual(numberOfFields + 1, trainData.DataStoreInfo.NumberOfFields);
            Assert.AreEqual(numberOfFields + 1, testData.DataStoreInfo.NumberOfFields);
        }
    }
}
