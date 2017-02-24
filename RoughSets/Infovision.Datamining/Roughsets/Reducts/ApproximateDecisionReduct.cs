using NRough.Core;
using NRough.Data;
using NRough.MachineLearning.Permutations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NRough.MachineLearning.Roughsets.Reducts
{
    [Serializable]
    public class ApproximateDecisionReduct : ModelBase
    {
        public FMeasure FMeasure { get; set; }
        public double Epsilon { get; set; }
        public int NumberOfReducts { get; set; }
        public int[][] Permutations { get; set; }
        public DataStore DecisionTable { get; set; }

        private IList<IReduct> reducts;
        private double datasetQuality;
        private object syncRoot = new object();

        public ApproximateDecisionReduct()
        {
            FMeasure = FMeasures.Majority;
            Epsilon = 0.0;
            NumberOfReducts = 100;
            Permutations = null;
            reducts = new List<IReduct>(NumberOfReducts);
        }

        public virtual IList<IReduct> Learn(DataStore data, int[] attributes)
        {
            if (data == null) throw new ArgumentNullException("data");
            if (attributes == null) throw new ArgumentNullException("attributes");

            if (NumberOfReducts == 0) return new List<IReduct>();
            if (Permutations == null)
            {
                Permutations = new int[NumberOfReducts][];
                for (int i = 0; i < Permutations.Length; i++)
                {
                    Permutations[i] = attributes.ToArray();
                    Permutations[i].Shuffle();
                }
            }

            datasetQuality = FMeasure(
                EquivalenceClassCollection.Create(
                    attributes, data, data.Weights));

            if(reducts == null || NumberOfReducts != Permutations.Length)
                reducts = new List<IReduct>(Permutations.Length);

            ParallelOptions options = new ParallelOptions()
            {
                MaxDegreeOfParallelism = ConfigManager.MaxDegreeOfParallelism
            };

            Parallel.For(0, Permutations.Length, options, i =>
            {
                var reduct = ComputeReduct(Permutations[i]);

                lock (syncRoot)
                {
                    if (!IsSuperSet(reduct.Attributes))
                        reducts.Add(reduct);
                }
            });

            return reducts;
        }

        private IReduct ComputeReduct(int[] permutation)
        {
            EquivalenceClassCollection eqClasses;
            int[] attr1, attr2;

            Reach(permutation, out attr1, out eqClasses);
            Reduce(attr1, out attr2, ref eqClasses);

            return new ReductWeights(DecisionTable, attr2, Epsilon, DecisionTable.Weights, eqClasses);
        }

        private void Reach(int[] permutation, out int[] reduct, out EquivalenceClassCollection eqClasses)
        {
            HashSet<int> attrSet = new HashSet<int>();
            eqClasses = null;
            for (int i = 0; i < permutation.Length; i++)
            {
                attrSet.Add(permutation[i]);

                if (IsSuperSet(attrSet))
                    break;

                eqClasses = EquivalenceClassCollection.Create(attrSet.ToArray(), DecisionTable, DecisionTable.Weights);
                if (ToleranceDoubleComparer.Instance.Compare(
                        FMeasure(eqClasses), (1.0 - this.Epsilon) * datasetQuality) >= 0)
                    break;
            }

            reduct = attrSet.ToArray();
            if(eqClasses == null)
                eqClasses = EquivalenceClassCollection.Create(reduct, DecisionTable, DecisionTable.Weights);
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

        private void Reduce(int[] subset, out int[] reduct, ref EquivalenceClassCollection eqClasses)
        {
            HashSet<int> attrSet = new HashSet<int>(subset);
            EquivalenceClassCollection prevEqClasses = null;

            for (int i = subset.Length - 1; i >= 0; i--)
            {
                prevEqClasses = eqClasses;
                attrSet.Remove(subset[i]);

                if (IsSuperSet(attrSet))
                    continue;

                eqClasses = EquivalenceClassCollection.Create(attrSet.ToArray(), DecisionTable, DecisionTable.Weights);
                if (ToleranceDoubleComparer.Instance.Compare(
                        FMeasure(eqClasses), (1.0 - this.Epsilon) * datasetQuality) >= 0)
                    continue;

                eqClasses = prevEqClasses;
                attrSet.Add(subset[i]);
            }

            reduct = attrSet.ToArray();
        }

    }
}
