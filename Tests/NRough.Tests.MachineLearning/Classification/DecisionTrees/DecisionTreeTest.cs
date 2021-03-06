﻿// 
// NRough Framework
// http://www.nrough.net
// 
// Copyright (c) Sebastian Widz, 2017
// authors at nrough.net
// 
// This file is part of NRough Framework.
// 
// NRough Framework is free software; you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation; either version 3 of the License, or
// (at your option) any later version.
// 
// NRough Framework is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with NRough Framework; if not, write to the Free Software
// Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA  02110-1301  USA
//

using System;
using System.Linq;
using NRough.Data;
using NRough.Core;
using NUnit.Framework;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using NRough.MachineLearning.Classification.DecisionTrees;
using NRough.MachineLearning.Classification.DecisionTrees.Pruning;
using NRough.MachineLearning.Classification.DecisionLookup;
using NRough.MachineLearning.Classification;
using NRough.MachineLearning.Roughsets;

namespace NRough.Tests.MachineLearning.Classification.DecisionTrees
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
            
            DataStore data = DataStore.Load(trainFile, DataFormat.RSES1);
            foreach (var attribute in data.DataStoreInfo.Attributes) attribute.IsNumeric = false;
            DataStore test = DataStore.Load(testFile, DataFormat.RSES1, data.DataStoreInfo);
            int[] attributes = data.DataStoreInfo.SelectAttributeIds(a => a.IsStandard).ToArray();

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

            Trace.WriteLine(Classifier.Default.Classify(treeRed, test, null));

            //-------------------------------------------------

            IReduct reduct = treeRed.Reduct;

            ReductStoreCollection reductStoreCollection = new ReductStoreCollection(1);
            ReductStore reductStore = new ReductStore(1);
            reductStore.AddReduct(reduct);
            reductStoreCollection.AddStore(reductStore);

            RoughClassifier classifier = new RoughClassifier(reductStoreCollection, RuleQualityAvgMethods.SupportW, RuleQualityMethods.SupportW, data.DataStoreInfo.GetDecisionValues());
            ClassificationResult result2 = classifier.Classify(test, null);
            result2.Epsilon = epsilon;
            result2.Gamma = reductEpsilon;
            result2.ModelName = "Reduct";
            result2.NumberOfRules = reduct.EquivalenceClasses.Count;
            Trace.WriteLine(result2);

            //-------------------------------------------------

            DecisionLookupMajority decTabMaj = new DecisionLookupMajority();            
            decTabMaj.Learn(data, reduct.Attributes.ToArray());
            ClassificationResult resultMaj = Classifier.Default.Classify(decTabMaj, test);

            Trace.WriteLine(resultMaj);
            //-------------------------------------------------

            DecisionLookupLocal decTabLoc = new DecisionLookupLocal();            
            decTabLoc.Learn(data, reduct.Attributes.ToArray());
            ClassificationResult resultLoc = Classifier.Default.Classify(decTabLoc, test);
            
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

            DataStore data = DataStore.Load(trainFile, DataFormat.RSES1);
            foreach (var attribute in data.DataStoreInfo.Attributes) attribute.IsNumeric = false;
            DataStore test = DataStore.Load(testFile, DataFormat.RSES1, data.DataStoreInfo);
            int[] attributes = data.DataStoreInfo.SelectAttributeIds(a => a.IsStandard).ToArray();

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
                
                Trace.WriteLine(Classifier.Default.Classify(treeRed, test, null));

                //-------------------------------------------------

                IReduct reduct = treeRed.Reduct;
                
                ReductStoreCollection reductStoreCollection = new ReductStoreCollection(1);
                ReductStore reductStore = new ReductStore(1);
                reductStore.AddReduct(reduct);
                reductStoreCollection.AddStore(reductStore);

                RoughClassifier classifier = new RoughClassifier(
                    reductStoreCollection, 
                    RuleQualityAvgMethods.SupportW, RuleQualityMethods.SupportW, 
                    data.DataStoreInfo.GetDecisionValues());

                ClassificationResult result2 = classifier.Classify(test, null);
                result2.Epsilon = epsilon;
                result2.Gamma = reductEpsilon;
                result2.ModelName = "Reduct";
                result2.NumberOfRules = reduct.EquivalenceClasses.Count;
                Trace.WriteLine(result2);

                //-------------------------------------------------

                DecisionLookupMajority decTabMaj = new DecisionLookupMajority();
                decTabMaj.Learn(data, reduct.Attributes.ToArray());
                Trace.WriteLine(Classifier.Default.Classify(decTabMaj, test));

                //-------------------------------------------------

                DecisionLookupLocal decTabLoc = new DecisionLookupLocal();
                decTabLoc.Learn(data, reduct.Attributes.ToArray());
                Trace.WriteLine(Classifier.Default.Classify(decTabLoc, test));


                //-------------------------------------------------            

                DecisionTreeC45 treeC45R = new DecisionTreeC45();
                if (epsilon >= 0)
                    treeC45R.Gamma = epsilon;
                treeC45R.PruningType = pruningType;
                treeC45R.Learn(data, reduct.Attributes.ToArray());

                var result6 = Classifier.Default.Classify(treeC45R, test, null);
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

                Trace.WriteLine(Classifier.Default.Classify(treeRough, test, null));
                //-------------------------------------------------

                DecisionTreeC45 treeC45 = new DecisionTreeC45();
                if (epsilon >= 0)
                    treeC45.Gamma = epsilon;
                treeC45.PruningType = pruningType;
                treeC45.Learn(data, attributes);

                Trace.WriteLine(Classifier.Default.Classify(treeC45, test, null));
            }
        }

        [Test, Repeat(1)]
        public void DecisionTreeRoughForNumericAttributeTest()
        {            
            int numOfFolds = 5;
            DataStore data = DataStore.Load(@"Data\german.data", DataFormat.CSV);
            DataStore train = null, test = null;
            
            for (double eps = 0.0; eps < 0.5; eps += 0.01)
            {
                double error = 0;
                DataSplitter splitter = new DataSplitter(data, numOfFolds);
                for (int f = 0; f < numOfFolds; f++)
                {                    
                    splitter.Split(out train, out test, f);

                    DecisionTreeRough tree = new DecisionTreeRough();
                    tree.Gamma = eps;
                    tree.Learn(train, train.DataStoreInfo.SelectAttributeIds(a => a.IsStandard).ToArray());

                    ClassificationResult result = Classifier.Default.Classify(tree, test);
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
            DataStore data = DataStore.Load(@"Data\german.data", DataFormat.CSV);
            DataStore train = null, tmp = null, prune = null, test = null;

            double error = 0;
            DataSplitter splitter = new DataSplitter(data, numOfFolds);
            for (int f = 0; f < numOfFolds; f++)
            {                
                splitter.Split(out tmp, out test, f);

                DataSplitter splitter2 = new DataSplitterRatio(tmp, 0.5);
                splitter2.Split(out train, out prune);

                DecisionTreeC45 tree = new DecisionTreeC45();
                tree.Learn(train, train.DataStoreInfo.SelectAttributeIds(a => a.IsStandard).ToArray());
                ErrorBasedPruning pruning = new ErrorBasedPruning(tree, prune);
                pruning.Prune();

                ClassificationResult result = Classifier.Default.Classify(tree, test);
                Console.WriteLine(result);

                error += result.Error;
            }

            Console.WriteLine("Error: {0}", error / (double)numOfFolds);                        
        }

        [Test, Repeat(1)]
        public void DecisionTreeC45NoPruningForNumericAttributeTest()
        {
            int numOfFolds = 5;
            DataStore data = DataStore.Load(@"Data\german.data", DataFormat.CSV);
            DataStore train = null, test = null;
            double error = 0;
            DataSplitter splitter = new DataSplitter(data, numOfFolds);
            for (int f = 0; f < numOfFolds; f++)
            {                
                splitter.Split(out train, out test, f);

                DecisionTreeC45 tree = new DecisionTreeC45();
                tree.Learn(train, train.DataStoreInfo.SelectAttributeIds(a => a.IsStandard).ToArray());

                ClassificationResult result = Classifier.Default.Classify(tree, test);

                error += result.Error;
            }

            Console.WriteLine("Error: {0}", error / (double)numOfFolds);

        }

        [Test, Repeat(1)]
        public void DecisionForestForNumericAttributeTest()
        {
            int numOfFolds = 5;
            DataStore data = DataStore.Load(@"Data\german.data", DataFormat.CSV);
            DataStore train = null, test = null;

            DataSplitter splitter = new DataSplitter(data, numOfFolds);
            for (int f = 0; f < numOfFolds; f++)
            {                
                splitter.Split(out train, out test, f);

                DecisionForestRandom<DecisionTreeC45> forest = new DecisionForestRandom<DecisionTreeC45>();
                forest.NumberOfAttributesToCheckForSplit = 3;
                forest.Size = 50;
                forest.Learn(train, train.DataStoreInfo.SelectAttributeIds(a => a.IsStandard).ToArray());

                ClassificationResult result = Classifier.Default.Classify(forest, test);
                Console.WriteLine(result);
            }            
        }

        [Test, Repeat(1)]
        public void DecisionForestRoughForNumericAttributeTest()
        {
            int numOfFolds = 5;
            DataStore data = DataStore.Load(@"Data\german.data", DataFormat.CSV);
            DataStore train = null, test = null;

            DataSplitter splitter = new DataSplitter(data, numOfFolds);
            for (int f = 0; f < numOfFolds; f++)
            {                
                splitter.Split(out train, out test, f);

                DecisionForestRandom<DecisionTreeRough> forest = new DecisionForestRandom<DecisionTreeRough>();
                forest.NumberOfAttributesToCheckForSplit = 3;
                forest.Size = 50;
                forest.Gamma = 0.22;
                forest.Learn(train, train.DataStoreInfo.SelectAttributeIds(a => a.IsStandard).ToArray());

                ClassificationResult result = Classifier.Default.Classify(forest, test);
                Console.WriteLine(result);
            }
        }

        [Test, Repeat(1)]
        public void TreeLearnPerformanceTest()
        {
            ConfigManager.MaxDegreeOfParallelism = Environment.ProcessorCount;

            string path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            path = Path.Combine(path, "Data", "dna_modified.trn");

            DataStore data = DataStore.Load(path, DataFormat.RSES1);
            foreach (var attribute in data.DataStoreInfo.Attributes) attribute.IsNumeric = false;

            int total = 20;
            long sum = 0;
            for (int i = 0; i < total; i++)
            {   
                int[] attributes = data.DataStoreInfo.SelectAttributeIds(a => a.IsStandard).ToArray();

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

            var data = Data.Benchmark.Factory.DnaModifiedTrain();
            //DataStore data = DataStore.Load(@"Data\dna_modified.trn", DataFormat.RSES1);

            foreach (var attribute in data.DataStoreInfo.Attributes) attribute.IsNumeric = false;
            DataStore test = DataStore.Load(@"Data\dna_modified.tst", DataFormat.RSES1, data.DataStoreInfo);

            DecisionTreeID3 treeID3 = new DecisionTreeID3();
            treeID3.Learn(data, data.DataStoreInfo.SelectAttributeIds(a => a.IsStandard).ToArray());
            Assert.Greater(DecisionTreeHelper.CountLeaves(treeID3.Root), 0);
        }

        [Test]
        public void GetRulesFromTreeTest()
        {
            var data = Data.Benchmark.Factory.DnaModifiedTrain();
            //DataStore data = DataStore.Load(@"Data\dna_modified.trn", DataFormat.RSES1);

            foreach (var attribute in data.DataStoreInfo.Attributes) attribute.IsNumeric = false;
            int[] attributes = data.DataStoreInfo.SelectAttributeIds(a => a.IsStandard).ToArray();
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
            var data = Data.Benchmark.Factory.DnaModifiedTrain();
            //DataStore data = DataStore.Load(@"Data\dna_modified.trn", DataFormat.RSES1);

            foreach (var attribute in data.DataStoreInfo.Attributes) attribute.IsNumeric = false;
            DataStore test = DataStore.Load(@"Data\dna_modified.tst", DataFormat.RSES1, data.DataStoreInfo);
            int[] attributes = data.DataStoreInfo.SelectAttributeIds(a => a.IsStandard).ToArray();

            for (double eps = 0.0; eps < 1.0; eps += 0.01)
            {
                DecisionTreeID3 treeID3 = new DecisionTreeID3();
                treeID3.Gamma = eps;
                double errorTrain = treeID3.Learn(data, attributes).Error;
                double errorTest = Classifier.Default.Classify(treeID3, test).Error;

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

            var data = Data.Benchmark.Factory.DnaModifiedTrain();
            //DataStore data = DataStore.Load(@"Data\dna_modified.trn", DataFormat.RSES1);

            foreach (var attribute in data.DataStoreInfo.Attributes) attribute.IsNumeric = false;
            DataStore test = DataStore.Load(@"Data\dna_modified.tst", DataFormat.RSES1, data.DataStoreInfo);

            DecisionTreeID3 treeID3 = new DecisionTreeID3();
            treeID3.Gamma = 0;
            treeID3.Learn(data, data.DataStoreInfo.SelectAttributeIds(a => a.IsStandard).ToArray());

            //Console.WriteLine(DecisionTreeFormatter.Construct(treeID3.Root, data, 2));
            Console.WriteLine(Classifier.Default.Classify(treeID3, data, null));
            Console.WriteLine(Classifier.Default.Classify(treeID3, test, null));
        }

        [Test, Repeat(1)]
        public void C45LearnTest()
        {            
            Console.WriteLine("C45LearnTest");

            var data = Data.Benchmark.Factory.DnaModifiedTrain();
            //DataStore data = DataStore.Load(@"Data\dna_modified.trn", DataFormat.RSES1);

            foreach (var attribute in data.DataStoreInfo.Attributes) attribute.IsNumeric = false;
            DataStore test = DataStore.Load(@"Data\dna_modified.tst", DataFormat.RSES1, data.DataStoreInfo);            

            DecisionTreeC45 treeC45 = new DecisionTreeC45();
            treeC45.MaxHeight = 3;
            treeC45.Learn(data, data.DataStoreInfo.SelectAttributeIds(a => a.IsStandard).ToArray());

            Console.WriteLine(DecisionTreeFormatter.Construct(treeC45.Root, data.DataStoreInfo));
            
            //Console.WriteLine(DecisionTreeFormatter.Construct(treeC45.Root, data, 2));
            Console.WriteLine(Classifier.Default.Classify(treeC45, data, null));
            Console.WriteLine(Classifier.Default.Classify(treeC45, test, null));
        }

        [Test, Repeat(1)]
        public void CARTLearnTest()
        {
            Console.WriteLine("CARTLearnTest");

            var data = Data.Benchmark.Factory.DnaModifiedTrain();
            //DataStore data = DataStore.Load(@"Data\dna_modified.trn", DataFormat.RSES1);

            foreach (var attribute in data.DataStoreInfo.Attributes) attribute.IsNumeric = false;
            DataStore test = DataStore.Load(@"Data\dna_modified.tst", DataFormat.RSES1, data.DataStoreInfo);

            DataStore train = null, validation = null;
            DataSplitter splitter = new DataSplitterRatio(data, 0.5);
            splitter.Split(out train, out validation);

            DecisionTreeCART treeCART = new DecisionTreeCART();
            treeCART.Gamma = 0;
            treeCART.Learn(train, train.DataStoreInfo.SelectAttributeIds(a => a.IsStandard).ToArray());

            ReducedErrorPruning prunning = new ReducedErrorPruning(treeCART, validation);

            //Console.WriteLine(DecisionTreeFormatter.Construct(treeCART.Root, data, 2));
            //Console.WriteLine(Classifier.DefaultClassifer.Classify(treeCART, data, null));
            Console.WriteLine(Classifier.Default.Classify(treeCART, test, null));
        }

        [Test, Repeat(1)]
        public void RoughTreeLearnTest()
        {
            Console.WriteLine("RoughTreeLearnTest");

            int maxHeight = -1;

            var data = Data.Benchmark.Factory.DnaModifiedTrain();
            //DataStore data = DataStore.Load(@"Data\dna_modified.trn", DataFormat.RSES1);

            foreach (var attribute in data.DataStoreInfo.Attributes) attribute.IsNumeric = false;
            DataStore test = DataStore.Load(@"Data\dna_modified.tst", DataFormat.RSES1, data.DataStoreInfo);

            DataStore prune = null, train = null;
            DataSplitter splitter = new DataSplitterRatio(data, 0.5);
            splitter.Split(out train, out prune);

            DecisionTreeRough treeRough = new DecisionTreeRough();
            treeRough.MaxHeight = maxHeight;
            treeRough.Learn(train, train.DataStoreInfo.SelectAttributeIds(a => a.IsStandard).ToArray());
            ReducedErrorPruning pruning = new ReducedErrorPruning(treeRough, prune);
            pruning.Prune();

            Console.WriteLine(DecisionTreeFormatter.Construct(treeRough.Root, data.DataStoreInfo));
            Console.WriteLine(DecisionTreeMetric.GetNumberOfRules(treeRough));
            Console.WriteLine(Classifier.Default.Classify(treeRough, data, null));
            Console.WriteLine(Classifier.Default.Classify(treeRough, test, null));

            Console.WriteLine("C45LearnTest");

            DecisionTreeC45 treeC45 = new DecisionTreeC45();
            treeC45.MaxHeight = maxHeight;
            treeC45.Learn(train, train.DataStoreInfo.SelectAttributeIds(a => a.IsStandard).ToArray());
            ErrorBasedPruning pruning2 = new ErrorBasedPruning(treeC45, prune);
            pruning2.Prune();

            Console.WriteLine(DecisionTreeFormatter.Construct(treeC45.Root, data.DataStoreInfo));
            Console.WriteLine(DecisionTreeMetric.GetNumberOfRules(treeC45));
            Console.WriteLine(Classifier.Default.Classify(treeC45, data, null));
            Console.WriteLine(Classifier.Default.Classify(treeC45, test, null));
        }                     

        
    }
}