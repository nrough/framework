using System;
using System.Collections.Generic;
using System.Text;
using Infovision.Utils;

namespace Infovision.Data
{
    public class ObjectSet : PascalSet<int>, IObjectSetInfo
    {
        DataStore dataStore = null;
        //decimal[] weights = null;
        Dictionary<long, int> decisionCount;
        //Dictionary<long, decimal> decisionWeight;
        private object mutex = new object();
                
        #region Properties

        public DataStore DataStore
        {
            get { return this.dataStore; }
        }

        public int NumberOfRecords
        {
            get { return this.Count; }
        }

        public int NumberOfDecisionValues
        {
            get { return decisionCount.Keys.Count; }
        }

        public string CacheKey
        {
            get { return this.ToString(); }
        }

        #endregion

        #region Constructors

        //public ObjectSet(DataStore dataStore, int[] initialData, decimal[] weights = null)
        public ObjectSet(DataStore dataStore, int[] initialData)
            : base(0, dataStore.NumberOfRecords - 1, initialData)
        {
            this.dataStore = dataStore;
            this.decisionCount = new Dictionary<long, int>(this.dataStore.DataStoreInfo.NumberOfDecisionValues);
            //this.weights = weights;
            //this.decisionWeight = new Dictionary<long, decimal>(this.dataStore.DataStoreInfo.NumberOfDecisionValues);
            int decisionIndex = this.dataStore.DataStoreInfo.DecisionFieldIndex;
            for (int i = 0; i < initialData.Length; i++)
            {
                long decisionValue = this.dataStore.GetFieldIndexValue(initialData[i], decisionIndex);
                
                int count = 0;
                this.decisionCount[decisionValue] = this.decisionCount.TryGetValue(decisionValue, out count) ? ++count : 1;
                
                /*
                decimal w = 0;
                if (weights != null)
                    this.decisionWeight[decisionValue] = this.decisionWeight.TryGetValue(decisionValue, out w) ? (w + weights[i]) : weights[i];
                else
                    this.decisionWeight[decisionValue] = this.decisionWeight.TryGetValue(decisionValue, out w) ? (w + 1) : 1;
                */
            }
        }

        public ObjectSet(DataStore dataStore)
            //: this(dataStore, new int[] { }, null)
            : this(dataStore, new int[] { })
        {
        }

        public ObjectSet(ObjectSet objectSet)
            //: this(objectSet.DataStore, objectSet.ToArray(), objectSet.weights)
            : this(objectSet.DataStore, objectSet.ToArray())
        {
        }

        #endregion

        #region Methods

        public ICollection<long> GetDecisionValues()
        {
            return this.decisionCount.Keys;
        }

        //public void AddElement(int element, decimal weight = 1)
        public override void AddElement(int element)
        {
            if (!this.ContainsElement(element))
            {
                long decisionValue = this.dataStore.GetDecisionValue(element);
                
                int count = 0;
                this.decisionCount[decisionValue] = this.decisionCount.TryGetValue(decisionValue, out count) ? ++count : 1;
                
                //decimal w = 0;
                //this.decisionWeight[decisionValue] = this.decisionWeight.TryGetValue(decisionValue, out w) ? (w + weight) : weight;
            }

            base.AddElement(element);
        }

        public override void RemoveElement(int element)
        {
            if (this.ContainsElement(element))
            {
                long decisionValue = this.dataStore.GetDecisionValue(element);
                
                int count = 0;
                this.decisionCount[decisionValue] = this.decisionCount.TryGetValue(decisionValue, out count) ? --count : 0;
                
                /*
                decimal w = 0;
                if(this.weights != null)
                    this.decisionWeight[decisionValue] = this.decisionWeight.TryGetValue(decisionValue, out w) ? (w - weights[element]) : 0;
                else
                    this.decisionWeight[decisionValue] = this.decisionWeight.TryGetValue(decisionValue, out w) ? (w - 1) : 0;
                */
            }
            
            base.RemoveElement(element);            
        }

        public static ObjectSet ConstructEmptyObjectSet(DataStore dataStore)
        {
            return new ObjectSet(dataStore);
        }        

        public int NumberOfObjectsWithDecision(long decisionValue)
        {
            int count = 0;
            if (this.decisionCount.TryGetValue(decisionValue, out count))
                return count;
            return 0;
        }

        /*
        public decimal DecisionWeight(long decisionValue)
        {
            if (weights == null)
                return (decimal)this.NumberOfObjectsWithDecision(decisionValue);
            decimal w = 0;
            if (this.decisionWeight.TryGetValue(decisionValue, out w))
                return w;
            return 0;
        }
        */

        #region System.Object Methods

        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder(this.Data.Length * 3);
            for (int i = 0; i < this.Data.Length; i++)
            {
                if (this.Data.Get(i))
                {
                    stringBuilder.Digits(i + this.LowerBound).Append(' ');
                }
            }

            return stringBuilder.ToString();
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            ObjectSet o = obj as ObjectSet;
            if (o == null)
                return false;

            return base.Equals((PascalSet<int>)obj);
        }
        #endregion

        #region ICloneable Members
        /// <summary>
        /// Clones the ObjectSet, performing a deep copy.
        /// </summary>
        /// <returns>A new instance of a FieldSet, using a deep copy.</returns>
        public override object Clone()
        {
            return new ObjectSet(this);
        }

        #endregion

        #endregion
    }
}
