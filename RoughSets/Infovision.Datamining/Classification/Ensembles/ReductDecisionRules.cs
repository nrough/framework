using NRough.Core;
using NRough.Data;
using NRough.MachineLearning.Roughset;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NRough.MachineLearning.Classification.Ensembles
{
    [Serializable]
    public class ReductDecisionRules : EnsembleBase
    {
        public Args ReductGeneratorArgs { get; set; }
        public RuleQualityMethod DecisionIdentificationMethod { get; set; }
        public RuleQualityMethod RuleVotingMethod { get; set; }

        private IList<IReduct> reducts;

        public ReductDecisionRules()
        {
            reducts = new List<IReduct>();

            Args parm = new Args();
            parm.SetParameter(ReductFactoryOptions.ReductType,
                ReductTypes.ApproximateDecisionReduct);
            parm.SetParameter(ReductFactoryOptions.FMeasure,
                (FMeasure)FMeasures.MajorityWeighted);
            parm.SetParameter(ReductFactoryOptions.Epsilon, 0.05);
            parm.SetParameter(ReductFactoryOptions.NumberOfReducts, 10);

            ReductGeneratorArgs = parm;

            DecisionIdentificationMethod = RuleQualityMethods.Confidence;
            RuleVotingMethod = RuleQualityMethods.SingleVote;
        }

        public ReductDecisionRules(IReduct reduct)
            : this()
        {
            reducts.Add(reduct);
        }

        public ReductDecisionRules(IEnumerable<IReduct> reductCollection)
            : this()
        {
            foreach (var reduct in reductCollection)
                reducts.Add(reduct);
        }

        public override ClassificationResult Learn(DataStore data, int[] attributes)
        {
            if (reducts == null || reducts.Count == 0)
            {
                if (ReductGeneratorArgs == null)
                    throw new InvalidOperationException("ReductGeneratorArgs == null");

                Args localArgs = (Args) ReductGeneratorArgs.Clone();
                localArgs.SetParameter(ReductFactoryOptions.DecisionTable, data);

                IReductGenerator generator = ReductFactory.GetReductGenerator(localArgs);
                generator.Run();

                reducts = new List<IReduct>(generator.GetReducts());
            }

            //Create rules and save them for Classify method

            
            return Classifier.Default.Classify(this, data);
        }
    }
}
