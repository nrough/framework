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