//
// NRough Framework
// http://www.nrough.net
//
// Copyright © Sebastian Widz, 2017
// authors at nrough.net

//This file is part of NRough.

//Foobar is free software; you can redistribute it and/or modify
//it under the terms of the GNU General Public License as published by
//the Free Software Foundation; either version 3 of the License, or
//(at your option) any later version.
//
//Foobar is distributed in the hope that it will be useful,
//but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
//GNU General Public License for more details.
//
//You should have received a copy of the GNU General Public License
//along with Foobar; if not, write to the Free Software
//Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA  02110-1301  USA
//
using System;
using System.Collections.Generic;
using System.Globalization;
using NRough.Core;
using NRough.Core.DataStructures;
using NRough.Core.BaseTypeExtensions;

namespace NRough.Data
{
    [Serializable]
    public class AttributeInfo
    {
        #region Members

        private Dictionary<long, object> indexDictionary;
        private Dictionary<object, long> valueDictionary;
        
        private Histogram<long> histogram;
        private Histogram<long> histogramWeights;

        private int initialNumberOfValues;
        private long maxValueInternalId;

        private bool isNumeric;
        private bool isSymbolic;        
        private bool isUnique;
        private bool isOrdered;

        public static long DefaultMissingValue = Int64.MaxValue;

        #endregion

        #region Properties

        public string Name { get; set; }
        public string Alias { get; set; }
        public int Id { get; set; }
        public Type DataType { get; set; }
        public int DerivedFrom { get; set; }
    
        public Histogram<long> Histogram { get { return histogram; } }
        public Histogram<long> HistogramWeights { get { return histogramWeights; } }

        public long MinValue { get { return histogram.Min; } }
        public long MaxValue { get { return histogram.Max; } }

        public bool HasMissingValues { get; set; }
        public object MissingValue { get; set; }
        public long MissingValueInternal { get; set; }

        public bool IsNumeric
        {
            get
            {
                return this.isNumeric;
            }
            set
            {
                this.isNumeric = value;
                this.isSymbolic = !this.isNumeric;
            }
        }

        public bool IsSymbolic
        {
            get
            {
                return this.isSymbolic;
            }
            set
            {
                this.isSymbolic = value;
                this.isNumeric = !this.isSymbolic;
            }
        }

        public bool IsUnique { get { return this.isUnique; } set { this.isUnique = value; } }        
        public bool IsOrdered { get { return this.isOrdered; } set { this.isOrdered = value; } }

        public bool IsStandard { get; set; }
        public bool IsDecision { get; set; }
        public bool IsIdentifier { get; set; }
        public bool IsSystem { get; set; }
        public bool IsWeight { get; set; }

        public long[] Cuts { get; set; }
        
        public int NumberOfValues
        {
            get { return this.Histogram.Count; }
        }

        public int NumberOfDecimals { get; set; }

        #endregion Properties

        #region Constructors

        public AttributeInfo(int attributeId, Type dataType, int initialNumberOfValues = 0)
        {
            this.initialNumberOfValues = initialNumberOfValues;
            this.DataType = dataType;
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
            this.NumberOfDecimals = 0;

            this.IsNumeric = AttributeInfo.IsNumericType(dataType);
            this.IsSymbolic = !this.IsNumeric;
            this.IsOrdered = false;

            this.IsDecision = false;
            this.IsIdentifier = false;
            this.IsUnique = false;
            this.IsSystem = false;
            this.IsStandard = true;
            this.IsWeight = false;
        }

        #endregion Constructors
    
        #region Methods

        public void InitFromReferenceAttribute(AttributeInfo referenceAttribute)
        {
            IsNumeric = referenceAttribute.IsNumeric;
            IsSymbolic = referenceAttribute.IsSymbolic;
            IsOrdered = referenceAttribute.IsOrdered;
            IsUnique = referenceAttribute.IsUnique;
            MissingValue = referenceAttribute.MissingValue;
            MissingValueInternal = referenceAttribute.MissingValueInternal;
            NumberOfDecimals = referenceAttribute.NumberOfDecimals;
            IsDecision = referenceAttribute.IsDecision;
            IsIdentifier = referenceAttribute.IsIdentifier;
            IsStandard = referenceAttribute.IsStandard;
            IsWeight = referenceAttribute.IsWeight;
            IsSystem = referenceAttribute.IsSystem;
        }

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
            this.histogramWeights = new Histogram<long>();

            this.HasMissingValues = false;
        }

        public bool CanDiscretize()
        {
            if (this.IsUnique)
                return false;

            if (this.IsNumeric && this.InternalValues().Count > 1)
                return true;

            return false;
        }

        public static bool IsNumericType(Type t)
        {
            if (t == typeof(int) || t == typeof(long)
                || t == typeof(double) || t == typeof(decimal) || t == typeof(float) 
                || t == typeof(short) || t == typeof(uint) || t == typeof(ulong) || t == typeof(ushort))
                return true;

            return false;
        }

