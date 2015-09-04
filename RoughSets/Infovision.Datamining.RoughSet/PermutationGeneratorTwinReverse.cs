using System;
using System.Collections.Generic;
using Infovision.Data;

namespace Infovision.Datamining.Roughset
{
    public class PermutationGeneratorTwinReverse : PermutationGeneratorTwin
    {
        #region Constructors

        public PermutationGeneratorTwinReverse(Int32[] elements)
            : base(elements)
        {
        }

        public PermutationGeneratorTwinReverse(DataStore dataStore)
            : base(dataStore.DataStoreInfo.GetFieldIds(FieldTypes.Standard))
        {
        }

        #endregion

        #region Methods

        public override PermutationList Generate(int numberOfPermutations)
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

            return new PermutationList(permutationList);
        }

        #endregion
    }
}
