using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NRough.MachineLearning.Roughsets
{    
    public static class ReductTypes
    {
        public const string ApproximateDecisionReduct = "ApproximateDecisionReduct";

        public const string GammaBireduct = "GammaBireduct";
        public const string BireductRelative = "BireductRelative";
        public const string Bireduct = "Bireduct";

        public const string ApproximateReductPositive = "ApproximateReductPositive";
        public const string ApproximateReductRelativeWeights = "ApproximateReductRelativeWeights";
        public const string ApproximateReductRelative = "ApproximateReductRelative";
        public const string ApproximateReductMajorityWeights = "ApproximateReductMajorityWeights";
        public const string ApproximateReductMajority = "ApproximateReductMajority";

        public const string ReductEnsembleStream = "ReductEnsembleStream";
        public const string ReductEnsemble = "ReductEnsemble";
        public const string ReductEnsembleBoosting = "ReductEnsembleBoosting";
        public const string ReductEnsembleBoostingWithDendrogram = "ReductEnsembleBoostingWithDendrogram";
        public const string ReductEnsembleBoostingWithDiversity = "ReductEnsembleBoostingWithDiversity";
        public const string ReductEnsembleBoostingWithAttributeDiversity = "ReductEnsembleBoostingWithAttributeDiversity";
        public const string ReductEnsembleBoostingVarEps = "ReductEnsembleBoostingVarEps";
        public const string ReductEnsembleBoostingVarEpsWithAttributeDiversity = "ReductEnsembleBoostingVarEpsWithAttributeDiversity";

        public const string GeneralizedMajorityDecision = "GeneralizedMajorityDecision";
        public const string GeneralizedMajorityDecisionApproximate = "GeneralizedMajorityDecisionApproximate";

        public const string RandomSubset = "RandomSubset";
    }
}
