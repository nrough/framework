using System;
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
        
        private Dictionary<long[], EquivalenceClass> partitions;
        private int[] attributes;
        private object mutex = new object();

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

        public decimal EqWeightSum { get; set; }
        
        #endregion

        #region Constructors        

        public EquivalenceClassCollection(int[] attr)
        {
            this.attributes = new int[attr.Length];
            Array.Copy(attr, this.attributes, attr.Length);
            this.InitPartitions();
        }

        public EquivalenceClassCollection()
        {
        }

        private EquivalenceClassCollection(EquivalenceClassCollection eqClassCollection)
        {
            this.partitions = (Dictionary<long[], EquivalenceClass>)eqClassCollection.Partitions.CloneDictionaryCloningValues<long[], EquivalenceClass>();
            this.attributes = new int[eqClassCollection.Attributes.Length];
            Array.Copy(eqClassCollection.Attributes, this.attributes, eqClassCollection.Attributes.Length);
        }        

        #endregion

        #region Methods

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
                result = new EquivalenceClassCollection();
                result.Calc(reduct.Attributes, data, weights);
            }

            if (useGlobalCache)
                ReductCache.Instance.Set(partitionKey, result);

            return result;
        }

        public static EquivalenceClassCollection Create(IReduct reduct, DataStore data, decimal[] weights, ObjectSet objectSet, bool updateStat = true)
        {
            EquivalenceClassCollection result = new EquivalenceClassCollection();
            result.Calc(reduct.Attributes, data, objectSet, weights);
            return result;
        }


        public static EquivalenceClassCollection Create(int[] attributes, DataStore dataStore, decimal epsilon, decimal[] weights = null, bool updateStat = true)
        {
            if (weights != null && dataStore.NumberOfRecords != weights.Length)
                throw new ArgumentOutOfRangeException("weights", "Weight vector must has the same length as number of records in data");

            EquivalenceClassCollection eqClassCollection = new EquivalenceClassCollection(attributes);

            if (weights == null)
            {
                decimal w = Decimal.Divide(1, dataStore.NumberOfRecords);

                //Parallel.For(0, dataStore.NumberOfRecords, i =>
                for(int i=0; i < dataStore.NumberOfRecords; i++)
                {
                    long[] attributeValues = dataStore.GetFieldValues(i, attributes);
                    long decision = dataStore.GetFieldValue(i, dataStore.DataStoreInfo.DecisionFieldId);

                    eqClassCollection.AddRecordInitial(attributeValues,
                                                        decision,
                                                        w,
                                                        dataStore,
                                                        i,
                                                        updateStat);
                }
                //);
            }
            else
            {
                for (int i = 0; i < dataStore.NumberOfRecords; i++)
                //Parallel.For(0, dataStore.NumberOfRecords, i =>
                {
                    long[] attributeValues = dataStore.GetFieldValues(i, attributes);
                    long decision = dataStore.GetFieldValue(i, dataStore.DataStoreInfo.DecisionFieldId);

                    eqClassCollection.AddRecordInitial(attributeValues,
                                                        decision,
                                                        weights[i],
                                                        dataStore,
                                                        i,
                                                        updateStat);
                }
                //);
            }
     
            return eqClassCollection;
        }

        protected void AddRecordInitial(            
            long[] attributeValues, 
            long decision, 
            decimal weight, 
            DataStore dataStore,
            int idx = -1,
            bool updateStat = true)
        {
            EquivalenceClass eq = null;
            lock (mutex)
            {                
                if (!this.partitions.TryGetValue(attributeValues, out eq))
                {
                    eq = new EquivalenceClass(attributeValues, dataStore, updateStat);
                    this.partitions.Add(attributeValues, eq);
                }

                if (idx != -1)
                    eq.AddObject(idx, decision, weight, updateStat);
                else
                    eq.AddDecision(decision, weight);
            }
        }

        protected void InitPartitions()
        {
            //TODO capacity
            this.partitions = new Dictionary<long[], EquivalenceClass>(new Int64ArrayEqualityComparer());
        }

        public virtual void Calc(FieldSet attributeSet, DataStore dataStore)
        {
            this.InitPartitions();
            
            this.attributes = attributeSet.ToArray();
            decimal w = Decimal.Divide(Decimal.One, dataStore.NumberOfRecords);
            
            //Parallel.For(0, dataStore.NumberOfRecords, objectIdx =>
            for(int objectIdx = 0; objectIdx < dataStore.NumberOfRecords; objectIdx++)
            {
                this.UpdateStatistic(this.attributes, dataStore, objectIdx, w);
            }
            //);    
        }

        public virtual void Calc(FieldSet attributeSet, DataStore dataStore, decimal[] objectWeights)
        {
            this.InitPartitions();
            this.attributes = attributeSet.ToArray();

            //Parallel.For(0, dataStore.NumberOfRecords, objectIdx =>
            for (int objectIdx = 0; objectIdx < dataStore.NumberOfRecords; objectIdx++)
            {
                this.UpdateStatistic(this.attributes, dataStore, objectIdx, objectWeights[objectIdx]);
            }
            //);
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
            lock (mutex)
            {
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

            foreach (int objectIndex in objectSet)
            {
                long[] record = dataStore.GetFieldValues(objectIndex, attributeArray);
                EquivalenceClass reductStatistic = null;
                if (localPartitions.TryGetValue(record, out reductStatistic))
                {
                    reductStatistic.AddObject(objectIndex, dataStore.GetDecisionValue(objectIndex), objectWeights[objectIndex]);

                    if (reductStatistic.NumberOfDecisions > 1)
                        return false;
                }
                else
                {
                    reductStatistic = new EquivalenceClass(record, dataStore);
                    reductStatistic.AddObject(objectIndex, dataStore.GetDecisionValue(objectIndex), objectWeights[objectIndex]);
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
            StringBuilder stringBuilder = new StringBuilder();
            foreach (var kvp in this.partitions)
                stringBuilder.AppendLine(kvp.Value.ToString());
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
