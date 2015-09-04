using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Infovision.Data;
using Infovision.Utils;

namespace Infovision.Datamining.Roughset
{
    [Serializable]
    public class EquivalenceClass : ICloneable
    {
        #region Members
                
        private Dictionary<int, double> instances;
        private Dictionary<long, HashSet<int>> decisionObjectIndexes;
        private AttributeValueVector dataVector;
        private bool calcStats;
        private long majorDecision = -1;
        private PascalSet decisionSet;
        //TODO add equivalence class description as Attributes and their values

        #endregion

        #region Properties

        public AttributeValueVector DataVector
        {
            get { return this.dataVector; }
        }

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

        public PascalSet DecisionSet
        {
            get
            {
                if (decisionSet == null)
                {
                    int min = Int32.MaxValue;
                    int max = Int32.MinValue;
                    int[] values = Array.ConvertAll<long, int>(this.decisionObjectIndexes.Keys.ToArray(), x => Convert.ToInt32(x));

                    foreach(long d in this.decisionObjectIndexes.Keys)
                    {
                        if(d < min)
                            min = Convert.ToInt32(d);

                        if(d > max)
                            max = Convert.ToInt32(d);
                    }

                    decisionSet = new PascalSet(min, max, values);
                }

                return decisionSet;
            }
        }

        #endregion

        #region Constructors

        public EquivalenceClass(AttributeValueVector dataVector)
        {
            this.dataVector = dataVector;
            this.instances = new Dictionary<int, double>();
            this.decisionObjectIndexes = new Dictionary<long, HashSet<int>>();
        }

        public EquivalenceClass(AttributeValueVector dataVector, int numberOfDecisions)
        {
            this.dataVector = dataVector;
            this.instances = new Dictionary<int, double>();
            this.decisionObjectIndexes = new Dictionary<long, HashSet<int>>(numberOfDecisions);
        }

        public EquivalenceClass(AttributeValueVector dataVector, DataStoreInfo dataStoreInfo)
            : this(dataVector, dataStoreInfo.GetFieldInfo(dataStoreInfo.DecisionFieldId).InternalValues().Count)
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

        #region Methods

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
                decisionSet = null;
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

        public int GetNumberOfObjectsWithDecision(long decisionValue)
        {
            HashSet<int> indices;
            if (this.decisionObjectIndexes.TryGetValue(decisionValue, out indices))
            {
                return indices.Count;
            }
            return 0;
        }

        public double GetDecisionProbability(long decisionValue)
        {
            return (double)this.GetNumberOfObjectsWithDecision(decisionValue) / (double)this.NumberOfObjects;
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
