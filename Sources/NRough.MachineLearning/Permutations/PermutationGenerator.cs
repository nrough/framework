﻿using System;
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