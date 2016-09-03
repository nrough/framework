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
    /// Generates <i>NumberOfPermutationsPerTree</i> Reducts with <i>Epsilon</i><br /> 
    /// approximation level. A reduct that is minimal in terms of length and then<br /> 
    /// in terms of least number of decision rules is selected for a single decision tree.<br />
    /// A decision tree is constructed based on attributes belonging to selected decision reduct.
    /// </summary>
    /// <typeparam name="T">
    /// Represents a class that implements <c>IDecisionTree</c> 
    /// interface and is resposble for decision tree construction
    /// </typeparam>
    public class DecisionForestReduct<T> : DecisionForestBase<T>
        where T : IDecisionTree, new()
    {
        //private Dictionary<int, int> attributeCount;

        public string ReductGeneratorFactory { get; set; }
        public decimal MaxRandomEpsilon { get; set; }

        public DecisionForestReduct()
            : base()
        {            
            this.ReductGeneratorFactory = ReductFactoryKeyHelper.ApproximateReductMajorityWeights;
            this.MaxRandomEpsilon = 0.2m;
        }

        /*
        public override ClassificationResult Learn(DataStore data, int[] attributes)
        {
            //this.attributeCount = new Dictionary<int, int>(attributes.Length);

            return base.Learn(data, attributes);
        }
        */

        protected override Tuple<T, double> LearnDecisionTree(DataStore data, int[] attributes, int iteration)
        {
            PermutationCollection permutations = new PermutationGenerator(attributes).Generate(this.NumberOfTreeProbes);

            decimal localEpsilon = this.Epsilon >= Decimal.Zero 
                                 ? this.Epsilon 
                                 : (decimal)((double)RandomSingleton.Random.Next(0, (int)this.MaxRandomEpsilon * 100) / 100.0);

            IReduct reduct = this.CalculateReduct(data, permutations, localEpsilon);

            /*
            foreach (int attr in reduct.Attributes)
                if (!this.attributeCount.ContainsKey(attr))
                    this.attributeCount[attr] = 1;
                else
                    this.attributeCount[attr] += 1;
            */

            T tree = this.InitDecisionTree();
            double error = 1.0 - tree.Learn(data, reduct.Attributes.ToArray()).Accuracy;
            return new Tuple<T, double>(tree, error);
        }

        protected IReduct CalculateReduct(DataStore data, PermutationCollection permutations, decimal epsilon)
        {
            Args parms = new Args(5);
            parms.SetParameter(ReductGeneratorParamHelper.TrainData, data);
            parms.SetParameter(ReductGeneratorParamHelper.FactoryKey, this.ReductGeneratorFactory);
            parms.SetParameter(ReductGeneratorParamHelper.Epsilon, epsilon);
            parms.SetParameter(ReductGeneratorParamHelper.PermutationCollection, permutations);
            parms.SetParameter(ReductGeneratorParamHelper.UseExceptionRules, false);

            IReductGenerator generator = ReductFactory.GetReductGenerator(parms);
            
            generator.Run();

            IReductStoreCollection reductStoreCollection = generator.GetReductStoreCollection();

            List<IReduct> reducts = new List<IReduct>(this.NumberOfTreeProbes);
            foreach (var store in reductStoreCollection)
                foreach (var reduct in store)
                    reducts.Add(reduct);

            //reducts.Sort(new ReductLengthComparer());
            reducts.Sort(new ReductRuleNumberComparer());
            IReduct bestReduct = reducts.First();
            
            /*
            int bestScore = Int32.MaxValue;
            foreach (var reduct in reducts)
            {
                int count = 0;
                foreach (int attr in reduct.Attributes)
                {
                    if (this.attributeCount.ContainsKey(attr))
                        count += this.attributeCount[attr];
                }

                if (count < bestScore)
                {
                    bestScore = count;
                    bestReduct = reduct;
                }

                if (bestScore == 0)
                    break;
            }
            */

            return bestReduct;
        }
    }
}
