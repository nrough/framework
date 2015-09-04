using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Infovision.Data;

namespace Infovision.Datamining.Roughset
{
    public class EquivalenceClassSortedMap : EquivalenceClassMap
    {        
        #region Constructors

        public EquivalenceClassSortedMap(DataStoreInfo dataStoreInfo)
            : base(dataStoreInfo)
        {   
        }        

        #endregion

        #region Methods

        public override void Calc(FieldSet attributeSet, DataStore dataStore)
        {
            int[] orderBy = attributeSet.ToArray();
            DataStoreOrderByComparer comparer = new DataStoreOrderByComparer(dataStore, orderBy);
            int[] sortedObjIdx = dataStore.OrderBy(orderBy, comparer);
            double weight = 1.0 / dataStore.NumberOfRecords;

            int i, j;
            for (i = 0; i < sortedObjIdx.Length; i++)
            {
                AttributeValueVector dataVector = dataStore.GetDataVector(i, orderBy);
                EquivalenceClass eq = new EquivalenceClass(dataVector);
                j = i;
                while (comparer.Compare(i, j) == 0)
                {
                    long dec = dataStore.GetDecisionValue(j);
                    eq.AddObject(j, dec, weight);
                    j++;
                }

                this.Partitions.Add(dataVector, eq);

                i = j + 1;
            }
        }

        public override void Calc(FieldSet attributeSet, DataStore dataStore, double[] objectWeights)
        {
            int[] orderBy = attributeSet.ToArray();
            DataStoreOrderByComparer comparer = new DataStoreOrderByComparer(dataStore, orderBy);
            int[] sortedObjIdx = dataStore.OrderBy(orderBy, comparer);

            int i, j;
            for (i = 0; i < sortedObjIdx.Length; i++)
            {
                AttributeValueVector dataVector = dataStore.GetDataVector(i, orderBy);
                EquivalenceClass eq = new EquivalenceClass(dataVector);
                j = i;
                while (comparer.Compare(i, j) == 0)
                {
                    long dec = dataStore.GetDecisionValue(j);
                    eq.AddObject(j, dec, objectWeights[j]);
                    j++;
                }

                this.Partitions.Add(dataVector, eq);

                i = j + 1;
            }
        }

        public override void Calc(FieldSet attributeSet, DataStore dataStore, ObjectSet objectSet, double[] objectWeights)
        {
            int[] orderBy = attributeSet.ToArray();
            DataStoreOrderByComparer comparer = new DataStoreOrderByComparer(dataStore, orderBy);
            int[] sortedObjIdx = dataStore.OrderBy(orderBy, objectSet, comparer);

            int i, j;
            for (i = 0; i < sortedObjIdx.Length; i++)
            {
                AttributeValueVector dataVector = dataStore.GetDataVector(i, orderBy);
                EquivalenceClass eq = new EquivalenceClass(dataVector);
                j = i;
                while (comparer.Compare(i, j) == 0)
                {
                    long dec = dataStore.GetDecisionValue(j);
                    eq.AddObject(j, dec, objectWeights[j]);
                    j++;
                }

                this.Partitions.Add(dataVector, eq);

                i = j + 1;
            }
        }        
        

        #endregion


    }    
}
