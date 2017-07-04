﻿//
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
using System.Collections.Generic;
using System.Linq;
using NRough.Data;
using NRough.Core;
using NRough.Core.CollectionExtensions;

namespace NRough.MachineLearning.Permutations
{
    public interface IPermutationGenerator
    {
        PermutationCollection Generate(int numberOfPermutations);
    }

    public class PermutationGenerator : IPermutationGenerator
    {
        #region Members

        protected int[] elements;

        #endregion Members

        #region Constructors

        protected PermutationGenerator()
        {
        }

        public PermutationGenerator(int[] elements)
            : this()
        {
            this.elements = new int[elements.Length];
            Buffer.BlockCopy(elements, 0, this.elements, 0, elements.Length * sizeof(int));
        }

        public PermutationGenerator(DataStore dataStore)
            : this(dataStore.GetStandardFields())
        {
        }

        #endregion Constructors

        #region Methods

        public virtual PermutationCollection Generate(int numberOfPermutations)
        {
            List<Permutation> permutationList = new List<Permutation>(numberOfPermutations);
            for (int i = 0; i < numberOfPermutations; i++)
            {
                permutationList.Add(this.CreatePermutation());
            }
            return new PermutationCollection(permutationList);
        }

        protected virtual Permutation CreatePermutation()
        {
            int[] localElements = new int[this.elements.Length];
            Buffer.BlockCopy(this.elements, 0, localElements, 0, this.elements.Length * sizeof(int));
            localElements.Shuffle();
            Permutation permutation = new Permutation(localElements);
            return permutation;
        }

        #endregion Methods
    }
}