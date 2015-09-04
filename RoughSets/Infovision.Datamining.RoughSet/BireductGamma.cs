﻿using System;
using Infovision.Data;

namespace Infovision.Datamining.Roughset
{
    [Serializable]
    public class BireductGamma : Bireduct
    {
        #region Constructors

        public BireductGamma(DataStore dataStore, int[] fieldIds, int[] objectIndexes, double epsilon)
            : base(dataStore, fieldIds, objectIndexes, epsilon)
        {
        }

        public BireductGamma(DataStore dataStore, int[] fieldIds, double epsilon)
            : this(dataStore, fieldIds, new int[] { }, epsilon)
        {
        }

        public BireductGamma(DataStore dataStore, double epsilon)
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
            
            EquivalenceClassMap localPartition = new EquivalenceClassMap(this.DataStore);
            localPartition.Calc(newAttributeSet, this.DataStore);

            foreach (int objectIdx in this.ObjectSet)
            {
                AttributeValueVector dataVector = this.DataStore.GetDataVector(objectIdx, newAttributeSet);
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
            
            EquivalenceClassMap localPartition = new EquivalenceClassMap(this.DataStore);
            localPartition.Calc(this.Attributes, this.DataStore);
            
            AttributeValueVector dataVector = this.DataStore.GetDataVector(objectIndex, this.Attributes);
            EquivalenceClass reductStatistics = localPartition.GetEquivalenceClass(dataVector);

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
        /// <returns>A new instance of a GammaBireduct, using a deep copy.</returns>
        public override object Clone()
        {
            return new BireductGamma(this);
        }
        #endregion
    }
}