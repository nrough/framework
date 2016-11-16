using NUnit.Framework;

namespace Infovision.Data.UnitTests
{
    [TestFixture]
    public class DataStoreSplitterTest
    {
        private DataStore dataStore;

        [SetUp]
        public void Init()
        {
            string fileName = @"Data\chess.dta";
            dataStore = DataStore.Load(fileName, FileFormat.Rses1);
        }

        [Test]
        public void SplitRatio()
        {
            DataStore dataStore1 = null;
            DataStore dataStore2 = null;

            for (int i = 0; i <= 100; i += 10)
            {
                DataStoreSplitter dataStoreSplitter = new DataStoreSplitterRatio(dataStore, (double)i / (double)100);                
                dataStoreSplitter.Split(ref dataStore1, ref dataStore2, 0);

                Assert.AreEqual(dataStore.DataStoreInfo.NumberOfRecords, dataStore1.DataStoreInfo.NumberOfRecords + dataStore2.DataStoreInfo.NumberOfRecords);
                Assert.AreEqual(dataStore.DataStoreInfo.NumberOfRecords, dataStore1.DataStoreInfo.NumberOfRecords + dataStore2.DataStoreInfo.NumberOfRecords);
                Assert.AreEqual(dataStore.DataStoreInfo.NumberOfFields, dataStore1.DataStoreInfo.NumberOfFields);
                Assert.AreEqual(dataStore.DataStoreInfo.NumberOfFields, dataStore2.DataStoreInfo.NumberOfFields);
                Assert.AreEqual(dataStore.DataStoreInfo.DecisionFieldId, dataStore1.DataStoreInfo.DecisionFieldId);
                Assert.AreEqual(dataStore.DataStoreInfo.DecisionFieldId, dataStore2.DataStoreInfo.DecisionFieldId);

                foreach (int fieldId in dataStore.DataStoreInfo.GetFieldIds(FieldTypes.All))
                {
                    Assert.AreEqual(
                        dataStore.DataStoreInfo.GetFieldInfo(fieldId).FieldValueType, 
                        dataStore1.DataStoreInfo.GetFieldInfo(fieldId).FieldValueType,
                        string.Format("Field Id {0}", fieldId));

                    Assert.AreEqual(
                        dataStore.DataStoreInfo.GetFieldInfo(fieldId).Alias,
                        dataStore1.DataStoreInfo.GetFieldInfo(fieldId).Alias,
                        string.Format("Field Id {0}", fieldId));

                    Assert.AreEqual(
                        dataStore.DataStoreInfo.GetFieldInfo(fieldId).Id,
                        dataStore1.DataStoreInfo.GetFieldInfo(fieldId).Id,
                        string.Format("Field Id {0}", fieldId));

                    Assert.AreEqual(
                        dataStore.DataStoreInfo.GetFieldInfo(fieldId).Name,
                        dataStore1.DataStoreInfo.GetFieldInfo(fieldId).Name,
                        string.Format("Field Id {0}", fieldId));

                    Assert.AreEqual(
                        dataStore.DataStoreInfo.GetFieldInfo(fieldId).HasMissingValues,
                        dataStore1.DataStoreInfo.GetFieldInfo(fieldId).HasMissingValues,
                        string.Format("Field Id {0}", fieldId));

                    Assert.AreEqual(
                        dataStore.DataStoreInfo.GetFieldInfo(fieldId).MissingValueInternal,
                        dataStore1.DataStoreInfo.GetFieldInfo(fieldId).MissingValueInternal,
                        string.Format("Field Id {0}", fieldId));

                    Assert.AreEqual(
                        dataStore.DataStoreInfo.GetFieldInfo(fieldId).MissingValue,
                        dataStore1.DataStoreInfo.GetFieldInfo(fieldId).MissingValue,
                        string.Format("Field Id {0}", fieldId));

                    Assert.AreEqual(
                        dataStore.DataStoreInfo.GetFieldInfo(fieldId).IsNumeric,
                        dataStore1.DataStoreInfo.GetFieldInfo(fieldId).IsNumeric,
                        string.Format("Field Id {0}", fieldId));

                    Assert.AreEqual(
                        dataStore.DataStoreInfo.GetFieldInfo(fieldId).IsOrdered,
                        dataStore1.DataStoreInfo.GetFieldInfo(fieldId).IsOrdered,
                        string.Format("Field Id {0}", fieldId));

                    Assert.AreEqual(
                        dataStore.DataStoreInfo.GetFieldInfo(fieldId).IsSymbolic,
                        dataStore1.DataStoreInfo.GetFieldInfo(fieldId).IsSymbolic,
                        string.Format("Field Id {0}", fieldId));

                    Assert.AreEqual(
                        dataStore.DataStoreInfo.GetFieldInfo(fieldId).IsUnique,
                        dataStore1.DataStoreInfo.GetFieldInfo(fieldId).IsUnique,
                        string.Format("Field Id {0}", fieldId));



                    Assert.AreEqual(
                        dataStore.DataStoreInfo.GetFieldInfo(fieldId).FieldValueType,
                        dataStore2.DataStoreInfo.GetFieldInfo(fieldId).FieldValueType,
                        string.Format("Field Id {0}", fieldId));

                    Assert.AreEqual(
                        dataStore.DataStoreInfo.GetFieldInfo(fieldId).Alias,
                        dataStore2.DataStoreInfo.GetFieldInfo(fieldId).Alias,
                        string.Format("Field Id {0}", fieldId));

                    Assert.AreEqual(
                        dataStore.DataStoreInfo.GetFieldInfo(fieldId).Id,
                        dataStore2.DataStoreInfo.GetFieldInfo(fieldId).Id,
                        string.Format("Field Id {0}", fieldId));

                    Assert.AreEqual(
                        dataStore.DataStoreInfo.GetFieldInfo(fieldId).Name,
                        dataStore2.DataStoreInfo.GetFieldInfo(fieldId).Name,
                        string.Format("Field Id {0}", fieldId));

                    Assert.AreEqual(
                        dataStore.DataStoreInfo.GetFieldInfo(fieldId).HasMissingValues,
                        dataStore2.DataStoreInfo.GetFieldInfo(fieldId).HasMissingValues,
                        string.Format("Field Id {0}", fieldId));

                    Assert.AreEqual(
                        dataStore.DataStoreInfo.GetFieldInfo(fieldId).MissingValueInternal,
                        dataStore2.DataStoreInfo.GetFieldInfo(fieldId).MissingValueInternal,
                        string.Format("Field Id {0}", fieldId));

                    Assert.AreEqual(
                        dataStore.DataStoreInfo.GetFieldInfo(fieldId).MissingValue,
                        dataStore2.DataStoreInfo.GetFieldInfo(fieldId).MissingValue,
                        string.Format("Field Id {0}", fieldId));

                    Assert.AreEqual(
                        dataStore.DataStoreInfo.GetFieldInfo(fieldId).IsNumeric,
                        dataStore2.DataStoreInfo.GetFieldInfo(fieldId).IsNumeric,
                        string.Format("Field Id {0}", fieldId));

                    Assert.AreEqual(
                        dataStore.DataStoreInfo.GetFieldInfo(fieldId).IsOrdered,
                        dataStore2.DataStoreInfo.GetFieldInfo(fieldId).IsOrdered,
                        string.Format("Field Id {0}", fieldId));

                    Assert.AreEqual(
                        dataStore.DataStoreInfo.GetFieldInfo(fieldId).IsSymbolic,
                        dataStore2.DataStoreInfo.GetFieldInfo(fieldId).IsSymbolic,
                        string.Format("Field Id {0}", fieldId));

                    Assert.AreEqual(
                        dataStore.DataStoreInfo.GetFieldInfo(fieldId).IsUnique,
                        dataStore2.DataStoreInfo.GetFieldInfo(fieldId).IsUnique,
                        string.Format("Field Id {0}", fieldId));
                }

                foreach (long objectId in dataStore.GetObjectIds())
                {
                    Assert.AreNotEqual(0, objectId);

                    if (dataStore1.Exists(objectId))
                    {
                        Assert.IsFalse(dataStore2.Exists(objectId));
                    }

                    if (dataStore2.Exists(objectId))
                    {
                        Assert.IsFalse(dataStore1.Exists(objectId));
                    }

                    Assert.IsFalse((!dataStore1.Exists(objectId)) && (!dataStore2.Exists(objectId)));
                }
            }
        }

