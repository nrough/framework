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
using System.Collections.Generic;
using NRough.MachineLearning.Weighting;

namespace NRough.Tests.MachineLearning.Classification.DecisionTrees
{
    [TestFixture]
    public class RoughDecisionTreeTest
    {       
        public static void PruneTree(IModel model, DataStore data)
        {
            IDecisionTree tree = (IDecisionTree)model;
            var pruning = new ErrorBasedPruning(tree, data);
            pruning.Prune();            
        }
        
        [TestCase(@"Data\soybean-small.2.data", DataFormat.RSES1, ReductTypes.ApproximateReductMajorityWeights, 5)]
        [TestCase(@"Data\house-votes-84.2.data", DataFormat.RSES1_1, ReductTypes.ApproximateReductMajorityWeights, 5)]
        [TestCase(@"Data\agaricus-lepiota.2.data", DataFormat.RSES1, ReductTypes.ApproximateReductMajorityWeights, 5)]
        [TestCase(@"Data\breast-cancer-wisconsin.2.data", DataFormat.RSES1, ReductTypes.ApproximateReductMajorityWeights, 5)]
        [TestCase(@"Data\promoters.2.data", DataFormat.RSES1, ReductTypes.ApproximateReductMajorityWeights, 5)]
        [TestCase(@"Data\semeion.data", DataFormat.RSES1, ReductTypes.ApproximateReductMajorityWeights, 5)]
        [TestCase(@"Data\nursery.2.data", DataFormat.RSES1, ReductTypes.ApproximateReductMajorityWeights, 5)]
        //[TestCase(@"Data\vehicle.tab", DataFormat.RSES1, ReductTypes.ApproximateReductMajorityWeights, 5)]
        //[TestCase(@"Data\german.data", DataFormat.CSV, ReductTypes.ApproximateReductMajorityWeights, 5)]
        [TestCase(@"Data\dermatology_modified.data", DataFormat.CSV, ReductTypes.ApproximateReductMajorityWeights, 5)]
        //[TestCase(@"Data\dermatology.data", DataFormat.CSV, ReductTypes.ApproximateReductMajorityWeights, 5)]
        [TestCase(@"Data\hypothyroid.data", DataFormat.CSV, ReductTypes.ApproximateReductMajorityWeights, 5)]
        [TestCase(@"Data\lymphography.all", DataFormat.CSV, ReductTypes.ApproximateReductMajorityWeights, 5)]
        [TestCase(@"Data\chess.data", DataFormat.RSES1, ReductTypes.ApproximateReductMajorityWeights, 5)]
        [TestCase(@"Data\zoo.dta", DataFormat.RSES1, ReductTypes.ApproximateReductMajorityWeights, 5)]
        public void DecisionTreeWithCV(string dataFile, DataFormat fileFormat, string reductFactoryKey, int folds)
        {
            DataStore data = DataStore.Load(dataFile, fileFormat);
            foreach (var attribute in data.DataStoreInfo.Attributes)
                attribute.IsNumeric = false;
            
            CrossValidation cv = new CrossValidation(data, folds);
            //cv.RunInParallel = false;

            var permutations = new PermutationCollection(
                20, data.SelectAttributeIds(a => a.IsStandard).ToArray());

            var reductFilter = new ReductFeatureSelectionFilter()
            {                
                ReductFactoryKey = reductFactoryKey,
                Permutations = permutations,
                Greedy = false
            };

            cv.Filters.Add(reductFilter);
            cv.PostLearningMethod = PruneTree;

            for (double eps = 0.0; eps < 1.0; eps += 0.01)
            {
                reductFilter.Epsilon = eps;
                TestStandardDecisionTree_CV(
                    cv, PruningType.ErrorBasedPruning, eps, Classifier.UnclassifiedOutput);
                
                ErrorImpurityTestIntPerReduct_CV(
                    cv, PruningType.ErrorBasedPruning, eps, Classifier.UnclassifiedOutput);
            }
        }


