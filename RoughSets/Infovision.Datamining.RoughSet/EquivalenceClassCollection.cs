using System;
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

        private Dictionary<long, double> decisionWeight;
        private Dictionary<long, int> decisionCount;

        #endregion Members

        #region Properties

        /*
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
        */

        public Dictionary<long, double> DecisionWeights
        {
            get { return this.decisionWeight; }
        }

        /// <summary>
        /// Returns number of partitions, which is equivalent to number of rules
        /// </summary>
        public int Count
        {
            get { return partitions.Count; }
        }

        /// <summary>
        /// returns attributes on which eqivalence classes are base
        /// </summary>
        public int[] Attributes
        {
            get { return this.attributes; }
        }

        /// <summary>
        /// Indexes of all trainig dataset objects that belong to collection of equivalence classes
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

        public DataStore Data { get { return this.data; } }

        #endregion Properties

        #region Constructors

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

        private EquivalenceClassCollection(EquivalenceClassCollection eqClassCollection)
        {
            this.data = eqClassCollection.data;

            this.decisionCount = new Dictionary<long, int>(eqClassCollection.decisionCount);
            this.decisionWeight = new Dictionary<long, double>(eqClassCollection.decisionWeight);

            this.attributes = new int[eqClassCollection.Attributes.Length];
            Array.Copy(eqClassCollection.Attributes, this.attributes, eqClassCollection.Attributes.Length);

            this.partitions = (Dictionary<long[], EquivalenceClass>)eqClassCollection.partitions.CloneDictionaryCloningValues<long[], EquivalenceClass>();
        }

        #endregion Constructors

        #region Methods

        #region MethodsUnderDevelopment

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

        public static EquivalenceClassCollection Create(IReduct reduct, DataStore data, double[] weights, ObjectSet objectSet)
        {
            EquivalenceClassCollection result = new EquivalenceClassCollection(data);
            result.Calc(reduct.Attributes, data, objectSet, weights);
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
                    //eqClassCollection.Data.GetFieldIndexValues(kvp.Key, attributesIdx, ref cursor);
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

        public static EquivalenceClassCollection CreateFromBinaryPartition(int attributeId, int[] idx1, int[] idx2, DataStore data)
        {                        
            EquivalenceClassCollection result = new EquivalenceClassCollection(data, new int[] { attributeId }, 2);
            
            if (idx1 != null && idx1.Length > 0)
            {
                long[] cursor1 = new long[] { 1 };
                EquivalenceClass eq1 = new EquivalenceClass(cursor1, data);
                result.Add(eq1);
                for (int i = 0; i < idx1.Length; i++)
                {
                    double w = data.GetWeight(idx1[i]);
                    long dec = data.GetDecisionValue(idx1[i]);
                    eq1.AddObject(idx1[i], dec, w);
                    result.AddDecision(dec, w);
                }
            }

            if (idx2 != null && idx2.Length > 0)
            {
                long[] cursor2 = new long[] { 2 };
                EquivalenceClass eq2 = new EquivalenceClass(cursor2, data);
                result.Add(eq2);
                for (int i = 0; i < idx2.Length; i++)
                {
                    double w = data.GetWeight(idx2[i]);
                    long dec = data.GetDecisionValue(idx2[i]);
                    eq2.AddObject(idx2[i], dec, w);
                    result.AddDecision(dec, w);
                }
            }
           
            return result;
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
            double objectWeight,
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

        public virtual void Calc(HashSet<int> attributeSet, DataStore dataStore, double[] objectWeights)
        {
            this.InitPartitions();
            this.attributes = attributeSet.ToArray();
            double sum = 0;
            for (int objectIdx = 0; objectIdx < dataStore.NumberOfRecords; objectIdx++)
            {
                this.UpdateStatistic(this.attributes, dataStore, objectIdx, objectWeights[objectIdx]);
                sum += objectWeights[objectIdx];
            }

            this.NumberOfObjects = dataStore.NumberOfRecords;
            this.WeightSum = sum;

            this.CalcAvgConfidence();
        }

        public virtual void Calc(HashSet<int> attributeSet, DataStore dataStore, ObjectSet objectSet, double[] objectWeights)
        {
            this.InitPartitions();
            this.attributes = attributeSet.ToArray();

            foreach (int objectIdx in objectSet)
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

        public static bool CheckRegionPositive(HashSet<int> attributeSet, DataStore dataStore, ObjectSet objectSet)
        {
            double[] weights = new double[dataStore.NumberOfRecords];
            for (int i = 0; i < weights.Length; i++)
                weights[i] = 1.0 / dataStore.NumberOfRecords;
            return EquivalenceClassCollection.CheckRegionPositive(attributeSet, dataStore, objectSet, weights);
        }

        public static bool CheckRegionPositive(
            HashSet<int> attributeSet, DataStore dataStore, ObjectSet objectSet, double[] objectWeights)
        {
            var localPartitions = new Dictionary<long[], EquivalenceClass>(Int64ArrayEqualityComparer.Instance);
            int[] attributeArray = attributeSet.ToArray();
            foreach (int objectIndex in objectSet)
            {
                long[] record = dataStore.GetFieldValues(objectIndex, attributeArray);
                EquivalenceClass reductStatistic = null;
                if (localPartitions.TryGetValue(record, out reductStatistic))
                {
                    reductStatistic.AddObject(objectIndex,
                        dataStore.GetDecisionValue(objectIndex),
                        objectWeights[objectIndex]);

                    if (reductStatistic.NumberOfDecisions > 1)
                        return false;
                }
                else
                {
                    reductStatistic = new EquivalenceClass(record, dataStore);
                    reductStatistic.AddObject(objectIndex,
                        dataStore.GetDecisionValue(objectIndex),
                        objectWeights[objectIndex]);

                    localPartitions[record] = reductStatistic;
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

                eq.AvgConfidenceWeight = eq.DecisionWeights.FindMaxValuePair().Value;
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
                tmpCollection.decisionWeight = new Dictionary<long, double>(newEqClass.DecisionWeights);
                tmpCollection.WeightSum = newEqClass.WeightSum;
                tmpCollection.NumberOfObjects = newEqClass.NumberOfObjects;
                tmpCollection.CalcAvgConfidence();
            }

            return result;
        }        

        /// <summary>
        /// Returns decision id and its weight when only a single decision exist with non-zero weight, otherwise -1 is returned
        /// </summary>
        /// <returns>decision id or -1 and zero weight</returns>
        public KeyValuePair<long, double> GetSingleDecision()
        {
            int countDec = 0;
            double minValue = 0.0;
            KeyValuePair<long, double> result = new KeyValuePair<long, double>(-1, 0.0);
            foreach (var kvp in this.DecisionWeights)
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

            return new KeyValuePair<long, double>(-1, 0.0);
        }

        public bool HasSingleDecision()
        {
            if (this.GetSingleDecision().Key != -1)
                return true;
            return false;
        }

        public EquivalenceClass[] ToArray()
        {
            return this.partitions.Values.ToArray();
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