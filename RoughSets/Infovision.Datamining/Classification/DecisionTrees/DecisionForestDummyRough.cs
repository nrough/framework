using NRough.Data;
using NRough.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NRough.MachineLearning.Roughset;
using NRough.MachineLearning.Permutations;

namespace NRough.MachineLearning.Classification.DecisionTrees
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class DecisionForestDummyRough<T> : DecisionForestBase<T>
        where T : IDecisionTree, new()
    {
        public string ReductGeneratorFactory { get; set; }

        //protected override Tuple<T, double> LearnDecisionTree(DataStore data, int[] attributes, int iteration)
        protected override T LearnDecisionTree(DataStore data, int[] attributes, int iteration)
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
            T tree = this.InitDecisionTree();
            double error = tree.Learn(data, reducts[0].Attributes.ToArray()).Error;

            //return new Tuple<T, double>(tree, error);
            return tree;
        }
    }
}
