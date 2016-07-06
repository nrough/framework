using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace Infovision.Data.UnitTests
{
    [TestFixture]
    public class DataStoreInfoTest
    {
        [Test]
        public void GetFieldIdsTest()
        {
            DataStore data = DataStore.Load(@"Data\dna_modified.trn", FileFormat.Rses1);

            Compare(data, FieldTypes.Standard);
            Compare(data, FieldTypes.All);
            Compare(data, FieldTypes.Decision);
            Compare(data, FieldTypes.None);
        }

        public void Compare(DataStore data, FieldTypes fieldType)
        {
            IEnumerable<int> fieldIds = data.DataStoreInfo.GetFieldIds(fieldType);
            int[] fieldIdsOld = data.DataStoreInfo.GetFieldIds_OLD(fieldType);
            int count = 0;
            foreach (int fieldId in fieldIds)
            {
                Assert.AreNotEqual(Array.IndexOf(fieldIdsOld, fieldId), -1);
                count++;
            }

            Assert.AreEqual(fieldIdsOld.Length, count);
        }
    }
}