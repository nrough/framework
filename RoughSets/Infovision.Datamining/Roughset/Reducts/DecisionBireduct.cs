using Raccoon.Core;
using Raccoon.Data;
using Raccoon.MachineLearning.Permutations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Raccoon.MachineLearning.Roughset.Reducts
{
    public class DecisionBireduct : ModelBase
    {
        public DataStore DecisionTable { get; set; }
        public int NumberOfReducts { get; set; }
        public int[][] Permutations{ get; set; }

        private IList<IReduct> reducts;
        private object syncRoot = new object();

        public DecisionBireduct()
        {
            NumberOfReducts = 100;
            Permutations = null;
            reducts = new List<IReduct>(NumberOfReducts);
        }

        public IList<IReduct> Learn(int[] attributes)
        {
            if (attributes == null) throw new ArgumentNullException("attributes");
            if (DecisionTable == null) throw new InvalidOperationException("DecisionTable is not set");

            if (NumberOfReducts == 0) return new List<IReduct>();

            if (Permutations == null)
            {
                var permutations = new PermutationAttributeObjectGenerator(
                    Enumerable.Range(0, DecisionTable.NumberOfRecords).ToArray(), attributes)
                    .Generate(NumberOfReducts);

                Permutations = new int[NumberOfReducts][];
                for(int i = 0; i < permutations.Count; i++)
                    Permutations[i] = permutations[i].ToArray();
            }

            if (reducts == null || NumberOfReducts != Permutations.Length)
                reducts = new List<IReduct>(Permutations.Length);

            ParallelOptions options = new ParallelOptions()
            {
                MaxDegreeOfParallelism = RaccoonConfiguration.MaxDegreeOfParallelism
            };

            Parallel.For(0, Permutations.Length, options, i =>
            {
                var reduct = ComputeReduct(attributes, Permutations[i]);

                lock (syncRoot)
                {
                    if (!IsSuperSet(reduct.Attributes))
                        reducts.Add(reduct);
                }
            });

            return reducts;
        }

        /// <summary>
        /// Checks if passed-in reduct is a superset of reducts already existing in the store
        /// </summary>
        /// <param name="reduct"></param>
        /// <returns></returns>
        private bool IsSuperSet(HashSet<int> attributeSubset)
        {
            foreach (IReduct localReduct in reducts.ToList())
                if (attributeSubset.IsSupersetOf(localReduct.Attributes))
                    return true;
            return false;
        }

        private IReduct ComputeReduct(int[] attributes, int[] permutation)
        {
            int[] attr = attributes.ToArray();
            int[] obj = new int[DecisionTable.NumberOfRecords];
            obj.SetAll(-1);

            for (int i = 0; i < permutation.Length; i++)
            {
                if (permutation[i] < 0)
                {
                    attr[-permutation[i]] = -1;
                    if (!EquivalenceClassCollection.CheckRegionPositive(attr, DecisionTable, obj))
                    {

                    }
                }
                else
                {
                    obj[permutation[i]] = permutation[i];

                }
            }

            return new Bireduct(DecisionTable, attr, obj);
        }

        
    }
}                                                                              
