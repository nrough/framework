using NUnit.Framework;
using Raccoon.Core;
using Raccoon.Data;
using Raccoon.Data.Filters;
using Raccoon.MachineLearning.Classification;
using Raccoon.MachineLearning.Classification.DecisionTables;
using Raccoon.MachineLearning.Classification.DecisionTrees;
using Raccoon.MachineLearning.Classification.DecisionTrees.Pruning;
using Raccoon.MachineLearning.Filters;
using Raccoon.MachineLearning.Roughset;
using System;
using System.Collections.Generic;

namespace Raccoon.MachineLearning.Tests.Classification.DecisionTrees
{
    [TestFixture]
    public class RoughDecisionTreeTest
    {
        [TestCase(@"Data\german.data", FileFormat.Csv, ReductFactoryKeyHelper.ApproximateReductMajorityWeights, 5)]
        public void DecisionTreeWithNewDiscretization(string dataFile, FileFormat fileFormat, string reductFactoryKey, int folds)
        {
            DataStore data = DataStore.Load(dataFile, fileFormat);            
            
            var emptyDistribution = EquivalenceClassCollection
                .Create(new int[] { }, data, data.Weights)
                .DecisionDistribution;

            CrossValidation cv = new CrossValidation(data, folds);            
            var discFilter = new DiscretizeFilter();
            var reductFilter = new ReductFeatureSelectionFilter();
            cv.Filters.Add(discFilter);
            cv.Filters.Add(reductFilter);

            double eps = -1.0;
            reductFilter.Epsilon = eps;

            ErrorImpurityTestIntPerReduct_CV(cv, PruningType.None, reductFactoryKey,
                    eps, emptyDistribution.Output);

            //ErrorImpurityTestIntPerReduct_CV(
            //    cv, PruningType.ReducedErrorPruning, reductFactoryKey, eps, emptyDistribution.Output);

            for (eps = 0.0; eps <= 0.99; eps += 0.01)
            {
                reductFilter.Epsilon = eps;

                ErrorImpurityTestIntPerReduct_CV(
                    cv, PruningType.None, reductFactoryKey, eps, emptyDistribution.Output);

                //ErrorImpurityTestIntPerReduct_CV(
                //    cv, PruningType.ReducedErrorPruning, reductFactoryKey, eps, emptyDistribution.Output);
            }
        }        

        private void ErrorImpurityTestIntPerReduct_CV(
            CrossValidation cv, PruningType pruningType, string redkey, double eps, long output)
        {
            DecisionTreeRough treeRough = new DecisionTreeRough("Rough-Majority-" + pruningType.ToSymbol());            
            treeRough.DefaultOutput = output;
            treeRough.PruningType = pruningType;

            var treeRoughResult = cv.Run<DecisionTreeRough>(treeRough);
            treeRoughResult.Epsilon = eps;
            Console.WriteLine(treeRoughResult);
        }
    }
}
