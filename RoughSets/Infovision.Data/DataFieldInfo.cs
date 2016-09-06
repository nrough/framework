using System;
using System.Collections.Generic;
using System.Globalization;
using Infovision.Utils;

namespace Infovision.Data
{
    [Serializable]
    public class DataFieldInfo
    {
        private Type fieldValueType;
        private Dictionary<long, object> indexDictionary;
        private Dictionary<object, long> valueDictionary;
        private long maxValueInternalId;
        private Histogram<long> histogram;
        private Histogram<long> histogramWeights;
        private int initialNumberOfValues;

        #region Constructors

        public DataFieldInfo(int attributeId, Type fieldValueType, int initialNumberOfValues = 0)
        {
            this.initialNumberOfValues = initialNumberOfValues;
            this.fieldValueType = fieldValueType;
            this.maxValueInternalId = 0;

            if (this.initialNumberOfValues == 0)
            {
                this.valueDictionary = new Dictionary<object, long>();
                this.indexDictionary = new Dictionary<long, object>();
                this.histogram = new Histogram<long>();
                this.histogramWeights = new Histogram<long>();
            }
            else
            {
                this.valueDictionary = new Dictionary<object, long>(this.initialNumberOfValues);
                this.indexDictionary = new Dictionary<long, object>(this.initialNumberOfValues);
                this.histogram = new Histogram<long>(this.initialNumberOfValues);
                this.histogramWeights = new Histogram<long>(this.initialNumberOfValues);
            }

            this.Id = attributeId;
            this.Name = String.Format(CultureInfo.InvariantCulture, "a{0}", attributeId);
            this.HasMissingValues = false;
            this.MissingValue = null;
        }

        #endregion Constructors

        #region Properties

        public string Name { get; set; }
        public string Alias { get; set; }
        public int Id { get; set; }

        public Type FieldValueType
        {
            get { return fieldValueType; }
            set { fieldValueType = value; }
        }

        public Histogram<long> Histogram { get { return histogram; } }
        public Histogram<long> HistogramWeights { get { return histogramWeights; } }

        public long MinValue { get { return histogram.Min; } }
        public long MaxValue { get { return histogram.Max; } }

        public bool HasMissingValues { get; set; }
        public object MissingValue { get; set; }
        public long MissingValueInternal { get; set; }

        public bool IsNumeric { get; set; }
        public double[] Cuts { get; set; }

        public int NumberOfValues
        {
            get { return this.Histogram.Count; }
        }

        #endregion Properties

        #region Methods

        public void Reset()
        {
            this.maxValueInternalId = 0;
            if (this.initialNumberOfValues == 0)
            {
                this.valueDictionary = new Dictionary<object, long>();
                this.indexDictionary = new Dictionary<long, object>();
            }
            else
            {
                this.valueDictionary = new Dictionary<object, long>(this.initialNumberOfValues);
                this.indexDictionary = new Dictionary<long, object>(this.initialNumberOfValues);
            }
            this.histogram = new Histogram<long>();
            this.HasMissingValues = false;
        }

        public bool CanDiscretize()
        {
            //TODO IsUnique IsIdentifier  --> return false

            if (this.HasMissingValues)
                return false;

            if ((this.IsNumeric || Type.GetTypeCode(this.FieldValueType) == TypeCode.Double))
                return true;

            if (Type.GetTypeCode(this.FieldValueType) == TypeCode.Int32)
                return true;

            return false;
        }

        public void InitFromDataFieldInfo(DataFieldInfo dataFieldInfo, bool initValues, bool initMissingValues)
        {
            if (initValues)
            {
                foreach (KeyValuePair<long, object> kvp in dataFieldInfo.indexDictionary)
                {
                    if (initMissingValues)
                    {
                        if (dataFieldInfo.HasMissingValues && dataFieldInfo.MissingValue.Equals(kvp.Value))
                            this.AddInternal(kvp.Key, kvp.Value, true);
                        else
                            this.AddInternal(kvp.Key, kvp.Value, false);
                    }
                    else
                    {
                        if (dataFieldInfo.HasMissingValues && dataFieldInfo.MissingValueInternal == kvp.Key)
                        {
                            continue; //do nothing
                            //this.AddInternal(kvp.Key, kvp.Value, true);
                        }
                        else
                        {
                            this.AddInternal(kvp.Key, kvp.Value, false);
                        }
                    }
                }

                this.maxValueInternalId = dataFieldInfo.maxValueInternalId;
            }

            this.fieldValueType = dataFieldInfo.FieldValueType;
            this.Name = dataFieldInfo.Name;
            this.Alias = dataFieldInfo.Alias;
            this.Id = dataFieldInfo.Id;
            this.IsNumeric = dataFieldInfo.IsNumeric;
            this.initialNumberOfValues = dataFieldInfo.initialNumberOfValues;

            if (initMissingValues)
            {
                this.HasMissingValues = dataFieldInfo.HasMissingValues;
            }
        }

