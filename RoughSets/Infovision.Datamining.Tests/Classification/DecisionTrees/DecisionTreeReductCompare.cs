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
using Infovision.MachineLearning.Roughset;
using Infovision.MachineLearning.Classification;
using Infovision.MachineLearning.Filters.Supervised.Attribute;
using Infovision.MachineLearning.Permutations;
using Infovision.MachineLearning.Classification.DecisionRules;

namespace Infovision.MachineLearning.Tests.Classification.DecisionTrees
{
    [TestFixture]
    public class DecisionTreeReductCompare
    {
        [Test, Repeat(1)]        
        [TestCase(@"Data\monks-1.train", @"Data\monks-1.test", FileFormat.Rses1, PruningType.ReducedErrorPruning, ReductFactoryKeyHelper.ApproximateReductMajorityWeights)]
        [TestCase(@"Data\monks-2.train", @"Data\monks-2.test", FileFormat.Rses1, PruningType.ReducedErrorPruning, ReductFactoryKeyHelper.ApproximateReductMajorityWeights)]
        [TestCase(@"Data\monks-3.train", @"Data\monks-3.test", FileFormat.Rses1, PruningType.ReducedErrorPruning, ReductFactoryKeyHelper.ApproximateReductMajorityWeights)]
        [TestCase(@"Data\dna_modified.trn", @"Data\dna_modified.tst", FileFormat.Rses1, PruningType.ReducedErrorPruning, ReductFactoryKeyHelper.ApproximateReductMajorityWeights)]
        [TestCase(@"Data\spect.train", @"Data\spect.test", FileFormat.Rses1, PruningType.ReducedErrorPruning, ReductFactoryKeyHelper.ApproximateReductMajorityWeights)]
        [TestCase(@"Data\dna.train", @"Data\dna.test", FileFormat.Rses1, PruningType.ReducedErrorPruning, ReductFactoryKeyHelper.ApproximateReductMajorityWeights)]
        [TestCase(@"Data\audiology.standardized.2.data", FileFormat.Rses1, @"Data\audiology.standardized.2.test", PruningType.ReducedErrorPruning, ReductFactoryKeyHelper.ApproximateReductMajorityWeights)]
        [TestCase(@"Data\soybean-large.data", @"Data\soybean-large.test", FileFormat.Rses1, PruningType.ReducedErrorPruning, ReductFactoryKeyHelper.ApproximateReductMajorityWeights)]
        [TestCase(@"Data\sat.disc.trn", @"Data\sat.disc.tst", FileFormat.Rses1, PruningType.ReducedErrorPruning, ReductFactoryKeyHelper.ApproximateReductMajorityWeights)]
        [TestCase(@"Data\pendigits.disc.trn", @"Data\pendigits.disc.tst", FileFormat.Rses1, PruningType.ReducedErrorPruning, ReductFactoryKeyHelper.ApproximateReductMajorityWeights)]
        [TestCase(@"Data\optdigits.disc.trn", @"Data\optdigits.disc.tst", FileFormat.Rses1, PruningType.ReducedErrorPruning, ReductFactoryKeyHelper.ApproximateReductMajorityWeights)]
        [TestCase(@"Data\letter.disc.trn", @"Data\letter.disc.tst", FileFormat.Rses1, PruningType.ReducedErrorPruning, ReductFactoryKeyHelper.ApproximateReductMajorityWeights)]
        [TestCase(@"Data\monks-1.train", @"Data\monks-1.test", FileFormat.Rses1, PruningType.None, ReductFactoryKeyHelper.ApproximateReductMajorityWeights)]
        [TestCase(@"Data\monks-2.train", @"Data\monks-2.test", FileFormat.Rses1, PruningType.None, ReductFactoryKeyHelper.ApproximateReductMajorityWeights)]
        [TestCase(@"Data\monks-3.train", @"Data\monks-3.test", FileFormat.Rses1, PruningType.None, ReductFactoryKeyHelper.ApproximateReductMajorityWeights)]
        [TestCase(@"Data\dna_modified.trn", @"Data\dna_modified.tst", FileFormat.Rses1, PruningType.None, ReductFactoryKeyHelper.ApproximateReductMajorityWeights)]
        [TestCase(@"Data\spect.train", @"Data\spect.test", FileFormat.Rses1, PruningType.None, ReductFactoryKeyHelper.ApproximateReductMajorityWeights)]
        [TestCase(@"Data\dna.train", @"Data\dna.test", FileFormat.Rses1, PruningType.None, ReductFactoryKeyHelper.ApproximateReductMajorityWeights)]
        [TestCase(@"Data\audiology.standardized.2.data", @"Data\audiology.standardized.2.test", FileFormat.Rses1, PruningType.None, ReductFactoryKeyHelper.ApproximateReductMajorityWeights)]
        [TestCase(@"Data\soybean-large.data", @"Data\soybean-large.test", FileFormat.Rses1, PruningType.None, ReductFactoryKeyHelper.ApproximateReductMajorityWeights)]
        [TestCase(@"Data\sat.disc.trn", @"Data\sat.disc.tst", FileFormat.Rses1, PruningType.None, ReductFactoryKeyHelper.ApproximateReductMajorityWeights)]
        [TestCase(@"Data\pendigits.disc.trn", @"Data\pendigits.disc.tst", FileFormat.Rses1, PruningType.None, ReductFactoryKeyHelper.ApproximateReductMajorityWeights)]
        [TestCase(@"Data\optdigits.disc.trn", @"Data\optdigits.disc.tst", FileFormat.Rses1, PruningType.None, ReductFactoryKeyHelper.ApproximateReductMajorityWeights)]
        [TestCase(@"Data\letter.disc.trn", @"Data\letter.disc.tst", FileFormat.Rses1, PruningType.None, ReductFactoryKeyHelper.ApproximateReductMajorityWeights)]       
        [TestCase(@"Data\vowel.disc.trn", @"Data\vowel.disc.tst", FileFormat.Csv, PruningType.ReducedErrorPruning, ReductFactoryKeyHelper.ApproximateReductMajorityWeights)]
        [TestCase(@"Data\vowel.disc.trn", @"Data\vowel.disc.tst", FileFormat.Csv, PruningType.None, ReductFactoryKeyHelper.ApproximateReductMajorityWeights)]
        public void ErrorImpurityTest(string trainFile, string testFile, FileFormat fileFormat, PruningType pruningType, string reductFactoryKey)
        {
            DataStore data = DataStore.Load(trainFile, fileFormat);
            foreach (var fieldInfo in data.DataStoreInfo.Fields) fieldInfo.IsNumeric = false;
            DataStore test = DataStore.Load(testFile, fileFormat, data.DataStoreInfo);

            int[] allAttributes = data.DataStoreInfo.GetFieldIds(FieldTypes.Standard).ToArray();

            EquivalenceClassCollection emptyClassCollection = EquivalenceClassCollection.Create(new int[] { }, data, data.Weights);
            DecisionDistribution emptyDistribution = emptyClassCollection.DecisionDistribution;

            int rednum = 100;
            PermutationGenerator permutationGenerator = new PermutationGenerator(allAttributes);
            PermutationCollection permutationCollection = permutationGenerator.Generate(rednum);

            Dictionary<string, Tuple<int[], DataStore>> localReductCache = new Dictionary<string, Tuple<int[], DataStore>>(5);
            object cacheLock = new object();

            double eps = 0.0;

            Func<IModel, int[], DataStore, Tuple<int[], DataStore>> calculateReduct =
                delegate (IModel model, int[] attr, DataStore trainingSet)
                {
                    Tuple<int[], DataStore> best = null;
                    if (localReductCache.TryGetValue(GetKey(trainingSet, eps, pruningType), out best))
                        return best;

                    lock (cacheLock)
                    {
                        best = null;
                        if (localReductCache.TryGetValue(GetKey(trainingSet, eps, pruningType), out best))
                            return best;

                        if (trainingSet.DataStoreInfo.GetFields(FieldTypes.Standard).Any(f => f.CanDiscretize()))
                        {
                            var descretizer = new DataStoreDiscretizer()
                            {
                                UseBetterEncoding = false,
                                UseKononenko = false,
                                Fields2Discretize = trainingSet.DataStoreInfo.GetFields(FieldTypes.Standard)
                                            .Where(f => f.CanDiscretize())
                                            .Select(fld => fld.Id)
                            };

                            descretizer.Discretize(trainingSet, trainingSet.Weights);
                        }

                        Args parms = new Args(4);
                        parms.SetParameter(ReductGeneratorParamHelper.TrainData, trainingSet);
                        parms.SetParameter(ReductGeneratorParamHelper.FactoryKey, reductFactoryKey);
                        parms.SetParameter(ReductGeneratorParamHelper.Epsilon, eps);
                        parms.SetParameter(ReductGeneratorParamHelper.PermutationCollection, permutationCollection);

                        IReductGenerator generator = ReductFactory.GetReductGenerator(parms);
                        generator.Run();

                        var reducts = generator.GetReducts();
                        reducts.Sort(ReductAccuracyComparer.Default);
                        IReduct bestReduct = reducts.FirstOrDefault();
                        best = new Tuple<int[], DataStore>(bestReduct.Attributes.ToArray(), trainingSet);

                        localReductCache.Add(GetKey(trainingSet, eps, pruningType), best);
                    }

                    return best;
                };            

            AttributeAndDataSelectionMethod attributeSelectionMethod = (m, a, d) => calculateReduct(m, a, d);
            
            ErrorImpurityTestIntPerReduct(
                data, test, pruningType, reductFactoryKey, 
                -1.0, rednum, emptyDistribution.Output, attributeSelectionMethod);            

            for (eps = 0.0; eps <= 0.99; eps += 0.01)
                ErrorImpurityTestIntPerReduct(
                    data, test, pruningType, reductFactoryKey, 
                    eps, rednum, emptyDistribution.Output, attributeSelectionMethod);            
        }

