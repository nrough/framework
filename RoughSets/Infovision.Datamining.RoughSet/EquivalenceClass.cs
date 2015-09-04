using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Infovision.Data;
using Infovision.Utils;
using Infovision.Math;
using System.Collections.Specialized;

namespace Infovision.Datamining.Roughset
{
    [Serializable]
    public class EquivalenceClass : ICloneable
    {
        #region Members

        private AttributeValueVector dataVector; //attributes and values for which EQ class was created
        private Dictionary<int, double> instances;  //objectId --> object weight
        private bool isStatCalculated; //flags if statistics have been calculated
        private Dictionary<long, HashSet<int>> decisionObjectIndexes; //decision value --> set of objects with decision
        private Dictionary<long, double> decisionWeigthSums; //list of decision weight sum pairs (sorted during statistics calculation)
        private long majorDecision; //decision within EQ class with greatest object weight sum
        private double majorDecisionWeightSum; //sum of w for a major decision
        private double totalWeightSum;
        private PascalSet decisionSet;  //set containing all decisions within EQ class
        private DataStore dataStore;

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
            get { return this.decisionSet.Count; }
        }

        public IEnumerable<int> ObjectIndexes
        {
            get { return this.instances.Keys; }
        }

        public long MajorDecision
        {
            get 
            {
                if (!this.isStatCalculated && this.ManualStatCalculation == false)
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
                if (!this.isStatCalculated && this.ManualStatCalculation == false)
                    this.CalcStatistics();
                return decisionSet;
            }

            internal set { this.decisionSet = value; }
        }

        public bool ManualStatCalculation
        {
            get;
            set;
        }

        #endregion

        #region Constructors        

        public EquivalenceClass(AttributeValueVector dataVector, DataStore data, bool manualStatCalculation = false)
        {                        
            this.dataVector = dataVector;
            this.dataStore = data;
            this.ManualStatCalculation = manualStatCalculation;

            this.instances = new Dictionary<int, double>();
            int numberOfDecisions = data.DataStoreInfo.GetDecisionFieldInfo().InternalValues().Count;
            this.decisionObjectIndexes = new Dictionary<long, HashSet<int>>(numberOfDecisions);

            this.decisionSet = new PascalSet(Convert.ToInt32(this.dataStore.DataStoreInfo.GetDecisionFieldInfo().MinValue),
                                             Convert.ToInt32(this.dataStore.DataStoreInfo.GetDecisionFieldInfo().MaxValue));

            this.decisionWeigthSums = new Dictionary<long, double>(numberOfDecisions);
        }

        public EquivalenceClass(AttributeValueVector dataVector, bool manualStatCalculation = false)
        {
            this.dataVector = dataVector;
            this.instances = new Dictionary<int, double>();
            this.ManualStatCalculation = manualStatCalculation;
        }

        private EquivalenceClass(EquivalenceClass eqClass)
        {            
            this.dataVector = new AttributeValueVector(eqClass.dataVector.GetAttributes(), eqClass.dataVector.GetValues(), true);
            this.instances = new Dictionary<int, double>(eqClass.instances);
            this.isStatCalculated = eqClass.isStatCalculated;            
            this.decisionObjectIndexes = new Dictionary<long, HashSet<int>>(eqClass.decisionObjectIndexes.Count);            
            foreach (var kvp in eqClass.decisionObjectIndexes)
                this.decisionObjectIndexes.Add(kvp.Key, new HashSet<int>(kvp.Value));
            this.decisionWeigthSums = new Dictionary<long, double>(eqClass.decisionWeigthSums);
            this.majorDecision = eqClass.majorDecision;
            this.majorDecisionWeightSum = eqClass.majorDecisionWeightSum;
            this.totalWeightSum = eqClass.totalWeightSum;
            this.decisionSet = new PascalSet(eqClass.decisionSet.LowerBound, eqClass.decisionSet.UpperBound, eqClass.decisionSet.Data);
            this.dataStore = eqClass.dataStore;
            this.ManualStatCalculation = eqClass.ManualStatCalculation;
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

        public double GetDecisionWeigth(long decision)
        {
            double result = 0.0;
            if (this.decisionWeigthSums.TryGetValue(decision, out result))
                return result;
            return 0.0;
        }

        private void CalcStatistics()
        {
            this.isStatCalculated = false;

            long tmpMajorDecision = -1;
            double maxWeightSum = Double.MinValue;
            this.decisionWeigthSums = new Dictionary<long, double>(this.decisionObjectIndexes.Count);
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
                this.decisionWeigthSums.Add(kvp.Key, sum);
                values[i++] = Convert.ToInt32(kvp.Key);                
            }

            decisionSet = new PascalSet(Convert.ToInt32(this.dataStore.DataStoreInfo.GetDecisionFieldInfo().MinValue), 
                                        Convert.ToInt32(this.dataStore.DataStoreInfo.GetDecisionFieldInfo().MaxValue), 
                                        values);

            this.majorDecision = tmpMajorDecision;
            this.majorDecisionWeightSum = maxWeightSum;

            this.isStatCalculated = true;
        }

