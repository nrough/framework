using NUnit.Framework;
using NRough.Data;
using NRough.Data.Filters;
using NRough.MachineLearning.Classification.DecisionLookup;
using NRough.MachineLearning.Classification.DecisionTrees;
using NRough.MachineLearning.Classification.DecisionTrees.Pruning;
using NRough.MachineLearning.Discretization;
using NRough.MachineLearning.Evaluation;
using NRough.MachineLearning.Filters;
using NRough.MachineLearning.Roughsets;
using System;
using System.Diagnostics;
using NRough.MachineLearning;
using System.Linq;
using NRough.MachineLearning.Classification;
using NRough.Core;
using NRough.MachineLearning.Permutations;

namespace NRough.Tests.MachineLearning.Classification.DecisionTrees
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

            discFilter.TableDiscretizer =
                new DecisionTableDiscretizer(
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

            for (eps = 0.0; eps <= 1.0; eps += 0.01)
            {
                reductFilter.Epsilon = eps;

                ErrorImpurityTestIntPerReduct_CV(cv, PruningType.None, reductFactoryKey, eps, emptyDistribution.Output);

                //ErrorImpurityTestIntPerReduct_CV(
                //    cv, PruningType.ReducedErrorPruning, reductFactoryKey, eps, emptyDistribution.Output);
            }
        }

        private void ErrorImpurityTestIntPerReduct_CV(
            CrossValidation cv, PruningType pruningType, string redkey, double eps, long output)
        {
            DecisionTreeC45 treeC45 = new DecisionTreeC45("C45-Entropy-" + pruningType.ToSymbol());
            treeC45.DefaultOutput = output;
            treeC45.PruningType = pruningType;

            var treeC45Result = cv.Run<DecisionTreeC45>(treeC45);
            treeC45Result.Epsilon = eps;
            Console.WriteLine(treeC45Result);

            DecisionTreeRough treeRough = new DecisionTreeRough("Rough-Majority-" + pruningType.ToSymbol());
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


        //[TestCase(@"Data\audiology.standardized.2.data", @"Data\audiology.standardized.2.test", DataFormat.RSES1, ReductTypes.ApproximateReductMajorityWeights)]
        [TestCase(@"Data\dna_modified.trn", @"Data\dna_modified.tst", DataFormat.RSES1, ReductTypes.ApproximateReductMajorityWeights)]
        //[TestCase(@"Data\dna.train", @"Data\dna.test", DataFormat.RSES1, ReductTypes.ApproximateReductMajorityWeights)]
        [TestCase(@"Data\monks-1.train", @"Data\monks-1.test", DataFormat.RSES1, ReductTypes.ApproximateReductMajorityWeights)]
        [TestCase(@"Data\monks-3.train", @"Data\monks-3.test", DataFormat.RSES1, ReductTypes.ApproximateReductMajorityWeights)]
        [TestCase(@"Data\sat.disc.trn", @"Data\sat.disc.tst", DataFormat.RSES1, ReductTypes.ApproximateReductMajorityWeights)]
        [TestCase(@"Data\spect.train", @"Data\spect.test", DataFormat.RSES1, ReductTypes.ApproximateReductMajorityWeights)]
        [TestCase(@"Data\vowel.disc.trn", @"Data\vowel.disc.tst", DataFormat.CSV, ReductTypes.ApproximateReductMajorityWeights)]        
        [TestCase(@"Data\soybean-large.data", @"Data\soybean-large.test", DataFormat.RSES1, ReductTypes.ApproximateReductMajorityWeights)]
        [TestCase(@"Data\letter.trn", @"Data\letter.tst", DataFormat.RSES1, ReductTypes.ApproximateReductMajorityWeights)]
        //[TestCase(@"Data\pendigits.disc.trn", @"Data\pendigits.disc.tst", DataFormat.RSES1, ReductTypes.ApproximateReductMajorityWeights)]
        //[TestCase(@"Data\optdigits.disc.trn", @"Data\optdigits.disc.tst", DataFormat.RSES1, ReductTypes.ApproximateReductMajorityWeights)]
        public void DecisionTreeBenchmarkSplittedData(
            string trainFile, string testFile, DataFormat fileFormat, string reductFactoryKey)
        {
            DataStore data = DataStore.Load(trainFile, fileFormat);
            foreach (var attribute in data.DataStoreInfo.Attributes) attribute.IsNumeric = false;
            DataStore test = DataStore.Load(testFile, fileFormat, data.DataStoreInfo);

            var permutations = new PermutationCollection(
                100, data.SelectAttributeIds(a => a.IsStandard).ToArray());

            var emptyDistribution = EquivalenceClassCollection
                .Create(new int[] { }, data, data.Weights)
                .DecisionDistribution;

            //Console.WriteLine(ClassificationResult.TableHeader());

            double eps = -1.0;            
            ErrorImpurityTestIntPerReduct_noCV(
                data, test, PruningType.None, 
                eps, data.SelectAttributeIds(a=>a.IsStandard).ToArray(),
                -1);

            for (eps = 0.0; eps <= 0.5; eps += 0.01)
            {
                int[] reduct = ComputeReduct(data, 
                    reductFactoryKey, (FMeasure)FMeasures.MajorityWeighted, eps, permutations);

                ErrorImpurityTestIntPerReduct_noCV(
                    data, test, PruningType.None, 
                    eps, reduct,
                    -1);
            }
        }

        private int[] ComputeReduct(
            DataStore data, string redkey, FMeasure f, double eps, 
            PermutationCollection permutations)
        {            
            var parm = new Args();
            parm.SetParameter(ReductFactoryOptions.DecisionTable, data);
            parm.SetParameter(ReductFactoryOptions.ReductType, redkey);
            parm.SetParameter(ReductFactoryOptions.FMeasure, f);
            parm.SetParameter(ReductFactoryOptions.Epsilon, eps);
            parm.SetParameter(ReductFactoryOptions.PermutationCollection, permutations);

            var reductGenerator = ReductFactory.GetReductGenerator(parm);
            var reducts = reductGenerator.GetReducts();

            var bestReduct = reducts.OrderBy(r => r.EquivalenceClasses.Count).First();

            return bestReduct.Attributes.ToArray();
        }

        private void ErrorImpurityTestIntPerReduct_noCV(
            DataStore train, DataStore test, 
            PruningType pruningType, double eps, int[] reduct,
            long output)
        {            
            DecisionTreeC45 treeC45 = new DecisionTreeC45("C45-Entropy-" + pruningType.ToSymbol());
            treeC45.DefaultOutput = output;
            treeC45.PruningType = pruningType;
            treeC45.Gamma = eps;
            treeC45.Learn(train, train.SelectAttributeIds(a => a.IsStandard).ToArray());

            var treeC45Result = Classifier.Default.Classify(treeC45, test);
            treeC45Result.Epsilon = eps;
            treeC45Result.Gamma = eps;
            Console.WriteLine(treeC45Result);

            DecisionTreeC45 treeC45_Reduct = new DecisionTreeC45("Reduct-Entropy-" + pruningType.ToSymbol());
            treeC45_Reduct.DefaultOutput = output;
            treeC45_Reduct.PruningType = pruningType;            
            treeC45_Reduct.Learn(train, reduct);

            var treeC45Result_Reduct = Classifier.Default.Classify(treeC45_Reduct, test);
            treeC45Result_Reduct.Epsilon = eps;            
            Console.WriteLine(treeC45Result_Reduct);

            DecisionTreeRough treeRough = new DecisionTreeRough("Reduct-Majority-" + pruningType.ToSymbol());
            treeRough.DefaultOutput = output;
            treeRough.PruningType = pruningType;
            treeRough.Learn(train, reduct);

            var treeRoughResult = Classifier.Default.Classify(treeRough, test);
            treeRoughResult.Epsilon = eps;
            Console.WriteLine(treeRoughResult);

            DecisionTreeRough treeRough2 = new DecisionTreeRough("Reduct-Gamma-" + pruningType.ToSymbol());
            treeRough2.DefaultOutput = output;
            treeRough2.PruningType = pruningType;
            treeRough2.Gamma = eps;
            treeRough2.Learn(train, reduct);

            var treeRoughResult2 = Classifier.Default.Classify(treeRough2, test);
            treeRoughResult2.Epsilon = eps;
            treeRoughResult2.Gamma = eps;
            Console.WriteLine(treeRoughResult2);
        }


    }
}
