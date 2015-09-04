using System;
using System.Text;
using Infovision.Data;
using Infovision.Utils;

namespace Infovision.Datamining.Roughset
{
    [Serializable]
    public class Bireduct : Reduct
    {
        #region Globals

        private ObjectSet objectSet;

        #endregion

        #region Constructors
        
        public Bireduct(DataStore dataStore, int[] fieldIds, int[] objectIndexes, double approxDegree)
            : base(dataStore, fieldIds, approxDegree)
        {
            objectSet = new ObjectSet(dataStore, objectIndexes);            
        }

        public Bireduct(DataStore dataStore, int[] fieldIds, double approxDegree)
            : this(dataStore, fieldIds, new int[] { }, approxDegree)
        {
        }

        public Bireduct(DataStore dataStore, double approxDegree)
            : this(dataStore, dataStore.DataStoreInfo.GetFieldIds(FieldTypes.Standard), new int[] { }, approxDegree)
        {
        }

        public Bireduct(Bireduct bireduct)
            : this(bireduct.DataStore, bireduct.Attributes.ToArray(), bireduct.ObjectSet.ToArray(), bireduct.ApproximationDegree)
        {
        }

        #endregion

        #region Properties

        public override ObjectSet ObjectSet
        {
            get { return this.objectSet; }
        }

        public override IObjectSetInfo ObjectSetInfo
        {
            get { return this.ObjectSet; }
        }

        #endregion

        #region Methods

        public override void BuildEquivalenceMap()
        {
            if (this.objectSet != null)
            {
                this.InitEquivalenceMap();
                this.EquivalenceClassMap.Calc(this.Attributes, this.DataStore, this.objectSet, this.Weights);
            }
        }

        protected override bool CheckRemoveAttribute(int attributeId)
        {
            if (base.CheckRemoveAttribute(attributeId) == false)
            {
                return false;
            }

            FieldSet newAttributeSet = (FieldSet) (this.Attributes - attributeId);
            return EquivalenceClassMap.CheckRegionPositive(newAttributeSet, this.DataStore, this.ObjectSet);
        }

        protected virtual bool CheckAddObject(int objectIndex)
        {
            DataVector dataVector = this.DataStore.GetDataVector(objectIndex, this.Attributes);
            EquivalenceClass reductStatistics = this.EquivalenceClassMap.GetEquivalenceClass(dataVector);

            if (reductStatistics.NumberOfDecisions <= 1)
            {
                long decisionValue = this.DataStore.GetDecisionValue(objectIndex);

                if (reductStatistics.NumberOfDecisions == 0
                    || reductStatistics.MajorDecision == decisionValue)
                {                                        
                    return true;
                }
            }
            
            return false;
        }

        public virtual bool AddObject(int objectIdx)
        {
            if (this.CheckAddObject(objectIdx))
            {                
                DataVector dataVector = this.DataStore.GetDataVector(objectIdx, this.Attributes);

                // important fist add to eqClass map, than to objectSet, otherwise 
                // calling this.EquivalenceClassMap will invoke calculating eqClassMap based on existing objectSet
                // than, what AddObject is called we will get duplicates of object id inside eqClass
                
                this.EquivalenceClassMap.GetEquivalenceClass(dataVector).AddObject(objectIdx, this.DataStore.GetDecisionValue(objectIdx), 1.0 / this.DataStore.NumberOfRecords);
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
                DataVector dataVector = this.DataStore.GetDataVector(objectIdx, this.Attributes);
                this.EquivalenceClassMap.GetEquivalenceClass(dataVector).RemoveObject(objectIdx);
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
                string attrName = !String.IsNullOrEmpty(dataField.NameAlias)
                                ? dataField.NameAlias
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

            Int64[] objectIds = Array.ConvertAll<int, Int64>(this.ObjectSet.ToArray(), 
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
            return (int) this.Attributes.GetHashCode() ^ this.ObjectSet.GetHashCode();
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
        #endregion

        #region ICloneable Members
        /// <summary>
        /// Clones the BiReduct, performing a deep copy.
        /// </summary>
        /// <returns>A new instance of a BiReduct, using a deep copy.</returns>
        public override object Clone()
        {
            return new Bireduct(this);
        }
        #endregion

        #endregion
    }
}
