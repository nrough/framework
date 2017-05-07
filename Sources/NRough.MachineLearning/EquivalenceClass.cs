using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NRough.Data;
using NRough.Core;
using NRough.Core.CollectionExtensions;

namespace NRough.MachineLearning
{
    [Serializable]
    public class EquivalenceClass : ICloneable
    {
        #region Members

        private long[] dataVector;
        private Dictionary<int, double> instances;  //map: objectIdx -> objectWeight        
        private HashSet<long> decisionSet;  //set containing all decisions within this class
        private Dictionary<long, double> decisionWeight; //map decisionInternalValue -> objectWeight
        private Dictionary<long, int> decisionCount; //map decisionInternalValue -> object value
        private double avgConfidenceWeight;
        private int confidenceCount;
        private readonly object syncRoot = new object();

        #endregion Members

        #region Properties

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

        public double AvgConfidenceWeight
        {
            get { return this.avgConfidenceWeight; }
            set { this.avgConfidenceWeight = value; }
        }

        public int AvgConfidenceSum
        {
            get { return this.confidenceCount; }
            set { this.confidenceCount = value; }
        }

        public IEnumerable<int> ObjectIndexes
        {
            get { return this.instances.Keys; }
        }

        public double WeightSum
        {
            get;
            internal set;
        }

        public IEnumerable<long> DecisionValues
        {
            get { return this.decisionSet; }
        }

        public HashSet<long> DecisionSet
        {
            get { return decisionSet; }
            internal set { this.decisionSet = value; }
        }

        public Dictionary<int, double> Instances
        {
            get { return this.instances; }
            internal set { this.instances = value; }
        }

        public Dictionary<long, double> DecisionWeight
        {
            get { return this.decisionWeight; }
            internal set { this.decisionWeight = value; }
        }

        public Dictionary<long, int> DecisionCount
        {
            get { return this.decisionCount; }
            internal set { this.decisionCount = value; }
        }

        public DecisionDistribution DecisionDistribution
        {
            get
            {
                return new DecisionDistribution(this.DecisionWeight);
            }
        }

        #endregion Properties

        #region Constructors

        public EquivalenceClass(long[] dataVector)
        {
            this.dataVector = dataVector;
            this.decisionSet = new HashSet<long>();
            this.instances = new Dictionary<int, double>();            
            this.decisionWeight = new Dictionary<long, double>();
            this.decisionCount = new Dictionary<long, int>();
        }        

        public EquivalenceClass(long[] dataVector, Dictionary<int, double> instances, HashSet<long> decSet)
        {
            this.dataVector = dataVector;
            this.decisionSet = new HashSet<long>(decSet);
            this.instances = new Dictionary<int, double>(instances);
            this.WeightSum = instances.Values.Sum();            
            int numberOfDecisions = decisionSet.Count;
            this.decisionWeight = new Dictionary<long, double>(numberOfDecisions);
            this.decisionCount = new Dictionary<long, int>(numberOfDecisions);
        }        

        public EquivalenceClass(long[] dataVector, DataStore data)
        {
            this.dataVector = dataVector;            
            this.decisionSet = new HashSet<long>();
            this.instances = new Dictionary<int, double>();
            int numberOfDecisions = data.DataStoreInfo.DecisionInfo.NumberOfValues;
            this.decisionWeight = new Dictionary<long, double>(numberOfDecisions);
            this.decisionCount = new Dictionary<long, int>(numberOfDecisions);
        }

        public EquivalenceClass(EquivalenceClass eqClass)
        {
            this.dataVector = new long[eqClass.dataVector.Length];
            Array.Copy(eqClass.dataVector, this.dataVector, eqClass.dataVector.Length);
            this.instances = new Dictionary<int, double>(eqClass.instances);
            this.decisionWeight = new Dictionary<long, double>(eqClass.decisionWeight);
            this.decisionCount = new Dictionary<long, int>(eqClass.decisionCount);
            this.decisionSet = new HashSet<long>(eqClass.DecisionSet);
            this.WeightSum = eqClass.WeightSum;
            this.AvgConfidenceWeight = eqClass.AvgConfidenceWeight;
            this.AvgConfidenceSum = eqClass.AvgConfidenceSum;
        }

        public EquivalenceClass(long[] dataVector, EquivalenceClass eqClass)
        {
            this.dataVector = dataVector;            
            this.instances = new Dictionary<int, double>(eqClass.instances);
            this.decisionWeight = new Dictionary<long, double>(eqClass.decisionWeight);
            this.decisionCount = new Dictionary<long, int>(eqClass.decisionCount);
            this.decisionSet = new HashSet<long>(eqClass.DecisionSet);
            this.WeightSum = eqClass.WeightSum;
            this.AvgConfidenceWeight = eqClass.AvgConfidenceWeight;
            this.AvgConfidenceSum = eqClass.AvgConfidenceSum;
        }

        public EquivalenceClass(EquivalenceClass eqClass, long[] dataVector)
        {
            this.dataVector = dataVector;
            this.instances = new Dictionary<int, double>(eqClass.instances);
            this.decisionWeight = new Dictionary<long, double>(eqClass.decisionWeight);
            this.decisionCount = new Dictionary<long, int>(eqClass.decisionCount);
            this.decisionSet = new HashSet<long>(eqClass.DecisionSet);
            this.WeightSum = eqClass.WeightSum;
            this.AvgConfidenceWeight = eqClass.AvgConfidenceWeight;
            this.AvgConfidenceSum = eqClass.AvgConfidenceSum;
        }

        #endregion Constructors

