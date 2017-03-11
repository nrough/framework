using NRough.Data;
using NRough.MachineLearning.Classification.DecisionTrees;
using NRough.MachineLearning.Classification.DecisionTrees.Pruning;
using NRough.Core;
using NUnit.Framework;
using System;
using System.Linq;
using NRough.MachineLearning.Classification;
using NRough.MachineLearning.Permutations;
using NRough.MachineLearning.Roughsets;
using NRough.MachineLearning.Evaluation;
using NRough.MachineLearning;
using NRough.Core.CollectionExtensions;

namespace NRough.Tests.MachineLearning.Classification.DecisionTrees
{
    [TestFixture]
    public class ObliviousDecisionTreeTest
    {
        public static void PrintTree(IModel model, DataStore data)
        {
            IDecisionTree tree = (IDecisionTree)model;
            Console.WriteLine(DecisionTreeFormatter.Construct(tree.Root, data.DataStoreInfo));
            Console.WriteLine("Number of rules: {0}", DecisionTreeMetric.GetNumberOfRules(tree));
        }

        [Test]
        public void Learn2Test()
        {
            var data = DataStore.Load(@"Data\nursery.2.data", DataFormat.RSES1);
            var tree = new ObliviousDecisionTree();
            var cv = new CrossValidation(data);
            cv.PostLearningMethod = ObliviousDecisionTreeTest.PrintTree;
            var result = cv.Run<ObliviousDecisionTree>(tree);
            Console.WriteLine(result);
        }

        [Test]
        public void TestObliviousTreeDepth()
        {
            DataStore data = DataStore.Load(@"Data\spect.train", DataFormat.RSES1);
            foreach (var attribute in data.DataStoreInfo.Attributes) attribute.IsNumeric = false;
            DataStore test = DataStore.Load(@"Data\spect.test", DataFormat.RSES1, data.DataStoreInfo);

            int[] attributes = new int[] { 19, 20, 16, 21, 17, 14, 8, 18, 9, 2, 13 };

            ObliviousDecisionTree treeOblivMaj = new ObliviousDecisionTree("Oblivious");
            treeOblivMaj.RankedAttributes = false;
            treeOblivMaj.UseLocalOutput = false;
            treeOblivMaj.Learn(data, attributes);

            Console.WriteLine(DecisionTreeFormatter.Construct(treeOblivMaj));

            ClassificationResult.OutputColumns = @"ds;m;t;eps;ens;acc;attr;numrul;dthm;dtha;gamma";
            Console.WriteLine(ClassificationResult.TableHeader());

            var treeOblivMajResult = Classifier.Default.Classify(treeOblivMaj, test);
            Console.WriteLine(treeOblivMajResult);

        }

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

            DataStore data = DataStore.Load(trainFile, DataFormat.RSES1);
            foreach (var attribute in data.DataStoreInfo.Attributes) attribute.IsNumeric = false;
            DataStore test = DataStore.Load(testFile, DataFormat.RSES1, data.DataStoreInfo);
            int[] attributes = data.DataStoreInfo.SelectAttributeIds(a => a.IsStandard).ToArray();                        

            ObliviousDecisionTree obiliviousTree = new ObliviousDecisionTree();
            if (epsilon >= 0)
                obiliviousTree.Gamma = epsilon;
            Console.WriteLine(obiliviousTree.Learn(data, attributes));
            Console.WriteLine(Classifier.Default.Classify(obiliviousTree, test));
            //Console.WriteLine(DecisionTreeFormatter.Construct(obiliviousTree));

            Console.WriteLine("Obilivious Decision Tree with Pruning");

            ErrorBasedPruning pruning = new ErrorBasedPruning(obiliviousTree, test);            
            pruning.Prune();

            Console.WriteLine(Classifier.Default.Classify(obiliviousTree, data));
            Console.WriteLine(Classifier.Default.Classify(obiliviousTree, test));


            Console.WriteLine("M Decision Tree based on Reduct");

            DecisionTreeReduct treeRed = new DecisionTreeReduct();
            if (epsilon >= 0)
                treeRed.Gamma = epsilon;
            treeRed.ReductEpsilon = reductEpsilon;
            treeRed.ReductIterations = numOfReducts;
            treeRed.Learn(data, attributes);

