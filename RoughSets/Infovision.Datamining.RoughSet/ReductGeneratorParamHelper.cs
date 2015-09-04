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
        public static readonly string ApproximationRatio = "ApproximationRatio";
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
    }

    //TODO Keys should be returned from Generator classes as static fields
    public static class ReductFactoryKeyHelper
    {
        public static readonly string GammaBireduct = "GammaBireduct";
        public static readonly string BireductRelative = "BireductRelative";
        public static readonly string Bireduct = "Bireduct";
        public static readonly string ApproximateReductPositive = "ApproximateReductPositive";
        public static readonly string ApproximateReductRelativeWeights = "ApproximateReductRelativeWeights";
        public static readonly string ApproximateReductRelative = "ApproximateReductRelative";
        public static readonly string ApproximateReductMajorityWeights = "ApproximateReductMajorityWeights";
        public static readonly string ApproximateReductMajority = "ApproximateReductMajority";
        public static readonly string ReductEnsembleStream = "ReductEnsembleStream";
        public static readonly string ReductEnsemble = "ReductEnsemble";
        public static readonly string ReductGeneralizedDecision = "ReductGeneralizedDecision";
        public static readonly string ReductEnsembleBoosting = "ReductEnsembleBoosting";
        public static readonly string ReductEnsembleBoostingWithDendrogram = "ReductEnsembleBoostingWithDendrogram";
        public static readonly string ReductEnsembleBoostingWithDiversity = "ReductEnsembleBoostingWithDiversity";
        public static readonly string ReductEnsembleBoostingWithAttributeDiversity = "ReductEnsembleBoostingWithAttributeDiversity";
    }
}
