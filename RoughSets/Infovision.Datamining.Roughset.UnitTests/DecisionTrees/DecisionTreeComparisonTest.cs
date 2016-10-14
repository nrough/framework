using Infovision.Datamining.Roughset.DecisionTrees.Pruning;
using Infovision.Data;
using Infovision.Datamining.Roughset.DecisionTrees;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infovision.Datamining.Roughset.UnitTests.DecisionTrees
{
    [TestFixture]
    class DecisionTreeComparisonTest
    {
        [Test, Repeat(10)]
        public void DecisionTreeRoughFor_GermanCredit()
        {
            Console.WriteLine("DecisionTreeRoughFor_GermanCredit");

            int numOfFolds = 5;
            DataStore data = DataStore.Load(@"Data\german.data", FileFormat.Csv);
            DataStore train = null, test = null;

            for (double eps = 0.0; eps < 0.3; eps += 0.01)
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
                    Console.WriteLine(result);

                    error += result.Error;
                }

                Console.WriteLine("{0} Error: {1}", eps, error / (double)numOfFolds);
            }
        }

        [Test, Repeat(10)]
        public void DecisionTreeC45_GermanCredit()
        {
            Console.WriteLine("DecisionTreeC45_GermanCredit");

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

        [Test, Repeat(10)]
        public void DecisionTreeC45NoPruning_GermanCredit()
        {
            Console.WriteLine("DecisionTreeC45NoPruning_GermanCredit");

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
                Console.WriteLine(result);

                error += result.Error;
            }

            Console.WriteLine("Error: {0}", error / (double)numOfFolds);

        }

        [Test, Repeat(10)]
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
                splitter.ActiveFold = f;
                splitter.Split(ref train, ref test);

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

        [Test, Repeat(10)]
        public void DecisionForestRough_GermanCredit()
        {
            Console.WriteLine("DecisionForestRough_GermanCredit");

            int numOfFolds = 5;
            DataStore data = DataStore.Load(@"Data\german.data", FileFormat.Csv);
            DataStore train = null, test = null;
            double error = 0;

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

                error += result.Error;
            }

            Console.WriteLine("Error: {0}", error / (double)numOfFolds);
        }        
    }


}
