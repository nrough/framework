//
// NRough Framework
// http://www.nrough.net
//
// Copyright © Sebastian Widz, 2017
// authors at nrough.net

//This file is part of NRough.

//Foobar is free software; you can redistribute it and/or modify
//it under the terms of the GNU General Public License as published by
//the Free Software Foundation; either version 3 of the License, or
//(at your option) any later version.
//
//Foobar is distributed in the hope that it will be useful,
//but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
//GNU General Public License for more details.
//
//You should have received a copy of the GNU General Public License
//along with Foobar; if not, write to the Free Software
//Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA  02110-1301  USA
//
using NRough.Data;
using NRough.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NRough.MachineLearning.Roughsets;
using NRough.MachineLearning.Permutations;
using NRough.Core.Random;
using NRough.MachineLearning.Roughsets.Reducts.Comparers;

namespace NRough.MachineLearning.Classification.DecisionTrees
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TTree"></typeparam>
    public class DecisionForestDummyRough<TTree> : DecisionForestBase<TTree>
        where TTree : IDecisionTree, new()
    {
        public string ReductGeneratorFactory { get; set; }

        //protected override Tuple<T, double> LearnDecisionTree(DataStore data, int[] attributes, int iteration)
        protected override TTree LearnDecisionTree(DataStore data, int[] attributes, int iteration)
        {
            var permutationCollection = new PermutationCollection(
                this.NumberOfTreeProbes, attributes, RandomSingleton.Random.Next(1, attributes.Length));
                       
            ParallelOptions options = new ParallelOptions
            {
                MaxDegreeOfParallelism = ConfigManager.MaxDegreeOfParallelism
            };

            IReduct[] reducts = new IReduct[this.NumberOfTreeProbes];
            Parallel.For(0, this.NumberOfTreeProbes, options, i =>
            {                
                var localPermutationCollection = new PermutationCollection(permutationCollection[i]);

                Args parms = new Args(6);
                parms.SetParameter<DataStore>(ReductFactoryOptions.DecisionTable, data);
                parms.SetParameter<string>(ReductFactoryOptions.ReductType, this.ReductGeneratorFactory);
                parms.SetParameter<double>(ReductFactoryOptions.Epsilon, this.Gamma);
                parms.SetParameter<PermutationCollection>(ReductFactoryOptions.PermutationCollection, localPermutationCollection);
                parms.SetParameter<bool>(ReductFactoryOptions.UseExceptionRules, false);

                if (this.ReductGeneratorFactory != ReductTypes.GeneralizedMajorityDecision
                    && this.ReductGeneratorFactory != ReductTypes.GeneralizedMajorityDecisionApproximate)
                {
                    parms.SetParameter<EquivalenceClassCollection>(ReductFactoryOptions.InitialEquivalenceClassCollection,
                        EquivalenceClassCollection.Create(permutationCollection[i].ToArray(), data, data.Weights));
                }

                IReductGenerator generator = ReductFactory.GetReductGenerator(parms);
                generator.Run();
                reducts[i] = generator.GetReductStoreCollection().First().First();
            }
            );
            
            Array.Sort(reducts, new ReductRuleNumberComparer());            
            TTree tree = this.InitDecisionTree();
            double error = tree.Learn(data, reducts[0].Attributes.ToArray()).Error;

            //return new Tuple<T, double>(tree, error);
            return tree;
        }
    }
}
