using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Infovision.Data;
using Infovision.Utils;

namespace Infovision.Datamining.Roughset
{
    [Serializable]
    public class EquivalenceClassCollection : IEnumerable<EquivalenceClass>, ICloneable
    {
        #region Members

        private Dictionary<AttributeValueVector, EquivalenceClass> partitions;
        private Dictionary<long, int> decisionCount;
        private int[] attributes;

        #endregion

        #region Properties

        public Dictionary<AttributeValueVector, EquivalenceClass> Partitions
        {
            get 
            { 
                if(this.partitions == null)
                    this.InitPartitions();
                return partitions;
            }
            protected set { this.partitions = value;  }
        }

        private Dictionary<long, int> DecisionCount
        {
            get { return decisionCount; }
        }        

        public int NumberOfPartitions
        {
            get { return partitions.Count; }
        }

        public int[] Attributes
        {
            get { return this.attributes; }
        }
        
        #endregion

        #region Constructors        

        public EquivalenceClassCollection(int[] attributes)
        {
            this.attributes = attributes;
            this.InitPartitions();
        }
        
        public EquivalenceClassCollection(DataStore data)
        {            
            this.InitDecisionCount(data.DataStoreInfo);            
        }

        private EquivalenceClassCollection(EquivalenceClassCollection equivalenceClassMap)
        {
            this.partitions = (Dictionary<AttributeValueVector, EquivalenceClass>)equivalenceClassMap.Partitions.CloneDictionaryCloningValues<AttributeValueVector, EquivalenceClass>();
            this.decisionCount = new Dictionary<long, int>(equivalenceClassMap.DecisionCount);
            this.attributes = equivalenceClassMap.Attributes;
        }        

        #endregion

        #region Methods

        public static EquivalenceClassCollection Create(DataStore dataStore, int[] attributes, double epsilon, double[] weights = null)
        {
            if (weights != null && dataStore.NumberOfRecords != weights.Length)
                throw new ArgumentOutOfRangeException("weights", "Weight vector must has the same length as number of recors in dataStore");

            EquivalenceClassCollection eqClassCollection = new EquivalenceClassCollection(attributes);

            for (int i = 0; i < dataStore.NumberOfRecords; i++)
            {
                long[] attributeValues = dataStore.GetObjectFields(i, attributes);
                long decision = dataStore.GetObjectField(i, dataStore.DataStoreInfo.DecisionFieldId);

                eqClassCollection.AddRecordInitial(attributes, 
                                                   attributeValues, 
                                                   decision, 
                                                   weights != null ? weights[i] : 1.0,
                                                   dataStore);
            }            

            return eqClassCollection;
        }

        protected void AddRecordInitial(int[] attributes, long[] attributeValues, long decision, double weight, DataStore dataStore)
        {            
            AttributeValueVector vector = new AttributeValueVector(attributes, attributeValues);
            EquivalenceClass eq = null;
            if (!this.partitions.TryGetValue(vector, out eq))
            {
                eq = new EquivalenceClass(vector, dataStore, true);
                this.partitions.Add(vector, eq);
            }
            eq.AddDecision(decision, weight);
        }              

        protected void InitPartitions()
        {
            this.partitions = new Dictionary<AttributeValueVector, EquivalenceClass>();
        }

        protected void InitDecisionCount(DataStoreInfo dataStoreInfo)
        {
            DataFieldInfo decisionInfo = dataStoreInfo.GetFieldInfo(dataStoreInfo.DecisionFieldId);
            this.decisionCount = new Dictionary<long, int>(decisionInfo.Values().Count);
            foreach (long decisionValue in decisionInfo.InternalValues())
                this.decisionCount.Add(decisionValue, decisionInfo.Histogram.GetBinValue(decisionValue));
        }

        public virtual void Calc(FieldSet attributeSet, DataStore dataStore)
        {
            this.InitPartitions();
            this.attributes = attributeSet.ToArray();

            foreach (int objectIndex in dataStore.GetObjectIndexes())
                this.UpdateStatistic(this.attributes, dataStore, objectIndex, 1.0 / dataStore.NumberOfRecords);
        }

