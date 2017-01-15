﻿using Infovision.MachineLearning.Classification.DecisionTrees.Pruning;
using Infovision.Data;
using Infovision.MachineLearning.Classification.DecisionTrees;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Infovision.MachineLearning.Classification;

namespace Infovision.MachineLearning.Tests.Classification.DecisionTrees
{
    [TestFixture]
    class DecisionTreeComparisonTest
    {
        [Test, Repeat(1)]
        public void DecisionTreeRough_GermanCredit()
        {
            Console.WriteLine("DecisionTreeRoughFor_GermanCredit");

            int numOfFolds = 5;
            DataStore data = DataStore.Load(@"Data\german.data", FileFormat.Csv);
            int[] attributes = data.DataStoreInfo.GetFieldIds(FieldTypes.Standard).ToArray();
            DataStore train = null, test = null;

            for (double eps = 0.0; eps < 0.3; eps += 0.01)
            {
                double error = 0;
                DataStoreSplitter splitter = new DataStoreSplitter(data, numOfFolds);
                for (int f = 0; f < numOfFolds; f++)
                {                    
                    splitter.Split(ref train, ref test, f);

                    DecisionTreeRough tree = new DecisionTreeRough();
                    tree.Gamma = eps;
                    tree.Learn(train, attributes);

                    ClassificationResult result = Classifier.DefaultClassifer.Classify(tree, test);
                    Console.WriteLine(result);

                    error += result.Error;
                }

                Console.WriteLine("{0} Error: {1}", eps, error / (double)numOfFolds);
            }
        }

        [Test, Repeat(1)]
        public void DecisionTreeC45_GermanCredit()
        {
            Console.WriteLine("DecisionTreeC45_GermanCredit");

            int numOfFolds = 10;
            DataStore data = DataStore.Load(@"Data\german.data", FileFormat.Csv);
            DataStore train = null, test = null;

            double error = 0;
            DataStoreSplitter splitter = new DataStoreSplitter(data, numOfFolds);
            for (int f = 0; f < numOfFolds; f++)
            {                
                splitter.Split(ref train, ref test, f);                

                DecisionTreeC45 tree = new DecisionTreeC45();
                tree.PruningType = PruningType.ErrorBasedPruning;
                tree.Learn(train, train.DataStoreInfo.GetFieldIds(FieldTypes.Standard).ToArray());

                ClassificationResult result = Classifier.DefaultClassifer.Classify(tree, test);
                Console.WriteLine(result);

                error += result.Error;
            }

            Console.WriteLine("Error: {0}", error / (double)numOfFolds);
        }

        [Test, Repeat(1)]
        public void DecisionTreeC45NoPruning_GermanCredit()
        {
            Console.WriteLine("DecisionTreeC45NoPruning_GermanCredit");

            int numOfFolds = 10;
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
                Console.WriteLine(result);

                error += result.Error;
            }

            Console.WriteLine("Error: {0}", error / (double)numOfFolds);

        }

        [Test, Repeat(1)]
        public void DecisionForestC45_GermanCredit()
        {
            Console.WriteLine("DecisionForestC45_GermanCredit");

            int numOfFolds = 5;
            DataStore data = DataStore.Load(@"Data\german.data", FileFormat.Csv);
            DataStore train = null, test = null;
            double error = 0;

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

                error += result.Error;
            }

            Console.WriteLine("Error: {0}", error / (double)numOfFolds);
        }

        [Test]
        public void DecisionForestRough_GermanCredit()
        {
            Console.WriteLine("DecisionForestRough_GermanCredit");

            int numOfFolds = 5;
            DataStore data = DataStore.Load(@"Data\german.data", FileFormat.Csv);
            DataStore train = null, test = null;
            double error = 0;
            int[] attributes = data.DataStoreInfo.GetFieldIds(FieldTypes.Standard).ToArray();

            DataStoreSplitter splitter = new DataStoreSplitter(data, numOfFolds);
            for (int f = 0; f < numOfFolds; f++)
            {                
                splitter.Split(ref train, ref test, f);

                DecisionForestRandom<DecisionTreeRough> forest = new DecisionForestRandom<DecisionTreeRough>();
                //forest.NumberOfAttributesToCheckForSplit = 3;
                forest.Size = 100;
                //forest.Gamma = 0.22;
                forest.Learn(train, attributes);

                ClassificationResult result = Classifier.DefaultClassifer.Classify(forest, test);
                Console.WriteLine(result);

                error += result.Error;
            }

            Console.WriteLine("Error: {0}", error / (double)numOfFolds);
        }        
    }


}
