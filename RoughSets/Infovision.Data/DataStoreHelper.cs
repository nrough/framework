using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using Accord.Statistics.Filters;

namespace Infovision.Data
{
    public static class DataStoreHelper
    {
        [CLSCompliant(false)]
        public static DataStore ToDataStore(this DataTable source, Codification codification, int decisionIdx = -1, int idIdx = -1)
        {
            //if (idIdx == -1 && codification.Columns.Count != source.Columns.Count)
            //    throw new InvalidOperationException("Number of columns in source table and codification description must be the same");
            //if (idIdx != -1 && codification.Columns.Count != (source.Columns.Count - 1))
            //    throw new InvalidOperationException("Number of columns in source table and codification description must be the same");

            DataStoreInfo dataStoreInfo = new DataStoreInfo(source.Columns.Count);
            dataStoreInfo.NumberOfRecords = source.Rows.Count;

            int[] fieldIds = new int[dataStoreInfo.NumberOfFields];

            for (int i = 0; i < source.Columns.Count; i++)
            {
                DataColumn col = source.Columns[i];
                bool isFieldCodified = (codification != null) ? codification.Columns.Contains(col.ColumnName) : false;
                Type columnType = isFieldCodified ? typeof(String) : col.DataType;

                DataFieldInfo fieldInfo = new DataFieldInfo(i + 1, columnType);
                fieldInfo.Name = col.ColumnName;
                fieldInfo.Alias = col.ColumnName;
                fieldIds[i] = fieldInfo.Id;
                
                if (i == idIdx || codification == null)
                {
                    //Get unchanged Id
                    for (int j = 0; j < source.Rows.Count; j++)
                    {
                        int idValue = source.Rows[j].Field<int>(i);
                        //We assume that all missing values are replaced
                        fieldInfo.AddInternal((long)idValue, idValue, false);
                    }
                }
                //Codification does not contain all columns e.g. continues attributes and id
                else if (isFieldCodified)
                {                    
                    foreach (KeyValuePair<string, int> kvp in codification.Columns[col.ColumnName].Mapping)
                    {
                        //We assume that all missing values are replaced
                        fieldInfo.AddInternal((long)kvp.Value, kvp.Key, false);
                    }
                }
                                

                if (i == decisionIdx)
                {
                    dataStoreInfo.AddFieldInfo(fieldInfo, FieldTypes.Decision);
                }
                else if (i == idIdx)
                {
                    dataStoreInfo.AddFieldInfo(fieldInfo, FieldTypes.Identifier);
                }
                else
                {
                    dataStoreInfo.AddFieldInfo(fieldInfo, FieldTypes.Standard);
                }
            }

            DataStore result = new DataStore(dataStoreInfo);
            result.Name = source.TableName;
            long[] vector = new long[dataStoreInfo.NumberOfFields];

            foreach (DataRow row in source.Rows)
            {
                for (int j = 0; j < source.Columns.Count; j++)
                {
                    vector[j] = Int64.Parse(row[j].ToString());
                }

                DataRecordInternal dataStoreRecord = new DataRecordInternal(fieldIds, vector);
                dataStoreRecord.ObjectId = vector[idIdx];
                                
                //TODO Check this idIdx is this record index in DataStore?
                dataStoreRecord.ObjectIdx = idIdx;

                result.Insert(dataStoreRecord);
            }

            return result;
        }    
    }
}
