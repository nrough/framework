using System;
using System.Data;
using System.Linq;
using Accord.MachineLearning.DecisionTrees;
using Accord.MachineLearning.DecisionTrees.Learning;
using Accord.Math;
using Accord.Statistics.Filters;
using Infovision.Data;
using Infovision.Utils;
using NUnit.Framework;

using DecTrees = Accord.MachineLearning.DecisionTrees;

namespace Infovision.Datamining.Roughset.UnitTests
{
    [TestFixture]
    public class DecisionTreeTest
    {
        [Test]
        public void ID3LearnTest()
        {
            Console.WriteLine("ID3LearnTest");

            DataStore data = DataStore.Load(@"Data\dna_modified.trn", FileFormat.Rses1);
            DataStore test = DataStore.Load(@"Data\dna_modified.tst", FileFormat.Rses1, data.DataStoreInfo);

            DecisionTreeID3 treeID3 = new DecisionTreeID3();
            treeID3.Learn(data, data.DataStoreInfo.GetFieldIds(FieldTypes.Standard).ToArray());

            Console.WriteLine(DecisionTreeFormatter.Construct(treeID3.Root, data, 2));
            Console.WriteLine(treeID3.Classify(data, null));
            Console.WriteLine(treeID3.Classify(test, null));
        }

        [Test]
        public void C45LearnTest()
        {
            Console.WriteLine("C45LearnTest");
            DataStore data = DataStore.Load(@"Data\dna_modified.trn", FileFormat.Rses1);
            DataStore test = DataStore.Load(@"Data\dna_modified.tst", FileFormat.Rses1, data.DataStoreInfo);

            DecisionTreeC45 treeC45 = new DecisionTreeC45();
            treeC45.Learn(data, data.DataStoreInfo.GetFieldIds(FieldTypes.Standard).ToArray());

            Console.WriteLine(DecisionTreeFormatter.Construct(treeC45.Root, data, 2));
            Console.WriteLine(treeC45.Classify(data, null));
            Console.WriteLine(treeC45.Classify(test, null));
        }

