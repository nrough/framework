using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Infovision.Data;
using Infovision.Utils;
using Infovision.Math;
using System.Collections.Specialized;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace Infovision.Datamining.Roughset
{
    [Serializable]
    public class EquivalenceClass : ICloneable
    {
        #region Members

        private bool isStatCalculated; //flags if statistics have been calculated
        
        private long[] dataVector;
        private Dictionary<int, decimal> instances;  //map: objectIdx -> weight 
        private decimal totalWeightSum; //sum of object weights belonging to this class
        private PascalSet<long> decisionSet;  //set containing all decisions within this class
        private DataStore dataStore; //reference to data
        private readonly object mutex = new object();

        //TODO to delete
        private Dictionary<long, HashSet<int>> decisionObjectIndexes; //map: decision -> set of objects with decision
        private Dictionary<long, decimal> decisionWeigthSums; //map decision -> weight
        
        private long majorDecision; //major decision within this class 
        private decimal majorDecisionWeightSum; //sum of object weights with major decision 

        #endregion

        #region Properties

        public Dictionary<int, decimal> Members
        {
            get { return this.instances; }
            set { this.instances = value; }
        }

        public long[] Instance
        {
            get { return this.dataVector; }
        }

        public int NumberOfObjects
        {
            get { return this.instances.Count; }
        }

        public int NumberOfDecisions
        {            
            get { return this.DecisionSet.Count; }
        }

        public IEnumerable<int> ObjectIndexes
        {
            get { return this.instances.Keys; }
        }

        public decimal WeightSum
        { 
            get { return this.totalWeightSum; }
            set { this.totalWeightSum = value; }
        }

        public long MajorDecision
        {
            get 
            {
                CalcStatistics();
                return this.majorDecision;                
            }            
        }                

        public IEnumerable<long> DecisionValues
        {
            get { return this.decisionSet; }
        }

        public PascalSet<long> DecisionSet
        {
            get
            {
                CalcStatistics();
                return decisionSet;
            }

            internal set { this.decisionSet = value; }
        }

        public bool UpdateStat { get; set; }

        public Dictionary<int, decimal> Instances 
        { 
            get { return this.instances; } 
        }

        #endregion

        #region Constructors        

        public EquivalenceClass(long[] dataVector, DataStore data, bool updateStat = true)
        {
            this.dataStore = data;
            this.dataVector = dataVector;
            DataFieldInfo decisionField = data.DataStoreInfo.DecisionInfo;
            this.decisionSet = new PascalSet<long>(decisionField.MinValue, decisionField.MaxValue);
            int numberOfDecisions = decisionField.InternalValues().Count;

            if (updateStat)
            {
                this.instances = new Dictionary<int, decimal>();
                this.decisionObjectIndexes = new Dictionary<long, HashSet<int>>(numberOfDecisions);
            }

            this.decisionWeigthSums = new Dictionary<long, decimal>(numberOfDecisions);
            this.majorDecisionWeightSum = Decimal.MinValue;
            this.majorDecision = -1;

            this.UpdateStat = updateStat;
        }

        private EquivalenceClass(EquivalenceClass eqClass)
        {                        
            this.dataVector = new long[eqClass.dataVector.Length];
            Array.Copy(eqClass.dataVector, this.dataVector, eqClass.dataVector.Length);
            this.UpdateStat = eqClass.UpdateStat;
            this.isStatCalculated = eqClass.isStatCalculated;

            if (this.UpdateStat)
            {
                this.instances = new Dictionary<int, decimal>(eqClass.instances);
                this.decisionObjectIndexes = new Dictionary<long, HashSet<int>>(eqClass.decisionObjectIndexes.Count);
                foreach (var kvp in eqClass.decisionObjectIndexes)
                    this.decisionObjectIndexes.Add(kvp.Key, new HashSet<int>(kvp.Value));
            }
            this.decisionWeigthSums = new Dictionary<long, decimal>(eqClass.decisionWeigthSums);
            this.majorDecision = eqClass.majorDecision;
            this.majorDecisionWeightSum = eqClass.majorDecisionWeightSum;
            this.totalWeightSum = eqClass.totalWeightSum;
            this.decisionSet = new PascalSet<long>(eqClass.DecisionSet.LowerBound, eqClass.DecisionSet.UpperBound, eqClass.decisionSet.Data);
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
        
        public decimal GetDecisionWeigth(long decision)
        {
            decimal result = Decimal.Zero;
            if (this.decisionWeigthSums.TryGetValue(decision, out result))
                return result;
            return Decimal.Zero;
        }

        protected void DoCalcStatistics()
        {
            lock (mutex)
            {                
                long tmpMajorDecision = -1;
                decimal maxWeightSum = Decimal.MinValue;
                this.totalWeightSum = Decimal.Zero;
                this.decisionWeigthSums = new Dictionary<long, decimal>(this.decisionObjectIndexes.Count);                                
                foreach (KeyValuePair<long, HashSet<int>> kvp in this.decisionObjectIndexes)
                {
                    decimal sum = Decimal.Zero;
                    foreach (int idx in kvp.Value)
                        sum += instances[idx];

                    if (sum > maxWeightSum)
                    {
                        tmpMajorDecision = kvp.Key;
                        maxWeightSum = sum;
                    }

                    this.totalWeightSum += sum;
                    this.decisionWeigthSums.Add(kvp.Key, sum);
                }

                DataFieldInfo decisionField = this.dataStore.DataStoreInfo.DecisionInfo;
                decisionSet = new PascalSet<long>(decisionField.MinValue,
                                                  decisionField.MaxValue,
                                                  this.decisionObjectIndexes.Keys);

                this.majorDecision = tmpMajorDecision;
                this.majorDecisionWeightSum = maxWeightSum;
                this.isStatCalculated = true;
            }
        }

        public void AddDecision(long decisionValue, decimal weight)
        {
            lock (mutex)
            {
                this.decisionSet += decisionValue;
                totalWeightSum += weight;
                
                decimal weightSum = Decimal.Zero;
                if (this.decisionWeigthSums.TryGetValue(decisionValue, out weightSum))
                {
                    weight += weightSum;
                    this.decisionWeigthSums[decisionValue] = weight;
                }
                else
                {
                    this.decisionWeigthSums.Add(decisionValue, weight);
                }

                if (this.majorDecisionWeightSum < weight)
                {
                    this.majorDecision = decisionValue;
                    this.majorDecisionWeightSum = weight;
                }                
            }
        }

        public void AddObject(int objectIndex, long decisionValue, decimal weight, bool updateStat = true)
        {
            lock (mutex)
            {                                
                if (updateStat)
                {
                    this.instances.Add(objectIndex, weight);

                    HashSet<int> localObjectSet = null;
                    if (!this.decisionObjectIndexes.TryGetValue(decisionValue, out localObjectSet))
                    {
                        localObjectSet = new HashSet<int>();
                        this.decisionObjectIndexes[decisionValue] = localObjectSet;
                    }
                    localObjectSet.Add(objectIndex);
                }

                this.AddDecision(decisionValue, weight);

                //this.isStatCalculated = false;
            }
        }

        public void AddObjectInstances(Dictionary<int, decimal> instancesToAdd)
        {
            lock (mutex)
            {
                foreach (var kvp in instancesToAdd)
                    this.instances.Add(kvp.Key, kvp.Value);
            }
        }
        
        public void RemoveObject(int objectIndex)
        {
            lock (mutex)
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
        }                

        public int GetNumberOfObjectsWithDecision(long decisionValue)
        {
            HashSet<int> indices;
            if (this.decisionObjectIndexes.TryGetValue(decisionValue, out indices))
                return indices.Count;
            return 0;
        }

        public decimal GetDecisionProbability(long decisionValue)
        {
            return (decimal)this.GetNumberOfObjectsWithDecision(decisionValue) / (decimal)this.NumberOfObjects;
        }

        protected void CalcStatistics()
        {
            lock (mutex)
            {
                if (!this.isStatCalculated && this.UpdateStat)
                    this.DoCalcStatistics();
            }
        }

        public void RemoveObjectsWithMinorDecisions()
        {
            lock (mutex)
            {
                DoCalcStatistics();

                foreach (KeyValuePair<long, decimal> kvp in this.decisionWeigthSums)
                {
                    if (Decimal.Round(kvp.Value, 17) != Decimal.Round(this.majorDecisionWeightSum, 17))
                    {
                        foreach (int objectIdx in decisionObjectIndexes[kvp.Key])
                            this.instances.Remove(objectIdx);

                        decisionObjectIndexes.Remove(kvp.Key);
                    }
                }

                this.DoCalcStatistics();
            }
        }

        public virtual void KeepMajorDecisions(decimal epsilon)
        {
            if (this.DecisionSet.Count <= 1)
                return;

            lock (mutex)
            {               
                bool isFirst = true;
                var items = from kvp in decisionWeigthSums
                            orderby kvp.Value descending
                            select kvp;

                HashSet<long> decisionsToRemove = new HashSet<long>();
                decimal totalWeightToRemove = Decimal.Zero;
                decimal max = Decimal.MinValue;
                foreach (var kvp in items)
                {
                    if (isFirst)
                    {
                        max = kvp.Value;
                        isFirst = false;
                    }
                    else
                    {                        
                        if (Decimal.Round(kvp.Value, 17) < Decimal.Round(((Decimal.One - epsilon) * max), 17))
                        {
                            decisionsToRemove.Add(kvp.Key);
                            totalWeightToRemove += kvp.Value;
                        }
                    }
                }

                foreach (long decision in decisionsToRemove)
                {
                    this.DecisionSet.RemoveElement(decision);
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
        }        
        
        public decimal GetWeight(long decision)
        {
            CalcStatistics();            
            
            decimal ret = 0;
            if (this.decisionWeigthSums.TryGetValue(decision, out ret))
                return ret;
            return 0;
        }

        #region ICloneable Members
        /// <summary>
        /// Clones the EquivalenceClassCollection, performing a deep copy.
        /// </summary>
        /// <returns>A new newInstance of a EquivalenceClassCollection, using a deep copy.</returns>
        public virtual object Clone()
        {
            return new EquivalenceClass(this);
        }
        #endregion

        #region System.Object Methods

        public string ToString2()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendFormat("Number of objects: {0}", this.NumberOfObjects);
            stringBuilder.Append(Environment.NewLine);
            stringBuilder.AppendFormat("Number of decisions: {0}", this.NumberOfDecisions);
            stringBuilder.Append(Environment.NewLine);
            stringBuilder.AppendFormat("Major decision: {0}", this.DecisionSet.ToString());
            stringBuilder.Append(Environment.NewLine);

            return stringBuilder.ToString();
        }

        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendFormat("d=[{0}] ", this.DecisionSet);
            stringBuilder.AppendFormat("{0}", this.Instance.ToStr());
            //stringBuilder.AppendFormat("d=[{0}] ", this.DecisionSet);
            return stringBuilder.ToString();
        }

        #endregion

        #endregion
    }
}