        [TestCase(@"Data\dna.train", @"Data\dna.test", DataFormat.RSES1, ReductTypes.ApproximateReductMajorityWeights)]
        [TestCase(@"Data\audiology.standardized.2.data", @"Data\audiology.standardized.2.test", DataFormat.RSES1, ReductTypes.ApproximateReductMajorityWeights)]
        //[TestCase(@"Data\dna_modified.trn", @"Data\dna_modified.tst", DataFormat.RSES1, ReductTypes.ApproximateReductMajority)]        
        [TestCase(@"Data\monks-1.train", @"Data\monks-1.test", DataFormat.RSES1, ReductTypes.ApproximateReductMajorityWeights)]
        //[TestCase(@"Data\monks-2.train", @"Data\monks-2.test", DataFormat.RSES1, ReductTypes.ApproximateReductMajorityWeights)]
        [TestCase(@"Data\monks-3.train", @"Data\monks-3.test", DataFormat.RSES1, ReductTypes.ApproximateReductMajorityWeights)]
        [TestCase(@"Data\sat.disc.trn", @"Data\sat.disc.tst", DataFormat.RSES1, ReductTypes.ApproximateReductMajorityWeights)]
        [TestCase(@"Data\spect.train", @"Data\spect.test", DataFormat.RSES1, ReductTypes.ApproximateReductMajorityWeights)]
        [TestCase(@"Data\vowel.disc.trn", @"Data\vowel.disc.tst", DataFormat.CSV, ReductTypes.ApproximateReductMajorityWeights)]        
        [TestCase(@"Data\soybean-large.data", @"Data\soybean-large.test", DataFormat.RSES1, ReductTypes.ApproximateReductMajorityWeights)]
        //[TestCase(@"Data\letter.trn", @"Data\letter.tst", DataFormat.RSES1, ReductTypes.ApproximateReductMajorityWeights)]
        [TestCase(@"Data\letter.disc.trn", @"Data\letter.disc.tst", DataFormat.RSES1, ReductTypes.ApproximateReductMajorityWeights)]
        [TestCase(@"Data\pendigits.disc.trn", @"Data\pendigits.disc.tst", DataFormat.RSES1, ReductTypes.ApproximateReductMajorityWeights)]
        //[TestCase(@"Data\optdigits.disc.trn", @"Data\optdigits.disc.tst", DataFormat.RSES1, ReductTypes.ApproximateReductMajorityWeights)]
        //[TestCase(@"Data\pendigits.trn", @"Data\pendigits.tst", DataFormat.RSES1, ReductTypes.ApproximateReductMajorityWeights)]
        //[TestCase(@"Data\optdigits.trn", @"Data\optdigits.tst", DataFormat.RSES1, ReductTypes.ApproximateReductMajorityWeights)]
        
        public void DecisionTreeBenchmarkSplittedData(
            string trainFile, string testFile, DataFormat fileFormat, string reductFactoryKey)
        {
            DataStore data = DataStore.Load(trainFile, fileFormat);
            foreach (var attribute in data.DataStoreInfo.Attributes)
                attribute.IsNumeric = false;
            DataStore test = DataStore.Load(testFile, fileFormat, data.DataStoreInfo);
           
            var permutations = new PermutationCollection(
                20, data.SelectAttributeIds(a => a.IsStandard).ToArray());            

            for (double eps = 0.0; eps < 1.0; eps += 0.01)
            {                
                TestStandardDecisionTree(data, test, PruningType.ErrorBasedPruning, eps, Classifier.UnclassifiedOutput);

                var reducts = ComputeReduct(data, reductFactoryKey, (FMeasure)FMeasures.MajorityWeighted, eps, permutations);
                reducts = reducts.OrderBy(r => r.EquivalenceClasses.Count).Take(1);

                foreach (var bestReduct in reducts)
                {                     
                    ErrorImpurityTestIntPerReduct_noCV(
                        data, test, PruningType.ErrorBasedPruning, eps, bestReduct.Attributes.ToArray(), Classifier.UnclassifiedOutput);
                }
            }
        }

        private IEnumerable<IReduct> ComputeReduct(
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

            var rGen = reductGenerator as ReductGeneratorMeasure;
            if (rGen != null)
            {                
                rGen.Greedy = false;
            }

            return reductGenerator.GetReducts();            
        }

        private void TestStandardDecisionTree(DataStore train, DataStore test,
            PruningType pruningType, double eps, long output, bool printTree = false)
        {
            var treeC45 = new DecisionTreeC45("C45-Entropy-" + pruningType.ToSymbol());
            treeC45.DefaultOutput = output;            
            treeC45.Gamma = eps;
            treeC45.Learn(train, train.SelectAttributeIds(a => a.IsStandard).ToArray());

            if (pruningType != PruningType.None)
            {
                IDecisionTreePruning pruningMethod = DecisionTreePruningBase.Construct(pruningType, treeC45, train);
                pruningMethod.Prune();
            }

            var treeC45Result = Classifier.Default.Classify(treeC45, test);
            treeC45Result.Epsilon = eps;
            treeC45Result.Gamma = eps;
            Console.WriteLine(treeC45Result);

            //Console.WriteLine(treeC45Result.ConfusionMatrix);

            if (printTree)
                Console.WriteLine(DecisionTreeFormatter.Construct(treeC45));            
        }