        [Test]
        public void RandomForestTest()
        {
            Console.WriteLine("RandomForestTest");
            DataStore data = DataStore.Load(@"Data\dna_modified.trn", FileFormat.Rses1);
            DataStore test = DataStore.Load(@"Data\dna_modified.tst", FileFormat.Rses1, data.DataStoreInfo);

            RandomForest<DecisionTreeC45> randomForest = new RandomForest<DecisionTreeC45>();
            randomForest.Size = 20;
            randomForest.NumberOfRandomAttributes = 5;
            double error = randomForest.Learn(data, data.DataStoreInfo.GetFieldIds(FieldTypes.Standard).ToArray());
            Console.WriteLine(randomForest.Classify(test, null));

            DecisionTreeC45 c45tree = new DecisionTreeC45();
            c45tree.Learn(data, data.DataStoreInfo.GetFieldIds(FieldTypes.Standard).ToArray());
            Console.WriteLine(c45tree.Classify(test, null));
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

            for (int i = 0; i < 20; i++)
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
                randomForest.NumberOfRandomAttributes = (int)(0.1 * data.DataStoreInfo.GetNumberOfFields(FieldTypes.Standard));
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

        public DataStore GetDataStore()
        {
            /*
            DataStore data = DataStore.Load(@"Data\playgolf.train", FileFormat.Rses1);
            data.DataStoreInfo.GetFieldInfo(1).Name = "Outlook";
            data.DataStoreInfo.GetFieldInfo(2).Name = "Temperature";
            data.DataStoreInfo.GetFieldInfo(3).Name = "Humidity";
            data.DataStoreInfo.GetFieldInfo(4).Name = "Wind";
            data.DataStoreInfo.GetFieldInfo(5).Name = "Play";
            */

            DataStore data = DataStore.Load(@"Data\dna_modified.trn", FileFormat.Rses1);

            return data;
        }

        [Test]
        public void ReductSubsetC45Tree()
        {
            DataStore data = DataStore.Load(@"Data\letter.trn", FileFormat.Rses1);
            DataStore test = DataStore.Load(@"Data\letter.tst", FileFormat.Rses1, data.DataStoreInfo);

            string factoryKey = ReductFactoryKeyHelper.ApproximateReductMajorityWeights;
            WeightGeneratorMajority weightGenerator = new WeightGeneratorMajority(data);
            PermutationCollection permList = new PermutationGenerator(data).Generate(10);

            for (int t = 0; t < 20; t++)
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

        #region Accord Trees

        [Test]
        public void AccordC45Test()
        {
            Console.WriteLine("AccordC45Test");

            DataStore ds = this.GetDataStore();
            DataTable data = ds.ToDataTable();
            DecisionVariable[] attributes = this.AccordAttributes(ds);
            int classCount = ds.DataStoreInfo.DecisionInfo.NumberOfValues;
            DecTrees.DecisionTree tree = new DecTrees.DecisionTree(attributes, classCount);
            C45Learning c45learning = new C45Learning(tree);

            Codification codebook = new Codification(data);
            DataTable symbols = codebook.Apply(data);
            double[][] inputs = symbols.ToArray<double>(this.AttributeNames(ds));
            int[] outputs = symbols.ToArray<int>(this.DecisionName(ds));

            // Learn the training instances!
            c45learning.Run(inputs, outputs);

            string[] conditionalAttributes = this.AttributeNames(ds);
            string decisionName = this.DecisionName(ds);

            this.PrintTree(tree.Root, 2, 0, codebook, decisionName);

            int count = 0;
            int correct = 0;
            foreach (DataRow row in data.Rows)
            {
                var rec = row.ToArray<string>(conditionalAttributes);
                string actual = row[decisionName].ToString();
                int[] query = codebook.Translate(conditionalAttributes, rec);
                int output = tree.Compute(query);
                string answer = codebook.Translate(decisionName, output);

                count++;
                if (actual == answer)
                    correct++;
            }

            Console.WriteLine("Accuracy: {0:0.0000}", (double)correct / (double)count);
        }

        [Test]
        public void AccordID3Test()
        {
            Console.WriteLine("AccordID3Test");

            DataStore ds = this.GetDataStore();
            DataTable data = ds.ToDataTable();
            DecisionVariable[] attributes = this.AccordAttributes(ds);
            int classCount = ds.DataStoreInfo.DecisionInfo.NumberOfValues;

            DecTrees.DecisionTree tree = new DecTrees.DecisionTree(attributes, classCount);
            ID3Learning id3learning = new ID3Learning(tree);

            Codification codebook = new Codification(data);
            DataTable symbols = codebook.Apply(data);
            int[][] inputs = symbols.ToArray<int>(this.AttributeNames(ds));
            int[] outputs = symbols.ToArray<int>(this.DecisionName(ds));

            // Learn the training instances!
            id3learning.Run(inputs, outputs);

            string[] conditionalAttributes = this.AttributeNames(ds);
            string decisionName = this.DecisionName(ds);

            this.PrintTree(tree.Root, 2, 0, codebook, decisionName);

            int count = 0;
            int correct = 0;
            foreach (DataRow row in data.Rows)
            {
                var rec = row.ToArray<string>(conditionalAttributes);
                string actual = row[decisionName].ToString();
                int[] query = codebook.Translate(conditionalAttributes, rec);
                int output = tree.Compute(query);
                string answer = codebook.Translate(decisionName, output);

                count++;
                if (actual == answer)
                    correct++;
            }

            Console.WriteLine("Accuracy: {0:0.0000}", (double)correct / (double)count);
        }

        public void PrintTree(DecisionNode node, int indentSize, int currentLevel, Codification codebook, string decisionName)
        {
            if (!node.IsRoot)
            {
                var currentNode = string.Format("{0}({1})", new string(' ', indentSize * currentLevel), node.ToString(codebook));
                Console.WriteLine(currentNode);
                if (node.Output != null)
                {
                    currentNode = string.Format("{0}({2} == {1})",
                        new string(' ', indentSize * (currentLevel + 1)),
                        codebook.Translate(decisionName, (int)node.Output),
                        decisionName);

                    Console.WriteLine(currentNode);
                }
            }

            if (node.Branches != null)
                foreach (var child in node.Branches)
                    PrintTree(child, indentSize, currentLevel + 1, codebook, decisionName);
        }

        public DecisionVariable[] AccordAttributes(DataStore data)
        {
            int[] fieldIds = data.DataStoreInfo.GetFieldIds().ToArray();
            DecisionVariable[] variables = null;
            if (data.DataStoreInfo.DecisionFieldId > 0)
                variables = new DecisionVariable[fieldIds.Length - 1]; //do not include decision attribute
            else
                variables = new DecisionVariable[fieldIds.Length];

            int j = 0;
            for (int i = 0; i < fieldIds.Length; i++)
            {
                DataFieldInfo field = data.DataStoreInfo.GetFieldInfo(fieldIds[i]);
                if (field.Id != data.DataStoreInfo.DecisionFieldId)
                    variables[j++] = new DecisionVariable(field.Name, field.NumberOfValues);
            }
            return variables;
        }

        public string[] AttributeNames(DataStore data)
        {
            int[] fieldIds = data.DataStoreInfo.GetFieldIds().ToArray();
            string[] names = null;
            if (data.DataStoreInfo.DecisionFieldId > 0)
                names = new string[fieldIds.Length - 1]; //do not include decision attribute
            else
                names = new string[fieldIds.Length];
            int j = 0;
            for (int i = 0; i < fieldIds.Length; i++)
            {
                DataFieldInfo field = data.DataStoreInfo.GetFieldInfo(fieldIds[i]);
                if (field.Id != data.DataStoreInfo.DecisionFieldId)
                    names[j++] = field.Name;
            }
            return names;
        }

        public string DecisionName(DataStore data)
        {
            return data.DataStoreInfo.DecisionInfo.Name;
        }

        #endregion Accord Trees
    }
}