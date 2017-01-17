using System;
using System.Linq;
using Infovision.Data;
using Infovision.Core;
using NUnit.Framework;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Infovision.MachineLearning.Classification.DecisionTrees;
using Infovision.MachineLearning.Classification.DecisionTrees.Pruning;
using Infovision.MachineLearning.Classification.DecisionTables;
using Infovision.MachineLearning.Classification;
using Infovision.MachineLearning.Roughset;

namespace Infovision.MachineLearning.Tests.Classification.DecisionTrees
{
    [TestFixture]
    public class DecisionTreeTest
    {       
        [Test, Repeat(1)]
        [TestCase(@"Data\monks-2.train", @"Data\monks-2.test")]        
        public void DecisionTableTest(string trainFile, string testFile)
        {
            int size = 200;
            PruningType pruningType = PruningType.None;

            Trace.Listeners.Add(new TextWriterTraceListener(Console.Out));
            
            DataStore data = DataStore.Load(trainFile, FileFormat.Rses1);
            foreach (var fieldInfo in data.DataStoreInfo.Fields) fieldInfo.IsNumeric = false;
            DataStore test = DataStore.Load(testFile, FileFormat.Rses1, data.DataStoreInfo);
            int[] attributes = data.DataStoreInfo.GetFieldIds(FieldTypes.Standard).ToArray();

            Trace.WriteLine(ClassificationResult.TableHeader());

            for (double eps = -1.0; eps < 0.4; eps += 0.01)
            {
                for (double redeps = 0.1; redeps < 0.25; redeps += 0.01)
                    this.QuickCompare(data, test, attributes, eps, redeps, size, pruningType);

                if (eps < 0)
                    eps = 0.0;
            }
        }

        public bool QuickCompare(
            DataStore data,
            DataStore test,
            int[] attributes,
            double epsilon,
            double reductEpsilon,
            int numOfReducts,
            PruningType pruningType)
        {
            DecisionTreeReduct treeRed = new DecisionTreeReduct();
            if (epsilon >= 0)
                treeRed.Gamma = epsilon;
            treeRed.ReductEpsilon = reductEpsilon;
            treeRed.ReductIterations = numOfReducts;
            treeRed.PruningType = pruningType;
            treeRed.Learn(data, attributes);

            Trace.WriteLine(Classifier.DefaultClassifer.Classify(treeRed, test, null));

            //-------------------------------------------------

            IReduct reduct = treeRed.Reduct;

            ReductStoreCollection reductStoreCollection = new ReductStoreCollection(1);
            ReductStore reductStore = new ReductStore(1);
            reductStore.AddReduct(reduct);
            reductStoreCollection.AddStore(reductStore);

            RoughClassifier classifier = new RoughClassifier(reductStoreCollection, RuleQualityAvg.SupportW, RuleQuality.SupportW, data.DataStoreInfo.GetDecisionValues());
            ClassificationResult result2 = classifier.Classify(test, null);
            result2.Epsilon = epsilon;
            result2.Gamma = reductEpsilon;
            result2.ModelName = "Reduct";
            result2.NumberOfRules = reduct.EquivalenceClasses.Count;
            Trace.WriteLine(result2);

            //-------------------------------------------------

            DecisionTableMajority decTabMaj = new DecisionTableMajority();            
            decTabMaj.Learn(data, reduct.Attributes.ToArray());
            ClassificationResult resultMaj = Classifier.DefaultClassifer.Classify(decTabMaj, test);

            Trace.WriteLine(resultMaj);
            //-------------------------------------------------

            DecisionTableLocal decTabLoc = new DecisionTableLocal();            
            decTabLoc.Learn(data, reduct.Attributes.ToArray());
            ClassificationResult resultLoc = Classifier.DefaultClassifer.Classify(decTabLoc, test);
            
            Trace.WriteLine(resultLoc);

            //if (resultLoc.Error != resultMaj.Error)
            //{
            //    Trace.WriteLine(DecisionTreeFormatter.Construct(decTabLoc.ObiliviousTree));
            //    return true;
            //}

            return false;
        }


