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
            Console.WriteLine(new CrossValidation<DecisionTreeC45>(new DecisionTreeC45()).Run(data));
        }

        [Test, Repeat(1)]
        [TestCase(@"Data\chess.data", FileFormat.Rses1)]
        public void RunTestWithSplitter(string dataFile, FileFormat fileFormat)
        {
            Console.WriteLine(ClassificationResult.TableHeader());
            DataStore data = DataStore.Load(dataFile, fileFormat);
            DataStoreSplitter splitter = new DataStoreSplitter(data, 10);

            var c45 = new DecisionTreeC45();            
            var cv = new CrossValidation<DecisionTreeC45>(c45);            
            Console.WriteLine(cv.Run(data, splitter));

            DecisionTreeRough roughTree = new DecisionTreeRough();
            var cv2 = new CrossValidation<DecisionTreeRough>(roughTree);
            Console.WriteLine(cv2.Run(data, splitter));

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

            CrossValidation<DecisionTreeC45> cv = new CrossValidation<DecisionTreeC45>(c45);

            Console.WriteLine(cv.Run(data, 10));
            Console.WriteLine(cv.Run(data, 5));

            DecisionTreeC45 c45b = new DecisionTreeC45();
            c45b.PruningType = PruningType.ErrorBasedPruning;
            c45b.ImpurityFunction = ImpurityMeasure.One;
            c45b.ImpurityNormalize = ImpurityMeasure.DummyNormalize;

            CrossValidation<DecisionTreeC45> cvb = new CrossValidation<DecisionTreeC45>(c45b);

            Console.WriteLine(cvb.Run(data, 10));
            Console.WriteLine(cvb.Run(data, 5));
        }
    }
}
