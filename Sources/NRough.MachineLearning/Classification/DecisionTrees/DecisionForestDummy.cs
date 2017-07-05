// 
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
