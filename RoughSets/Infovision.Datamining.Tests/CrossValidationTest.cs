using Infovision.Data;
using Infovision.Datamining.Roughset;
using Infovision.Datamining.Roughset.DecisionTrees;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infovision.Datamining.Tests
{
    [TestFixture]
    public class CrossValidationTest
    {
        [Test, Repeat(1)]
        [TestCase(@"Data\chess.data", FileFormat.Rses1)]
        public void RunTest(string dataFile, FileFormat fileFormat)
        {
            Console.WriteLine(ClassificationResult.ResultHeader());
            DataStore data = DataStore.Load(dataFile, fileFormat);
            Console.WriteLine(new CrossValidation<DecisionTreeC45>(new DecisionTreeC45()).Run(data));
        }

        [Test, Repeat(1)]
        [TestCase(@"Data\chess.data", FileFormat.Rses1)]
        public void RunTestWithSplitter(string dataFile, FileFormat fileFormat)
        {
            Console.WriteLine(ClassificationResult.ResultHeader());
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
            Console.WriteLine(ClassificationResult.ResultHeader());
            DataStore data = DataStore.Load(dataFile, fileFormat);            
            DecisionTreeC45 c45 = new DecisionTreeC45();
            c45.PruningType = Roughset.DecisionTrees.Pruning.PruningType.ErrorBasedPruning;            
            c45.ImpurityFunction = ImpurityFunctions.Majority;
            c45.ImpurityNormalize = ImpurityFunctions.DummyNormalize;

            CrossValidation<DecisionTreeC45> cv = new CrossValidation<DecisionTreeC45>(c45);

            Console.WriteLine(cv.Run(data, 10));
            Console.WriteLine(cv.Run(data, 5));

            DecisionTreeC45 c45b = new DecisionTreeC45();
            c45b.PruningType = Roughset.DecisionTrees.Pruning.PruningType.ErrorBasedPruning;
            c45b.ImpurityFunction = ImpurityFunctions.One;
            c45b.ImpurityNormalize = ImpurityFunctions.DummyNormalize;

            CrossValidation<DecisionTreeC45> cvb = new CrossValidation<DecisionTreeC45>(c45b);

            Console.WriteLine(cvb.Run(data, 10));
            Console.WriteLine(cvb.Run(data, 5));
        }
    }
}
