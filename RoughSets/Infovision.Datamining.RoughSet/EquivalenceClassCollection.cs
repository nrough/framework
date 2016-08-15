﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Infovision.Data;
using Infovision.Utils;

namespace Infovision.Datamining.Roughset
{
    [Serializable]
    public class EquivalenceClassCollection : IEnumerable<EquivalenceClass>, ICloneable
    {
        #region Members

        private DataStore data;
        private Dictionary<long[], EquivalenceClass> partitions;
        private int[] attributes;
        private object mutex = new object();

        private Dictionary<long, decimal> decisionWeight;
        private Dictionary<long, int> decisionCount;

        #endregion Members

        #region Properties

        public Dictionary<long[], EquivalenceClass> Partitions
        {
            get
            {
                if (this.partitions == null)
                    this.InitPartitions();
                return partitions;
            }
            protected set { this.partitions = value; }
        }

        public Dictionary<long, decimal> DecisionWeights
        {
            get { return this.decisionWeight; }
        }

        public int NumberOfPartitions
        {
            get { return partitions.Count; }
        }

        public int[] Attributes
        {
            get { return this.attributes; }
        }

        internal decimal WeightSum { get; set; }
        internal int ObjectsCount { get; set; }

        public DataStore Data { get { return this.data; } }

        public PascalSet<long> DecisionSet
        {
            get
            {
                return new PascalSet<long>(
                    this.data.DataStoreInfo.DecisionInfo.MinValue,
                    this.data.DataStoreInfo.DecisionInfo.MaxValue,
                    this.decisionCount.Where(kvp => kvp.Value != 0).Select(kvp => kvp.Key));
            }
        }

        #endregion Properties

        #region Constructors

        public EquivalenceClassCollection(DataStore data, int[] attrCopy, int initialPartitionSize)
        {
            this.data = data;
            int decisionCount = data.DataStoreInfo.GetDecisionValues().Count;
            this.decisionWeight = new Dictionary<long, decimal>(decisionCount);
            this.decisionCount = new Dictionary<long, int>(decisionCount);
            this.attributes = attrCopy;
            this.InitPartitions(initialPartitionSize);
        }

        public EquivalenceClassCollection(DataStore data, int[] attr)
        {
            this.data = data;
            int numOfDecisions = data.DataStoreInfo.GetDecisionValues().Count;
            this.decisionWeight = new Dictionary<long, decimal>(numOfDecisions);
            this.decisionCount = new Dictionary<long, int>(numOfDecisions);
            this.attributes = new int[attr.Length];
            Array.Copy(attr, this.attributes, attr.Length);
            this.InitPartitions();
        }

        public EquivalenceClassCollection(DataStore data)
        {
            this.data = data;

            this.decisionWeight = new Dictionary<long, decimal>(data.DataStoreInfo.GetDecisionValues().Count);
            this.decisionCount = new Dictionary<long, int>(data.DataStoreInfo.GetDecisionValues().Count);
        }

        private EquivalenceClassCollection(EquivalenceClassCollection eqClassCollection)
        {
            this.data = eqClassCollection.data;

            this.decisionCount = new Dictionary<long, int>(eqClassCollection.decisionCount);
            this.decisionWeight = new Dictionary<long, decimal>(eqClassCollection.decisionWeight);

            this.attributes = new int[eqClassCollection.Attributes.Length];
            Array.Copy(eqClassCollection.Attributes, this.attributes, eqClassCollection.Attributes.Length);

            this.partitions = (Dictionary<long[], EquivalenceClass>)eqClassCollection.Partitions.CloneDictionaryCloningValues<long[], EquivalenceClass>();
        }

        #endregion Constructors

        #region Methods

        #region MethodsInProgress

        //|U| (Returns number of suppoerted objects (may differ from all number of records in dataset because of exception rules)
        public int CountSupportedObjects()
        {
            int sum = 0;
            foreach (var e in this)
                sum += e.NumberOfObjects;
            return sum;
        }

        //|E|
        public int CountEquivalenceClass(long[] internalValueVector)
        {
            EquivalenceClass eqClass = null;
            if (this.Partitions.TryGetValue(internalValueVector, out eqClass))
                return eqClass.Instances.Keys.Count;
            return 0;
        }

