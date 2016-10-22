using Infovision.Data;
using Infovision.Datamining.Roughset.DecisionTrees;
using Infovision.Datamining.Roughset.DecisionTrees.Pruning;
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
        //[TestCase(@"Data\monks-1.train", @"Data\monks-1.test")]
        [TestCase(@"Data\dna_modified.trn", @"Data\dna_modified.tst")]
        public void LearnTest(string trainFile, string testFile)
        {
            double epsilon = -1;
            int numOfReducts = 100;
            double reductEpsilon = 0.0;

            //Trace.Listeners.Add(new TextWriterTraceListener(Console.Out));

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

            ErrorBasedPruning pruning = new ErrorBasedPruning(obiliviousTree, test);
            //ReducedErrorPruning pruning = new ReducedErrorPruning(obiliviousTree, test);
            pruning.Prune();

            Console.WriteLine(Classifier.DefaultClassifer.Classify(obiliviousTree, data));
            Console.WriteLine(Classifier.DefaultClassifer.Classify(obiliviousTree, test));

            DecisionTreeReduct treeRed = new DecisionTreeReduct();
            if (epsilon >= 0)
                treeRed.Epsilon = epsilon;
            treeRed.ReductEpsilon = reductEpsilon;
            treeRed.ReductIterations = numOfReducts;
            treeRed.Learn(data, attributes);

            Console.WriteLine(Classifier.DefaultClassifer.Classify(treeRed, data, null));
            Console.WriteLine(Classifier.DefaultClassifer.Classify(treeRed, test, null));

            IReduct reduct = treeRed.Reduct;

            ObliviousDecisionTree obiliviousTree2 = new ObliviousDecisionTree();
            if (epsilon >= 0)
                obiliviousTree2.Epsilon = epsilon;
            Console.WriteLine(obiliviousTree2.Learn(data, reduct.Attributes.ToArray()));
            Console.WriteLine(Classifier.DefaultClassifer.Classify(obiliviousTree2, test));

            ErrorBasedPruning pruning2 = new ErrorBasedPruning(obiliviousTree2, test);
            //ReducedErrorPruning pruning = new ReducedErrorPruning(obiliviousTree, test);
            pruning2.Prune();

            Console.WriteLine(Classifier.DefaultClassifer.Classify(obiliviousTree2, data));
            Console.WriteLine(Classifier.DefaultClassifer.Classify(obiliviousTree2, test));
        }
    }
}
