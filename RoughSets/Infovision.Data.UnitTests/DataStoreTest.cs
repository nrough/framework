using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace Raccoon.Data.Tests
{
    [TestFixture]
    internal class DataStoreTest
    {                
        [Test]
        public void AddColumnTest()
        {
            var data = DataStore.Load(@"data\german.data", FileFormat.Csv);
            Assert.NotNull(data);
            int numOfCols = data.DataStoreInfo.NumberOfFields;
            int[] newColumn = Enumerable.Range(0, data.NumberOfRecords).ToArray();
            int newFieldId = data.AddColumn<int>(newColumn);
            
            Assert.AreEqual(numOfCols + 1, data.DataStoreInfo.NumberOfFields);
        }

        [Test]
        public void AddAndRemoveColumnTest()
        {
            var data = DataStore.Load(@"data\german.data", FileFormat.Csv);                        
            var data2 = (DataStore) data.Clone();

            int[] newColumn = Enumerable.Range(0, data2.NumberOfRecords).ToArray();
            data2.AddColumn<int>(newColumn);
            data2.AddColumn<int>(newColumn);
            data2.AddColumn<int>(newColumn);

            data2.RemoveColumn(2);

            Assert.AreEqual(data.DataStoreInfo.NumberOfFields + 3 - 1, data.DataStoreInfo.NumberOfFields);

            for (int i = 0; i < data.NumberOfRecords; i++)
            {
                AttributeValueVector vector = data2.GetDataVector(i);
                for (int j = 0; j < vector.Length; j++)
                    Assert.AreEqual(data.GetFieldValue(i, vector.Attributes[j]), vector.Values[j]);
            }
        }

        [Test]
        public void CloneTest()
        {
            var data = DataStore.Load(@"data\german.data", FileFormat.Csv);
            var data2 = (DataStore)data.Clone();

            Assert.IsNotNull(data2);
            Assert.AreNotSame(data, data2);
            Assert.AreEqual(data.Name, data2.Name);
            Assert.AreEqual(data.Fold, data2.Fold);
            Assert.AreEqual(data.TableId, data2.TableId);
            Assert.AreEqual(data.NumberOfRecords, data2.NumberOfRecords);

            for (int i = 0; i < data.NumberOfRecords; i++)
            {
                AttributeValueVector vector = data2.GetDataVector(i);
                for (int j = 0; j < vector.Length; j++)
                    Assert.AreEqual(data.GetFieldValue(i, vector.Attributes[j]), vector.Values[j]);
            }

        }

        [Test]
        public void RemoveColumnTest()
        {
            var data = DataStore.Load(@"data\german.data", FileFormat.Csv);            
            var data2 = (DataStore) data.Clone();

            data2.RemoveColumn(1);
            data2.RemoveColumn(new int[] { 1 });
            data2.RemoveColumn(new int[] { 2, 3, 4, 7, 11, 13 } );

            for (int i = 0; i < data2.NumberOfRecords; i++)
            {
                AttributeValueVector vector = data2.GetDataVector(i);
                for (int j = 0; j < vector.Length; j++)
                    Assert.AreEqual(data.GetFieldValue(i, vector.Attributes[j]), vector.Values[j]);
                Assert.AreEqual(data.GetDecisionValue(i), data2.GetDecisionValue(i));
            }
        }

        [Test]
        public void NumericAttributeTest()
        {
            var data = DataStore.Load(@"data\german.data", FileFormat.Csv);
            Assert.NotNull(data);

            Assert.IsTrue(data.DataStoreInfo.GetFieldInfo(2).IsNumeric, "Field 2 is not numeric");
            Assert.IsTrue(data.DataStoreInfo.GetFieldInfo(5).IsNumeric, "Field 5 is not numeric");
            Assert.IsTrue(data.DataStoreInfo.GetFieldInfo(8).IsNumeric, "Field 8 is not numeric");
            Assert.IsTrue(data.DataStoreInfo.GetFieldInfo(11).IsNumeric, "Field 11 is not numeric");
            Assert.IsTrue(data.DataStoreInfo.GetFieldInfo(13).IsNumeric, "Field 13 is not numeric");
            Assert.IsTrue(data.DataStoreInfo.GetFieldInfo(16).IsNumeric, "Field 16 is not numeric");
            Assert.IsTrue(data.DataStoreInfo.GetFieldInfo(18).IsNumeric, "Field 18 is not numeric");
            Assert.IsTrue(data.DataStoreInfo.GetFieldInfo(21).IsNumeric, "Field 21 is not numeric");

            Assert.AreEqual(data.DataStoreInfo.GetFieldInfo(2).External2Internal(48), 48);
            Assert.AreEqual(data.DataStoreInfo.GetFieldInfo(2).Internal2External(48), 48);

            Assert.AreEqual(data.DataStoreInfo.GetFieldInfo(2).External2Internal(0), 0);
            Assert.AreEqual(data.DataStoreInfo.GetFieldInfo(2).Internal2External(0), 0);

            Assert.AreEqual(data.DataStoreInfo.GetFieldInfo(2).External2Internal(1), 1);
            Assert.AreEqual(data.DataStoreInfo.GetFieldInfo(2).Internal2External(1), 1);

            Assert.AreEqual(data.DataStoreInfo.GetFieldInfo(2).External2Internal(-1), -1);
            Assert.AreEqual(data.DataStoreInfo.GetFieldInfo(2).Internal2External(-1), -1);

            Assert.AreEqual(data.DataStoreInfo.GetFieldInfo(5).External2Internal(5951), 5951);
            Assert.AreEqual(data.DataStoreInfo.GetFieldInfo(5).Internal2External(5951), 5951);

            Assert.AreEqual(data.DataStoreInfo.GetFieldInfo(5).External2Internal(-5951), -5951);
            Assert.AreEqual(data.DataStoreInfo.GetFieldInfo(5).Internal2External(-5951), -5951);
        }

        [Test]
        public void TestNumbercInternalValueEncoding()
        {
            var data = DataStore.Load(@"data\german.data", FileFormat.Csv);
            Assert.NotNull(data);
            Assert.AreEqual(data.GetFieldIndexValue(1, 1), 48);
        }

        [Test]
        public void ToDoubleArrayTest()
        {
            var data = DataStore.Load(@"data\german.data", FileFormat.Csv);
            Assert.NotNull(data);

            double[][] rawData = data.ToArray<double>();

            Assert.IsNotNull(rawData);
            Assert.AreEqual(data.NumberOfRecords, rawData.Length);
            Assert.AreEqual(data.DataStoreInfo.NumberOfFields, rawData[0].Length);

            for (int i = 0; i < rawData.Length; i++)
            {
                for (int j = 0; j < rawData[i].Length; j++)
                    Console.Write("{0} ", rawData[i][j]);
                Console.Write(Environment.NewLine);
            }
        }

        [Test]
        public void ToLongArrayTest()
        {
            var data = DataStore.Load(@"data\german.data", FileFormat.Csv);
            Assert.NotNull(data);

            long[][] rawData = data.ToArray<long>();

            Assert.IsNotNull(rawData);
            Assert.AreEqual(data.NumberOfRecords, rawData.Length);
            Assert.AreEqual(data.DataStoreInfo.NumberOfFields, rawData[0].Length);

            for (int i = 0; i < rawData.Length; i++)
            {
                for (int j = 0; j < rawData[i].Length; j++)
                    Console.Write("{0} ", rawData[i][j]);
                Console.Write(Environment.NewLine);
            }
        }

        [Test]
        public void ToIntArrayTest()
        {
            var data = DataStore.Load(@"data\german.data", FileFormat.Csv);
            Assert.NotNull(data);

            int[][] rawData = data.ToArray<int>();

            Assert.IsNotNull(rawData);
            Assert.AreEqual(data.NumberOfRecords, rawData.Length);
            Assert.AreEqual(data.DataStoreInfo.NumberOfFields, rawData[0].Length);

            for (int i = 0; i < rawData.Length; i++)
            {
                for (int j = 0; j < rawData[i].Length; j++)
                    Console.Write("{0} ", rawData[i][j]);
                Console.Write(Environment.NewLine);
            }
        }

        [Test]
        public void TestData()
        {
            string trainFileName = @"Data\monks-1.train";
            string testFileName = @"Data\monks-1.test";

            DataStore dataStoreTrain = DataStore.Load(trainFileName, FileFormat.Rses1);
            DataStore dataStoreTest = DataStore.Load(testFileName, FileFormat.Rses1, dataStoreTrain.DataStoreInfo);

            DataStoreInfo dataStoreTrainInfo = dataStoreTrain.DataStoreInfo;
            DataStoreInfo dataStoreTestInfo = dataStoreTest.DataStoreInfo;

            Assert.AreEqual(7, dataStoreTrainInfo.DecisionFieldId);
            Assert.AreEqual(7, dataStoreTestInfo.DecisionFieldId);

            Assert.AreEqual(432, dataStoreTestInfo.NumberOfRecords);
            Assert.AreEqual(124, dataStoreTrainInfo.NumberOfRecords);

            Assert.AreEqual(7, dataStoreTrainInfo.NumberOfFields);
            Assert.AreEqual(7, dataStoreTestInfo.NumberOfFields);

            Assert.AreEqual(3, dataStoreTrainInfo.GetFieldInfo(1).NumberOfValues);
            Assert.AreEqual(3, dataStoreTrainInfo.GetFieldInfo(2).NumberOfValues);
            Assert.AreEqual(2, dataStoreTrainInfo.GetFieldInfo(3).NumberOfValues);
            Assert.AreEqual(3, dataStoreTrainInfo.GetFieldInfo(4).NumberOfValues);
            Assert.AreEqual(4, dataStoreTrainInfo.GetFieldInfo(5).NumberOfValues);
            Assert.AreEqual(2, dataStoreTrainInfo.GetFieldInfo(6).NumberOfValues);
            Assert.AreEqual(2, dataStoreTrainInfo.GetFieldInfo(7).NumberOfValues);

            Assert.AreEqual(3, dataStoreTestInfo.GetFieldInfo(1).NumberOfValues);
            Assert.AreEqual(3, dataStoreTestInfo.GetFieldInfo(2).NumberOfValues);
            Assert.AreEqual(2, dataStoreTestInfo.GetFieldInfo(3).NumberOfValues);
            Assert.AreEqual(3, dataStoreTestInfo.GetFieldInfo(4).NumberOfValues);
            Assert.AreEqual(4, dataStoreTestInfo.GetFieldInfo(5).NumberOfValues);
            Assert.AreEqual(2, dataStoreTestInfo.GetFieldInfo(6).NumberOfValues);
            Assert.AreEqual(2, dataStoreTestInfo.GetFieldInfo(7).NumberOfValues);
        }

        [Test]
        public void ExternalFieldEncoding()
        {
            string trainFileName = @"Data\monks-1.train";
            string testFileName = @"Data\monks-1.test";

            DataStore dataStoreTrain = DataStore.Load(trainFileName, FileFormat.Rses1);
            DataStore dataStoreTest = DataStore.Load(testFileName, FileFormat.Rses1, dataStoreTrain.DataStoreInfo);

            DataStoreInfo dataStoreTrainInfo = dataStoreTrain.DataStoreInfo;
            DataStoreInfo dataStoreTestInfo = dataStoreTest.DataStoreInfo;

            //Item1 newInstance
            //1 1 1 1 3 1 1

            long internalValue = dataStoreTrain.GetFieldValue(0, 1);
            Object externalValue = dataStoreTrainInfo.GetFieldInfo(1).Internal2External(internalValue);
            Assert.AreEqual(1, (int)externalValue);
            Assert.AreEqual(1, (int)internalValue); //added to test numeric attributes

            internalValue = dataStoreTrain.GetFieldValue(0, 5);
            externalValue = dataStoreTrainInfo.GetFieldInfo(5).Internal2External(internalValue);
            Assert.AreEqual(3, (int)externalValue);
            Assert.AreEqual(3, (int)internalValue); //added to test numeric attributes

            internalValue = dataStoreTrain.GetFieldValue(0, 7);
            externalValue = dataStoreTrainInfo.GetFieldInfo(7).Internal2External(internalValue);
            Assert.AreEqual(1, (int)externalValue);
            Assert.AreEqual(1, (int)internalValue); //added to test numeric attributes

            //Last newInstance
            //3 3 2 3 4 2 1

            internalValue = dataStoreTrain.GetFieldValue(dataStoreTrainInfo.NumberOfRecords - 1, 1);
            externalValue = dataStoreTrainInfo.GetFieldInfo(1).Internal2External(internalValue);
            Assert.AreEqual(3, (int)externalValue);
            Assert.AreEqual(3, (int)internalValue); //added to test numeric attributes

            internalValue = dataStoreTrain.GetFieldValue(dataStoreTrainInfo.NumberOfRecords - 1, 5);
            externalValue = dataStoreTrainInfo.GetFieldInfo(5).Internal2External(internalValue);
            Assert.AreEqual(4, (int)externalValue);
            Assert.AreEqual(4, (int)internalValue); //added to test numeric attributes

            internalValue = dataStoreTrain.GetFieldValue(dataStoreTrainInfo.NumberOfRecords - 1, 7);
            externalValue = dataStoreTrainInfo.GetFieldInfo(7).Internal2External(internalValue);
            Assert.AreEqual(1, (int)externalValue);
            Assert.AreEqual(1, (int)internalValue); //added to test numeric attributes

            //60th newInstance
            //2 1 2 3 4 1 0

            internalValue = dataStoreTrain.GetFieldValue(60, 1);
            externalValue = dataStoreTrainInfo.GetFieldInfo(1).Internal2External(internalValue);
            Assert.AreEqual(2, (int)externalValue);
            Assert.AreEqual(2, (int)internalValue); //added to test numeric attributes

            internalValue = dataStoreTrain.GetFieldValue(60, 5);
            externalValue = dataStoreTrainInfo.GetFieldInfo(5).Internal2External(internalValue);
            Assert.AreEqual(4, (int)externalValue);
            Assert.AreEqual(4, (int)internalValue); //added to test numeric attributes

            internalValue = dataStoreTrain.GetFieldValue(60, 7);
            externalValue = dataStoreTrainInfo.GetFieldInfo(7).Internal2External(internalValue);
            Assert.AreEqual(0, (int)externalValue);
            Assert.AreEqual(0, (int)internalValue); //added to test numeric attributes
        }

        [Test]
        public void TrainDataFieldEncoding()
        {
            string trainFileName = @"Data\monks-1.train";
            string testFileName = @"Data\monks-1.test";

            DataStore dataStoreTrain = DataStore.Load(trainFileName, FileFormat.Rses1);
            DataStore dataStoreTest = DataStore.Load(testFileName, FileFormat.Rses1, dataStoreTrain.DataStoreInfo);

            DataStoreInfo dataStoreTrainInfo = dataStoreTrain.DataStoreInfo;
            DataStoreInfo dataStoreTestInfo = dataStoreTest.DataStoreInfo;

            for (int i = 0; i < dataStoreTrainInfo.NumberOfRecords; i++)
            {
                DataRecordInternal dataRecord = dataStoreTrain.GetRecordByIndex(i, false);

                foreach (int fieldId in dataRecord.GetFields())
                {
                    long internalValue = dataStoreTrain.GetFieldValue(i, fieldId);
                    Object externalValue = dataStoreTrainInfo.GetFieldInfo(fieldId).Internal2External(internalValue);
                    Object externalValueFromRecord = dataStoreTrainInfo.GetFieldInfo(fieldId).Internal2External(dataRecord[fieldId]);
                    Assert.AreEqual((int)externalValueFromRecord, (int)externalValue);
                }
            }
        }

        [Test]
        public void TestDataFieldEncoding()
        {
            string trainFileName = @"Data\monks-1.train";
            string testFileName = @"Data\monks-1.test";

            DataStore dataStoreTrain = DataStore.Load(trainFileName, FileFormat.Rses1);
            DataStore dataStoreTest = DataStore.Load(testFileName, FileFormat.Rses1, dataStoreTrain.DataStoreInfo);

            DataStoreInfo dataStoreTrainInfo = dataStoreTrain.DataStoreInfo;
            DataStoreInfo dataStoreTestInfo = dataStoreTest.DataStoreInfo;

            for (int i = 0; i < dataStoreTestInfo.NumberOfRecords; i++)
            {
                DataRecordInternal dataRecord = dataStoreTest.GetRecordByIndex(i, false);

                foreach (int fieldId in dataRecord.GetFields())
                {
                    long internalValue = dataStoreTest.GetFieldValue(i, fieldId);
                    Object externalValue = dataStoreTestInfo.GetFieldInfo(fieldId).Internal2External(internalValue);
                    Object externalValueFromRecord = dataStoreTestInfo.GetFieldInfo(fieldId).Internal2External(dataRecord[fieldId]);
                    Assert.AreEqual((int)externalValueFromRecord, (int)externalValue);

                    Object testExternalValue = dataStoreTestInfo.GetFieldInfo(fieldId).Internal2External(internalValue);
                    Assert.AreEqual((int)testExternalValue, (int)externalValue);
                }
            }
        }

        [Test]
        public void DataRecordInternal()
        {
            string trainFileName = @"Data\monks-1.train";
            string testFileName = @"Data\monks-1.test";

            DataStore dataStoreTrain = DataStore.Load(trainFileName, FileFormat.Rses1);
            DataStore dataStoreTest = DataStore.Load(testFileName, FileFormat.Rses1, dataStoreTrain.DataStoreInfo);

            DataStoreInfo dataStoreTrainInfo = dataStoreTrain.DataStoreInfo;
            DataStoreInfo dataStoreTestInfo = dataStoreTest.DataStoreInfo;

            Dictionary<DataRecordInternal, int> dict = new Dictionary<DataRecordInternal, int>(dataStoreTestInfo.NumberOfRecords);
            int count = 0;

            for (int i = 0; i < dataStoreTestInfo.NumberOfRecords; i++)
            {
                DataRecordInternal dataRecord1 = dataStoreTest.GetRecordByIndex(i);
                DataRecordInternal dataRecord2 = dataStoreTest.GetRecordByIndex(i);

                Assert.AreEqual(dataRecord1, dataRecord2);
                Assert.AreNotSame(dataRecord1, dataRecord2);

                dict[dataRecord1] = dict.TryGetValue(dataRecord1, out count) ? ++count : 1;
            }

            foreach (KeyValuePair<DataRecordInternal, int> kvp in dict)
            {
                Assert.AreEqual(1, kvp.Value);
                DataRecordInternal record = kvp.Key;

                foreach (int fieldId in record.GetFields())
                {
                    Assert.AreEqual(dataStoreTest.GetFieldValue((int)record.ObjectId - 1, fieldId), record[fieldId]);
                }
            }
        }

        [Test]
        public void DataStoreInfoTest()
        {
            string trainFileName = @"Data\monks-1.train";
            string testFileName = @"Data\monks-1.test";

            DataStore dataStoreTrain = DataStore.Load(trainFileName, FileFormat.Rses1);
            DataStore dataStoreTest = DataStore.Load(testFileName, FileFormat.Rses1, dataStoreTrain.DataStoreInfo);

            DataStoreInfo dataStoreTrainInfo = dataStoreTrain.DataStoreInfo;
            DataStoreInfo dataStoreTestInfo = dataStoreTest.DataStoreInfo;

            foreach (int fieldId in dataStoreTestInfo.GetFieldIds(FieldGroup.All))
            {
                foreach (long internalValue in dataStoreTestInfo.GetFieldInfo(fieldId).InternalValues())
                {
                    Object externalValue = dataStoreTestInfo.GetFieldInfo(fieldId).Internal2External(internalValue);
                    long trainInternalValue = dataStoreTrainInfo.GetFieldInfo(fieldId).External2Internal(externalValue);
                    Assert.AreEqual(internalValue, trainInternalValue);
                }
            }

            foreach (int fieldId in dataStoreTrainInfo.GetFieldIds(FieldGroup.All))
            {
                foreach (long internalValue in dataStoreTrainInfo.GetFieldInfo(fieldId).InternalValues())
                {
                    Object externalValue = dataStoreTrainInfo.GetFieldInfo(fieldId).Internal2External(internalValue);
                    long testInternalValue = dataStoreTestInfo.GetFieldInfo(fieldId).External2Internal(externalValue);
                    Assert.AreEqual(internalValue, testInternalValue);
                }
            }
        }

        [Test]
        public void FieldTypeFlag()
        {
            FieldGroup fieldTypeFlags = FieldGroup.All;
            Assert.IsTrue(fieldTypeFlags.HasFlag(FieldGroup.All));
        }

        [Test]
        public void OrderByTest()
        {
            DataStore data = DataStore.Load(@"Data\dna_modified.trn", FileFormat.Rses1);

            int[][] orderBy = new int[][]
            {
                new int[] { 1, 3, 5 },
                new int[] { 2, 1},
                new int[] { 5, 3, 2},
                new int[] { 19, 17, 18, 15, 14, 13, 12, 11, 7, 6, 4, 3, 2, 1},
                new int[] { 19, 11, 15, 14, 10, 12, 7, 6, 5, 3, 2, 1}
            };

            for (int i = 0; i < orderBy.Length; i++)
            {
                DataStoreOrderByComparer comparer = new DataStoreOrderByComparer(data, orderBy[i]);
                int[] sortedArray = data.Sort(comparer);

                for (int j = 1; j < data.NumberOfRecords; j++)
                {
                    AttributeValueVector record = data.GetDataVector(sortedArray[j - 1], orderBy[i]);

                    int result = comparer.Compare(sortedArray[j - 1], sortedArray[j]);
                    Assert.AreNotEqual(1, result);

                    //Console.WriteLine(record);
                }
            }
        }

        [Test]
        public void CopyTest()
        {
            DataStore data = DataStore.Load(@"Data\dna_modified.trn", FileFormat.Rses1);
            DataStore data2 = DataStore.Copy(data, 0, data.NumberOfRecords);
            int[] fieldIds = data.DataStoreInfo.GetFieldIds().ToArray();

            for (int i = 0; i < data.NumberOfRecords; i++)
            {
                var rec1 = data.GetDataVector(i);
                var rec2 = data2.GetDataVector(i);
                Assert.AreEqual(rec1, rec2);
            }

            DataStore data3 = DataStore.Copy(data, data.NumberOfRecords / 2, data.NumberOfRecords - (data.NumberOfRecords / 2));
            Assert.AreEqual(data.NumberOfRecords - (data.NumberOfRecords / 2), data3.NumberOfRecords);

            DataStore data4 = DataStore.Copy(data, 0, data.NumberOfRecords - (data.NumberOfRecords / 2));
            Assert.AreEqual(data.NumberOfRecords - (data.NumberOfRecords / 2), data3.NumberOfRecords);
        }

        [Test]
        public void SwapTest()
        {
            DataStore data = DataStore.Load(@"Data\dna_modified.trn", FileFormat.Rses1);
            for (int i = 0; i < data.NumberOfRecords; i++)
            {
                for (int j = 0; j < data.NumberOfRecords; j++)
                {
                    var rec1 = data.GetDataVector(i);
                    var rec2 = data.GetDataVector(j);

                    data.Swap(i, j);

                    var rec3 = data.GetDataVector(i);
                    var rec4 = data.GetDataVector(j);

                    Assert.AreEqual(rec2, rec3);
                    Assert.AreEqual(rec1, rec4);
                }
            }
        }

        [Test]
        public void ShuffleTest()
        {
            DataStore data = DataStore.Load(@"Data\dna_modified.trn", FileFormat.Rses1);
            data.Shuffle();
        }
    }
}