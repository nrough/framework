using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NRough.Data;
using NRough.Core;
using NRough.MachineLearning.Classification;
using NRough.Core.CollectionExtensions;
using NRough.Core.Comparers;

namespace NRough.MachineLearning
{
    [Serializable]
    public class EquivalenceClassCollection : IEnumerable<EquivalenceClass>, ICloneable
    {
        #region Members

        private readonly object mutex = new object();
        private DataStore data;
        private Dictionary<long[], EquivalenceClass> partitions;
        private int[] attributes;        
        private Dictionary<long, double> decisionWeight;
        private Dictionary<long, int> decisionCount;

        #endregion Members

        #region Properties

        public Dictionary<long, double> DecisionWeight
        {
            get { return this.decisionWeight; }
            internal set { this.decisionWeight = value; }
        }

        public Dictionary<long, int> DecisionCount
        {
            get { return this.decisionCount; }
            internal set { this.decisionCount = value; }
        }

        public DecisionDistribution DecisionDistribution
        {
            get { return new DecisionDistribution(this.DecisionWeight); }
        }

        /// <summary>
        /// Returns number of partitions, which is equivalent to number of rules
        /// </summary>
        public int Count
        {
            get { return partitions.Count; }
        }

        /// <summary>
        /// returns attributes on which equivalence classes are base
        /// </summary>
        public int[] Attributes
        {
            get { return this.attributes; }
        }

        /// <summary>
        /// Indexes of all training dataset objects that belong to collection of equivalence classes
        /// </summary>
        public int[] Indices
        {
            get
            {
                List<int> indices = new List<int>();
                foreach (var eqClass in this)
                    indices.AddRange(eqClass.ObjectIndexes);
                return indices.ToArray();
            }
        }

        internal double WeightSum { get; set; }
        internal int NumberOfObjects { get; set; }
        public EquivalenceClass this[long[] key] { get { return this.partitions[key]; }}
        public DataStore Data { get { return this.data; } }

        #endregion Properties

        #region Constructors

        public EquivalenceClassCollection(int[] attributes)
        {            
            this.attributes = attributes.ToArray();
            this.decisionWeight = new Dictionary<long, double>();
            this.decisionCount = new Dictionary<long, int>();
            this.InitPartitions();
        }

        public EquivalenceClassCollection(DataStore data, int[] attrCopy, int initialPartitionSize)
        {
            this.data = data;
            int decisionCount = data.DataStoreInfo.GetDecisionValues().Count;
            this.decisionWeight = new Dictionary<long, double>(decisionCount);
            this.decisionCount = new Dictionary<long, int>(decisionCount);
            this.attributes = attrCopy;
            this.InitPartitions(initialPartitionSize);
        }

        public EquivalenceClassCollection(DataStore data, int[] attr)
        {
            this.data = data;
            int numOfDecisions = data.DataStoreInfo.GetDecisionValues().Count;
            this.decisionWeight = new Dictionary<long, double>(numOfDecisions);
            this.decisionCount = new Dictionary<long, int>(numOfDecisions);
            this.attributes = new int[attr.Length];
            Array.Copy(attr, this.attributes, attr.Length);
            this.InitPartitions();
        }

        public EquivalenceClassCollection(DataStore data)
        {
            this.data = data;

            this.decisionWeight = new Dictionary<long, double>(data.DataStoreInfo.GetDecisionValues().Count);
            this.decisionCount = new Dictionary<long, int>(data.DataStoreInfo.GetDecisionValues().Count);
        }

        public EquivalenceClassCollection(EquivalenceClass eqClass)
        {
            this.attributes = new int[0];
            this.InitPartitions(1);
            this.decisionWeight = new Dictionary<long, double>(eqClass.DecisionWeight);
            this.decisionCount = new Dictionary<long, int>(eqClass.DecisionCount);
            this.NumberOfObjects = eqClass.NumberOfObjects;
            this.WeightSum = eqClass.WeightSum;
            this.partitions.Add(new long[] { }, new EquivalenceClass(new long[] { }, eqClass));
        }

        /// <summary>
        /// Creates an equivalence class collection with a single equivalence class
        /// </summary>
        /// <param name="labels">decision attribute values</param>
        /// <param name="weights">object weights</param>
        public EquivalenceClassCollection(long[] labels, double[] weights, int[] sortedIndices = null)
            : this(labels, weights, 0, sortedIndices == null ? labels.Length : sortedIndices.Length, sortedIndices)
        {
        }

