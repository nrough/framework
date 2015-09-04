using System;
using Infovision.Data;

namespace Infovision.Datamining.Roughset
{
    [Serializable]
    public class BireductGamma : Bireduct
    {
        #region Constructors

        public BireductGamma(DataStore dataStore, int[] fieldIds, int[] objectIndexes, double approxDegree)
            : base(dataStore, fieldIds, objectIndexes, approxDegree)
        {
        }

        public BireductGamma(DataStore dataStore, int[] fieldIds, double approxDegree)
            : this(dataStore, fieldIds, new int[] { }, approxDegree)
        {
        }

        public BireductGamma(DataStore dataStore, double approxDegree)
            : this(dataStore, dataStore.DataStoreInfo.GetFieldIds(FieldTypes.Standard), new int[] { }, approxDegree)
        {
        }

        public BireductGamma(BireductGamma gammaBireduct)
            : this(gammaBireduct.DataStore, gammaBireduct.Attributes.ToArray(), gammaBireduct.ObjectSet.ToArray(), gammaBireduct.ApproximationDegree)
        {

        }

        #endregion

        #region Methods

        protected override bool CheckRemoveAttribute(int attributeId)
        {

            if (base.CheckRemoveAttribute(attributeId) == false)
                return false;
            
            FieldSet newAttributeSet = (FieldSet) (this.Attributes - attributeId);
            
            EquivalenceClassMap localPartition = new EquivalenceClassMap(this.DataStore.DataStoreInfo);
            localPartition.Calc(newAttributeSet, this.DataStore);

            foreach (int objectIdx in this.ObjectSet)
            {
                DataVector dataVector = this.DataStore.GetDataVector(objectIdx, newAttributeSet);
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
            EquivalenceClassMap localPartition = new EquivalenceClassMap(this.DataStore.DataStoreInfo);
            localPartition.Calc(this.Attributes, this.DataStore);
            
            DataVector dataVector = this.DataStore.GetDataVector(objectIndex, this.Attributes);
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
