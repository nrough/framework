using Infovision.Data;
using Infovision.MachineLearning.Classification.DecisionTables;
using Infovision.MachineLearning.Classification.DecisionTrees;
using Infovision.MachineLearning.Classification.DecisionTrees.Pruning;
using Infovision.Core;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Infovision.MachineLearning;
using Infovision.MachineLearning.Classification;

namespace Infovision.Datamining.Roughset.UnitTests.DecisionTrees
{
    [TestFixture]
    public class DecisionTreeReductCompare
    {
        [Test, Repeat(25)]
        [TestCase(@"Data\monks-1.train", @"Data\monks-1.test", PruningType.ReducedErrorPruning, ReductFactoryKeyHelper.ApproximateReductMajorityWeights)]
        [TestCase(@"Data\monks-2.train", @"Data\monks-2.test", PruningType.ReducedErrorPruning, ReductFactoryKeyHelper.ApproximateReductMajorityWeights)]
        [TestCase(@"Data\monks-3.train", @"Data\monks-3.test", PruningType.ReducedErrorPruning, ReductFactoryKeyHelper.ApproximateReductMajorityWeights)]
        [TestCase(@"Data\dna_modified.trn", @"Data\dna_modified.tst", PruningType.ReducedErrorPruning, ReductFactoryKeyHelper.ApproximateReductMajorityWeights)]
        [TestCase(@"Data\spect.train", @"Data\spect.test", PruningType.ReducedErrorPruning, ReductFactoryKeyHelper.ApproximateReductMajorityWeights)]
        [TestCase(@"Data\dna.train", @"Data\dna.test", PruningType.ReducedErrorPruning, ReductFactoryKeyHelper.ApproximateReductMajorityWeights)]
        [TestCase(@"Data\audiology.standardized.2.data", @"Data\audiology.standardized.2.test", PruningType.ReducedErrorPruning, ReductFactoryKeyHelper.ApproximateReductMajorityWeights)]
        [TestCase(@"Data\soybean-large.data", @"Data\soybean-large.test", PruningType.ReducedErrorPruning, ReductFactoryKeyHelper.ApproximateReductMajorityWeights)]
        [TestCase(@"Data\sat.disc.trn", @"Data\sat.disc.tst", PruningType.ReducedErrorPruning, ReductFactoryKeyHelper.ApproximateReductMajorityWeights)]
        [TestCase(@"Data\pendigits.disc.trn", @"Data\pendigits.disc.tst", PruningType.ReducedErrorPruning, ReductFactoryKeyHelper.ApproximateReductMajorityWeights)]
        [TestCase(@"Data\optdigits.disc.trn", @"Data\optdigits.disc.tst", PruningType.ReducedErrorPruning, ReductFactoryKeyHelper.ApproximateReductMajorityWeights)]
        [TestCase(@"Data\letter.disc.trn", @"Data\letter.disc.tst", PruningType.ReducedErrorPruning, ReductFactoryKeyHelper.ApproximateReductMajorityWeights)]
        [TestCase(@"Data\monks-1.train", @"Data\monks-1.test", PruningType.None, ReductFactoryKeyHelper.ApproximateReductMajorityWeights)]
        [TestCase(@"Data\monks-2.train", @"Data\monks-2.test", PruningType.None, ReductFactoryKeyHelper.ApproximateReductMajorityWeights)]
        [TestCase(@"Data\monks-3.train", @"Data\monks-3.test", PruningType.None, ReductFactoryKeyHelper.ApproximateReductMajorityWeights)]
        [TestCase(@"Data\dna_modified.trn", @"Data\dna_modified.tst", PruningType.None, ReductFactoryKeyHelper.ApproximateReductMajorityWeights)]
        [TestCase(@"Data\spect.train", @"Data\spect.test", PruningType.None, ReductFactoryKeyHelper.ApproximateReductMajorityWeights)]
        [TestCase(@"Data\dna.train", @"Data\dna.test", PruningType.None, ReductFactoryKeyHelper.ApproximateReductMajorityWeights)]
        [TestCase(@"Data\audiology.standardized.2.data", @"Data\audiology.standardized.2.test", PruningType.None, ReductFactoryKeyHelper.ApproximateReductMajorityWeights)]
        [TestCase(@"Data\soybean-large.data", @"Data\soybean-large.test", PruningType.None, ReductFactoryKeyHelper.ApproximateReductMajorityWeights)]
        [TestCase(@"Data\sat.disc.trn", @"Data\sat.disc.tst", PruningType.None, ReductFactoryKeyHelper.ApproximateReductMajorityWeights)]
        [TestCase(@"Data\pendigits.disc.trn", @"Data\pendigits.disc.tst", PruningType.None, ReductFactoryKeyHelper.ApproximateReductMajorityWeights)]
        [TestCase(@"Data\optdigits.disc.trn", @"Data\optdigits.disc.tst", PruningType.None, ReductFactoryKeyHelper.ApproximateReductMajorityWeights)]
        [TestCase(@"Data\letter.disc.trn", @"Data\letter.disc.tst", PruningType.None, ReductFactoryKeyHelper.ApproximateReductMajorityWeights)]
        public void ErrorImpurityTest(string trainFile, string testFile, PruningType pruningType, string reductFactoryKey)
        {
            DataStore data = DataStore.Load(trainFile, FileFormat.Rses1);
            foreach (var fieldInfo in data.DataStoreInfo.Fields) fieldInfo.IsNumeric = false;
            DataStore test = DataStore.Load(testFile, FileFormat.Rses1, data.DataStoreInfo);
            int[] allAttributes = data.DataStoreInfo.GetFieldIds(FieldTypes.Standard).ToArray();
            EquivalenceClassCollection emptyClassCollection = EquivalenceClassCollection.Create(new int[] { }, data, data.Weights);
            DecisionDistribution emptyDistribution = emptyClassCollection.DecisionDistribution;
            int rednum = 100;
            //ClassificationResult.OutputColumns = @"ds;m;t;f;eps;ens;acc;attr;numrul;dthm;dtha;gamma";
            //Console.WriteLine(ClassificationResult.ResultHeader());

            IReduct reductAllAttributes = new ReductWeights(data, allAttributes, 0, data.Weights);
            ErrorImpurityTestIntPerReduct(data, test, pruningType, reductFactoryKey, -1.0, rednum, emptyDistribution.Output, reductAllAttributes);

            if (pruningType == PruningType.None)
            {
                for (double eps = 0.0; eps <= 0.99; eps += 0.01)
                {
                    Args parms = new Args(4);
                    parms.SetParameter(ReductGeneratorParamHelper.TrainData, data);
                    parms.SetParameter(ReductGeneratorParamHelper.FactoryKey, reductFactoryKey);
                    parms.SetParameter(ReductGeneratorParamHelper.Epsilon, eps);
                    parms.SetParameter(ReductGeneratorParamHelper.NumberOfReducts, rednum);

                    IReductGenerator generator = ReductFactory.GetReductGenerator(parms);
                    generator.Run();

                    var reducts = generator.GetReducts();
                    reducts.Sort(ReductAccuracyComparer.Default);
                    IReduct bestReduct = reducts.FirstOrDefault();

                    ErrorImpurityTestIntPerReduct(data, test, pruningType, reductFactoryKey, eps, rednum, emptyDistribution.Output, bestReduct);
                }
            }
            else
            {
                for (double eps = 0.0; eps <= 0.99; eps += 0.01)
                    ErrorImpurityTestIntPerReduct(data, test, pruningType, reductFactoryKey, eps, rednum, emptyDistribution.Output, null);
            }
        }

