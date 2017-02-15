using Raccoon.Data;
using Raccoon.MachineLearning.Classification.DecisionTables;
using Raccoon.MachineLearning.Classification.DecisionTrees;
using Raccoon.MachineLearning.Classification.DecisionTrees.Pruning;
using Raccoon.Core;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using Raccoon.MachineLearning.Roughset;
using Raccoon.MachineLearning.Classification;
using Raccoon.MachineLearning.Permutations;
using Raccoon.MachineLearning.Discretization;
using Raccoon.MachineLearning.Evaluation;

namespace Raccoon.MachineLearning.Tests.Classification.DecisionTrees
{
    [TestFixture]
    public class DecisionTreeReductCompare
    {
        [Test, Repeat(1)]                
        [TestCase(@"Data\monks-1.train", @"Data\monks-1.test", FileFormat.Rses1, ReductTypes.ApproximateReductMajorityWeights)]
        [TestCase(@"Data\monks-2.train", @"Data\monks-2.test", FileFormat.Rses1, ReductTypes.ApproximateReductMajorityWeights)]
        [TestCase(@"Data\monks-3.train", @"Data\monks-3.test", FileFormat.Rses1, ReductTypes.ApproximateReductMajorityWeights)]
        [TestCase(@"Data\dna_modified.trn", @"Data\dna_modified.tst", FileFormat.Rses1, ReductTypes.ApproximateReductMajorityWeights)]
        [TestCase(@"Data\spect.train", @"Data\spect.test", FileFormat.Rses1, ReductTypes.ApproximateReductMajorityWeights)]
        //[TestCase(@"Data\dna.train", @"Data\dna.test", FileFormat.Rses1, ReductFactoryKeyHelper.ApproximateReductMajorityWeights)]
        [TestCase(@"Data\audiology.standardized.2.data", @"Data\audiology.standardized.2.test", FileFormat.Rses1, ReductTypes.ApproximateReductMajorityWeights)]
        [TestCase(@"Data\soybean-large.data", @"Data\soybean-large.test", FileFormat.Rses1, ReductTypes.ApproximateReductMajorityWeights)]
        [TestCase(@"Data\sat.disc.trn", @"Data\sat.disc.tst", FileFormat.Rses1, ReductTypes.ApproximateReductMajorityWeights)]
        //[TestCase(@"Data\pendigits.disc.trn", @"Data\pendigits.disc.tst", FileFormat.Rses1, ReductFactoryKeyHelper.ApproximateReductMajorityWeights)]
        //[TestCase(@"Data\optdigits.disc.trn", @"Data\optdigits.disc.tst", FileFormat.Rses1, ReductFactoryKeyHelper.ApproximateReductMajorityWeights)]
        //[TestCase(@"Data\letter.disc.trn", @"Data\letter.disc.tst", FileFormat.Rses1, ReductFactoryKeyHelper.ApproximateReductMajorityWeights)]
        [TestCase(@"Data\vowel.disc.trn", @"Data\vowel.disc.tst", FileFormat.Csv, ReductTypes.ApproximateReductMajorityWeights)]
        public void ErrorImpurityTest(string trainFile, string testFile, FileFormat fileFormat, string reductFactoryKey)
        {
            DataStore data = DataStore.Load(trainFile, fileFormat);
            foreach (var fieldInfo in data.DataStoreInfo.Fields) fieldInfo.IsNumeric = false;
            DataStore test = DataStore.Load(testFile, fileFormat, data.DataStoreInfo);

            int[] allAttributes = data.DataStoreInfo.GetFieldIds(FieldGroup.Standard).ToArray();

            EquivalenceClassCollection emptyClassCollection = EquivalenceClassCollection.Create(new int[] { }, data, data.Weights);
            DecisionDistribution emptyDistribution = emptyClassCollection.DecisionDistribution;

            int rednum = 100;
            PermutationGenerator permutationGenerator = new PermutationGenerator(allAttributes);
            PermutationCollection permutationCollection = permutationGenerator.Generate(rednum);
            
            object cacheLock = new object();
            var localDataFoldCache = new Dictionary<string, DataStore>(6);
            var localReductCache = new Dictionary<string, int[]>(300);

            double eps = -1.0;

            Func<IModel, int[], DataStore, DataStore> trainingDataSubmit
                = delegate (IModel model, int[] attr, DataStore trainingSet)
                {
                    DataStore cachedData = null;
                    if (localDataFoldCache.TryGetValue(trainingSet.Name, out cachedData))
                        return cachedData;

                    lock (cacheLock)
                    {
                        cachedData = null;
                        if (localDataFoldCache.TryGetValue(trainingSet.Name, out cachedData))
                            return cachedData;

                        if (trainingSet.DataStoreInfo.GetFields(FieldGroup.Standard).Any(f => f.CanDiscretize()))
                        {
                            var discretizer = new DataStoreDiscretizer();
                            discretizer.Fields2Discretize = trainingSet.DataStoreInfo.GetFields(FieldGroup.Standard)
                                            .Where(f => f.CanDiscretize())
                                            .Select(fld => fld.Id);                            
                            discretizer.Discretize(trainingSet, trainingSet.Weights);
                        }

                        localDataFoldCache.Add(trainingSet.Name, trainingSet);
                        return trainingSet;
                    }                    
                };

            Func<IModel, int[], DataStore, DataStore> validationDataSubmit
                = delegate (IModel model, int[] attr, DataStore validationSet)
                {
                    string cacheKey = validationSet.Name;
                    DataStore cachedData = null;
                    if (localDataFoldCache.TryGetValue(cacheKey, out cachedData))
                        return cachedData;

                    lock (cacheLock)
                    {
                        cachedData = null;
                        if (localDataFoldCache.TryGetValue(cacheKey, out cachedData))
                            return cachedData;                        

                        if (validationSet.DataStoreInfo.GetFields(FieldGroup.Standard).Any(f => f.CanDiscretize()))
                        {
                            string trainingCacheKey = cacheKey.Replace("-TST-", "-TRN-");
                            cachedData = null;
                            if (!localDataFoldCache.TryGetValue(trainingCacheKey, out cachedData))
                                throw new InvalidOperationException(String.Format("{0} not found", trainingCacheKey));
                                                        
                            DataStoreDiscretizer.Discretize(validationSet, cachedData);
                        }

                        localDataFoldCache.Add(cacheKey, validationSet);
                        return validationSet;
                    }
                };

            Func<IModel, int[], DataStore, int[]> inputAttributeSubmit
                = delegate (IModel model, int[] attr, DataStore trainingSet)
                {
                    int[] cachedReduct = null;
                    if (localReductCache.TryGetValue(GetCachedReductKey(trainingSet, eps), out cachedReduct))
                        return cachedReduct;

                    lock (cacheLock)
                    {
                        cachedReduct = null;
                        if (localReductCache.TryGetValue(GetCachedReductKey(trainingSet, eps), out cachedReduct))
                            return cachedReduct;

                        Args parms = new Args(4);
                        parms.SetParameter(ReductFactoryOptions.DecisionTable, trainingSet);
                        parms.SetParameter(ReductFactoryOptions.ReductType, reductFactoryKey);
                        parms.SetParameter(ReductFactoryOptions.Epsilon, eps);
                        parms.SetParameter(ReductFactoryOptions.PermutationCollection, permutationCollection);
                        IReductGenerator generator = ReductFactory.GetReductGenerator(parms);
                        generator.Run();

                        var reducts = generator.GetReducts();
                        reducts.Sort(ReductRuleNumberComparer.Default);
                        IReduct bestReduct = reducts.FirstOrDefault();

                        localReductCache.Add(GetCachedReductKey(trainingSet, eps), bestReduct.Attributes.ToArray());
                        return bestReduct.Attributes.ToArray();
                    }                    
                };

            OnTrainingDataSubmission trainingSubmission = (m, a, d) => trainingDataSubmit(m, a, d);
            OnInputAttributeSubmission inputAttributesSubmission = (m, a, d) => inputAttributeSubmit(m, a, d);
            OnValidationDataSubmission validationSubmission = (m, a, d) => validationDataSubmit(m, a, d);

            eps = -1.0;
            ErrorImpurityTestIntPerReduct(data, test, PruningType.None, reductFactoryKey, 
                eps, rednum, emptyDistribution.Output, trainingSubmission, inputAttributesSubmission, validationSubmission);

            ErrorImpurityTestIntPerReduct(data, test, PruningType.ReducedErrorPruning, reductFactoryKey,
                eps, rednum, emptyDistribution.Output, trainingSubmission, inputAttributesSubmission, validationSubmission);

            for (eps = 0.0; eps <= 0.99; eps += 0.01)
            {
                ErrorImpurityTestIntPerReduct(data, test, PruningType.None, reductFactoryKey,
                    eps, rednum, emptyDistribution.Output, trainingSubmission, inputAttributesSubmission, validationSubmission);

                ErrorImpurityTestIntPerReduct(data, test, PruningType.ReducedErrorPruning, reductFactoryKey,
                    eps, rednum, emptyDistribution.Output, trainingSubmission, inputAttributesSubmission, validationSubmission);
            }
        }