        //|E|w
        public decimal CountWeightEquivalenceClass(long[] internalValueVector)
        {
            EquivalenceClass eqClass = null;
            if (this.Partitions.TryGetValue(internalValueVector, out eqClass))
                return eqClass.WeightSum;
            return 0;
        }

        //|X|
        public int CountDecision(long decisionInternalValue)
        {
            int count = 0;
            if (this.decisionCount.TryGetValue(decisionInternalValue, out count))
                return count;
            return 0;
        }

        //|X|w
        public decimal CountWeightDecision(long decisionInternalValue)
        {
            decimal w = 0;
            if (this.decisionWeight.TryGetValue(decisionInternalValue, out w))
                return w;
            return 0;
        }

        //|X,E|
        public int CountDecisionInEquivalenceClass(long[] internalValueVector, long decisionInternalValue)
        {
            EquivalenceClass eqClass = null;
            if (this.Partitions.TryGetValue(internalValueVector, out eqClass))
                return eqClass.GetNumberOfObjectsWithDecision(decisionInternalValue);
            return 0;
        }

        //|X,E|w
        public decimal CountWeightDecisionInEquivalenceClass(long[] internalValueVector, long decisionInternalValue)
        {
            EquivalenceClass eqClass = null;
            if (this.Partitions.TryGetValue(internalValueVector, out eqClass))
                return eqClass.GetDecisionWeight(decisionInternalValue);
            return 0;
        }

        #endregion MethodsInProgress

        public static EquivalenceClassCollection DEL_Create(IReduct reduct, DataStore data, decimal[] weights = null, bool useGlobalCache = false)
        {
            EquivalenceClassCollection result = null;
            string partitionKey = null;

            if (useGlobalCache)
            {
                partitionKey = reduct.ReductPartitionCacheKey;
                result = ReductCache.Instance.Get(partitionKey) as EquivalenceClassCollection;
            }

            if (result == null)
            {
                result = new EquivalenceClassCollection(data);
                result.Calc(reduct.Attributes, data, weights);
            }

            if (useGlobalCache)
                ReductCache.Instance.Set(partitionKey, result);

            return result;
        }

        public static EquivalenceClassCollection Create(IReduct reduct, DataStore data, decimal[] weights, ObjectSet objectSet)
        {
            EquivalenceClassCollection result = new EquivalenceClassCollection(data);
            result.Calc(reduct.Attributes, data, objectSet, weights);
            return result;
        }

        public static EquivalenceClassCollection Create(int[] attributes, DataStore data, decimal[] weights = null)
        {
            EquivalenceClassCollection eqClassCollection = new EquivalenceClassCollection(data, attributes);

            int decisionIdx = data.DataStoreInfo.DecisionFieldIndex;

            int[] attributesIdx = new int[attributes.Length];
            for (int i = 0; i < attributes.Length; i++)
                attributesIdx[i] = data.DataStoreInfo.GetFieldIndex(attributes[i]);

            if (weights == null)
                weights = data.Weights;

            long[] cursor = new long[attributes.Length];
            decimal sum = 0;
            for (int i = 0; i < data.NumberOfRecords; i++)
            {
                data.GetFieldIndexValues(i, attributesIdx, ref cursor);
                eqClassCollection.AddRecordInitial(cursor,
                                                   data.GetFieldIndexValue(i, decisionIdx),
                                                   weights[i], data, i);
                sum += weights[i];
            }

            eqClassCollection.ObjectsCount = data.NumberOfRecords;
            eqClassCollection.WeightSum = sum;
            eqClassCollection.CalcAvgConfidence();
            return eqClassCollection;
        }

        private void CalcAvgConfidence()
        {
            foreach (EquivalenceClass eq in this)
            {
                eq.AvgConfidenceWeight = eq.DecisionWeights.FindMaxValuePair().Value;
                eq.AvgConfidenceSum = eq.DecisionCount.FindMaxValuePair().Value;
            }
        }

