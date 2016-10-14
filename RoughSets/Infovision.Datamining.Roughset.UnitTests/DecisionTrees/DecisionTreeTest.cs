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
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Infovision.Datamining.Roughset.DecisionTrees;
using Infovision.Datamining.Roughset.DecisionTrees.Pruning;

namespace Infovision.Datamining.Roughset.UnitTests.DecisionTrees
{
    [TestFixture]
    public class DecisionTreeTest
    {
        [Test, Repeat(1)]
        public void DecisionTreeRoughForNumericAttributeTest()
        {            
            int numOfFolds = 5;
            DataStore data = DataStore.Load(@"Data\german.data", FileFormat.Csv);
            DataStore train = null, test = null;
            
            for (double eps = 0.0; eps < 0.5; eps += 0.01)
            {
                double error = 0;
                DataStoreSplitter splitter = new DataStoreSplitter(data, numOfFolds);
                for (int f = 0; f < numOfFolds; f++)
                {
                    splitter.ActiveFold = f;
                    splitter.Split(ref train, ref test);

                    DecisionTreeRough tree = new DecisionTreeRough();
                    tree.Epsilon = eps;
                    tree.Learn(train, train.DataStoreInfo.GetFieldIds(FieldTypes.Standard).ToArray());

                    ClassificationResult result = Classifier.DefaultClassifer.Classify(tree, test);
                    //Console.WriteLine(result);

                    error += result.Error;
                }

                Console.WriteLine("{0} Error: {1}", eps, error / (double)numOfFolds);
            }
        }

        [Test, Repeat(1)]
        public void DecisionTreeC45ForNumericAttributeTest()
        {
            int numOfFolds = 5;
            DataStore data = DataStore.Load(@"Data\german.data", FileFormat.Csv);
            DataStore train = null, tmp = null, prune = null, test = null;

            double error = 0;
            DataStoreSplitter splitter = new DataStoreSplitter(data, numOfFolds);
            for (int f = 0; f < numOfFolds; f++)
            {
                splitter.ActiveFold = f;
                splitter.Split(ref tmp, ref test);

                DataStoreSplitter splitter2 = new DataStoreSplitterRatio(tmp, 0.5);
                splitter2.Split(ref train, ref prune);

                DecisionTreeC45 tree = new DecisionTreeC45();
                tree.Learn(train, train.DataStoreInfo.GetFieldIds(FieldTypes.Standard).ToArray());
                ErrorBasedPruning pruning = new ErrorBasedPruning(tree, prune);
                pruning.Prune();

                ClassificationResult result = Classifier.DefaultClassifer.Classify(tree, test);
                Console.WriteLine(result);

                error += result.Error;
            }

            Console.WriteLine("Error: {0}", error / (double)numOfFolds);                        
        }

        [Test, Repeat(50)]
        public void DecisionTreeC45NoPruningForNumericAttributeTest()
        {
            int numOfFolds = 5;
            DataStore data = DataStore.Load(@"Data\german.data", FileFormat.Csv);
            DataStore train = null, test = null;
            double error = 0;
            DataStoreSplitter splitter = new DataStoreSplitter(data, numOfFolds);
            for (int f = 0; f < numOfFolds; f++)
            {
                splitter.ActiveFold = f;
                splitter.Split(ref train, ref test);

                DecisionTreeC45 tree = new DecisionTreeC45();
                tree.Learn(train, train.DataStoreInfo.GetFieldIds(FieldTypes.Standard).ToArray());

                ClassificationResult result = Classifier.DefaultClassifer.Classify(tree, test);

                error += result.Error;
            }

            Console.WriteLine("Error: {0}", error / (double)numOfFolds);

        }

