using NUnit.Framework;
using NRough.Data;
using NRough.Data.Filters;
using NRough.MachineLearning.Classification.DecisionLookup;
using NRough.MachineLearning.Classification.DecisionTrees;
using NRough.MachineLearning.Classification.DecisionTrees.Pruning;
using NRough.MachineLearning.Discretization;
using NRough.MachineLearning.Evaluation;
using NRough.MachineLearning.Filters;
using NRough.MachineLearning.Roughset;
using System;
using System.Diagnostics;

namespace NRough.MachineLearning.Tests.Classification.DecisionTrees
{
    [TestFixture]
    public class RoughDecisionTreeTest
    {
        [Repeat(30)]
        [TestCase(@"Data\german.data", DataFormat.CSV, ReductTypes.ApproximateReductMajorityWeights, 5)]
        public void DecisionTreeWithNewDiscretization(string dataFile, DataFormat fileFormat, string reductFactoryKey, int folds)
        {
            DataStore data = DataStore.Load(dataFile, fileFormat);
            
            var emptyDistribution = EquivalenceClassCollection
                .Create(new int[] { }, data, data.Weights)
                .DecisionDistribution;

            CrossValidation cv = new CrossValidation(data, folds);
            var discFilter = new DiscretizeFilter();

            discFilter.DataStoreDiscretizer =
                new DataStoreDiscretizer(
                    new DiscretizeSupervisedBase()
                    {
                        NumberOfBuckets = 5,
                        SortCuts = false
                    })
                {
                    RemoveColumnAfterDiscretization = true,
                    UpdateDataColumns = false,
                    AddColumnsBasedOnCuts = true,
                    UseBinaryCuts = true
                };

            var reductFilter = new ReductFeatureSelectionFilter() { NumberOfReductsToTest = 20 };
            cv.Filters.Add(discFilter);
            cv.Filters.Add(reductFilter);

            double eps = -1.0;
            reductFilter.Epsilon = eps;

            ErrorImpurityTestIntPerReduct_CV(
                cv, PruningType.None, reductFactoryKey, eps, emptyDistribution.Output);

            //ErrorImpurityTestIntPerReduct_CV(
            //    cv, PruningType.ReducedErrorPruning, reductFactoryKey, eps, emptyDistribution.Output);

            for (eps = 0.0; eps <= 0.05; eps += 0.01)
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
            Trace.WriteLine("");
            Trace.WriteLine("");

            DecisionTreeC45 treeC45 = new DecisionTreeC45("Rough-Majority-" + pruningType.ToSymbol());            
            treeC45.DefaultOutput = output;
            treeC45.PruningType = pruningType;

            var treeC45Result = cv.Run<DecisionTreeC45>(treeC45);
            treeC45Result.Epsilon = eps;
            Console.WriteLine(treeC45Result);

            DecisionTreeRough treeRough = new DecisionTreeRough("C45-Entropy-" + pruningType.ToSymbol());
            treeRough.DefaultOutput = output;
            treeRough.PruningType = pruningType;

            var treeRoughResult = cv.Run<DecisionTreeRough>(treeRough);
            treeRoughResult.Epsilon = eps;
            Console.WriteLine(treeRoughResult);            

            if (pruningType == PruningType.None)
            {
                DecisionLookupMajority decTabMaj = new DecisionLookupMajority("DecTab-Majority-" + pruningType.ToSymbol());                
                decTabMaj.DefaultOutput = output;
                var decTabMajResult = cv.Run<DecisionLookupMajority>(decTabMaj);
                decTabMajResult.Epsilon = eps;
                Console.WriteLine(decTabMajResult);
            }
        }
    }
}