        protected void AddRecordInitial(
            long[] attributeInternalValues,
            long decisionInternalValue,
            decimal objectWeight,
            DataStore dataStore,
            int objectIdx = -1)
        {
            lock (mutex)
            {
                EquivalenceClass eq = null;
                if (!this.partitions.TryGetValue(attributeInternalValues, out eq))
                {
                    long[] attributeIntValuesCopy = new long[attributeInternalValues.Length];
                    Array.Copy(attributeInternalValues, attributeIntValuesCopy, attributeInternalValues.Length);
                    eq = new EquivalenceClass(attributeIntValuesCopy, dataStore);
                    this.partitions.Add(attributeIntValuesCopy, eq);
                }

                if (objectIdx != -1)
                    eq.AddObject(objectIdx, decisionInternalValue, objectWeight);
                else
                    eq.AddDecision(decisionInternalValue, objectWeight);

                int count = 0;
                this.decisionCount[decisionInternalValue]
                    = this.decisionCount.TryGetValue(decisionInternalValue, out count) ? ++count : 1;
                decimal w = 0;
                this.decisionWeight[decisionInternalValue]
                    = this.decisionWeight.TryGetValue(decisionInternalValue, out w) ? (w + objectWeight) : objectWeight;
            }
        }

        protected void InitPartitions(int initialSize = 0)
        {
            if (initialSize > 0)
                this.partitions = new Dictionary<long[], EquivalenceClass>(initialSize, Int64ArrayEqualityComparer.Instance);
            else
                this.partitions = new Dictionary<long[], EquivalenceClass>(Int64ArrayEqualityComparer.Instance);

            this.ObjectsCount = 0;
            this.WeightSum = 0;
        }

        public virtual void Calc(FieldSet attributeSet, DataStore dataStore)
        {
            this.InitPartitions();

            this.attributes = attributeSet.ToArray();
            decimal w = Decimal.Divide(Decimal.One, dataStore.NumberOfRecords);
            for (int objectIdx = 0; objectIdx < dataStore.NumberOfRecords; objectIdx++)
            {
                this.UpdateStatistic(this.attributes, dataStore, objectIdx, w);
            }

            this.ObjectsCount = dataStore.NumberOfRecords;
            this.WeightSum = Decimal.One;
        }

        public virtual void Calc(FieldSet attributeSet, DataStore dataStore, decimal[] objectWeights)
        {
            this.InitPartitions();
            this.attributes = attributeSet.ToArray();
            decimal sum = 0;
            for (int objectIdx = 0; objectIdx < dataStore.NumberOfRecords; objectIdx++)
            {
                this.UpdateStatistic(this.attributes, dataStore, objectIdx, objectWeights[objectIdx]);
                sum += objectWeights[objectIdx];
            }

            this.ObjectsCount = dataStore.NumberOfRecords;
            this.WeightSum = sum;

            this.CalcAvgConfidence();
        }

        public virtual void Calc(FieldSet attributeSet, DataStore dataStore, ObjectSet objectSet, decimal[] objectWeights)
        {
            this.InitPartitions();
            this.attributes = attributeSet.ToArray();

            foreach (int objectIdx in objectSet)
                this.UpdateStatistic(this.attributes, dataStore, objectIdx, objectWeights[objectIdx]);
        }

        private void UpdateStatistic(int[] attributeArray, DataStore dataStore, int objectIndex, decimal objectWeight)
        {
            lock (mutex)
            {
                EquivalenceClass eqClass = null;
                long[] record = dataStore.GetFieldValues(objectIndex, attributeArray);
                long decisionValue = dataStore.GetDecisionValue(objectIndex);

                int count = 0;
                this.decisionCount[decisionValue]
                    = this.decisionCount.TryGetValue(decisionValue, out count) ? ++count : 1;
                decimal w = 0;
                this.decisionWeight[decisionValue]
                    = this.decisionWeight.TryGetValue(decisionValue, out w) ? (w + objectWeight) : objectWeight;

                if (this.partitions.TryGetValue(record, out eqClass))
                {
                    eqClass.AddObject(objectIndex, decisionValue, objectWeight);
                }
                else
                {
                    eqClass = new EquivalenceClass(record, dataStore);
                    eqClass.AddObject(objectIndex, decisionValue, objectWeight);
                    this.partitions.Add(record, eqClass);
                }
            }
        }

        public static bool CheckRegionPositive(FieldSet attributeSet, DataStore dataStore, ObjectSet objectSet)
        {
            decimal[] weights = new decimal[dataStore.NumberOfRecords];
            for (int i = 0; i < weights.Length; i++)
                weights[i] = Decimal.Divide(Decimal.One, dataStore.NumberOfRecords);
            return EquivalenceClassCollection.CheckRegionPositive(attributeSet, dataStore, objectSet, weights);
        }