        [Test, Repeat(1)]
        public void DecisionForestForNumericAttributeTest()
        {
            int numOfFolds = 5;
            DataStore data = DataStore.Load(@"Data\german.data", FileFormat.Csv);
            DataStore train = null, test = null;

            DataStoreSplitter splitter = new DataStoreSplitter(data, numOfFolds);
            for (int f = 0; f < numOfFolds; f++)
            {
                splitter.ActiveFold = f;
                splitter.Split(ref train, ref test);

                DecisionForestRandom<DecisionTreeC45> forest = new DecisionForestRandom<DecisionTreeC45>();
                forest.NumberOfAttributesToCheckForSplit = 3;
                forest.Size = 50;
                forest.Learn(train, train.DataStoreInfo.GetFieldIds(FieldTypes.Standard).ToArray());

                ClassificationResult result = Classifier.DefaultClassifer.Classify(forest, test);
                Console.WriteLine(result);
            }            
        }

        [Test, Repeat(10)]
        public void DecisionForestRoughForNumericAttributeTest()
        {
            int numOfFolds = 5;
            DataStore data = DataStore.Load(@"Data\german.data", FileFormat.Csv);
            DataStore train = null, test = null;

            DataStoreSplitter splitter = new DataStoreSplitter(data, numOfFolds);
            for (int f = 0; f < numOfFolds; f++)
            {
                splitter.ActiveFold = f;
                splitter.Split(ref train, ref test);

                DecisionForestRandom<DecisionTreeRough> forest = new DecisionForestRandom<DecisionTreeRough>();
                forest.NumberOfAttributesToCheckForSplit = 3;
                forest.Size = 50;
                forest.Epsilon = 0.22;
                forest.Learn(train, train.DataStoreInfo.GetFieldIds(FieldTypes.Standard).ToArray());

                ClassificationResult result = Classifier.DefaultClassifer.Classify(forest, test);
                Console.WriteLine(result);
            }
        }

        [Test, Repeat(10)]
        public void TreeLearnPerformanceTest()
        {
            InfovisionConfiguration.MaxDegreeOfParallelism = Environment.ProcessorCount;

            string path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            path = Path.Combine(path, "Data", "dna_modified.trn");

            DataStore data = DataStore.Load(path, FileFormat.Rses1);
            foreach (var fieldInfo in data.DataStoreInfo.Fields) fieldInfo.IsNumeric = false;

            int total = 20;
            long sum = 0;
            for (int i = 0; i < total; i++)
            {   
                int[] attributes = data.DataStoreInfo.GetFieldIds(FieldTypes.Standard).ToArray();

                Stopwatch s = new Stopwatch();
                s.Start();

                DecisionTreeC45 tree = new DecisionTreeC45();
                tree.Epsilon = 0;
                tree.Learn(data, attributes);

                s.Stop();
                sum += s.ElapsedMilliseconds;
            }

            Console.WriteLine("C45: {0} ms", sum / total);
        }

        [Test]
        public void CountLeavesTest()
        {
            Console.WriteLine("CountLeavesTest");

            DataStore data = DataStore.Load(@"Data\dna_modified.trn", FileFormat.Rses1);
            foreach (var fieldInfo in data.DataStoreInfo.Fields) fieldInfo.IsNumeric = false;
            DataStore test = DataStore.Load(@"Data\dna_modified.tst", FileFormat.Rses1, data.DataStoreInfo);

            DecisionTreeID3 treeID3 = new DecisionTreeID3();
            treeID3.Learn(data, data.DataStoreInfo.GetFieldIds(FieldTypes.Standard).ToArray());
            Assert.Greater(DecisionTreeHelper.CountLeaves(treeID3.Root), 0);
        }

        [Test]
        public void GetRulesFromTreeTest()
        {
            DataStore data = DataStore.Load(@"Data\dna_modified.trn", FileFormat.Rses1);
            foreach (var fieldInfo in data.DataStoreInfo.Fields) fieldInfo.IsNumeric = false;
            int[] attributes = data.DataStoreInfo.GetFieldIds(FieldTypes.Standard).ToArray();
            int prevCount = Int32.MaxValue;

            for (double eps = 0.0; eps < 1.0; eps += 0.01)
            {                
                DecisionTreeID3 treeID3 = new DecisionTreeID3();
                treeID3.Epsilon = eps;
                treeID3.Learn(data, attributes);
                AttributeValueVector[] ruleConditions = DecisionTreeHelper.GetRulesFromTree(treeID3.Root, data);
                //Console.WriteLine(ruleConditions.Length);
                Assert.AreEqual(
                    ruleConditions.Length, 
                    DecisionTreeHelper.CountLeaves(treeID3.Root));

                Assert.GreaterOrEqual(prevCount, ruleConditions.Length);
                prevCount = ruleConditions.Length;
            }
        }