        [Test, Repeat(1)]
        [TestCase(@"Data\chess.data", FileFormat.Rses1, ReductTypes.ApproximateReductMajorityWeights, 5)]
        [TestCase(@"Data\zoo.dta", FileFormat.Rses1, ReductTypes.ApproximateReductMajorityWeights, 5)]
        [TestCase(@"Data\soybean-small.2.data", FileFormat.Rses1, ReductTypes.ApproximateReductMajorityWeights, 5)]
        [TestCase(@"Data\house-votes-84.2.data", FileFormat.Rses1_1, ReductTypes.ApproximateReductMajorityWeights, 5)]
        [TestCase(@"Data\agaricus-lepiota.2.data", FileFormat.Rses1, ReductTypes.ApproximateReductMajorityWeights, 5)]
        [TestCase(@"Data\breast-cancer-wisconsin.2.data", FileFormat.Rses1, ReductTypes.ApproximateReductMajorityWeights, 5)]
        [TestCase(@"Data\promoters.2.data", FileFormat.Rses1, ReductTypes.ApproximateReductMajorityWeights, 5)]
        [TestCase(@"Data\semeion.data", FileFormat.Rses1, ReductTypes.ApproximateReductMajorityWeights, 5)]
        //[TestCase(@"Data\nursery.2.data", FileFormat.Rses1, ReductFactoryKeyHelper.ApproximateReductMajorityWeights, 5)]
        [TestCase(@"Data\vehicle.tab", FileFormat.Rses1, ReductTypes.ApproximateReductMajorityWeights, 5)]
        [TestCase(@"Data\german.data", FileFormat.Csv, ReductTypes.ApproximateReductMajorityWeights, 5)]
        [TestCase(@"Data\dermatology_modified.data", FileFormat.Csv, ReductTypes.ApproximateReductMajorityWeights, 5)]
        [TestCase(@"Data\dermatology.data", FileFormat.Csv, ReductTypes.ApproximateReductMajorityWeights, 5)]
        [TestCase(@"Data\hypothyroid.data", FileFormat.Csv, ReductTypes.ApproximateReductMajorityWeights, 5)]
        [TestCase(@"Data\lymphography.all", FileFormat.Csv, ReductTypes.ApproximateReductMajorityWeights, 5)]
        public void ErrorImpurityTest_CV(string dataFile, FileFormat fileFormat, string reductFactoryKey, int folds)
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
            
