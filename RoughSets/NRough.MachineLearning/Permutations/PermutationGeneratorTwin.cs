using System;
using System.Linq;
using NRough.Data;
using NRough.Core;
using NRough.Core.Random;

namespace NRough.MachineLearning.Permutations
{
    public class PermutationGeneratorTwin : PermutationGenerator
    {
        #region Constructors

        public PermutationGeneratorTwin(int[] elements)
            : base(elements)
        {
        }

        public PermutationGeneratorTwin(DataStore dataStore)
            : base(dataStore.GetStandardFields())
        {
        }

        #endregion Constructors

        #region Methods

        protected override Permutation CreatePermutation()
        {
            int[] localElements = new int[this.elements.Length * 2];

            Buffer.BlockCopy(this.elements, 0, localElements, 0, elements.Length * sizeof(int));
            Buffer.BlockCopy(this.elements, 0, localElements, this.elements.Length * sizeof(int), this.elements.Length * sizeof(int));

            for (int i = 0; i < this.elements.Length; i++)
            {
                localElements[i] *= -1;
            }

            for (int i = this.elements.Length * 2 - 1; i > 0; i--)
            {
                int k = RandomSingleton.Random.Next() % (i + 1);
                int element = localElements[k];
                localElements[k] = localElements[i];
                localElements[i] = element;
            }

            Permutation permutation = new Permutation(localElements);
            return permutation;
        }

        #endregion Methods
    }
}