        [Test]
        public void CheckTreeConvergedTest()
        {
            DataStore data = DataStore.Load(@"Data\dna_modified.trn", FileFormat.Rses1);
            foreach (var fieldInfo in data.DataStoreInfo.Fields) fieldInfo.IsNumeric = false;
            DataStore test = DataStore.Load(@"Data\dna_modified.tst", FileFormat.Rses1, data.DataStoreInfo);
            int[] attributes = data.DataStoreInfo.GetFieldIds(FieldTypes.Standard).ToArray();

            for (double eps = 0.0; eps < 1.0; eps += 0.01)
            {
                DecisionTreeID3 treeID3 = new DecisionTreeID3();
                treeID3.Epsilon = eps;
                double errorTrain = 1.0 - treeID3.Learn(data, attributes).Accuracy;
                double errorTest = 1.0 - Classifier.DefaultClassifer.Classify(treeID3, test).Accuracy;

                /*
                Console.WriteLine("eps={0} numrul={1} errtrn={2} errtst={3}",
                    eps,
                    DecisionTreeHelper.CountLeaves(treeID3.Root),
                    errorTrain,
                    errorTest);
                */
            }
        }

        [Test]
        public void ID3LearnTest()
        {
            Console.WriteLine("ID3LearnTest");

            DataStore data = DataStore.Load(@"Data\dna_modified.trn", FileFormat.Rses1);
            foreach (var fieldInfo in data.DataStoreInfo.Fields) fieldInfo.IsNumeric = false;
            DataStore test = DataStore.Load(@"Data\dna_modified.tst", FileFormat.Rses1, data.DataStoreInfo);

            DecisionTreeID3 treeID3 = new DecisionTreeID3();
            treeID3.Epsilon = 0;
            treeID3.Learn(data, data.DataStoreInfo.GetFieldIds(FieldTypes.Standard).ToArray());

            //Console.WriteLine(DecisionTreeFormatter.Construct(treeID3.Root, data, 2));
            Console.WriteLine(Classifier.DefaultClassifer.Classify(treeID3, data, null));
            Console.WriteLine(Classifier.DefaultClassifer.Classify(treeID3, test, null));
        }

        [Test, Repeat(1)]
        public void C45LearnTest()
        {            
            Console.WriteLine("C45LearnTest");
            DataStore data = DataStore.Load(@"Data\dna_modified.trn", FileFormat.Rses1);
            foreach (var fieldInfo in data.DataStoreInfo.Fields) fieldInfo.IsNumeric = false;
            DataStore test = DataStore.Load(@"Data\dna_modified.tst", FileFormat.Rses1, data.DataStoreInfo);            

            DecisionTreeC45 treeC45 = new DecisionTreeC45();
            treeC45.MaxHeight = 2;
            treeC45.Learn(data, data.DataStoreInfo.GetFieldIds(FieldTypes.Standard).ToArray());

            Console.WriteLine(DecisionTreeFormatter.Construct(treeC45.Root, data));
            
            //Console.WriteLine(DecisionTreeFormatter.Construct(treeC45.Root, data, 2));
            Console.WriteLine(Classifier.DefaultClassifer.Classify(treeC45, data, null));
            Console.WriteLine(Classifier.DefaultClassifer.Classify(treeC45, test, null));
        }

