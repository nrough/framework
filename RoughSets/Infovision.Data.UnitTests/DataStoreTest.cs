using System;
using System.Collections.Generic;

using NUnit.Framework;

namespace Infovision.Data.UnitTests
{
    [TestFixture]
    class DataStoreTest
    {
        DataStore dataStoreTrain = null;
        DataStore dataStoreTest = null;

        DataStoreInfo dataStoreTrainInfo = null;
        DataStoreInfo dataStoreTestInfo = null;

        public DataStoreTest()
        {
            string trainFileName = @"monks-1.train";
            string testFileName = @"monks-1.test";

            dataStoreTrain = DataStore.Load(trainFileName, FileFormat.Rses1);
            dataStoreTest = DataStore.Load(testFileName, FileFormat.Rses1, dataStoreTrain.DataStoreInfo);

            dataStoreTrainInfo = dataStoreTrain.DataStoreInfo;
            dataStoreTestInfo = dataStoreTest.DataStoreInfo;
        }

        [Test]
        public void TestData()
        {
            Assert.AreEqual(7, dataStoreTrainInfo.DecisionFieldId);
            Assert.AreEqual(7, dataStoreTestInfo.DecisionFieldId);
            
            Assert.AreEqual(432, dataStoreTestInfo.NumberOfRecords);
            Assert.AreEqual(124, dataStoreTrainInfo.NumberOfRecords);
            
            Assert.AreEqual(7, dataStoreTrainInfo.NumberOfFields);
            Assert.AreEqual(7, dataStoreTestInfo.NumberOfFields);

            Assert.AreEqual(3, dataStoreTrainInfo.GetFieldInfo(1).InternalValues().Count);
            Assert.AreEqual(3, dataStoreTrainInfo.GetFieldInfo(2).InternalValues().Count);
            Assert.AreEqual(2, dataStoreTrainInfo.GetFieldInfo(3).InternalValues().Count);
            Assert.AreEqual(3, dataStoreTrainInfo.GetFieldInfo(4).InternalValues().Count);
            Assert.AreEqual(4, dataStoreTrainInfo.GetFieldInfo(5).InternalValues().Count);
            Assert.AreEqual(2, dataStoreTrainInfo.GetFieldInfo(6).InternalValues().Count);
            Assert.AreEqual(2, dataStoreTrainInfo.GetFieldInfo(7).InternalValues().Count);

            Assert.AreEqual(3, dataStoreTrainInfo.GetFieldInfo(1).Values().Count);
            Assert.AreEqual(3, dataStoreTrainInfo.GetFieldInfo(2).Values().Count);
            Assert.AreEqual(2, dataStoreTrainInfo.GetFieldInfo(3).Values().Count);
            Assert.AreEqual(3, dataStoreTrainInfo.GetFieldInfo(4).Values().Count);
            Assert.AreEqual(4, dataStoreTrainInfo.GetFieldInfo(5).Values().Count);
            Assert.AreEqual(2, dataStoreTrainInfo.GetFieldInfo(6).Values().Count);
            Assert.AreEqual(2, dataStoreTrainInfo.GetFieldInfo(7).Values().Count);

            Assert.AreEqual(3, dataStoreTestInfo.GetFieldInfo(1).InternalValues().Count);
            Assert.AreEqual(3, dataStoreTestInfo.GetFieldInfo(2).InternalValues().Count);
            Assert.AreEqual(2, dataStoreTestInfo.GetFieldInfo(3).InternalValues().Count);
            Assert.AreEqual(3, dataStoreTestInfo.GetFieldInfo(4).InternalValues().Count);
            Assert.AreEqual(4, dataStoreTestInfo.GetFieldInfo(5).InternalValues().Count);
            Assert.AreEqual(2, dataStoreTestInfo.GetFieldInfo(6).InternalValues().Count);
            Assert.AreEqual(2, dataStoreTestInfo.GetFieldInfo(7).InternalValues().Count);

            Assert.AreEqual(3, dataStoreTestInfo.GetFieldInfo(1).Values().Count);
            Assert.AreEqual(3, dataStoreTestInfo.GetFieldInfo(2).Values().Count);
            Assert.AreEqual(2, dataStoreTestInfo.GetFieldInfo(3).Values().Count);
            Assert.AreEqual(3, dataStoreTestInfo.GetFieldInfo(4).Values().Count);
            Assert.AreEqual(4, dataStoreTestInfo.GetFieldInfo(5).Values().Count);
            Assert.AreEqual(2, dataStoreTestInfo.GetFieldInfo(6).Values().Count);
            Assert.AreEqual(2, dataStoreTestInfo.GetFieldInfo(7).Values().Count);

        }

