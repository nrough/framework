using Infovision.Data;
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
        public void RandomForestTest()
        {
            Console.WriteLine("RandomForestTest");
            DataStore data = DataStore.Load(@"Data\dna_modified.trn", FileFormat.Rses1);
            DataStore test = DataStore.Load(@"Data\dna_modified.tst", FileFormat.Rses1, data.DataStoreInfo);

            RandomForest<DecisionTreeC45> randomForest = new RandomForest<DecisionTreeC45>();
            randomForest.Size = 100;
            randomForest.NumberOfAttributesToCheckForSplit = 5;
            double error = randomForest.Learn(data, data.DataStoreInfo.GetFieldIds(FieldTypes.Standard).ToArray());
            Console.WriteLine(randomForest.Classify(test, null));

            DecisionTreeC45 c45tree = new DecisionTreeC45();
            c45tree.Learn(data, data.DataStoreInfo.GetFieldIds(FieldTypes.Standard).ToArray());
            Console.WriteLine(c45tree.Classify(test, null));
        }

        [Test]
        public void RandomForestCARTTest()
        {
            Console.WriteLine("RandomForestCARTTest");
            DataStore data = DataStore.Load(@"Data\dna_modified.trn", FileFormat.Rses1);
            DataStore test = DataStore.Load(@"Data\dna_modified.tst", FileFormat.Rses1, data.DataStoreInfo);

            RandomForest<DecisionTreeCART> randomForest = new RandomForest<DecisionTreeCART>();
            randomForest.Size = 10;
            randomForest.NumberOfAttributesToCheckForSplit = 5;
            double error = randomForest.Learn(data, data.DataStoreInfo.GetFieldIds(FieldTypes.Standard).ToArray());
            Console.WriteLine(randomForest.Classify(test, null));

            DecisionTreeCART cartTree = new DecisionTreeCART();
            cartTree.Learn(data, data.DataStoreInfo.GetFieldIds(FieldTypes.Standard).ToArray());
            Console.WriteLine(cartTree.Classify(test, null));
        }

        [Test]
        public void RandomForestRoughMTest()
        {
            Console.WriteLine("RandomForestRoughTest");
            DataStore data = DataStore.Load(@"Data\dna_modified.trn", FileFormat.Rses1);
            DataStore test = DataStore.Load(@"Data\dna_modified.tst", FileFormat.Rses1, data.DataStoreInfo);

            RandomForest<DecisionTreeRough> randomForest = new RandomForest<DecisionTreeRough>();
            randomForest.Size = 100;
            randomForest.NumberOfAttributesToCheckForSplit = 5;
            double error = randomForest.Learn(data, data.DataStoreInfo.GetFieldIds(FieldTypes.Standard).ToArray());
            Console.WriteLine(randomForest.Classify(test, null));

            DecisionTreeRough roughTree = new DecisionTreeRough();
            roughTree.Learn(data, data.DataStoreInfo.GetFieldIds(FieldTypes.Standard).ToArray());
            Console.WriteLine(roughTree.Classify(test, null));
        }

        [Test]
        public void RoughForestRoughGammaTest()
        {
            Console.WriteLine("RandomForestRoughGammaTest");
            DataStore data = DataStore.Load(@"Data\dna_modified.trn", FileFormat.Rses1);
            DataStore test = DataStore.Load(@"Data\dna_modified.tst", FileFormat.Rses1, data.DataStoreInfo);

            RoughForest<DecisionTreeRough> randomForest = new RoughForest<DecisionTreeRough>();
            randomForest.Size = 2;
            randomForest.NumberOfAttributesToCheckForSplit = 5;
            randomForest.ReductGeneratorFactory = ReductFactoryKeyHelper.GeneralizedMajorityDecisionApproximate;
            randomForest.Epsilon = 0.4m;


            double error = randomForest.Learn(data, data.DataStoreInfo.GetFieldIds(FieldTypes.Standard).ToArray());
            Console.WriteLine(randomForest.Classify(test, null));

            DecisionTreeRough roughTree = new DecisionTreeRough();
            roughTree.Learn(data, data.DataStoreInfo.GetFieldIds(FieldTypes.Standard).ToArray());
            Console.WriteLine(roughTree.Classify(test, null));
        }

        [Test]
        public void RoughForestTest()
        {
            Console.WriteLine("RoughForestTest");

            //DataStore data = DataStore.Load(@"Data\dna_modified.trn", FileFormat.Rses1);
            //DataStore test = DataStore.Load(@"Data\dna_modified.tst", FileFormat.Rses1, data.DataStoreInfo);

            DataStore data = DataStore.Load(@"Data\letter.trn", FileFormat.Rses1);
            DataStore test = DataStore.Load(@"Data\letter.tst", FileFormat.Rses1, data.DataStoreInfo);

            int size = 100;

            for (int i = 0; i < 1; i++)
            {
                DataSampler sampler = new DataSampler(data, true);

                PermutationCollection permutations = new PermutationCollection();
                for (int j = 0; j < size; j++)
                {
                    int[] attributes = data.DataStoreInfo.GetFieldIds(FieldTypes.Standard).ToArray();
                    int len = attributes.Length;
                    attributes = attributes.RandomSubArray(RandomSingleton.Random.Next(1, len));
                    permutations.Add(new Permutation(attributes));
                }

                DummyForest<DecisionTreeC45> dummyForest = new DummyForest<DecisionTreeC45>();
                dummyForest.DataSampler = sampler;
                dummyForest.Size = size;
                dummyForest.PermutationCollection = permutations;
                dummyForest.Learn(data, data.DataStoreInfo.GetFieldIds(FieldTypes.Standard).ToArray());

                ClassificationResult dummyForestResult = dummyForest.Classify(test, null);
                dummyForestResult.ModelName = "Dummy";
                dummyForestResult.TestNum = i;
                dummyForestResult.Fold = 0;
                dummyForestResult.Epsilon = Decimal.Zero;
                dummyForestResult.QualityRatio = dummyForest.AverageNumberOfAttributes;
                Console.WriteLine(dummyForestResult);


                SemiRoughForest<DecisionTreeC45> semiRoughForest = new SemiRoughForest<DecisionTreeC45>();
                semiRoughForest.DataSampler = sampler;
                semiRoughForest.Size = size;
                semiRoughForest.Epsilon = 0.05m;
                semiRoughForest.PermutationCollection = permutations;
                semiRoughForest.ReductGeneratorFactory = ReductFactoryKeyHelper.ApproximateReductRelativeWeights;
                semiRoughForest.Learn(data, data.DataStoreInfo.GetFieldIds(FieldTypes.Standard).ToArray());

                ClassificationResult semiRoughForestResult = semiRoughForest.Classify(test, null);
                semiRoughForestResult.ModelName = "SemiRough";
                semiRoughForestResult.TestNum = i;
                semiRoughForestResult.Fold = 0;
                semiRoughForestResult.Epsilon = semiRoughForest.Epsilon;
                semiRoughForestResult.QualityRatio = semiRoughForest.AverageNumberOfAttributes;
                Console.WriteLine(semiRoughForestResult);


                RoughForest<DecisionTreeC45> roughForest = new RoughForest<DecisionTreeC45>();
                roughForest.DataSampler = sampler;
                roughForest.Size = size;
                roughForest.NumberOfPermutationsPerTree = 20;
                roughForest.ReductGeneratorFactory = ReductFactoryKeyHelper.ApproximateReductRelativeWeights;
                roughForest.Learn(data, data.DataStoreInfo.GetFieldIds(FieldTypes.Standard).ToArray());

                ClassificationResult roughForestResult = roughForest.Classify(test, null);
                roughForestResult.ModelName = "Rough";
                roughForestResult.TestNum = i;
                roughForestResult.Fold = 0;
                roughForestResult.Epsilon = Decimal.Zero;
                roughForestResult.QualityRatio = roughForest.AverageNumberOfAttributes;
                Console.WriteLine(roughForestResult);


                RandomForest<DecisionTreeC45> randomForest = new RandomForest<DecisionTreeC45>();
                randomForest.DataSampler = sampler;
                randomForest.Size = size;
                randomForest.NumberOfAttributesToCheckForSplit = (int)(0.1 * data.DataStoreInfo.GetNumberOfFields(FieldTypes.Standard));
                randomForest.Learn(data, data.DataStoreInfo.GetFieldIds(FieldTypes.Standard).ToArray());

                ClassificationResult randomForestResult = randomForest.Classify(test, null);
                randomForestResult.ModelName = "RandomC45";
                randomForestResult.TestNum = i;
                randomForestResult.Fold = 0;
                randomForestResult.Epsilon = Decimal.Zero;
                randomForestResult.QualityRatio = randomForest.AverageNumberOfAttributes;
                Console.WriteLine(randomForestResult);


                DecisionTreeC45 c45tree = new DecisionTreeC45();
                c45tree.Learn(data, data.DataStoreInfo.GetFieldIds(FieldTypes.Standard).ToArray());

                ClassificationResult c45treeResult = c45tree.Classify(test, null);
                c45treeResult.ModelName = "C45Tree";
                c45treeResult.TestNum = i;
                c45treeResult.Fold = 0;
                c45treeResult.Epsilon = Decimal.Zero;
                c45treeResult.QualityRatio = ((DecisionTreeNode)c45tree.Root)
                    .GroupBy(x => x.Key)
                    .Select(g => g.First().Key)
                    .Where(x => x != -1 && x != data.DataStoreInfo.DecisionFieldId)
                    .OrderBy(x => x).ToArray().Length;
                Console.WriteLine(c45treeResult);


                Console.WriteLine();
            }
        }

        [Test]
        public void ReductSubsetC45Tree()
        {
            DataStore data = DataStore.Load(@"Data\letter.trn", FileFormat.Rses1);
            DataStore test = DataStore.Load(@"Data\letter.tst", FileFormat.Rses1, data.DataStoreInfo);

            string factoryKey = ReductFactoryKeyHelper.ApproximateReductMajorityWeights;
            WeightGeneratorMajority weightGenerator = new WeightGeneratorMajority(data);
            PermutationCollection permList = new PermutationGenerator(data).Generate(10);

            for (int t = 0; t < 1; t++)
            {
                DecisionTreeC45 treeC45_full = new DecisionTreeC45();
                treeC45_full.Learn(data, data.DataStoreInfo.GetFieldIds(FieldTypes.Standard).ToArray());
                ClassificationResult resultC45_full = treeC45_full.Classify(test);
                Console.WriteLine("{0} | {1} | {2}", "C4.5 All", 0.0m, resultC45_full);
                Console.WriteLine();

                for (decimal eps = Decimal.Zero; eps < Decimal.One; eps += 0.01m)
                {
                    Args parms = new Args();
                    parms.SetParameter(ReductGeneratorParamHelper.TrainData, data);
                    parms.SetParameter(ReductGeneratorParamHelper.FactoryKey, factoryKey);
                    parms.SetParameter(ReductGeneratorParamHelper.WeightGenerator, weightGenerator);
                    parms.SetParameter(ReductGeneratorParamHelper.Epsilon, eps);
                    parms.SetParameter(ReductGeneratorParamHelper.PermutationCollection, permList);
                    parms.SetParameter(ReductGeneratorParamHelper.UseExceptionRules, false);

                    IReductGenerator generator = ReductFactory.GetReductGenerator(parms);
                    if (generator is ReductGeneratorMeasure)
                        ((ReductGeneratorMeasure)generator).UsePerformanceImprovements = false;
                    generator.Run();

                    IReductStoreCollection reducts = generator.GetReductStoreCollection();

                    IReductStoreCollection reductsfiltered = null;
                    if (generator is ReductGeneratorMeasure)
                        reductsfiltered = reducts.Filter(1, new ReductLengthComparer());
                    else
                        reductsfiltered = reducts.FilterInEnsemble(1, new ReductStoreLengthComparer(true));

                    IReduct reduct = reductsfiltered.First().Where(r => r.IsException == false).FirstOrDefault();

                    DecisionTreeC45 treeC45 = new DecisionTreeC45();
                    treeC45.Learn(data, reduct.Attributes.ToArray());
                    //Console.WriteLine(DecisionTreeFormatter.Construct(treeC45.Root, data, 2));
                    ClassificationResult resultC45 = treeC45.Classify(test);
                    Console.WriteLine("{0} | {1} | {2}", "C4.5    ", eps, resultC45);

                    RoughClassifier roughClassifier = new RoughClassifier(reductsfiltered, RuleQuality.ConfidenceW, RuleQuality.ConfidenceW, data.DataStoreInfo.GetDecisionValues());
                    ClassificationResult reductResult = roughClassifier.Classify(test);
                    Console.WriteLine("{0} | {1} | {2}", "RS      ", eps, reductResult);

                    int[] nodeAttributes = ((DecisionTreeNode)treeC45.Root).GroupBy(x => x.Key).Select(g => g.First().Key).Where(x => x != -1 && x != data.DataStoreInfo.DecisionFieldId).OrderBy(x => x).ToArray();
                    int[] reductAttributes = reduct.Attributes.ToArray();

                    //Console.WriteLine("Tree: {0}", nodeAttributes.ToStr(' '));
                    //Console.WriteLine("Reduct: {0}", reductAttributes.ToStr(' '));

                    for (int i = 0; i < nodeAttributes.Length; i++)
                        Assert.AreEqual(nodeAttributes[i], reductAttributes[i]);
                }
            }
        }
    }
}