        [Test]
        public void CARTLearnTest()
        {
            Console.WriteLine("CARTLearnTest");
            DataStore data = DataStore.Load(@"Data\dna_modified.trn", FileFormat.Rses1);
            foreach (var fieldInfo in data.DataStoreInfo.Fields) fieldInfo.IsNumeric = false;
            DataStore test = DataStore.Load(@"Data\dna_modified.tst", FileFormat.Rses1, data.DataStoreInfo);

            DecisionTreeCART treeCART = new DecisionTreeCART();
            treeCART.Epsilon = 0;
            treeCART.Learn(data, data.DataStoreInfo.GetFieldIds(FieldTypes.Standard).ToArray());

            //Console.WriteLine(DecisionTreeFormatter.Construct(treeCART.Root, data, 2));
            Console.WriteLine(Classifier.DefaultClassifer.Classify(treeCART, data, null));
            Console.WriteLine(Classifier.DefaultClassifer.Classify(treeCART, test, null));
        }

        [Test]
        public void RoughTreeLearnTest()
        {
            Console.WriteLine("RoughTreeLearnTest");

            int maxHeight = -1;

            DataStore data = DataStore.Load(@"Data\dna_modified.trn", FileFormat.Rses1);
            foreach (var fieldInfo in data.DataStoreInfo.Fields) fieldInfo.IsNumeric = false;
            DataStore test = DataStore.Load(@"Data\dna_modified.tst", FileFormat.Rses1, data.DataStoreInfo);

            DataStore prune = null, train = null;
            DataStoreSplitter splitter = new DataStoreSplitterRatio(data, 0.5);
            splitter.Split(ref train, ref prune);

            DecisionTreeRough treeRough = new DecisionTreeRough();
            treeRough.MaxHeight = maxHeight;
            treeRough.Learn(train, train.DataStoreInfo.GetFieldIds(FieldTypes.Standard).ToArray());
            ReducedErrorPruning pruning = new ReducedErrorPruning(treeRough, prune);
            pruning.Prune();

            Console.WriteLine(DecisionTreeFormatter.Construct(treeRough.Root, data));
            Console.WriteLine(DecisionTreeBase.GetNumberOfRules(treeRough));
            Console.WriteLine(Classifier.DefaultClassifer.Classify(treeRough, data, null));
            Console.WriteLine(Classifier.DefaultClassifer.Classify(treeRough, test, null));

            Console.WriteLine("C45LearnTest");

            DecisionTreeC45 treeC45 = new DecisionTreeC45();
            treeC45.MaxHeight = maxHeight;
            treeC45.Learn(train, train.DataStoreInfo.GetFieldIds(FieldTypes.Standard).ToArray());
            ErrorBasedPruning pruning2 = new ErrorBasedPruning(treeC45, prune);
            pruning2.Prune();

            Console.WriteLine(DecisionTreeFormatter.Construct(treeC45.Root, data));
            Console.WriteLine(DecisionTreeBase.GetNumberOfRules(treeC45));
            Console.WriteLine(Classifier.DefaultClassifer.Classify(treeC45, data, null));
            Console.WriteLine(Classifier.DefaultClassifer.Classify(treeC45, test, null));


        }        


        #region Accord Trees

