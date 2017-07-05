// 
// NRough Framework
// http://www.nrough.net
// 
// Copyright (c) Sebastian Widz, 2017
// authors at nrough.net
// 
// This file is part of NRough Framework.
// 
// NRough Framework is free software; you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation; either version 3 of the License, or
// (at your option) any later version.
// 
// NRough Framework is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with NRough Framework; if not, write to the Free Software
// Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA  02110-1301  USA
//

namespace NRough.MachineLearning.Roughsets
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
}