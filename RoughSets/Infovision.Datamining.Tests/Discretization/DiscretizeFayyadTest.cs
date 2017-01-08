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
        [TestCase(@"Data\sat.trn", @"Data\sat.tst", FileFormat.Rses1)]
        [TestCase(@"Data\pendigits.trn", @"Data\pendigits.tst", FileFormat.Rses1)]
        [TestCase(@"Data\optdigits.trn", @"Data\optdigits.tst", FileFormat.Rses1)]
        [TestCase(@"Data\letter.trn", @"Data\letter.tst", FileFormat.Rses1)]
        [TestCase(@"Data\vowel.trn", @"Data\vowel.tst", FileFormat.Csv)]
        public void DiscretizeData(string fileTrain, string fileTest, FileFormat fileFormat)
        {
            DataStore train = DataStore.Load(fileTrain, fileFormat);
            DataStore test = DataStore.Load(fileTest, fileFormat, train.DataStoreInfo);

            train.Dump(@"C:\" + fileTrain + ".int", " ", false);
            test.Dump(@"C:\" + fileTest + ".int", " ", false);

            train.DumpExt(@"C:\" + fileTrain + ".ext", " ", false, true);
            test.DumpExt(@"C:\" + fileTest + ".ext", " ", false, true);

            var discretizer = new DataStoreDiscretizer(new DiscretizeFayyad());

            discretizer.Fields2Discretize
                = train.DataStoreInfo.GetFieldIds(FieldTypes.Standard)
                .Where(fieldId => train.DataStoreInfo.GetFieldInfo(fieldId).IsNumeric);

            discretizer.Discretize(train, train.Weights);
            discretizer.Discretize(test, train);

            train.DumpExt(@"C:\" + fileTrain + ".disc", " ", false, true);
            test.DumpExt(@"C:\" + fileTest + ".disc", " ", false, true);
        }

        public void CreateDiscretizedDataTableTest()
        {

            DataStore data = DataStore.Load(@"Data\german.data", FileFormat.Csv);            
            int[] numericFields = data.DataStoreInfo.GetFields(FieldTypes.Standard)
                    .Where(i => i.IsNumeric)
                    .Select(f => f.Id)
                    .ToArray();

            DataStoreSplitter splitter = new DataStoreSplitter(data, 5);

            DataStore trainData = null, testData = null;
            splitter.Split(ref trainData, ref testData, 0);

            var discretizer = new DataStoreDiscretizer(new DiscretizeFayyad());
            discretizer.Fields2Discretize = numericFields;
            discretizer.Discretize(trainData, trainData.Weights);
            discretizer.Discretize(testData, trainData);

            foreach (int fieldId in numericFields)
            {
                if (trainData.DataStoreInfo.GetFieldInfo(fieldId).Cuts.Length == 0)
                    Console.WriteLine(String.Format("Field {0}", fieldId));
            }
        }

        [Test]
        public void ComputeTest()
        {
            IDiscretizationSupervised fayyad = new DiscretizeFayyad();
            fayyad.Compute(data, labels, null);

            for (int i = 0; i < data.Length; i++)
                Console.WriteLine("{0} {1}", data[i], fayyad.Apply(data[i]));

            for (int i = 0; i < dataNotExisting.Length; i++)
                Console.WriteLine("{0} {1}", dataNotExisting[i], fayyad.Apply(dataNotExisting[i]));

        }
    }
}
