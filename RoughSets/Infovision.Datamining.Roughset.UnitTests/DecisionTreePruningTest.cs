using Infovision.Data;
using Infovision.Datamining.Roughset.DecisionTrees;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Infovision.Datamining.Roughset.DecisionTrees.Pruning;

namespace Infovision.Datamining.Roughset.UnitTests
{
    [TestFixture]
    public class DecisionTreePruningTest
    {
        [Test, Repeat(10)]
        public void ErrorBasedPruningTest()
        {
            DataStore data = DataStore.Load(@"Data\dna_modified.trn", FileFormat.Rses1);

            /*
            foreach (var fieldInfo in data.DataStoreInfo.Fields)
            {
                fieldInfo.IsNumeric = false;
                fieldInfo.IsSymbolic = true;
            }
            */


            DataStore test = DataStore.Load(@"Data\dna_modified.tst", FileFormat.Rses1, data.DataStoreInfo);            

            DataStore train = null, prune = null;
            DataStoreSplitter splitter = new DataStoreSplitterRatio(data, 0.5);
            splitter.Split(ref train, ref prune);            

            DecisionTreeC45 c45WithPruning = new DecisionTreeC45();
            c45WithPruning.Learn(train, train.DataStoreInfo.GetFieldIds(FieldTypes.Standard).ToArray());

            ClassificationResult resultBeforePruning = Classifier.DefaultClassifer.Classify(c45WithPruning, test);
            Console.WriteLine("resultBeforePruning = {0}", resultBeforePruning);
            Console.WriteLine("number of rules: {0}", this.GetNumberOfRules(c45WithPruning));

            ErrorBasedPruning pruning = new ErrorBasedPruning(c45WithPruning, prune);
            pruning.Prune();

            ClassificationResult resultAfterPruning = Classifier.DefaultClassifer.Classify(c45WithPruning, test);
            Console.WriteLine("resultAfterPruning = {0}", resultAfterPruning);
            Console.WriteLine("number of rules: {0}", this.GetNumberOfRules(c45WithPruning));                                              
        }

        [Test]
        public void DecisionTreeC45ForNumericAttributeTest()
        {
            DataStore data = DataStore.Load(@"Data\german.data", FileFormat.Csv);
                        
            DataStore train = null, tmp = null, prune = null, test = null;

            DataStoreSplitter splitter = new DataStoreSplitterRatio(data, 0.33);
            splitter.Split(ref train, ref tmp);

            DataStoreSplitter splitter2 = new DataStoreSplitterRatio(tmp, 0.5);
            splitter2.Split(ref prune, ref test);


            DecisionTreeC45 c45WithPruning = new DecisionTreeC45();
            c45WithPruning.Learn(train, train.DataStoreInfo.GetFieldIds(FieldTypes.Standard).ToArray());

            ClassificationResult resultBeforePruning = Classifier.DefaultClassifer.Classify(c45WithPruning, test);
            Console.WriteLine("resultBeforePruning = {0}", resultBeforePruning);
            Console.WriteLine("number of rules: {0}", this.GetNumberOfRules(c45WithPruning));

            ErrorBasedPruning pruning = new ErrorBasedPruning(c45WithPruning, prune);
            pruning.Prune();

            ClassificationResult resultAfterPruning = Classifier.DefaultClassifer.Classify(c45WithPruning, test);
            Console.WriteLine("resultAfterPruning = {0}", resultAfterPruning);
            Console.WriteLine("number of rules: {0}", this.GetNumberOfRules(c45WithPruning));
        }