        /// <summary>
        /// Creates an equivalence class collection with a single equivalence class based on object index range
        /// </summary>
        /// <param name="labels">decision attribute values</param>
        /// <param name="weights">object weights</param>
        /// <param name="start">start index</param>
        /// <param name="end">end index</param>
        /// <param name="sortedIndices">list of object indexes</param>
        public EquivalenceClassCollection(
            long[] labels, double[] weights, int start, int end, int[] sortedIndices = null)
        {
            this.attributes = new int[0];
            this.InitPartitions(1);
            this.decisionWeight = new Dictionary<long, double>();
            this.decisionCount = new Dictionary<long, int>();
            this.NumberOfObjects = end - start;
            var instances = new Dictionary<int, double>(this.NumberOfObjects);
            
            if (sortedIndices == null)
            {
                for (int i = start; i < labels.Length && i < end; i++)
                {
                    if (this.decisionWeight.ContainsKey(labels[i]))
                    {
                        this.decisionCount[labels[i]] += 1;
                        this.decisionWeight[labels[i]] += (weights == null) ? 1.0 : weights[i];                        
                    }
                    else
                    {
                        this.decisionCount.Add(labels[i], 1);
                        this.decisionWeight.Add(
                            labels[i], 
                            (weights == null) ? 1.0 : weights[i]);                        
                    }

                    this.WeightSum += (weights == null) ? 1.0 : weights[i];
                    instances.Add(i, (weights == null) ? 1.0 : weights[i]);
                }
            }
            else
            {
                for (int i = start; i < labels.Length && i < end; i++)
                {
                    if (this.decisionWeight.ContainsKey(labels[sortedIndices[i]]))
                    {
                        this.decisionCount[labels[sortedIndices[i]]] += 1;
                        this.decisionWeight[labels[sortedIndices[i]]] +=
                            (weights == null) ? 1.0 : weights[sortedIndices[i]];                        
                    }
                    else
                    {
                        this.decisionCount.Add(labels[sortedIndices[i]], 1);
                        this.decisionWeight.Add(
                            labels[sortedIndices[i]], 
                            (weights == null) ? 1.0 : weights[sortedIndices[i]]);
                        
                    }

                    this.WeightSum += (weights == null) ? 1.0 : weights[sortedIndices[i]];
                    instances.Add(sortedIndices[i], (weights == null) ? 1.0 : weights[sortedIndices[i]]);
                }
            }

            
            var equivalenceClass = new EquivalenceClass(
                new long[] { }, instances, new HashSet<long>(this.decisionCount.Keys));

            equivalenceClass.DecisionWeight = new Dictionary<long, double>(this.decisionWeight);
            equivalenceClass.DecisionCount = new Dictionary<long, int>(this.decisionCount);
            equivalenceClass.WeightSum = this.WeightSum;            
            this.partitions.Add(new long[] { }, equivalenceClass);
        }

        private EquivalenceClassCollection(EquivalenceClassCollection eqClassCollection)
        {
            this.data = eqClassCollection.data;
            this.decisionCount = new Dictionary<long, int>(eqClassCollection.decisionCount);
            this.decisionWeight = new Dictionary<long, double>(eqClassCollection.decisionWeight);
            this.attributes = eqClassCollection.Attributes.ToArray();
            this.partitions = (Dictionary<long[], EquivalenceClass>)eqClassCollection.partitions.CloneDictionaryCloningValues<long[], EquivalenceClass>();
            this.WeightSum = eqClassCollection.WeightSum;
            this.NumberOfObjects = eqClassCollection.NumberOfObjects;            
        }

        #endregion Constructors

        #region Methods

        #region MethodsUnderDevelopment

        //|U| (Returns number of supported objects (may differ from all number of records in dataset because of exception rules)
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
            if (this.partitions.TryGetValue(internalValueVector, out eqClass))
                return eqClass.Instances.Keys.Count;
            return 0;
        }