        [Test, Repeat(1)]
        //[TestCase(@"Data\monks-1.train", @"Data\monks-1.test")]
        [TestCase(@"Data\monks-2.train", @"Data\monks-2.test")]
        //[TestCase(@"Data\monks-3.train", @"Data\monks-3.test")]
        //[TestCase(@"Data\dna_modified.trn", @"Data\dna_modified.test")]
        //[TestCase(@"Data\spect.train", @"Data\spect.test")]
        //[TestCase(@"Data\dna.train", @"Data\dna.test")]        
        public void ReductTreeLearnTest(string trainFile, string testFile)
        {           
            int size = 200;
            PruningType pruningType = PruningType.None;

            Trace.Listeners.Add(new TextWriterTraceListener(Console.Out));
            //Trace.Listeners.Add(new TextWriterTraceListener(@"C:\temp\treeComparisonTest.log"));

            DataStore data = DataStore.Load(trainFile, FileFormat.Rses1);
            foreach (var fieldInfo in data.DataStoreInfo.Fields) fieldInfo.IsNumeric = false;
            DataStore test = DataStore.Load(testFile, FileFormat.Rses1, data.DataStoreInfo);
            int[] attributes = data.DataStoreInfo.GetFieldIds(FieldTypes.Standard).ToArray();

            Trace.WriteLine(ClassificationResult.TableHeader());

            //for (double eps = 0.15; eps < 0.25; eps += 0.01)
            //double eps = -1;


            for (double eps = -1.0; eps < 0.4; eps += 0.01)
            {
                this.TreeComparisonTest(data, test, attributes, eps, 0.0, size, false, pruningType);

                for (double redeps = 0.0; redeps < 0.25; redeps += 0.01)                    
                    this.TreeComparisonTest(data, test, attributes, eps, redeps, size, true, pruningType);

                if (eps < 0)
                    eps = 0.0;
            }
        }

        public void TreeComparisonTest(
            DataStore data, 
            DataStore test, 
            int[] attributes, 
            double epsilon, 
            double reductEpsilon, 
            int numOfReducts, 
            bool runReduct,
            PruningType pruningType)
        {
            if (runReduct)
            {
                DecisionTreeReduct treeRed = new DecisionTreeReduct();
                if (epsilon >= 0)
                    treeRed.Gamma = epsilon;
                treeRed.ReductEpsilon = reductEpsilon;
                treeRed.ReductIterations = numOfReducts;
                treeRed.PruningType = pruningType;
                treeRed.Learn(data, attributes);
                
                Trace.WriteLine(Classifier.DefaultClassifer.Classify(treeRed, test, null));

                //-------------------------------------------------

                IReduct reduct = treeRed.Reduct;
                
                ReductStoreCollection reductStoreCollection = new ReductStoreCollection(1);
                ReductStore reductStore = new ReductStore(1);
                reductStore.AddReduct(reduct);
                reductStoreCollection.AddStore(reductStore);

                RoughClassifier classifier = new RoughClassifier(
                    reductStoreCollection, 
                    RuleQualityAvg.SupportW, RuleQuality.SupportW, 
                    data.DataStoreInfo.GetDecisionValues());

                ClassificationResult result2 = classifier.Classify(test, null);
                result2.Epsilon = epsilon;
                result2.Gamma = reductEpsilon;
                result2.ModelName = "Reduct";
                result2.NumberOfRules = reduct.EquivalenceClasses.Count;
                Trace.WriteLine(result2);

                //-------------------------------------------------

                DecisionTableMajority decTabMaj = new DecisionTableMajority();
                decTabMaj.Learn(data, reduct.Attributes.ToArray());
                Trace.WriteLine(Classifier.DefaultClassifer.Classify(decTabMaj, test));

                //-------------------------------------------------

                DecisionTableLocal decTabLoc = new DecisionTableLocal();
                decTabLoc.Learn(data, reduct.Attributes.ToArray());
                Trace.WriteLine(Classifier.DefaultClassifer.Classify(decTabLoc, test));


                //-------------------------------------------------            

                DecisionTreeC45 treeC45R = new DecisionTreeC45();
                if (epsilon >= 0)
                    treeC45R.Gamma = epsilon;
                treeC45R.PruningType = pruningType;
                treeC45R.Learn(data, reduct.Attributes.ToArray());

                var result6 = Classifier.DefaultClassifer.Classify(treeC45R, test, null);
                result6.ModelName = "C45+REDUCT";
                result6.Gamma = reductEpsilon;
                Trace.WriteLine(result6);
            }
            else
            {
                //-------------------------------------------------

                DecisionTreeRough treeRough = new DecisionTreeRough();
                if (epsilon >= 0)
                    treeRough.Gamma = epsilon;
                treeRough.PruningType = pruningType;
                treeRough.Learn(data, attributes);

                Trace.WriteLine(Classifier.DefaultClassifer.Classify(treeRough, test, null));
                //-------------------------------------------------

                DecisionTreeC45 treeC45 = new DecisionTreeC45();
                if (epsilon >= 0)
                    treeC45.Gamma = epsilon;
                treeC45.PruningType = pruningType;
                treeC45.Learn(data, attributes);

                Trace.WriteLine(Classifier.DefaultClassifer.Classify(treeC45, test, null));
            }
        }

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
                    splitter.Split(ref train, ref test, f);

