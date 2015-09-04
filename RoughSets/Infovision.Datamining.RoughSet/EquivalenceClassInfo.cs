using System;
using System.Collections.Generic;
using System.Text;
using Infovision.Data;

namespace Infovision.Datamining.Roughset
{
    [Serializable]
    public class EquivalenceClassInfo : ICloneable
    {
        #region Globals
        
        private Dictionary<Int64, int> decisionCount;
        private HashSet<int> objectIndexes;
        private Dictionary<Int64, HashSet<int>> decisionObjectIndexes;

        #endregion

        #region Constructors

        public EquivalenceClassInfo()
        {
            this.decisionCount = new Dictionary<Int64, int>();
            this.objectIndexes = new HashSet<int>();
            this.decisionObjectIndexes = new Dictionary<Int64, HashSet<int>>();
        }

        public EquivalenceClassInfo(int numberOfDecisions)
        {
            this.decisionCount = new Dictionary<Int64, int>(numberOfDecisions);
            this.objectIndexes = new HashSet<int>();
            this.decisionObjectIndexes = new Dictionary<Int64, HashSet<int>>(numberOfDecisions);
        }

        public EquivalenceClassInfo(DataStoreInfo dataStoreInfo)
            : this(dataStoreInfo.GetFieldInfo(dataStoreInfo.DecisionFieldId).InternalValues().Count)
        {            
        }

        public EquivalenceClassInfo(EquivalenceClassInfo equivalenceClassInfo)
        {
            this.decisionCount = new Dictionary<Int64, int>(equivalenceClassInfo.DecisionCount);
            this.objectIndexes = new HashSet<int>(equivalenceClassInfo.objectIndexes);
            
            //copy elements
            this.decisionObjectIndexes = new Dictionary<Int64, HashSet<int>>(equivalenceClassInfo.decisionObjectIndexes.Count);
            foreach (KeyValuePair<Int64, HashSet<int>> kvp in equivalenceClassInfo.decisionObjectIndexes)
            {
                this.decisionObjectIndexes.Add(kvp.Key, new HashSet<int>(kvp.Value));
            }
        }

        #endregion

        #region Properties

        public int NumberOfObjects
        {
            get { return this.objectIndexes.Count; }
        }

        public int NumberOfDecisions
        {
            get { return this.decisionCount.Keys.Count; }
        }

        public IEnumerable<int> ObjectIndexes
        {
            get { return this.objectIndexes; }
        }

        /// <summary>
        /// Returns IEnumerable collection of object indexes having specified decision value
        /// </summary>
        /// <param name="decisionValue">internal value of object decision attribute</param>
        /// <returns>Collection of objects having specified decision</returns>
        public IEnumerable<int> GetObjectIndexes(Int64 decisionValue)
        {
            HashSet<int> localObjectIndexes;
            if (this.decisionObjectIndexes.TryGetValue(decisionValue, out localObjectIndexes))
                return localObjectIndexes;
            return new HashSet<int>();
        }

        public Int64 MostFrequentDecision
        {
            get 
            {
                int maxCount = 0;
                Int64 decision = -1;
                foreach (KeyValuePair<Int64, int> decCount in decisionCount)
                {
                    if (maxCount < decCount.Value)
                    {
                        decision = decCount.Key;
                        maxCount = decCount.Value;
                    }
                }
                return decision;
            }
        }

        public int MostFrequentDecisionCount
        {
            get
            {
                Int64 decision = this.MostFrequentDecision;
                int maxCount = 0;
                if (decisionCount.TryGetValue(decision, out maxCount))
                {
                    return maxCount;
                }
                return 0;
            }
        }

        private Dictionary<Int64, int> DecisionCount
        {
            get { return this.decisionCount; }
        }

        public IEnumerable<Int64> DecisionValues
        {
            get { return this.decisionCount.Keys; }
        }

        #endregion

        #region Methods

        private bool AddObject(int objectIndex, Int64 decisionValue)
        {
            if (this.objectIndexes.Add(objectIndex))
            {
                int value = 0;
                this.decisionCount[decisionValue] = decisionCount.TryGetValue(decisionValue, out value) ? ++value : 1;

                HashSet<int> localObjectSet = null;
                if (!this.decisionObjectIndexes.TryGetValue(decisionValue, out localObjectSet))
                {
                    localObjectSet = new HashSet<int>();
                    this.decisionObjectIndexes[decisionValue] = localObjectSet;
                }
                localObjectSet.Add(objectIndex);

                return true;
            }

            return false;
        }

        public virtual bool AddObject(int objectIndex, DataStore dataStore)
        {
            return this.AddObject(objectIndex, dataStore.GetDecisionValue(objectIndex));
        }

        private void RemoveObject(int objectIndex, Int64 decisionValue)
        {
            if (this.objectIndexes.Contains(objectIndex))
            {
                int value = 0;
                if (decisionCount.TryGetValue(decisionValue, out value))
                {
                    if (value > 0)
                    {
                        this.decisionCount[decisionValue] = value - 1;                      
                    }
                }

                HashSet<int> localObjectSet = null;
                if (this.decisionObjectIndexes.TryGetValue(decisionValue, out localObjectSet))
                {
                    localObjectSet.Remove(objectIndex);
                }

                this.objectIndexes.Remove(objectIndex);
            }
        }
        
        public virtual void RemoveObject(int objectIndex, DataStore dataStore)
        {
            Int64 decisionValue = dataStore.GetDecisionValue(objectIndex);
            this.RemoveObject(objectIndex, decisionValue);
        }

        public int NumberOfObjectsWithDecision(Int64 decisionValue)
        {
            int count = 0;
            if (this.decisionCount.TryGetValue(decisionValue, out count))
            {
                return count;
            }
            return 0;
        }

        public double DecisionProbability(Int64 decisionValue)
        {
            return (double)this.NumberOfObjectsWithDecision(decisionValue) / (double)this.NumberOfObjects;
        }

        #region ICloneable Members
        /// <summary>
        /// Clones the EquivalenceClassMap, performing a deep copy.
        /// </summary>
        /// <returns>A new instance of a EquivalenceClassMap, using a deep copy.</returns>
        public virtual object Clone()
        {
            return new EquivalenceClassInfo(this);
        }
        #endregion

        #region System.Object Methods

        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendFormat("Number of objects: {0}", this.NumberOfObjects);
            stringBuilder.Append(Environment.NewLine);
            stringBuilder.AppendFormat("Number of decisions: {0}", this.NumberOfDecisions);
            stringBuilder.Append(Environment.NewLine);
            stringBuilder.AppendFormat("Most frequent decision: {0}", this.MostFrequentDecision);
            stringBuilder.Append(Environment.NewLine);

            return stringBuilder.ToString();
        }

        #endregion

        #endregion
    }
}