        [Test, Repeat(1)]        
        [TestCase(@"Data\chess.data", FileFormat.Rses1, PruningType.ReducedErrorPruning, ReductFactoryKeyHelper.ApproximateReductMajorityWeights, 5)]
        [TestCase(@"Data\zoo.dta", FileFormat.Rses1, PruningType.ReducedErrorPruning, ReductFactoryKeyHelper.ApproximateReductMajorityWeights, 5)]
        [TestCase(@"Data\soybean-small.2.data", FileFormat.Rses1, PruningType.ReducedErrorPruning, ReductFactoryKeyHelper.ApproximateReductMajorityWeights, 5)]
        [TestCase(@"Data\house-votes-84.2.data", FileFormat.Rses1_1, PruningType.ReducedErrorPruning, ReductFactoryKeyHelper.ApproximateReductMajorityWeights, 5)]
        [TestCase(@"Data\agaricus-lepiota.2.data", FileFormat.Rses1, PruningType.ReducedErrorPruning, ReductFactoryKeyHelper.ApproximateReductMajorityWeights, 5)]
        [TestCase(@"Data\breast-cancer-wisconsin.2.data", FileFormat.Rses1, PruningType.ReducedErrorPruning, ReductFactoryKeyHelper.ApproximateReductMajorityWeights, 5)]
        [TestCase(@"Data\promoters.2.data", FileFormat.Rses1, PruningType.ReducedErrorPruning, ReductFactoryKeyHelper.ApproximateReductMajorityWeights, 5)]
        [TestCase(@"Data\semeion.data", FileFormat.Rses1, PruningType.ReducedErrorPruning, ReductFactoryKeyHelper.ApproximateReductMajorityWeights, 5)]
        [TestCase(@"Data\nursery.2.data", FileFormat.Rses1, PruningType.ReducedErrorPruning, ReductFactoryKeyHelper.ApproximateReductMajorityWeights, 5)]
        [TestCase(@"Data\chess.data", FileFormat.Rses1, PruningType.None, ReductFactoryKeyHelper.ApproximateReductMajorityWeights, 5)]        
        [TestCase(@"Data\zoo.dta", FileFormat.Rses1, PruningType.None, ReductFactoryKeyHelper.ApproximateReductMajorityWeights, 5)]        
        [TestCase(@"Data\soybean-small.2.data", FileFormat.Rses1, PruningType.None, ReductFactoryKeyHelper.ApproximateReductMajorityWeights, 5)]        
        [TestCase(@"Data\house-votes-84.2.data", FileFormat.Rses1_1, PruningType.None, ReductFactoryKeyHelper.ApproximateReductMajorityWeights, 5)]        
        [TestCase(@"Data\agaricus-lepiota.2.data", FileFormat.Rses1, PruningType.None, ReductFactoryKeyHelper.ApproximateReductMajorityWeights, 5)]        
        [TestCase(@"Data\breast-cancer-wisconsin.2.data", FileFormat.Rses1, PruningType.None, ReductFactoryKeyHelper.ApproximateReductMajorityWeights, 5)]        
        [TestCase(@"Data\promoters.2.data", FileFormat.Rses1, PruningType.None, ReductFactoryKeyHelper.ApproximateReductMajorityWeights, 5)]
        [TestCase(@"Data\semeion.data", FileFormat.Rses1, PruningType.None, ReductFactoryKeyHelper.ApproximateReductMajorityWeights, 5)]
        [TestCase(@"Data\nursery.2.data", FileFormat.Rses1, PruningType.None, ReductFactoryKeyHelper.ApproximateReductMajorityWeights, 5)]
        public void ErrorImpurityTest_CV(string dataFile, FileFormat fileFormat, PruningType pruningType, string reductFactoryKey, int folds)
        {
            DataStore data = DataStore.Load(dataFile, fileFormat);
            foreach (var fieldInfo in data.DataStoreInfo.Fields) fieldInfo.IsNumeric = false;
            int[] allAttributes = data.DataStoreInfo.GetFieldIds(FieldTypes.Standard).ToArray();
            EquivalenceClassCollection emptyClassCollection = EquivalenceClassCollection.Create(new int[] { }, data, data.Weights);
            DecisionDistribution emptyDistribution = emptyClassCollection.DecisionDistribution;

            DataStoreSplitter splitter = new DataStoreSplitter(data, folds, true);            
            int rednum = 100;            
            IReduct reductAllAttributes = new ReductWeights(data, allAttributes, 0, data.Weights);
            
            if (pruningType == PruningType.None)
            {                
                for (double eps = 0.0; eps <= 0.99; eps += 0.01)
                {
                    Dictionary<int, int[]> attributes = new Dictionary<int, int[]>(folds);
                    for (int f = 0; f < folds; f++)
                    {
                        DataStore trainDS = null, testDS = null;
                        splitter.Split(ref trainDS, ref testDS, f);

                        Args parms = new Args(4);
                        parms.SetParameter(ReductGeneratorParamHelper.TrainData, trainDS);
                        parms.SetParameter(ReductGeneratorParamHelper.FactoryKey, reductFactoryKey);
                        parms.SetParameter(ReductGeneratorParamHelper.Epsilon, eps);
                        parms.SetParameter(ReductGeneratorParamHelper.NumberOfReducts, rednum);
                        IReductGenerator generator = ReductFactory.GetReductGenerator(parms);
                        generator.Run();

                        var reducts = generator.GetReducts();
                        reducts.Sort(ReductAccuracyComparer.Default);
                        IReduct bestReduct = reducts.FirstOrDefault();
                        attributes.Add(f, bestReduct != null ? bestReduct.Attributes.ToArray() : new int[] { });
                    }

                    ErrorImpurityTestIntPerReduct_CV(data, splitter, pruningType, reductFactoryKey, 
                        eps, rednum, emptyDistribution.Output, attributes);
                }                
            }
            else
            {                                
                for (double eps = 0.0; eps <= 0.99; eps += 0.01)
                {
                    ErrorImpurityTestIntPerReduct_CV(data, splitter, pruningType, reductFactoryKey,
                        eps, rednum, emptyDistribution.Output, null);
                }                
            }

            ErrorImpurityTestIntPerReduct_CV(data, splitter, pruningType, reductFactoryKey,
                    -1.0, rednum, emptyDistribution.Output, null);
        }