        public long External2Internal(object externalValue)
        {
            long internalValue;
            if (valueDictionary.TryGetValue(externalValue, out internalValue))
            {
                return internalValue;
            }
            return -1;
        }

        public object Internal2External(long internalValue)
        {
            object externalValue;
            if (indexDictionary.TryGetValue(internalValue, out externalValue))
            {
                return externalValue;
            }
            return null;
        }

        public long Add(object value, bool isMissing)
        {
            if (!valueDictionary.ContainsKey(value))
            {
                maxValueInternalId++;
                valueDictionary.Add(value, maxValueInternalId);
                indexDictionary.Add(maxValueInternalId, value);

                if (isMissing)
                {
                    if (this.MissingValue == null)
                    {
                        this.MissingValue = value;
                        this.MissingValueInternal = maxValueInternalId;
                    }
                    else if (!this.MissingValue.Equals(value))
                    {
                        throw new InvalidOperationException(String.Format("Missing key is already set. Trying to substitute current key {0} with {1}", this.MissingValue, value));
                    }
                }

                return maxValueInternalId;
            }

            if (isMissing)
            {
                if (this.MissingValue == null)
                    throw new InvalidOperationException(String.Format("Value {0} was indicated as missing key, while the existing missing key is null", value));
                else if (!this.MissingValue.Equals(value))
                    throw new InvalidOperationException(String.Format("Value {0{ was indicated as missing key, while the existing missing key is {1}", value, this.MissingValue));
            }

            return this.External2Internal(value);
        }

        public void AddInternal(long internalValue, object externalValue, bool isMissing)
        {
            //TODO in case we just copy values from existing FieldInfo we need to increase maxValueInternalId
            if (!valueDictionary.ContainsKey(externalValue))
            {
                valueDictionary.Add(externalValue, internalValue);
                indexDictionary.Add(internalValue, externalValue);

                if (isMissing)
                {
                    if (this.MissingValue == null)
                    {
                        this.MissingValue = externalValue;
                        this.MissingValueInternal = internalValue;
                    }
                    else if (!this.MissingValue.Equals(externalValue))
                    {
                        throw new InvalidOperationException(String.Format("Missing key is already set. Trying to substitute current key {0} with {1}", this.MissingValue, externalValue));
                    }
                }
            }

            if (isMissing)
            {
                if (this.MissingValue == null)
                    throw new InvalidOperationException(String.Format("Value {0} was indicated as missing key, while the existing missing key is null", externalValue));
                else if (!this.MissingValue.Equals(externalValue))
                    throw new InvalidOperationException(String.Format("Value {0{ was indicated as missing key, while the existing missing key is {1}", externalValue, this.MissingValue));
            }
        }

        public ICollection<object> Values()
        {
            return indexDictionary.Values;
        }

        public ICollection<long> InternalValues()
        {
            return valueDictionary.Values;
        }

        #region Histogram Methods

        public void IncreaseHistogramCount(long value)
        {
            histogram.Increase(value);
        }

        public int GetAttribiteValueCount(long value)
        {
            return (int)histogram.GetBinValue(value);
        }

        public double GetAttribiteValueWeight(long value)
        {
            return histogramWeights.GetBinValue(value);
        }

        public void CreateWeightHistogram(DataStore data, double[] weights)
        {
            int len = 0;
            if (histogramWeights != null)
            {
                len = histogramWeights.Count;
                histogramWeights = new Histogram<long>(len);
            }
            else
            {
                histogramWeights = new Histogram<long>();
            }

            int fieldIdx = data.DataStoreInfo.GetFieldIndex(this.Id);

            for (int i = 0; i < data.NumberOfRecords; i++)
                histogramWeights.Increase(data.GetFieldIndexValue(i, fieldIdx), weights[i]);
        }

        #endregion Histogram Methods

        #endregion Methods
    }
}