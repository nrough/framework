using NRough.Data;
using NRough.Core;
using System;
using System.Linq;
using NRough.MachineLearning.Permutations;
using NRough.Core.Random;

namespace NRough.MachineLearning.Classification.DecisionTrees
{
    public class DecisionForestDummy<TTree> : DecisionForestBase<TTree>
        where TTree : IDecisionTree, new()
    {
        //protected override Tuple<T, double> LearnDecisionTree(DataStore data, int[] attributes, int iteration)
        protected override TTree LearnDecisionTree(DataStore data, int[] attributes, int iteration)
        {            
            var permutationCollection = new PermutationCollection(
                this.NumberOfTreeProbes, attributes, RandomSingleton.Random.Next(1, attributes.Length));

            //Tuple<T, double> bestTree = null;
            TTree bestTree = default(TTree);
            int minNumberOfLeaves = int.MaxValue;
            foreach (var perm in permutationCollection)
            {
                TTree tree = this.InitDecisionTree();
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
