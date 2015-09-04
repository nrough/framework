﻿using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Infovision.Data;
using Infovision.Utils;
using Infovision.Math;

namespace Infovision.Datamining.Roughset
{
    [Serializable]
    public class EquivalenceClass : ICloneable
    {
        #region Members

        private AttributeValueVector dataVector; //attributes and values for which EQ class was created
        private Dictionary<int, double> instances;  //objectId --> object weight

        private bool calcStats; //flags if statistics have been calculated        
        private Dictionary<long, HashSet<int>> decisionObjectIndexes; //decision value --> set of objects with decision
        private List<KeyValuePair<long, double>> decisionWeigthSums; //list of decision weight sum pairs (sorted during statistics calculation)                                                 
        private long majorDecision; //decision within EQ class with greatest object weight sum
        private double majorDecisionWeightSum; //sum of weights for a major decision
        private double totalWeightSum;
        private PascalSet decisionSet;  //pascal set containing all decisions within EQ class        
        private DataStore dataStore;
        
        //TODO add equivalence class description as Attributes and their values

        #endregion

        #region Properties

        public AttributeValueVector Instance
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
                    this.CalcStatistics();

                return this.majorDecision;                                
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
                if (!this.calcStats)
                    this.CalcStatistics();
                return decisionSet;                
            }
        }

        #endregion

        #region Constructors        

        public EquivalenceClass(AttributeValueVector dataVector, DataStore data)
        {                        
            this.dataVector = dataVector;
            this.dataStore = data;

            this.instances = new Dictionary<int, double>();
            int numberOfDecisions = data.DataStoreInfo.GetDecisionFieldInfo().InternalValues().Count;
            this.decisionObjectIndexes = new Dictionary<long, HashSet<int>>(numberOfDecisions);
            
        }

        private EquivalenceClass(EquivalenceClass eqClass)
        {
            this.instances = new Dictionary<int, double>(eqClass.instances);
            //copy elements
            this.decisionObjectIndexes = new Dictionary<long, HashSet<int>>(eqClass.decisionObjectIndexes.Count);            
            foreach (KeyValuePair<long, HashSet<int>> kvp in eqClass.decisionObjectIndexes)
            {
                this.decisionObjectIndexes.Add(kvp.Key, new HashSet<int>(kvp.Value));
            }
            this.dataStore = eqClass.dataStore;
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

        private void CalcStatistics()
        {
            this.calcStats = false;

            long tmpMajorDecision = -1;
            double maxWeightSum = Double.MinValue;
            this.decisionWeigthSums = new List<KeyValuePair<long, double>>(this.decisionObjectIndexes.Count);
            this.totalWeightSum = 0.0;
            int[] values = new int[this.decisionObjectIndexes.Count];

            double minWeight = Double.MaxValue;

            int i = 0;
            foreach (KeyValuePair<long, HashSet<int>> kvp in this.decisionObjectIndexes)
            {                
                double sum = 0;
                foreach (int idx in kvp.Value)
                {
                    double weight = instances[idx];
                    sum += weight;
                    if(minWeight > weight)
                        minWeight = weight;
                }
                
                if (sum > maxWeightSum)
                {
                    tmpMajorDecision = kvp.Key;
                    maxWeightSum = sum;
                }

                this.totalWeightSum += sum;
                this.decisionWeigthSums.Add(new KeyValuePair<long, double>(kvp.Key, sum));
                values[i++] = Convert.ToInt32(kvp.Key);                
            }

            decisionSet = new PascalSet(Convert.ToInt32(this.dataStore.DataStoreInfo.GetDecisionFieldInfo().MinValue), 
                                        Convert.ToInt32(this.dataStore.DataStoreInfo.GetDecisionFieldInfo().MaxValue), 
                                        values);

            this.decisionWeigthSums.Sort((firstPair, nextPair) => -firstPair.Value.CompareTo(nextPair.Value));            

            this.majorDecision = tmpMajorDecision;
            this.majorDecisionWeightSum = maxWeightSum;

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

        public void RemoveObjectsWithMinorDecisions()
        {            
            if (!this.calcStats)
                this.CalcStatistics();

            DoubleEpsilonComparer comparer = new DoubleEpsilonComparer(0.0001 / this.dataStore.NumberOfRecords);

            foreach (KeyValuePair<long, double> kvp in this.decisionWeigthSums)
            {
                if (comparer.Equals(kvp.Value, this.majorDecisionWeightSum) == false)                    
                    decisionObjectIndexes.Remove(kvp.Key);                                        
            }

            this.CalcStatistics();            
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
