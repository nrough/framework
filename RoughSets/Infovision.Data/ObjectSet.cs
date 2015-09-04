using System;
using System.Collections.Generic;
using System.Text;
using Infovision.Utils;

namespace Infovision.Data
{
    public class ObjectSet : PascalSet, IObjectSetInfo
    {
        DataStore dataStore = null;
        Dictionary<Int64, Int32> decisionCount = new Dictionary<Int64, Int32>();
        
        #region Contructors

        public ObjectSet(DataStore dataStore, Int32[] initialData)
            : base(0, dataStore.NumberOfRecords-1, initialData)
        {
            this.dataStore = dataStore;
            this.InitDecisionCount(initialData);
        }

        public ObjectSet(DataStore dataStore)
            : this(dataStore, new Int32[] { }) 
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

        public Int32 NumberOfRecords
        {
            get { return this.Count; }
        }

        public Int32 NumberOfDecisionValues
        {
            get { return decisionCount.Keys.Count; }
        }

        public String CacheKey
        {
            get { return this.ToString(); }
        }

        #endregion

        #region Methods

        public ICollection<Int64> GetDecisionValues()
        {
            return this.decisionCount.Keys;
        }

        private void InitDecisionCount(Int32[] data)
        {
            this.decisionCount = new Dictionary<Int64, Int32>();
            for (Int32 i = 0; i < data.Length; i++)
            {
                Int64 decisionValue = this.dataStore.GetDecisionValue(data[i]);
                Int32 count = 0;
                this.decisionCount[decisionValue] = this.decisionCount.TryGetValue(decisionValue, out count) ? ++count : 1;
            }
        }

        public override void AddElement(Int32 element)
        {
            if (!this.ContainsElement(element))
            {
                Int64 decisionValue = this.dataStore.GetDecisionValue(element);
                Int32 count = 0;
                decisionCount[decisionValue] = decisionCount.TryGetValue(decisionValue, out count) ? ++count : 1;
            }

            base.AddElement(element);
        }

        public override void RemoveElement(Int32 element)
        {
            if (this.ContainsElement(element))
            {
                Int64 decisionValue = this.dataStore.GetDecisionValue(element);
                Int32 count = 0;
                decisionCount[decisionValue] = decisionCount.TryGetValue(decisionValue, out count) ? --count : 0;
            }
            
            base.RemoveElement(element);            
        }

        public static ObjectSet ConstructEmptyObjectSet(DataStore dataStore)
        {
            return new ObjectSet(dataStore);
        }

        public Double PriorDecisionProbability(Int64 decisionValue)
        {
            Int32 count = 0;
            if (this.decisionCount.TryGetValue(decisionValue, out count))
            {
                return (this.NumberOfRecords > 0) ? (Double)count / (Double)this.NumberOfRecords : 0;
            }
            return 0;
        }

        public Int32 NumberOfObjectsWithDecision(Int64 decisionValue)
        {
            Int32 count = 0;
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
            for (Int32 i = 0; i < this.Data.Length; i++)
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
