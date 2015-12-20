using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Infovision.Data;
using Infovision.Utils;

namespace Infovision.Datamining.Roughset
{
    public class ReductGeneralizedMajorityDecision : ReductWeights
    {
        private HashSet<int> removedAttributes;
        private bool isEqMapCreated;

        #region Constructors        

        public ReductGeneralizedMajorityDecision(DataStore dataStore, int[] fieldIds, decimal[] weights, decimal epsilon)
            : base(dataStore, fieldIds, weights, epsilon)
        {
            this.Init();
        }

        public ReductGeneralizedMajorityDecision(ReductGeneralizedMajorityDecision reduct)
            : base(reduct as ReductWeights)
        {
            //TODO Casting Error:
            /*
            Test 'Infovision.Datamining.Roughset.UnitTests.ReductGeneralDecisionGeneratorTest.GenerateTest(System.Collections.Generic.Dictionary`2[System.String,System.Object])' failed:
                System.InvalidCastException : Nie można rzutować obiektu typu 'Infovision.Datamining.Roughset.EquivalenceClassCollection' na typ 'Infovision.Datamining.Roughset.EquivalenceClassSortedMap'.
                w Infovision.Datamining.Roughset.ReductGeneralizedMajorityDecision..ctor(ReductGeneralizedMajorityDecision reduct) w f:\Projects\Infovision\Infovision.Datamining.RoughSet\ReductGeneralizedMajorityDecision.cs:wiersz 38
                w Infovision.Datamining.Roughset.ReductGeneralizedMajorityDecision.Clone() w f:\Projects\Infovision\Infovision.Datamining.RoughSet\ReductGeneralizedMajorityDecision.cs:wiersz 105
                w Infovision.Datamining.Roughset.ReductStore..ctor(ReductStore reductStore) w f:\Projects\Infovision\Infovision.Datamining.RoughSet\ReductStore.cs:wiersz 121
                w Infovision.Datamining.Roughset.ReductStore.RemoveDuplicates() w f:\Projects\Infovision\Infovision.Datamining.RoughSet\ReductStore.cs:wiersz 157
                w Infovision.Datamining.Roughset.ReductGeneralizedDecisionGenerator.Generate() w f:\Projects\Infovision\Infovision.Datamining.RoughSet\ReductGeneralizedDecisionGenerator.cs:wiersz 56
                w Infovision.Datamining.Roughset.UnitTests.ReductGeneralDecisionGeneratorTest.GenerateTest(Dictionary`2 args) w f:\Projects\Infovision\Infovision.Datamining.Roughset.UnitTests\ReductGeneralDecisionGeneratorTest.cs:wiersz 87
            */

            //this.EquivalenceClassCollection = (EquivalenceClassSortedMap) reduct.EquivalenceClassCollection.Clone();

            //TODO Temporary fix : This will cause EQ map to be recalculated on next call
            this.EquivalenceClasses = null;
            this.removedAttributes = new HashSet<int>(reduct.removedAttributes);
        }

        #endregion

        #region Methods

        private void Init()
        {
            this.removedAttributes = new HashSet<int>();
            this.isEqMapCreated = false;
        }

        protected override void InitEquivalenceMap()
        {
            //if (isEqMapCreated)
            //    throw new InvalidOperationException("EquicalenceClassMap can only be initialized once.");
            //this.EquivalenceClasses = new EquivalenceClassSortedMap(this.DataStore);
            this.EquivalenceClasses = new EquivalenceClassCollection(this.DataStore);
        }

        public override void BuildEquivalenceMap()
        {
            //base.BuildEquivalenceMap();
            if (isEqMapCreated == false)
            {
                lock (syncRoot)
                {
                    if (isEqMapCreated == false)
                    {
                        this.InitEquivalenceMap();
                        this.EquivalenceClasses.Calc(this.Attributes, this.DataStore, this.Weights);
                        this.isEqMapCreated = true;
                    }
                }
            }
        }

