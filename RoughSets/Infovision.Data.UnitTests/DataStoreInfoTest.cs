using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace Raccoon.Data.Tests
{
    [TestFixture]
    public class DataStoreInfoTest
    {
        [Test]
        public void GetFieldIdsTest()
        {
            DataStore data = DataStore.Load(@"Data\dna_modified.trn", FileFormat.RSES1);

            Compare(data, FieldGroup.Standard);
            Compare(data, FieldGroup.All);
            Compare(data, FieldGroup.Output);
            Compare(data, FieldGroup.None);
        }

        public void Compare(DataStore data, FieldGroup fieldType)
        {
            IEnumerable<int> fieldIds = data.DataStoreInfo.GetFieldIds(fieldType);
            int[] fieldIdsOld = this.GetFieldIds_OLD(data.DataStoreInfo, fieldType);
            int count = 0;
            foreach (int fieldId in fieldIds)
            {
                Assert.AreNotEqual(Array.IndexOf(fieldIdsOld, fieldId), -1);
                count++;
            }

            Assert.AreEqual(fieldIdsOld.Length, count);
        }

        //An old method from DataStoreInfo class
        public int[] GetFieldIds_OLD(DataStoreInfo dataStoreInfo, FieldGroup fieldTypeFlags)
        {
            int[] fieldIds = new int[dataStoreInfo.GetNumberOfFields(fieldTypeFlags)];
            int i = 0;
            foreach (DataFieldInfo field in dataStoreInfo.Fields)
            {
                if (fieldTypeFlags == FieldGroup.All
                    || fieldTypeFlags == FieldGroup.None
                    || dataStoreInfo.GetFieldType(field.Id).HasFlag(fieldTypeFlags))
                {
                    fieldIds[i++] = field.Id;
                }
            }
            return fieldIds;
        }
    }
}