        public void InitFromDataFieldInfo(AttributeInfo dataFieldInfo, bool initValues, bool initMissingValues)
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
                            continue;
                        }
                        else
                        {
                            this.AddInternal(kvp.Key, kvp.Value, false);
                        }
                    }
                }

                this.maxValueInternalId = dataFieldInfo.maxValueInternalId;
            }

            this.DataType = dataFieldInfo.DataType;
            this.Name = dataFieldInfo.Name;
            this.Alias = dataFieldInfo.Alias;
            this.Id = dataFieldInfo.Id;

            this.IsNumeric = dataFieldInfo.IsNumeric;
            this.IsUnique = dataFieldInfo.IsUnique;
            this.IsSymbolic = dataFieldInfo.IsSymbolic;
            this.IsOrdered = dataFieldInfo.IsOrdered;

            this.initialNumberOfValues = dataFieldInfo.initialNumberOfValues;
            this.NumberOfDecimals = dataFieldInfo.NumberOfDecimals;

            if (initMissingValues)
            {
                this.HasMissingValues = dataFieldInfo.HasMissingValues;
                this.MissingValueInternal = dataFieldInfo.MissingValueInternal;
                this.MissingValue = dataFieldInfo.MissingValue;
            }
        }

        public long External2Internal(object externalValue)
        {
            if (this.HasMissingValues && this.MissingValue != null && this.MissingValue.Equals(externalValue))
            {
                return this.MissingValueInternal;
            }

            if (this.IsNumeric)
            {
                switch (Type.GetTypeCode(externalValue.GetType()))
                {
                    case TypeCode.Int32:
                        return (long)(int)externalValue;

                    case TypeCode.Int64:
                        return (long)externalValue;

                    case TypeCode.Double:
                        double tmp = (double)externalValue;
                        return (long)(tmp * System.Math.Pow(10, this.NumberOfDecimals));
                }
            }
            else
            {
                long internalValue;
                if (valueDictionary.TryGetValue(externalValue, out internalValue))
                {
                    return internalValue;
                }
            }

            throw new InvalidOperationException(String.Format("Unknown external value {0}", externalValue));
        }

        public object Internal2External(long internalValue)
        {
            if (this.HasMissingValues 
                && this.MissingValue != null 
                && this.MissingValueInternal == internalValue)
            {
                return this.MissingValue;
            }

            if (this.IsNumeric)
            {
                switch (Type.GetTypeCode(this.DataType))
                {
                    case TypeCode.Double: return internalValue.ConvertToDouble(this.NumberOfDecimals);
                    case TypeCode.Int32: return internalValue.ConvertToInt(this.NumberOfDecimals);
                    case TypeCode.Int64: return internalValue;
                }
            }
            else
            {
                object externalValue;
                if (indexDictionary.TryGetValue(internalValue, out externalValue))
                {
                    return externalValue;
                }
            }

            throw new InvalidOperationException(String.Format("Unknown internal value: {0}", internalValue));
        }

        public long Add(object value, bool isMissing)
        {
            if (isMissing)
            {
                if (this.MissingValue == null)
                {
                    this.MissingValue = value;
                    this.MissingValueInternal = AttributeInfo.DefaultMissingValue;
                }
                else if (!this.MissingValue.Equals(value))
                {
                    throw new InvalidOperationException(String.Format("Missing key is already set. Trying to substitute current key {0} with {1}", this.MissingValue, value));
                }

                return this.MissingValueInternal;
            }

            if (this.IsNumeric)
            {                 
                long internalValue = this.External2Internal(value);
                if (!indexDictionary.ContainsKey(internalValue))
                {
                    valueDictionary.Add(value, internalValue);
                    indexDictionary.Add(internalValue, value);

                    if (internalValue > maxValueInternalId)
                        maxValueInternalId = internalValue;
                }

                return internalValue;
            }
            
            if (!valueDictionary.ContainsKey(value))
            {                                
                maxValueInternalId++;
                valueDictionary.Add(value, maxValueInternalId);
                indexDictionary.Add(maxValueInternalId, value);

                return maxValueInternalId;
            }

            if (isMissing)
            {
                if (this.MissingValue == null)
                    throw new InvalidOperationException(String.Format("Value {0} was indicated as missing key, while the existing missing key is null", value));
                else if (!this.MissingValue.Equals(value))
                    throw new InvalidOperationException(String.Format("Value {0} was indicated as missing key, while the existing missing key is {1}", value, this.MissingValue));
            }

            return this.External2Internal(value);
        }
        
        public void AddInternal(long internalValue, object externalValue, bool isMissing)
        {                        
            //TODO in case we just copy values from existing FieldInfo we need to increase maxValueInternalId
            //if (!valueDictionary.ContainsKey(externalValue))
            if(!indexDictionary.ContainsKey(internalValue))
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
                    throw new InvalidOperationException(String.Format("Value {0} was indicated as missing key, while the existing missing key is {1}", externalValue, this.MissingValue));
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

        public void IncreaseHistogramWeightsCount(long value, double weight)
        {
            histogramWeights.Increase(value, weight);
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
            this.histogramWeights = this.histogramWeights != null
                                  ? new Histogram<long>(histogramWeights.Count)
                                  : histogramWeights = new Histogram<long>();            
            int fieldIdx = data.DataStoreInfo.GetFieldIndex(this.Id);
            for (int i = 0; i < data.NumberOfRecords; i++)
                histogramWeights.Increase(data.GetFieldIndexValue(i, fieldIdx), weights[i]);
        }

        #endregion Histogram Methods

        #endregion Methods
    }
}