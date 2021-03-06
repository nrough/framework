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

using NRough.MachineLearning.Classification.DecisionTrees.Pruning;
using NRough.Data;
using NRough.MachineLearning.Classification.DecisionTrees;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NRough.MachineLearning.Classification;

namespace NRough.Tests.MachineLearning.Classification.DecisionTrees
{
    [TestFixture]
    class DecisionTreeComparisonTest
    {
        [Test, Repeat(1)]
        public void DecisionTreeRough_GermanCredit()
        {
            Console.WriteLine("DecisionTreeRoughFor_GermanCredit");

            int numOfFolds = 5;
            DataStore data = DataStore.Load(@"Data\german.data", DataFormat.CSV);
            int[] attributes = data.DataStoreInfo.SelectAttributeIds(a => a.IsStandard).ToArray();
            DataStore train = null, test = null;

            for (double eps = 0.0; eps < 0.3; eps += 0.01)
            {
                double error = 0;
                DataSplitter splitter = new DataSplitter(data, numOfFolds);
                for (int f = 0; f < numOfFolds; f++)
                {                    
                    splitter.Split(out train, out test, f);

                    DecisionTreeRough tree = new DecisionTreeRough();
                    tree.Gamma = eps;
                    tree.Learn(train, attributes);

                    ClassificationResult result = Classifier.Default.Classify(tree, test);
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
            DataStore data = DataStore.Load(@"Data\german.data", DataFormat.CSV);
            DataStore train = null, test = null;

            double error = 0;
            DataSplitter splitter = new DataSplitter(data, numOfFolds);
            for (int f = 0; f < numOfFolds; f++)
            {                
                splitter.Split(out train, out test, f);                

                DecisionTreeC45 tree = new DecisionTreeC45();
                tree.PruningType = PruningType.ErrorBasedPruning;
                tree.Learn(train, train.DataStoreInfo.SelectAttributeIds(a => a.IsStandard).ToArray());

                ClassificationResult result = Classifier.Default.Classify(tree, test);
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
            DataStore data = DataStore.Load(@"Data\german.data", DataFormat.CSV);
            DataStore train = null, test = null;
            double error = 0;

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

                error += result.Error;
            }

            Console.WriteLine("Error: {0}", error / (double)numOfFolds);
        }

        [Test]
        public void DecisionForestRough_GermanCredit()
        {
            Console.WriteLine("DecisionForestRough_GermanCredit");

            int numOfFolds = 5;
            DataStore data = DataStore.Load(@"Data\german.data", DataFormat.CSV);
            DataStore train = null, test = null;
            double error = 0;
            int[] attributes = data.DataStoreInfo.SelectAttributeIds(a => a.IsStandard).ToArray();

            DataSplitter splitter = new DataSplitter(data, numOfFolds);
            for (int f = 0; f < numOfFolds; f++)
            {                
                splitter.Split(out train, out test, f);

                DecisionForestRandom<DecisionTreeRough> forest = new DecisionForestRandom<DecisionTreeRough>();
                //forest.NumberOfAttributesToCheckForSplit = 3;
                forest.Size = 100;
                //forest.Gamma = 0.22;
                forest.Learn(train, attributes);

                ClassificationResult result = Classifier.Default.Classify(forest, test);
                Console.WriteLine(result);

                error += result.Error;
            }

            Console.WriteLine("Error: {0}", error / (double)numOfFolds);
        }        
    }


}
