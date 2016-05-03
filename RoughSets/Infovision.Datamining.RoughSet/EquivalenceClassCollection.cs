using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
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
        //private Dictionary<long, decimal> decisionWeight;
        //private Dictionary<long, int> decisionCount;

        #endregion

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

        public int NumberOfPartitions
        {
            get { return partitions.Count; }
        }

        public int[] Attributes
        {
            get { return this.attributes; }
        }

        internal decimal CountWeightObjects { get; set; }
        internal int CountObjects { get; set; }
        
        #endregion

        #region Constructors        

        //public EquivalenceClassCollection(DataStore data, int[] attr, int numberOfDecisionValues = 0)
        public EquivalenceClassCollection(DataStore data, int[] attr)
        {
            this.data = data;
            this.attributes = new int[attr.Length];
            Array.Copy(attr, this.attributes, attr.Length);
            this.InitPartitions();
            
            /*
            if(numberOfDecisionValues > 0)
            {
                this.decisionWeight = new Dictionary<long, decimal>(numberOfDecisionValues);
                this.decisionCount = new Dictionary<long, int>(numberOfDecisionValues);
            }
            else
            {
                this.decisionWeight = new Dictionary<long, decimal>();
                this.decisionCount = new Dictionary<long, int>();
            }
            */
        }

        public EquivalenceClassCollection(DataStore data)
        {
            this.data = data;
            //this.decisionWeight = new Dictionary<long, decimal>(data.DataStoreInfo.GetDecisionValues().Count);
            //this.decisionCount = new Dictionary<long, int>(data.DataStoreInfo.GetDecisionValues().Count);
        }

        private EquivalenceClassCollection(EquivalenceClassCollection eqClassCollection)
        {
            this.data = eqClassCollection.data;
            this.partitions = (Dictionary<long[], EquivalenceClass>)eqClassCollection.Partitions.CloneDictionaryCloningValues<long[], EquivalenceClass>();
            this.attributes = new int[eqClassCollection.Attributes.Length];
            Array.Copy(eqClassCollection.Attributes, this.attributes, eqClassCollection.Attributes.Length);
            //this.decisionCount = new Dictionary<long, int>(eqClassCollection.decisionCount);
            //this.decisionWeight = new Dictionary<long, decimal>(eqClassCollection.decisionWeight);
        }        

        #endregion

        #region Methods

        #region MethodsInProgress               
        
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
            if(this.Partitions.TryGetValue(internalValueVector, out eqClass))
                return eqClass.WeightSum;
            return 0;
        }
        
        /*
        //|X|
        public int CountDecision(long decisionInternalValue)
        {            
            int count = 0;
            if(this.decisionCount.TryGetValue(decisionInternalValue, out count))
                return count;
            return 0;
        }
        */
        
        /*
        //|X|w
        public decimal CountWeightDecision(long decisionInternalValue)
        {
            decimal w = 0;
            if(this.decisionWeight.TryGetValue(decisionInternalValue, out w))
                return w;
            return 0;
        }
        */

        //|X,E|
        public int CountDecisionInEquivalenceClass(long[] internalValueVector, long decisionInternalValue)
        {
            EquivalenceClass eqClass = null;
            if(this.Partitions.TryGetValue(internalValueVector, out eqClass))
                return eqClass.GetNumberOfObjectsWithDecision(decisionInternalValue);
            return 0;
        }
        
        //|X,E|w
        public decimal CountWeightDecisionInEquivalenceClass(long[] internalValueVector, long decisionInternalValue)
        {
            EquivalenceClass eqClass = null;
            if(this.Partitions.TryGetValue(internalValueVector, out eqClass))
                return eqClass.GetDecisionWeight(decisionInternalValue);
            return 0;
        }        

        #endregion

        public static EquivalenceClassCollection Create(IReduct reduct, DataStore data, decimal[] weights = null, bool useGlobalCache = false)
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

        public static EquivalenceClassCollection Create(int[] attributes, DataStore dataStore, decimal epsilon, decimal[] weights = null)
        {            
            //EquivalenceClassCollection eqClassCollection = new EquivalenceClassCollection(dataStore, attributes, dataStore.DataStoreInfo.NumberOfDecisionValues);
            EquivalenceClassCollection eqClassCollection = new EquivalenceClassCollection(dataStore, attributes);
            int decisionIdx = dataStore.DataStoreInfo.DecisionFieldIndex;

            if (weights == null)
            {
                decimal w = Decimal.Divide(1, dataStore.NumberOfRecords);
                for(int i = 0; i < dataStore.NumberOfRecords; i++)
                {                    
                    eqClassCollection.AddRecordInitial(dataStore.GetFieldValues(i, attributes),
                                                        dataStore.GetFieldIndexValue(i, decisionIdx),
                                                        w,
                                                        dataStore,
                                                        i);
                }

                eqClassCollection.CountObjects = dataStore.NumberOfRecords;
                eqClassCollection.CountWeightObjects = Decimal.One;
                
            }
            else
            {
                decimal sum = 0;
                for (int i = 0; i < dataStore.NumberOfRecords; i++)                
                {                    
                    eqClassCollection.AddRecordInitial(dataStore.GetFieldValues(i, attributes),
                                                        dataStore.GetFieldIndexValue(i, decisionIdx),
                                                        weights[i],
                                                        dataStore,
                                                        i);                                                        

                    sum += weights[i];
                }

                eqClassCollection.CountObjects = dataStore.NumberOfRecords;
                eqClassCollection.CountWeightObjects = sum;                               
            }

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
                    eq = new EquivalenceClass(attributeInternalValues, dataStore);
                    this.partitions.Add(attributeInternalValues, eq);
                }

                if (objectIdx != -1)
                    eq.AddObject(objectIdx, decisionInternalValue, objectWeight);
                else
                    eq.AddDecision(decisionInternalValue, objectWeight);

                //int count = 0;                
                //this.decisionCount[decisionInternalValue] = this.decisionCount.TryGetValue(decisionInternalValue, out count) ? ++count : 1;
                //decimal w = 0;
                //this.decisionWeight[decisionInternalValue] = this.decisionWeight.TryGetValue(decisionInternalValue, out w) ? (w + objectWeight) : objectWeight;
            }
        }

        protected void InitPartitions(int initialSize = 0)
        {
            if (initialSize > 0)
                this.partitions = new Dictionary<long[], EquivalenceClass>(initialSize, new Int64ArrayEqualityComparer());
            else
                this.partitions = new Dictionary<long[], EquivalenceClass>(new Int64ArrayEqualityComparer());

            this.CountObjects = 0;
            this.CountWeightObjects = 0;
        }

        public virtual void Calc(FieldSet attributeSet, DataStore dataStore)
        {
            this.InitPartitions();
            
            this.attributes = attributeSet.ToArray();
            decimal w = Decimal.Divide(Decimal.One, dataStore.NumberOfRecords);                        
            for(int objectIdx = 0; objectIdx < dataStore.NumberOfRecords; objectIdx++)
            {
                this.UpdateStatistic(this.attributes, dataStore, objectIdx, w);
            }

            this.CountObjects = dataStore.NumberOfRecords;
            this.CountWeightObjects = Decimal.One;
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

            this.CountObjects = dataStore.NumberOfRecords;
            this.CountWeightObjects = sum;

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
            long[] record = dataStore.GetFieldValues(objectIndex, attributeArray);
            EquivalenceClass eqClass = null;
            long decisionValue = dataStore.GetDecisionValue(objectIndex);           
            //int count = 0;
            //decimal w = 0;
            
            lock (mutex)
            {            
                //this.decisionCount[decisionValue] = this.decisionCount.TryGetValue(decisionValue, out count) ? ++count : 1;
                //this.decisionWeight[decisionValue] = this.decisionWeight.TryGetValue(decisionValue, out w) ? (w + objectWeight) : objectWeight;

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

        public static bool CheckRegionPositive(FieldSet attributeSet, DataStore dataStore, ObjectSet objectSet, decimal[] objectWeights)
        {
            //TODO Capacity
            var localPartitions = new Dictionary<long[], EquivalenceClass>(new Int64ArrayEqualityComparer());
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
            //int numOfDec = data.DataStoreInfo.NumberOfDecisionValues;
            //this.decisionCount = new Dictionary<long, int>(numOfDec);
            //this.decisionWeight = new Dictionary<long, decimal>(numOfDec);

            foreach(var eq in this)
            {
                eq.RecalcStatistics(data);

                /*
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
                */

                eq.AvgConfidenceWeight = eq.DecisionWeights.FindMaxValuePair().Value;
                eq.AvgConfidenceSum = eq.DecisionCount.FindMaxValuePair().Value;
            }            
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
        #endregion

        #region ICloneable Members
        /// <summary>
        /// Clones the EquivalenceClassCollection, performing a deep copy.
        /// </summary>
        /// <returns>A new newInstance of a EquivalenceClassCollection, using a deep copy.</returns>
        public virtual object Clone()
        {
            return new EquivalenceClassCollection(this);
        }
        #endregion

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
            for (int i = 0; i < this.Attributes.Length; i++ )
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

        #endregion
        #endregion
    }
}