        [Test, Repeat(1)]                                
        [TestCase(@"Data\soybean-small.2.data", FileFormat.Rses1, PruningType.ReducedErrorPruning, ReductFactoryKeyHelper.ApproximateReductMajorityWeights, 5)]        
        [TestCase(@"Data\agaricus-lepiota.2.data", FileFormat.Rses1, PruningType.ReducedErrorPruning, ReductFactoryKeyHelper.ApproximateReductMajorityWeights, 5)]
        [TestCase(@"Data\breast-cancer-wisconsin.2.data", FileFormat.Rses1, PruningType.ReducedErrorPruning, ReductFactoryKeyHelper.ApproximateReductMajorityWeights, 5)]
        [TestCase(@"Data\promoters.2.data", FileFormat.Rses1, PruningType.ReducedErrorPruning, ReductFactoryKeyHelper.ApproximateReductMajorityWeights, 5)]
        [TestCase(@"Data\semeion.data", FileFormat.Rses1, PruningType.ReducedErrorPruning, ReductFactoryKeyHelper.ApproximateReductMajorityWeights, 5)]
        [TestCase(@"Data\nursery.2.data", FileFormat.Rses1, PruningType.ReducedErrorPruning, ReductFactoryKeyHelper.ApproximateReductMajorityWeights, 5)]
        [TestCase(@"Data\chess.data", FileFormat.Rses1, PruningType.None, ReductFactoryKeyHelper.ApproximateReductMajorityWeights, 5)]
        [TestCase(@"Data\chess.data", FileFormat.Rses1, PruningType.ReducedErrorPruning, ReductFactoryKeyHelper.ApproximateReductMajorityWeights, 5)]
        [TestCase(@"Data\zoo.dta", FileFormat.Rses1, PruningType.None, ReductFactoryKeyHelper.ApproximateReductMajorityWeights, 5)]
        [TestCase(@"Data\zoo.dta", FileFormat.Rses1, PruningType.ReducedErrorPruning, ReductFactoryKeyHelper.ApproximateReductMajorityWeights, 5)]
        [TestCase(@"Data\soybean-small.2.data", FileFormat.Rses1, PruningType.None, ReductFactoryKeyHelper.ApproximateReductMajorityWeights, 5)]        
        [TestCase(@"Data\house-votes-84.2.data", FileFormat.Rses1_1, PruningType.None, ReductFactoryKeyHelper.ApproximateReductMajorityWeights, 5)]
        [TestCase(@"Data\house-votes-84.2.data", FileFormat.Rses1_1, PruningType.ReducedErrorPruning, ReductFactoryKeyHelper.ApproximateReductMajorityWeights, 5)]
        [TestCase(@"Data\agaricus-lepiota.2.data", FileFormat.Rses1, PruningType.None, ReductFactoryKeyHelper.ApproximateReductMajorityWeights, 5)]        
        [TestCase(@"Data\breast-cancer-wisconsin.2.data", FileFormat.Rses1, PruningType.None, ReductFactoryKeyHelper.ApproximateReductMajorityWeights, 5)]        
        [TestCase(@"Data\promoters.2.data", FileFormat.Rses1, PruningType.None, ReductFactoryKeyHelper.ApproximateReductMajorityWeights, 5)]
        [TestCase(@"Data\semeion.data", FileFormat.Rses1, PruningType.None, ReductFactoryKeyHelper.ApproximateReductMajorityWeights, 5)]
        [TestCase(@"Data\nursery.2.data", FileFormat.Rses1, PruningType.None, ReductFactoryKeyHelper.ApproximateReductMajorityWeights, 5)]
        [TestCase(@"Data\vehicle.tab", FileFormat.Rses1, PruningType.ReducedErrorPruning, ReductFactoryKeyHelper.ApproximateReductMajorityWeights, 5)]
        [TestCase(@"Data\vehicle.tab", FileFormat.Rses1, PruningType.None, ReductFactoryKeyHelper.ApproximateReductMajorityWeights, 5)]
        [TestCase(@"Data\german.data", FileFormat.Csv, PruningType.ReducedErrorPruning, ReductFactoryKeyHelper.ApproximateReductMajorityWeights, 5)]
        [TestCase(@"Data\german.data", FileFormat.Csv, PruningType.None, ReductFactoryKeyHelper.ApproximateReductMajorityWeights, 5)]
        [TestCase(@"Data\dermatology_modified.data", FileFormat.Csv, PruningType.ReducedErrorPruning, ReductFactoryKeyHelper.ApproximateReductMajorityWeights, 5)]
        [TestCase(@"Data\dermatology_modified.data", FileFormat.Csv, PruningType.None, ReductFactoryKeyHelper.ApproximateReductMajorityWeights, 5)]
        [TestCase(@"Data\dermatology.data", FileFormat.Csv, PruningType.ReducedErrorPruning, ReductFactoryKeyHelper.ApproximateReductMajorityWeights, 5)]
        [TestCase(@"Data\dermatology.data", FileFormat.Csv, PruningType.None, ReductFactoryKeyHelper.ApproximateReductMajorityWeights, 5)]
        [TestCase(@"Data\hypothyroid.data", FileFormat.Csv, PruningType.ReducedErrorPruning, ReductFactoryKeyHelper.ApproximateReductMajorityWeights, 5)]
        [TestCase(@"Data\hypothyroid.data", FileFormat.Csv, PruningType.None, ReductFactoryKeyHelper.ApproximateReductMajorityWeights, 5)]       
        [TestCase(@"Data\lymphography.all", FileFormat.Csv, PruningType.ReducedErrorPruning, ReductFactoryKeyHelper.ApproximateReductMajorityWeights, 5)]
        [TestCase(@"Data\lymphography.all", FileFormat.Csv, PruningType.None, ReductFactoryKeyHelper.ApproximateReductMajorityWeights, 5)]
        public void ErrorImpurityTest_CV(string dataFile, FileFormat fileFormat, PruningType pruningType, string reductFactoryKey, int folds)
        {
            DataStore data = DataStore.Load(dataFile, fileFormat);

            if (dataFile != @"Data\vehicle.tab"
                && dataFile != @"Data\german.data"
                && dataFile != @"Data\hypothyroid.data")
            {
                if (dataFile == @"Data\dermatology.data")
                {
                    foreach (var fieldInfo in data.DataStoreInfo.Fields)
                    {
                        if (fieldInfo.Id != 34) //Age Attribute
                            fieldInfo.IsNumeric = false;
                    }
                }
                else
                {
                    foreach (var fieldInfo in data.DataStoreInfo.Fields)
                        fieldInfo.IsNumeric = false;
                }
            }
            
            int[] allAttributes = data.DataStoreInfo.GetFieldIds(FieldTypes.Standard).ToArray();
            var emptyDistribution = EquivalenceClassCollection
                .Create(new int[] { }, data, data.Weights)
                .DecisionDistribution;            

            DataStoreSplitter splitter = new DataStoreSplitter(data, folds, true);            

            int rednum = 100;
            PermutationGenerator permutationGenerator = new PermutationGenerator(allAttributes);
            PermutationCollection permutationCollection = permutationGenerator.Generate(rednum);

            var localReductCache = new Dictionary<string, Tuple<int[], DataStore>>(15);
            object cacheLock = new object();

            double eps = 0.0;

            Func<IModel, int[], DataStore, Tuple<int[], DataStore>> calculateReduct 
                = delegate (IModel model, int[] attr, DataStore trainingSet)
            {
                Tuple<int[], DataStore> best = null;
                if (localReductCache.TryGetValue(GetKey(trainingSet, eps, pruningType), out best))
                    return best;

                lock (cacheLock)
                {
                    best = null;
                    if (localReductCache.TryGetValue(GetKey(trainingSet, eps, pruningType), out best))
                        return best;

                    if (trainingSet.DataStoreInfo.GetFields(FieldTypes.Standard).Any(f => f.CanDiscretize()))
                    {
                        var descretizer = new DataStoreDiscretizer()
                        {
                            UseBetterEncoding = false,
                            UseKononenko = false,
                            Fields2Discretize = trainingSet.DataStoreInfo.GetFields(FieldTypes.Standard)
                                        .Where(f => f.CanDiscretize())
                                        .Select(fld => fld.Id)
                        };

                        descretizer.Discretize(trainingSet, trainingSet.Weights);
                    }

                    Args parms = new Args(4);
                    parms.SetParameter(ReductGeneratorParamHelper.TrainData, trainingSet);
                    parms.SetParameter(ReductGeneratorParamHelper.FactoryKey, reductFactoryKey);
                    parms.SetParameter(ReductGeneratorParamHelper.Epsilon, eps);
                    parms.SetParameter(ReductGeneratorParamHelper.PermutationCollection, permutationCollection);
                    IReductGenerator generator = ReductFactory.GetReductGenerator(parms);
                    generator.Run();

                    var reducts = generator.GetReducts();
                    reducts.Sort(ReductRuleNumberComparer.Default);
                    IReduct bestReduct = reducts.FirstOrDefault();

                    best = new Tuple<int[], DataStore>(bestReduct.Attributes.ToArray(), trainingSet);

                    localReductCache.Add(GetKey(trainingSet, eps, pruningType), best);
                }

                return best;
            };            

            AttributeAndDataSelectionMethod attributeSelectionMethod = (m, a, d) => calculateReduct(m, a, d);

            ErrorImpurityTestIntPerReduct_CV(data, splitter, pruningType, reductFactoryKey,
                    -1.0, emptyDistribution.Output, attributeSelectionMethod);

            for (eps = 0.0; eps <= 0.99; eps += 0.01)
                ErrorImpurityTestIntPerReduct_CV(data, splitter, pruningType, reductFactoryKey,
                    eps, emptyDistribution.Output, attributeSelectionMethod);
        }        

