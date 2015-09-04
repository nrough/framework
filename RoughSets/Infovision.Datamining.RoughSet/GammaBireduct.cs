using System;
using Infovision.Data;

namespace Infovision.Datamining.Roughset
{
    [Serializable]
    public class GammaBireduct : Bireduct
    {
        #region Constructors
        
        public GammaBireduct(DataStore dataStore, int[] fieldIds, int[] objectIndexes)
            : base(dataStore, fieldIds, objectIndexes)
        {
        }

        public GammaBireduct(DataStore dataStore, int[] fieldIds)
            : this(dataStore, fieldIds, new int[] {})
        {
        }

        public GammaBireduct(DataStore dataStore)
            : this(dataStore, dataStore.DataStoreInfo.GetFieldIds(FieldTypes.Standard), new int[] {})
        {
        }

        public GammaBireduct(GammaBireduct gammaBireduct)
            : this(gammaBireduct.DataStore, gammaBireduct.AttributeSet.ToArray(), gammaBireduct.ObjectSet.ToArray())
        {

        }

        #endregion

        #region Methods

        protected override bool CheckRemoveAttribute(int attributeId)
        {

            if (base.CheckRemoveAttribute(attributeId) == false)
                return false;
            
            FieldSet newAttributeSet = (FieldSet) (this.AttributeSet - attributeId);
            
            EquivalenceClassMap localPartition = new EquivalenceClassMap(this.DataStore.DataStoreInfo);
            localPartition.Calc(newAttributeSet, this.DataStore);

            foreach (int objectIdx in this.ObjectSet)
            {
                DataVector dataVector = this.DataStore.GetDataVector(objectIdx, newAttributeSet);
                EquivalenceClassInfo reductStat = localPartition.GetStatistics(dataVector);

                if (reductStat.NumberOfDecisions > 1)
                {
                    return false;
                }
            }

            return true;
        }

        protected override bool CheckAddObject(int objectIndex)
        {         
            EquivalenceClassMap localPartition = new EquivalenceClassMap(this.DataStore.DataStoreInfo);
            localPartition.Calc(this.AttributeSet, this.DataStore);
            
            DataVector dataVector = this.DataStore.GetDataVector(objectIndex, this.AttributeSet);
            EquivalenceClassInfo reductStatistics = localPartition.GetStatistics(dataVector);

            if (reductStatistics.NumberOfDecisions > 1)
            {
                return false;
            }

            return true;
        }

        #endregion

        #region System.Object Methods

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            GammaBireduct gammaBireduct = obj as GammaBireduct;
            if (gammaBireduct == null)
                return false;

            return base.Equals( (Bireduct) obj);
        }

        #endregion

        #region ICloneable Members
        /// <summary>
        /// Clones the GammaBireduct, performing a deep copy.
        /// </summary>
        /// <returns>A new instance of a GammaBireduct, using a deep copy.</returns>
        public override object Clone()
        {
            return new GammaBireduct(this);
        }
        #endregion
    }
}
