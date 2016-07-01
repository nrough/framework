﻿using System;
using System.Linq;
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
            string trainFileName = @"Data\monks-1.train";
            string testFileName = @"Data\monks-1.test";

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
            //First newInstance
            //1 1 1 1 3 1 1 

            long internalValue = dataStoreTrain.GetFieldValue(0, 1);
            Object externalValue = dataStoreTrainInfo.GetFieldInfo(1).Internal2External(internalValue);
            Assert.AreEqual(1, (int)externalValue);

            internalValue = dataStoreTrain.GetFieldValue(0, 5);
            externalValue = dataStoreTrainInfo.GetFieldInfo(5).Internal2External(internalValue);
            Assert.AreEqual(3, (int)externalValue);

            internalValue = dataStoreTrain.GetFieldValue(0, 7);
            externalValue = dataStoreTrainInfo.GetFieldInfo(7).Internal2External(internalValue);
            Assert.AreEqual(1, (int)externalValue);

            //Last newInstance
            //3 3 2 3 4 2 1

            internalValue = dataStoreTrain.GetFieldValue(dataStoreTrainInfo.NumberOfRecords - 1, 1);
            externalValue = dataStoreTrainInfo.GetFieldInfo(1).Internal2External(internalValue);
            Assert.AreEqual(3, (int)externalValue);

            internalValue = dataStoreTrain.GetFieldValue(dataStoreTrainInfo.NumberOfRecords - 1, 5);
            externalValue = dataStoreTrainInfo.GetFieldInfo(5).Internal2External(internalValue);
            Assert.AreEqual(4, (int)externalValue);

            internalValue = dataStoreTrain.GetFieldValue(dataStoreTrainInfo.NumberOfRecords - 1, 7);
            externalValue = dataStoreTrainInfo.GetFieldInfo(7).Internal2External(internalValue);
            Assert.AreEqual(1, (int)externalValue);

            //60th newInstance
            //2 1 2 3 4 1 0 

            internalValue = dataStoreTrain.GetFieldValue(60, 1);
            externalValue = dataStoreTrainInfo.GetFieldInfo(1).Internal2External(internalValue);
            Assert.AreEqual(2, (int)externalValue);

            internalValue = dataStoreTrain.GetFieldValue(60, 5);
            externalValue = dataStoreTrainInfo.GetFieldInfo(5).Internal2External(internalValue);
            Assert.AreEqual(4, (int)externalValue);

            internalValue = dataStoreTrain.GetFieldValue(60, 7);
            externalValue = dataStoreTrainInfo.GetFieldInfo(7).Internal2External(internalValue);
            Assert.AreEqual(0, (int)externalValue);
        }

        [Test]
        public void TrainDataFieldEncoding()
        {
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
                
                foreach(int fieldId in record.GetFields())
                {
                    Assert.AreEqual(dataStoreTest.GetFieldValue((int)record.ObjectId - 1, fieldId), record[fieldId]);
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
            for(int i=0; i<data.NumberOfRecords; i++)
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

        [Test]
        public void ShuffleTest()
        {
            DataStore data = DataStore.Load(@"Data\dna_modified.trn", FileFormat.Rses1);
            data.Shuffle();
        }

    }
}
