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
            DataStore test = DataStore.Load(@"Data\dna_modified.tst", FileFormat.Rses1, data.DataStoreInfo);

            DataStore train = null, prune = null;
            DataStoreSplitter splitter = new DataStoreSplitterRatio(data, 0.5);
            splitter.Split(ref train, ref prune);

            DecisionTreeC45 c45WithoutPruning = new DecisionTreeC45();
            c45WithoutPruning.Learn(data, data.DataStoreInfo.GetFieldIds(FieldTypes.Standard).ToArray());

            ClassificationResult resultWithoutPruning = Classifier.DefaultClassifer.Classify(c45WithoutPruning, test);
            Console.WriteLine("resultWithoutPruning =  {0}", resultWithoutPruning);
            Console.WriteLine("number of rules: {0}", this.GetNumberOfRules(c45WithoutPruning));

            //===============================================

            DecisionTreeC45 c45WithPruning = new DecisionTreeC45();
            c45WithPruning.Learn(train, train.DataStoreInfo.GetFieldIds(FieldTypes.Standard).ToArray());

            ClassificationResult resultBeforePruning = Classifier.DefaultClassifer.Classify(c45WithPruning, test);
            Console.WriteLine("resultBeforePruning = {0}", resultBeforePruning);
            Console.WriteLine("number of rules: {0}", this.GetNumberOfRules(c45WithPruning));

            ErrorBasedPruning pruning = new ErrorBasedPruning(c45WithPruning, prune);
            pruning.Threshold = 0.1;
            pruning.Run();
            
            ClassificationResult resultAfterPruning = Classifier.DefaultClassifer.Classify(c45WithPruning, test);
            Console.WriteLine("resultAfterPruning = {0}", resultAfterPruning);
            Console.WriteLine("number of rules: {0}", this.GetNumberOfRules(c45WithPruning));

            //===============================================

            DecisionTreeC45 c45WithPrePruning = new DecisionTreeC45();
            c45WithPrePruning.Epsilon = 0.1;
            c45WithPrePruning.Learn(data, data.DataStoreInfo.GetFieldIds(FieldTypes.Standard).ToArray());

            ClassificationResult resultForTreeWithPrePruning = Classifier.DefaultClassifer.Classify(c45WithPrePruning, test);
            Console.WriteLine("resultForTreeWithPrePruning = {0}", resultForTreeWithPrePruning);
            Console.WriteLine("number of rules: {0}", this.GetNumberOfRules(c45WithPrePruning));

            Console.WriteLine();
        }

        public int GetNumberOfRules(IDecisionTree tree)
        {
            int count = 0;
            TreeNodeTraversal.TraversePostOrder(tree.Root, x => count = x.IsLeaf ? count + 1 : count);
            return count;
        }
    }
}
