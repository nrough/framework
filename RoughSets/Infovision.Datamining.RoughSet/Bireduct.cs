﻿using System;
using System.Collections.Generic;
using System.Text;
using Infovision.Data;
using Infovision.Utils;

namespace Infovision.Datamining.Roughset
{
    [Serializable]
    public class Bireduct : Reduct
    {
        #region Members

        private ObjectSet objectSet;

        #endregion Members

        #region Constructors

        public Bireduct(DataStore dataStore, IEnumerable<int> fieldIds, IEnumerable<int> objectIndexes, decimal epsilon, decimal[] weights)
            : base(dataStore, fieldIds, epsilon, weights)
        {
            this.objectSet = new ObjectSet(dataStore, objectIndexes);
        }

        public Bireduct(DataStore dataStore, IEnumerable<int> fieldIds, IEnumerable<int> objectIndexes, decimal epsilon)
            : base(dataStore, fieldIds, epsilon)
        {
            this.objectSet = new ObjectSet(dataStore, objectIndexes);
        }

        public Bireduct(DataStore dataStore, IEnumerable<int> fieldIds, decimal epsilon)
            : this(dataStore, fieldIds, new int[] { }, epsilon)
        {
        }

        public Bireduct(DataStore dataStore, decimal epsilon)
            : this(dataStore, dataStore.DataStoreInfo.GetFieldIds(FieldTypes.Standard), new int[] { }, epsilon)
        {
        }

        public Bireduct(Bireduct bireduct)
            : base(bireduct as Reduct)
        {
            this.objectSet = new ObjectSet(bireduct.DataStore, bireduct.ObjectSet);
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
                            this.eqClassMap = EquivalenceClassCollection.Create(this, this.DataStore, this.Weights, this.ObjectSet);
                        }
                    }
                }

                return this.eqClassMap;
            }
        }

        public override ObjectSet ObjectSet
        {
            get { return this.objectSet; }
        }

        public override IObjectSetInfo ObjectSetInfo
        {
            get { return this.ObjectSet; }
        }

        #endregion Properties

        #region Methods

        protected override bool CheckRemoveAttribute(int attributeId)
        {
            if (base.CheckRemoveAttribute(attributeId) == false)
            {
                return false;
            }

            FieldSet newAttributeSet = (FieldSet)(this.Attributes - attributeId);
            return EquivalenceClassCollection.CheckRegionPositive(newAttributeSet, this.DataStore, this.ObjectSet);
        }

        protected virtual bool CheckAddObject(int objectIndex)
        {
            if (this.ObjectSet.ContainsElement(objectIndex))
                return false;

            var dataVector = this.DataStore.GetFieldValues(objectIndex, this.Attributes);
            EquivalenceClass eqClass = this.EquivalenceClasses.GetEquivalenceClass(dataVector);

            if (eqClass == null)
                return true;

            if (eqClass.NumberOfDecisions <= 1)
            {
                long decisionValue = this.DataStore.GetDecisionValue(objectIndex);

                if (eqClass.NumberOfDecisions == 0 || eqClass.DecisionSet.ContainsElement(decisionValue))
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
                EquivalenceClass eq = this.EquivalenceClasses.GetEquivalenceClass(dataVector);

                if (eq == null)
                {
                    eq = new EquivalenceClass(dataVector, this.DataStore);
                    this.EquivalenceClasses.Partitions.Add(dataVector, eq);
                }

                eq.AddObject(
                    objectIdx,
                    this.DataStore.GetDecisionValue(objectIdx),
                    Decimal.Divide(Decimal.One, this.DataStore.NumberOfRecords));

                objectSet.AddElement(objectIdx);

                return true;
            }

            return false;
        }

        public override bool ContainsObject(int objectIndex)
        {
            return this.objectSet.ContainsElement(objectIndex);
        }

        protected virtual bool CheckRemoveObject(int objectIdx)
        {
            return true;
        }

        public virtual bool RemoveObject(int objectIdx)
        {
            if (this.CheckRemoveObject(objectIdx))
            {
                objectSet.RemoveElement(objectIdx);
                var dataVector = this.DataStore.GetFieldValues(objectIdx, this.Attributes);
                this.EquivalenceClasses.GetEquivalenceClass(dataVector).RemoveObject(objectIdx, this.DataStore);
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
                DataFieldInfo dataField = this.DataStore.DataStoreInfo.GetFieldInfo(attr[i]);
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

            long[] objectIds = Array.ConvertAll<int, long>(this.ObjectSet.ToArray(),
                                                               delegate(int i)
                                                               {
                                                                   return this.DataStore.ObjectIndex2ObjectId(i);
                                                               }
                                                              );
            stringBuilder.Append(InfovisionHelper.IntArray2Ranges(objectIds));
            stringBuilder.Append('}');
            stringBuilder.Append(')');

            return stringBuilder.ToString();
        }

        public override int GetHashCode()
        {
            return (int)this.Attributes.GetHashCode() ^ this.ObjectSet.GetHashCode();
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

            if (!this.ObjectSet.Equals(bireduct.ObjectSet))
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