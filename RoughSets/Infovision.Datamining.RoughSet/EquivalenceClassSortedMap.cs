using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Infovision.Data;

namespace Infovision.Datamining.Roughset
{
    public class EquivalenceClassSortedMap : EquivalenceClassCollection
    {        
        #region Constructors

        public EquivalenceClassSortedMap()
            : base()
        {   
        }        

        #endregion

        #region Methods

        public override void Calc(FieldSet attributeSet, DataStore dataStore)
        {
            this.InitPartitions();
            int[] orderByTmp = attributeSet.ToArray();
            int[] orderBy = orderByTmp.Union(new int[1] { dataStore.DataStoreInfo.DecisionFieldId }).ToArray();

            DataStoreOrderByComparer comparer = new DataStoreOrderByComparer(dataStore, orderBy);
            int[] sortedObjIdx = dataStore.Sort(comparer);
            decimal weight = Decimal.Divide(Decimal.One, dataStore.NumberOfRecords);

            int i, j;
            for (i = 0; i < sortedObjIdx.Length; i++)
            {
                var dataVector = dataStore.GetFieldValues(i, orderByTmp);
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

        public override void Calc(FieldSet attributeSet, DataStore dataStore, decimal[] objectWeights)
        {                        
            this.InitPartitions();            
            int[] orderByTmp = attributeSet.ToArray();
            int[] orderBy = new int[orderByTmp.Length + 1];
            Array.Copy(orderByTmp, orderBy, orderByTmp.Length);
            orderBy[orderByTmp.Length] = dataStore.DataStoreInfo.DecisionFieldId;

            DataStoreOrderByComparer comparer = new DataStoreOrderByComparer(dataStore, orderBy);
            int[] sortedObjIdx = dataStore.Sort(comparer);

            comparer = new DataStoreOrderByComparer(dataStore, orderByTmp);

            int i = 0, j = 0, sum = 0;                        
            while(i < sortedObjIdx.Length)
            {
                var dataVector = dataStore.GetFieldValues(sortedObjIdx[i], orderByTmp);
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
                sum += eq.NumberOfObjects;
                i = j;
            }

            if (sum != sortedObjIdx.Length)
                throw new InvalidProgramException("Sum of eqivalence classes does not equal to all objects.");
        }

        public override void Calc(FieldSet attributeSet, DataStore dataStore, ObjectSet objectSet, decimal[] objectWeights)
        {
            this.InitPartitions();
            int[] orderByTmp = attributeSet.ToArray();
            int[] orderBy = new int[orderByTmp.Length + 1];
            Array.Copy(orderByTmp, orderBy, orderByTmp.Length);
            orderBy[orderByTmp.Length] = dataStore.DataStoreInfo.DecisionFieldId;

            DataStoreOrderByComparer comparer = new DataStoreOrderByComparer(dataStore, orderBy);
            
            //TODO code smell: Do I sort data or objectSet?
            int[] sortedObjIdx = dataStore.Sort(objectSet, comparer);

            int i, j;
            for (i = 0; i < sortedObjIdx.Length; i++)
            {
                var dataVector = dataStore.GetFieldValues(i, orderByTmp);
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