            int[] allAttributes = data.DataStoreInfo.GetFieldIds(FieldGroup.Standard).ToArray();
            var emptyDistribution = EquivalenceClassCollection
                .Create(new int[] { }, data, data.Weights)
                .DecisionDistribution;            

            DataSplitter splitter = new DataSplitter(data, folds, true);

            int rednum = 100;
            PermutationGenerator permutationGenerator = new PermutationGenerator(allAttributes);
            PermutationCollection permutationCollection = permutationGenerator.Generate(rednum);

            var localDataFoldCache = new Dictionary<string, DataStore>(30);
            var localReductCache = new Dictionary<string, int[]>(1500);

            object cacheLock = new object();

            double eps = -1.0;

            Func<IModel, int[], DataStore, DataStore> trainingDataSubmit
                = delegate (IModel model, int[] attr, DataStore trainingSet)
            {
                DataStore cachedData = null;
                if (localDataFoldCache.TryGetValue(trainingSet.Name, out cachedData))
                    return cachedData;

                lock (cacheLock)
                {
                    cachedData = null;
                    if (localDataFoldCache.TryGetValue(trainingSet.Name, out cachedData))
                        return cachedData;

                    if (trainingSet.DataStoreInfo.GetFields(FieldGroup.Standard).Any(f => f.CanDiscretize()))
                    {
                        var discretizer = new DataStoreDiscretizer();
                        discretizer.Fields2Discretize = trainingSet.DataStoreInfo.GetFields(FieldGroup.Standard)
                                        .Where(f => f.CanDiscretize())
                                        .Select(fld => fld.Id);                        
                        discretizer.Discretize(trainingSet, trainingSet.Weights);
                    }

                    localDataFoldCache.Add(trainingSet.Name, trainingSet);
                    return trainingSet;
                }
            };

