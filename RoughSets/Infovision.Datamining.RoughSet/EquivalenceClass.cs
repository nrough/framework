using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Infovision.Data;
using Infovision.Utils;

namespace Infovision.Datamining.Roughset
{
    [Serializable]
    public class EquivalenceClass : ICloneable
    {
        #region Members

        private long[] dataVector;
        private Dictionary<int, decimal> instances;  //map: objectIdx -> objectWeight

        //private decimal totalWeightSum; //sum of object weights belonging to this class

        private HashSet<long> decisionSet;  //set containing all decisions within this class

        private Dictionary<long, decimal> decisionWeightSums; //map decisionInternalValue -> objectWeight
        private Dictionary<long, int> decisionCount; //map decisionInternalValue -> object value

        private decimal avgConfidenceWeight;
        private int confidenceCount;

        private readonly object mutex = new object();

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

        public decimal AvgConfidenceWeight
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

        public decimal WeightSum
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

        public Dictionary<int, decimal> Instances
        {
            get { return this.instances; }
            internal set { this.instances = value; }
        }

        public Dictionary<long, decimal> DecisionWeights
        {
            get { return this.decisionWeightSums; }
            internal set { this.decisionWeightSums = value; }
        }

        public Dictionary<long, int> DecisionCount
        {
            get { return this.decisionCount; }
            internal set { this.decisionCount = value; }
        }

        #endregion Properties

        #region Constructors

        public EquivalenceClass(long[] dataVector, Dictionary<int, decimal> instances, HashSet<long> decSet)
        {
            this.dataVector = dataVector;
            this.decisionSet = new HashSet<long>(decSet);
            this.instances = new Dictionary<int, decimal>(instances);
            int numberOfDecisions = decisionSet.Count;
            this.decisionWeightSums = new Dictionary<long, decimal>(numberOfDecisions);
            this.decisionCount = new Dictionary<long, int>(numberOfDecisions);
        }

        public EquivalenceClass(long[] dataVector, DataStore data, Dictionary<int, decimal> instances)
        {
            this.dataVector = dataVector;
            DataFieldInfo decisionField = data.DataStoreInfo.DecisionInfo;
            this.decisionSet = new HashSet<long>();
            this.instances = new Dictionary<int, decimal>(instances);
            int numberOfDecisions = decisionField.NumberOfValues;
            this.decisionWeightSums = new Dictionary<long, decimal>(numberOfDecisions);
            this.decisionCount = new Dictionary<long, int>(numberOfDecisions);
        }

        public EquivalenceClass(long[] dataVector, DataStore data, int capacity)
        {
            this.dataVector = dataVector;
            DataFieldInfo decisionField = data.DataStoreInfo.DecisionInfo;
            this.decisionSet = new HashSet<long>();

            if (capacity > 0)
                this.instances = new Dictionary<int, decimal>(capacity);
            else
                this.instances = new Dictionary<int, decimal>();

            int numberOfDecisions = decisionField.NumberOfValues;
            this.decisionWeightSums = new Dictionary<long, decimal>(numberOfDecisions);
            this.decisionCount = new Dictionary<long, int>(numberOfDecisions);
        }

        public EquivalenceClass(long[] dataVector, DataStore data)
        {
            this.dataVector = dataVector;
            //DataFieldInfo decisionField = data.DataStoreInfo.DecisionInfo;
            this.decisionSet = new HashSet<long>();
            this.instances = new Dictionary<int, decimal>();
            int numberOfDecisions = data.DataStoreInfo.DecisionInfo.NumberOfValues;
            this.decisionWeightSums = new Dictionary<long, decimal>(numberOfDecisions);
            this.decisionCount = new Dictionary<long, int>(numberOfDecisions);
        }

        public EquivalenceClass(EquivalenceClass eqClass)
        {
            this.dataVector = new long[eqClass.dataVector.Length];
            Array.Copy(eqClass.dataVector, this.dataVector, eqClass.dataVector.Length);
            this.instances = new Dictionary<int, decimal>(eqClass.instances);
            this.decisionWeightSums = new Dictionary<long, decimal>(eqClass.decisionWeightSums);
            this.decisionCount = new Dictionary<long, int>(eqClass.decisionCount);
            this.decisionSet = new HashSet<long>(eqClass.DecisionSet);
            this.WeightSum = eqClass.WeightSum;
            this.AvgConfidenceWeight = eqClass.AvgConfidenceWeight;
            this.AvgConfidenceSum = eqClass.AvgConfidenceSum;
        }

