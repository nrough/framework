namespace NRough.MachineLearning.Roughset
{
    public static class ReductFactoryOptions
    {
        public static readonly string FMeasure = "FMeasure";
        public static readonly string ReductType = "ReductType";
        public static readonly string DecisionTable = "Data";
        public static readonly string TestData = "TestData";
        public static readonly string NumberOfPermutations = "NumberOfPermutations";
        
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
        public static readonly string UpdateWeights = "SetWeights";
        public static readonly string CalcModelConfidence = "CalcModelConfidence";
        public static readonly string UseExceptionRules = "UseExceptionRules";
        public static readonly string ReductionStep = "ReductionStep";
        public static readonly string InnerParameters = "InnerParameters";
        public static readonly string PermuatationGenerator = "PermutationGenerator";
        public static readonly string MaxNumberOfWeightResets = "MaxNumberOfWeightResets";
        public static readonly string MinimumVoteValue = "MinimumVoteValue";
        public static readonly string FixedPermutations = "FixedPermutations";
        public static readonly string UseClassificationCost = "UseClassificationCost";
        public static readonly string CVActiveFold = "CVActiveFold";
        public static readonly string EquivalenceClassSortDirection = "EquivalenceClassSortDirection";
        public static readonly string DataSetQuality = "DataSetQuality";
        public static readonly string InitialEquivalenceClassCollection = "InitialEquivalenceClassCollection";


        public static readonly string Diversify = "Diversify";
        public static readonly string ReductComparer = "ReductComparer";
        public static readonly string SelectTopReducts = "SelectTopReducts";
    }

    //TODO Keys should be returned from Generator classes as static fileLine
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