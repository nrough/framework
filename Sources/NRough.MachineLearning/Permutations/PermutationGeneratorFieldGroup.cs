﻿// 
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

using System;
using System.Collections;
using System.Linq;
using NRough.Data;
using NRough.Core;
using NRough.Core.Random;
using NRough.Core.CollectionExtensions;

namespace NRough.MachineLearning.Permutations
{
    [CLSCompliant(false)]
    public class PermutatioGeneratorFieldGroup : PermutationGenerator
    {
        private int[][] fieldGroups;

        protected PermutatioGeneratorFieldGroup()
            : base()
        {
        }

        public PermutatioGeneratorFieldGroup(int[][] fieldGroups)
            : this()
        {
            this.fieldGroups = new int[fieldGroups.Length][];
            for (int i = 0; i < fieldGroups.Length; i++)
            {
                this.fieldGroups[i] = new int[fieldGroups[i].Length];
                Buffer.BlockCopy(fieldGroups[i], 0, this.fieldGroups[i], 0, this.fieldGroups[i].Length * sizeof(int));
            }
        }

        public PermutatioGeneratorFieldGroup(DataStore dataStore)
        {
            int[] fieldIds = dataStore.DataStoreInfo.SelectAttributeIds(a => a.IsStandard).ToArray();
            this.fieldGroups = new int[fieldIds.Length][];
            for (int i = 0; i < fieldIds.Length; i++)
            {
                this.fieldGroups[i] = new int[1];
                this.fieldGroups[i][0] = fieldIds[i];
            }
        }

        #region Methods

        protected override Permutation CreatePermutation()
        {
            int size = 0;
            ArrayList[] localElements = new ArrayList[this.fieldGroups.Length];

            for (int i = 0; i < this.fieldGroups.Length; i++)
            {
                localElements[i] = new ArrayList(this.fieldGroups[i]);
                size += this.fieldGroups[i].Length;
            }

            int[] randomGroups = Enumerable.Range(0, this.fieldGroups.Length).ToArray();
            randomGroups.Shuffle();

            int[] result = new int[size];

            //take key one by one from each group
            int pos = 0;
            bool flag = true;
            while (flag)
            {
                flag = false;
                for (int groupId = 0; groupId < randomGroups.Length; groupId++)
                {
                    if (localElements[randomGroups[groupId]].Count > 0)
                    {
                        int k = RandomSingleton.Random.Next() % (localElements[randomGroups[groupId]].Count);
                        int element = (int)localElements[randomGroups[groupId]][k];
                        localElements[randomGroups[groupId]].RemoveAt(k);
                        result[pos++] = element;

                        flag = true;
                    }
                }
            }

            Permutation permutation = new Permutation(result);

            return permutation;
        }

        #endregion Methods
    }
}