        [Test]
        public void SplitNFold()
        {
            DataStore dataStore1 = null;
            DataStore dataStore2 = null;

            DataStoreSplitter dataStoreSplitter = new DataStoreSplitter(dataStore, 10);

            for (int i = 0; i < 10; i++)
            {                
                dataStoreSplitter.Split(ref dataStore1, ref dataStore2, i);

                Assert.AreEqual(dataStore.DataStoreInfo.NumberOfRecords, dataStore1.DataStoreInfo.NumberOfRecords + dataStore2.DataStoreInfo.NumberOfRecords);
                Assert.AreEqual(dataStore.DataStoreInfo.NumberOfRecords, dataStore1.DataStoreInfo.NumberOfRecords + dataStore2.DataStoreInfo.NumberOfRecords);
                Assert.AreEqual(dataStore.DataStoreInfo.NumberOfFields, dataStore1.DataStoreInfo.NumberOfFields);
                Assert.AreEqual(dataStore.DataStoreInfo.NumberOfFields, dataStore2.DataStoreInfo.NumberOfFields);
                Assert.AreEqual(dataStore.DataStoreInfo.DecisionFieldId, dataStore1.DataStoreInfo.DecisionFieldId);
                Assert.AreEqual(dataStore.DataStoreInfo.DecisionFieldId, dataStore2.DataStoreInfo.DecisionFieldId);

                foreach (int fieldId in dataStore.DataStoreInfo.GetFieldIds(FieldTypes.All))
                {
                    Assert.AreEqual(
                        dataStore.DataStoreInfo.GetFieldInfo(fieldId).FieldValueType,
                        dataStore1.DataStoreInfo.GetFieldInfo(fieldId).FieldValueType,
                        string.Format("Field Id {0}", fieldId));

                    Assert.AreEqual(
                        dataStore.DataStoreInfo.GetFieldInfo(fieldId).Alias,
                        dataStore1.DataStoreInfo.GetFieldInfo(fieldId).Alias,
                        string.Format("Field Id {0}", fieldId));

                    Assert.AreEqual(
                        dataStore.DataStoreInfo.GetFieldInfo(fieldId).Id,
                        dataStore1.DataStoreInfo.GetFieldInfo(fieldId).Id,
                        string.Format("Field Id {0}", fieldId));

                    Assert.AreEqual(
                        dataStore.DataStoreInfo.GetFieldInfo(fieldId).Name,
                        dataStore1.DataStoreInfo.GetFieldInfo(fieldId).Name,
                        string.Format("Field Id {0}", fieldId));

                    Assert.AreEqual(
                        dataStore.DataStoreInfo.GetFieldInfo(fieldId).HasMissingValues,
                        dataStore1.DataStoreInfo.GetFieldInfo(fieldId).HasMissingValues,
                        string.Format("Field Id {0}", fieldId));

                    Assert.AreEqual(
                        dataStore.DataStoreInfo.GetFieldInfo(fieldId).MissingValueInternal,
                        dataStore1.DataStoreInfo.GetFieldInfo(fieldId).MissingValueInternal,
                        string.Format("Field Id {0}", fieldId));

                    Assert.AreEqual(
                        dataStore.DataStoreInfo.GetFieldInfo(fieldId).MissingValue,
                        dataStore1.DataStoreInfo.GetFieldInfo(fieldId).MissingValue,
                        string.Format("Field Id {0}", fieldId));

                    Assert.AreEqual(
                        dataStore.DataStoreInfo.GetFieldInfo(fieldId).IsNumeric,
                        dataStore1.DataStoreInfo.GetFieldInfo(fieldId).IsNumeric,
                        string.Format("Field Id {0}", fieldId));

                    Assert.AreEqual(
                        dataStore.DataStoreInfo.GetFieldInfo(fieldId).IsOrdered,
                        dataStore1.DataStoreInfo.GetFieldInfo(fieldId).IsOrdered,
                        string.Format("Field Id {0}", fieldId));

                    Assert.AreEqual(
                        dataStore.DataStoreInfo.GetFieldInfo(fieldId).IsSymbolic,
                        dataStore1.DataStoreInfo.GetFieldInfo(fieldId).IsSymbolic,
                        string.Format("Field Id {0}", fieldId));

                    Assert.AreEqual(
                        dataStore.DataStoreInfo.GetFieldInfo(fieldId).IsUnique,
                        dataStore1.DataStoreInfo.GetFieldInfo(fieldId).IsUnique,
                        string.Format("Field Id {0}", fieldId));



                    Assert.AreEqual(
                        dataStore.DataStoreInfo.GetFieldInfo(fieldId).FieldValueType,
                        dataStore2.DataStoreInfo.GetFieldInfo(fieldId).FieldValueType,
                        string.Format("Field Id {0}", fieldId));

                    Assert.AreEqual(
                        dataStore.DataStoreInfo.GetFieldInfo(fieldId).Alias,
                        dataStore2.DataStoreInfo.GetFieldInfo(fieldId).Alias,
                        string.Format("Field Id {0}", fieldId));

                    Assert.AreEqual(
                        dataStore.DataStoreInfo.GetFieldInfo(fieldId).Id,
                        dataStore2.DataStoreInfo.GetFieldInfo(fieldId).Id,
                        string.Format("Field Id {0}", fieldId));

                    Assert.AreEqual(
                        dataStore.DataStoreInfo.GetFieldInfo(fieldId).Name,
                        dataStore2.DataStoreInfo.GetFieldInfo(fieldId).Name,
                        string.Format("Field Id {0}", fieldId));

                    Assert.AreEqual(
                        dataStore.DataStoreInfo.GetFieldInfo(fieldId).HasMissingValues,
                        dataStore2.DataStoreInfo.GetFieldInfo(fieldId).HasMissingValues,
                        string.Format("Field Id {0}", fieldId));

                    Assert.AreEqual(
                        dataStore.DataStoreInfo.GetFieldInfo(fieldId).MissingValueInternal,
                        dataStore2.DataStoreInfo.GetFieldInfo(fieldId).MissingValueInternal,
                        string.Format("Field Id {0}", fieldId));

                    Assert.AreEqual(
                        dataStore.DataStoreInfo.GetFieldInfo(fieldId).MissingValue,
                        dataStore2.DataStoreInfo.GetFieldInfo(fieldId).MissingValue,
                        string.Format("Field Id {0}", fieldId));

                    Assert.AreEqual(
                        dataStore.DataStoreInfo.GetFieldInfo(fieldId).IsNumeric,
                        dataStore2.DataStoreInfo.GetFieldInfo(fieldId).IsNumeric,
                        string.Format("Field Id {0}", fieldId));

                    Assert.AreEqual(
                        dataStore.DataStoreInfo.GetFieldInfo(fieldId).IsOrdered,
                        dataStore2.DataStoreInfo.GetFieldInfo(fieldId).IsOrdered,
                        string.Format("Field Id {0}", fieldId));

                    Assert.AreEqual(
                        dataStore.DataStoreInfo.GetFieldInfo(fieldId).IsSymbolic,
                        dataStore2.DataStoreInfo.GetFieldInfo(fieldId).IsSymbolic,
                        string.Format("Field Id {0}", fieldId));

                    Assert.AreEqual(
                        dataStore.DataStoreInfo.GetFieldInfo(fieldId).IsUnique,
                        dataStore2.DataStoreInfo.GetFieldInfo(fieldId).IsUnique,
                        string.Format("Field Id {0}", fieldId));
                }

                foreach (long objectId in dataStore.GetObjectIds())
                {
                    Assert.AreNotEqual(0, objectId);

                    if (dataStore1.Exists(objectId))
                    {
                        Assert.IsFalse(dataStore2.Exists(objectId));
                    }

                    if (dataStore2.Exists(objectId))
                    {
                        Assert.IsFalse(dataStore1.Exists(objectId));
                    }

                    Assert.IsFalse((!dataStore1.Exists(objectId)) && (!dataStore2.Exists(objectId)));
                }
            }
        }

