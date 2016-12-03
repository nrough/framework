using Infovision.Data;
using Infovision.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Infovision.MachineLearning.Classification;
using Infovision.MachineLearning.Weighting;
using Infovision.MachineLearning.Permutations;
using Infovision.MachineLearning.Roughset;

namespace Infovision.MachineLearning.Classification.DecisionTrees
{
    public class DecisionTreeReduct : DecisionTreeRough
    {
        public string ReductFactoryKey { get; set; }
        public WeightGenerator WeightGenerator { get; set; }
        public PermutationCollection PermutationCollection { get; set; }
        public IComparer<IReduct> ReductComparer { get; set; }
        public int ReductIterations { get; set; }
        public double ReductEpsilon { get; set; }
        public IReduct Reduct { get; private set; }

        public DecisionTreeReduct()
            : base()
        {
            this.ReductFactoryKey = ReductFactoryKeyHelper.ApproximateReductMajorityWeights;
            this.ReductIterations = 1;
            this.Gamma = 0.0;
            this.ReductEpsilon = 0.0;
            this.ReductComparer = null;
        }

        public DecisionTreeReduct(string modelName)
            : base(modelName)
        {
            this.ReductFactoryKey = ReductFactoryKeyHelper.ApproximateReductMajorityWeights;
            this.ReductIterations = 1;
            this.Gamma = 0.0;
            this.ReductEpsilon = 0.0;
            this.ReductComparer = null;
        }        

        public override void SetClassificationResultParameters(ClassificationResult result)
        {
            base.SetClassificationResultParameters(result);

            result.Epsilon = this.ReductEpsilon;
        }

        public override ClassificationResult Learn(DataStore data, int[] attributes)
        {
            if (this.WeightGenerator == null)
                this.WeightGenerator = new WeightGeneratorMajority(data);

            if (this.PermutationCollection == null)
                this.PermutationCollection = new PermutationCollection(this.ReductIterations, attributes);

            if (this.ReductComparer == null)
                this.ReductComparer = new ReductAccuracyComparer(data);

            Args parms = new Args(8);
            parms.SetParameter(ReductGeneratorParamHelper.TrainData, data);
            parms.SetParameter(ReductGeneratorParamHelper.FactoryKey, this.ReductFactoryKey);
            parms.SetParameter(ReductGeneratorParamHelper.WeightGenerator, this.WeightGenerator);
            parms.SetParameter(ReductGeneratorParamHelper.Epsilon, this.ReductEpsilon);
            parms.SetParameter(ReductGeneratorParamHelper.PermutationCollection, this.PermutationCollection);
            parms.SetParameter(ReductGeneratorParamHelper.UseExceptionRules, false);
            parms.SetParameter(ReductGeneratorParamHelper.NumberOfReducts, this.ReductIterations);
            parms.SetParameter(ReductGeneratorParamHelper.NumberOfReductsToTest, this.ReductIterations);

            IReductGenerator generator = ReductFactory.GetReductGenerator(parms);
            generator.Run();

            IReductStoreCollection reductStores = generator.GetReductStoreCollection();
            if (reductStores.ReductPerStore == true)
                throw new InvalidOperationException("reductStores.ReductPerStore == true is not supported");

            IReductStore reducts = reductStores.FirstOrDefault();
            IReduct reduct = reducts.FilterReducts(1, this.ReductComparer).FirstOrDefault();
            this.Reduct = reduct;

            return base.Learn(data, reduct.Attributes.ToArray());
        }
    }
}