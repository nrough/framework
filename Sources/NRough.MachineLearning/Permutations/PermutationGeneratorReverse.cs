//
// NRough Framework
// http://www.nrough.net
//
// Copyright © Sebastian Widz, 2017
// authors at nrough.net

//This file is part of NRough.

//Foobar is free software; you can redistribute it and/or modify
//it under the terms of the GNU General Public License as published by
//the Free Software Foundation; either version 3 of the License, or
//(at your option) any later version.
//
//Foobar is distributed in the hope that it will be useful,
//but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
//GNU General Public License for more details.
//
//You should have received a copy of the GNU General Public License
//along with Foobar; if not, write to the Free Software
//Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA  02110-1301  USA
//
using System.Collections.Generic;
using System.Linq;
using NRough.Data;

namespace NRough.MachineLearning.Permutations
{
    public class PermutationGeneratorReverse : PermutationGenerator
    {
        #region Constructors

        public PermutationGeneratorReverse(int[] elements)
            : base(elements)
        {
        }

        public PermutationGeneratorReverse(DataStore dataStore)
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