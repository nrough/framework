﻿using System;
using System.Collections.Generic;
using NRough.Data;

namespace NRough.MachineLearning.MissingValues
{
    public class ReplaceMissingValues
    {
        public ReplaceMissingValues()
        {
        }

        public DataStore Compute(DataStore dataStore, DataStore referenceDataStore = null)
        {
            if (dataStore.DataStoreInfo.HasMissingData == false)
                return dataStore;

            if (referenceDataStore != null)
            {
                return this.ComputeFromReference(dataStore, referenceDataStore);
            }

            return this.ComputeNoReference(dataStore);
        }

        private DataStore ComputeFromReference(DataStore dataStore, DataStore referenceDataStore)
        {
            Dictionary<int, Dictionary<long, long>> mostFrequentValues = this.CalcMissingValues(referenceDataStore);
            return this.ReplaceValues(dataStore, mostFrequentValues);
        }

        private DataStore ComputeNoReference(DataStore dataStore)
        {
            Dictionary<int, Dictionary<long, long>> mostFrequentValues = this.CalcMissingValues(dataStore);            
            return this.ReplaceValues(dataStore, mostFrequentValues);
        }

        private Dictionary<int, Dictionary<long, long>> CalcMissingValues(DataStore dataStore)
        {
            //fieldId --> decision --> field key --> value
            var fieldMap = new Dictionary<int, Dictionary<long, Dictionary<long, int>>>();
            for (int i = 0; i < dataStore.NumberOfRecords; i++)
            {
                foreach (int fieldId in dataStore.DataStoreInfo.GetFieldIds(FieldGroup.Standard))
                {
                    AttributeInfo fieldInfo = dataStore.DataStoreInfo.GetFieldInfo(fieldId);
                    if (fieldInfo.HasMissingValues == false)
                        continue;

                    long internalValue = dataStore.GetFieldValue(i, fieldId);
                    long decisionValue = dataStore.GetDecisionValue(i);

                    Dictionary<long, Dictionary<long, int>> decisionMap = null;
                    if (fieldMap.TryGetValue(fieldId, out decisionMap))
                    {
                        Dictionary<long, int> internalCountMap = null;
                        if (decisionMap.TryGetValue(decisionValue, out internalCountMap))
                        {
                            int count = 0;
                            if (internalCountMap.TryGetValue(internalValue, out count))
                            {
                                internalCountMap[internalValue] = count + 1;
                            }
                            else
                            {
                                internalCountMap[internalValue] = 1;
                            }
                        }
                        else
                        {
                            internalCountMap = new Dictionary<long, int>();
                            internalCountMap.Add(internalValue, 1);
                            decisionMap[decisionValue] = internalCountMap;
                        }
                    }
                    else
                    {
                        var internalCountMap = new Dictionary<long, int>();
                        internalCountMap.Add(internalValue, 1);

                        decisionMap = new Dictionary<long, Dictionary<long, int>>();
                        decisionMap.Add(decisionValue, internalCountMap);

                        fieldMap.Add(fieldId, decisionMap);
                    }
                }
            }

            var mostFrequentValues = new Dictionary<int, Dictionary<long, long>>(dataStore.DataStoreInfo.NumberOfFields);
            foreach (var field in fieldMap)
            {
                AttributeInfo fieldInfo = dataStore.DataStoreInfo.GetFieldInfo(field.Key);

                int fieldMaxCount = Int32.MinValue;
                long fieldMaxValue = 0;
                bool fieldFound = false;

                foreach (var decision in field.Value)
                {
                    int maxCount = Int32.MinValue;
                    long maxValue = 0;
                    bool found = false;

                    foreach (var values in decision.Value)
                    {
                        if (values.Key != fieldInfo.MissingValueInternal)
                        {
                            if (maxCount < values.Value)
                            {
                                maxCount = values.Value;
                                maxValue = values.Key;
                                found = true;
                            }

                            if (fieldMaxCount < values.Value)
                            {
                                fieldMaxCount = values.Value;
                                fieldMaxValue = values.Key;
                                fieldFound = true;
                            }
                        }
                    }

                    if (found == false && fieldFound == true)
                    {
                        maxCount = fieldMaxCount;
                        maxValue = fieldMaxValue;
                    }
                    else if (found == false && fieldFound == false)
                    {
                        //TODO Cancel the whole column
                    }

                    Dictionary<long, long> decMap = null;
                    if (mostFrequentValues.TryGetValue(field.Key, out decMap))
                    {
                        decMap[decision.Key] = maxValue;
                    }
                    else
                    {
                        decMap = new Dictionary<long, long>();
                        decMap[decision.Key] = maxValue;
                        mostFrequentValues[field.Key] = decMap;
                    }
                }
            }

            return mostFrequentValues;
        }

        private DataStore ReplaceValues(DataStore dataStore, Dictionary<int, Dictionary<long, long>> frequentFieldValues)
        {
            DataStoreInfo newDataStoreInfo = new DataStoreInfo(dataStore.DataStoreInfo.NumberOfFields);            
            newDataStoreInfo.InitFromDataStoreInfo(dataStore.DataStoreInfo, true, false);
            newDataStoreInfo.NumberOfRecords = dataStore.NumberOfRecords;

            DataStore newDataStore = new DataStore(newDataStoreInfo);

            for (int i = 0; i < dataStore.NumberOfRecords; i++)
            {
                long decisionValue = dataStore.GetDecisionValue(i);
                DataRecordInternal record = dataStore.GetRecordByIndex(i);

                foreach (int fieldId in dataStore.DataStoreInfo.GetFieldIds(FieldGroup.Standard))
                {
                    AttributeInfo fieldInfo = dataStore.DataStoreInfo.GetFieldInfo(fieldId);
                    if (fieldInfo.HasMissingValues == false)
                        continue;

                    if (record[fieldId] == fieldInfo.MissingValueInternal)
                        record[fieldId] = frequentFieldValues[fieldId][decisionValue];
                }

                newDataStore.Insert(record);
            }

            newDataStoreInfo.NumberOfRecords = dataStore.NumberOfRecords;

            return newDataStore;
        }
    }
}