                    DecisionTreeRough tree = new DecisionTreeRough();
                    tree.Gamma = eps;
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
                splitter.Split(ref tmp, ref test, f);

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

        [Test, Repeat(1)]
        public void DecisionTreeC45NoPruningForNumericAttributeTest()
        {
            int numOfFolds = 5;
            DataStore data = DataStore.Load(@"Data\german.data", FileFormat.Csv);
            DataStore train = null, test = null;
            double error = 0;
            DataStoreSplitter splitter = new DataStoreSplitter(data, numOfFolds);
            for (int f = 0; f < numOfFolds; f++)
            {                
                splitter.Split(ref train, ref test, f);

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
                splitter.Split(ref train, ref test, f);

                DecisionForestRandom<DecisionTreeC45> forest = new DecisionForestRandom<DecisionTreeC45>();
                forest.NumberOfAttributesToCheckForSplit = 3;
                forest.Size = 50;
                forest.Learn(train, train.DataStoreInfo.GetFieldIds(FieldTypes.Standard).ToArray());

                ClassificationResult result = Classifier.DefaultClassifer.Classify(forest, test);
                Console.WriteLine(result);
            }            
        }

        [Test, Repeat(1)]
        public void DecisionForestRoughForNumericAttributeTest()
        {
            int numOfFolds = 5;
            DataStore data = DataStore.Load(@"Data\german.data", FileFormat.Csv);
            DataStore train = null, test = null;

            DataStoreSplitter splitter = new DataStoreSplitter(data, numOfFolds);
            for (int f = 0; f < numOfFolds; f++)
            {                
                splitter.Split(ref train, ref test, f);

                DecisionForestRandom<DecisionTreeRough> forest = new DecisionForestRandom<DecisionTreeRough>();
                forest.NumberOfAttributesToCheckForSplit = 3;
                forest.Size = 50;
                forest.Gamma = 0.22;
                forest.Learn(train, train.DataStoreInfo.GetFieldIds(FieldTypes.Standard).ToArray());

                ClassificationResult result = Classifier.DefaultClassifer.Classify(forest, test);
                Console.WriteLine(result);
            }
        }

        [Test, Repeat(1)]
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
                tree.Gamma = 0;
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
                treeID3.Gamma = eps;
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
                treeID3.Gamma = eps;
                double errorTrain = treeID3.Learn(data, attributes).Error;
                double errorTest = Classifier.DefaultClassifer.Classify(treeID3, test).Error;

