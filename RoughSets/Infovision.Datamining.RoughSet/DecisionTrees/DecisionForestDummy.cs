using Infovision.Data;
using Infovision.Utils;
using System;
using System.Linq;

namespace Infovision.Datamining.Roughset.DecisionTrees
{
    public class DecisionForestDummy<T> : DecisionForestBase<T>
        where T : IDecisionTree, new()
    {       
        protected override Tuple<T, double> LearnDecisionTree(DataStore data, int[] attributes, int iteration)
        {
            int length = RandomSingleton.Random.Next(1, attributes.Length);
            var permutationCollection = new PermutationCollection(this.NumberOfTreeProbes, attributes, length);

            Tuple<T, double> bestTree = null;
            int minNumberOfLeaves = int.MaxValue;
            foreach (var perm in permutationCollection)
            {
                T tree = this.InitDecisionTree();
                double error = 1.0 - tree.Learn(data, perm.ToArray()).Accuracy;
                int numOfLeaves = DecisionTreeHelper.CountLeaves(tree.Root);

                if (numOfLeaves < minNumberOfLeaves)
                {
                    minNumberOfLeaves = numOfLeaves;                    
                    bestTree = new Tuple<T, double>(tree, error);
                    if (minNumberOfLeaves == 0)
                        break;
                }
            }

            return bestTree;
        }
    }
}