            Func<IModel, int[], DataStore, DataStore> validationDataSubmit
                = delegate (IModel model, int[] attr, DataStore validationSet)
                {
                    string cacheKey = validationSet.Name;
                    DataStore cachedData = null;
                    if (localDataFoldCache.TryGetValue(cacheKey, out cachedData))
                        return cachedData;

                    lock (cacheLock)
                    {
                        cachedData = null;
                        if (localDataFoldCache.TryGetValue(cacheKey, out cachedData))
                            return cachedData;
                       
                        if (validationSet.DataStoreInfo.GetFields(FieldGroup.Standard).Any(f => f.CanDiscretize()))
                        {
                            string trainingCacheKey = cacheKey.Replace("-TST-", "-TRN-");
                            cachedData = null;
                            if (!localDataFoldCache.TryGetValue(trainingCacheKey, out cachedData))
                                throw new InvalidOperationException(String.Format("{0} not found", trainingCacheKey));
                            
                            DataStoreDiscretizer.Discretize(validationSet, cachedData);                                                        
                        }

                        localDataFoldCache.Add(cacheKey, validationSet);
                        return validationSet;
                    }
                };

            Func<IModel, int[], DataStore, int[]> inputAttributeSubmit
                = delegate (IModel model, int[] attr, DataStore trainingSet)
            {
                int[] cachedReduct = null;
                if (localReductCache.TryGetValue(GetCachedReductKey(trainingSet, eps), out cachedReduct))
                    return cachedReduct;

                lock (cacheLock)
                {
                    cachedReduct = null;
                    if (localReductCache.TryGetValue(GetCachedReductKey(trainingSet, eps), out cachedReduct))
                        return cachedReduct;

                    if (eps < 0)
                    {
                        localReductCache.Add(GetCachedReductKey(trainingSet, eps), attr);
                        return attr;
                    }

                    Args parms = new Args(4);
                    parms.SetParameter(ReductFactoryOptions.DecisionTable, trainingSet);
                    parms.SetParameter(ReductFactoryOptions.ReductType, reductFactoryKey);
                    parms.SetParameter(ReductFactoryOptions.Epsilon, eps);
                    parms.SetParameter(ReductFactoryOptions.PermutationCollection, permutationCollection);
                    IReductGenerator generator = ReductFactory.GetReductGenerator(parms);
                    generator.Run();

                    var reducts = generator.GetReducts();
                    reducts.Sort(ReductRuleNumberComparer.Default);
                    IReduct bestReduct = reducts.FirstOrDefault();

                    localReductCache.Add(GetCachedReductKey(trainingSet, eps), bestReduct.Attributes.ToArray());
                    return bestReduct.Attributes.ToArray();
                }                
            };            

            OnTrainingDataSubmission trainingSubmission = (m, a, d) => trainingDataSubmit(m, a, d);
            OnInputAttributeSubmission inputAttributesSubmission = (m, a, d) => inputAttributeSubmit(m, a, d);
            OnValidationDataSubmission validationSubmission = (m, a, d) => validationDataSubmit(m, a, d);

            var cv = new CrossValidation(data, folds);

            eps = -1.0;
            ErrorImpurityTestIntPerReduct_CV(cv, PruningType.None, reductFactoryKey,
                    eps, emptyDistribution.Output, trainingSubmission, inputAttributesSubmission, validationSubmission);

