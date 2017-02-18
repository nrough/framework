using Raccoon.Data;
using Raccoon.MachineLearning.Classification.DecisionTrees;
using Raccoon.Core;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Raccoon.MachineLearning.Classification;
using Raccoon.MachineLearning.Roughset;
using Raccoon.MachineLearning.Permutations;
using Raccoon.MachineLearning.Weighting;

namespace Raccoon.MachineLearning.Tests.Classification.DecisionTrees
{
    [TestFixture]
    public class DecisionForestTest
    {
        [Test, Repeat(1)]
        public void DecisionForestForNumericAttributeTest()
        {
            int numOfFolds = 5;
            DataStore data = DataStore.Load(@"Data\german.data", FileFormat.CSV);
            DataStore train = null, test = null;

            DataSplitter splitter = new DataSplitter(data, numOfFolds);
            int[] attributes = data.DataStoreInfo.GetFieldIds(FieldGroup.Standard).ToArray();
            for (int f = 0; f < numOfFolds; f++)
            {                
                splitter.Split(out train, out test, f);

                DecisionForestRandom<DecisionTreeC45> forest = new DecisionForestRandom<DecisionTreeC45>();
                //forest.NumberOfAttributesToCheckForSplit = (int)System.Math.Floor(System.Math.Sqrt(attributes.Length));
                forest.Size = 50;
                forest.Learn(train, attributes);

                ClassificationResult result = Classifier.Default.Classify(forest, test);
                Console.WriteLine(result);
            }
        }

        [Test, Repeat(1)]
        public void DecisionForestRoughForNumericAttributeTest()
        {
            int numOfFolds = 5;
            DataStore data = DataStore.Load(@"Data\german.data", FileFormat.CSV);
            DataStore train = null, test = null;

            DataSplitter splitter = new DataSplitter(data, numOfFolds);
            for (int f = 0; f < numOfFolds; f++)
            {                
                splitter.Split(out train, out test, f);

                DecisionForestRandom<DecisionTreeRough> forest = new DecisionForestRandom<DecisionTreeRough>();
                //forest.NumberOfAttributesToCheckForSplit = 3;
                forest.Size = 50;
                forest.Gamma = 0.22;
                forest.Learn(train, train.DataStoreInfo.GetFieldIds(FieldGroup.Standard).ToArray());

                ClassificationResult result = Classifier.Default.Classify(forest, test);
                Console.WriteLine(result);
            }
        }

        [Test, Repeat(1)]
        public void DecisionForestRandomTest()
        {
            Console.WriteLine("RandomForestTest");
            DataStore data = DataStore.Load(@"Data\dna_modified.trn", FileFormat.RSES1);
            foreach (var fieldInfo in data.DataStoreInfo.Fields)
                fieldInfo.IsNumeric = false;
            DataStore test = DataStore.Load(@"Data\dna_modified.tst", FileFormat.RSES1, data.DataStoreInfo);

            //double epsilon = 0.07;

            DecisionForestRandom<DecisionTreeC45> randomForest = new DecisionForestRandom<DecisionTreeC45>();
            int[] attributes = data.DataStoreInfo.GetFieldIds(FieldGroup.Standard).ToArray();
            randomForest.Size = 100;
            //randomForest.NumberOfAttributesToCheckForSplit = (int) System.Math.Floor(System.Math.Sqrt(attributes.Length));
            randomForest.NumberOfTreeProbes = 1;
            randomForest.VoteType = DecisionForestVoteType.ErrorBased;
            //randomForest.Epsilon = epsilon;
            double error = randomForest.Learn(data, attributes).Error;
            Console.WriteLine(Classifier.Default.Classify(randomForest, test, null));

            DecisionTreeC45 c45tree = new DecisionTreeC45();
            //c45tree.Epsilon = epsilon;
            c45tree.Learn(data, attributes);
            Console.WriteLine(Classifier.Default.Classify(c45tree, test, null));
        }

