using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Infovision.Data;
using Infovision.Utils;

namespace Infovision.Datamining.Roughset
{ 
    [Serializable]
    public class Reduct : IReduct, IFormattable
    {
        #region Members

        private FieldSet attributeSet;
        protected object mutex = new object();

        //TODO To Remove
        private decimal[] objectWeights;
        private DataStore dataStore;
        protected EquivalenceClassCollection eqClassMap;
        
        #endregion

        #region Properties

        public string Id { get; set; }

        public DataStore DataStore
        {
            get { return this.dataStore; }
        }
                 
        public decimal[] Weights
        {
            get { return this.objectWeights; }
            protected set { this.objectWeights = value; }
        }

        public FieldSet Attributes
        {
            get { return this.attributeSet; }
        }

        public decimal Epsilon { get; private set; }        

        public virtual ObjectSet ObjectSet
        {
            get { return new ObjectSet(this.dataStore, this.dataStore.GetObjectIndexes().ToArray()); }
        }

        public virtual IObjectSetInfo ObjectSetInfo
        {
            get { return this.dataStore.DataStoreInfo; }
        }

        public virtual EquivalenceClassCollection EquivalenceClasses
        {
            get 
            {
                if (this.eqClassMap == null)
                {
                    lock (mutex)
                    {
                        if (this.eqClassMap == null)
                        {
                            this.eqClassMap = EquivalenceClassCollection.Create(this, this.DataStore, this.Weights);
                        }
                    }
                }

                return this.eqClassMap; 
            }
            
            protected set 
            {
                lock (mutex)
                {
                    this.eqClassMap = value;
                }
            }
        }

        public virtual string ReductPartitionCacheKey
        {
            get
            {
                StringBuilder stringBuilder = new StringBuilder();
                stringBuilder.Append("m=Prt");
                stringBuilder.Append("|a=").Append(this.attributeSet.CacheKey);
                return stringBuilder.ToString();
            }
        }

        public virtual bool IsException { get; set; }
        public virtual bool IsLocal { get; set; }

        #endregion

        #region Constructors

        public Reduct(DataStore dataStore, IEnumerable<int> fieldIds, decimal epsilon, decimal[] weights)
        {
            this.dataStore = dataStore;
            this.attributeSet = new FieldSet(dataStore.DataStoreInfo, fieldIds);
            this.Epsilon = epsilon;

            if (weights != null)
            {
                this.objectWeights = weights;
            }
            else
            {
                this.objectWeights = new decimal[this.dataStore.NumberOfRecords];
                for (int i = 0; i < dataStore.NumberOfRecords; i++)
                    this.objectWeights[i] = Decimal.Divide(Decimal.One, this.dataStore.NumberOfRecords);
            }

        }

        public Reduct(DataStore dataStore, IEnumerable<int> fieldIds, decimal epsilon)
        {
            this.dataStore = dataStore;
            this.attributeSet = new FieldSet(dataStore.DataStoreInfo, fieldIds);
            this.Epsilon = epsilon;

            this.objectWeights = new decimal[this.dataStore.NumberOfRecords];
            decimal w = Decimal.Divide(Decimal.One, this.dataStore.NumberOfRecords);
            for (int i = 0; i < dataStore.NumberOfRecords; i++)
                this.objectWeights[i] = w;            
        }

        public Reduct(DataStore dataStore, decimal epsilon)
            : this(dataStore, new int[] { }, epsilon)
        {            
        }

        public Reduct(DataStore dataStore)
            : this(dataStore, new int[] { }, Decimal.Zero)
        {            
        }

        public Reduct(Reduct reduct)
        {
            this.attributeSet = new FieldSet(reduct.attributeSet);
            this.dataStore = reduct.DataStore;
            this.Epsilon = reduct.Epsilon;                                             
            this.objectWeights = new decimal[dataStore.NumberOfRecords];
            this.Id = reduct.Id;
            Array.Copy(reduct.Weights, this.objectWeights, reduct.DataStore.NumberOfRecords);

            this.eqClassMap = (EquivalenceClassCollection)reduct.EquivalenceClasses.Clone();
        }

        #endregion        

        #region Methods        
        
        public virtual EquivalenceClassCollection CreateEquivalenceClassCollection(bool useGlobalCache = false)
        {
            EquivalenceClassCollection result = null;
            string partitionKey = null;

            if (useGlobalCache)
            {
                partitionKey = this.ReductPartitionCacheKey;
                result = ReductCache.Instance.Get(partitionKey) as EquivalenceClassCollection;                
            }

            if (result == null)
            {
                result = new EquivalenceClassCollection();
                result.Calc(this.attributeSet, this.dataStore, this.objectWeights);
            }

            if (useGlobalCache)
                ReductCache.Instance.Set(partitionKey, result);

            return result;
        }