        /// <summary>
        /// Method tries to remove attributes from current reduct. Attributes are removed in order passed in attributeOrder array.
        /// </summary>
        /// <param name="attributeOrder">Attributes to be tried to be removed in given order.</param>
        public virtual void Reduce(int[] attributeOrder, int minimumLength)
        {
            if (minimumLength == this.DataStore.DataStoreInfo.GetNumberOfFields(FieldTypes.Standard))
                return;

            foreach (EquivalenceClass eq in this.EquivalenceClasses)
                eq.RemoveObjectsWithMinorDecisions();

            bool isReduced = false;
            int len = attributeOrder.Length;
            int reduced = 0;
            for (int i = len - 1; (i >= 0) && (len - minimumLength - reduced > 0); i--)
            {
                if (this.TryRemoveAttribute(attributeOrder[i]))
                {
                    reduced++;
                    isReduced = true;
                }
            }

            if (isReduced)
            {
                this.EquivalenceClasses.Calc(this.Attributes, this.DataStore, this.Weights);

                /*
                EquivalenceClassSortedMap newEqMap = new EquivalenceClassSortedMap(this.DataStore);
                foreach (EquivalenceClass eq in this.EquivalenceClassCollection)
                {                    
                    AttributeValueVector newInstance = eq.Instance;
                    foreach (int removedAttribute in this.removedAttributes)
                        newInstance = newInstance.RemoveAttribute(removedAttribute);

                    EquivalenceClass existingEqClass = newEqMap.GetEquivalenceClass(newInstance);

                    if (existingEqClass == null)
                    {
                        existingEqClass = new EquivalenceClass(newInstance, this.DataStore);
                        newEqMap.Partitions.Add(newInstance, existingEqClass);
                    }

                    existingEqClass.Merge(eq);                             
                }

                this.EquivalenceClassCollection = newEqMap;
                */

                this.removedAttributes = new HashSet<int>();
            }
        }


        /// <summary>
        /// Method tries to remove attributes from current reduct
        /// </summary>
        /// <remarks>
        /// this method always reduce attributes in increasing order, consider using Reduce(int[] attributes)
        /// </remarks>
        public virtual void Reduce()
        {
            this.Reduce(this.Attributes.ToArray(), 0);
        }

        public override bool TryRemoveAttribute(int attributeId)
        {
            //base.TryRemoveAttribute(attributeId);
            if (this.CheckRemoveAttribute(attributeId))
            {
                this.Attributes.RemoveElement(attributeId);
                this.removedAttributes.Add(attributeId);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Checks if an attribute can be removed from super-reduct.
        /// </summary>
        /// <param name="attributeId">Attribute Id to remove</param>
        /// <returns></returns>
        protected override bool CheckRemoveAttribute(int attributeId)
        {
            //checks if attribute exists in current reduct
            bool ret = base.CheckRemoveAttribute(attributeId);

            if (ret == false)
                return false;

            //duplicate current attribute set and remove selected attribute
            FieldSet newFieldSet = (FieldSet)(this.Attributes - attributeId);

            //new temporary equivalence class map with decision set intersection  (data vector --> generalized decision)
            Dictionary<long[], PascalSet<long>> generalDecisionMap = new Dictionary<long[], PascalSet<long>>(new Int64ArrayEqualityComparer());
            DataFieldInfo decisionFieldInfo = this.DataStore.DataStoreInfo.DecisionInfo;

            foreach (EquivalenceClass eq in this.EquivalenceClasses)
            {
                //newInstance of a record belonging to equivalence class
                var instance = eq.Instance.RemoveValue(attributeId);
                foreach (int removedAttribute in this.removedAttributes)
                    instance = instance.RemoveValue(removedAttribute);

                //add EQ class to map and calculate intersection of decisions
                PascalSet<long> existingGeneralDecisions = null;

                if (generalDecisionMap.TryGetValue(instance, out existingGeneralDecisions))
                    existingGeneralDecisions = existingGeneralDecisions.Intersection(eq.DecisionSet);
                else
                    existingGeneralDecisions = eq.DecisionSet;
                generalDecisionMap[instance] = existingGeneralDecisions;

                //empty intersection => we cannot remove the attribute
                if (existingGeneralDecisions.GetCardinality() == 0)
                    return false;
            }

            return true;
        }

        #region ICloneable Members
        /// <summary>
        /// Clones the Reduct, performing a deep copy.
        /// </summary>
        /// <returns>A new newInstance of a FieldSet, using a deep copy.</returns>
        public override object Clone()
        {
            return new ReductGeneralizedMajorityDecision(this);
        }
        #endregion

        #region System.Object Methods

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            ReductGeneralizedMajorityDecision reduct = obj as ReductGeneralizedMajorityDecision;
            if (reduct == null)
                return false;

            return base.Equals(reduct);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        #endregion

        #endregion
    }

}
