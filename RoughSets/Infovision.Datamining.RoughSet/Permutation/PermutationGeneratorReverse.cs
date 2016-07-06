﻿using System.Collections.Generic;
using System.Linq;
using Infovision.Data;

namespace Infovision.Datamining.Roughset
{
    public class PermutationGeneratorReverse : PermutationGenerator
    {
        #region Constructors

        public PermutationGeneratorReverse(int[] elements)
            : base(elements)
        {
        }

        public PermutationGeneratorReverse(DataStore dataStore)
            : base(dataStore.DataStoreInfo.GetFieldIds(FieldTypes.Standard).ToArray())
        {
        }

        #endregion Constructors

        #region Methods

        public override PermutationCollection Generate(int numberOfPermutations)
        {
            List<Permutation> permutationList = new List<Permutation>(numberOfPermutations);
            for (int i = 0; i < (numberOfPermutations / 2); i++)
            {
                Permutation permutation = this.CreatePermutation();
                permutationList.Add(permutation);
                permutationList.Add(permutation.Reverse());
            }

            if (numberOfPermutations % 2 == 1)
            {
                permutationList.Add(this.CreatePermutation());
            }

            return new PermutationCollection(permutationList);
        }

        #endregion Methods
    }
}