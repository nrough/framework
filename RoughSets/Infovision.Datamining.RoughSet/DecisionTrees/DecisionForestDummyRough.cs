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
            int length = RandomSingleton.Random.Next(1, attributes.Length);
            var permutationCollection = new PermutationCollection(this.NumberOfTreeProbes, attributes, length);

            List<IReduct> reducts = new List<IReduct>(this.NumberOfTreeProbes);

            foreach (var perm in permutationCollection)
            {
                IReduct r = new ReductWeights(data, perm.ToArray(), 0, data.Weights);
                decimal q = InformationMeasureWeights.Instance.Calc(r);

                var localPermutationCollection = new PermutationCollection(perm);

                Args parms = new Args(6);
                parms.SetParameter<DataStore>(ReductGeneratorParamHelper.TrainData, data);
                parms.SetParameter<string>(ReductGeneratorParamHelper.FactoryKey, this.ReductGeneratorFactory);
                parms.SetParameter<decimal>(ReductGeneratorParamHelper.Epsilon, this.Epsilon);
                parms.SetParameter<PermutationCollection>(ReductGeneratorParamHelper.PermutationCollection, localPermutationCollection);
                parms.SetParameter<bool>(ReductGeneratorParamHelper.UseExceptionRules, false);
                parms.SetParameter<decimal>(ReductGeneratorParamHelper.DataSetQuality, q);

                IReductGenerator generator = ReductFactory.GetReductGenerator(parms);
                generator.Run();

                IReductStoreCollection reductStoreCollection = generator.GetReductStoreCollection();
                IReduct reduct = reductStoreCollection.First().First();

                reducts.Add(reduct);
            }

            reducts.Sort(new ReductRuleNumberComparer());
            IReduct bestReduct = reducts.First();

            T tree = this.InitDecisionTree();   
            double error = 1.0 - tree.Learn(data, bestReduct.Attributes.ToArray()).Accuracy;

            return new Tuple<T, double>(tree, error);
        }
    }
}