        protected virtual bool CheckAddAttribute(int attributeId)
        {
            return this.attributeSet.ContainsElement(attributeId) == false;
        }

        public virtual bool AddAttribute(int attributeId)
        {
            if (this.CheckAddAttribute(attributeId))
            {
                lock (mutex)
                {
                    this.attributeSet.AddElement(attributeId);
                    this.eqClassMap = null;
                }
                return true;
            }

            return false;
        }

        protected virtual bool CheckRemoveAttribute(int attributeId)
        {
            return this.attributeSet.ContainsElement(attributeId);
        }

        public virtual bool TryRemoveAttribute(int attributeId)
        {
            if(this.CheckRemoveAttribute(attributeId))
            {
                lock (mutex)
                {
                    this.attributeSet.RemoveElement(attributeId);
                    this.eqClassMap = null;
                }
                return true;
            }

            return false;
        }

        public virtual bool ContainsAttribute(int attributeId)
        {
            return this.attributeSet.ContainsElement(attributeId);
        }

        public virtual bool ContainsObject(int objectIndex)
        {
            return true;
        }

        #region ICloneable Members
        /// <summary>
        /// Clones the Reduct, performing a deep copy.
        /// </summary>
        /// <returns>A new newInstance of a FieldSet, using a deep copy.</returns>
        public virtual object Clone()
        {
            return new Reduct(this);
        }
        #endregion

        #region IComparable Members
        
        public virtual int CompareTo(object reduct)
        {
            Reduct r = reduct as Reduct;            
            return this.CompareTo(r);
        }

        public virtual int CompareTo(Reduct r)
        {
            if (r == null)
                return 1;

            int retval = this.Attributes.Count.CompareTo(r.Attributes.Count);

            if (retval != 0)
            {
                return retval;
            }

            for (int i = 0; i < this.Attributes.Count; i++)
            {
                int xval = Convert.ToInt32(this.Attributes.Data.Get(i));
                int yval = Convert.ToInt32(r.Attributes.Data.Get(i));
                if (xval < yval)
                {
                    return -1;
                }
                else if (xval > yval)
                {
                    return 1;
                }
            }

            return 0;
        }

        #endregion


        #region System.Object Methods

        public override string ToString()
        {
            return String.Format(
                "[Id:{0}] {1} (eps:{2})", 
                this.Id, 
                this.attributeSet.Count > 0 ? this.attributeSet.ToString() : "empty", 
                this.Epsilon);
        }

        public virtual string ToString(string format, IFormatProvider fp)
        {
            if (format != null && format.Equals("ext"))
            {
                StringBuilder sb = new StringBuilder();
                int[] fieldIds = this.attributeSet.ToArray();
                for (int i = 0; i < this.attributeSet.Count; i++)
                {
                    if (i != 0)
                        sb.Append(' ');
                    sb.Append(this.DataStore.DataStoreInfo.GetFieldInfo(fieldIds[i]).Alias);
                }

                return String.Format(
                    "[Id:{0}] {1} (eps:{2})",
                    this.Id,
                    this.attributeSet.Count > 0 ? sb.ToString() : "empty",
                    this.Epsilon);
            }
            else
                return this.ToString();
        }

        public override int GetHashCode()
        {
            return HashHelper.GetHashCode(this.attributeSet.Data);
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
           
            Reduct reduct = obj as Reduct;
            if (reduct == null)
                return false;

            return this.attributeSet.Equals(reduct.attributeSet);
        }
        #endregion

        #endregion
    }

    public class ReductNumericalEpsilonComparer : Comparer<IReduct>
    {
        public override int Compare(IReduct x, IReduct y)
        {
            if (x == null)
            {
                return y == null ? 0 : -1;                    
            }
            else
            {
                if (y == null)
                    return 1;
                else
                {
                    int retval = x.Attributes.Count.CompareTo(y.Attributes.Count);
                    if (retval != 0)
                        return retval;
                    else
                    {
                        for (int i = 0; i < x.Attributes.Data.Count; i++)
                        {
                            int xval = Convert.ToInt32(x.Attributes.Data.Get(i));
                            int yval = Convert.ToInt32(y.Attributes.Data.Get(i));
                            if (xval < yval)
                                return 1;
                            else if (xval > yval)
                                return -1;
                        }

                        return x.Epsilon.CompareTo(y.Epsilon);                        
                    }
                }
            }
        }
    }
}
