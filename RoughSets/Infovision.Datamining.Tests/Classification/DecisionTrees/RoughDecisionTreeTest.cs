﻿using NUnit.Framework;
using Raccoon.Core;
using Raccoon.Data; 
using Raccoon.MachineLearning.Classification;
using Raccoon.MachineLearning.Classification.DecisionTables;
using Raccoon.MachineLearning.Classification.DecisionTrees;
using Raccoon.MachineLearning.Classification.DecisionTrees.Pruning;
using Raccoon.MachineLearning.Discretization;
using Raccoon.MachineLearning.Permutations;
using Raccoon.MachineLearning.Roughset;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Raccoon.MachineLearning.Tests.Classification.DecisionTrees
{
    [TestFixture]
    public class RoughDecisionTreeTest
    {
        [TestCase(@"Data\german.data", FileFormat.Csv, ReductFactoryKeyHelper.ApproximateReductMajorityWeights, 5)]
        public void DecisionTreeWithNewDiscretization(string dataFile, FileFormat fileFormat, string reductFactoryKey, int folds)
        {
            DataStore data = DataStore.Load(dataFile, fileFormat);            

            int[] allAttributes = data.DataStoreInfo.GetFieldIds(FieldGroup.Standard).ToArray();
            var emptyDistribution = EquivalenceClassCollection
                .Create(new int[] { }, data, data.Weights)
                .DecisionDistribution;

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
                            discretizer.AddColumnsBasedOnCuts = true;
                            discretizer.RemoveColumnAfterDiscretization = true;
                            discretizer.UseBinaryCuts = false;

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
                        parms.SetParameter(ReductGeneratorParamHelper.TrainData, trainingSet);
                        parms.SetParameter(ReductGeneratorParamHelper.FactoryKey, reductFactoryKey);
                        parms.SetParameter(ReductGeneratorParamHelper.Epsilon, eps);
                        parms.SetParameter(ReductGeneratorParamHelper.PermutationCollection, permutationCollection);
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

            CrossValidation cv = new CrossValidation(data, folds);

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

        private string GetCachedReductKey(DataStore data, double epsilon)
        {
            return String.Format("{0}#{1}", data.Name, epsilon);
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
        }
    }
}
