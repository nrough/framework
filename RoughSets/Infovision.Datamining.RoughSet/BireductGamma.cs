using System;
using System.Collections.Generic;
using Infovision.Data;

namespace Infovision.Datamining.Roughset
{
    [Serializable]
    public class BireductGamma : Bireduct
    {
        #region Constructors

        public BireductGamma(DataStore dataStore, IEnumerable<int> fieldIds, int[] objectIndexes, decimal epsilon)
            : base(dataStore, fieldIds, objectIndexes, epsilon)
        {
        }

        public BireductGamma(DataStore dataStore, IEnumerable<int> fieldIds, decimal epsilon)
            : this(dataStore, fieldIds, new int[] { }, epsilon)
        {
        }

        public BireductGamma(DataStore dataStore, decimal epsilon)
            : this(dataStore, dataStore.DataStoreInfo.GetFieldIds(FieldTypes.Standard), new int[] { }, epsilon)
        {
        }

        public BireductGamma(BireductGamma gammaBireduct)
            : this(gammaBireduct.DataStore, gammaBireduct.Attributes.ToArray(), gammaBireduct.ObjectSet.ToArray(), 
                   gammaBireduct.Epsilon)
        {

        }

        #endregion

        #region Methods

        protected override bool CheckRemoveAttribute(int attributeId)
        {

            if (base.CheckRemoveAttribute(attributeId) == false)
                return false;
            
            FieldSet newAttributeSet = (FieldSet) (this.Attributes - attributeId);
            
            //TODO Performance killer !!!!!!
            EquivalenceClassCollection localPartition = new EquivalenceClassCollection(this.DataStore);
            localPartition.Calc(newAttributeSet, this.DataStore);

            foreach (int objectIdx in this.ObjectSet)
            {
                var dataVector = this.DataStore.GetFieldValues(objectIdx, newAttributeSet);
                EquivalenceClass reductStat = localPartition.GetEquivalenceClass(dataVector);

                if (reductStat.NumberOfDecisions > 1)
                {
                    return false;
                }
            }

            return true;
        }

        protected override bool CheckAddObject(int objectIndex)
        {
            if (this.ObjectSet.ContainsElement(objectIndex))
                return false;
            var dataVector = this.DataStore.GetFieldValues(objectIndex, this.Attributes);

            //TODO  Performance killer !!!!!!
            EquivalenceClassCollection localPartition = new EquivalenceClassCollection(this.DataStore);
            localPartition.Calc(this.Attributes, this.DataStore);
                        
            EquivalenceClass eqClass = localPartition.GetEquivalenceClass(dataVector);

            if (eqClass.NumberOfDecisions > 1)
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

            BireductGamma gammaBireduct = obj as BireductGamma;
            if (gammaBireduct == null)
                return false;

            return base.Equals( (Bireduct) obj);
        }

        #endregion

        #region ICloneable Members
        /// <summary>
        /// Clones the GammaBireduct, performing a deep copy.
        /// </summary>
        /// <returns>A new newInstance of a GammaBireduct, using a deep copy.</returns>
        public override object Clone()
        {
            return new BireductGamma(this);
        }
        #endregion
    }
}