        public virtual void Calc(FieldSet attributeSet, DataStore dataStore, double[] objectWeights)
        {
            this.InitPartitions();
            int[] attributeArray = attributeSet.ToArray();
            foreach (int objectIndex in dataStore.GetObjectIndexes()
                this.UpdateStatistic(attributeArray, dataStore, objectIndex, objectWeights[objectIndex]);
        }

        public virtual void Calc(FieldSet attributeSet, DataStore dataStore, ObjectSet objectSet, double[] objectWeights)
        {
            this.InitPartitions();
            int[] attributeArray = attributeSet.ToArray();
            foreach (int objectIndex in objectSet)
                this.UpdateStatistic(attributeArray, dataStore, objectIndex, objectWeights[objectIndex]);            
        }

        private void UpdateStatistic(int[] attributeArray, DataStore dataStore, int objectIndex, double objectWeight)
        {
            AttributeValueVector record = dataStore.GetDataVector(objectIndex, attributeArray);
            EquivalenceClass reductStatistic = null;
            if (this.partitions.TryGetValue(record, out reductStatistic))
            {
                reductStatistic.AddObject(objectIndex, dataStore.GetDecisionValue(objectIndex), objectWeight);
            }
            else
            {
                reductStatistic = new EquivalenceClass(record, dataStore);
                reductStatistic.AddObject(objectIndex, dataStore.GetDecisionValue(objectIndex), objectWeight);
                this.partitions[record] = reductStatistic;
            }
        }

        public static bool CheckRegionPositive(FieldSet attributeSet, DataStore dataStore, ObjectSet objectSet)
        {
            double[] weights = new double[dataStore.NumberOfRecords];
            for (int i = 0; i < weights.Length; i++)
            {
                weights[i] = 1.0 / dataStore.NumberOfRecords;
            }

            return EquivalenceClassCollection.CheckRegionPositive(attributeSet, dataStore, objectSet, weights);
        }
        
        public static bool CheckRegionPositive(FieldSet attributeSet, DataStore dataStore, ObjectSet objectSet, double[] objectWeights)
        {
            Dictionary<AttributeValueVector, EquivalenceClass> localPartitions = new Dictionary<AttributeValueVector, EquivalenceClass>();
            int[] attributeArray = attributeSet.ToArray();

            foreach (int objectIndex in objectSet)
            {
                AttributeValueVector record = dataStore.GetDataVector(objectIndex, attributeArray);
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

        public EquivalenceClass GetEquivalenceClass(AttributeValueVector dataVector)
        {
            EquivalenceClass reductStatstic = null;
            if (this.partitions.TryGetValue(dataVector, out reductStatstic))
            {
                return reductStatstic;
            }

            return null; //TODO create special object return new EquivalenceClassNull();
        }

        public int GetDecisionValueCount(long decisionValue)
        {
            int count = 0;
            if(this.decisionCount.TryGetValue(decisionValue, out count))
            {
                return count;
            }
            return 0;
        }        

        #region IEnumerable Members
        /// <summary>
        /// Returns an IEnumerator to enumerate through the partition map.
        /// </summary>
        /// <returns>An IEnumerator instance.</returns>
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
        /// <returns>A new instance of a EquivalenceClassCollection, using a deep copy.</returns>
        public virtual object Clone()
        {
            return new EquivalenceClassCollection(this);
        }
        #endregion

        #region System.Object Methods

        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();

            foreach (KeyValuePair<AttributeValueVector, EquivalenceClass> kvp in this.partitions)
            {
                stringBuilder.AppendLine(kvp.Value.ToString());
            }

            return stringBuilder.ToString();
        }

        public string ToString2()
        {
            StringBuilder stringBuilder = new StringBuilder();
            for (int i = 0; i < this.Attributes.Length; i++ )
                stringBuilder.AppendFormat("a{0} ", this.Attributes[i]);
            stringBuilder.Append(Environment.NewLine);
            foreach (KeyValuePair<AttributeValueVector, EquivalenceClass> kvp in this.partitions)
                stringBuilder.AppendLine(kvp.Value.ToString2());
            return stringBuilder.ToString();
        }

        #endregion
        #endregion
    }
}
