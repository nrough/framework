﻿using System;
using System.Collections.Generic;
using Infovision.Data;
using Infovision.Utils;

namespace Infovision.Datamining.Roughset
{
    public interface IPermutationGenerator
    {
        PermutationCollection Generate(int numberOfPermutations);
    }
    
    public class PermutationGenerator : IPermutationGenerator
    {
        #region Members

        protected int[] elements;

        #endregion

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
            : this(dataStore.DataStoreInfo.GetFieldIds(FieldTypes.Standard))
        {
        }

        #endregion

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

            for (int i = elements.Length - 1; i > 0; i--)
            {
                int k = RandomSingleton.Random.Next() % (i + 1);
                int element = localElements[k];
                localElements[k] = localElements[i];
                localElements[i] = element;
            }

            Permutation permutation = new Permutation(localElements);
            return permutation;
        }

        #endregion
    }
}