        private void ErrorImpurityTestIntPerReduct_noCV(
            DataStore train, DataStore test, 
            PruningType pruningType, double eps, int[] reduct,
            long output, bool printTree = false)
        {                        
            var treeC45_Reduct = new DecisionTreeC45("Reduct-Entropy-" + pruningType.ToSymbol());
            treeC45_Reduct.DefaultOutput = output;
            treeC45_Reduct.Learn(train, reduct);

            if (pruningType != PruningType.None)
            {
                IDecisionTreePruning pruningMethod = DecisionTreePruningBase.Construct(pruningType, treeC45_Reduct, train);
                pruningMethod.Prune();
            }

            var treeC45Result_Reduct = Classifier.Default.Classify(treeC45_Reduct, test);
            treeC45Result_Reduct.Epsilon = eps;            
            Console.WriteLine(treeC45Result_Reduct);

            if (printTree)
                Console.WriteLine(DecisionTreeFormatter.Construct(treeC45_Reduct));

            DecisionTreeRough treeRough = new DecisionTreeRough("Reduct-Majority-" + pruningType.ToSymbol());
            treeRough.DefaultOutput = output;            
            treeRough.Learn(train, reduct);

            if (pruningType != PruningType.None)
            {
                IDecisionTreePruning pruningMethod = DecisionTreePruningBase.Construct(pruningType, treeRough, train);
                pruningMethod.Prune();
            }

            var treeRoughResult = Classifier.Default.Classify(treeRough, test);
            treeRoughResult.Epsilon = eps;
            Console.WriteLine(treeRoughResult);

            if (printTree)
                Console.WriteLine(DecisionTreeFormatter.Construct(treeRough));

            DecisionTreeRough treeRough2 = new DecisionTreeRough("RedGam-Majority-" + pruningType.ToSymbol());
            treeRough2.DefaultOutput = output;            
            treeRough2.Gamma = eps;
            treeRough2.Learn(train, reduct);

            if (pruningType != PruningType.None)
            {
                IDecisionTreePruning pruningMethod = DecisionTreePruningBase.Construct(pruningType, treeRough2, train);
                pruningMethod.Prune();
            }

            var treeRoughResult2 = Classifier.Default.Classify(treeRough2, test);
            treeRoughResult2.Epsilon = eps;
            treeRoughResult2.Gamma = eps;
            Console.WriteLine(treeRoughResult2);

            if (printTree)
                Console.WriteLine(DecisionTreeFormatter.Construct(treeRough2));
        }

        private void TestStandardDecisionTree_CV(CrossValidation cv,
            PruningType pruningType, double eps, long output, bool printTree = false)
        {
            cv.Filters.First().Enabled = false; //disable reduct filter

            var treeC45 = new DecisionTreeC45("C45-Entropy-" + pruningType.ToSymbol());
            treeC45.DefaultOutput = output;
            treeC45.Gamma = eps;
            var treeC45_Result = cv.Run<DecisionTreeC45>(treeC45);
            treeC45_Result.Epsilon = eps;
            treeC45_Result.Gamma = eps;
            Console.WriteLine(treeC45_Result);                        

            if (printTree)
                Console.WriteLine(DecisionTreeFormatter.Construct(treeC45));

            cv.Filters.First().Enabled = true; //disable reduct filter
        }

        private void ErrorImpurityTestIntPerReduct_CV(
            CrossValidation cv,
            PruningType pruningType, double eps,
            long output, bool printTree = false)
        {
            var treeC45_Reduct = new DecisionTreeC45("Reduct-Entropy-" + pruningType.ToSymbol());
            treeC45_Reduct.DefaultOutput = output;
            var treeC45Result_Reduct = cv.Run<DecisionTreeC45>(treeC45_Reduct);
            treeC45Result_Reduct.Epsilon = eps;
            Console.WriteLine(treeC45Result_Reduct);                      

            if (printTree)
                Console.WriteLine(DecisionTreeFormatter.Construct(treeC45_Reduct));

            DecisionTreeRough treeRough = new DecisionTreeRough("Reduct-Majority-" + pruningType.ToSymbol());
            treeRough.DefaultOutput = output;
            var treeRoughResult = cv.Run<DecisionTreeRough>(treeRough);
            treeRoughResult.Epsilon = eps;
            Console.WriteLine(treeRoughResult);            

            if (printTree)
                Console.WriteLine(DecisionTreeFormatter.Construct(treeRough));

            DecisionTreeRough treeRough2 = new DecisionTreeRough("RedGam-Majority-" + pruningType.ToSymbol());
            treeRough2.DefaultOutput = output;
            treeRough2.Gamma = eps;
            var treeRoughResult2 = cv.Run<DecisionTreeRough>(treeRough2);
            treeRoughResult2.Epsilon = eps;
            treeRoughResult2.Gamma = eps;
            Console.WriteLine(treeRoughResult2);                       

            if (printTree)
                Console.WriteLine(DecisionTreeFormatter.Construct(treeRough2));
        }


    }
}
