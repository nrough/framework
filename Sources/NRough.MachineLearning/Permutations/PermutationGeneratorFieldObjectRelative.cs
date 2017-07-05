// 
// NRough Framework
// http://www.nrough.net
// 
// Copyright (c) Sebastian Widz, 2017
// authors at nrough.net
// 
// This file is part of NRough Framework.
// 
// NRough Framework is free software; you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation; either version 3 of the License, or
// (at your option) any later version.
// 
// NRough Framework is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with NRough Framework; if not, write to the Free Software
// Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA  02110-1301  USA
//

using System.Collections.Generic;
using NRough.Data;
using NRough.Core;
using NRough.Core.Random;

namespace NRough.MachineLearning.Permutations
{
    public class PermutationGeneratorFieldObjectRelative : PermutationAttributeObjectGenerator
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

        public PermutationGeneratorFieldObjectRelative(DataStore dataStore, double fieldSelectionRatio)
            : base(dataStore, fieldSelectionRatio)
        {
            this.dataStore = dataStore;
        }

        #endregion Constructors

        #region Methods

        protected virtual void InitWeights()
        {
            this.elementWeights = new int[dataStore.NumberOfRecords];
            this.sumWeights = 0;
            for (int objectIdx = 0; objectIdx < dataStore.NumberOfRecords; objectIdx++)
            //foreach (int objectIdx in dataStore.GetObjectIndexes())
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

        #endregion Methods
    }
}