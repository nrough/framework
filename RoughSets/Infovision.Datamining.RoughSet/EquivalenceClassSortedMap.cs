using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Infovision.Data;

namespace Infovision.Datamining.Roughset
{
    public class EquivalenceClassSortedMap
    {        
        #region Members                

        private Dictionary<DataVector, EquivalenceClass> partitions;
        private Dictionary<long, int> decisionCount;        

        #endregion
        
        #region Constructors

        private EquivalenceClassSortedMap()
        {
            this.partitions = new Dictionary<DataVector, EquivalenceClass>();
            this.decisionCount = new Dictionary<long, int>();            
        }

        public EquivalenceClassSortedMap(DataStoreInfo dataStoreInfo)
        {            
        }        

        #endregion

        #region Methods

        public void Calc(FieldSet attributeSet, DataStore dataStore)
        {
            int[] orderBy = attributeSet.ToArray();
            this.partitions = new Dictionary<DataVector, EquivalenceClass>();
            DataStoreOrderByComparer comparer = new DataStoreOrderByComparer(dataStore, orderBy);
            int[] sortedObjIdx = dataStore.OrderBy(orderBy, comparer);
            double weight = 1.0 / dataStore.NumberOfRecords;

            int i, j;
            for (i = 0; i < sortedObjIdx.Length; i++)
            {
                EquivalenceClass eq = new EquivalenceClass();
                j = i;
                while (comparer.Compare(i, j) == 0)
                {
                    long dec = dataStore.GetDecisionValue(j);
                    eq.AddObject(j, dec, weight);
                    j++;
                }

                this.partitions.Add(dataStore.GetDataVector(i, orderBy), eq);

                i = j + 1;
            }
        }

        public void Calc(FieldSet attributeSet, DataStore dataStore, double[] objectWeights)
        {
            int[] orderBy = attributeSet.ToArray();
            this.partitions = new Dictionary<DataVector, EquivalenceClass>();
            DataStoreOrderByComparer comparer = new DataStoreOrderByComparer(dataStore, orderBy);
            int[] sortedObjIdx = dataStore.OrderBy(orderBy, comparer);

            int i, j;
            for (i = 0; i < sortedObjIdx.Length; i++)
            {
                EquivalenceClass eq = new EquivalenceClass();
                j = i;
                while (comparer.Compare(i, j) == 0)
                {
                    long dec = dataStore.GetDecisionValue(j);
                    eq.AddObject(j, dec, objectWeights[j]);
                    j++;
                }

                this.partitions.Add(dataStore.GetDataVector(i, orderBy), eq);

                i = j + 1;
            }
        }

        public void Calc(FieldSet attributeSet, DataStore dataStore, ObjectSet objectSet, double[] objectWeights)
        {
            int[] orderBy = attributeSet.ToArray();
            this.partitions = new Dictionary<DataVector, EquivalenceClass>();
            DataStoreOrderByComparer comparer = new DataStoreOrderByComparer(dataStore, orderBy);
            int[] sortedObjIdx = dataStore.OrderBy(orderBy, objectSet, comparer);

            int i, j;
            for (i = 0; i < sortedObjIdx.Length; i++)
            {
                EquivalenceClass eq = new EquivalenceClass();
                j = i;
                while (comparer.Compare(i, j) == 0)
                {
                    long dec = dataStore.GetDecisionValue(j);
                    eq.AddObject(j, dec, objectWeights[j]);
                    j++;
                }

                this.partitions.Add(dataStore.GetDataVector(i, orderBy), eq);

                i = j + 1;
            }
        }        
        

        #endregion


    }    
}