        [Test, Repeat(10)]
        public void ReducedErrorPruningTest()
        {
            DataStore data = DataStore.Load(@"Data\dna_modified.trn", FileFormat.Rses1);
            DataStore test = DataStore.Load(@"Data\dna_modified.tst", FileFormat.Rses1, data.DataStoreInfo);

            DataStore train = null, prune = null;
            DataStoreSplitter splitter = new DataStoreSplitterRatio(data, 0.5);
            splitter.Split(ref train, ref prune);

            var c45WithPruning = new DecisionTreeC45();
            c45WithPruning.Learn(train, train.DataStoreInfo.GetFieldIds(FieldTypes.Standard).ToArray());

            var resultBeforePruning = Classifier.DefaultClassifer.Classify(c45WithPruning, test);
            Console.WriteLine("resultBeforePruning = {0}", resultBeforePruning);
            Console.WriteLine("number of rules: {0}", this.GetNumberOfRules(c45WithPruning));

            ReducedErrorPruning reducedErrorPruning = new ReducedErrorPruning(c45WithPruning, prune);
            reducedErrorPruning.Prune();

            var resultAfterPruning = Classifier.DefaultClassifer.Classify(c45WithPruning, test);
            Console.WriteLine("resultAfterPruning = {0}", resultAfterPruning);
            Console.WriteLine("number of rules: {0}", this.GetNumberOfRules(c45WithPruning));
        }

        [Test]
        public void PrePruningTest()
        {
            DataStore data = DataStore.Load(@"Data\dna_modified.trn", FileFormat.Rses1);
            DataStore test = DataStore.Load(@"Data\dna_modified.tst", FileFormat.Rses1, data.DataStoreInfo);

            for (double eps = 0.0; eps < 0.5; eps += 0.01)
            {
                DecisionTreeC45 c45WithPrePruning = new DecisionTreeC45();
                c45WithPrePruning.Epsilon = eps;
                c45WithPrePruning.Learn(data, data.DataStoreInfo.GetFieldIds(FieldTypes.Standard).ToArray());

                ClassificationResult resultForTreeWithPrePruning = Classifier.DefaultClassifer.Classify(c45WithPrePruning, test);
                Console.WriteLine("resultForTreeWithPrePruning = {0}", resultForTreeWithPrePruning);
                Console.WriteLine("number of rules: {0}", this.GetNumberOfRules(c45WithPrePruning));
            }
        }

        [Test, Repeat(20)]
        public void PrePrunningTest2()
        {
            DataStore data = DataStore.Load(@"Data\dna_modified.trn", FileFormat.Rses1);
            DataStore test = DataStore.Load(@"Data\dna_modified.tst", FileFormat.Rses1, data.DataStoreInfo);

            var tree = new DecisionTreeReduct();
            tree.Learn(data, data.DataStoreInfo.GetFieldIds(FieldTypes.Standard).ToArray());

            ClassificationResult resultForTreeWithPrePruning = Classifier.DefaultClassifer.Classify(tree, test);
            Console.WriteLine("DecisionTreeReduct {0}", resultForTreeWithPrePruning);
            Console.WriteLine("number of rules: {0}", this.GetNumberOfRules(tree));

        }

        [Test]
        public void PrePrunningTest3()
        {
            DataStore data = DataStore.Load(@"Data\dna_modified.trn", FileFormat.Rses1);
            DataStore test = DataStore.Load(@"Data\dna_modified.tst", FileFormat.Rses1, data.DataStoreInfo);

            for (double eps = 0.0; eps < 0.5; eps += 0.01)
            {
                var tree = new DecisionTreeRough();
                tree.Epsilon = eps;
                tree.Learn(data, data.DataStoreInfo.GetFieldIds(FieldTypes.Standard).ToArray());

                ClassificationResult resultForTreeWithPrePruning = Classifier.DefaultClassifer.Classify(tree, test);
                Console.WriteLine("DecisionTreeRough {0}", resultForTreeWithPrePruning);
                Console.WriteLine("number of rules: {0}", this.GetNumberOfRules(tree));
            }
        }



        public int GetNumberOfRules(IDecisionTree tree)
        {            
            int count = 0;
            TreeNodeTraversal.TraversePostOrder(tree.Root, x => count = x.IsLeaf ? count + 1 : count);
            return count;
        }
    }
}
