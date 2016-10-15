using Infovision.Data;
using Infovision.Utils;
using System;
using System.Linq;

namespace Infovision.Datamining.Roughset.DecisionTrees
{
    public class DecisionForestDummy<T> : DecisionForestBase<T>
        where T : IDecisionTree, new()
    {
        //protected override Tuple<T, double> LearnDecisionTree(DataStore data, int[] attributes, int iteration)
        protected override T LearnDecisionTree(DataStore data, int[] attributes, int iteration)
        {            
            var permutationCollection = new PermutationCollection(
                this.NumberOfTreeProbes, attributes, RandomSingleton.Random.Next(1, attributes.Length));

            //Tuple<T, double> bestTree = null;
            T bestTree = default(T);
            int minNumberOfLeaves = int.MaxValue;
            foreach (var perm in permutationCollection)
            {
                T tree = this.InitDecisionTree();
                double error = tree.Learn(data, perm.ToArray()).Error;
                int numOfLeaves = DecisionTreeHelper.CountLeaves(tree.Root);

                if (numOfLeaves < minNumberOfLeaves)
                {
                    minNumberOfLeaves = numOfLeaves;
                    //bestTree = new Tuple<T, double>(tree, error);
                    bestTree = tree;

                    if (minNumberOfLeaves == 0)
                        break;
                }
            }

            return bestTree;
        }
    }
}