        [Test, Repeat(1)]
        public void RandomForestCARTTest()
        {
            //double epsilon = 0.07;

            Console.WriteLine("RandomForestCARTTest");
            DataStore data = DataStore.Load(@"Data\dna_modified.trn", FileFormat.RSES1);
            foreach (var fieldInfo in data.DataStoreInfo.Fields)
                fieldInfo.IsNumeric = false;
            DataStore test = DataStore.Load(@"Data\dna_modified.tst", FileFormat.RSES1, data.DataStoreInfo);

            DecisionForestRandom<DecisionTreeCART> randomForest = new DecisionForestRandom<DecisionTreeCART>();
            int[] attributes = data.DataStoreInfo.GetFieldIds(FieldGroup.Standard).ToArray();
            randomForest.Size = 200;
            //randomForest.NumberOfAttributesToCheckForSplit = (int)System.Math.Floor(System.Math.Sqrt(attributes.Length));
            randomForest.NumberOfTreeProbes = 1;
            //randomForest.Epsilon = epsilon;
            double error = randomForest.Learn(data, attributes).Error;
            Console.WriteLine(Classifier.Default.Classify(randomForest, test, null));

            DecisionTreeCART cartTree = new DecisionTreeCART();
            cartTree.Learn(data, data.DataStoreInfo.GetFieldIds(FieldGroup.Standard).ToArray());
            Console.WriteLine(Classifier.Default.Classify(cartTree, test, null));
        }

        [Test]
        public void RandomForestRoughMTest()
        {
            double epsilon = 0.07;

            Console.WriteLine("RandomForestRoughTest");
            DataStore data = DataStore.Load(@"Data\dna_modified.trn", FileFormat.RSES1);
            foreach (var fieldInfo in data.DataStoreInfo.Fields)
                fieldInfo.IsNumeric = false;
            DataStore test = DataStore.Load(@"Data\dna_modified.tst", FileFormat.RSES1, data.DataStoreInfo);

            DecisionForestRandom<DecisionTreeRough> randomForest = new DecisionForestRandom<DecisionTreeRough>();
            randomForest.Size = 100;
            randomForest.NumberOfAttributesToCheckForSplit = 5;
            randomForest.NumberOfTreeProbes = 10;
            randomForest.Gamma = epsilon;
            double error = randomForest.Learn(data, data.DataStoreInfo.GetFieldIds(FieldGroup.Standard).ToArray()).Error;
            Console.WriteLine(Classifier.Default.Classify(randomForest, test, null));

            DecisionTreeRough roughTree = new DecisionTreeRough();
            roughTree.Learn(data, data.DataStoreInfo.GetFieldIds(FieldGroup.Standard).ToArray());
            Console.WriteLine(Classifier.Default.Classify(roughTree, test, null));
        }

        [Test]
        public void RoughForestRoughGammaTest()
        {
            double epsilon = 0.07;

            Console.WriteLine("RandomForestRoughGammaTest");
            DataStore data = DataStore.Load(@"Data\dna_modified.trn", FileFormat.RSES1);
            foreach (var fieldInfo in data.DataStoreInfo.Fields)
                fieldInfo.IsNumeric = false;
            DataStore test = DataStore.Load(@"Data\dna_modified.tst", FileFormat.RSES1, data.DataStoreInfo);

            DecisionForestReduct<DecisionTreeRough> randomForest = new DecisionForestReduct<DecisionTreeRough>();
            randomForest.Size = 10;
            randomForest.NumberOfTreeProbes = 10;
            randomForest.Gamma = epsilon;
            randomForest.ReductGeneratorFactory = ReductTypes.GeneralizedMajorityDecisionApproximate;

            double error = randomForest.Learn(data, data.DataStoreInfo.GetFieldIds(FieldGroup.Standard).ToArray()).Error;
            Console.WriteLine(Classifier.Default.Classify(randomForest, test, null));

            DecisionTreeRough roughTree = new DecisionTreeRough();
            roughTree.Gamma = epsilon;
            roughTree.Learn(data, data.DataStoreInfo.GetFieldIds(FieldGroup.Standard).ToArray());
            Console.WriteLine(Classifier.Default.Classify(roughTree, test, null));
        }

