using NUnit.Framework;
using Raccoon.Core;
using Raccoon.Data;
using Raccoon.Data.Filters;
using Raccoon.MachineLearning.Classification;
using Raccoon.MachineLearning.Classification.DecisionTables;
using Raccoon.MachineLearning.Classification.DecisionTrees;
using Raccoon.MachineLearning.Classification.DecisionTrees.Pruning;
using Raccoon.MachineLearning.Roughset;
using System;

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
            var filter = new ReductFeatureSelectionFilter();

            double eps = -1.0;
            filter.Epsilon = eps;

            ErrorImpurityTestIntPerReduct_CV(cv, PruningType.None, reductFactoryKey,
                    eps, emptyDistribution.Output, filter);

            ErrorImpurityTestIntPerReduct_CV(cv, PruningType.ReducedErrorPruning, reductFactoryKey,
                    eps, emptyDistribution.Output, filter);

            for (eps = 0.0; eps <= 0.99; eps += 0.01)
            {
                filter.Epsilon = eps;
                ErrorImpurityTestIntPerReduct_CV(cv, PruningType.None, reductFactoryKey,
                    eps, emptyDistribution.Output, filter);

                ErrorImpurityTestIntPerReduct_CV(cv, PruningType.ReducedErrorPruning, reductFactoryKey,
                    eps, emptyDistribution.Output, filter);
            }
        }        

        private void ErrorImpurityTestIntPerReduct_CV(
            CrossValidation cv,
            PruningType pruningType, string redkey,
            double eps, long output, IFilter filter)
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