            Console.WriteLine(Classifier.Default.Classify(treeRed, data, null));
            Console.WriteLine(Classifier.Default.Classify(treeRed, test, null));

            IReduct reduct = treeRed.Reduct;

            Console.WriteLine("Obilivious Decision Tree based on Reduct");

            ObliviousDecisionTree obiliviousTree2 = new ObliviousDecisionTree();
            if (epsilon >= 0)
                obiliviousTree2.Gamma = epsilon;
            Console.WriteLine(obiliviousTree2.Learn(data, reduct.Attributes.ToArray()));
            Console.WriteLine(Classifier.Default.Classify(obiliviousTree2, test));

            Console.WriteLine("Obilivious Decision Tree based on Reduct after Pruning");

            ErrorBasedPruning pruning2 = new ErrorBasedPruning(obiliviousTree2, test);            
            pruning2.Prune();

            Console.WriteLine(Classifier.Default.Classify(obiliviousTree2, data));
            Console.WriteLine(Classifier.Default.Classify(obiliviousTree2, test));

            Console.WriteLine("C45 Decision Tree");

            DecisionTreeC45 c45 = new DecisionTreeC45();
            if (epsilon >= 0)
                c45.Gamma = epsilon;
            Console.WriteLine(c45.Learn(data, attributes));
            Console.WriteLine(Classifier.Default.Classify(c45, test));

            Console.WriteLine("C45 Decision Tree with Prunning");

            DecisionTreeC45 c45p = new DecisionTreeC45();
            c45p.PruningType = PruningType.ErrorBasedPruning;
            c45p.PruningObjective = PruningObjectiveType.MinimizeError;
            if (epsilon >= 0)
                c45p.Gamma = epsilon;
            Console.WriteLine(c45p.Learn(data, attributes));
            Console.WriteLine(Classifier.Default.Classify(c45p, test));            
        }

        [Test]
        [TestCase(@"Data\dna_modified.trn", @"Data\dna_modified.tst")]
        public void AttributeOrderingTest(string trainFile, string testFile)
        {
            double epsilon = -1;
            double reductEpsilon = 0.05;
            int numOfReducts = 100;

            DataStore data = DataStore.Load(trainFile, DataFormat.RSES1);
            foreach (var attribute in data.DataStoreInfo.Attributes) attribute.IsNumeric = false;
            DataStore test = DataStore.Load(testFile, DataFormat.RSES1, data.DataStoreInfo);
            int[] attributes = data.DataStoreInfo.SelectAttributeIds(a => a.IsStandard).ToArray();

            ObliviousDecisionTree obiliviousTree = new ObliviousDecisionTree();
            if (epsilon >= 0)
                obiliviousTree.Gamma = epsilon;
            var result = obiliviousTree.Learn(data, attributes);
            Console.WriteLine(Classifier.Default.Classify(obiliviousTree, test));

            DecisionTreeReduct treeRed = new DecisionTreeReduct();
            if (epsilon >= 0)
                treeRed.Gamma = epsilon;
            treeRed.ReductEpsilon = reductEpsilon;
            treeRed.ReductIterations = numOfReducts;
            var result2 = treeRed.Learn(data, attributes);            

            IReduct reduct = treeRed.Reduct;
            Console.WriteLine(Classifier.Default.Classify(treeRed, test));

            PermutationCollection permutations = new PermutationCollection(5, reduct.Attributes.ToArray());
            foreach (var perm in permutations)
            {
                Console.WriteLine(perm.ToArray().ToStr());

                ObliviousDecisionTree obiliviousTree_Ordered = new ObliviousDecisionTree();
                if (epsilon >= 0)
                    obiliviousTree_Ordered.Gamma = epsilon;
                obiliviousTree_Ordered.RankedAttributes = true;
                obiliviousTree_Ordered.Learn(data, perm.ToArray());

                var result_Ordered = Classifier.Default.Classify(obiliviousTree_Ordered, test);                
                Console.WriteLine(result_Ordered);
                //Console.WriteLine(DecisionTreeFormatter.Construct(obiliviousTree_Ordered));
            }            
        }
    }
}
