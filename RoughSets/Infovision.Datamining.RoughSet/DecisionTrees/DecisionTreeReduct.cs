using Infovision.Data;
using Infovision.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infovision.Datamining.Roughset.DecisionTrees
{
    public class DecisionTreeReduct : DecisionTreeRough
    {
        public string ReductFactoryKey { get; set; }
        public WeightGenerator WeightGenerator { get; set; }
        public PermutationCollection PermutationCollection { get; set; }
        public int Iterations { get; set; }

        public DecisionTreeReduct()
            : base()
        {
            this.ReductFactoryKey = ReductFactoryKeyHelper.ApproximateReductMajorityWeights;
            this.Iterations = 1;
            this.Epsilon = 0.0;
        }

        public override ClassificationResult Learn(DataStore data, int[] attributes)
        {
            if (this.WeightGenerator == null)
                this.WeightGenerator = new WeightGeneratorMajority(data);

            if (this.PermutationCollection == null)
                this.PermutationCollection = new PermutationCollection(this.Iterations, attributes);

            Args parms = new Args();
            parms.SetParameter(ReductGeneratorParamHelper.TrainData, data);
            parms.SetParameter(ReductGeneratorParamHelper.FactoryKey, this.ReductFactoryKey);
            parms.SetParameter(ReductGeneratorParamHelper.WeightGenerator, this.WeightGenerator);
            parms.SetParameter(ReductGeneratorParamHelper.Epsilon, this.Epsilon);
            parms.SetParameter(ReductGeneratorParamHelper.PermutationCollection, this.PermutationCollection);
            parms.SetParameter(ReductGeneratorParamHelper.UseExceptionRules, false);
            parms.SetParameter(ReductGeneratorParamHelper.NumberOfReducts, this.Iterations);
            parms.SetParameter(ReductGeneratorParamHelper.NumberOfReductsToTest, this.Iterations);

            IReductGenerator generator = ReductFactory.GetReductGenerator(parms);
            if (generator is ReductGeneratorMeasure)
                ((ReductGeneratorMeasure)generator).UsePerformanceImprovements = true;
            generator.Run();

            IReductStoreCollection reducts = generator.GetReductStoreCollection();
            IReductStoreCollection reductsfiltered = null;
            if (generator is ReductGeneratorMeasure)
                reductsfiltered = reducts.Filter(1, new ReductLengthComparer());
            else
                reductsfiltered = reducts.FilterInEnsemble(1, new ReductStoreLengthComparer(true));

            IReduct reduct = reductsfiltered.First().Where(r => r.IsException == false).FirstOrDefault();

            return base.Learn(data, reduct.Attributes.ToArray());
        }
    }
}
