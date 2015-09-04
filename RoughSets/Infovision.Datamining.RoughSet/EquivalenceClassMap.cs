using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Infovision.Data;
using Infovision.Utils;

namespace Infovision.Datamining.Roughset
{
    [Serializable]
    public class EquivalenceClassMap : IEnumerable<EquivalenceClassInfo>, ICloneable
    {
        #region Globals

        private Dictionary<DataVector, EquivalenceClassInfo> partitions;
        private Dictionary<Int64, int> decisionCount;
        private bool decisionCountCalculated;

        #endregion

        #region Constructors

        private EquivalenceClassMap()
        {
            this.partitions = new Dictionary<DataVector, EquivalenceClassInfo>();
            this.decisionCount = new Dictionary<Int64, int>();
            this.decisionCountCalculated = false;
        }
        
        public EquivalenceClassMap(DataStoreInfo dataStoreInfo)
        {
            this.partitions = new Dictionary<DataVector, EquivalenceClassInfo>();
            this.InitDecisionCount(dataStoreInfo);
        }

        private EquivalenceClassMap(EquivalenceClassMap roughPartitionMap)
        {
            this.partitions = (Dictionary<DataVector, EquivalenceClassInfo>) roughPartitionMap.Partitions.CloneDictionaryCloningValues<DataVector, EquivalenceClassInfo>();
            this.decisionCount = new Dictionary<Int64, int>(roughPartitionMap.DecisionCount);
            this.decisionCountCalculated = roughPartitionMap.DecisionCountCalculated;
        }

        #endregion

        #region Properties

        public Dictionary<DataVector, EquivalenceClassInfo> Partitions
        {
            get { return partitions; }
        }

        private Dictionary<Int64, int> DecisionCount
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
            this.decisionCount = new Dictionary<Int64, int>(decisionInfo.Values().Count);
            foreach (Int64 decisionValue in decisionInfo.InternalValues())
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
                this.UpdateStatistic(attributeArray, dataStore, objectIndex);
            }
        }

        public void Calc(FieldSet attributeSet, DataStore dataStore, ObjectSet objectSet)
        {
            int[] attributeArray = attributeSet.ToArray();
            foreach (int objectIndex in objectSet)
            {
                this.UpdateStatistic(attributeArray, dataStore, objectIndex);
            }
        }

        private void UpdateStatistic(int[] attributeArray, DataStore dataStore, int objectIndex)
        {
            DataVector record = dataStore.GetDataVector(objectIndex, attributeArray);
            EquivalenceClassInfo reductStatistic = null;
            if (this.partitions.TryGetValue(record, out reductStatistic))
            {
                reductStatistic.AddObject(objectIndex, dataStore);
            }
            else
            {
                reductStatistic = new EquivalenceClassInfo(dataStore.DataStoreInfo);
                reductStatistic.AddObject(objectIndex, dataStore);
                this.partitions[record] = reductStatistic;
            }
        }        

        public static bool CheckRegionPositive(FieldSet attributeSet, DataStore dataStore, ObjectSet objectSet)
        {
            Dictionary<DataVector, EquivalenceClassInfo> localPartitions = new Dictionary<DataVector, EquivalenceClassInfo>();
            int[] attributeArray = attributeSet.ToArray();

            foreach (int objectIndex in objectSet)
            {
                DataVector record = dataStore.GetDataVector(objectIndex, attributeArray);
                EquivalenceClassInfo reductStatistic = null;
                if (localPartitions.TryGetValue(record, out reductStatistic))
                {
                    reductStatistic.AddObject(objectIndex, dataStore);

                    if (reductStatistic.NumberOfDecisions > 1)
                        return false;
                }
                else
                {
                    reductStatistic = new EquivalenceClassInfo(dataStore.DataStoreInfo);
                    reductStatistic.AddObject(objectIndex, dataStore);
                    localPartitions[record] = reductStatistic;
                }
            }

            return true;
        }

        public EquivalenceClassInfo GetStatistics(DataVector dataVector)
        {
            EquivalenceClassInfo reductStatstic = null;
            if (this.partitions.TryGetValue(dataVector, out reductStatstic))
            {
                return reductStatstic;
            }
            else
            {
                reductStatstic = new EquivalenceClassInfo();
                this.partitions.Add(dataVector, reductStatstic);
            }

            return reductStatstic;
        }

        public int GetDecisionValueCount(Int64 decisionValue)
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
        public IEnumerator<EquivalenceClassInfo> GetEnumerator()
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

            foreach (KeyValuePair<DataVector, EquivalenceClassInfo> kvp in this.partitions)
            {
                stringBuilder.AppendLine(kvp.Value.ToString());
            }

            return stringBuilder.ToString();
        }

        #endregion
        #endregion
    }
}
