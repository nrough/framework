using System;
using System.Collections.Generic;
using System.Text;
using Infovision.Data;

namespace Infovision.Datamining.Roughset
{
    [Serializable]
    public class EquivalenceClass : ICloneable
    {
        #region Globals
                
        private Dictionary<int, double> instances;
        private Dictionary<long, HashSet<int>> decisionObjectIndexes;
        private bool calcStats;
        private long majorDecision = -1;        
        //TODO add equivalence class description as Attributes and their values

        #endregion

        #region Constructors

        public EquivalenceClass()
        {
            this.instances = new Dictionary<int, double>();
            this.decisionObjectIndexes = new Dictionary<long, HashSet<int>>();            
        }

        public EquivalenceClass(int numberOfDecisions)
        {
            this.instances = new Dictionary<int, double>();
            this.decisionObjectIndexes = new Dictionary<long, HashSet<int>>(numberOfDecisions);
        }

        public EquivalenceClass(DataStoreInfo dataStoreInfo)
            : this(dataStoreInfo.GetFieldInfo(dataStoreInfo.DecisionFieldId).InternalValues().Count)
        {            
        }

        public EquivalenceClass(EquivalenceClass equivalenceClassInfo)
        {
            this.instances = new Dictionary<int, double>(equivalenceClassInfo.instances);
            
            //copy elements
            this.decisionObjectIndexes = new Dictionary<long, HashSet<int>>(equivalenceClassInfo.decisionObjectIndexes.Count);
            foreach (KeyValuePair<long, HashSet<int>> kvp in equivalenceClassInfo.decisionObjectIndexes)
            {
                this.decisionObjectIndexes.Add(kvp.Key, new HashSet<int>(kvp.Value));
            }
        }

        #endregion

        #region Properties

        public int NumberOfObjects
        {
            get { return this.instances.Count; }
        }

        public int NumberOfDecisions
        {
            get { return this.decisionObjectIndexes.Keys.Count; }
        }

        public IEnumerable<int> ObjectIndexes
        {
            get { return this.instances.Keys; }
        }

        /// <summary>
        /// Returns IEnumerable collection of object indexes having specified decision value
        /// </summary>
        /// <param name="decisionValue">internal value of object decision attribute</param>
        /// <returns>Collection of objects having specified decision</returns>
        public IEnumerable<int> GetObjectIndexes(long decisionValue)
        {
            HashSet<int> localObjectIndexes;
            if (this.decisionObjectIndexes.TryGetValue(decisionValue, out localObjectIndexes))
                return localObjectIndexes;
            return new HashSet<int>();
        }

        public long MajorDecision
        {
            get 
            {
                if (!this.calcStats)
                {
                    this.CalcMajorDecision();
                }

                return this.majorDecision;                                
            }

            private set
            {
                this.majorDecision = value;
            }
        }                

        public IEnumerable<long> DecisionValues
        {
            get { return this.decisionObjectIndexes.Keys; }
        }

        #endregion

        #region Methods

        private void CalcMajorDecision()
        {
            long tmpMajorDecision = -1;
            double maxWeightSum = Double.MinValue;
            foreach (KeyValuePair<long, HashSet<int>> kvp in this.decisionObjectIndexes)
            {
                double sum = 0;
                foreach(int idx in kvp.Value)
                    sum += instances[idx];
                if (sum > maxWeightSum)
                {
                    tmpMajorDecision = kvp.Key;
                    maxWeightSum = sum;
                }
            }

            this.MajorDecision = tmpMajorDecision;
            this.calcStats = true;                        
        }

        public void AddObject(int objectIndex, long decisionValue, double weight)
        {
            this.instances.Add(objectIndex, weight);
                            
            HashSet<int> localObjectSet = null;
            if (!this.decisionObjectIndexes.TryGetValue(decisionValue, out localObjectSet))
            {
                localObjectSet = new HashSet<int>();
                this.decisionObjectIndexes[decisionValue] = localObjectSet;
            }
            localObjectSet.Add(objectIndex);
            this.calcStats = false;           
        }        

        public void RemoveObject(int objectIndex)
        {
            if (this.instances.ContainsKey(objectIndex))
            {
                foreach (KeyValuePair<long, HashSet<int>> kvp in this.decisionObjectIndexes)
                {
                    if (kvp.Value.Contains(objectIndex))
                    {
                        kvp.Value.Remove(objectIndex);
                        break;
                    }
                }

                this.instances.Remove(objectIndex);                                
            }
            this.calcStats = false;
        }                

        public int NumberOfObjectsWithDecision(long decisionValue)
        {
            HashSet<int> indices;
            if (this.decisionObjectIndexes.TryGetValue(decisionValue, out indices))
            {
                return indices.Count;
            }
            return 0;
        }

        public double DecisionProbability(long decisionValue)
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
            return new EquivalenceClass(this);
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
            stringBuilder.AppendFormat("Most frequent decision: {0}", this.MajorDecision);
            stringBuilder.Append(Environment.NewLine);

            return stringBuilder.ToString();
        }

        #endregion

        #endregion
    }
}