        private void ErrorImpurityTestIntPerReduct_CV(
            DataStore data, DataStoreSplitter splitter, 
            PruningType pruningType, string redkey, 
            double eps, long output,
            AttributeAndDataSelectionMethod attributeSelectionMethod)
        {                        
            if (pruningType == PruningType.None)
            {
                DecisionTableMajority decTabMaj = new DecisionTableMajority("DecTabMaj-" + pruningType.ToSymbol());
                decTabMaj.DefaultOutput = output;
                CrossValidation<DecisionTableMajority> decTabMajCV = new CrossValidation<DecisionTableMajority>(decTabMaj);               
                var decTabMajResult = decTabMajCV.Run(data, splitter);
                decTabMajResult.Epsilon = eps;
                Console.WriteLine(decTabMajResult);
            }

            DecisionTreeRough treeRough = new DecisionTreeRough("Rough-Majority-" + pruningType.ToSymbol());
            treeRough.PreLearn = attributeSelectionMethod;            
            treeRough.DefaultOutput = output;
            treeRough.PruningType = pruningType;
            CrossValidation<DecisionTreeRough> treeRoughCV = new CrossValidation<DecisionTreeRough>(treeRough);            
            var treeRoughResult = treeRoughCV.Run(data, splitter);
            treeRoughResult.Epsilon = eps;
            Console.WriteLine(treeRoughResult);

            DecisionTreeC45 treec45 = new DecisionTreeC45("C45-Entropy-" + pruningType.ToSymbol());
            treec45.PreLearn = attributeSelectionMethod;            
            treec45.DefaultOutput = output;
            treec45.PruningType = pruningType;
            CrossValidation<DecisionTreeC45> treec45CV = new CrossValidation<DecisionTreeC45>(treec45);            
            var treec45Result = treec45CV.Run(data, splitter);
            treec45Result.Epsilon = eps;
            Console.WriteLine(treec45Result);
            
            ObliviousDecisionTree treeOblivEntropy = new ObliviousDecisionTree("Olv-Entropy-" + pruningType.ToSymbol());
            treeOblivEntropy.PreLearn = attributeSelectionMethod;            
            treeOblivEntropy.ImpurityFunction = ImpurityFunctions.Entropy;
            treeOblivEntropy.DefaultOutput = output;
            treeOblivEntropy.PruningType = pruningType;
            CrossValidation<ObliviousDecisionTree> treeOblivEntropyCV = new CrossValidation<ObliviousDecisionTree>(treeOblivEntropy);            
            var treeOblivEntropyResult = treeOblivEntropyCV.Run(data, splitter);
            treeOblivEntropyResult.Epsilon = eps;
            Console.WriteLine(treeOblivEntropyResult);
        }

