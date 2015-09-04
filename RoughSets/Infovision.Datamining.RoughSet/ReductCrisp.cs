using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Infovision.Data;
using Infovision.Utils;

namespace Infovision.Datamining.Roughset
{
    public class ReductCrisp : Reduct
    {
        #region Construct
        
        public ReductCrisp(DataStore dataStore)
            : base(dataStore, 100)
        {
        }

        public ReductCrisp(DataStore dataStore, int approximationDegree)
            : base(dataStore, 100)
        {
        }

        public ReductCrisp(DataStore dataStore, int[] fieldIds, int approximationDegree)
            : base(dataStore, fieldIds, 100)
        {
        }

        public ReductCrisp(ReductCrisp reduct)
            : base(reduct as Reduct)
        {
        }

        #endregion

        #region Methods

        protected override void InitEquivalenceMap()
        {
            this.EquivalenceClassMap = new EquivalenceClassSortedMap(this.DataStore.DataStoreInfo);
        }

        protected override bool CheckRemoveAttribute(int attributeId)
        {
            bool ret = base.CheckRemoveAttribute(attributeId);
            FieldSet newFieldSet = (FieldSet) (this.Attributes - attributeId);
            Dictionary<AttributeValueVector, PascalSet> generalDecisionMap = new Dictionary<AttributeValueVector, PascalSet>();
                        
            if (ret)
            {
                DataFieldInfo decisionFieldInfo = this.DataStore.DataStoreInfo.GetDecisionFieldInfo();
                
                foreach (EquivalenceClass eq in this.EquivalenceClassMap)
                {
                    int[] attributes = eq.DataVector.GetAttributes();
                    long[] values = eq.DataVector.GetValues();

                    int[] newAttributes = new int[attributes.Length - 1];
                    long[] newValues = new long[values.Length - 1];

                    int k = 0;
                    for (int i = 0; i < attributes.Length; i++)
                    {
                        if (attributes[i] != attributeId)
                        {
                            newAttributes[k] = attributes[i];
                            newValues[k] = values[i];
                            k++;
                        }
                    }
                    
                    AttributeValueVector record = new AttributeValueVector(newAttributes, newValues, false);

                    PascalSet existingGeneralDecisions = null;
                    generalDecisionMap.TryGetValue(record, out existingGeneralDecisions);
                    if (existingGeneralDecisions != null)
                    {
                        existingGeneralDecisions = existingGeneralDecisions.Intersection(eq.DecisionSet);
                    }
                    else
                    {
                        existingGeneralDecisions = eq.DecisionSet;
                    }
                    generalDecisionMap[record] = existingGeneralDecisions;

                    if (existingGeneralDecisions.Count == 0)
                    {
                        ret = false;
                        break;
                    }
                }
            }

            return ret;
        }

        #endregion
    }
}
