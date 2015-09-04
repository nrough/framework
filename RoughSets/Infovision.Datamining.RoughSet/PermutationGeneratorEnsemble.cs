using Infovision.Data;
using Infovision.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infovision.Datamining.Roughset
{
    public class PermutationGeneratorEnsemble : PermutationGenerator
    {
        #region Members

        private int[][] existingAttributeSets;
        private Dictionary<int, int> attributeCount;
        private bool generateWithProbability;
        private int countSum;        

        #endregion

        #region Constructors

        public PermutationGeneratorEnsemble(int[] elements, int[][] attributes)
            : base(elements)
        {
            this.generateWithProbability = false;
            this.Setup(elements, attributes);
        }
        
        public PermutationGeneratorEnsemble(DataStore dataStore, int[][] attributes)
            : this(dataStore.DataStoreInfo.GetFieldIds(FieldTypes.Standard), attributes)
        {
        }

        public PermutationGeneratorEnsemble(DataStore dataStore, IReductStoreCollection models)
            : base(dataStore)
        {            
            this.generateWithProbability = false;

            if (models != null && models.Count > 0)
            {
                int[][] selectedAttributes;
                int count = 0;
                foreach (IReductStore rs in models)
                    if(rs.IsActive)
                        count += rs.Count;
                selectedAttributes = new int[count][];

                int k = 0;
                foreach (IReductStore rs in models)
                    if(rs.IsActive)
                        foreach (IReduct r in rs)
                            selectedAttributes[k++] = r.Attributes.ToArray();

                this.Setup(dataStore.DataStoreInfo.GetFieldIds(FieldTypes.Standard), selectedAttributes);
            }
        }

        #endregion

        private void Setup(int[] elements, int[][] attributes)
        {
            
            if (attributes != null)
            {
                this.existingAttributeSets = new int[attributes.Length][];
                for (int i = 0; i < attributes.Length; i++)
                {
                    this.existingAttributeSets[i] = new int[attributes[i].Length];
                    Array.Copy(attributes[i], this.existingAttributeSets[i], attributes[i].Length);
                }
            }

            this.attributeCount = new Dictionary<int, int>(elements.Length);
            this.countSum = 0;
            foreach (int attributeId in elements)
                this.attributeCount.Add(attributeId, 1);
            this.countSum += elements.Length;

            if (attributes != null && attributes.Length > 0)
            {                
                for (int k = 0; k < this.existingAttributeSets.Length; k++)
                    for (int i = 0; i < existingAttributeSets[k].Length; i++)
                    {
                        this.attributeCount[existingAttributeSets[k][i]]++;
                        countSum++;
                    }
                
                this.generateWithProbability = true;
            }
        }

        protected override Permutation CreatePermutation()
        {
            if(this.generateWithProbability == false)
                return base.CreatePermutation();

            int[] pds = new int[this.countSum];
            int pos = 0;
            foreach (var kvp in this.attributeCount)                
                for (int i = 0; i < kvp.Value; i++)
                    pds[pos++] = kvp.Key;

            int[] result = new int[this.elements.Length];
            int idx = this.elements.Length - 1;
            int size = this.countSum;
            foreach (var kvp in this.attributeCount)
            {
                pos = RandomSingleton.Random.Next(size);
                result[idx--] = pds[pos];

                int newSize = size - this.attributeCount[pds[pos]];
                int[] newPds = new int[newSize];
                int newPdsPos = 0;
                for (int i = 0; i < size; i++)
                {
                    if (pds[i] != pds[pos])
                        newPds[newPdsPos++] = pds[i];
                }

                size = newSize;
                pds = newPds;
            }

            Permutation permutation = new Permutation(result);
            return permutation;
        }
    }
}