            ErrorImpurityTestIntPerReduct_CV(cv, PruningType.ReducedErrorPruning, reductFactoryKey,
                    eps, emptyDistribution.Output, trainingSubmission, inputAttributesSubmission, validationSubmission);

            for (eps = 0.0; eps <= 0.99; eps += 0.01)
            {
                ErrorImpurityTestIntPerReduct_CV(cv, PruningType.None, reductFactoryKey,
                    eps, emptyDistribution.Output, trainingSubmission, inputAttributesSubmission, validationSubmission);

                ErrorImpurityTestIntPerReduct_CV(cv, PruningType.ReducedErrorPruning, reductFactoryKey,
                    eps, emptyDistribution.Output, trainingSubmission, inputAttributesSubmission, validationSubmission);
            }
        }        

        private void ErrorImpurityTestIntPerReduct_CV(
            CrossValidation cv, 
            PruningType pruningType, string redkey, 
            double eps, long output,
            OnTrainingDataSubmission onTrainingDataSubmission,
            OnInputAttributeSubmission onInputAttributeSubmission,
            OnValidationDataSubmission onValidationDataSubmission)
        {
            DecisionTreeRough treeRough = new DecisionTreeRough("Rough-Majority-" + pruningType.ToSymbol());
            treeRough.OnTrainingDataSubmission = onTrainingDataSubmission;
            treeRough.OnInputAttributeSubmission = onInputAttributeSubmission;
            treeRough.OnValidationDataSubmission = onValidationDataSubmission;
            treeRough.DefaultOutput = output;
            treeRough.PruningType = pruningType;
                        
            var treeRoughResult = cv.Run<DecisionTreeRough>(treeRough);
            treeRoughResult.Epsilon = eps;
            Console.WriteLine(treeRoughResult);

            DecisionTreeC45 treec45 = new DecisionTreeC45("C45-Entropy-" + pruningType.ToSymbol());
            treec45.OnTrainingDataSubmission = onTrainingDataSubmission;
            treec45.OnInputAttributeSubmission = onInputAttributeSubmission;
            treec45.OnValidationDataSubmission = onValidationDataSubmission;
            treec45.DefaultOutput = output;
            treec45.PruningType = pruningType;
            var treec45Result = cv.Run<DecisionTreeC45>(treec45);
            treec45Result.Epsilon = eps;
            Console.WriteLine(treec45Result);
            
            ObliviousDecisionTree treeOblivEntropy = new ObliviousDecisionTree("Olv-Entropy-" + pruningType.ToSymbol());
            treeOblivEntropy.OnTrainingDataSubmission = onTrainingDataSubmission;
            treeOblivEntropy.OnInputAttributeSubmission = onInputAttributeSubmission;
            treeOblivEntropy.OnValidationDataSubmission = onValidationDataSubmission;
            treeOblivEntropy.ImpurityFunction = ImpurityMeasure.Entropy;
            treeOblivEntropy.DefaultOutput = output;
            treeOblivEntropy.PruningType = pruningType;            
            var treeOblivEntropyResult = cv.Run<ObliviousDecisionTree>(treeOblivEntropy);
            treeOblivEntropyResult.Epsilon = eps;
            Console.WriteLine(treeOblivEntropyResult);

            if (pruningType == PruningType.None)
            {
                DecisionTableMajority decTabMaj = new DecisionTableMajority("DecTabMaj-" + pruningType.ToSymbol());
                decTabMaj.OnTrainingDataSubmission = onTrainingDataSubmission;
                decTabMaj.OnInputAttributeSubmission = onInputAttributeSubmission;
                decTabMaj.OnValidationDataSubmission = onValidationDataSubmission;
                decTabMaj.DefaultOutput = output;
                var decTabMajResult = cv.Run<DecisionTableMajority>(decTabMaj);
                decTabMajResult.Epsilon = eps;
                Console.WriteLine(decTabMajResult);
            }
        }