        private void ErrorImpurityTestIntPerReduct(
            DataStore trainDS, DataStore testDS, PruningType pruningType, 
            string redkey, double eps, int rednum, long output,
            AttributeAndDataSelectionMethod attributeSelectionMethod)
        {                        
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
            treeRough.PreLearn = attributeSelectionMethod;
            treeRough.DefaultOutput = output;
            treeRough.PruningType = pruningType;
            treeRough.Learn(trainDS, attributes);
            var treeRoughResult = Classifier.DefaultClassifer.Classify(treeRough, testDS);
            treeRoughResult.Epsilon = eps;
            Console.WriteLine(treeRoughResult);

            DecisionTreeC45 treec45 = new DecisionTreeC45("C45-Entropy-" + pruningType.ToSymbol());
            treec45.PreLearn = attributeSelectionMethod;
            treec45.DefaultOutput = output;
            treec45.PruningType = pruningType;
            treec45.Learn(trainDS, attributes);
            var treec45Result = Classifier.DefaultClassifer.Classify(treec45, testDS);
            treec45Result.Epsilon = eps;
            Console.WriteLine(treec45Result);

            ObliviousDecisionTree treeOblivEntropy = new ObliviousDecisionTree("Olv-Entropy-" + pruningType.ToSymbol());
            treeOblivEntropy.PreLearn = attributeSelectionMethod;
            treeOblivEntropy.ImpurityFunction = ImpurityFunctions.Entropy;
            treeOblivEntropy.DefaultOutput = output;
            treeOblivEntropy.PruningType = pruningType;
            treeOblivEntropy.Learn(trainDS, attributes);
            var treeOblivEntropyResult = Classifier.DefaultClassifer.Classify(treeOblivEntropy, testDS);
            treeOblivEntropyResult.Epsilon = eps;
            Console.WriteLine(treeOblivEntropyResult);
        }
        