        [Test]
        public void Ratio()
        {
            DataStore dataStore1 = null;
            DataStore dataStore2 = null;

            DataStoreSplitter dataStoreSplitter = new DataStoreSplitterRatio(dataStore, 0.75);
            dataStoreSplitter.Split(ref dataStore1, ref dataStore2);

            Assert.Greater(dataStore1.DataStoreInfo.NumberOfRecords, dataStore2.DataStoreInfo.NumberOfRecords);
        }

        [Test]
        public void SplitHistogram()
        {
            DataStore dataStore1 = null;
            DataStore dataStore2 = null;
            int elementSum;
            int elementSum1;
            int elementSum2;

            DataStoreSplitter dataStoreSplitter = new DataStoreSplitterRatio(dataStore, 0.75);            
            dataStoreSplitter.Split(ref dataStore1, ref dataStore2, 0);

            foreach (int fieldId in dataStore.DataStoreInfo.GetFieldIds(FieldTypes.Standard))
            {
                elementSum = 0;
                elementSum1 = 0;
                elementSum2 = 0;

                foreach (long internalValue in dataStore.DataStoreInfo.GetFieldInfo(fieldId).InternalValues())
                {
                    elementSum += (int)dataStore.DataStoreInfo.GetFieldInfo(fieldId).Histogram.GetBinValue(internalValue);
                }

                foreach (long internalValue in dataStore1.DataStoreInfo.GetFieldInfo(fieldId).InternalValues())
                {
                    elementSum1 += (int)dataStore1.DataStoreInfo.GetFieldInfo(fieldId).Histogram.GetBinValue(internalValue);
                }

                foreach (long internalValue in dataStore2.DataStoreInfo.GetFieldInfo(fieldId).InternalValues())
                {
                    elementSum2 += (int)dataStore2.DataStoreInfo.GetFieldInfo(fieldId).Histogram.GetBinValue(internalValue);
                }

                Assert.AreEqual(elementSum, elementSum1 + elementSum2);
            }
        }
    }
}