        public static bool CheckRegionPositive(
            FieldSet attributeSet, DataStore dataStore, ObjectSet objectSet, decimal[] objectWeights)
        {
            //TODO Capacity
            var localPartitions = new Dictionary<long[], EquivalenceClass>(Int64ArrayEqualityComparer.Instance);
            int[] attributeArray = attributeSet.ToArray();
            int decisionIndex = dataStore.DataStoreInfo.DecisionFieldIndex;

            foreach (int objectIndex in objectSet)
            {
                long[] record = dataStore.GetFieldValues(objectIndex, attributeArray);
                EquivalenceClass reductStatistic = null;
                if (localPartitions.TryGetValue(record, out reductStatistic))
                {
                    reductStatistic.AddObject(objectIndex,
                        dataStore.GetFieldIndexValue(objectIndex, decisionIndex),
                        objectWeights[objectIndex]);

                    if (reductStatistic.NumberOfDecisions > 1)
                        return false;
                }
                else
                {
                    reductStatistic = new EquivalenceClass(record, dataStore);
                    reductStatistic.AddObject(objectIndex,
                        dataStore.GetFieldIndexValue(objectIndex, decisionIndex),
                        objectWeights[objectIndex]);

                    localPartitions[record] = reductStatistic;
                }
            }

            return true;
        }

        public EquivalenceClass GetEquivalenceClass(long[] values)
        {
            EquivalenceClass reductStatstic = null;
            if (this.partitions.TryGetValue(values, out reductStatstic))
                return reductStatstic;
            return null;
        }

        public EquivalenceClass GetEquivalenceClass(DataRecordInternal record)
        {
            long[] values = new long[this.attributes.Length];
            for (int i = 0; i < this.attributes.Length; i++)
                values[i] = record[this.attributes[i]];
            return GetEquivalenceClass(values);
        }

        public void RecalcEquivalenceClassStatistic(DataStore data)
        {
            int numOfDec = data.DataStoreInfo.NumberOfDecisionValues;
            this.decisionCount = new Dictionary<long, int>(numOfDec);
            this.decisionWeight = new Dictionary<long, decimal>(numOfDec);
            this.ObjectsCount = 0;
            this.WeightSum = 0;

            /*
            ParallelOptions options = new ParallelOptions()
            {
                MaxDegreeOfParallelism = System.Math.Max(1, Environment.ProcessorCount / 2)
            };

#if DEBUG
            options.MaxDegreeOfParallelism = 1;
#endif

            Parallel.ForEach(this, options, eq =>
            */

            foreach (var eq in this)
            {
                eq.RecalcStatistics(data);

                foreach (long decision in eq.DecisionSet)
                {
                    int count = 0;
                    this.decisionCount[decision] = this.decisionCount.TryGetValue(decision, out count)
                        ? (count + eq.GetNumberOfObjectsWithDecision(decision))
                        : eq.GetNumberOfObjectsWithDecision(decision);

                    decimal w = 0;
                    this.decisionWeight[decision] = this.decisionWeight.TryGetValue(decision, out w)
                        ? (w + eq.GetDecisionWeight(decision))
                        : eq.GetDecisionWeight(decision);
                }

                eq.AvgConfidenceWeight = eq.DecisionWeights.FindMaxValuePair().Value;
                eq.AvgConfidenceSum = eq.DecisionCount.FindMaxValuePair().Value;

                this.ObjectsCount += eq.Instances.Count;
                this.WeightSum += eq.Instances.Sum(x => x.Value);
            }
            //);
        }

        public static Dictionary<long, EquivalenceClassCollection> Split(
            EquivalenceClassCollection collectionToSplit,
            int attributeId)
        {
            int attributeIdx = collectionToSplit.Attributes.IndexOf(attributeId);

            int[] newAttributes = collectionToSplit.Attributes.RemoveAt(attributeIdx);
            DataFieldInfo attributeInfo = collectionToSplit.data.DataStoreInfo.GetFieldInfo(attributeId);
            Dictionary<long, EquivalenceClassCollection> result
                = new Dictionary<long, EquivalenceClassCollection>(attributeInfo.NumberOfValues);

            foreach (var eqClass in collectionToSplit)
            {
                long[] newInstance = eqClass.Instance.RemoveAt(attributeIdx);
                long attributeValue = eqClass.Instance[attributeIdx];
                EquivalenceClassCollection tmpCollection = null;
                if (!result.TryGetValue(attributeValue, out tmpCollection))
                {
                    //TODO Decision tries: do we know better estimation of partition size?
                    tmpCollection = new EquivalenceClassCollection(
                        collectionToSplit.data,
                        newAttributes,
                        (int)(collectionToSplit.NumberOfPartitions / attributeInfo.NumberOfValues));
                    result.Add(attributeValue, tmpCollection);
                }

                EquivalenceClass newEqClass = new EquivalenceClass(newInstance, eqClass.Instances, eqClass.DecisionSet);

                //TODO Decision tries: Update eq class statistics
                //WeigthSum is calculated in RecalcEquivalenceClassStatistic
                //newEqClass.WeightSum = eqClass.WeightSum;

                tmpCollection.Partitions.Add(newInstance, newEqClass);
            }

            //TODO Remove this Recalc, try to update stat in previous loop
            foreach (var kvp in result)
                kvp.Value.RecalcEquivalenceClassStatistic(collectionToSplit.data);

            return result;
        }