        public void AddDecision(long decisionValue, double weight)
        {            
            int decisionValueInt = Convert.ToInt32(decisionValue);
            this.DecisionSet.AddElement(decisionValueInt);
            double weightSum = 0.0;
            if (!this.decisionWeigthSums.TryGetValue(decisionValue, out weightSum))
                this.decisionWeigthSums.Add(decisionValue, weight);
            else
                this.decisionWeigthSums[decisionValue] = weightSum + weight;

            totalWeightSum += weight;
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

            this.isStatCalculated = false;
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
            this.isStatCalculated = false;
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
            if (!this.isStatCalculated && this.ManualStatCalculation == false)
                this.CalcStatistics();

            DoubleEpsilonComparer comparer = new DoubleEpsilonComparer(0.0001 / this.dataStore.NumberOfRecords);

            foreach (KeyValuePair<long, double> kvp in this.decisionWeigthSums)
            {
                if (comparer.Equals(kvp.Value, this.majorDecisionWeightSum) == false)
                {
                    foreach (int objectIdx in decisionObjectIndexes[kvp.Key])
                        this.instances.Remove(objectIdx);

                    decisionObjectIndexes.Remove(kvp.Key);
                }
            }

            this.CalcStatistics();            
        }

        public void KeepMajorDecisions(double epsilon)
        {
            bool isFirst = true;
            var items = from pair in decisionWeigthSums
                                    orderby pair.Value descending
                                        select pair;

            HashSet<long> decisionsToRemove = new HashSet<long>();
            double totalWeightToRemove = 0.0;
            double max = Double.MinValue;
            foreach (KeyValuePair<long, double> pair in items)
            {
                if (isFirst)
                {
                    max = pair.Value;
                    isFirst = false;
                }
                else
                {
                    if (pair.Value < (((1.0 - epsilon) * max) - 0.00000000001))
                    {
                        decisionsToRemove.Add(pair.Key);
                        totalWeightToRemove += pair.Value;
                    }
                }
            }

            foreach (long decision in decisionsToRemove)
            {
                this.DecisionSet.RemoveElement(Convert.ToInt32(decision));
                this.decisionWeigthSums.Remove(decision);
                
                if (this.decisionObjectIndexes != null
                    && this.decisionObjectIndexes.ContainsKey(decision))
                {
                    foreach (int objectIdx in this.decisionObjectIndexes[decision])
                        this.instances.Remove(objectIdx);

                    decisionObjectIndexes.Remove(decision);
                }
            }

            this.totalWeightSum -= totalWeightToRemove;
        }

        public void KeepMajorDecisions2(double epsilon)
        {
            if (this.DecisionSet.Count <= 1)
                return;
            
            HashSet<long> decisionsToRemove = new HashSet<long>();
            double totalWeightToRemove = 0.0;
            foreach (KeyValuePair<long, double> kvp in this.decisionWeigthSums)
                if ((kvp.Value / this.totalWeightSum) < (1.0 - epsilon))
                {
                    decisionsToRemove.Add(kvp.Key);
                    totalWeightToRemove += kvp.Value;                    
                }

            foreach (long decision in decisionsToRemove)
            {
                this.DecisionSet.RemoveElement(Convert.ToInt32(decision));
                this.decisionWeigthSums.Remove(decision);

                if (this.decisionObjectIndexes != null
                    && this.decisionObjectIndexes.ContainsKey(decision))
                {
                    foreach (int objectIdx in this.decisionObjectIndexes[decision])
                        this.instances.Remove(objectIdx);

                    decisionObjectIndexes.Remove(decision);                    
                }                
            }

            this.totalWeightSum -= totalWeightToRemove;
        }

        public double GetWeight()
        {
            return this.totalWeightSum;
        }

        public double GetWeight(long decision)
        {
            if (!this.isStatCalculated && this.ManualStatCalculation == false)
                this.CalcStatistics();            
            
            double ret = 0.0;
            if (this.decisionWeigthSums.TryGetValue(decision, out ret))
                return ret;
            return 0.0;
        }

        #region ICloneable Members
        /// <summary>
        /// Clones the EquivalenceClassCollection, performing a deep copy.
        /// </summary>
        /// <returns>A new instance of a EquivalenceClassCollection, using a deep copy.</returns>
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

        public string ToString2()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendFormat("{0} ", this.Instance);
            foreach(int decision in this.DecisionSet)
                stringBuilder.AppendFormat("d={0} ({1}) ", decision, this.GetDecisionWeigth((long)decision));
            return stringBuilder.ToString();
        }

        #endregion

        #endregion
    }
}
