using Raccoon.Data;
using Raccoon.MachineLearning.Roughset;
using Raccoon.MachineLearning.Classification.DecisionTrees;
using Raccoon.MachineLearning.Classification.DecisionTrees.Pruning;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Raccoon.MachineLearning.Classification;

namespace Raccoon.MachineLearning.Tests
{
    [TestFixture]
    public class CrossValidationTest
    {
        [Test, Repeat(1)]
        [TestCase(@"Data\chess.data", FileFormat.Rses1)]
        public void RunTest(string dataFile, FileFormat fileFormat)
        {
            Console.WriteLine(ClassificationResult.TableHeader());
            DataStore data = DataStore.Load(dataFile, fileFormat);
            var cv = new CrossValidation(data);
            Console.WriteLine(cv.Run<DecisionTreeC45>(new DecisionTreeC45()));
        }



        [Test, Repeat(1)]
        [TestCase(@"Data\chess.data", FileFormat.Rses1)]
        public void RunTestWithSplitter(string dataFile, FileFormat fileFormat)
        {
            Console.WriteLine(ClassificationResult.TableHeader());
            DataStore data = DataStore.Load(dataFile, fileFormat);
            
            var cv = new CrossValidation(data, 10);

            var c45 = new DecisionTreeC45();            
            Console.WriteLine(cv.Run<DecisionTreeC45>(c45));

            DecisionTreeRough roughTree = new DecisionTreeRough();
            Console.WriteLine(cv.Run<DecisionTreeRough>(roughTree));

        }

        [Test, Repeat(1)]
        [TestCase(@"Data\chess.data", FileFormat.Rses1)]
        public void RunTestWithFolds(string dataFile, FileFormat fileFormat)
        {
            Console.WriteLine(ClassificationResult.TableHeader());
            DataStore data = DataStore.Load(dataFile, fileFormat);            
            DecisionTreeC45 c45 = new DecisionTreeC45();
            c45.PruningType = PruningType.ErrorBasedPruning;            
            c45.ImpurityFunction = ImpurityMeasure.Majority;
            c45.ImpurityNormalize = ImpurityMeasure.DummyNormalize;

            CrossValidation cv10 = new CrossValidation(data, 10);
            CrossValidation cv5 = new CrossValidation(data, 5);

            Console.WriteLine(cv10.Run<DecisionTreeC45>(c45));
            Console.WriteLine(cv5.Run<DecisionTreeC45>(c45));

            DecisionTreeC45 c45b = new DecisionTreeC45();
            c45b.PruningType = PruningType.ErrorBasedPruning;
            c45b.ImpurityFunction = ImpurityMeasure.One;
            c45b.ImpurityNormalize = ImpurityMeasure.DummyNormalize;

            Console.WriteLine(cv10.Run<DecisionTreeC45>(c45b));
            Console.WriteLine(cv5.Run<DecisionTreeC45>(c45b));
        }
    }
}