        private void ErrorImpurityTestIntPerReduct_CV(DataStore data, DataStoreSplitter splitter, PruningType pruningType, string redkey, double eps, int rednum, long output, Dictionary<int, int[]> attributes)
        {
            Dictionary<string, Tuple<int[], DataStore>> localReductCache = new Dictionary<string, Tuple<int[], DataStore>>(5);
            object cacheLock = new object();

            Func<int[], DataStore, Tuple<int[], DataStore>> calculateReduct_Prunning = delegate (int[] attr, DataStore dta)
            {
                //assumption: in case of pruning dta.Name returns DSName-X-Y, where X is the first CV and Y is the second CV for prunning
                Tuple<int[], DataStore> best = null;
                if (localReductCache.TryGetValue(dta.Name, out best))
                    return best;

                lock (cacheLock)
                {
                    best = null;
                    if (localReductCache.TryGetValue(dta.Name, out best))
                        return best;

                    Args parms = new Args(4);
                    parms.SetParameter(ReductGeneratorParamHelper.TrainData, dta);
                    parms.SetParameter(ReductGeneratorParamHelper.FactoryKey, redkey);
                    parms.SetParameter(ReductGeneratorParamHelper.Epsilon, eps);
                    parms.SetParameter(ReductGeneratorParamHelper.NumberOfReducts, 100);
                    IReductGenerator generator = ReductFactory.GetReductGenerator(parms);
                    generator.Run();

                    var reducts = generator.GetReducts();
                    reducts.Sort(ReductAccuracyComparer.Default);
                    IReduct bestReduct = reducts.FirstOrDefault();
                    best = new Tuple<int[], DataStore>(bestReduct.Attributes.ToArray(), dta);

                    localReductCache.Add(dta.Name, best);
                }

                return best;
            };

            Func<int[], DataStore, Tuple<int[], DataStore>> calculateReduct_NoPrunning = delegate (int[] attr, DataStore dta)
            {
                return new Tuple<int[], DataStore>(attr, dta);
            };

            Func<int[], DataStore, Tuple<int[], DataStore>> attributeSelection =
                (pruningType == PruningType.None) ? calculateReduct_NoPrunning : calculateReduct_Prunning;

            AttributeAndDataSelectionMethod attributeSelectionMethod = (a, d) => attributeSelection(a, d);

            if (pruningType == PruningType.None)
            {
                DecisionTableMajority decTabMaj = new DecisionTableMajority("DecTabMaj-" + pruningType.ToSymbol());
                decTabMaj.DefaultOutput = output;
                CrossValidation<DecisionTableMajority> decTabMajCV = new CrossValidation<DecisionTableMajority>(decTabMaj);
                decTabMajCV.Attributes = attributes;
                var decTabMajResult = decTabMajCV.Run(data, splitter);
                decTabMajResult.Epsilon = eps;
                Console.WriteLine(decTabMajResult);
            }

            DecisionTreeRough treeRough = new DecisionTreeRough("Rough-Majority-" + pruningType.ToSymbol());
            treeRough.AttributeSelection = attributeSelectionMethod;            
            treeRough.DefaultOutput = output;
            treeRough.PruningType = pruningType;
            CrossValidation<DecisionTreeRough> treeRoughCV = new CrossValidation<DecisionTreeRough>(treeRough);
            treeRoughCV.Attributes = attributes;
            var treeRoughResult = treeRoughCV.Run(data, splitter);
            treeRoughResult.Epsilon = eps;
            Console.WriteLine(treeRoughResult);

            DecisionTreeC45 treec45 = new DecisionTreeC45("C45-Entropy-" + pruningType.ToSymbol());
            treec45.AttributeSelection = attributeSelectionMethod;            
            treec45.DefaultOutput = output;
            treec45.PruningType = pruningType;
            CrossValidation<DecisionTreeC45> treec45CV = new CrossValidation<DecisionTreeC45>(treec45);
            treec45CV.Attributes = attributes;
            var treec45Result = treec45CV.Run(data, splitter);
            treec45Result.Epsilon = eps;
            Console.WriteLine(treec45Result);

            ObliviousDecisionTree treeOblivEntropy = new ObliviousDecisionTree("Olv-Entropy-" + pruningType.ToSymbol());
            treeOblivEntropy.AttributeSelection = attributeSelectionMethod;            
            treeOblivEntropy.ImpurityFunction = ImpurityFunctions.Entropy;
            treeOblivEntropy.DefaultOutput = output;
            treeOblivEntropy.PruningType = pruningType;
            CrossValidation<ObliviousDecisionTree> treeOblivEntropyCV = new CrossValidation<ObliviousDecisionTree>(treeOblivEntropy);
            treeOblivEntropyCV.Attributes = attributes;
            var treeOblivEntropyResult = treeOblivEntropyCV.Run(data, splitter);
            treeOblivEntropyResult.Epsilon = eps;
            Console.WriteLine(treeOblivEntropyResult);
        }

