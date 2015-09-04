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

        public EquivalenceClassSortedMap(DataStore data)
            : base(data)
        {   
        }        

        #endregion

        #region Methods

        public override void Calc(FieldSet attributeSet, DataStore dataStore)
        {
            this.InitPartitions();
            int[] orderByTmp = attributeSet.ToArray();
            int[] orderBy = new int[orderByTmp.Length + 1];
            Array.Copy(orderByTmp, orderBy, orderByTmp.Length);
            orderBy[orderByTmp.Length] = dataStore.DataStoreInfo.DecisionFieldId;

            DataStoreOrderByComparer comparer = new DataStoreOrderByComparer(dataStore, orderBy);
            int[] sortedObjIdx = dataStore.OrderBy(orderBy, comparer);
            double weight = 1.0 / dataStore.NumberOfRecords;

            int i, j;
            for (i = 0; i < sortedObjIdx.Length; i++)
            {
                AttributeValueVector dataVector = dataStore.GetDataVector(i, orderByTmp);
                EquivalenceClass eq = new EquivalenceClass(dataVector, dataStore);
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
            this.InitPartitions();            
            int[] orderByTmp = attributeSet.ToArray();
            int[] orderBy = new int[orderByTmp.Length + 1];
            Array.Copy(orderByTmp, orderBy, orderByTmp.Length);
            orderBy[orderByTmp.Length] = dataStore.DataStoreInfo.DecisionFieldId;

            DataStoreOrderByComparer comparer = new DataStoreOrderByComparer(dataStore, orderBy);            
            int[] sortedObjIdx = dataStore.OrderBy(orderBy, comparer);

            comparer = new DataStoreOrderByComparer(dataStore, orderByTmp);

            int i = 0, j = 0;                        
            while(i < sortedObjIdx.Length)
            {
                AttributeValueVector dataVector = dataStore.GetDataVector(sortedObjIdx[i], orderByTmp);
                EquivalenceClass eq = new EquivalenceClass(dataVector, dataStore);
                long dec = dataStore.GetDecisionValue(sortedObjIdx[i]);
                eq.AddObject(sortedObjIdx[i], dec, objectWeights[sortedObjIdx[i]]);

                j = i + 1;
                while (j < sortedObjIdx.Length && comparer.Compare(sortedObjIdx[i], sortedObjIdx[j]) == 0)
                {
                    dec = dataStore.GetDecisionValue(sortedObjIdx[j]);
                    eq.AddObject(sortedObjIdx[j], dec, objectWeights[sortedObjIdx[j]]);
                    j++;
                }
                
                this.Partitions.Add(dataVector, eq);
                i = j;
            }
        }

        public override void Calc(FieldSet attributeSet, DataStore dataStore, ObjectSet objectSet, double[] objectWeights)
        {
            this.InitPartitions();
            int[] orderByTmp = attributeSet.ToArray();
            int[] orderBy = new int[orderByTmp.Length + 1];
            Array.Copy(orderByTmp, orderBy, orderByTmp.Length);
            orderBy[orderByTmp.Length] = dataStore.DataStoreInfo.DecisionFieldId;

            DataStoreOrderByComparer comparer = new DataStoreOrderByComparer(dataStore, orderBy);
            int[] sortedObjIdx = dataStore.OrderBy(orderBy, objectSet, comparer);

            int i, j;
            for (i = 0; i < sortedObjIdx.Length; i++)
            {
                AttributeValueVector dataVector = dataStore.GetDataVector(i, orderByTmp);
                EquivalenceClass eq = new EquivalenceClass(dataVector, dataStore);
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