        [Test]
        public void AccordC45Test()
        {
            Console.WriteLine("AccordC45Test");

            DataStore ds = DataStore.Load(@"Data\dna_modified.trn", FileFormat.Rses1);
            foreach (var fieldInfo in ds.DataStoreInfo.Fields) fieldInfo.IsNumeric = false;
            DataStore tst = DataStore.Load(@"Data\dna_modified.tst", FileFormat.Rses1, ds.DataStoreInfo);

            DataTable data = ds.ToDataTable();
            DataTable data_tst = tst.ToDataTable();

            DecisionVariable[] attributes = this.AccordAttributes(ds);
            int classCount = ds.DataStoreInfo.DecisionInfo.NumberOfValues;
            DecTrees.DecisionTree tree = new DecTrees.DecisionTree(attributes, classCount);
            C45Learning c45learning = new C45Learning(tree);

            Codification codebook = new Codification(data);
            DataTable symbols = codebook.Apply(data);
            double[][] inputs = symbols.ToArray<double>(this.AttributeNames(ds));
            int[] outputs = symbols.ToArray<int>(this.DecisionName(ds));

            DataTable symbols_tst = codebook.Apply(data_tst);
            int[][] inputs_tst = symbols_tst.ToArray<int>(this.AttributeNames(ds));
            int[] outputs_tst = symbols_tst.ToArray<int>(this.DecisionName(ds));


            // Learn the training instances!
            c45learning.Run(inputs, outputs);

            string[] conditionalAttributes = this.AttributeNames(ds);
            string decisionName = this.DecisionName(ds);

            //this.PrintTree(tree.Root, 2, 0, codebook, decisionName);

            int count = 0;
            int correct = 0;
            foreach (DataRow row in data.Rows)
            {
                var rec = row.ToArray<string>(conditionalAttributes);
                string actual = row[decisionName].ToString();
                int[] query = codebook.Translate(conditionalAttributes, rec);
                int output = tree.Decide(query);
                string answer = codebook.Translate(decisionName, output);

                count++;
                if (actual == answer)
                    correct++;
            }

            Console.WriteLine("Accuracy train: {0:0.0000}", (double)correct / (double)count);

            count = 0;
            correct = 0;
            foreach (DataRow row in data_tst.Rows)
            {
                var rec = row.ToArray<string>(conditionalAttributes);
                string actual = row[decisionName].ToString();
                int[] query = codebook.Translate(conditionalAttributes, rec);
                int output = tree.Decide(query);
                string answer = codebook.Translate(decisionName, output);

                count++;
                if (actual == answer)
                    correct++;
            }

            Console.WriteLine("Accuracy test: {0:0.0000}", (double)correct / (double)count);
        }

        [Test]
        public void AccordID3Test()
        {
            Console.WriteLine("AccordID3Test");

            DataStore ds = DataStore.Load(@"Data\dna_modified.trn", FileFormat.Rses1);
            DataStore tst = DataStore.Load(@"Data\dna_modified.tst", FileFormat.Rses1, ds.DataStoreInfo);

            DataTable data = ds.ToDataTable();
            DataTable data_tst = tst.ToDataTable();

            DecisionVariable[] attributes = this.AccordAttributes(ds);
            int classCount = ds.DataStoreInfo.DecisionInfo.NumberOfValues;

            DecTrees.DecisionTree tree = new DecTrees.DecisionTree(attributes, classCount);
            ID3Learning id3learning = new ID3Learning(tree);

            Codification codebook = new Codification(data);
            DataTable symbols = codebook.Apply(data);
            int[][] inputs = symbols.ToArray<int>(this.AttributeNames(ds));
            int[] outputs = symbols.ToArray<int>(this.DecisionName(ds));

            DataTable symbols_tst = codebook.Apply(data_tst);
            int[][] inputs_tst = symbols_tst.ToArray<int>(this.AttributeNames(ds));
            int[] outputs_tst = symbols_tst.ToArray<int>(this.DecisionName(ds));

            // Learn the training instances!
            id3learning.Run(inputs, outputs);

            string[] conditionalAttributes = this.AttributeNames(ds);
            string decisionName = this.DecisionName(ds);

            //this.PrintTree(tree.Root, 2, 0, codebook, decisionName);

            int count = 0;
            int correct = 0;
            foreach (DataRow row in data.Rows)
            {
                var rec = row.ToArray<string>(conditionalAttributes);
                string actual = row[decisionName].ToString();
                int[] query = codebook.Translate(conditionalAttributes, rec);
                int output = tree.Decide(query);
                string answer = codebook.Translate(decisionName, output);

                count++;
                if (actual == answer)
                    correct++;
            }

            Console.WriteLine("Accuracy train: {0:0.0000}", (double)correct / (double)count);

            count = 0;
            correct = 0;
            foreach (DataRow row in data_tst.Rows)
            {
                var rec = row.ToArray<string>(conditionalAttributes);
                string actual = row[decisionName].ToString();
                int[] query = codebook.Translate(conditionalAttributes, rec);
                int output = tree.Decide(query);
                string answer = codebook.Translate(decisionName, output);

                count++;
                if (actual == answer)
                    correct++;
            }

            Console.WriteLine("Accuracy test: {0:0.0000}", (double)correct / (double)count);
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