        public EquivalenceClass(EquivalenceClass eqClass, long[] dataVector)
        {
            this.dataVector = dataVector;
            this.instances = new Dictionary<int, decimal>(eqClass.instances);
            this.decisionWeightSums = new Dictionary<long, decimal>(eqClass.decisionWeightSums);
            this.decisionCount = new Dictionary<long, int>(eqClass.decisionCount);
            this.decisionSet = new HashSet<long>(eqClass.DecisionSet);
            this.WeightSum = eqClass.WeightSum;
            this.AvgConfidenceWeight = eqClass.AvgConfidenceWeight;
            this.AvgConfidenceSum = eqClass.AvgConfidenceSum;
        }

        #endregion Constructors

        #region Methods

        public decimal GetDecisionWeight(long decision)
        {
            decimal result = 0;
            if (this.decisionWeightSums.TryGetValue(decision, out result))
                return result;
            return 0;
        }

        public void RecalcStatistics(DataStore data)
        {
            lock (mutex)
            {
                this.WeightSum = 0;
                int decCount = this.DecisionSet.Count;
                this.decisionWeightSums = new Dictionary<long, decimal>(decCount);
                this.decisionCount = new Dictionary<long, int>(decCount);
                int decisionIndex = data.DataStoreInfo.DecisionFieldIndex;
                foreach (var instance in this.instances)
                {
                    long decision = data.GetFieldIndexValue(instance.Key, decisionIndex);
                    decimal w = 0; int count = 0;
                    this.decisionWeightSums[decision] = this.decisionWeightSums.TryGetValue(decision, out w) ? (w + instance.Value) : instance.Value;
                    this.decisionCount[decision] = this.decisionCount.TryGetValue(decision, out count) ? ++count : 1;
                    this.WeightSum += instance.Value;
                }
            }
        }

        public void AddDecision(long decisionValue, decimal weight)
        {
            lock (mutex)
            {
                this.decisionSet.Add(decisionValue);
                this.WeightSum += weight;

                decimal weightSum = Decimal.Zero;
                if (this.decisionWeightSums.TryGetValue(decisionValue, out weightSum))
                {
                    weight += weightSum;
                    this.decisionWeightSums[decisionValue] = weight;
                }
                else
                {
                    this.decisionWeightSums.Add(decisionValue, weight);
                }

                int count = 0;
                if (this.decisionCount.TryGetValue(decisionValue, out count))
                {
                    count += 1;
                    this.decisionCount[decisionValue] = count;
                }
                else
                {
                    this.decisionCount.Add(decisionValue, 1);
                }
            }
        }

        public void RemoveDecision(long decisionValue, decimal weight)
        {
            int count = 0;
            if (this.decisionCount.TryGetValue(decisionValue, out count))
                this.decisionCount[decisionValue] = --count;

            decimal w = 0;
            if (this.decisionWeightSums.TryGetValue(decisionValue, out w))
                this.decisionWeightSums[decisionValue] = (w - weight);

            this.WeightSum -= weight;
        }

        public void AddObject(int objectIndex, long decisionValue, decimal weight)
        {
            lock (mutex)
            {
                this.instances.Add(objectIndex, weight);
                this.AddDecision(decisionValue, weight);
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

        public void RemoveObject(int objectIndex, DataStore data)
        {
            lock (mutex)
            {
                if (this.instances.ContainsKey(objectIndex))
                {
                    long decisionValue = data.GetDecisionValue(objectIndex);
                    decimal weight = this.instances[objectIndex];
                    this.RemoveDecision(decisionValue, weight);
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

        public virtual void KeepMajorDecisions(decimal epsilon)
        {
            if (this.DecisionSet.Count <= 1) return;
            if (epsilon >= Decimal.One) return;

            lock (mutex)
            {
                var list = decisionWeightSums.ToList();
                list.Sort((pair1, pair2) => pair2.Value.CompareTo(pair1.Value)); //descending order
                decimal max = list[0].Value;
                decimal threshold = Decimal.Round(((Decimal.One - epsilon) * max), 17);
                for (int i = 1; i < list.Count; i++)
                {
                    if (Decimal.Round(list[i].Value, 17) < threshold)
                    {
                        this.DecisionSet.Remove(list[i].Key);
                        this.decisionWeightSums.Remove(list[i].Key);
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