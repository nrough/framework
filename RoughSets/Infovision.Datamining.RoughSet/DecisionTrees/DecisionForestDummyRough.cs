using Infovision.Data;
using Infovision.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infovision.Datamining.Roughset.DecisionTrees
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class DecisionForestDummyRough<T> : DecisionForestBase<T>
        where T : IDecisionTree, new()
    {
        public string ReductGeneratorFactory { get; set; }

        protected override Tuple<T, double> LearnDecisionTree(DataStore data, int[] attributes, int iteration)
        {
            var permutationCollection = new PermutationCollection(
                this.NumberOfTreeProbes, attributes, RandomSingleton.Random.Next(1, attributes.Length));
                       
            ParallelOptions options = new ParallelOptions
            {
                MaxDegreeOfParallelism = InfovisionConfiguration.MaxDegreeOfParallelism
            };

            IReduct[] reducts = new IReduct[this.NumberOfTreeProbes];
            Parallel.For(0, this.NumberOfTreeProbes, options, i =>
            {                
                var localPermutationCollection = new PermutationCollection(permutationCollection[i]);

                Args parms = new Args(6);
                parms.SetParameter<DataStore>(ReductGeneratorParamHelper.TrainData, data);
                parms.SetParameter<string>(ReductGeneratorParamHelper.FactoryKey, this.ReductGeneratorFactory);
                parms.SetParameter<decimal>(ReductGeneratorParamHelper.Epsilon, this.Epsilon);
                parms.SetParameter<PermutationCollection>(ReductGeneratorParamHelper.PermutationCollection, localPermutationCollection);
                parms.SetParameter<bool>(ReductGeneratorParamHelper.UseExceptionRules, false);

                if (this.ReductGeneratorFactory != ReductFactoryKeyHelper.GeneralizedMajorityDecision
                    && this.ReductGeneratorFactory != ReductFactoryKeyHelper.GeneralizedMajorityDecisionApproximate)
                {
                    parms.SetParameter<EquivalenceClassCollection>(ReductGeneratorParamHelper.InitialEquivalenceClassCollection,
                        EquivalenceClassCollection.Create(permutationCollection[i].ToArray(), data, data.Weights));
                }

                IReductGenerator generator = ReductFactory.GetReductGenerator(parms);
                generator.Run();
                reducts[i] = generator.GetReductStoreCollection().First().First();
            }
            );
            
            Array.Sort(reducts, new ReductRuleNumberComparer());            
            T tree = this.InitDecisionTree();
            double error = 1.0 - tree.Learn(data, reducts[0].Attributes.ToArray()).Accuracy;

            return new Tuple<T, double>(tree, error);
        }
    }
}
