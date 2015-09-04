using System;
using System.Text;
using Infovision.Data;
using Infovision.Utils;

namespace Infovision.Datamining.Roughset
{
    [Serializable]
    public class Reduct : IReduct
    {
        #region Globals

        private DataStore dataStore;
        private FieldSet attributeSet;
        private EquivalenceClassMap eqClassMap;
        
        protected double[] objectWeights;
        //TODO
        //private List<int> attributeOrder;

        #endregion

        #region Constructors

        public Reduct(DataStore dataStore, int [] fieldIds)
        {
            this.dataStore = dataStore;
            this.attributeSet = new FieldSet(dataStore.DataStoreInfo, fieldIds);
            //this.attributeOrder = new List<int>(fieldIds);
            this.BuildEquivalenceMap(true);
            this.objectWeights = new double[dataStore.NumberOfRecords];
            
            for (int i = 0; i < dataStore.NumberOfRecords; i++)
                this.objectWeights[i] = (double)1 / (double)this.DataStore.NumberOfRecords;
        }

        public Reduct(DataStore dataStore)
            : this(dataStore, new int[] { })
        {
            //set size to number of fields minus one (decision attribute)
            //this.attributeOrder = new List<int>(dataStore.DataStoreInfo.NumberOfFields - 1);
        }

        public Reduct(Reduct reduct)
        {
            this.attributeSet = new FieldSet(reduct.attributeSet);
            this.dataStore = reduct.DataStore;
            
            //this.attributeOrder = new List<int>(reduct.attributeOrder);
            
            this.eqClassMap = (EquivalenceClassMap) reduct.EquivalenceClassMap.Clone();
            this.objectWeights = new double[dataStore.NumberOfRecords];
            Buffer.BlockCopy(reduct.Weights, 0, this.objectWeights, 0, reduct.DataStore.NumberOfRecords * sizeof(double));            
        }

        #endregion

        #region Properties

        public DataStore DataStore
        {
            get { return this.dataStore; }
        }

        public virtual double[] Weights
        {
            get { return this.objectWeights; }
        }

        public FieldSet AttributeSet
        {
            get { return this.attributeSet; }
        }

        public virtual ObjectSet ObjectSet
        {
            get { return new ObjectSet(this.dataStore, this.dataStore.GetObjectIndexes()); }
        }

        public virtual IObjectSetInfo ObjectSetInfo
        {
            get { return this.dataStore.DataStoreInfo; }
        }

        public EquivalenceClassMap EquivalenceClassMap
        {
            get { return this.eqClassMap; }
            protected set { this.eqClassMap = value; }
        }

        protected string ReductPartitionCacheKey
        {
            get
            {
                StringBuilder stringBuilder = new StringBuilder();

                stringBuilder.Append("m=Partition");
                stringBuilder.Append("|a=").Append(this.attributeSet.CacheKey);
                stringBuilder.Append("|d=").Append(this.dataStore.Name);
                
                return stringBuilder.ToString();
            }
        }

        #endregion

        #region Methods

        protected void InitEquivalenceMap()
        {
            eqClassMap = new EquivalenceClassMap(this.DataStore.DataStoreInfo);
        }
        
        public virtual void BuildEquivalenceMap(bool useCache = true)
        {

            if (useCache == false)
            {
                this.InitEquivalenceMap();
                eqClassMap.Calc(this.attributeSet, this.dataStore);
                return;
            }

            string partitionKey = this.ReductPartitionCacheKey;
            eqClassMap = ReductCache.Instance.Get(partitionKey) as EquivalenceClassMap;
            if (eqClassMap == null)
            {
                this.InitEquivalenceMap();
                eqClassMap.Calc(this.attributeSet, this.dataStore);
                ReductCache.Instance.Set(partitionKey, eqClassMap);
            }
        }

        protected virtual bool CheckAddAttribute(int attributeId)
        {
            return this.attributeSet.ContainsElement(attributeId) == false;
        }

        public virtual bool AddAttribute(int attributeId)
        {
            if (this.CheckAddAttribute(attributeId))
            {
                this.attributeSet.AddElement(attributeId);
                this.BuildEquivalenceMap(true);
                return true;
            }

            return false;
        }

        protected virtual bool CheckRemoveAttribute(int attributeId)
        {
            return this.attributeSet.ContainsElement(attributeId) == true;
        }

        //TODO removeAt
        public virtual bool RemoveAttribute(int attributeId)
        {
            if(this.CheckRemoveAttribute(attributeId))
            {
                this.attributeSet.RemoveElement(attributeId);
                //this.attributeOrder.Remove(attributeId);
                this.BuildEquivalenceMap(true);
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
        /// <returns>A new instance of a FieldSet, using a deep copy.</returns>
        public virtual object Clone()
        {
            return new Reduct(this);
        }
        #endregion


        #region System.Object Methods

        public override string ToString()
        {
            return this.attributeSet.ToString();
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
}