        public static EquivalenceClassCollection Create(int[] attributes, EquivalenceClassCollection eqClassCollection)
        {
            //TODO Decision tries : Update statistics

            int combinations = 1;
            for (int i = 0; i < attributes.Length; i++)
                combinations *= eqClassCollection.data.DataStoreInfo.GetFieldInfo(attributes[i]).NumberOfValues;

            EquivalenceClassCollection result = new EquivalenceClassCollection(eqClassCollection.data, attributes, combinations);
            int[] attributeIndices = eqClassCollection.Attributes.IndicesOfOrderedByValue(attributes);

            foreach (var eq in eqClassCollection)
            {
                long[] newInstance = eq.Instance.KeepIndices(attributeIndices);
                EquivalenceClass newEqClass = null;
                if (!result.Partitions.TryGetValue(newInstance, out newEqClass))
                {
                    newEqClass = new EquivalenceClass(newInstance, eq.Instances, eq.DecisionSet);
                    newEqClass.WeightSum = eq.WeightSum;
                    result.Partitions.Add(newInstance, newEqClass);
                }
                else
                {
                    newEqClass.AddObjectInstances(eq.Instances);
                    newEqClass.WeightSum += eq.WeightSum;
                    newEqClass.DecisionSet = newEqClass.DecisionSet.UnionFast(eq.DecisionSet);
                }

                result.ObjectsCount += eq.Instances.Count;
                result.WeightSum += eq.Instances.Sum(x => x.Value);
            }

            return result;
        }

        #region IEnumerable Members

        /// <summary>
        /// Returns an IEnumerator to enumerate through the partition map.
        /// </summary>
        /// <returns>An IEnumerator newInstance.</returns>
        public IEnumerator<EquivalenceClass> GetEnumerator()
        {
            return partitions.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return (IEnumerator)GetEnumerator();
        }

        #endregion IEnumerable Members

        #region ICloneable Members

        /// <summary>
        /// Clones the EquivalenceClassCollection, performing a deep copy.
        /// </summary>
        /// <returns>A new newInstance of a EquivalenceClassCollection, using a deep copy.</returns>
        public virtual object Clone()
        {
            return new EquivalenceClassCollection(this);
        }

        #endregion ICloneable Members

        #region System.Object Methods

        public override string ToString()
        {
            //return this.ToStringInt();
            return this.ToStringExt();
        }

        public string ToStringInt()
        {
            StringBuilder stringBuilder = new StringBuilder();
            foreach (var kvp in this.partitions)
                stringBuilder.AppendLine(kvp.Value.ToString());
            return stringBuilder.ToString();
        }

        public string ToStringExt()
        {
            StringBuilder stringBuilder = new StringBuilder();
            foreach (var kvp in this.partitions)
                stringBuilder.AppendLine(kvp.Value.ToStringExt(this.data, this.attributes));
            return stringBuilder.ToString();
        }

        public string ToString2()
        {
            StringBuilder stringBuilder = new StringBuilder();
            for (int i = 0; i < this.Attributes.Length; i++)
                stringBuilder.AppendFormat("a{0} ", this.Attributes[i]);
            stringBuilder.Append(Environment.NewLine);
            foreach (var kvp in this.partitions)
                stringBuilder.AppendLine(kvp.Value.ToString2());
            return stringBuilder.ToString();
        }

        public string ToString3()
        {
            return this.Attributes.ToStr();
        }

        #endregion System.Object Methods

        #endregion Methods
    }
}