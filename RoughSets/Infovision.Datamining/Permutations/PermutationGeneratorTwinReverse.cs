using System.Collections.Generic;
using System.Linq;
using Infovision.Data;

namespace Infovision.MachineLearning.Permutations
{
    public class PermutationGeneratorTwinReverse : PermutationGeneratorTwin
    {
        #region Constructors

        public PermutationGeneratorTwinReverse(int[] elements)
            : base(elements)
        {
        }

        public PermutationGeneratorTwinReverse(DataStore dataStore)
            : base(dataStore.GetStandardFields())
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