        [Test]
        public void RoughForestTest()
        {
            Console.WriteLine("RoughForestTest");

            double epsilon = 0.07;
            int numberOfAttributesToCheckForSplit = 5;
            int numberOfTreeProbes = 10;
            int size = 10;

            //DataStore data = DataStore.Load(@"Data\dna_modified.trn", FileFormat.Rses1);
            //DataStore test = DataStore.Load(@"Data\dna_modified.tst", FileFormat.Rses1, data.DataStoreInfo);

            DataStore data = DataStore.Load(@"Data\letter.trn", FileFormat.RSES1);
            DataStore test = DataStore.Load(@"Data\letter.tst", FileFormat.RSES1, data.DataStoreInfo);

            
            for (int i = 0; i < 1; i++)
            {
                DataSampler sampler = new DataSampler(data, true);

                DecisionForestDummy<DecisionTreeC45> dummyForest = new DecisionForestDummy<DecisionTreeC45>();
                dummyForest.DataSampler = sampler;
                dummyForest.Size = size;
                dummyForest.Gamma = epsilon;
                dummyForest.NumberOfTreeProbes = numberOfTreeProbes;
                dummyForest.Learn(data, data.DataStoreInfo.GetFieldIds(FieldGroup.Standard).ToArray());
                var dummyForestResult = Classifier.Default.Classify(dummyForest, test, null);
                dummyForestResult.ModelName = "Dummy";
                dummyForestResult.TestNum = i;
                dummyForestResult.Fold = 0;
                dummyForestResult.Epsilon = dummyForest.Gamma;
                dummyForestResult.AvgNumberOfAttributes = dummyForest.AverageNumberOfAttributes;
                Console.WriteLine(dummyForestResult);

                DecisionForestDummyRough<DecisionTreeC45> semiRoughForest = new DecisionForestDummyRough<DecisionTreeC45>();
                semiRoughForest.DataSampler = sampler;
                semiRoughForest.Size = size;
                semiRoughForest.Gamma = epsilon;
                semiRoughForest.NumberOfTreeProbes = numberOfTreeProbes;
                semiRoughForest.ReductGeneratorFactory = ReductTypes.ApproximateReductRelativeWeights;
                semiRoughForest.Learn(data, data.DataStoreInfo.GetFieldIds(FieldGroup.Standard).ToArray());
                var semiRoughForestResult = Classifier.Default.Classify(semiRoughForest, test, null);
                semiRoughForestResult.ModelName = "SemiRough";
                semiRoughForestResult.TestNum = i;
                semiRoughForestResult.Fold = 0;
                semiRoughForestResult.Epsilon = semiRoughForest.Gamma;
                semiRoughForestResult.AvgNumberOfAttributes = semiRoughForest.AverageNumberOfAttributes;
                Console.WriteLine(semiRoughForestResult);

                DecisionForestReduct<DecisionTreeC45> roughForest = new DecisionForestReduct<DecisionTreeC45>();
                roughForest.DataSampler = sampler;
                roughForest.Size = size;
                roughForest.NumberOfTreeProbes = numberOfTreeProbes;
                roughForest.Gamma = epsilon;
                //roughForest.NumberOfAttributesToCheckForSplit = numberOfAttributesToCheckForSplit;
                roughForest.ReductGeneratorFactory = ReductTypes.ApproximateReductRelativeWeights;
                roughForest.Learn(data, data.DataStoreInfo.GetFieldIds(FieldGroup.Standard).ToArray());
                var roughForestResult = Classifier.Default.Classify(roughForest, test, null);
                roughForestResult.ModelName = "Rough";
                roughForestResult.TestNum = i;
                roughForestResult.Fold = 0;
                roughForestResult.Epsilon = roughForest.Gamma;
                roughForestResult.AvgNumberOfAttributes = roughForest.AverageNumberOfAttributes;
                Console.WriteLine(roughForestResult);

                DecisionForestRandom<DecisionTreeC45> randomForest = new DecisionForestRandom<DecisionTreeC45>();
                randomForest.DataSampler = sampler;
                randomForest.Size = size;
                roughForest.NumberOfTreeProbes = numberOfTreeProbes;
                roughForest.Gamma = epsilon;
                randomForest.NumberOfAttributesToCheckForSplit = numberOfAttributesToCheckForSplit;
                randomForest.Learn(data, data.DataStoreInfo.GetFieldIds(FieldGroup.Standard).ToArray());
                var randomForestResult = Classifier.Default.Classify(randomForest, test, null);
                randomForestResult.ModelName = "RandomC45";
                randomForestResult.TestNum = i;
                randomForestResult.Fold = 0;
                randomForestResult.Epsilon = roughForest.Gamma;
                randomForestResult.AvgNumberOfAttributes = randomForest.AverageNumberOfAttributes;
                Console.WriteLine(randomForestResult);

                Console.WriteLine();
            }
        }

