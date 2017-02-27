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
    public class DataStoreDiscretizationTest
    {
        [TestCase(@"Data\german.data", DataFormat.CSV, new int[] { 2 })]
        public void DiscretizeTest(string filename, DataFormat fileFormat, IEnumerable<int> fields)
        {
            DataStore data = DataStore.Load(filename, fileFormat);

            IEnumerable<int> numericFields = fields == null ? data.DataStoreInfo
                .SelectAttributeIds(a => a.IsStandard && a.CanDiscretize()) : fields;

            var discretizer = new TableDiscretizer();            
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

        [TestCase(@"Data\german.data", DataFormat.CSV, new int[] { 2 })]
        public void AddNewColumnsTest(string filename, DataFormat fileFormat, IEnumerable<int> fields)
        {
            DataStore data = DataStore.Load(filename, fileFormat);
            int numberOfFields = data.DataStoreInfo.NumberOfFields;

            IEnumerable<int> numericFields = fields == null ? data.DataStoreInfo
                .SelectAttributeIds(a => a.IsStandard && a.CanDiscretize()): fields;

            var discretizer =
                new TableDiscretizer(
                    new DiscretizeSupervisedBase()
                    {
                        NumberOfBuckets = 5
                    })
                {
                    RemoveColumnAfterDiscretization = true,
                    UpdateDataColumns = false,
                    AddColumnsBasedOnCuts = true,
                    UseBinaryCuts = true
                };

            discretizer.FieldsToDiscretize = numericFields;
            discretizer.AddColumnsBasedOnCuts = true;
            discretizer.Discretize(data);            
        }

        [TestCase(@"Data\german.data", DataFormat.CSV, new int[] { 2 })]
        public void CreateDiscretizedDataTableTest(string filename, DataFormat fileFormat, int[] fields)
        {
            DataStore data = DataStore.Load(filename, fileFormat);
            DataSplitter splitter = new DataSplitter(data, 5);

            DataStore trainData = null, testData = null;
            splitter.Split(out trainData, out testData, 0);

            int numberOfFields = trainData.DataStoreInfo.NumberOfFields;
            IEnumerable<int> numericFields = fields == null ? trainData.DataStoreInfo
                .SelectAttributeIds(a => a.IsStandard && a.CanDiscretize()) : fields;

            var discretizer = new TableDiscretizer();
            discretizer.FieldsToDiscretize = numericFields;
            discretizer.AddColumnsBasedOnCuts = true;
            discretizer.Discretize(trainData);
            
            TableDiscretizer.Discretize(testData, trainData);

            Assert.AreEqual(numberOfFields + 1, trainData.DataStoreInfo.NumberOfFields);
            Assert.AreEqual(numberOfFields + 1, testData.DataStoreInfo.NumberOfFields);
        }
    }
}
