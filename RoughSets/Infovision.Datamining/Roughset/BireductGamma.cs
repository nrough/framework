using System;
using System.Collections.Generic;
using Infovision.Data;
using Infovision.Core;
using System.Linq;

namespace Infovision.MachineLearning.Roughset
{
    [Serializable]
    public class BireductGamma : Bireduct
    {
        #region Constructors

        public BireductGamma(DataStore dataStore, IEnumerable<int> fieldIds, int[] objectIndexes, double epsilon)
            : base(dataStore, fieldIds, objectIndexes, epsilon)
        {
        }

        public BireductGamma(DataStore dataStore, IEnumerable<int> fieldIds, double epsilon)
            : this(dataStore, fieldIds, new int[] { }, epsilon)
        {
        }

        public BireductGamma(DataStore dataStore, double epsilon)
            : this(dataStore, dataStore.DataStoreInfo.GetFieldIds(FieldTypes.Standard), new int[] { }, epsilon)
        {
        }

        public BireductGamma(BireductGamma gammaBireduct)
            : this(gammaBireduct.DataStore, gammaBireduct.Attributes.ToArray(), gammaBireduct.SupportedObjects.ToArray(),
                   gammaBireduct.Epsilon)
        {
        }

        #endregion Constructors

        #region Methods

        protected override bool CheckRemoveAttribute(int attributeId)
        {
            if (base.CheckRemoveAttribute(attributeId) == false)
                return false;

            HashSet<int> newAttributeSet = new HashSet<int>(this.Attributes);
            newAttributeSet.Remove(attributeId);

            //TODO Performance killer !!!!!!
            EquivalenceClassCollection localPartition = new EquivalenceClassCollection(this.DataStore);
            localPartition.Calc(newAttributeSet, this.DataStore);

            foreach (int objectIdx in this.SupportedObjects)
            {
                var dataVector = this.DataStore.GetFieldValues(objectIdx, newAttributeSet);
                EquivalenceClass reductStat = localPartition.Find(dataVector);

                if (reductStat.NumberOfDecisions > 1)
                {
                    return false;
                }
            }

            return true;
        }

        protected override bool CheckAddObject(int objectIndex)
        {
            if (this.SupportedObjects.Contains(objectIndex))
                return false;
            var dataVector = this.DataStore.GetFieldValues(objectIndex, this.Attributes);

            //TODO Performance killer !!!!!!
            EquivalenceClassCollection localPartition = new EquivalenceClassCollection(this.DataStore);
            localPartition.Calc(this.Attributes, this.DataStore);

            EquivalenceClass eqClass = localPartition.Find(dataVector);

            if (eqClass.NumberOfDecisions > 1)
            {
                return false;
            }

            return true;
        }

        #endregion Methods

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

            return base.Equals((Bireduct)obj);
        }

        #endregion System.Object Methods

        #region ICloneable Members

        /// <summary>
        /// Clones the GammaBireduct, performing a deep copy.
        /// </summary>
        /// <returns>A new newInstance of a GammaBireduct, using a deep copy.</returns>
        public override object Clone()
        {
            return new BireductGamma(this);
        }

        #endregion ICloneable Members
    }
}