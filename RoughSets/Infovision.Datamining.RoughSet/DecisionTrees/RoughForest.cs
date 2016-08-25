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
    /// Generates <i>NumberOfPermutationsPerTree</i> Reducts with <i>Epsilon</i> 
    /// approximation level. A reduct that is minimal in terms of length and then 
    /// in terms of least number of decision rules is selected for a single decision tree.
    /// A decision tree is constructed based on attributes belonging to selected decision reduct.
    /// </summary>
    /// <typeparam name="T">
    /// Represents a class that implements <c>IDecisionTree</c> 
    /// interface and is resposble for decision tree construction
    /// </typeparam>
    public class RoughForest<T> : RandomForest<T>
        where T : IDecisionTree, new()
    {
        private Dictionary<int, int> attributeCount;

        public decimal Epsilon { get; set; }
        public int NumberOfPermutationsPerTree { get; set; }
        public string ReductGeneratorFactory { get; set; }
        public virtual PermutationCollection PermutationCollection { get; set; }

        public RoughForest()
            : base()
        {
            this.attributeCount = new Dictionary<int, int>();
            this.Epsilon = Decimal.MinValue;
            this.NumberOfPermutationsPerTree = 20;
            this.ReductGeneratorFactory = ReductFactoryKeyHelper.ApproximateReductMajorityWeights;
        }

        protected virtual IReduct CalculateReduct(DataStore data)
        {
            PermutationCollection permutations = null;
            if (this.PermutationCollection != null)
                permutations = this.PermutationCollection;
            else
                permutations = new PermutationGenerator(data).Generate(this.NumberOfPermutationsPerTree);

            decimal localEpsilon = Decimal.MinValue;
            if (this.Epsilon >= Decimal.Zero)
                localEpsilon = this.Epsilon;
            else
                localEpsilon = (decimal)((double)RandomSingleton.Random.Next(0, 20) / 100.0);

            Args parms = new Args(5);
            parms.SetParameter(ReductGeneratorParamHelper.TrainData, data);
            parms.SetParameter(ReductGeneratorParamHelper.FactoryKey, this.ReductGeneratorFactory);
            parms.SetParameter(ReductGeneratorParamHelper.Epsilon, localEpsilon);
            parms.SetParameter(ReductGeneratorParamHelper.PermutationCollection, permutations);
            parms.SetParameter(ReductGeneratorParamHelper.UseExceptionRules, false);

            IReductGenerator generator = ReductFactory.GetReductGenerator(parms);
            generator.Run();

            IReductStoreCollection reductStoreCollection = generator.GetReductStoreCollection();

            int bestScore = Int32.MaxValue;
            IReduct bestReduct = null;

            List<IReduct> reducts = new List<IReduct>(this.NumberOfPermutationsPerTree);
            foreach (var store in reductStoreCollection)
                foreach (var reduct in store)
                {
                    reducts.Add(reduct);
                }

            //reducts.Sort(new ReductLengthComparer());
            reducts.Sort(new ReductRuleNumberComparer());

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
            return bestReduct;
        }

        public override double Learn(DataStore data, int[] attributes)
        {
            DataSampler sampler = (this.DataSampler != null) ? this.DataSampler : new DataSampler(data);
            if (this.BagSizePercent != -1)
                sampler.BagSizePercent = this.BagSizePercent;

            for (int iter = 0; iter < this.Size; iter++)
            {
                DataStore baggedData = sampler.GetData(iter);

                IReduct reduct = this.CalculateReduct(baggedData);
                foreach (int attr in reduct.Attributes)
                {
                    if (!this.attributeCount.ContainsKey(attr))
                        this.attributeCount[attr] = 1;
                    else
                        this.attributeCount[attr] += 1;
                }

                T tree = new T();
                tree.NumberOfAttributesToCheckForSplit = -1;
                double error = tree.Learn(baggedData, reduct.Attributes.ToArray());
                this.AddTree(tree, error);
            }

            ClassificationResult trainResult = this.Classify(data, data.Weights);
            return 1 - trainResult.Accuracy;
        }
    }
}
