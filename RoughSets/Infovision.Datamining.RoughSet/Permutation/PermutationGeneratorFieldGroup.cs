using System;
using System.Collections;
using Infovision.Data;
using Infovision.Utils;

namespace Infovision.Datamining.Roughset
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
            int[] fieldIds = dataStore.DataStoreInfo.GetFieldIds(FieldTypes.Standard);
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

            int[] randomGroups = new int[this.fieldGroups.Length];
            for(int i = 0; i<this.fieldGroups.Length; i++)
                randomGroups[i] = i;

            randomGroups.Shuffle();

            int[] result = new int[size];

            //take attribute one by one from each group
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

        #endregion


    }
}
