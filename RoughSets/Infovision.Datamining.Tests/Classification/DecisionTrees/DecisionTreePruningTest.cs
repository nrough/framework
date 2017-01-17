using Infovision.Data;
using Infovision.MachineLearning.Classification.DecisionTrees;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Infovision.MachineLearning.Classification.DecisionTrees.Pruning;
using System.Diagnostics;
using Infovision.MachineLearning.Classification;

namespace Infovision.MachineLearning.Tests.Classification.DecisionTrees
{
    [TestFixture]
    public class DecisionTreePruningTest
    {
        [Test, Repeat(1)]
        public void ErrorBasedPruningTest()
        {
            DataStore data = DataStore.Load(@"Data\dna_modified.trn", FileFormat.Rses1);
            
           foreach (var fieldInfo in data.DataStoreInfo.Fields)
              fieldInfo.IsNumeric = false;

            DataStore test = DataStore.Load(@"Data\dna_modified.tst", FileFormat.Rses1, data.DataStoreInfo);

            DataStore train = null, prune = null;
            DataStoreSplitter splitter = new DataStoreSplitterRatio(data, 0.5);
            splitter.Split(ref train, ref prune);

            DecisionTreeC45 c45WithPruning = new DecisionTreeC45();
            c45WithPruning.Learn(train, train.DataStoreInfo.GetFieldIds(FieldTypes.Standard).ToArray());

            ClassificationResult resultBeforePruning = Classifier.DefaultClassifer.Classify(c45WithPruning, test);

            Console.WriteLine("DecisionTreeC45 B4PR ERRBSD {0:0.00} {1:0.00000} {2} {3} {4}",
                    "N/A",
                    resultBeforePruning.Accuracy,
                    DecisionTreeMetric.GetNumberOfRules(c45WithPruning),
                    resultBeforePruning.AvgNumberOfAttributes,
                    DecisionTreeMetric.GetHeight(c45WithPruning));

            //Console.WriteLine("resultBeforePruning = {0}", resultBeforePruning);
            //Console.WriteLine("number of rules: {0}", DecisionTreeBase.GetNumberOfRules(c45WithPruning));

            ErrorBasedPruning pruning = new ErrorBasedPruning(c45WithPruning, prune);
            pruning.Prune();

            ClassificationResult resultAfterPruning = Classifier.DefaultClassifer.Classify(c45WithPruning, test);

            Console.WriteLine("DecisionTreeC45 B4PR ERRBSD {0:0.00} {1:0.00000} {2} {3} {4}",
                    "N/A",
                    resultAfterPruning.Accuracy,
                    DecisionTreeMetric.GetNumberOfRules(c45WithPruning),
                    resultAfterPruning.AvgNumberOfAttributes,
                    DecisionTreeMetric.GetHeight(c45WithPruning));

            //Console.WriteLine("resultAfterPruning = {0}", resultAfterPruning);
            //Console.WriteLine("number of rules: {0}", DecisionTreeBase.GetNumberOfRules(c45WithPruning));

            Console.WriteLine();
        }        

        [Test, Repeat(1)]
        public void ReducedErrorPruningTest()
        {
            DataStore data = DataStore.Load(@"Data\dna_modified.trn", FileFormat.Rses1);

            foreach (var fieldInfo in data.DataStoreInfo.Fields)
                fieldInfo.IsNumeric = false;

            DataStore test = DataStore.Load(@"Data\dna_modified.tst", FileFormat.Rses1, data.DataStoreInfo);

            DataStore train = null, prune = null;
            DataStoreSplitter splitter = new DataStoreSplitterRatio(data, 0.5);
            splitter.Split(ref train, ref prune);

            var c45WithPruning = new DecisionTreeC45();
            c45WithPruning.Learn(train, train.DataStoreInfo.GetFieldIds(FieldTypes.Standard).ToArray());

            var resultBeforePruning = Classifier.DefaultClassifer.Classify(c45WithPruning, test);

            Console.WriteLine("DecisionTreeC45 B4PR REDERR {0:0.00} {1:0.00000} {2} {3} {4}",
                    "N/A",
                    resultBeforePruning.Accuracy,
                    DecisionTreeMetric.GetNumberOfRules(c45WithPruning),
                    resultBeforePruning.AvgNumberOfAttributes,
                    DecisionTreeMetric.GetHeight(c45WithPruning));

            //Console.WriteLine("resultBeforePruning = {0}", resultBeforePruning);
            //Console.WriteLine("number of rules: {0}", DecisionTreeBase.GetNumberOfRules(c45WithPruning));
            //Console.WriteLine(DecisionTreeFormatter.Construct(c45WithPruning.Root, train.DataStoreInfo));

            ReducedErrorPruning reducedErrorPruning = new ReducedErrorPruning(c45WithPruning, prune);
            reducedErrorPruning.Prune();

            var resultAfterPruning = Classifier.DefaultClassifer.Classify(c45WithPruning, test);
            Console.WriteLine("DecisionTreeC45 PRND REDERR {0:0.00} {1:0.00000} {2} {3} {4}",
                    "N/A",
                    resultAfterPruning.Accuracy,
                    DecisionTreeMetric.GetNumberOfRules(c45WithPruning),
                    resultAfterPruning.AvgNumberOfAttributes,
                    DecisionTreeMetric.GetHeight(c45WithPruning));

            //Console.WriteLine("resultAfterPruning = {0}", resultAfterPruning);
            //Console.WriteLine("number of rules: {0}", DecisionTreeBase.GetNumberOfRules(c45WithPruning));
            //Console.WriteLine(DecisionTreeFormatter.Construct(c45WithPruning.Root, train.DataStoreInfo));

            Console.WriteLine();
        }