        #region Methods

        public double GetDecisionWeight(long decision)
        {
            double result = 0;
            if (this.decisionWeight.TryGetValue(decision, out result))
                return result;
            return 0;
        }

        public void RecalcStatistics(DataStore data)
        {
            lock (syncRoot)
            {
                this.WeightSum = 0;
                int decCount = this.DecisionSet.Count;
                this.decisionWeight = new Dictionary<long, double>(decCount);
                this.decisionCount = new Dictionary<long, int>(decCount);
                foreach (var instance in this.instances)
                {
                    long decision = data.GetDecisionValue(instance.Key);
                    double w = 0; int count = 0;
                    this.decisionWeight[decision] = this.decisionWeight.TryGetValue(decision, out w) ? (w + instance.Value) : instance.Value;
                    this.decisionCount[decision] = this.decisionCount.TryGetValue(decision, out count) ? ++count : 1;
                    this.WeightSum += instance.Value;
                }
            }
        }

        public void AddDecision(long decision, double weight)
        {
            lock (syncRoot)
            {
                this.decisionSet.Add(decision);
                this.WeightSum += weight;

                if(this.decisionCount.ContainsKey(decision))
                {
                    this.decisionWeight[decision] += weight;
                    this.decisionCount[decision] += 1;
                }
                else
                {
                    this.decisionWeight.Add(decision, weight);
                    this.decisionCount.Add(decision, 1);
                }                
            }
        }

        public void RemoveDecision(long decision, double weight)
        {
            if (this.decisionCount.ContainsKey(decision))
            {
                this.decisionCount[decision]--;
                this.decisionWeight[decision] -= weight;
                this.WeightSum -= weight;
            }                        
        }

        public void AddObject(int objectIndex, long decision, double weight)
        {
            lock (syncRoot)
            {
                if (objectIndex != -1)
                    this.instances.Add(objectIndex, weight);
                this.AddDecision(decision, weight);
            }
        }

        public void AddObjectInstances(Dictionary<int, double> instancesToAdd)
        {
            lock (syncRoot)
            {
                foreach (var kvp in instancesToAdd)
                    this.instances.Add(kvp.Key, kvp.Value);
            }
        }

        public void RemoveObject(int objectIndex, DataStore data)
        {
            lock (syncRoot)
            {
                if (this.instances.ContainsKey(objectIndex))
                {                    
                    this.RemoveDecision(data.GetDecisionValue(objectIndex), this.instances[objectIndex]);
                    this.instances.Remove(objectIndex);
                }
            }
        }

        public void RemoveObject(int objectIndex, long decision, double weight)
        {
            lock (syncRoot)
            {
                if (this.instances.ContainsKey(objectIndex))
                {                                        
                    this.RemoveDecision(decision, weight);
                    this.instances.Remove(objectIndex);
                }
            }
        }

        public int GetNumberOfObjectsWithDecision(long decisionValue)
        {
            int count = 0;
            if (this.decisionCount.TryGetValue(decisionValue, out count))
                return count;
            return 0;
        }

        public virtual void KeepMajorDecisions(double epsilon)
        {
            if (this.DecisionSet.Count <= 1) return;
            if (epsilon >= 1.0) return;

            lock (syncRoot)
            {
                var list = decisionWeight.ToList();
                list.Sort((pair1, pair2) => pair2.Value.CompareTo(pair1.Value)); //descending order
                double max = list[0].Value;
                double threshold = (1.0 - epsilon) * max;
                for (int i = 1; i < list.Count; i++)
                {
                    if (list[i].Value < threshold)
                    {
                        this.DecisionSet.Remove(list[i].Key);
                        this.decisionWeight.Remove(list[i].Key);
                        this.decisionCount.Remove(list[i].Key);
                    }
                }
            }
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

        #endregion ICloneable Members

        #region System.Object Methods

        public string ToString2()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendFormat("Number of objects: {0}", this.NumberOfObjects);
            stringBuilder.Append(Environment.NewLine);
            stringBuilder.AppendFormat("Number of decisions: {0}", this.NumberOfDecisions);
            stringBuilder.Append(Environment.NewLine);
            stringBuilder.AppendFormat("Major decisionInternalValue: {0}", this.DecisionSet.ToString());
            stringBuilder.Append(Environment.NewLine);

            return stringBuilder.ToString();
        }

        public override string ToString()
        {
            return this.ToStringInt();
        }

        public string ToStringInt()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendFormat("d=[{0}] ", this.DecisionSet);
            stringBuilder.AppendFormat("{0}", this.Instance.ToStr());
            //stringBuilder.AppendFormat("d=[{0}] ", this.DecisionSet);
            return stringBuilder.ToString();
        }

        public string ToStringExt(DataStore data, int[] fieldIds)
        {
            if (fieldIds.Length != this.Instance.Length)
                throw new ArgumentException("Field Id array has different length than current key array", "fieldIds");

            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append("d=[");
            bool first = true;
            foreach (var element in this.DecisionSet)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    stringBuilder.Append(' ');
                }
                stringBuilder.Append(data.DataStoreInfo.DecisionInfo.Internal2External(element));
            }
            stringBuilder.Append("] ");

            first = true;
            for (int i = 0; i < this.Instance.Length; i++)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    stringBuilder.Append(' ');
                }
                stringBuilder.Append(data.DataStoreInfo.GetFieldInfo(fieldIds[i]).Internal2External(this.Instance[i]));
            }

            return stringBuilder.ToString();
        }

        #endregion System.Object Methods

        #endregion Methods
    }
}