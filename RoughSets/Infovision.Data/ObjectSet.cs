using System;
using System.Collections.Generic;
using System.Text;
using Infovision.Utils;

namespace Infovision.Data
{
    public class ObjectSet : PascalSet, IObjectSetInfo
    {
        DataStore dataStore = null;
        Dictionary<long, int> decisionCount = new Dictionary<long, int>();
        
        #region Contructors

        public ObjectSet(DataStore dataStore, int[] initialData)
            : base(0, dataStore.NumberOfRecords-1, initialData)
        {
            this.dataStore = dataStore;
            this.InitDecisionCount(initialData);
        }

        public ObjectSet(DataStore dataStore)
            : this(dataStore, new int[] { }) 
        {
        }

        public ObjectSet(ObjectSet objectSet)
            : this(objectSet.DataStore, objectSet.ToArray())
        {
        }

        #endregion

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

        #region Methods

        public ICollection<long> GetDecisionValues()
        {
            return this.decisionCount.Keys;
        }

        private void InitDecisionCount(int[] data)
        {
            this.decisionCount = new Dictionary<long, int>();
            for (int i = 0; i < data.Length; i++)
            {
                long decisionValue = this.dataStore.GetDecisionValue(data[i]);
                int count = 0;
                this.decisionCount[decisionValue] = this.decisionCount.TryGetValue(decisionValue, out count) ? ++count : 1;
            }
        }

        public override void AddElement(int element)
        {
            if (!this.ContainsElement(element))
            {
                long decisionValue = this.dataStore.GetDecisionValue(element);
                int count = 0;
                decisionCount[decisionValue] = decisionCount.TryGetValue(decisionValue, out count) ? ++count : 1;
            }

            base.AddElement(element);
        }

        public override void RemoveElement(int element)
        {
            if (this.ContainsElement(element))
            {
                long decisionValue = this.dataStore.GetDecisionValue(element);
                int count = 0;
                decisionCount[decisionValue] = decisionCount.TryGetValue(decisionValue, out count) ? --count : 0;
            }
            
            base.RemoveElement(element);            
        }

        public static ObjectSet ConstructEmptyObjectSet(DataStore dataStore)
        {
            return new ObjectSet(dataStore);
        }

        public double PriorDecisionProbability(long decisionValue)
        {
            int count = 0;
            if (this.decisionCount.TryGetValue(decisionValue, out count))
            {
                return (this.NumberOfRecords > 0) ? (double)count / (double)this.NumberOfRecords : 0;
            }
            return 0;
        }

        public int NumberOfObjectsWithDecision(long decisionValue)
        {
            int count = 0;
            if (this.decisionCount.TryGetValue(decisionValue, out count))
            {
                return count;
            }
            return 0;
        }

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

            return base.Equals((PascalSet)obj);
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