        [Test, Repeat(1)]
        public void PrePruningTest()
        {
            DataStore data = DataStore.Load(@"Data\dna_modified.trn", FileFormat.Rses1);

            foreach (var fieldInfo in data.DataStoreInfo.Fields)
                fieldInfo.IsNumeric = false;

            DataStore test = DataStore.Load(@"Data\dna_modified.tst", FileFormat.Rses1, data.DataStoreInfo);

            for (double eps = 0.0; eps < 0.4; eps += 0.01)
            {
                DecisionTreeC45 c45WithPrePruning = new DecisionTreeC45();
                c45WithPrePruning.Gamma = eps;
                c45WithPrePruning.Learn(data, data.DataStoreInfo.GetFieldIds(FieldTypes.Standard).ToArray());

                ClassificationResult resultForTreeWithPrePruning = Classifier.DefaultClassifer.Classify(c45WithPrePruning, test);

                Console.WriteLine("C45/Epsilon {0:0.00} {1:0.00000} {2} {3} {4}",
                    eps,
                    resultForTreeWithPrePruning.Accuracy,
                    DecisionTreeMetric.GetNumberOfRules(c45WithPrePruning),
                    resultForTreeWithPrePruning.AvgNumberOfAttributes,
                    DecisionTreeMetric.GetHeight(c45WithPrePruning));

                //Console.WriteLine("C45/Epsilon {0}", resultForTreeWithPrePruning);
                //Console.WriteLine("number of rules: {0}", DecisionTreeBase.GetNumberOfRules(c45WithPrePruning));
            }

            Console.WriteLine();
        }

        [Test, Repeat(1)]
        public void PrePrunningTest2()
        {
            DataStore data = DataStore.Load(@"Data\dna_modified.trn", FileFormat.Rses1);

            foreach (var fieldInfo in data.DataStoreInfo.Fields)
                fieldInfo.IsNumeric = false;

            DataStore test = DataStore.Load(@"Data\dna_modified.tst", FileFormat.Rses1, data.DataStoreInfo);

            for (double eps = 0.0; eps < 0.4; eps += 0.01)
            {
                var tree = new DecisionTreeReduct();
                tree.Gamma = eps;
                tree.Learn(data, data.DataStoreInfo.GetFieldIds(FieldTypes.Standard).ToArray());

                ClassificationResult resultForTreeWithPrePruning = Classifier.DefaultClassifer.Classify(tree, test);
                Console.WriteLine("DecisionTreeReduct {0:0.00} {1:0.00000} {2} {3} {4}",
                    eps,
                    resultForTreeWithPrePruning.Accuracy,
                    DecisionTreeMetric.GetNumberOfRules(tree),
                    resultForTreeWithPrePruning.AvgNumberOfAttributes,
                    DecisionTreeMetric.GetHeight(tree));


                //Console.WriteLine("DecisionTreeReduct {0}", resultForTreeWithPrePruning);
                //Console.WriteLine("number of rules: {0}", DecisionTreeBase.GetNumberOfRules(tree));
            }

            Console.WriteLine();
        }

        public void PrunningInternalTest()
        {
            DataStore data = DataStore.Load(@"Data\dna_modified.trn", FileFormat.Rses1);
            foreach (var fieldInfo in data.DataStoreInfo.Fields)
                fieldInfo.IsNumeric = false;
            DataStore test = DataStore.Load(@"Data\dna_modified.tst", FileFormat.Rses1, data.DataStoreInfo);
            int[] attributes = data.DataStoreInfo.GetFieldIds(FieldTypes.Standard).ToArray();

            DecisionTreeC45 c45 = new DecisionTreeC45();
            c45.PruningType = PruningType.ErrorBasedPruning;
            c45.Learn(data, attributes);
        }

        [Test, Repeat(1)]
        public void PrePrunningTest3()
        {
            DataStore data = DataStore.Load(@"Data\dna_modified.trn", FileFormat.Rses1);

            foreach (var fieldInfo in data.DataStoreInfo.Fields)
                fieldInfo.IsNumeric = false;

            DataStore test = DataStore.Load(@"Data\dna_modified.tst", FileFormat.Rses1, data.DataStoreInfo);

            for (double eps = 0.0; eps < 0.4; eps += 0.01)
            {
                var tree = new DecisionTreeRough();
                tree.Gamma = eps;
                tree.Learn(data, data.DataStoreInfo.GetFieldIds(FieldTypes.Standard).ToArray());

                ClassificationResult resultForTreeWithPrePruning = Classifier.DefaultClassifer.Classify(tree, test);
                Console.WriteLine("DecisionTreeRough {0:0.00} {1:0.00000} {2} {3} {4}", 
                    eps, 
                    resultForTreeWithPrePruning.Accuracy,
                    DecisionTreeMetric.GetNumberOfRules(tree), 
                    resultForTreeWithPrePruning.AvgNumberOfAttributes,
                    DecisionTreeMetric.GetHeight(tree));
            }

            Console.WriteLine();
        }        
    }
}