        private void ErrorImpurityTestIntPerReduct(
            DataStore trainDS, DataStore testDS, PruningType pruningType, 
            string redkey, double eps, int rednum, long output,
            OnTrainingDataSubmission onTrainingDataSubmission,
            OnInputAttributeSubmission onInputAttributeSubmission,
            OnValidationDataSubmission onValidationDataSubmission)
        {                        
            int[] attributes = trainDS.GetStandardFields();            

            DecisionTreeRough treeRough = new DecisionTreeRough("Rough-Majority-" + pruningType.ToSymbol());
            treeRough.OnTrainingDataSubmission = onTrainingDataSubmission;
            treeRough.OnInputAttributeSubmission = onInputAttributeSubmission;
            treeRough.OnValidationDataSubmission = onValidationDataSubmission;
            treeRough.DefaultOutput = output;
            treeRough.PruningType = pruningType;
            treeRough.Learn(trainDS, attributes);
            var treeRoughResult = Classifier.DefaultClassifer.Classify(treeRough, testDS);
            treeRoughResult.Epsilon = eps;
            Console.WriteLine(treeRoughResult);

            DecisionTreeC45 treec45 = new DecisionTreeC45("C45-Entropy-" + pruningType.ToSymbol());
            treec45.OnTrainingDataSubmission = onTrainingDataSubmission;
            treec45.OnInputAttributeSubmission = onInputAttributeSubmission;
            treec45.OnValidationDataSubmission = onValidationDataSubmission;
            treec45.DefaultOutput = output;
            treec45.PruningType = pruningType;
            treec45.Learn(trainDS, attributes);
            var treec45Result = Classifier.DefaultClassifer.Classify(treec45, testDS);
            treec45Result.Epsilon = eps;
            Console.WriteLine(treec45Result);

            ObliviousDecisionTree treeOblivEntropy = new ObliviousDecisionTree("Olv-Entropy-" + pruningType.ToSymbol());
            treeOblivEntropy.OnTrainingDataSubmission = onTrainingDataSubmission;
            treeOblivEntropy.OnInputAttributeSubmission = onInputAttributeSubmission;
            treeOblivEntropy.OnValidationDataSubmission = onValidationDataSubmission;
            treeOblivEntropy.ImpurityFunction = ImpurityMeasure.Entropy;
            treeOblivEntropy.DefaultOutput = output;
            treeOblivEntropy.PruningType = pruningType;
            treeOblivEntropy.Learn(trainDS, attributes);
            var treeOblivEntropyResult = Classifier.DefaultClassifer.Classify(treeOblivEntropy, testDS);
            treeOblivEntropyResult.Epsilon = eps;
            Console.WriteLine(treeOblivEntropyResult);

            if (pruningType == PruningType.None)
            {
                DecisionTableMajority decTabMaj = new DecisionTableMajority("DecTabMaj-" + pruningType.ToSymbol());
                decTabMaj.OnTrainingDataSubmission = onTrainingDataSubmission;
                decTabMaj.OnInputAttributeSubmission = onInputAttributeSubmission;
                decTabMaj.OnValidationDataSubmission = onValidationDataSubmission;
                decTabMaj.DefaultOutput = output;
                decTabMaj.Learn(trainDS, attributes);
                var decTabMajResult = Classifier.DefaultClassifer.Classify(decTabMaj, testDS);
                decTabMajResult.Epsilon = eps;
                Console.WriteLine(decTabMajResult);
            }
        }               

        private string GetCachedReductKey(DataStore data, double epsilon)
        {
            return String.Format("{0}#{1}", data.Name, epsilon);
        }

        [TestCase(@"Data\vehicle.tab", FileFormat.Rses1, PruningType.None, ReductTypes.ApproximateReductMajorityWeights, 5)]
        [TestCase(@"Data\vehicle.tab", FileFormat.Rses1, PruningType.ReducedErrorPruning, ReductTypes.ApproximateReductMajorityWeights, 5)]
        public void Discretize_CV_Test(string dataFile, FileFormat fileFormat, PruningType pruningType, int folds)
        {            
            DataStore data = DataStore.Load(dataFile, fileFormat);
            
            int[] allAttributes = data.DataStoreInfo.GetFieldIds(FieldGroup.Standard).ToArray();
            DecisionTreeC45 treec45 = new DecisionTreeC45();
            treec45.PruningType = pruningType;

            CrossValidation cv = new CrossValidation(data, folds);            
            var result = cv.Run<DecisionTreeC45>(treec45);

            Console.WriteLine(result);
        }
        
    }
}
