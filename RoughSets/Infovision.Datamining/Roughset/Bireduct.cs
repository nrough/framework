using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Raccoon.Data;
using Raccoon.Core;

namespace Raccoon.MachineLearning.Roughset
{
    [Serializable]
    public class Bireduct : Reduct
    {
        #region Members

        private HashSet<int> objectSet;

        #endregion Members

        #region Constructors

        public Bireduct(DataStore dataStore, IEnumerable<int> fieldIds, IEnumerable<int> objectIndexes, double epsilon, double[] weights)
            : base(dataStore, fieldIds, epsilon, weights)
        {
            this.objectSet = new HashSet<int>(objectIndexes);
        }

        public Bireduct(DataStore dataStore, IEnumerable<int> fieldIds, IEnumerable<int> objectIndexes, double epsilon)
            : base(dataStore, fieldIds, epsilon)
        {
            this.objectSet = new HashSet<int>(objectIndexes);
        }

        public Bireduct(DataStore dataStore, IEnumerable<int> fieldIds, double epsilon)
            : this(dataStore, fieldIds, new int[] { }, epsilon)
        {
        }

        public Bireduct(DataStore dataStore, double epsilon)
            : this(dataStore, dataStore.DataStoreInfo.GetFieldIds(FieldGroup.Standard), new int[] { }, epsilon)
        {
        }

        public Bireduct(Bireduct bireduct)
            : base(bireduct as Reduct)
        {
            this.objectSet = new HashSet<int>(bireduct.SupportedObjects);
        }

        #endregion Constructors

        #region Properties

        public override EquivalenceClassCollection EquivalenceClasses
        {
            get
            {
                if (this.eqClassMap == null)
                {
                    lock (mutex)
                    {
                        if (this.eqClassMap == null)
                        {
                            this.eqClassMap = EquivalenceClassCollection.Create(
                                this.Attributes.ToArray(), this.DataStore, this.Weights, this.SupportedObjects.ToArray());
                        }
                    }
                }

                return this.eqClassMap;
            }
        }

        public override HashSet<int> SupportedObjects
        {
            get { return this.objectSet; }
        }

        #endregion Properties

        #region Methods

        public override bool TryRemoveAttribute(int attributeId)
        {
            if (this.CheckRemoveAttribute(attributeId))
            {
                return base.TryRemoveAttribute(attributeId);
            }

            return false;
        }
        
        protected override bool CheckRemoveAttribute(int attributeId)
        {
            if (base.CheckRemoveAttribute(attributeId) == false)
            {
                return false;
            }

            HashSet<int> newAttributeSet = new HashSet<int>(this.Attributes);
            newAttributeSet.Remove(attributeId);

            return EquivalenceClassCollection.CheckRegionPositive(newAttributeSet, this.DataStore, this.SupportedObjects);
        }

        protected virtual bool CheckAddObject(int objectIndex)
        {
            if (this.SupportedObjects.Contains(objectIndex))
                return false;

            var dataVector = this.DataStore.GetFieldValues(objectIndex, this.Attributes);
            EquivalenceClass eqClass = this.EquivalenceClasses.Find(dataVector);

            if (eqClass == null)
                return true;

            if (eqClass.NumberOfDecisions <= 1)
            {
                long decisionValue = this.DataStore.GetDecisionValue(objectIndex);

                if (eqClass.NumberOfDecisions == 0 || eqClass.DecisionSet.Contains(decisionValue))
                {
                    return true;
                }
            }

            return false;
        }

        public virtual bool TryAddObject(int objectIdx)
        {
            if (this.CheckAddObject(objectIdx))
            {
                var dataVector = this.DataStore.GetFieldValues(objectIdx, this.Attributes);
                EquivalenceClass eq = this.EquivalenceClasses.Find(dataVector);

                if (eq == null)
                    eq = new EquivalenceClass(dataVector, this.DataStore);

                eq.AddObject(
                    objectIdx,
                    this.DataStore.GetDecisionValue(objectIdx),
                    1.0 / this.DataStore.NumberOfRecords);

                objectSet.Add(objectIdx);

                return true;
            }

            return false;
        }

        public override bool ContainsObject(int objectIndex)
        {
            return this.objectSet.Contains(objectIndex);
        }

        protected virtual bool CheckRemoveObject(int objectIdx)
        {
            return true;
        }

        public virtual bool RemoveObject(int objectIdx)
        {
            if (this.CheckRemoveObject(objectIdx))
            {
                objectSet.Remove(objectIdx);
                var dataVector = this.DataStore.GetFieldValues(objectIdx, this.Attributes);
                this.EquivalenceClasses.Find(dataVector).RemoveObject(objectIdx, this.DataStore);
                return true;
            }

            return false;
        }

        #region System.Object Methods

        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();

            stringBuilder.Append('(');
            stringBuilder.Append('{');
            int[] attr = this.Attributes.ToArray();
            for (int i = 0; i < attr.Length; i++)
            {
                AttributeInfo dataField = this.DataStore.DataStoreInfo.GetFieldInfo(attr[i]);
                string attrName = !String.IsNullOrEmpty(dataField.Alias)
                                ? dataField.Alias
                                : !String.IsNullOrEmpty(dataField.Name)
                                ? dataField.Name
                                : (-attr[i]).ToString();
                if (i == 0)
                {
                    stringBuilder.Append(attrName);
                }
                else
                {
                    stringBuilder.Append(' ').Append(attrName);
                }
            }
            stringBuilder.Append("},{");

            long[] objectIds = Array.ConvertAll<int, long>(this.SupportedObjects.ToArray(),
                                                               delegate(int i)
                                                               {
                                                                   return this.DataStore.ObjectIndex2ObjectId(i);
                                                               }
                                                              );
            stringBuilder.Append(RaccoonHelper.IntArray2Ranges(objectIds));
            stringBuilder.Append('}');
            stringBuilder.Append(')');

            return stringBuilder.ToString();
        }

        public override int GetHashCode()
        {
            return (int)this.Attributes.GetHashCode() ^ this.SupportedObjects.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            Bireduct bireduct = obj as Bireduct;
            if (bireduct == null)
                return false;

            if (!this.Attributes.Equals(bireduct.Attributes))
                return false;

            if (!this.SupportedObjects.Equals(bireduct.SupportedObjects))
                return false;

            return true;
        }

        #endregion System.Object Methods

        #region ICloneable Members

        /// <summary>
        /// Clones the BiReduct, performing a deep copy.
        /// </summary>
        /// <returns>A new newInstance of a BiReduct, using a deep copy.</returns>
        public override object Clone()
        {
            return new Bireduct(this);
        }

        #endregion ICloneable Members

        #endregion Methods
    }
}