        private string GetKey(DataStore data, double epsilon, PruningType pruningType)
        {
            return String.Format("{0}#{1}#{2}", data.Name, epsilon, pruningType.ToSymbol());
        }

        [TestCase(@"Data\vehicle.tab", FileFormat.Rses1, PruningType.None, ReductFactoryKeyHelper.ApproximateReductMajorityWeights, 5)]
        [TestCase(@"Data\vehicle.tab", FileFormat.Rses1, PruningType.ReducedErrorPruning, ReductFactoryKeyHelper.ApproximateReductMajorityWeights, 5)]
        public void Discretize_CV_Test(string dataFile, FileFormat fileFormat, PruningType pruningType, string reductFactoryKey, int folds)
        {            
            DataStore data = DataStore.Load(dataFile, fileFormat);
            
            int[] allAttributes = data.DataStoreInfo.GetFieldIds(FieldTypes.Standard).ToArray();
            EquivalenceClassCollection emptyClassCollection = EquivalenceClassCollection.Create(new int[] { }, data, data.Weights);
            DecisionDistribution emptyDistribution = emptyClassCollection.DecisionDistribution;
            long output = emptyDistribution.Output;

            DataStoreSplitter splitter = new DataStoreSplitter(data, folds, true);
            splitter.PostSplitMethod = (trn, tst) =>
            {
                var descretizer = new DataStoreDiscretizer()
                {
                    UseBetterEncoding = false,
                    UseKononenko = false, //use FayadAndIraniMDL
                    Fields2Discretize = trn.DataStoreInfo.GetFieldIds(FieldTypes.Standard)
                        .Where(fieldId => tst.DataStoreInfo.GetFieldInfo(fieldId).IsNumeric)
                };

                descretizer.Discretize(trn, trn.Weights);
                descretizer.Discretize(tst, trn);
            };

            DecisionTreeC45 treec45 = new DecisionTreeC45();
            treec45.PruningType = pruningType;
            treec45.DefaultOutput = output;

            CrossValidation<DecisionTreeC45> cv = new CrossValidation<DecisionTreeC45>(treec45);            
            var result = cv.Run(data, splitter);

            Console.WriteLine(result);                                               
        }

