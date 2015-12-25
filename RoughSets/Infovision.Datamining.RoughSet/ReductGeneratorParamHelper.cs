using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infovision.Datamining.Roughset
{    
    public static class ReductGeneratorParamHelper
    {
        public static readonly string FactoryKey = "FactoryKey";
        public static readonly string DataStore = "DataStore";
        public static readonly string NumberOfPermutations = "NumberOfPermutations";
        public static readonly string NumberOfThreads = "NumberOfThreads";
        public static readonly string PermutationCollection = "PermutationCollection";
        public static readonly string Epsilon = "Epsilon";
        public static readonly string PermutationEpsilon = "PermutationEpsilon";
        public static readonly string Distance = "Distance";
        public static readonly string Linkage = "Linkage";
        public static readonly string NumberOfClusters = "NumberOfClusters";
        public static readonly string WeightGenerator = "WeightGenerator";
        public static readonly string ReconWeights = "ReconWeights";
        public static readonly string DendrogramBitmapFile = "DendrogramBitmapFile";
        public static readonly string NumberOfReducts = "NumberOfReducts";
        public static readonly string ReductSize = "ReductSize";
        public static readonly string MinimumNumberOfInstances = "MinimumNumberOfInstances";
        public static readonly string IdentificationType = "IdentificationType";
        public static readonly string VoteType = "VoteType";
        public static readonly string MaxReductLength = "MaxReductLength";
        public static readonly string MinReductLength = "MinReductLength";
        public static readonly string Threshold = "Threshold";
        public static readonly string NumberOfReductsInWeakClassifier = "NumberOfReductsInWeakClassifier";
        public static readonly string MaxIterations = "MaxIterations";
        public static readonly string NumberOfReductsToTest = "NumberOfReductsToTest";
        public static readonly string AgregateFunction = "AgregateFunction";
        public static readonly string CheckEnsembleErrorDuringTraining = "CheckEnsembleErrorDuringTraining";
        public static readonly string UpdateWeights = "UpdateWeights";
        public static readonly string CalcModelConfidence = "CalcModelConfidence";
        public static readonly string UseExceptionRules = "UseExceptionRules";
        public static readonly string AttributeReductionStep = "AttributeReductionStep";
        public static readonly string InnerParameters = "InnerParameters";
    }

    //TODO Keys should be returned from Generator classes as static fields
    public static class ReductFactoryKeyHelper
    {
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
        public const string ReductGeneralizedDecision = "ReductGeneralizedDecision";
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
