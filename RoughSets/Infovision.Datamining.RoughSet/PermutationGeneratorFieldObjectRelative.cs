using System.Collections.Generic;
using Infovision.Data;
using Infovision.Utils;

namespace Infovision.Datamining.Roughset
{
    public class PermutationGeneratorFieldObjectRelative : PermutationGeneratorFieldObject
    {
        private int[] elementWeights;
        private int sumWeights = 0;
        private DataStore dataStore;

        #region Constructors

        public PermutationGeneratorFieldObjectRelative(DataStore dataStore)
            : base(dataStore)
        {
            this.dataStore = dataStore;            
        }

        public PermutationGeneratorFieldObjectRelative(DataStore dataStore, decimal fieldSelectionRatio)
            : base(dataStore, fieldSelectionRatio)
        {
            this.dataStore = dataStore;
        }

        #endregion

        #region Methods

        protected virtual void InitWeights()
        {
            this.elementWeights = new int[dataStore.NumberOfRecords];
            this.sumWeights = 0;
            foreach (int objectIdx in dataStore.GetObjectIndexes())
            {
                this.elementWeights[objectIdx] = dataStore.DataStoreInfo.NumberOfObjectsWithDecision(dataStore.GetDecisionValue(objectIdx));
                this.sumWeights += this.elementWeights[objectIdx];
            }
        }

        protected override Permutation CreatePermutation()
        {
            this.InitWeights();

            List<int> fieldList = new List<int>(this.GetFieldsArray());
            List<int> objectList = new List<int>(this.GetObjectArray());
            int[] localElements = new int[this.elements.Length];

            for (int j = 0; j < this.elements.Length; j++)
            {
                int element = 0;

                if (fieldList.Count > 0 && objectList.Count > 0)
                {
                    if (RandomSingleton.Random.NextDouble() <= (double)this.FieldSelectionRatio)
        
                    {
                        element = this.GetAndRemoveListElement<int>(fieldList);
                    }
                    else
                    {
                        element = this.GetNextObjectIndex<int>(objectList);
                    }
                }
                else if (fieldList.Count > 0)
                {
                    element = this.GetAndRemoveListElement<int>(fieldList);
                }
                else
                {
                    element = this.GetNextObjectIndex<int>(objectList);
                }

                localElements[j] = element;
            }

            Permutation permutation = new Permutation(localElements);
            return permutation;
        }

        private T GetNextObjectIndex<T>(List<T> list)
        {
            int number = RandomSingleton.Random.Next(this.sumWeights) + 1;
            int sum = 0;
            int i = -1;
            int j = -1;
            
            while (sum < number && i < this.elementWeights.Length - 1)
            {
                i++;
                sum += this.elementWeights[i];
                if (this.elementWeights[i] > 0)
                    j++;
            }

            this.sumWeights -= this.elementWeights[i];
            this.elementWeights[i] = 0;

            T element = list[j];
            list.RemoveAt(j);

            return element;
        }

        #endregion
    }
}