        [Test]
        public void ReductSubsetC45Tree()
        {
            DataStore data = DataStore.Load(@"Data\dna_modified.trn", FileFormat.RSES1);
            foreach (var fieldInfo in data.DataStoreInfo.Fields)
                fieldInfo.IsNumeric = false;
            DataStore test = DataStore.Load(@"Data\dna_modified.tst", FileFormat.RSES1, data.DataStoreInfo);

            for (int t = 0; t < 1; t++)
            {
                for (double eps = 0.0; eps < 1.0; eps += 0.05)
                {
                    string factoryKey = ReductTypes.ApproximateReductMajorityWeights;
                    WeightGeneratorMajority weightGenerator = new WeightGeneratorMajority(data);
                    PermutationCollection permList = new PermutationGenerator(data).Generate(10);

                    Args parms = new Args(6);
                    parms.SetParameter<DataStore>(ReductFactoryOptions.DecisionTable, data);
                    parms.SetParameter<string>(ReductFactoryOptions.ReductType, factoryKey);
                    parms.SetParameter<WeightGenerator>(ReductFactoryOptions.WeightGenerator, weightGenerator);
                    parms.SetParameter<double>(ReductFactoryOptions.Epsilon, eps);
                    parms.SetParameter<PermutationCollection>(ReductFactoryOptions.PermutationCollection, permList);
                    parms.SetParameter<bool>(ReductFactoryOptions.UseExceptionRules, false);

                    IReductGenerator generator = ReductFactory.GetReductGenerator(parms);
                    if (generator is ReductGeneratorMeasure)
                        ((ReductGeneratorMeasure)generator).UsePerformanceImprovements = true;
                    generator.Run();

                    IReductStoreCollection reducts = generator.GetReductStoreCollection();

                    IReductStoreCollection reductsfiltered = null;
                    if (generator is ReductGeneratorMeasure)
                        reductsfiltered = reducts.Filter(1, new ReductRuleNumberComparer());
                    else
                        reductsfiltered = reducts.FilterInEnsemble(1, new ReductStoreLengthComparer(true));

                    IReduct reduct = reductsfiltered.First().Where(r => r.IsException == false).FirstOrDefault();

                    Assert.NotNull(reduct);

                    DecisionTreeC45 treeC45 = new DecisionTreeC45();
                    treeC45.Gamma = 0.0; //eps
                    treeC45.Learn(data, reduct.Attributes.ToArray());

                    ClassificationResult resultC45 = Classifier.Default.Classify(treeC45, test);
                    resultC45.Epsilon = eps;
                    resultC45.ModelName = "C4.5";
                    Console.WriteLine(resultC45);

                    RoughClassifier roughClassifier 
                        = new RoughClassifier(reductsfiltered, 
                                              RuleQualityMethods.ConfidenceW, 
                                              RuleQualityMethods.ConfidenceW, 
                                              data.DataStoreInfo.GetDecisionValues());

                    ClassificationResult reductResult = roughClassifier.Classify(test);
                    reductResult.Epsilon = resultC45.Epsilon;
                    reductResult.ModelName = "RS";
                    reductResult.AvgNumberOfAttributes = reductsfiltered.GetAvgMeasure(new ReductMeasureLength(), false);
                    Console.WriteLine(reductResult);

                    int[] nodeAttributes = ((DecisionTreeNode)treeC45.Root).GroupBy(x => x.Attribute).Select(g => g.First().Attribute).Where(x => x != -1 && x != data.DataStoreInfo.DecisionFieldId).OrderBy(x => x).ToArray();
                    int[] reductAttributes = reduct.Attributes.ToArray();

                    Array.Sort(nodeAttributes);
                    Array.Sort(reductAttributes);

                    for (int i = 0; i < nodeAttributes.Length; i++)
                        Assert.AreEqual(nodeAttributes[i], reductAttributes[i]);
                }
            }
        }
    }
}
