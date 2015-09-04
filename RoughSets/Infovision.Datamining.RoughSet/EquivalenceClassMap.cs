using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Infovision.Data;
using Infovision.Utils;

namespace Infovision.Datamining.Roughset
{
    [Serializable]
    public class EquivalenceClassMap : IEnumerable<EquivalenceClass>, ICloneable
    {
        #region Globals

        private Dictionary<DataVector, EquivalenceClass> partitions;
        private Dictionary<long, int> decisionCount;
        private bool decisionCountCalculated;

        #endregion

        #region Constructors

        private EquivalenceClassMap()
        {
            this.partitions = new Dictionary<DataVector, EquivalenceClass>();
            this.decisionCount = new Dictionary<long, int>();
            this.decisionCountCalculated = false;
        }
        
        public EquivalenceClassMap(DataStoreInfo dataStoreInfo)
        {
            this.partitions = new Dictionary<DataVector, EquivalenceClass>();
            this.InitDecisionCount(dataStoreInfo);
        }

        private EquivalenceClassMap(EquivalenceClassMap roughPartitionMap)
        {
            this.partitions = (Dictionary<DataVector, EquivalenceClass>) roughPartitionMap.Partitions.CloneDictionaryCloningValues<DataVector, EquivalenceClass>();
            this.decisionCount = new Dictionary<long, int>(roughPartitionMap.DecisionCount);
            this.decisionCountCalculated = roughPartitionMap.DecisionCountCalculated;
        }

        #endregion

        #region Properties

        public Dictionary<DataVector, EquivalenceClass> Partitions
        {
            get { return partitions; }
        }

        private Dictionary<long, int> DecisionCount
        {
            get { return decisionCount; }
        }

        public bool DecisionCountCalculated
        {
            get { return decisionCountCalculated; }
        }

        public int Count
        {
            get { return partitions.Count; }
        }

        #endregion

        #region Methods

        protected void InitDecisionCount(DataStoreInfo dataStoreInfo)
        {
            DataFieldInfo decisionInfo = dataStoreInfo.GetFieldInfo(dataStoreInfo.DecisionFieldId);
            this.decisionCount = new Dictionary<long, int>(decisionInfo.Values().Count);
            foreach (long decisionValue in decisionInfo.InternalValues())
            {
                this.decisionCount.Add(decisionValue, decisionInfo.Histogram.GetBinValue(decisionValue));
            }
            this.decisionCountCalculated = true;
        }

        public void Calc(FieldSet attributeSet, DataStore dataStore)
        {
            int[] attributeArray = attributeSet.ToArray();
            foreach (int objectIndex in dataStore.GetObjectIndexes())
            {
                this.UpdateStatistic(attributeArray, dataStore, objectIndex, 1.0 / dataStore.NumberOfRecords);
            }
        }

        public void Calc(FieldSet attributeSet, DataStore dataStore, double[] objectWeights)
        {
            int[] attributeArray = attributeSet.ToArray();
            foreach (int objectIndex in dataStore.GetObjectIndexes())
            {
                this.UpdateStatistic(attributeArray, dataStore, objectIndex, objectWeights[objectIndex]);
            }
        }

        public void Calc(FieldSet attributeSet, DataStore dataStore, ObjectSet objectSet, double[] objectWeights)
        {
            int[] attributeArray = attributeSet.ToArray();
            foreach (int objectIndex in objectSet)
            {
                this.UpdateStatistic(attributeArray, dataStore, objectIndex, objectWeights[objectIndex]);
            }
        }

        private void UpdateStatistic(int[] attributeArray, DataStore dataStore, int objectIndex, double objectWeight)
        {
            DataVector record = dataStore.GetDataVector(objectIndex, attributeArray);
            EquivalenceClass reductStatistic = null;
            if (this.partitions.TryGetValue(record, out reductStatistic))
            {
                reductStatistic.AddObject(objectIndex, dataStore.GetDecisionValue(objectIndex), objectWeight);
            }
            else
            {
                reductStatistic = new EquivalenceClass(dataStore.DataStoreInfo);
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

            return EquivalenceClassMap.CheckRegionPositive(attributeSet, dataStore, objectSet, weights);
        }
        
        public static bool CheckRegionPositive(FieldSet attributeSet, DataStore dataStore, ObjectSet objectSet, double[] objectWeights)
        {
            Dictionary<DataVector, EquivalenceClass> localPartitions = new Dictionary<DataVector, EquivalenceClass>();
            int[] attributeArray = attributeSet.ToArray();

            foreach (int objectIndex in objectSet)
            {
                DataVector record = dataStore.GetDataVector(objectIndex, attributeArray);
                EquivalenceClass reductStatistic = null;
                if (localPartitions.TryGetValue(record, out reductStatistic))
                {
                    reductStatistic.AddObject(objectIndex, dataStore.GetDecisionValue(objectIndex), objectWeights[objectIndex]);

                    if (reductStatistic.NumberOfDecisions > 1)
                        return false;
                }
                else
                {
                    reductStatistic = new EquivalenceClass(dataStore.DataStoreInfo);
                    reductStatistic.AddObject(objectIndex, dataStore.GetDecisionValue(objectIndex), objectWeights[objectIndex]);
                    localPartitions[record] = reductStatistic;
                }
            }

            return true;
        }

        public EquivalenceClass GetEquivalenceClass(DataVector dataVector)
        {
            EquivalenceClass reductStatstic = null;
            if (this.partitions.TryGetValue(dataVector, out reductStatstic))
            {
                return reductStatstic;
            }
            else
            {
                reductStatstic = new EquivalenceClass();
                this.partitions.Add(dataVector, reductStatstic);
            }

            return reductStatstic;
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
        /// Clones the EquivalenceClassMap, performing a deep copy.
        /// </summary>
        /// <returns>A new instance of a EquivalenceClassMap, using a deep copy.</returns>
        public virtual object Clone()
        {
            return new EquivalenceClassMap(this);
        }
        #endregion

        #region System.Object Methods

        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();

            foreach (KeyValuePair<DataVector, EquivalenceClass> kvp in this.partitions)
            {
                stringBuilder.AppendLine(kvp.Value.ToString());
            }

            return stringBuilder.ToString();
        }

        #endregion
        #endregion
    }
}
