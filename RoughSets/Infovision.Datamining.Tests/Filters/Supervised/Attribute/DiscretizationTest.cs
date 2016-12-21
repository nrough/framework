using System;
using System.Linq;
using Infovision.Data;
using Infovision.MachineLearning.Filters.Supervised.Attribute;
using NUnit.Framework;

namespace Infovision.MachineLearning.Tests.Filters.Supervised.Attribute
{
    [TestFixture]
    public class DiscretizationTest
    {
        private int[] data = { 5, 5, 5, 5, 5, 7, 8, 9,
                        10, 12, 13, 14, 16, 40, 41, 42,
                        43, 44, 45, 45, 46, 47, 48, 49 };

        private int[] labels = { 1, 1, 1, 1, 1, 1, 1, 1,
                        1, 1, 1, 2, 2, 2, 2, 2,
                        2, 2, 2, 2, 2, 3, 3, 3 };

        private int[] dataNotExisting = { 0, 1, 2, 3, 4, 5, 6,
                                    11, 15, 16, 17, 20, 21, 22, 23, 24, 30, 31, 32, 33, 34,
                                    50, 60, 70, 80 };

        [Test]
        /*
        [TestCase(@"Data\sat.trn", @"Data\sat.tst", FileFormat.Rses1)]
        [TestCase(@"Data\pendigits.trn", @"Data\pendigits.tst", FileFormat.Rses1)]
        [TestCase(@"Data\optdigits.trn", @"Data\optdigits.tst", FileFormat.Rses1)]
        [TestCase(@"Data\letter.trn", @"Data\letter.tst", FileFormat.Rses1)]
        */
        [TestCase(@"Data\vowel.trn", @"Data\vowel.tst", FileFormat.Csv)]
        public void DiscretizeData(string fileTrain, string fileTest, FileFormat fileFormat)
        {
            DataStore train = DataStore.Load(fileTrain, fileFormat);
            DataStore test = DataStore.Load(fileTest, fileFormat, train.DataStoreInfo);

            train.Dump(@"C:\" + fileTrain + ".int", " ", false);
            test.Dump(@"C:\" + fileTest + ".int", " ", false);

            train.DumpExt(@"C:\" + fileTrain + ".ext", " ", false, true);
            test.DumpExt(@"C:\" + fileTest + ".ext", " ", false, true);

            var discretizer = new DataStoreDiscretizer()
            {
                UseBetterEncoding = false,
                UseKononenko = false, //use FayadAndIraniMDL
                Fields2Discretize = train.DataStoreInfo.GetFieldIds(FieldTypes.Standard)
                    .Where(fieldId => train.DataStoreInfo.GetFieldInfo(fieldId).IsNumeric)
            };

            discretizer.Discretize(train, train.Weights);
            discretizer.Discretize(test, train);

            train.DumpExt(@"C:\"+fileTrain + ".disc", " ", false, true);
            test.DumpExt(@"C:\" + fileTest + ".disc", " ", false, true);
        }

        //[TestCase(false, false)]
        //[TestCase(false, true)]
        [TestCase(true, false)]
        //[TestCase(true, true)]
        public void CreateDiscretizedDataTableTest(bool useBetterEncoding, bool useKononenko)
        {
            
            DataStore data = DataStore.Load(@"Data\german.data", FileFormat.Csv);

            //int[] numericFields = new int[] { 2, 5, 13, 8, 11, 16, 18 }; //8, 11, 16, 18
            //foreach (int fieldId in numericFields)
            //{
            //    data.DataStoreInfo.GetFieldInfo(fieldId).IsNumeric = true;
            //}

            int[] numericFields = data.DataStoreInfo.GetFields(FieldTypes.Standard)
                    .Where(i => i.IsNumeric)
                    .Select(f => f.Id)                    
                    .ToArray();

            DataStoreSplitter splitter = new DataStoreSplitter(data, 5);

            DataStore trainData = null, testData = null;
            splitter.Split(ref trainData, ref testData, 0);

            var discretizer = new Infovision.MachineLearning.Filters.Supervised.Attribute.DataStoreDiscretizer()
            {
                UseBetterEncoding = useBetterEncoding,
                UseKononenko = useKononenko,                
                Fields2Discretize = numericFields
            };

            discretizer.Discretize(trainData, trainData.Weights);
            discretizer.Discretize(testData, trainData);

            foreach (int fieldId in numericFields)
            {
                if (trainData.DataStoreInfo.GetFieldInfo(fieldId).Cuts.Length == 0)
                    Console.WriteLine(String.Format("Field {0}", fieldId));
            }
        }

        [TestCase(false, false)]
        [TestCase(false, true)]
        [TestCase(true, false)]
        [TestCase(true, true)]
        public void ComputeTest(bool useBetterEncoding, bool useKononenko)
        {
            Discretization<int, int> disc = new Discretization<int, int>()
            {
                UseBetterEncoding = useBetterEncoding,
                UseKononenko = useKononenko
            };

            disc.Compute(data, labels, false, null);            

            for (int i = 0; i < data.Length; i++)
                Console.WriteLine("{0} {1}", data[i], Discretization<int, int>.Search(data[i], disc.Cuts));

            for (int i = 0; i < data.Length; i++)
                Console.WriteLine("{0} {1}", dataNotExisting[i], Discretization<int, int>.Search(dataNotExisting[i], disc.Cuts));
        }        
    }
}