        [Test, Repeat(25)]        
        [TestCase(@"Data\house-votes-84.2.data", FileFormat.Rses1_1, PruningType.None, ReductFactoryKeyHelper.ApproximateReductMajorityWeights, 5)]        
        public void TestHouse(string dataFile, FileFormat fileFormat, PruningType pruningType, string reductFactoryKey, int folds)
        {
            DataStore data = DataStore.Load(dataFile, fileFormat);
            int[] allAttributes = data.DataStoreInfo.GetFieldIds(FieldTypes.Standard).ToArray();
            EquivalenceClassCollection emptyClassCollection = EquivalenceClassCollection.Create(new int[] { }, data, data.Weights);
            DecisionDistribution emptyDistribution = emptyClassCollection.DecisionDistribution;

            DataStoreSplitter splitter = new DataStoreSplitter(data, folds, true);

            //Console.WriteLine(ClassificationResult.TableHeader());

            int rednum = 100;
            IReduct reductAllAttributes = new ReductWeights(data, allAttributes, 0, data.Weights);

            PermutationGenerator permutationGenerator = new PermutationGenerator(
                        data.DataStoreInfo.GetFields(FieldTypes.Standard)
                        .Select(f2 => f2.Id)
                        .ToArray());

            PermutationCollection permutationCollection = permutationGenerator.Generate(rednum);

            //foreach (var perm in permutationCollection)
            //    Console.WriteLine(perm);

            for (double eps = 0.00; eps <= 0.99; eps += 0.01)
            {               
                Dictionary<int, int[]> attributes = new Dictionary<int, int[]>(folds);
                for (int f = 0; f < folds; f++)
                {
                    DataStore trainDS = null, testDS = null;
                    splitter.Split(ref trainDS, ref testDS, f);                    

                    Args parms = new Args(5);
                    parms.SetParameter(ReductGeneratorParamHelper.TrainData, trainDS);
                    parms.SetParameter(ReductGeneratorParamHelper.FactoryKey, reductFactoryKey);
                    parms.SetParameter(ReductGeneratorParamHelper.Epsilon, eps);
                    parms.SetParameter(ReductGeneratorParamHelper.NumberOfReducts, rednum);
                    parms.SetParameter(ReductGeneratorParamHelper.PermutationCollection, permutationCollection);

                    IReductGenerator generator = ReductFactory.GetReductGenerator(parms);
                    generator.Run();

                    var reducts = generator.GetReducts();

                    //foreach (var red in reducts)
                    //    Console.WriteLine("{0} {1}", f, red);
                    
                    reducts.Sort(ReductAccuracyComparer.Default);
                    IReduct bestReduct = reducts.FirstOrDefault();
                    attributes.Add(f, bestReduct != null ? bestReduct.Attributes.ToArray() : new int[] { });

                    //Console.WriteLine("{0} {1}", f, bestReduct);
                }                
                
                DecisionTableMajority decTabMaj = new DecisionTableMajority("DecTabMaj-" + pruningType.ToSymbol());
                CrossValidation<DecisionTableMajority> decTabMajCV = new CrossValidation<DecisionTableMajority>(decTabMaj);
                decTabMajCV.Attributes = attributes;
                var decTabMajResult = decTabMajCV.Run(data, splitter);
                decTabMajResult.Epsilon = eps;
                Console.WriteLine(decTabMajResult);

                DecisionTreeC45 treec45 = new DecisionTreeC45("C45-Entropy-" + pruningType.ToSymbol());                                
                treec45.PruningType = pruningType;

                CrossValidation<DecisionTreeC45> treec45CV = new CrossValidation<DecisionTreeC45>(treec45);
                treec45CV.Attributes = attributes;
                //treec45CV.PostLearningMethod = (tree) => Console.WriteLine(DecisionTreeFormatter.Construct((IDecisionTree)tree));
                var treec45Result = treec45CV.Run(data, splitter);                
                treec45Result.Epsilon = eps;
                Console.WriteLine(treec45Result);

                Holte1R holte = new Holte1R();                
                CrossValidation<Holte1R> holteCV = new CrossValidation<Holte1R>(holte);
                //holteCV.Attributes = attributes;
                var holteResult = holteCV.Run(data, splitter);
                holteResult.Epsilon = eps;
                Console.WriteLine(holteResult);

            }                                  
        }
    }
}