        //|E|w
        public double CountWeightEquivalenceClass(long[] internalValueVector)
        {
            EquivalenceClass eqClass = null;
            if (this.partitions.TryGetValue(internalValueVector, out eqClass))
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
        public double CountWeightDecision(long decisionInternalValue)
        {
            double w = 0;
            if (this.decisionWeight.TryGetValue(decisionInternalValue, out w))
                return w;
            return 0;
        }

        //|X,E|
        public int CountDecisionInEquivalenceClass(long[] internalValueVector, long decisionInternalValue)
        {
            EquivalenceClass eqClass = null;
            if (this.partitions.TryGetValue(internalValueVector, out eqClass))
                return eqClass.GetNumberOfObjectsWithDecision(decisionInternalValue);
            return 0;
        }

        //|X,E|w
        public double CountWeightDecisionInEquivalenceClass(long[] internalValueVector, long decisionInternalValue)
        {
            EquivalenceClass eqClass = null;
            if (this.partitions.TryGetValue(internalValueVector, out eqClass))
                return eqClass.GetDecisionWeight(decisionInternalValue);
            return 0;
        }

        #endregion        

        public static EquivalenceClassCollection Create(int[] attributes, DataStore data, double[] weights, int[] objectIndices)
        {
            EquivalenceClassCollection result = new EquivalenceClassCollection(data);
            result.Calc(attributes, data, objectIndices, weights);
            return result;
        }

        public static EquivalenceClassCollection Create(int[] attributes, DataStore data, double[] weights = null)
        {
            EquivalenceClassCollection eqClassCollection = new EquivalenceClassCollection(data, attributes);
            int[] attributesIdx = new int[attributes.Length];
            for (int i = 0; i < attributes.Length; i++)
                attributesIdx[i] = data.DataStoreInfo.GetFieldIndex(attributes[i]);

            if (weights == null)
                weights = data.Weights;

            long[] cursor = new long[attributes.Length];
            double sum = 0;
            for (int i = 0; i < data.NumberOfRecords; i++)
            {
                data.GetFieldIndexValues(i, attributesIdx, ref cursor);
                eqClassCollection.AddRecordInitial(cursor,
                                                   data.GetDecisionValue(i),
                                                   weights[i], data, i);
                sum += weights[i];
            }
            eqClassCollection.NumberOfObjects = data.NumberOfRecords;
            eqClassCollection.WeightSum = sum;
            eqClassCollection.CalcAvgConfidence();

            return eqClassCollection;
        }

        public static EquivalenceClassCollection Create(int attributeId, EquivalenceClassCollection eqClassCollection)
        {
            int[] attributes = new int[] { attributeId };
            int[] attributesIdx = new int[] { eqClassCollection.data.DataStoreInfo.GetFieldIndex(attributeId) };
            long[] cursor = new long[1];

            EquivalenceClassCollection result = new EquivalenceClassCollection(
                eqClassCollection.data,
                attributes,
                eqClassCollection.data.DataStoreInfo.GetFieldInfo(attributeId).NumberOfValues);

            foreach (var eq in eqClassCollection)
            {
                foreach (var kvp in eq.Instances)
                {                    
                    cursor[0] = eqClassCollection.Data.GetFieldIndexValue(kvp.Key, attributesIdx[0]);
                    result.AddRecordInitial(cursor, eqClassCollection.Data.GetDecisionValue(kvp.Key), 
                        kvp.Value, eqClassCollection.Data, kvp.Key);
                }
            }

            result.NumberOfObjects = eqClassCollection.NumberOfObjects;
            result.WeightSum = eqClassCollection.WeightSum;
            result.CalcAvgConfidence();

            return result;
        }        

        public static EquivalenceClassCollection CreateBinaryPartition(DataStore data, int attributeId, int[] indices, long threshold)
        {
            if (!data.DataStoreInfo.GetFieldInfo(attributeId).IsNumeric)
                throw new ArgumentException("Attribute is not numeric", "attributeId");

            EquivalenceClassCollection result = new EquivalenceClassCollection(data, new int[] { attributeId }, 2);
            int attributeIdx = data.DataStoreInfo.GetFieldIndex(attributeId);

            EquivalenceClass eq1 = new EquivalenceClass(new long[] { 1 }, data);
            EquivalenceClass eq2 = new EquivalenceClass(new long[] { 2 }, data);

            double weightSum = 0;

            for (int i = 0; i < indices.Length; i++)
            {
                double w = data.GetWeight(indices[i]);
                weightSum += w;

                long dec = data.GetDecisionValue(indices[i]);
                result.AddDecision(dec, w);

                if (data.GetFieldIndexValue(indices[i], attributeIdx) <= threshold)
                    eq1.AddObject(indices[i], dec, w);
                else
                    eq2.AddObject(indices[i], dec, w);
            }

            result.Add(eq1);
            result.Add(eq2);

            result.NumberOfObjects = indices.Length;
            result.WeightSum = weightSum;
            result.CalcAvgConfidence();

            return result;
        }

        private void CalcAvgConfidence()
        {
            foreach (EquivalenceClass eq in this)
            {
                eq.AvgConfidenceWeight = eq.DecisionWeight.FindMaxValuePair().Value;
                eq.AvgConfidenceSum = eq.DecisionCount.FindMaxValuePair().Value;
            }
        }

        protected void AddRecordInitial(
            long[] attributeInternalValues, long decisionInternalValue, double objectWeight, 
            DataStore dataStore, int objectIdx = -1)
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

                this.AddDecision(decisionInternalValue, objectWeight);
            }
        }

        protected void AddDecision(long decisionInternalValue, double objectWeight)
        {
            int count = 0;
            this.decisionCount[decisionInternalValue]
                = this.decisionCount.TryGetValue(decisionInternalValue, out count) ? ++count : 1;

            double w = 0;
            this.decisionWeight[decisionInternalValue]
                = this.decisionWeight.TryGetValue(decisionInternalValue, out w) ? (w + objectWeight) : objectWeight;
        }

        protected void InitPartitions(int initialSize = 0)
        {
            if (initialSize > 0)
                this.partitions = new Dictionary<long[], EquivalenceClass>(initialSize, Int64ArrayEqualityComparer.Instance);
            else
                this.partitions = new Dictionary<long[], EquivalenceClass>(Int64ArrayEqualityComparer.Instance);

            this.NumberOfObjects = 0;
            this.WeightSum = 0;
        }

        public virtual void Calc(HashSet<int> attributeSet, DataStore dataStore)
        {
            this.InitPartitions();

            this.attributes = attributeSet.ToArray();
            double w = 1.0 / dataStore.NumberOfRecords;
            for (int objectIdx = 0; objectIdx < dataStore.NumberOfRecords; objectIdx++)
            {
                this.UpdateStatistic(this.attributes, dataStore, objectIdx, w);
            }

            this.NumberOfObjects = dataStore.NumberOfRecords;
            this.WeightSum = 1.0;
        }
        
        public virtual void Calc(int[] attr, DataStore dataStore, int[] objectIndices, double[] objectWeights)
        {
            this.InitPartitions();
            this.attributes = new int[attr.Length];
            Array.Copy(attr, this.attributes, attr.Length);

            foreach (int objectIdx in objectIndices)
                this.UpdateStatistic(this.attributes, dataStore, objectIdx, objectWeights[objectIdx]);
        }

        private void UpdateStatistic(int[] attributeArray, DataStore dataStore, int objectIndex, double objectWeight)
        {
            lock (mutex)
            {
                EquivalenceClass eqClass = null;
                long[] record = dataStore.GetFieldValues(objectIndex, attributeArray);
                long decisionValue = dataStore.GetDecisionValue(objectIndex);

                int count = 0;
                this.decisionCount[decisionValue]
                    = this.decisionCount.TryGetValue(decisionValue, out count) ? ++count : 1;
                double w = 0;
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

        public static bool CheckRegionPositive(
            int[] attributes, DataStore decisionTable, int[] objects)
        {
            var localPartitions = new Dictionary<long[], long>(Int64ArrayEqualityComparer.Instance);
            foreach (int objectIndex in objects)
            {
                long[] record = decisionTable.GetFieldValues(objectIndex, attributes);
                long decision;
                if (localPartitions.TryGetValue(record, out decision))
                {
                    if (decisionTable.GetDecisionValue(objectIndex) != decision)
                        return false;
                }
                else
                {

                    localPartitions[record] = decisionTable.GetDecisionValue(objectIndex);
                }
            }

            return true;
        }

        public EquivalenceClass Find(long[] values)
        {
            EquivalenceClass eqClass = null;
            if (this.partitions.TryGetValue(values, out eqClass))
                return eqClass;
            return null;
        }

        public EquivalenceClass Find(DataRecordInternal record)
        {
            long[] values = new long[this.attributes.Length];
            for (int i = 0; i < this.attributes.Length; i++)
                values[i] = record[this.attributes[i]];
            return Find(values);
        }

        public void Add(EquivalenceClass equivalenceClass)
        {
            this.partitions.Add(equivalenceClass.Instance, equivalenceClass);
        }

        public void RecalcEquivalenceClassStatistic(DataStore data)
        {
            int numOfDec = data.DataStoreInfo.NumberOfDecisionValues;
            this.decisionCount = new Dictionary<long, int>(numOfDec);
            this.decisionWeight = new Dictionary<long, double>(numOfDec);
            this.NumberOfObjects = 0;
            this.WeightSum = 0;

            foreach (var eq in this)
            {
                eq.RecalcStatistics(data);

                foreach (long decision in eq.DecisionSet)
                {
                    int count = 0;
                    this.decisionCount[decision] = this.decisionCount.TryGetValue(decision, out count)
                        ? (count + eq.GetNumberOfObjectsWithDecision(decision))
                        : eq.GetNumberOfObjectsWithDecision(decision);

                    double w = 0;
                    this.decisionWeight[decision] = this.decisionWeight.TryGetValue(decision, out w)
                        ? (w + eq.GetDecisionWeight(decision))
                        : eq.GetDecisionWeight(decision);
                }

                eq.AvgConfidenceWeight = eq.DecisionWeight.FindMaxValuePair().Value;
                eq.AvgConfidenceSum = eq.DecisionCount.FindMaxValuePair().Value;

                this.NumberOfObjects += eq.Instances.Count;
                this.WeightSum += eq.Instances.Sum(x => x.Value);
            }
        }

        public static Dictionary<long, EquivalenceClassCollection> Split(int attributeId, EquivalenceClassCollection collectionToSplit)
        {
            if (collectionToSplit == null)
                throw new ArgumentNullException("collectionToSplit");

            if (collectionToSplit.Attributes.Length != 1)
                throw new ArgumentException("collectionToSplit.Attributes.Length != 1", "collectionToSplit");

            Dictionary<long, EquivalenceClassCollection> result
                = new Dictionary<long, EquivalenceClassCollection>(
                    collectionToSplit.data.DataStoreInfo.GetFieldInfo(attributeId).NumberOfValues);

            foreach (var eqClass in collectionToSplit)
            {
                EquivalenceClassCollection tmpCollection = new EquivalenceClassCollection(collectionToSplit.data, new int[] { }, 1);

                result.Add(eqClass.Instance[0], tmpCollection);

                EquivalenceClass newEqClass = new EquivalenceClass(eqClass, new long[] { });

                tmpCollection.partitions.Add(newEqClass.Instance, newEqClass);
                tmpCollection.decisionCount = new Dictionary<long, int>(newEqClass.DecisionCount);
                tmpCollection.decisionWeight = new Dictionary<long, double>(newEqClass.DecisionWeight);
                tmpCollection.WeightSum = newEqClass.WeightSum;
                tmpCollection.NumberOfObjects = newEqClass.NumberOfObjects;
                tmpCollection.CalcAvgConfidence();
            }

            return result;
        }

        /// <summary>
        /// Returns decision id and its weight when only a single decision exist with non-zero weight, otherwise <c>Classifier</c>.UnclassifiedOutput is returned
        /// </summary>
        /// <returns>decision id or <c>Classifier<c>.UnclassifiedOutput and zero weight</returns>
        public KeyValuePair<long, double> GetSingleDecision()
        {
            int countDec = 0;
            double minValue = 0.0;
            KeyValuePair<long, double> result = new KeyValuePair<long, double>(Classifier.UnclassifiedOutput, 0.0);
            foreach (var kvp in this.DecisionWeight)
            {
                if (kvp.Value > 0.0)
                {
                    countDec++;
                    if (kvp.Value > minValue)
                    {
                        result = new KeyValuePair<long, double>(kvp.Key, kvp.Value);
                        minValue = kvp.Value;
                    }
                }
            }

            if (countDec == 1)
                return result;

            return new KeyValuePair<long, double>(Classifier.UnclassifiedOutput, 0.0);
        }

        public bool HasSingleDecision()
        {
            if (this.GetSingleDecision().Key != Classifier.UnclassifiedOutput)
                return true;
            return false;
        }

        public EquivalenceClass[] ToArray()
        {
            return this.partitions.Values.ToArray();
        }        

        public void AddInstance(long[] instance, long decision, double weight, int instanceIdx = -1)
        {
            this.WeightSum += weight;
            this.NumberOfObjects += 1;
            if (this.decisionCount.ContainsKey(decision))
            {
                this.decisionWeight[decision] += weight;
                this.decisionCount[decision] += 1;
            }
            else
            {
                this.decisionWeight.Add(decision, weight);
                this.decisionCount.Add(decision, 1);
            }                        

            EquivalenceClass eqClass = null;
            if (!this.partitions.TryGetValue(instance, out eqClass))
                eqClass = new EquivalenceClass(instance);
            eqClass.AddObject(instanceIdx, decision, weight);
        }

        public void RemoveInstance(long[] instance, long decision, double weight, int instanceIdx = -1)
        {
            this.WeightSum -= weight;
            this.NumberOfObjects -= 1;

            if (this.decisionCount.ContainsKey(decision))
            {
                this.decisionCount[decision] -= 1;
                this.decisionWeight[decision] -= weight;
            }                                                      

            EquivalenceClass eqClass = null;
            if (instanceIdx != -1 && this.partitions.TryGetValue(instance, out eqClass))
                eqClass.RemoveObject(instanceIdx, decision, weight);
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