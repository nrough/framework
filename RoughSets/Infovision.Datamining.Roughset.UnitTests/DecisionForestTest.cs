using Infovision.Data;
using Infovision.Datamining.Roughset.DecisionTrees;
using Infovision.Utils;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infovision.Datamining.Roughset.UnitTests
{
    [TestFixture]
    public class DecisionForestTest
    {
        [Test]
        public void DecisionForestRandomTest()
        {
            Console.WriteLine("RandomForestTest");
            DataStore data = DataStore.Load(@"Data\dna_modified.trn", FileFormat.Rses1);
            DataStore test = DataStore.Load(@"Data\dna_modified.tst", FileFormat.Rses1, data.DataStoreInfo);

            decimal epsilon = 0.07m;

            DecisionForestRandom<DecisionTreeC45> randomForest = new DecisionForestRandom<DecisionTreeC45>();
            randomForest.Size = 10;
            randomForest.NumberOfAttributesToCheckForSplit = 5;
            randomForest.NumberOfTreeProbes = 10;
            randomForest.Epsilon = epsilon;
            double error = randomForest.Learn(data, data.DataStoreInfo.GetFieldIds(FieldTypes.Standard).ToArray());
            Console.WriteLine(Classifier.Instance.Classify(randomForest, test, null));

            DecisionTreeC45 c45tree = new DecisionTreeC45();
            c45tree.Epsilon = epsilon;
            c45tree.Learn(data, data.DataStoreInfo.GetFieldIds(FieldTypes.Standard).ToArray());
            Console.WriteLine(Classifier.Instance.Classify(c45tree, test, null));
        }

        [Test]
        public void RandomForestCARTTest()
        {
            decimal epsilon = 0.07m;

            Console.WriteLine("RandomForestCARTTest");
            DataStore data = DataStore.Load(@"Data\dna_modified.trn", FileFormat.Rses1);
            DataStore test = DataStore.Load(@"Data\dna_modified.tst", FileFormat.Rses1, data.DataStoreInfo);

            DecisionForestRandom<DecisionTreeCART> randomForest = new DecisionForestRandom<DecisionTreeCART>();
            randomForest.Size = 10;
            randomForest.NumberOfAttributesToCheckForSplit = 5;
            randomForest.NumberOfTreeProbes = 10;
            randomForest.Epsilon = epsilon;
            double error = randomForest.Learn(data, data.DataStoreInfo.GetFieldIds(FieldTypes.Standard).ToArray());
            Console.WriteLine(Classifier.Instance.Classify(randomForest, test, null));

            DecisionTreeCART cartTree = new DecisionTreeCART();
            cartTree.Learn(data, data.DataStoreInfo.GetFieldIds(FieldTypes.Standard).ToArray());
            Console.WriteLine(Classifier.Instance.Classify(cartTree, test, null));
        }

        [Test]
        public void RandomForestRoughMTest()
        {
            decimal epsilon = 0.07m;

            Console.WriteLine("RandomForestRoughTest");
            DataStore data = DataStore.Load(@"Data\dna_modified.trn", FileFormat.Rses1);
            DataStore test = DataStore.Load(@"Data\dna_modified.tst", FileFormat.Rses1, data.DataStoreInfo);

            DecisionForestRandom<DecisionTreeRough> randomForest = new DecisionForestRandom<DecisionTreeRough>();
            randomForest.Size = 100;
            randomForest.NumberOfAttributesToCheckForSplit = 5;
            randomForest.NumberOfTreeProbes = 10;
            randomForest.Epsilon = epsilon;
            double error = randomForest.Learn(data, data.DataStoreInfo.GetFieldIds(FieldTypes.Standard).ToArray());
            Console.WriteLine(Classifier.Instance.Classify(randomForest, test, null));

            DecisionTreeRough roughTree = new DecisionTreeRough();
            roughTree.Learn(data, data.DataStoreInfo.GetFieldIds(FieldTypes.Standard).ToArray());
            Console.WriteLine(Classifier.Instance.Classify(roughTree, test, null));
        }

        [Test]
        public void RoughForestRoughGammaTest()
        {
            decimal epsilon = 0.07m;

            Console.WriteLine("RandomForestRoughGammaTest");
            DataStore data = DataStore.Load(@"Data\dna_modified.trn", FileFormat.Rses1);
            DataStore test = DataStore.Load(@"Data\dna_modified.tst", FileFormat.Rses1, data.DataStoreInfo);

            DecisionForestReduct<DecisionTreeRough> randomForest = new DecisionForestReduct<DecisionTreeRough>();
            randomForest.Size = 10;
            randomForest.NumberOfTreeProbes = 10;
            randomForest.Epsilon = epsilon;
            randomForest.ReductGeneratorFactory = ReductFactoryKeyHelper.GeneralizedMajorityDecisionApproximate;

            double error = randomForest.Learn(data, data.DataStoreInfo.GetFieldIds(FieldTypes.Standard).ToArray());
            Console.WriteLine(Classifier.Instance.Classify(randomForest, test, null));

            DecisionTreeRough roughTree = new DecisionTreeRough();
            roughTree.Epsilon = epsilon;
            roughTree.Learn(data, data.DataStoreInfo.GetFieldIds(FieldTypes.Standard).ToArray());
            Console.WriteLine(Classifier.Instance.Classify(roughTree, test, null));
        }

        [Test]
        public void RoughForestTest()
        {
            Console.WriteLine("RoughForestTest");

            decimal epsilon = 0.07m;
            int numberOfAttributesToCheckForSplit = 5;
            int numberOfTreeProbes = 10;
            int size = 10;

            //DataStore data = DataStore.Load(@"Data\dna_modified.trn", FileFormat.Rses1);
            //DataStore test = DataStore.Load(@"Data\dna_modified.tst", FileFormat.Rses1, data.DataStoreInfo);

            DataStore data = DataStore.Load(@"Data\letter.trn", FileFormat.Rses1);
            DataStore test = DataStore.Load(@"Data\letter.tst", FileFormat.Rses1, data.DataStoreInfo);

            
            for (int i = 0; i < 1; i++)
            {
                DataSampler sampler = new DataSampler(data, true);

                DecisionForestDummy<DecisionTreeC45> dummyForest = new DecisionForestDummy<DecisionTreeC45>();
                dummyForest.DataSampler = sampler;
                dummyForest.Size = size;
                dummyForest.Epsilon = epsilon;
                dummyForest.NumberOfTreeProbes = numberOfTreeProbes;
                dummyForest.Learn(data, data.DataStoreInfo.GetFieldIds(FieldTypes.Standard).ToArray());
                var dummyForestResult = Classifier.Instance.Classify(dummyForest, test, null);
                dummyForestResult.ModelName = "Dummy";
                dummyForestResult.TestNum = i;
                dummyForestResult.Fold = 0;
                dummyForestResult.Epsilon = dummyForest.Epsilon;
                dummyForestResult.QualityRatio = dummyForest.AverageNumberOfAttributes;
                Console.WriteLine(dummyForestResult);

                DecisionForestDummyRough<DecisionTreeC45> semiRoughForest = new DecisionForestDummyRough<DecisionTreeC45>();
                semiRoughForest.DataSampler = sampler;
                semiRoughForest.Size = size;
                semiRoughForest.Epsilon = epsilon;
                semiRoughForest.NumberOfTreeProbes = numberOfTreeProbes;
                semiRoughForest.ReductGeneratorFactory = ReductFactoryKeyHelper.ApproximateReductRelativeWeights;
                semiRoughForest.Learn(data, data.DataStoreInfo.GetFieldIds(FieldTypes.Standard).ToArray());
                var semiRoughForestResult = Classifier.Instance.Classify(semiRoughForest, test, null);
                semiRoughForestResult.ModelName = "SemiRough";
                semiRoughForestResult.TestNum = i;
                semiRoughForestResult.Fold = 0;
                semiRoughForestResult.Epsilon = semiRoughForest.Epsilon;
                semiRoughForestResult.QualityRatio = semiRoughForest.AverageNumberOfAttributes;
                Console.WriteLine(semiRoughForestResult);

                DecisionForestReduct<DecisionTreeC45> roughForest = new DecisionForestReduct<DecisionTreeC45>();
                roughForest.DataSampler = sampler;
                roughForest.Size = size;
                roughForest.NumberOfTreeProbes = numberOfTreeProbes;
                roughForest.Epsilon = epsilon;
                //roughForest.NumberOfAttributesToCheckForSplit = numberOfAttributesToCheckForSplit;
                roughForest.ReductGeneratorFactory = ReductFactoryKeyHelper.ApproximateReductRelativeWeights;
                roughForest.Learn(data, data.DataStoreInfo.GetFieldIds(FieldTypes.Standard).ToArray());
                var roughForestResult = Classifier.Instance.Classify(roughForest, test, null);
                roughForestResult.ModelName = "Rough";
                roughForestResult.TestNum = i;
                roughForestResult.Fold = 0;
                roughForestResult.Epsilon = roughForest.Epsilon;
                roughForestResult.QualityRatio = roughForest.AverageNumberOfAttributes;
                Console.WriteLine(roughForestResult);

                DecisionForestRandom<DecisionTreeC45> randomForest = new DecisionForestRandom<DecisionTreeC45>();
                randomForest.DataSampler = sampler;
                randomForest.Size = size;
                roughForest.NumberOfTreeProbes = numberOfTreeProbes;
                roughForest.Epsilon = epsilon;
                randomForest.NumberOfAttributesToCheckForSplit = numberOfAttributesToCheckForSplit;
                randomForest.Learn(data, data.DataStoreInfo.GetFieldIds(FieldTypes.Standard).ToArray());
                var randomForestResult = Classifier.Instance.Classify(randomForest, test, null);
                randomForestResult.ModelName = "RandomC45";
                randomForestResult.TestNum = i;
                randomForestResult.Fold = 0;
                randomForestResult.Epsilon = roughForest.Epsilon;
                randomForestResult.QualityRatio = randomForest.AverageNumberOfAttributes;
                Console.WriteLine(randomForestResult);

                Console.WriteLine();
            }
        }

        [Test]
        public void ReductSubsetC45Tree()
        {
            DataStore data = DataStore.Load(@"Data\dna_modified.trn", FileFormat.Rses1);
            DataStore test = DataStore.Load(@"Data\dna_modified.tst", FileFormat.Rses1, data.DataStoreInfo);

            for (int t = 0; t < 1; t++)
            {
                for (decimal eps = Decimal.Zero; eps < Decimal.One; eps += 0.05m)
                {
                    string factoryKey = ReductFactoryKeyHelper.ApproximateReductMajorityWeights;
                    WeightGeneratorMajority weightGenerator = new WeightGeneratorMajority(data);
                    PermutationCollection permList = new PermutationGenerator(data).Generate(10);

                    Args parms = new Args(6);
                    parms.SetParameter<DataStore>(ReductGeneratorParamHelper.TrainData, data);
                    parms.SetParameter<string>(ReductGeneratorParamHelper.FactoryKey, factoryKey);
                    parms.SetParameter<WeightGenerator>(ReductGeneratorParamHelper.WeightGenerator, weightGenerator);
                    parms.SetParameter<decimal>(ReductGeneratorParamHelper.Epsilon, eps);
                    parms.SetParameter<PermutationCollection>(ReductGeneratorParamHelper.PermutationCollection, permList);
                    parms.SetParameter<bool>(ReductGeneratorParamHelper.UseExceptionRules, false);

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
                    treeC45.Epsilon = Decimal.Zero; //eps
                    treeC45.Learn(data, reduct.Attributes.ToArray());

                    ClassificationResult resultC45 = Classifier.Instance.Classify(treeC45, test);
                    resultC45.Epsilon = eps;
                    resultC45.ModelName = "C4.5";
                    Console.WriteLine(resultC45);

                    RoughClassifier roughClassifier 
                        = new RoughClassifier(reductsfiltered, 
                                              RuleQuality.ConfidenceW, 
                                              RuleQuality.ConfidenceW, 
                                              data.DataStoreInfo.GetDecisionValues());

                    ClassificationResult reductResult = roughClassifier.Classify(test);
                    reductResult.Epsilon = resultC45.Epsilon;
                    reductResult.ModelName = "RS";
                    reductResult.QualityRatio = reductsfiltered.GetAvgMeasure(new ReductMeasureLength(), false);
                    Console.WriteLine(reductResult);

                    int[] nodeAttributes = ((DecisionTreeNode)treeC45.Root).GroupBy(x => x.Key).Select(g => g.First().Key).Where(x => x != -1 && x != data.DataStoreInfo.DecisionFieldId).OrderBy(x => x).ToArray();
                    int[] reductAttributes = reduct.Attributes.ToArray();

                    for (int i = 0; i < nodeAttributes.Length; i++)
                        Assert.AreEqual(nodeAttributes[i], reductAttributes[i]);
                }
            }
        }
    }
}