        private void ErrorImpurityTestIntPerReduct(DataStore trainDS, DataStore testDS, PruningType pruningType, string redkey, double eps, int rednum, long output, IReduct reduct)
        {
            Dictionary<string, Tuple<int[], DataStore>> localReductCache = new Dictionary<string, Tuple<int[], DataStore>>(5);
            object cacheLock = new object();

            Func<int[], DataStore, Tuple<int[], DataStore>> calculateReduct_Prunning = delegate (int[] attr, DataStore dta)
            {
                //assumption: in case of pruning dta.Name returns DSName-X-Y, where X is the first CV and Y is the second CV for prunning
                Tuple<int[], DataStore> best = null;
                if (localReductCache.TryGetValue(dta.Name, out best))
                    return best;

                lock (cacheLock)
                {
                    best = null;
                    if (localReductCache.TryGetValue(dta.Name, out best))
                        return best;

                    Args parms = new Args(4);
                    parms.SetParameter(ReductGeneratorParamHelper.TrainData, dta);
                    parms.SetParameter(ReductGeneratorParamHelper.FactoryKey, redkey);
                    parms.SetParameter(ReductGeneratorParamHelper.Epsilon, eps);
                    parms.SetParameter(ReductGeneratorParamHelper.NumberOfReducts, 100);
                    IReductGenerator generator = ReductFactory.GetReductGenerator(parms);
                    generator.Run();

                    var reducts = generator.GetReducts();
                    reducts.Sort(ReductAccuracyComparer.Default);
                    IReduct bestReduct = reducts.FirstOrDefault();
                    best = new Tuple<int[], DataStore>(bestReduct.Attributes.ToArray(), dta);

                    localReductCache.Add(dta.Name, best);
                }

                return best;
            };

            Func<int[], DataStore, Tuple<int[], DataStore>> calculateReduct_NoPrunning = delegate (int[] attr, DataStore dta)
            {
                return new Tuple<int[], DataStore>(reduct.Attributes.ToArray(), dta);
            };

            Func<int[], DataStore, Tuple<int[], DataStore>> attributeSelection =
                (pruningType == PruningType.None) ? calculateReduct_NoPrunning : calculateReduct_Prunning;

            AttributeAndDataSelectionMethod attributeSelectionMethod = (a, d) => attributeSelection(a, d);
            
            int[] attributes = trainDS.GetStandardFields();

            if (pruningType == PruningType.None)
            {
                DecisionTableMajority decTabMaj = new DecisionTableMajority("DecTabMaj-" + pruningType.ToSymbol());
                decTabMaj.DefaultOutput = output;
                decTabMaj.Learn(trainDS, attributes);
                var decTabMajResult = Classifier.DefaultClassifer.Classify(decTabMaj, testDS);
                decTabMajResult.Epsilon = eps;
                Console.WriteLine(decTabMajResult);
            }

            DecisionTreeRough treeRough = new DecisionTreeRough("Rough-Majority-" + pruningType.ToSymbol());
            treeRough.AttributeSelection = attributeSelectionMethod;
            treeRough.DefaultOutput = output;
            treeRough.PruningType = pruningType;
            treeRough.Learn(trainDS, attributes);
            var treeRoughResult = Classifier.DefaultClassifer.Classify(treeRough, testDS);
            treeRoughResult.Epsilon = eps;
            Console.WriteLine(treeRoughResult);

            DecisionTreeC45 treec45 = new DecisionTreeC45("C45-Entropy-" + pruningType.ToSymbol());
            treec45.AttributeSelection = attributeSelectionMethod;
            treec45.DefaultOutput = output;
            treec45.PruningType = pruningType;
            treec45.Learn(trainDS, attributes);
            var treec45Result = Classifier.DefaultClassifer.Classify(treec45, testDS);
            treec45Result.Epsilon = eps;
            Console.WriteLine(treec45Result);

            ObliviousDecisionTree treeOblivEntropy = new ObliviousDecisionTree("Olv-Entropy-" + pruningType.ToSymbol());
            treeOblivEntropy.AttributeSelection = attributeSelectionMethod;
            treeOblivEntropy.ImpurityFunction = ImpurityFunctions.Entropy;
            treeOblivEntropy.DefaultOutput = output;
            treeOblivEntropy.PruningType = pruningType;
            treeOblivEntropy.Learn(trainDS, attributes);
            var treeOblivEntropyResult = Classifier.DefaultClassifer.Classify(treeOblivEntropy, testDS);
            treeOblivEntropyResult.Epsilon = eps;
            Console.WriteLine(treeOblivEntropyResult);
        }
    }
}
