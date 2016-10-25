﻿using Infovision.Data;
using Infovision.Datamining.Roughset.DecisionTrees;
using Infovision.Datamining.Roughset.DecisionTrees.Pruning;
using Infovision.Utils;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infovision.Datamining.Roughset.UnitTests.DecisionTrees
{
    [TestFixture]
    public class ObiliviousDecisionTreeTest
    {
        [Test]
        [TestCase(@"Data\spect.train", @"Data\spect.test")]
        [TestCase(@"Data\monks-1.train", @"Data\monks-1.test")]
        [TestCase(@"Data\monks-2.train", @"Data\monks-2.test")]
        [TestCase(@"Data\monks-3.train", @"Data\monks-3.test")]
        [TestCase(@"Data\dna_modified.trn", @"Data\dna_modified.tst")]
        public void LearnTest(string trainFile, string testFile)
        {
            double epsilon = -1;
            int numOfReducts = 100;
            double reductEpsilon = 0.01;            

            Console.WriteLine("Obilivious Decision Tree");

            DataStore data = DataStore.Load(trainFile, FileFormat.Rses1);
            foreach (var fieldInfo in data.DataStoreInfo.Fields) fieldInfo.IsNumeric = false;
            DataStore test = DataStore.Load(testFile, FileFormat.Rses1, data.DataStoreInfo);
            int[] attributes = data.DataStoreInfo.GetFieldIds(FieldTypes.Standard).ToArray();                        

            ObliviousDecisionTree obiliviousTree = new ObliviousDecisionTree();
            if (epsilon >= 0)
                obiliviousTree.Epsilon = epsilon;
            Console.WriteLine(obiliviousTree.Learn(data, attributes));
            Console.WriteLine(Classifier.DefaultClassifer.Classify(obiliviousTree, test));
            //Console.WriteLine(DecisionTreeFormatter.Construct(obiliviousTree));

            Console.WriteLine("Obilivious Decision Tree with Pruning");

            ErrorBasedPruning pruning = new ErrorBasedPruning(obiliviousTree, test);            
            pruning.Prune();

            Console.WriteLine(Classifier.DefaultClassifer.Classify(obiliviousTree, data));
            Console.WriteLine(Classifier.DefaultClassifer.Classify(obiliviousTree, test));


            Console.WriteLine("M Decision Tree based on Reduct");

            DecisionTreeReduct treeRed = new DecisionTreeReduct();
            if (epsilon >= 0)
                treeRed.Epsilon = epsilon;
            treeRed.ReductEpsilon = reductEpsilon;
            treeRed.ReductIterations = numOfReducts;
            treeRed.Learn(data, attributes);

            Console.WriteLine(Classifier.DefaultClassifer.Classify(treeRed, data, null));
            Console.WriteLine(Classifier.DefaultClassifer.Classify(treeRed, test, null));

            IReduct reduct = treeRed.Reduct;

            Console.WriteLine("Obilivious Decision Tree based on Reduct");

            ObliviousDecisionTree obiliviousTree2 = new ObliviousDecisionTree();
            if (epsilon >= 0)
                obiliviousTree2.Epsilon = epsilon;
            Console.WriteLine(obiliviousTree2.Learn(data, reduct.Attributes.ToArray()));
            Console.WriteLine(Classifier.DefaultClassifer.Classify(obiliviousTree2, test));

            Console.WriteLine("Obilivious Decision Tree based on Reduct after Pruning");

            ErrorBasedPruning pruning2 = new ErrorBasedPruning(obiliviousTree2, test);            
            pruning2.Prune();

            Console.WriteLine(Classifier.DefaultClassifer.Classify(obiliviousTree2, data));
            Console.WriteLine(Classifier.DefaultClassifer.Classify(obiliviousTree2, test));

            Console.WriteLine("C45 Decision Tree");

            DecisionTreeC45 c45 = new DecisionTreeC45();
            if (epsilon >= 0)
                c45.Epsilon = epsilon;
            Console.WriteLine(c45.Learn(data, attributes));
            Console.WriteLine(Classifier.DefaultClassifer.Classify(c45, test));

            Console.WriteLine("C45 Decision Tree with Prunning");

            DecisionTreeC45 c45p = new DecisionTreeC45();
            c45p.PruningType = PruningType.ErrorBasedPruning;
            c45p.PruningObjective = PruningObjectiveType.MinimizeError;
            if (epsilon >= 0)
                c45p.Epsilon = epsilon;
            Console.WriteLine(c45p.Learn(data, attributes));
            Console.WriteLine(Classifier.DefaultClassifer.Classify(c45p, test));            
        }

        [Test]
        [TestCase(@"Data\dna_modified.trn", @"Data\dna_modified.tst")]
        public void AttributeOrderingTest(string trainFile, string testFile)
        {
            double epsilon = -1;
            double reductEpsilon = 0.05;
            int numOfReducts = 100;

            DataStore data = DataStore.Load(trainFile, FileFormat.Rses1);
            foreach (var fieldInfo in data.DataStoreInfo.Fields) fieldInfo.IsNumeric = false;
            DataStore test = DataStore.Load(testFile, FileFormat.Rses1, data.DataStoreInfo);
            int[] attributes = data.DataStoreInfo.GetFieldIds(FieldTypes.Standard).ToArray();

            ObliviousDecisionTree obiliviousTree = new ObliviousDecisionTree();
            if (epsilon >= 0)
                obiliviousTree.Epsilon = epsilon;
            var result = obiliviousTree.Learn(data, attributes);
            Console.WriteLine(Classifier.DefaultClassifer.Classify(obiliviousTree, test));

            DecisionTreeReduct treeRed = new DecisionTreeReduct();
            if (epsilon >= 0)
                treeRed.Epsilon = epsilon;
            treeRed.ReductEpsilon = reductEpsilon;
            treeRed.ReductIterations = numOfReducts;
            var result2 = treeRed.Learn(data, attributes);            

            IReduct reduct = treeRed.Reduct;
            Console.WriteLine(Classifier.DefaultClassifer.Classify(treeRed, test));

            PermutationCollection permutations = new PermutationCollection(5, reduct.Attributes.ToArray());
            foreach (var perm in permutations)
            {
                Console.WriteLine(perm.ToArray().ToStr());

                ObliviousDecisionTree obiliviousTree_Ordered = new ObliviousDecisionTree();
                if (epsilon >= 0)
                    obiliviousTree_Ordered.Epsilon = epsilon;
                obiliviousTree_Ordered.RankedAttributes = true;
                obiliviousTree_Ordered.Learn(data, perm.ToArray());

                var result_Ordered = Classifier.DefaultClassifer.Classify(obiliviousTree_Ordered, test);                
                Console.WriteLine(result_Ordered);
                //Console.WriteLine(DecisionTreeFormatter.Construct(obiliviousTree_Ordered));
            }            
        }
    }
}