        [Test]
        public void ExternalFieldEncoding()
        {
            //First instance
            //1 1 1 1 3 1 1 

            long internalValue = dataStoreTrain.GetObjectField(0, 1);
            Object externalValue = dataStoreTrainInfo.GetFieldInfo(1).Internal2External(internalValue);
            Assert.AreEqual(1, (int)externalValue);

            internalValue = dataStoreTrain.GetObjectField(0, 5);
            externalValue = dataStoreTrainInfo.GetFieldInfo(5).Internal2External(internalValue);
            Assert.AreEqual(3, (int)externalValue);

            internalValue = dataStoreTrain.GetObjectField(0, 7);
            externalValue = dataStoreTrainInfo.GetFieldInfo(7).Internal2External(internalValue);
            Assert.AreEqual(1, (int)externalValue);

            //Last instance
            //3 3 2 3 4 2 1
            
            internalValue = dataStoreTrain.GetObjectField(dataStoreTrainInfo.NumberOfRecords - 1, 1);
            externalValue = dataStoreTrainInfo.GetFieldInfo(1).Internal2External(internalValue);
            Assert.AreEqual(3, (int)externalValue);

            internalValue = dataStoreTrain.GetObjectField(dataStoreTrainInfo.NumberOfRecords - 1, 5);
            externalValue = dataStoreTrainInfo.GetFieldInfo(5).Internal2External(internalValue);
            Assert.AreEqual(4, (int)externalValue);

            internalValue = dataStoreTrain.GetObjectField(dataStoreTrainInfo.NumberOfRecords - 1, 7);
            externalValue = dataStoreTrainInfo.GetFieldInfo(7).Internal2External(internalValue);
            Assert.AreEqual(1, (int)externalValue);

            //60th instance
            //2 1 2 3 4 1 0 

            internalValue = dataStoreTrain.GetObjectField(60, 1);
            externalValue = dataStoreTrainInfo.GetFieldInfo(1).Internal2External(internalValue);
            Assert.AreEqual(2, (int)externalValue);

            internalValue = dataStoreTrain.GetObjectField(60, 5);
            externalValue = dataStoreTrainInfo.GetFieldInfo(5).Internal2External(internalValue);
            Assert.AreEqual(4, (int)externalValue);

            internalValue = dataStoreTrain.GetObjectField(60, 7);
            externalValue = dataStoreTrainInfo.GetFieldInfo(7).Internal2External(internalValue);
            Assert.AreEqual(0, (int)externalValue);
        }

        [Test]
        public void TrainDataFieldEncoding()
        {
            for (int i = 0; i < dataStoreTrainInfo.NumberOfRecords; i++)
            {
                DataRecordInternal dataRecord = dataStoreTrain.GetRecordByIndex(i);

                foreach (int fieldId in dataRecord.GetFields())
                {
                    long internalValue = dataStoreTrain.GetObjectField(i, fieldId);
                    Object externalValue = dataStoreTrainInfo.GetFieldInfo(fieldId).Internal2External(internalValue);
                    Object externalValueFromRecord = dataStoreTrainInfo.GetFieldInfo(fieldId).Internal2External(dataRecord[fieldId]);
                    Assert.AreEqual((int)externalValueFromRecord, (int)externalValue);                    
                }
            }
        }

        [Test]
        public void TestDataFieldEncoding()
        {
            for (int i = 0; i < dataStoreTestInfo.NumberOfRecords; i++)
            {
                DataRecordInternal dataRecord = dataStoreTest.GetRecordByIndex(i);

                foreach (int fieldId in dataRecord.GetFields())
                {
                    long internalValue = dataStoreTest.GetObjectField(i, fieldId);
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
            Dictionary<DataRecordInternal, Int32> dict = new Dictionary<DataRecordInternal, Int32>();
            int count = 0;

            for (int i = 0; i < dataStoreTestInfo.NumberOfRecords; i++)
            {
                DataRecordInternal dataRecord1 = dataStoreTest.GetRecordByIndex(i);
                DataRecordInternal dataRecord2 = dataStoreTest.GetRecordByIndex(i);
                
                Assert.AreEqual(dataRecord1, dataRecord2);
                Assert.AreNotSame(dataRecord1, dataRecord2);
                
                dict[dataRecord1] = dict.TryGetValue(dataRecord1, out count) ? ++count : 1;
            }

            foreach (KeyValuePair<DataRecordInternal, Int32> kvp in dict)
            {
                Assert.AreEqual(1, kvp.Value);
                DataRecordInternal record = kvp.Key;
                
                foreach(int fieldId in record.GetFields())
                {
                    Assert.AreEqual(dataStoreTest.GetObjectField((int)record.ObjectId - 1, fieldId), record[fieldId]);
                }
            }
        }

        [Test]
        public void DataStoreInfoTest()
        {
            foreach (int fieldId in dataStoreTestInfo.GetFieldIds(FieldTypes.All))
            {
                foreach (long internalValue in dataStoreTestInfo.GetFieldInfo(fieldId).InternalValues())
                {
                    Object externalValue = dataStoreTestInfo.GetFieldInfo(fieldId).Internal2External(internalValue);
                    long trainInternalValue = dataStoreTrainInfo.GetFieldInfo(fieldId).External2Internal(externalValue);
                    Assert.AreEqual(internalValue, trainInternalValue);
                }
            }

            foreach (int fieldId in dataStoreTrainInfo.GetFieldIds(FieldTypes.All))
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
            FieldTypes fieldTypeFlags = FieldTypes.All;
            Assert.IsTrue(fieldTypeFlags.HasFlag(FieldTypes.All));
            
        }

        [Test]
        public void OrderByTest()
        {
            int[] orderBy = new int[] { 1, 3, 5 };
            DataStoreOrderByComparer comparer = new DataStoreOrderByComparer(this.dataStoreTrain, orderBy);
            int[] sortedArray = this.dataStoreTrain.OrderBy(orderBy, comparer);
            
            for (int i = 1; i < this.dataStoreTrain.NumberOfRecords; i++)
            {
                AttributeValueVector record = this.dataStoreTrain.GetDataVector(sortedArray[i - 1], orderBy);
                int result = comparer.Compare(sortedArray[i - 1], sortedArray[i]);

                Assert.AreNotEqual(1, result);
            }
        }

    }
}
