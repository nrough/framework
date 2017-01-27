using Raccoon.Core;
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
    public class DiscretizeSupervisedBaseTest : DiscretizeBaseTest
    {        
        protected long[] labels = {
                        1, 1, 1, 2, 2, 2, 2, 2,
                        1, 1, 1, 1, 1, 1, 1, 1,
                        2, 2, 2, 2, 2, 3, 3, 3 };

        public override IDiscretizer GetDiscretizer()
        {
            DiscretizeSupervisedBase result = new DiscretizeSupervisedBase();
            result.NumberOfBuckets = 10;
            return result;
        }

        [TestCase(@"Data\sat.trn", @"Data\sat.tst", FileFormat.Rses1)]
        [TestCase(@"Data\pendigits.trn", @"Data\pendigits.tst", FileFormat.Rses1)]
        [TestCase(@"Data\optdigits.trn", @"Data\optdigits.tst", FileFormat.Rses1)]
        [TestCase(@"Data\letter.trn", @"Data\letter.tst", FileFormat.Rses1)]
        [TestCase(@"Data\vowel.trn", @"Data\vowel.tst", FileFormat.Csv)]
        public void DiscretizeData(string fileTrain, string fileTest, FileFormat fileFormat)
        {
            Console.WriteLine(fileTrain);

            DataStore train = DataStore.Load(fileTrain, fileFormat);
            DataStore test = DataStore.Load(fileTest, fileFormat, train.DataStoreInfo);

            train.Dump(@"C:\" + fileTrain + ".int", " ", false);
            test.Dump(@"C:\" + fileTest + ".int", " ", false);

            train.DumpExt(@"C:\" + fileTrain + ".ext", " ", false, true);
            test.DumpExt(@"C:\" + fileTest + ".ext", " ", false, true);

            var discretizer = new DataStoreDiscretizer(this.GetDiscretizer());

            discretizer.Fields2Discretize
                = train.DataStoreInfo.GetFieldIds(FieldGroup.Standard)
                .Where(fieldId => train.DataStoreInfo.GetFieldInfo(fieldId).IsNumeric);

            discretizer.Discretize(train, train.Weights);
            DataStoreDiscretizer.Discretize(test, train);

            train.DumpExt(@"C:\" + fileTrain + ".disc", " ", false, true);
            test.DumpExt(@"C:\" + fileTest + ".disc", " ", false, true);
        }

        [TestCase(@"Data\letter.trn", @"Data\letter.tst", FileFormat.Rses1)]
        public void LetterBug(string fileTrain, string fileTest, FileFormat fileFormat)
        {
            Console.WriteLine(fileTrain);

            DataStore train = DataStore.Load(fileTrain, fileFormat);
            DataStore test = DataStore.Load(fileTest, fileFormat, train.DataStoreInfo);

            var discretizer = new DataStoreDiscretizer(this.GetDiscretizer());

            discretizer.Fields2Discretize = new int[] { 2 };
            discretizer.Discretize(train, train.Weights);
        }

        [TestCase(@"Data\german.data", FileFormat.Csv, null)]        
        public virtual void DiscretizeTest(string filename, FileFormat fileFormat, IEnumerable<int> fields)
        {
            DataStore data = DataStore.Load(filename, fileFormat);                        
            IEnumerable<int> numericFields = fields == null ? data.DataStoreInfo
                .GetFields(FieldGroup.Standard)
                .Where(f => f.CanDiscretize())
                .Select(g => g.Id) : fields;

            var discretizer = new DataStoreDiscretizer(this.GetDiscretizer());
            discretizer.Fields2Discretize = numericFields;
            discretizer.Discretize(data, data.Weights);
            
            foreach (var kvp in discretizer.FieldDiscretizer)
                Console.WriteLine("Field {0} Cuts {1}", kvp.Key, (kvp.Value == null || kvp.Value.Cuts == null) ? "All" : kvp.Value.ToString());

            foreach (int fieldId in numericFields)
                Assert.IsNotNull(data.DataStoreInfo.GetFieldInfo(fieldId).Cuts, "Training file {0} Field {1}", filename, fieldId);
        }

        [TestCase(@"Data\german.data", FileFormat.Csv, null)]
        public virtual void CreateDiscretizedDataTableTest(string filename, FileFormat fileFormat, IEnumerable<int> fields)
        {
            DataStore data = DataStore.Load(filename, fileFormat);
            DataStoreSplitter splitter = new DataStoreSplitter(data, 5);            

            DataStore trainData = null, testData = null;
            splitter.Split(out trainData, out testData, 0);

            IEnumerable<int> numericFields = fields == null ? trainData.DataStoreInfo
                .GetFields(FieldGroup.Standard)
                .Where(f => f.CanDiscretize())
                .Select(g => g.Id) : fields;

            var discretizer = new DataStoreDiscretizer(this.GetDiscretizer());
            discretizer.Fields2Discretize = numericFields;
            discretizer.Discretize(trainData, trainData.Weights);
            DataStoreDiscretizer.Discretize(testData, trainData);

            foreach (var kvp in discretizer.FieldDiscretizer)
                Console.WriteLine("Field {0} Cuts {1}", kvp.Key, kvp.Value == null ? "All" : kvp.Value.ToString());
            
            foreach (int fieldId in numericFields)
            {
                Assert.IsNotNull(trainData.DataStoreInfo.GetFieldInfo(fieldId).Cuts, "Training file {0} Field {1}", filename, fieldId);
                Assert.IsNotNull(testData.DataStoreInfo.GetFieldInfo(fieldId).Cuts, "Test file {0} Field {1}", filename, fieldId);
            }            
        }

        [Test]
        public void ComputeTest()
        {
            IDiscretizerSupervised discretizer = this.GetDiscretizer() as IDiscretizerSupervised;
            if (discretizer == null)
                throw new InvalidOperationException();
            discretizer.Compute(data, labels, null);
            Assert.IsNotNull(discretizer.Cuts);
        }
    }
}