                /*
                Console.WriteLine("eps={0} numrul={1} errtrn={2} errtst={3}",
                    eps,
                    DecisionTreeHelper.CountLeaves(treeID3.Root),
                    errorTrain,
                    errorTest);
                */
            }
        }

        [Test, Repeat(1)]
        public void ID3LearnTest()
        {
            Console.WriteLine("ID3LearnTest");

            DataStore data = DataStore.Load(@"Data\dna_modified.trn", FileFormat.Rses1);
            foreach (var fieldInfo in data.DataStoreInfo.Fields) fieldInfo.IsNumeric = false;
            DataStore test = DataStore.Load(@"Data\dna_modified.tst", FileFormat.Rses1, data.DataStoreInfo);

            DecisionTreeID3 treeID3 = new DecisionTreeID3();
            treeID3.Gamma = 0;
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
            treeC45.MaxHeight = 3;
            treeC45.Learn(data, data.DataStoreInfo.GetFieldIds(FieldTypes.Standard).ToArray());

            Console.WriteLine(DecisionTreeFormatter.Construct(treeC45.Root, data.DataStoreInfo));
            
            //Console.WriteLine(DecisionTreeFormatter.Construct(treeC45.Root, data, 2));
            Console.WriteLine(Classifier.DefaultClassifer.Classify(treeC45, data, null));
            Console.WriteLine(Classifier.DefaultClassifer.Classify(treeC45, test, null));
        }

        [Test, Repeat(1)]
        public void CARTLearnTest()
        {
            Console.WriteLine("CARTLearnTest");
            DataStore data = DataStore.Load(@"Data\dna_modified.trn", FileFormat.Rses1);
            foreach (var fieldInfo in data.DataStoreInfo.Fields) fieldInfo.IsNumeric = false;
            DataStore test = DataStore.Load(@"Data\dna_modified.tst", FileFormat.Rses1, data.DataStoreInfo);

            DataStore train = null, validation = null;
            DataStoreSplitter splitter = new DataStoreSplitterRatio(data, 0.5);
            splitter.Split(ref train, ref validation);

            DecisionTreeCART treeCART = new DecisionTreeCART();
            treeCART.Gamma = 0;
            treeCART.Learn(train, train.DataStoreInfo.GetFieldIds(FieldTypes.Standard).ToArray());

            ReducedErrorPruning prunning = new ReducedErrorPruning(treeCART, validation);

            //Console.WriteLine(DecisionTreeFormatter.Construct(treeCART.Root, data, 2));
            //Console.WriteLine(Classifier.DefaultClassifer.Classify(treeCART, data, null));
            Console.WriteLine(Classifier.DefaultClassifer.Classify(treeCART, test, null));
        }

        [Test, Repeat(1)]
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

            Console.WriteLine(DecisionTreeFormatter.Construct(treeRough.Root, data.DataStoreInfo));
            Console.WriteLine(DecisionTreeMetric.GetNumberOfRules(treeRough));
            Console.WriteLine(Classifier.DefaultClassifer.Classify(treeRough, data, null));
            Console.WriteLine(Classifier.DefaultClassifer.Classify(treeRough, test, null));

            Console.WriteLine("C45LearnTest");

            DecisionTreeC45 treeC45 = new DecisionTreeC45();
            treeC45.MaxHeight = maxHeight;
            treeC45.Learn(train, train.DataStoreInfo.GetFieldIds(FieldTypes.Standard).ToArray());
            ErrorBasedPruning pruning2 = new ErrorBasedPruning(treeC45, prune);
            pruning2.Prune();

            Console.WriteLine(DecisionTreeFormatter.Construct(treeC45.Root, data.DataStoreInfo));
            Console.WriteLine(DecisionTreeMetric.GetNumberOfRules(treeC45));
            Console.WriteLine(Classifier.DefaultClassifer.Classify(treeC45, data, null));
            Console.WriteLine(Classifier.DefaultClassifer.Classify(treeC45, test, null));
        }                     

        
    }
}